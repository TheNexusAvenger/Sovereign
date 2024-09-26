using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Bouncer.Diagnostic;
using Bouncer.State.Loop;
using Bouncer.Web.Client;
using Bouncer.Web.Client.Response;
using Bouncer.Web.Client.Response.Group;
using Microsoft.EntityFrameworkCore;
using Sovereign.Bans.JoinRequests.Configuration;
using Sovereign.Core.Database;
using Sovereign.Core.Database.Model.Api;
using Sovereign.Core.Database.Model.JoinRequests;
using Sovereign.Core.Model;

namespace Sovereign.Bans.JoinRequests.Loop;

public class JoinRequestBanLoop : BaseConfigurableLoop<JoinRequestsGroupConfiguration>
{
    /// <summary>
    /// Status of the last step of the loop.
    /// </summary>
    public GroupJoinRequestLoopStatus Status { get; private set; } = GroupJoinRequestLoopStatus.NotStarted;
    
    /// <summary>
    /// Override path to the bans database.
    /// Only intended for use in unit tests.
    /// </summary>
    public string? OverrideBansDatabasePath { get; set; }
    
    /// <summary>
    /// Override path to the join request bans database.
    /// Only intended for use in unit tests.
    /// </summary>
    public string? OverrideJoinRequestBansDatabasePath { get; set; }
    
    /// <summary>
    /// Client used for sending Roblox group requests.
    /// </summary>
    private readonly RobloxGroupClient _robloxGroupClient;
    
    /// <summary>
    /// Creates a join request ban loop.
    /// </summary>
    /// <param name="initialConfiguration">Initial configuration of the loop.</param>
    /// <param name="robloxGroupClient">Client for managing groups.</param>
    public JoinRequestBanLoop(JoinRequestsGroupConfiguration initialConfiguration, RobloxGroupClient robloxGroupClient) : base($"JoinRequestBanLoop_{initialConfiguration.Domain}_{initialConfiguration.GroupId}", initialConfiguration)
    {
        this._robloxGroupClient = robloxGroupClient;
    }
    
    /// <summary>
    /// Creates a join request ban loop.
    /// </summary>
    /// <param name="initialConfiguration">Initial configuration of the loop.</param>
    public JoinRequestBanLoop(JoinRequestsGroupConfiguration initialConfiguration) : this(initialConfiguration, new RobloxGroupClient())
    {
        
    }

    /// <summary>
    /// Handles a join request.
    /// </summary>
    /// <param name="joinRequest">Join request entry to handle.</param>
    /// <returns>Whether the join request was denied.</returns>
    public async Task<bool> HandleJoinRequestAsync(GroupJoinRequestEntry joinRequest)
    {
        var domain = this.Configuration.Domain!;
        var robloxGroupId = this.Configuration.GroupId!.Value;
        var joinRequestUserId = joinRequest.UserId;
        var dryRun = this.Configuration.DryRun ?? false;
        var logPrefix = (dryRun ? "[DRY RUN] " : "");
        
        // Ignore the user if they aren't banned.
        await using var context = new BansContext(this.OverrideBansDatabasePath);
        var latestBan = await context.BanEntries
            .Where(entry => entry.Domain.ToLower() == domain.ToLower())
            .Where(entry => entry.TargetRobloxUserId == joinRequestUserId)
            .OrderByDescending(entry => entry.StartTime)
            .FirstOrDefaultAsync();
        if (latestBan == null || latestBan.Action == BanAction.Unban)
        {
            Logger.Debug($"Ignoring join request from {joinRequestUserId} for group {robloxGroupId} in domain {domain} because they aren't banned.");
            return false;
        }
        if (latestBan.EndTime != null && latestBan.EndTime < DateTime.Now)
        {
            Logger.Debug($"Ignoring join request from {joinRequestUserId} for group {robloxGroupId} in domain {domain} because their temporary ban expired.");
            return false;
        }
        
        // Deny the join request.
        Logger.Info($"{logPrefix}Declining {joinRequestUserId} from group {robloxGroupId} in domain {domain} because they are banned.");
        if (!dryRun)
        {
            await this._robloxGroupClient.DeclineJoinRequestAsync(robloxGroupId, joinRequestUserId);
            await using var joinRequestBansContext = new JoinRequestBansContext(this.OverrideJoinRequestBansDatabasePath);
            joinRequestBansContext.JoinRequestDeclineHistory.Add(new JoinRequestDeclineHistoryEntry()
            {
                BanId = latestBan.Id,
                Domain = latestBan.Domain,
                GroupId = robloxGroupId,
                UserId = joinRequestUserId,
            });
            await joinRequestBansContext.SaveChangesAsync();
        }
        return true;
    }

    /// <summary>
    /// Handles join requests from a ban entry.
    /// </summary>
    /// <param name="banEntry">Ban entry to handle.</param>
    public async Task HandleJoinRequestsFromBanAsync(BanEntry banEntry)
    {
        // Return if the domain or ban is incorrect.
        var robloxGroupId = this.Configuration.GroupId!.Value;
        if (!string.Equals(banEntry.Domain, this.Configuration.Domain!, StringComparison.CurrentCultureIgnoreCase))
        {
            Logger.Trace($"Ignoring ban {banEntry.Id} for domain {banEntry.Domain} with group id {robloxGroupId} because the ban is not for the domain.");
            return;
        }
        if (banEntry.Action != BanAction.Ban)
        {
            Logger.Trace($"Ignoring ban {banEntry.Id} for domain {banEntry.Domain} with group id {robloxGroupId} because the ban action is not a ban.");
            return;
        }
        
        // Get and handle the join requests.
        var joinRequests = await this._robloxGroupClient.GetJoinRequests(robloxGroupId, filter: $"user == 'users/{banEntry.TargetRobloxUserId}'");
        foreach (var joinRequest in joinRequests.GroupJoinRequests)
        {
            await this.HandleJoinRequestAsync(joinRequest);
        }
    } 
    
    /// <summary>
    /// Runs a step in the loop.
    /// </summary>
    public override async Task RunAsync()
    {
        // Prepare the stats.
        var domain = this.Configuration.Domain!;
        var robloxGroupId = this.Configuration.GroupId!.Value;
        var dryRun = this.Configuration.DryRun ?? false;
        var declinedJoinRequests = 0;
        var ignoredJoinRequests = 0;
        var logPrefix = (dryRun ? "[DRY RUN] " : "");
        this.Status = GroupJoinRequestLoopStatus.Running;
        
        try
        {
            // Get the initial page of join requests.
            var joinRequests = await this._robloxGroupClient.GetJoinRequests(robloxGroupId);
            
            // Process pages until the end is reached.
            while (true)
            {
                // Handle the join requests.
                Logger.Info($"{logPrefix}Handling {joinRequests.GroupJoinRequests.Count} join requests for group {robloxGroupId} in domain {domain}.");
                foreach (var joinRequest in joinRequests.GroupJoinRequests)
                {
                    var joinRequestDeclined = await this.HandleJoinRequestAsync(joinRequest);
                    if (joinRequestDeclined)
                    {
                        declinedJoinRequests += 1;
                    }
                    else
                    {
                        ignoredJoinRequests += 1;
                    }
                }
                
                // Stop the loop if there are no more join requests.
                if (string.IsNullOrEmpty(joinRequests.NextPageToken))
                {
                    break;
                }
                
                // Prepare the next page of join requests.
                var pageToken = joinRequests.NextPageToken;
                joinRequests = await this._robloxGroupClient.GetJoinRequests(robloxGroupId, pageToken: pageToken);
                if (pageToken == joinRequests.NextPageToken) // Original bug: https://devforum.roblox.com/t/open-cloud-groups-api-users-api-beta/2909090/38
                {
                    throw new InvalidDataException($"Duplicate next page token returned for group join requests ({pageToken}).");
                }
            }
            Logger.Info($"Reached end of join requests for {robloxGroupId}.");
            this.Status = GroupJoinRequestLoopStatus.Complete;
        }
        catch (OpenCloudAccessException e)
        {
            // Change the status and throw the exception if it wasn't too many requests.
            if (e.Issue == OpenCloudAccessIssue.TooManyRequests)
            {
                Logger.Warn($"Loop \"{this.Name}\" ran out of requests. Join requests will be continued in the next step.");
                this.Status = GroupJoinRequestLoopStatus.TooManyRequests;
            }
            else if (e.Issue != OpenCloudAccessIssue.Unknown)
            {
                Logger.Error($"Loop \"{this.Name}\" failed due to an invalid or misconfigured API key.");
                this.Status = GroupJoinRequestLoopStatus.InvalidApiKey;
                throw;
            }
            else
            {
                this.Status = GroupJoinRequestLoopStatus.Error;
                throw;
            }
        }
        catch (Exception)
        {
            // Change the status and throw the exception up.
            this.Status = GroupJoinRequestLoopStatus.Error;
            throw;
        }
        finally
        {
            // Log the stats.
            Logger.Info($"{logPrefix}Join requests for {robloxGroupId} summary: {declinedJoinRequests} declined, {ignoredJoinRequests} ignored.");
        }
    }

    /// <summary>
    /// Handles the configuration being set.
    /// This must handle starting the loop.
    /// </summary>
    public override void OnConfigurationSet()
    {
        this._robloxGroupClient.OpenCloudApiKey = this.Configuration.ApiKey;
        this.Start(this.Configuration.LoopDelaySeconds ?? 30);
    }
}
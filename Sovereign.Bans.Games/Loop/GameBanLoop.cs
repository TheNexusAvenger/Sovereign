﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bouncer.Diagnostic;
using Bouncer.State.Loop;
using Bouncer.Web.Client.Response;
using Microsoft.EntityFrameworkCore;
using Sovereign.Bans.Games.Configuration;
using Sovereign.Core.Database;
using Sovereign.Core.Database.Model.Api;
using Sovereign.Core.Model;
using Sovereign.Core.Web.Client;

namespace Sovereign.Bans.Games.Loop;

public enum GameBanLoopStatus
{
    /// <summary>
    /// The loop has not been started yet.
    /// </summary>
    NotStarted,
    
    /// <summary>
    /// The loop is actively handling join requests.
    /// </summary>
    Running,
    
    /// <summary>
    /// The loop completed with no exceptions.
    /// </summary>
    Complete,
    
    /// <summary>
    /// Too many requests and won't run again until the next step.
    /// </summary>
    TooManyRequests,
    
    /// <summary>
    /// API key is invalid.
    /// </summary>
    InvalidApiKey,
    
    /// <summary>
    /// An error occured during the last step.
    /// </summary>
    Error,
}

public class GameBanLoop : BaseConfigurableLoop<GameConfiguration>
{
    /// <summary>
    /// Maximum bans to index per fetch.
    /// </summary>
    public const int BansToIndexInBatch = 100;
    
    /// <summary>
    /// Status of the last step of the loop.
    /// </summary>
    public GameBanLoopStatus Status { get; private set; } = GameBanLoopStatus.NotStarted;
    
    /// <summary>
    /// Override path to the bans database.
    /// Only intended for use in unit tests.
    /// </summary>
    public string? OverrideBansDatabasePath { get; set; }
    
    /// <summary>
    /// Last index from the database that was checked in the background.
    /// </summary>
    public int? LastSuccessfulIndex { get; private set; }

    /// <summary>
    /// Cache for which bans have been handled.
    /// </summary>
    private readonly HandledBanCache _handledBanCache;

    /// <summary>
    /// Client for managing bans and unbans.
    /// </summary>
    private readonly RobloxUserRestrictionClient _robloxUserRestrictionClient;
    
    /// <summary>
    /// Creates a game ban loop.
    /// </summary>
    /// <param name="initialConfiguration">Initial configuration of the loop.</param>
    /// <param name="handledBanCache">Cache for which bans have been handled.</param>
    /// <param name="robloxUserRestrictionClient">Client for managing bans and unbans.</param>
    public GameBanLoop(GameConfiguration initialConfiguration, HandledBanCache handledBanCache, RobloxUserRestrictionClient robloxUserRestrictionClient) : base($"GameBanLoop_{initialConfiguration.Domain}_{initialConfiguration.GameId}", initialConfiguration)
    {
        this._handledBanCache = handledBanCache;
        this._robloxUserRestrictionClient = robloxUserRestrictionClient;
    }
    
    /// <summary>
    /// Creates a game ban loop.
    /// </summary>
    /// <param name="initialConfiguration">Initial configuration of the loop.</param>
    public GameBanLoop(GameConfiguration initialConfiguration) : this(initialConfiguration, new HandledBanCache(initialConfiguration.Domain!, initialConfiguration.GameId!.Value), new RobloxUserRestrictionClient())
    {
        
    }

    /// <summary>
    /// Performs a ban or unban.
    /// Bans/unbans that have been performed before will not be performed again.
    /// </summary>
    /// <param name="banEntry">Ban entry to handle from the main bans database.</param>
    public async Task HandleBanAsync(BanEntry banEntry)
    {
        // Return if the ban is already handled.
        if (!string.Equals(banEntry.Domain, this.Configuration.Domain, StringComparison.CurrentCultureIgnoreCase)) return;
        if (this._handledBanCache.IsHandled(banEntry.Id)) return;
        
        // Send the ban request.
        var domain = this.Configuration.Domain!;
        var gameId = this.Configuration.GameId!.Value;
        if (banEntry.Action == BanAction.Ban)
        {
            Logger.Debug($"Banning user in {domain} with {gameId} with ban id {banEntry.Id}");
            long? duration = (banEntry.EndTime != null ? (long) (banEntry.EndTime - banEntry.StartTime).Value.Duration().TotalSeconds : null);
            await this._robloxUserRestrictionClient.BanAsync(gameId, banEntry.TargetRobloxUserId, banEntry.DisplayReason, banEntry.PrivateReason, duration);
        }
        else
        {
            Logger.Debug($"Unbanning user in {domain} with {gameId} with ban id {banEntry.Id}");
            await this._robloxUserRestrictionClient.UnbanAsync(gameId, banEntry.TargetRobloxUserId);
        }
        
        // Set the ban as handled.
        await this._handledBanCache.SetHandledAsync(new List<long>() { banEntry.Id });
    }
    
    /// <summary>
    /// Runs a step in the loop.
    /// </summary>
    public override async Task RunAsync()
    {
        var domain = this.Configuration.Domain!;
        var gameId = this.Configuration.GameId!.Value;
        this.Status = GameBanLoopStatus.Running;
        try
        {
            while (true)
            {
                // TODO: Handle loop being stopped.
                // Build the query and get the bans to handle.
                await using var bansContext = new BansContext(this.OverrideBansDatabasePath, connectMode: DatabaseConnectMode.ReadOnly);
                var bansQuery = bansContext.GetCurrentBans(domain).Take(BansToIndexInBatch);
                if (this.LastSuccessfulIndex.HasValue)
                {
                    bansQuery = bansQuery.Skip(this.LastSuccessfulIndex.Value + 1);
                }
                var bansToHandle = await bansQuery.ToListAsync();

                // Break the loop if no bans remain.
                if (bansToHandle.Count == 0)
                {
                    Logger.Debug($"Finished processing bans for {domain} game {gameId}.");
                    this.LastSuccessfulIndex = null;
                    break;
                }

                // Handle the bans/unbans.
                foreach (var banEntry in bansQuery)
                {
                    // Handle the ban action.
                    await this.HandleBanAsync(banEntry);

                    // Set the index as handled.
                    this.LastSuccessfulIndex = (this.LastSuccessfulIndex ?? -1) + 1;
                }
            }
            this.Status = GameBanLoopStatus.Complete;
        }
        catch (OpenCloudAccessException e)
        {
            // Change the status and throw the exception if it wasn't too many requests.
            if (e.Issue == OpenCloudAccessIssue.TooManyRequests)
            {
                Logger.Warn($"Loop \"{this.Name}\" ran out of requests. Background bans will be continued in the next step.");
                this.Status = GameBanLoopStatus.TooManyRequests;
            }
            else if (e.Issue != OpenCloudAccessIssue.Unknown)
            {
                Logger.Error($"Loop \"{this.Name}\" failed due to an invalid or misconfigured API key.");
                this.Status = GameBanLoopStatus.InvalidApiKey;
                throw;
            }
            else
            {
                this.Status = GameBanLoopStatus.Error;
                this.LastSuccessfulIndex = (this.LastSuccessfulIndex ?? -1) + 1;
                throw;
            }
        }
    }
    
    /// <summary>
    /// Handles the configuration being set.
    /// This must handle starting the loop.
    /// </summary>
    public override void OnConfigurationSet()
    {
        this._robloxUserRestrictionClient.OpenCloudApiKey = this.Configuration.ApiKey;
        this.Start(60);
    }
}
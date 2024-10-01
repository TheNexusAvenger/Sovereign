using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bouncer.Diagnostic;
using Bouncer.State.Loop;
using Bouncer.Web.Server.Model;
using Microsoft.EntityFrameworkCore;
using Sovereign.Bans.Games.Configuration;
using Sovereign.Bans.Games.Web.Server.Model;
using Sovereign.Core.Database;
using Sovereign.Core.Database.Model.Api;
using Sovereign.Core.Model.Response;
using Sovereign.Core.Web.Client;
using Sovereign.Core.Web.Server.Request;

namespace Sovereign.Bans.Games.Loop;

public class GameBanLoopCollection : GenericLoopCollection<GameBanLoop, GamesConfiguration, GameConfiguration>
{
    /// <summary>
    /// Returns the status of the loops.
    /// </summary>
    /// <returns>The status of the loops.</returns>
    public async Task<GameBansHealthCheckResult> GetStatusAsync()
    {
        var loopStatuses = new List<GameBansGameLoopHealthCheckResult>();
        var openCloudStatuses = new List<GameBansOpenCloudHealthCheckResult>();
        var openCloudKeyToStatus = new Dictionary<string, HealthCheckResultStatus>();
        foreach (var (_, loop) in this.ActiveLoops)
        {
            // Add the loop status.
            var domain = loop.Configuration.Domain!;
            var gameId = loop.Configuration.GameId!.Value;
            var healthCheckStatus = HealthCheckResultStatus.Up;
            if (loop.Status == GameBanLoopStatus.InvalidApiKey || loop.Status == GameBanLoopStatus.Error)
            {
                healthCheckStatus = HealthCheckResultStatus.Down;
            }
            loopStatuses.Add(new GameBansGameLoopHealthCheckResult()
            {
                Status = healthCheckStatus,
                Domain = domain,
                GameId = gameId,
                LastStepStatus = loop.Status,
            });
            
            // Add the Open Cloud status.
            var apiKey = loop.Configuration.ApiKey!;
            if (!openCloudKeyToStatus.ContainsKey(apiKey))
            {
                try
                {
                    var userRestrictionsClient = new RobloxUserRestrictionClient();
                    userRestrictionsClient.OpenCloudApiKey = apiKey;
                    await userRestrictionsClient.GetUserRestriction(gameId, 1);
                    openCloudKeyToStatus[apiKey] = HealthCheckResultStatus.Up;
                }
                catch (Exception e)
                {
                    openCloudKeyToStatus[apiKey] = HealthCheckResultStatus.Down;
                    Logger.Error($"Error occured handling health check for Open Cloud API key in {domain} game id {gameId}.\n{e}");
                }
            }
            openCloudStatuses.Add(new GameBansOpenCloudHealthCheckResult()
            {
                Status = openCloudKeyToStatus[apiKey],
                Domain = domain,
                GameId = gameId,
            });
        }
        
        return GameBansHealthCheckResult.FromLoopHealthResults(loopStatuses, openCloudStatuses);
    }
    
    /// <summary>
    /// Returns the list of configuration entries from the current configuration.
    /// </summary>
    /// <param name="configuration">Configuration to get the entries from.</param>
    /// <returns>List of configuration entries.</returns>
    public override List<GameConfiguration> GetConfigurationEntries(GamesConfiguration configuration)
    {
        return configuration.Games!;
    }

    /// <summary>
    /// Returns the loop id for the configuration.
    /// </summary>
    /// <param name="configuration">Configuration to get the key from.</param>
    /// <returns>Key id for the configuration loop.</returns>
    public override string GetLoopKeyId(GameConfiguration configuration)
    {
        var domain = configuration.Domain ?? "UNKNOWN_DOMAIN";
        var gameId = configuration.GameId?.ToString() ?? "0";
        return $"{domain}_{gameId}";
    }

    /// <summary>
    /// Returns the loop instance for a configuration.
    /// </summary>
    /// <param name="configuration">Configuration to get the key from.</param>
    /// <returns>Loop for the configuration.</returns>
    public override GameBanLoop CreateLoop(GameConfiguration configuration)
    {
        return new GameBanLoop(configuration);
    }

    /// <summary>
    /// Handles a webhook request for realtime processing.
    /// </summary>
    /// <param name="request">Webhook request to handle.</param>
    /// <returns>Response for the webhook.</returns>
    public async Task<JsonResponse> HandleWebhookAsync(SovereignWebhookRequest request)
    {
        // Get the bans to handle.
        await using var context = new BansContext();
        var bansToHandle = new List<BanEntry>();
        foreach (var banId in request.Ids!)
        {
            var banEntry = await context.BanEntries.FirstOrDefaultAsync(entry => entry.Id == banId);
            if (banEntry == null)
            {
                Logger.Warn($"Webhook sent ban id {banId} but was not found.");
                continue;
            }
            Logger.Trace($"Found ban id {banId} from webhook.");
            bansToHandle.Add(banEntry);
        }
        
        // Process the bans.
        var domain = request.Domain!;
        var loops = this.ActiveLoops.Values.Where(loop =>
            string.Equals(loop.Configuration.Domain!, domain, StringComparison.CurrentCultureIgnoreCase)).ToList();
        Logger.Info($"Handling {bansToHandle.Count} webhook bans for domain {domain} in {loops.Count} games.");
        foreach (var loop in loops)
        {
            foreach (var banEntry in bansToHandle)
            {
                try
                {
                    await loop.HandleBanAsync(banEntry);
                }
                catch (Exception e)
                {
                    Logger.Error($"Failed to process ban {banEntry.Id} in domain {domain} in loop {loop.Name}: {e}");
                    return new JsonResponse(new SimpleResponse(ResponseStatus.ServerProcessingError), 500);
                }
            }
        }
        return SimpleResponse.SuccessResponse;
    }
}
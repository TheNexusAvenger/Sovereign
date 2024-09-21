using System.Collections.Generic;
using Bouncer.State.Loop;
using Bouncer.Web.Server.Model;
using Sovereign.Bans.Games.Configuration;
using Sovereign.Bans.Games.Web.Server.Model;

namespace Sovereign.Bans.Games.Loop;

public class GameBanLoopCollection : GenericLoopCollection<GameBanLoop, GamesConfiguration, GameConfiguration>
{
    /// <summary>
    /// Returns the status of the loops.
    /// </summary>
    /// <returns>The status of the loops.</returns>
    public List<GameBansGameLoopHealthCheckResult> GetStatus()
    {
        var loopStatuses = new List<GameBansGameLoopHealthCheckResult>();
        foreach (var (_, loop) in this.ActiveLoops)
        {
            var healthCheckStatus = HealthCheckResultStatus.Up;
            if (loop.Status == GameBanLoopStatus.InvalidApiKey || loop.Status == GameBanLoopStatus.Error)
            {
                healthCheckStatus = HealthCheckResultStatus.Down;
            }
            loopStatuses.Add(new GameBansGameLoopHealthCheckResult()
            {
                Status = healthCheckStatus,
                Domain = loop.Configuration.Domain!,
                GameId = loop.Configuration.GameId!.Value,
                LastStepStatus = loop.Status,
            });
        }
        return loopStatuses;
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
}
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bouncer.Diagnostic;
using Sovereign.Core.Database;
using Sovereign.Core.Database.Model.Bans;

namespace Sovereign.Bans.Games.Loop;

public class HandledBanCache
{
    /// <summary>
    /// Domain of the handled bans to cache.
    /// </summary>
    public readonly string Domain;

    /// <summary>
    /// Game id of the handled bans to cache.
    /// </summary>
    public readonly long GameId;

    /// <summary>
    /// Override path of the database context.
    /// </summary>
    private readonly string? _databaseFilePath;

    /// <summary>
    /// Cache of the handled ban ids.
    /// There is no eviction policy for this. This might be a problem with hundreds of thousands of active bans.
    /// </summary>
    private readonly HashSet<long> _handledBanIds;

    /// <summary>
    /// Semaphore for setting the cache.
    /// </summary>
    private SemaphoreSlim _updateSemaphore = new SemaphoreSlim(1);

    /// <summary>
    /// Creates a handled ban cache.
    /// </summary>
    /// <param name="domain">Domain of the handled bans to cache.</param>
    /// <param name="gameId">Game id of the handled bans to cache.</param>
    /// <param name="databaseFilePath">Optional file path of the database context.</param>
    public HandledBanCache(string domain, long gameId, string? databaseFilePath = null)
    {
        this.Domain = domain;
        this.GameId = gameId;
        this._databaseFilePath = databaseFilePath;

        using var context = new GameBansContext(databaseFilePath);
        this._handledBanIds = context.GameBansHistory
            .Where(entry => entry.Domain.ToLower() == domain.ToLower() && entry.GameId == gameId)
            .Select(entry => entry.BanId)
            .ToHashSet();
    }

    /// <summary>
    /// Returns if a ban entry was handled.
    /// </summary>
    /// <param name="banId">Id of the ban.</param>
    /// <returns>Whether the ban was handled or not.</returns>
    public bool IsHandled(long banId)
    {
        return this._handledBanIds.Contains(banId);
    }

    /// <summary>
    /// Sets a list of ban ids as handled.
    /// </summary>
    /// <param name="banIds">Ban ids to set as handled.</param>
    public async Task SetHandledAsync(List<long> banIds)
    {
        await _updateSemaphore.WaitAsync();
        try
        {
            // Add the ban ids to the database.
            await using var context = new GameBansContext(this._databaseFilePath);
            foreach (var banId in banIds)
            {
                if (this.IsHandled(banId)) continue;
                Logger.Trace($"Setting ban {banId} for domain {this.Domain} with game id {this.GameId} as handled.");
                context.GameBansHistory.Add(new GameBansHistoryEntry()
                {
                    BanId = banId,
                    Domain = this.Domain,
                    GameId = this.GameId,
                });
            }
            await context.SaveChangesAsync();
        
            // Add the ids to the hash set.
            foreach (var banId in banIds)
            {
                this._handledBanIds.Add(banId);
            }
        }
        finally
        {
            _updateSemaphore.Release();
        }
    }
}
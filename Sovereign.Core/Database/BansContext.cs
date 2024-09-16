using System.Linq;
using Microsoft.EntityFrameworkCore;
using Sovereign.Core.Database.Model.Api;

namespace Sovereign.Core.Database;

public class BansContext : BaseContext
{
    /// <summary>
    /// List of the ban actions.
    /// </summary>
    public DbSet<BanEntry> BanEntries { get; set; } = null!;

    /// <summary>
    /// External account links to match to Roblox accounts.
    /// </summary>
    public DbSet<ExternalAccountLink> ExternalAccountLinks { get; set; } = null!;
    
    /// <summary>
    /// Creates a bans SQLite context.
    /// </summary>
    /// <param name="filePath">File path to use for the context.</param>
    public BansContext(string? filePath = null) : base("Bans", filePath)
    {
        
    }

    /// <summary>
    /// Returns the latest ban entries for each user in the database.
    /// </summary>
    /// <param name="domain">Domain to fetch the bans for.</param>
    /// <returns>Query for the current bans.</returns>
    public IQueryable<BanEntry> GetCurrentBans(string domain)
    {
        return this.BanEntries
            .Where(entry => entry.Domain.ToLower() == domain.ToLower())
            .GroupBy(entry => entry.TargetRobloxUserId)
            .Select(entryGroup => entryGroup.OrderByDescending(entry => entry.StartTime).First());
    }
}
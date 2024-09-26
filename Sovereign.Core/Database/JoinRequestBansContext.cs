using Microsoft.EntityFrameworkCore;
using Sovereign.Core.Database.Model.JoinRequests;
using Sovereign.Core.Database.Model.JoinRequests.Compiled;

namespace Sovereign.Core.Database;

public class JoinRequestBansContext : BaseContext
{
    /// <summary>
    /// History of the handled join requests.
    /// </summary>
    public DbSet<JoinRequestDeclineHistoryEntry> JoinRequestDeclineHistory { get; set; } = null!;
    
    /// <summary>
    /// Creates a join request bans SQLite context.
    /// </summary>
    /// <param name="filePath">File path to use for the context.</param>
    public JoinRequestBansContext(string? filePath = null) : base("JoinRequestBans", filePath)
    {
        
    }
    
    /// <summary>
    /// Configures the database.
    /// </summary>
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder.UseModel(JoinRequestBansContextModel.Instance));
    }
}
﻿using Microsoft.EntityFrameworkCore;
using Sovereign.Core.Database.Model.Bans;
using Sovereign.Core.Database.Model.Bans.Compiled;

namespace Sovereign.Core.Database;

public class GameBansContext : BaseContext
{
    /// <summary>
    /// History of the handled game bans.
    /// </summary>
    public DbSet<GameBansHistoryEntry> GameBansHistory { get; set; } = null!;
    
    /// <summary>
    /// Creates a bans SQLite context.
    /// </summary>
    /// <param name="filePath">File path to use for the context.</param>
    public GameBansContext(string? filePath = null) : base("GameBans", filePath)
    {
        
    }
    
    /// <summary>
    /// Configures the database.
    /// </summary>
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder.UseModel(GameBansContextModel.Instance));
    }
}
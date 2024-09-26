using System;
using Bouncer.Diagnostic;
using Sovereign.Bans.Games.Configuration;
using Sovereign.Bans.Games.Loop;
using Sovereign.Bans.Games.Web.Server;
using Sovereign.Core;
using Sovereign.Core.Database;

namespace Sovereign.Bans.Games;

public class Program : BaseProgram<GamesConfiguration>
{
    /// <summary>
    /// Runs the program.
    /// </summary>
    public override void Run()
    {
        // Migrate the database.
        using var context = new GameBansContext();
        context.MigrateAsync().Wait();
        if (Environment.GetEnvironmentVariable("DATABASE_DIRECTORY_LOCATION") == null)
        {
            Logger.Debug("Creating bans database for testing due to DATABASE_DIRECTORY_LOCATION not being defined.");
            using var bansContext = new BansContext();
            bansContext.MigrateAsync().Wait();
            
        }
        
        // Start the game ban loops.
        var gameBanLoopCollection = new GameBanLoopCollection();
        
        // Start the web server.
        new GamesWebServer().StartAsync(gameBanLoopCollection).Wait();
    }

    /// <summary>
    /// Runs the program.
    /// </summary>
    /// <param name="args">Arguments from the command line.</param>
    public static void Main(string[] args)
    {
        new Program()
        {
            DefaultConfiguration = GamesConfiguration.GetDefaultConfiguration(),
            ConfigurationJsonTypeInfo = GamesConfigurationJsonContext.Default.GamesConfiguration,
        }.Main();
    }
}
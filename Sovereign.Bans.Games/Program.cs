using Sovereign.Bans.Games.Configuration;
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
        
        // Start the web server.
        new GamesWebServer().StartAsync().Wait();
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
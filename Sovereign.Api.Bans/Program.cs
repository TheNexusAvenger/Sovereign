using Sovereign.Api.Bans.Configuration;
using Sovereign.Api.Bans.Web.Server;
using Sovereign.Core;
using Sovereign.Core.Database;

namespace Sovereign.Api.Bans;

public class Program : BaseProgram<BansConfiguration>
{
    /// <summary>
    /// Runs the program.
    /// </summary>
    public override void Run()
    {
        // Migrate the database.
        using var context = new BansContext();
        context.MigrateAsync().Wait();
        
        // Start the web server.
        new BansWebServer().StartAsync().Wait();
    }

    /// <summary>
    /// Runs the program.
    /// </summary>
    /// <param name="args">Arguments from the command line.</param>
    public static void Main(string[] args)
    {
        new Program()
        {
            DefaultConfiguration = BansConfiguration.GetDefaultConfiguration(),
            ConfigurationJsonTypeInfo = BansConfigurationJsonContext.Default.BansConfiguration,
        }.Main();
    }
}
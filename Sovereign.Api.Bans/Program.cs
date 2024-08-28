using Sovereign.Api.Bans.Configuration;
using Sovereign.Api.Bans.Web.Server;
using Sovereign.Core;

namespace Sovereign.Api.Bans;

public class Program : BaseProgram<BansConfiguration>
{
    /// <summary>
    /// Runs the program.
    /// </summary>
    public override void Run()
    {
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
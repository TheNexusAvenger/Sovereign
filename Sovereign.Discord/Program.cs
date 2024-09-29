using Sovereign.Core;
using Sovereign.Discord.Configuration;
using Sovereign.Discord.Discord;
using Sovereign.Discord.Web.Server;

namespace Sovereign.Discord;

public class Program : BaseProgram<DiscordConfiguration>
{
    /// <summary>
    /// Runs the program.
    /// </summary>
    public override void Run()
    {
        // Start the Discord bot.
        var bot = new Bot();
        bot.StartAsync().Wait();
        
        // Start the web server.
        new DiscordWebServer().StartAsync(bot).Wait();
    }

    /// <summary>
    /// Runs the program.
    /// </summary>
    /// <param name="args">Arguments from the command line.</param>
    public static void Main(string[] args)
    {
        new Program()
        {
            DefaultConfiguration = DiscordConfiguration.GetDefaultConfiguration(),
            ConfigurationJsonTypeInfo = DiscordConfigurationJsonContext.Default.DiscordConfiguration,
        }.Main();
    }
}
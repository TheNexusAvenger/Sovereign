using System;
using Bouncer.Diagnostic;
using Sovereign.Bans.JoinRequests.Configuration;
using Sovereign.Bans.JoinRequests.Loop;
using Sovereign.Bans.JoinRequests.Web;
using Sovereign.Core;
using Sovereign.Core.Database;

namespace Sovereign.Bans.JoinRequests;

public class Program : BaseProgram<JoinRequestsConfiguration>
{
    /// <summary>
    /// Runs the program.
    /// </summary>
    public override void Run()
    {
        // Migrate the database.
        if (Environment.GetEnvironmentVariable("DATABASE_DIRECTORY_LOCATION") == null)
        {
            Logger.Debug("Creating bans database for testing due to DATABASE_DIRECTORY_LOCATION not being defined.");
            using var bansContext = new BansContext();
            bansContext.MigrateAsync().Wait();
            
        }
        // Start the join request ban loops.
        var joinRequestBanLoopCollection = new JoinRequestBanLoopCollection();
        
        // Start the web server.
        new JoinRequestsWebServer().StartAsync(joinRequestBanLoopCollection).Wait();
    }

    /// <summary>
    /// Runs the program.
    /// </summary>
    /// <param name="args">Arguments from the command line.</param>
    public static void Main(string[] args)
    {
        new Program()
        {
            DefaultConfiguration = JoinRequestsConfiguration.GetDefaultConfiguration(),
            ConfigurationJsonTypeInfo = JoinRequestsConfigurationJsonContext.Default.JoinRequestsConfiguration,
        }.Main();
    }
}
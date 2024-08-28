using System.Text.Json.Serialization.Metadata;
using Bouncer.Diagnostic;
using Bouncer.State;
using Sovereign.Core.Configuration;

namespace Sovereign.Core;

public abstract class BaseProgram<T> where T : BaseConfiguration
{
    /// <summary>
    /// Default configuration to use when the configuration does not exist.
    /// </summary>
    public T DefaultConfiguration { get; set; } = null!;
    
    /// <summary>
    /// JSON type information for the logging configuration.
    /// </summary>
    public JsonTypeInfo<T> ConfigurationJsonTypeInfo { get; set; } = null!;

    /// <summary>
    /// Runs the program.
    /// </summary>
    public void Main()
    {
        // Prepare the configuration.
        Configurations.PrepareConfiguration(DefaultConfiguration, ConfigurationJsonTypeInfo);

        // Prepare the logging.
        var configuration = Configurations.GetConfiguration<T>();
        var minimumLogLevel = configuration.Logging.MinimumLogLevel;
        Logger.AddNamespaceWhitelist("Sovereign");
        Logger.SetMinimumLogLevel(minimumLogLevel);
        Logger.Debug($"Set log level to {minimumLogLevel}.");
        
        // Run the rest of the program.
        this.Run();
        Logger.WaitForCompletionAsync().Wait();
    }

    /// <summary>
    /// Runs the program.
    /// </summary>
    public abstract void Run();
}
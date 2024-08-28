using Bouncer.State;

namespace Sovereign.Core.Configuration;

public abstract class BaseConfiguration
{
    /// <summary>
    /// Configuration for logging.
    /// </summary>
    public LoggingConfiguration Logging { get; set; } = new LoggingConfiguration();
}
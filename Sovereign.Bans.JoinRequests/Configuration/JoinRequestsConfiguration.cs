using System.Collections.Generic;
using System.Text.Json.Serialization;
using Sovereign.Core.Configuration;

namespace Sovereign.Bans.JoinRequests.Configuration;

public class JoinRequestsGroupConfiguration
{
    /// <summary>
    /// Name of the ban domain (games and groups).
    /// </summary>
    public string? Domain { get; set; }
    
    /// <summary>
    /// Id of the group to deny join requests of.
    /// </summary>
    public long? GroupId { get; set; }

    /// <summary>
    /// Open cloud API key for the game.
    /// </summary>
    public string? ApiKey { get; set; }

    /// <summary>
    /// Time (in seconds) between running steps of the join request ban loop.
    /// </summary>
    public ulong? LoopDelaySeconds { get; set; } = 30;

    /// <summary>
    /// If true, bans and unbans will not be actually performed.
    /// </summary>
    public bool? DryRun { get; set; } = false;
}

public class JoinRequestsConfiguration : BaseConfiguration
{
    /// <summary>
    /// Groups to control bans for.
    /// </summary>
    public List<JoinRequestsGroupConfiguration>? Groups { get; set; } = null!;
    
    /// <summary>
    /// Returns the default configuration to use if the configuration file doesn't exist.
    /// </summary>
    /// <returns>Default configuration to store.</returns>
    public static JoinRequestsConfiguration GetDefaultConfiguration()
    {
        return new JoinRequestsConfiguration()
        {
            Groups = new List<JoinRequestsGroupConfiguration>()
            {
                new JoinRequestsGroupConfiguration()
                {
                    Domain = "MyGame",
                    GroupId = 12345,
                    ApiKey = "OpenCloudApiKey1",
                    DryRun = true,
                },
                new JoinRequestsGroupConfiguration()
                {
                    Domain = "MyGame",
                    GroupId = 23456,
                    ApiKey = "OpenCloudApiKey2",
                    DryRun = true,
                },
            },
        };
    }
}

[JsonSerializable(typeof(JoinRequestsConfiguration))]
[JsonSourceGenerationOptions(WriteIndented = true, IncludeFields = true, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
public partial class JoinRequestsConfigurationJsonContext : JsonSerializerContext
{
}
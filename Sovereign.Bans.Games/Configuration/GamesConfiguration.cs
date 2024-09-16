using System.Collections.Generic;
using System.Text.Json.Serialization;
using Sovereign.Core.Configuration;

namespace Sovereign.Bans.Games.Configuration;

public class GameConfiguration
{
    /// <summary>
    /// Id of the game to assign bans.
    /// </summary>
    public long? GameId { get; set; }

    /// <summary>
    /// Open cloud API key for the game.
    /// </summary>
    public string? ApiKey { get; set; }
}

public class DomainConfiguration
{
    /// <summary>
    /// Name of the ban domain (games and groups).
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Games controlled by the domain.
    /// </summary>
    public List<GameConfiguration>? Games { get; set; }
}

public class GamesConfiguration : BaseConfiguration
{
    /// <summary>
    /// Domains to control bans for.
    /// </summary>
    public List<DomainConfiguration>? Domains { get; set; } = null!;
    
    /// <summary>
    /// Returns the default configuration to use if the configuration file doesn't exist.
    /// </summary>
    /// <returns>Default configuration to store.</returns>
    public static GamesConfiguration GetDefaultConfiguration()
    {
        return new GamesConfiguration()
        {
            Domains = new List<DomainConfiguration>()
            {
                new DomainConfiguration() {
                    Name = "MyGame",
                    Games = new List<GameConfiguration>()
                    {
                        new GameConfiguration()
                        {
                            GameId = 12345,
                            ApiKey = "OpenCloudApiKey1",
                        },
                        new GameConfiguration()
                        {
                            GameId = 23456,
                            ApiKey = "OpenCloudApiKey2",
                        },
                    },
                },
            },
        };
    }
}

[JsonSerializable(typeof(GamesConfiguration))]
[JsonSourceGenerationOptions(WriteIndented = true, IncludeFields = true)]
public partial class GamesConfigurationJsonContext : JsonSerializerContext
{
}
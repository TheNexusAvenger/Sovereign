using System.Collections.Generic;
using System.Text.Json.Serialization;
using Sovereign.Core.Configuration;

namespace Sovereign.Bans.Games.Configuration;

public class GameConfiguration
{
    /// <summary>
    /// Name of the ban domain (games and groups).
    /// </summary>
    public string? Domain { get; set; }
    
    /// <summary>
    /// Id of the game to assign bans.
    /// </summary>
    public long? GameId { get; set; }

    /// <summary>
    /// Open cloud API key for the game.
    /// </summary>
    public string? ApiKey { get; set; }

    /// <summary>
    /// If true, bans and unbans will not be actually performed.
    /// </summary>
    public bool? DryRun { get; set; } = false;
}

public class GamesConfiguration : BaseConfiguration
{
    /// <summary>
    /// Games to control bans for.
    /// </summary>
    public List<GameConfiguration>? Games { get; set; } = null!;
    
    /// <summary>
    /// Returns the default configuration to use if the configuration file doesn't exist.
    /// </summary>
    /// <returns>Default configuration to store.</returns>
    public static GamesConfiguration GetDefaultConfiguration()
    {
        return new GamesConfiguration()
        {
            Games = new List<GameConfiguration>()
            {
                new GameConfiguration()
                {
                    Domain = "MyGame",
                    GameId = 12345,
                    ApiKey = "OpenCloudApiKey1",
                    DryRun = true,
                },
                new GameConfiguration()
                {
                    Domain = "MyGame",
                    GameId = 23456,
                    ApiKey = "OpenCloudApiKey2",
                    DryRun = true,
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
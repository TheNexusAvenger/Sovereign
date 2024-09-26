using System.Collections.Generic;
using System.Text.Json.Serialization;
using Sovereign.Core.Configuration;

namespace Sovereign.Discord.Configuration;

public class DiscordServerConfiguration
{
    /// <summary>
    /// Id of the Discord server.
    /// </summary>
    [JsonPropertyName("id")]
    public ulong? Id { get; set; }
    
    /// <summary>
    /// Domain the Discord server controls.
    /// Due to command limitations, 1 server can only control 1 domain.
    /// </summary>
    [JsonPropertyName("domain")]
    public string? Domain { get; set; }
}

public class DiscordDomainConfiguration
{
    /// <summary>
    /// Domain controlled by Discord servers.
    /// </summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }
    
    /// <summary>
    /// Secret key used for the authorization header.
    /// </summary>
    [JsonPropertyName("apiSecretKey")]
    public string? ApiSecretKey { get; set; }
}

public class DiscordBotConfiguration
{
    /// <summary>
    /// Token for the Discord bot.
    /// </summary>
    [JsonPropertyName("token")]
    public string? Token { get; set; }
    
    /// <summary>
    /// Discord server controlled by the bots.
    /// This functions as a server whitelist for the bot.
    /// </summary>
    [JsonPropertyName("servers")]
    public List<DiscordServerConfiguration>? Servers { get; set; }
}

public class DiscordConfiguration : BaseConfiguration
{
    /// <summary>
    /// Discord bot configuration.
    /// </summary>
    [JsonPropertyName("discord")]
    public DiscordBotConfiguration? Discord { get; set; }
    
    /// <summary>
    /// Domain configurations.
    /// </summary>
    [JsonPropertyName("domains")]
    public List<DiscordDomainConfiguration>? Domains { get; set; }
    
    /// <summary>
    /// Returns the default configuration to use if the configuration file doesn't exist.
    /// </summary>
    /// <returns>Default configuration to store.</returns>
    public static DiscordConfiguration GetDefaultConfiguration()
    {
        return new DiscordConfiguration()
        {
            Discord = new DiscordBotConfiguration()
            {
                Token = "default",
                Servers = new List<DiscordServerConfiguration>()
                {
                    new DiscordServerConfiguration()
                    {
                        Id = 12345,
                        Domain = "MyGame",
                    },
                },
            },
            Domains = new List<DiscordDomainConfiguration>()
            {
                new DiscordDomainConfiguration()
                {
                    Name = "MyGame",
                    ApiSecretKey = "TestSecretKey",
                }  
            },
        };
    }
}

[JsonSerializable(typeof(DiscordConfiguration))]
[JsonSourceGenerationOptions(WriteIndented = true, IncludeFields = true)]
public partial class DiscordConfigurationJsonContext : JsonSerializerContext
{
}
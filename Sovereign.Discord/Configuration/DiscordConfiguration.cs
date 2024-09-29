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

public class DiscordDomainBanOptionConfiguration
{
    /// <summary>
    /// Name of the ban option shown in the /startban menu.
    /// </summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }
    
    /// <summary>
    /// Description of the ban option shown in the /startban menu.
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; set; }
    
    /// <summary>
    /// Default display reason to show when banning.
    /// </summary>
    [JsonPropertyName("defaultDisplayReason")]
    public string? DefaultDisplayReason { get; set; }
    
    /// <summary>
    /// Default private reason to show when banning.
    /// </summary>
    [JsonPropertyName("defaultPrivateReason")]
    public string? DefaultPrivateReason { get; set; }
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
    
    /// <summary>
    /// Options to show when banning.
    /// </summary>
    [JsonPropertyName("banOptions")]
    public List<DiscordDomainBanOptionConfiguration>? BanOptions { get; set; }
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
                    BanOptions = new List<DiscordDomainBanOptionConfiguration>()
                    {
                        new DiscordDomainBanOptionConfiguration()
                        {
                            Name = "Exploiting",
                            Description = "Please specify details in the private reason. Use the Discord server in the game's social links to appeal",
                            DefaultDisplayReason = "Banned for exploiting.",
                        },
                        new DiscordDomainBanOptionConfiguration()
                        {
                            Name = "Harassment",
                            DefaultDisplayReason = "Banned for harassment. Use the Discord server in the game's social links to appeal",
                            DefaultPrivateReason = "No information given.",
                        },
                        new DiscordDomainBanOptionConfiguration()
                        {
                            Name = "Other",
                            DefaultDisplayReason = "You are banned. Use the Discord server in the game's social links to appeal",
                        },
                    },
                },
            },
        };
    }
}

[JsonSerializable(typeof(DiscordConfiguration))]
[JsonSourceGenerationOptions(WriteIndented = true, IncludeFields = true)]
public partial class DiscordConfigurationJsonContext : JsonSerializerContext
{
}
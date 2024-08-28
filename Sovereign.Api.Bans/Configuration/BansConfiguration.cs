using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bouncer.State;
using Sovereign.Core.Configuration;

namespace Sovereign.Api.Bans.Configuration;

public enum AuthenticationRuleAction
{
    Allow,
    Deny
}

public class AuthenticationRuleEntry : BaseRuleEntry<AuthenticationRuleAction?>
{
}

public class DomainConfiguration
{
    /// <summary>
    /// Name of the ban domain (games and groups).
    /// </summary>
    public string? Name { get; set; } = null!;

    /// <summary>
    /// Rules to authenticate users.
    /// </summary>
    public List<AuthenticationRuleEntry>? Rules { get; set; } = null!;
}

public class BansConfiguration : BaseConfiguration
{
    /// <summary>
    /// Domains to control bans for.
    /// </summary>
    public List<DomainConfiguration>? Domains { get; set; } = null!;
    
    /// <summary>
    /// Returns the default configuration to use if the configuration file doesn't exist.
    /// </summary>
    /// <returns>Default configuration to store.</returns>
    public static BansConfiguration GetDefaultConfiguration()
    {
        return new BansConfiguration()
        {
            Domains = new List<DomainConfiguration>()
            {
                new DomainConfiguration() {
                    Name = "MyGame",
                    Rules = new List<AuthenticationRuleEntry>()
                    {
                        new AuthenticationRuleEntry()
                        {
                            Name = "Test Rule 1",
                            Rule = "not GroupRankIs(12345, \"EqualTo\", 230)",
                            Action = AuthenticationRuleAction.Deny,
                        },
                        new AuthenticationRuleEntry()
                        {
                            Name = "Test Rule 2",
                            Rule = "not GroupRankIs(12345, \"LessThan\", 200)",
                            Action = AuthenticationRuleAction.Allow,
                        },
                    },
                },
            },
        };
    }
}


[JsonSerializable(typeof(BansConfiguration))]
[JsonSourceGenerationOptions(WriteIndented = true, IncludeFields = true)]
public partial class BansConfigurationJsonContext : JsonSerializerContext
{
}
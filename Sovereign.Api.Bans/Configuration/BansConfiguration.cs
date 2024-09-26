using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bouncer.Diagnostic;
using Bouncer.Expression;
using Bouncer.Parser;
using Bouncer.State;
using Sovereign.Core.Configuration;
using Sovereign.Core.Model.Response;
using Sprache;

namespace Sovereign.Api.Bans.Configuration;

public enum AuthenticationRuleAction
{
    Allow,
    Deny
}

public class AuthenticationRuleEntry : BaseRuleEntry<AuthenticationRuleAction>
{
    /// <summary>
    /// Action to perform when the rule applies.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter<AuthenticationRuleAction>))]
    public override AuthenticationRuleAction? Action { get; set; }
}

public class DomainConfiguration
{
    /// <summary>
    /// Name of the ban domain (games and groups).
    /// </summary>
    public string? Name { get; set; }
    
    /// <summary>
    /// API keys used to authenticate requests.
    /// </summary>
    public List<string>? ApiKeys { get; set; }
    
    /// <summary>
    /// Secret keys used to authenticate requests with HMAC SHA256.
    /// </summary>
    public List<string>? SecretKeys { get; set; }

    /// <summary>
    /// Rules to authenticate users.
    /// </summary>
    public List<AuthenticationRuleEntry>? Rules { get; set; }
    
    /// <summary>
    /// Optional list of group id ranks to compare when a banning a user.
    /// This is meant to prevent banning users with higher ranks.
    /// </summary>
    public List<long>? GroupIdRankChecks { get; set; }

    /// <summary>
    /// Verifies that a Roblox user passes the rules for the domain.
    /// </summary>
    /// <param name="robloxUserId">Roblox user id to verify.</param>
    /// <returns>Error response for the authorization if the user is unauthorized.</returns>
    public JsonResponse? IsRobloxUserAuthorized(long robloxUserId)
    {
        // Return an error if there is no rules.
        if (this.Rules == null)
        {
            Logger.Error($"Domain \"{this.Name}\" was does not have rules.");
            return new JsonResponse(new SimpleResponse(ResponseStatus.ServerConfigurationError), 503);
        }
        
        // Iterate over the rules and return if there is a server error.
        var action = AuthenticationRuleAction.Deny;
        foreach (var rule in this.Rules)
        {
            try
            {
                if (!Condition.FromParsedCondition(ExpressionParser.FullExpressionParser.Parse(rule.Rule)).Evaluate(robloxUserId)) continue;
                action = rule.Action ?? AuthenticationRuleAction.Deny;
                break;
            }
            catch (Exception e)
            {
                Logger.Error($"Error evaluating rule for {robloxUserId} in domain \"{this.Name}\".\n{e}");
                return new JsonResponse(new SimpleResponse(ResponseStatus.ServerProcessingError), 503); 
            }
        }

        // Return an error if the user is unauthorized.
        return action == AuthenticationRuleAction.Deny ? SimpleResponse.ForbiddenResponse : null;
    }
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
                    GroupIdRankChecks = new List<long>()
                    {
                        12345L,
                    },
                    ApiKeys = new List<string>() { "TestApiKey" },
                    SecretKeys = new List<string>() { "TestSecretKey" },
                    Rules = new List<AuthenticationRuleEntry>()
                    {
                        new AuthenticationRuleEntry()
                        {
                            Name = "Test Rule 1",
                            Rule = "IsUser(12345)",
                            Action = AuthenticationRuleAction.Allow,
                        },
                        new AuthenticationRuleEntry()
                        {
                            Name = "Test Rule 2",
                            Rule = "GroupRankIs(12345, \"EqualTo\", 230)",
                            Action = AuthenticationRuleAction.Deny,
                        },
                        new AuthenticationRuleEntry()
                        {
                            Name = "Test Rule 3",
                            Rule = "GroupRankIs(12345, \"AtLeast\", 200)",
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
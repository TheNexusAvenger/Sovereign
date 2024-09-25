using Bouncer.Diagnostic;
using Bouncer.Diagnostic.Model;
using Bouncer.State;
using Sovereign.Api.Bans.Configuration;

namespace Sovereign.Api.Bans.Diagnostic;

public class BansConfigurationVerification : BaseConfigurationVerification<AuthenticationRuleAction>
{
    /// <summary>
    /// Verifies the rules for the groups.
    /// </summary>
    /// <param name="configuration">Configuration to validate.</param>
    public static VerifyRulesResult VerifyRules(BansConfiguration configuration)
    {
        // Get the configuration and return if there are no rules.
        var result = new VerifyRulesResult();
        if (configuration.Domains == null)
        {
            Logger.Warn("\"Domains\" is missing from the configuration.");
            result.TotalRuleConfigurationErrors += 1;
            return result;
        }
        
        // Iterate over the domains and add any errors.
        foreach (var domainRules in configuration.Domains)
        {
            // Add configuration errors for missing fields.
            Logger.Info($"Configuration for domain \"{domainRules.Name}\":");
            if (domainRules.Name == null)
            {
                Logger.Error("\"Name\" is missing from Domains configuration entry.");
                result.TotalRuleConfigurationErrors += 1;
            }
            if (domainRules.Rules == null)
            {
                Logger.Error("\"Rules\" is missing from Domains configuration entry.");
                result.TotalRuleConfigurationErrors += 1;
            }
            if (domainRules.Rules == null) continue;
            
            // Iterate over the rules.
            foreach (var rule in domainRules.Rules)
            {
                VerifyRule(rule, AuthenticationRuleAction.Deny, result);
            }
        }
        
        // Return the final errors.
        return result;
    }

    /// <summary>
    /// Verifies the rules for the groups.
    /// </summary>
    public static VerifyRulesResult VerifyRules()
    {
        return VerifyRules(Configurations.GetConfiguration<BansConfiguration>());
    }
}
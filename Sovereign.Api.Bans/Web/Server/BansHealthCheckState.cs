using Bouncer.Diagnostic;
using Bouncer.Diagnostic.Model;
using Bouncer.State;
using Bouncer.Web.Server.Model;
using Sovereign.Api.Bans.Configuration;
using Sovereign.Api.Bans.Diagnostic;
using Sovereign.Api.Bans.Web.Server.Model;

namespace Sovereign.Api.Bans.Web.Server;

public class BansHealthCheckState
{
    /// <summary>
    /// Last result for the rules being verified.
    /// </summary>
    private VerifyRulesResult? _lastVerifyRulesResult;
    
    /// <summary>
    /// Connects the configuration changing.
    /// </summary>
    public void ConnectConfigurationChanges()
    {
        // Set the initial health check values.
        this._lastVerifyRulesResult = BansConfigurationVerification.VerifyRules();
        
        // Connect the configuration changing.
        Configurations.GetConfigurationState<BansConfiguration>().ConfigurationChanged += (_) => this.UpdateVerifyRulesResult();
    }
    
    /// <summary>
    /// Determines the current health check result.
    /// </summary>
    /// <param name="verifyRulesResult">Rules result to create the health check for.</param>
    /// <returns>The current health check result.</returns>
    public BansHealthCheckResult GetHealthCheckResult(VerifyRulesResult verifyRulesResult)
    {
        var hasConfigurationIssues = (verifyRulesResult.TotalRuleConfigurationErrors != 0 ||
                                      verifyRulesResult.TotalParseErrors != 0 ||
                                      verifyRulesResult.TotalTransformErrors != 0);
        return new BansHealthCheckResult()
        {
            Status = (hasConfigurationIssues ? HealthCheckResultStatus.Down : HealthCheckResultStatus.Up),
            Configuration = new HealthCheckConfigurationProblems()
            {
                Status = (hasConfigurationIssues ? HealthCheckResultStatus.Down : HealthCheckResultStatus.Up),
                TotalRuleConfigurationErrors = verifyRulesResult.TotalRuleConfigurationErrors,
                TotalRuleParseErrors = verifyRulesResult.TotalParseErrors,
                TotalRuleTransformErrors = verifyRulesResult.TotalTransformErrors,
            },
        };
    }

    /// <summary>
    /// Determines the current health check result.
    /// </summary>
    /// <returns>The current health check result.</returns>
    public BansHealthCheckResult GetHealthCheckResult()
    {
        return this.GetHealthCheckResult(this._lastVerifyRulesResult!);
    }
    
    /// <summary>
    /// Updates the verify rules result.
    /// </summary>
    private void UpdateVerifyRulesResult()
    {
        this._lastVerifyRulesResult = BansConfigurationVerification.VerifyRules();
    }
}
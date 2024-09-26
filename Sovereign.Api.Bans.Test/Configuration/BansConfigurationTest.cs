using System.Collections.Generic;
using NUnit.Framework;
using Sovereign.Api.Bans.Configuration;
using Sovereign.Core.Model.Response;

namespace Sovereign.Api.Bans.Test.Configuration;

public class DomainConfigurationTest
{
    [Test]
    public void TestIsRobloxUserAuthorizedNullRules()
    {
        var testConfiguration = new DomainConfiguration();
        var response = testConfiguration.IsRobloxUserAuthorized(12345);
        Assert.That(((SimpleResponse) response!.Response).Status, Is.EqualTo(ResponseStatus.ServerConfigurationError));
        Assert.That(response.StatusCode, Is.EqualTo(503));
    }
    
    [Test]
    public void TestIsRobloxUserAuthorizedRullException()
    {
        var testConfiguration = new DomainConfiguration()
        {
            Rules = new List<AuthenticationRuleEntry>()
            {
                new AuthenticationRuleEntry()
                {
                    Rule = "UnknownRule()",
                    Action = AuthenticationRuleAction.Deny,
                },
            },
        };
        var response = testConfiguration.IsRobloxUserAuthorized(12345);
        Assert.That(((SimpleResponse) response!.Response).Status, Is.EqualTo(ResponseStatus.ServerProcessingError));
        Assert.That(response.StatusCode, Is.EqualTo(503));
    }
    
    [Test]
    public void TestIsRobloxUserAuthorizedDenyByDefault()
    {
        var testConfiguration = new DomainConfiguration()
        {
            Rules = new List<AuthenticationRuleEntry>(),
        };
        var response = testConfiguration.IsRobloxUserAuthorized(12345);
        Assert.That(((SimpleResponse) response!.Response).Status, Is.EqualTo(ResponseStatus.Forbidden));
        Assert.That(response.StatusCode, Is.EqualTo(403));
    }
    
    [Test]
    public void TestIsRobloxUserAuthorizedDenyRule()
    {
        var testConfiguration = new DomainConfiguration()
        {
            Rules = new List<AuthenticationRuleEntry>()
            {
                new AuthenticationRuleEntry()
                {
                    Rule = "IsUser(12345)",
                    Action = AuthenticationRuleAction.Deny,
                },
            },
        };
        var response = testConfiguration.IsRobloxUserAuthorized(12345);
        Assert.That(((SimpleResponse) response!.Response).Status, Is.EqualTo(ResponseStatus.Forbidden));
        Assert.That(response.StatusCode, Is.EqualTo(403));
    }
    
    [Test]
    public void TestIsRobloxUserAuthorizedAllowRule()
    {
        var testConfiguration = new DomainConfiguration()
        {
            Rules = new List<AuthenticationRuleEntry>()
            {
                new AuthenticationRuleEntry()
                {
                    Rule = "IsUser(12345)",
                    Action = AuthenticationRuleAction.Allow,
                },
            },
        };
        var response = testConfiguration.IsRobloxUserAuthorized(12345);
        Assert.That(response, Is.Null);
    }
}
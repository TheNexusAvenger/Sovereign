using System.Collections.Generic;
using Bouncer.Expression;
using Bouncer.Expression.Definition;
using NUnit.Framework;
using Sovereign.Api.Bans.Configuration;
using Sovereign.Api.Bans.Diagnostic;

namespace Sovereign.Api.Bans.Test.Diagnostic;

public class BansConfigurationVerificationTest
{
    [OneTimeSetUp]
    public void SetUp()
    {
        Condition.AddConditionDefinition(new ConditionDefinition()
        {
            Name = "Test",
            TotalArguments = 0,
            Evaluate = (_, _) => true,
        });
    }
    
    [Test]
    public void TestVerifyRules()
    {
        var result = BansConfigurationVerification.VerifyRules(new BansConfiguration()
        {
            Domains = new List<DomainConfiguration>()
            {
                new DomainConfiguration()
                {
                    Name = "test",
                    Rules = new List<AuthenticationRuleEntry>()
                    {
                        new AuthenticationRuleEntry()
                        {
                            Name = "Test Rule",
                            Rule = "Test()",
                            Action = AuthenticationRuleAction.Allow,
                        },
                    },
                }
            },
        });
        Assert.That(result.TotalRuleConfigurationErrors, Is.EqualTo(0));
        Assert.That(result.TotalParseErrors, Is.EqualTo(0));
        Assert.That(result.TotalTransformErrors, Is.EqualTo(0));
    }
    
    [Test]
    public void TestVerifyRulesNullGroups()
    {
        var result = BansConfigurationVerification.VerifyRules(new BansConfiguration()
        {
            Domains = null,
        });
        Assert.That(result.TotalRuleConfigurationErrors, Is.EqualTo(1));
        Assert.That(result.TotalParseErrors, Is.EqualTo(0));
        Assert.That(result.TotalTransformErrors, Is.EqualTo(0));
    }
    
    [Test]
    public void TestVerifyRulesNullGroupConfigurationFields()
    {
        var result = BansConfigurationVerification.VerifyRules(new BansConfiguration()
        {
            Domains = new List<DomainConfiguration>()
            {
                new DomainConfiguration()
                {
                    Name = null,
                    Rules = null,
                },
            },
        });
        Assert.That(result.TotalRuleConfigurationErrors, Is.EqualTo(2));
        Assert.That(result.TotalParseErrors, Is.EqualTo(0));
        Assert.That(result.TotalTransformErrors, Is.EqualTo(0));
    }
    
    [Test]
    public void TestVerifyRulesNullRuleFields()
    {
        var result = BansConfigurationVerification.VerifyRules(new BansConfiguration()
        {
            Domains = new List<DomainConfiguration>()
            {
                new DomainConfiguration()
                {
                    Name = "test",
                    Rules = new List<AuthenticationRuleEntry>()
                    {
                        new AuthenticationRuleEntry()
                        {
                            Name = null,
                            Rule = null,
                            Action = null,
                        },
                    },
                },
            },
        });
        Assert.That(result.TotalRuleConfigurationErrors, Is.EqualTo(2));
        Assert.That(result.TotalParseErrors, Is.EqualTo(0));
        Assert.That(result.TotalTransformErrors, Is.EqualTo(0));
    }
    
    [Test]
    public void TestVerifyRulesParseError()
    {
        var result = BansConfigurationVerification.VerifyRules(new BansConfiguration()
        {
            Domains = new List<DomainConfiguration>()
            {
                new DomainConfiguration()
                {
                    Name = "test",
                    Rules = new List<AuthenticationRuleEntry>()
                    {
                        new AuthenticationRuleEntry()
                        {
                            Name = "Test Rule",
                            Rule = "Test(",
                            Action = AuthenticationRuleAction.Allow,
                        },
                    },
                },
            },
        });
        Assert.That(result.TotalRuleConfigurationErrors, Is.EqualTo(0));
        Assert.That(result.TotalParseErrors, Is.EqualTo(1));
        Assert.That(result.TotalTransformErrors, Is.EqualTo(0));
    }
    
    [Test]
    public void TestVerifyRulesTransformError()
    {
        var result = BansConfigurationVerification.VerifyRules(new BansConfiguration()
        {
            Domains = new List<DomainConfiguration>()
            {
                new DomainConfiguration()
                {
                    Name = "test",
                    Rules = new List<AuthenticationRuleEntry>()
                    {
                        new AuthenticationRuleEntry()
                        {
                            Name = "Test Rule",
                            Rule = "Unknown()",
                            Action = AuthenticationRuleAction.Allow,
                        },
                    },
                },
            },
        });
        Assert.That(result.TotalRuleConfigurationErrors, Is.EqualTo(0));
        Assert.That(result.TotalParseErrors, Is.EqualTo(0));
        Assert.That(result.TotalTransformErrors, Is.EqualTo(1));
    }
}
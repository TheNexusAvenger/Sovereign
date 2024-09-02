using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Sovereign.Api.Bans.Configuration;
using Sovereign.Api.Bans.Test.Web.Server.Controller.Shim;
using Sovereign.Api.Bans.Web.Server.Controller;
using Sovereign.Api.Bans.Web.Server.Model;
using Sovereign.Core.Database.Model.Api;
using Sovereign.Core.Model;
using Sovereign.Core.Model.Response;
using Sovereign.Core.Test.Model.Request.Authorization;

namespace Sovereign.Api.Bans.Test.Web.Server.Controller;

public class BanControllerTest
{
    private TestBanControllerResources _testResources;
    private BanController _banController;

    [SetUp]
    public void SetUp()
    {
        this._testResources = new TestBanControllerResources();
        this._banController = new BanController()
        {
            ControllerResources = this._testResources,
        };
    }
    
    [Test]
    public void TestHandleBanRequestAllFieldsMissing()
    {
        var response = this._banController.HandleBanRequest(GetContext(new BanRequest())).Result;
        var validationErrorResponse = (ValidationErrorResponse) response.Response;
        Assert.That(response.StatusCode, Is.EqualTo(400));
        Assert.That(validationErrorResponse.Errors.Count, Is.EqualTo(4));
        Assert.That(validationErrorResponse.Errors[0].Path, Is.EqualTo("domain"));
        Assert.That(validationErrorResponse.Errors[0].Message, Is.EqualTo("domain was not provided."));
        Assert.That(validationErrorResponse.Errors[1].Path, Is.EqualTo("authentication"));
        Assert.That(validationErrorResponse.Errors[1].Message, Is.EqualTo("authentication was not provided."));
        Assert.That(validationErrorResponse.Errors[2].Path, Is.EqualTo("action"));
        Assert.That(validationErrorResponse.Errors[2].Message, Is.EqualTo("action was not provided."));
        Assert.That(validationErrorResponse.Errors[3].Path, Is.EqualTo("reason"));
        Assert.That(validationErrorResponse.Errors[3].Message, Is.EqualTo("reason was not provided."));
    }
    
    [Test]
    public void TestHandleBanRequestAllSubFieldsMissing()
    {
        var response = this._banController.HandleBanRequest(GetContext(new BanRequest()
        {
            Domain = "Domain",
            Authentication = new BanRequestAuthentication(),
            Action = new BanRequestAction(),
            Reason = new BanRequestReason(),
        })).Result;
        var validationErrorResponse = (ValidationErrorResponse) response.Response;
        Assert.That(response.StatusCode, Is.EqualTo(400));
        Assert.That(validationErrorResponse.Errors.Count, Is.EqualTo(6));
        Assert.That(validationErrorResponse.Errors[0].Path, Is.EqualTo("authentication.method"));
        Assert.That(validationErrorResponse.Errors[0].Message, Is.EqualTo("authentication.method was not provided."));
        Assert.That(validationErrorResponse.Errors[1].Path, Is.EqualTo("authentication.data"));
        Assert.That(validationErrorResponse.Errors[1].Message, Is.EqualTo("authentication.data was not provided."));
        Assert.That(validationErrorResponse.Errors[2].Path, Is.EqualTo("action.type"));
        Assert.That(validationErrorResponse.Errors[2].Message, Is.EqualTo("action.type was not provided."));
        Assert.That(validationErrorResponse.Errors[3].Path, Is.EqualTo("action.userIds"));
        Assert.That(validationErrorResponse.Errors[3].Message, Is.EqualTo("action.userIds was not provided."));
        Assert.That(validationErrorResponse.Errors[4].Path, Is.EqualTo("reason.display"));
        Assert.That(validationErrorResponse.Errors[4].Message, Is.EqualTo("reason.display was not provided."));
        Assert.That(validationErrorResponse.Errors[5].Path, Is.EqualTo("reason.private"));
        Assert.That(validationErrorResponse.Errors[5].Message, Is.EqualTo("reason.private was not provided."));
    }
    
    [Test]
    public void TestHandleBanRequestAllInvalidSubFieldsMissing()
    {
        var response = this._banController.HandleBanRequest(GetContext(new BanRequest()
        {
            Domain = "Domain",
            Authentication = new BanRequestAuthentication()
            {
                Method = "Method",
                Data = "Data",
            },
            Action = new BanRequestAction()
            {
                Type = BanAction.Ban,
                UserIds = new List<long>(),
                Duration = 0,
            },
            Reason = new BanRequestReason()
            {
                Display = "",
                Private = "",
            },
        })).Result;
        var validationErrorResponse = (ValidationErrorResponse) response.Response;
        Assert.That(response.StatusCode, Is.EqualTo(400));
        Assert.That(validationErrorResponse.Errors.Count, Is.EqualTo(2));
        Assert.That(validationErrorResponse.Errors[0].Path, Is.EqualTo("action.userIds"));
        Assert.That(validationErrorResponse.Errors[0].Message, Is.EqualTo("action.userIds was empty."));
        Assert.That(validationErrorResponse.Errors[1].Path, Is.EqualTo("action.duration"));
        Assert.That(validationErrorResponse.Errors[1].Message, Is.EqualTo("action.duration was not a positive number."));
    }
    
    [Test]
    public void TestHandleBanRequestNoDomains()
    {
        this._testResources.Configuration = new BansConfiguration()
        {
            Domains = null,
        };
        var response = this._banController.HandleBanRequest(GetContext(new BanRequest()
        {
            Domain = "TestDomain",
            Authentication = new BanRequestAuthentication()
            {
                Method = "Roblox",
                Data = "12345",
            },
            Action = new BanRequestAction()
            {
                Type = BanAction.Ban,
                UserIds = new List<long>() { 23456 },
            },
            Reason = new BanRequestReason()
            {
                Display = "Test Message 1",
                Private = "Test Message 2",
            },
        })).Result;
        var simpleResponse = response.Response;
        Assert.That(response.StatusCode, Is.EqualTo(503));
        Assert.That(simpleResponse.Status, Is.EqualTo("ServerConfigurationError"));
    }
    
    [Test]
    public void TestHandleBanRequestMissingDomain()
    {
        this._testResources.Configuration = new BansConfiguration()
        {
            Domains = new List<DomainConfiguration>(),
        };
        var response = this._banController.HandleBanRequest(GetContext(new BanRequest()
        {
            Domain = "TestDomain",
            Authentication = new BanRequestAuthentication()
            {
                Method = "Roblox",
                Data = "12345",
            },
            Action = new BanRequestAction()
            {
                Type = BanAction.Ban,
                UserIds = new List<long>() { 23456 },
            },
            Reason = new BanRequestReason()
            {
                Display = "Test Message 1",
                Private = "Test Message 2",
            },
        })).Result;
        var simpleResponse = response.Response;
        Assert.That(response.StatusCode, Is.EqualTo(401));
        Assert.That(simpleResponse.Status, Is.EqualTo("Unauthorized"));
    }
    
    [Test]
    public void TestHandleBanRequestUnauthorizedHeader()
    {
        PrepareValidConfiguration();
        var context = GetContext(this.PrepareValidResponse());
        context.Authorized = false;
        var response = this._banController.HandleBanRequest(context).Result;
        var simpleResponse = response.Response;
        Assert.That(response.StatusCode, Is.EqualTo(401));
        Assert.That(simpleResponse.Status, Is.EqualTo("Unauthorized"));
    }
    
    [Test]
    public void TestHandleBanRequestNoLinkData()
    {
        this._testResources.Configuration = new BansConfiguration()
        {
            Domains = new List<DomainConfiguration>()
            {
                new DomainConfiguration()
                {
                    Name = "TestDomain",
                    Rules = new List<AuthenticationRuleEntry>(),
                },
            },
        };
        var response = this._banController.HandleBanRequest(GetContext(new BanRequest()
        {
            Domain = "TestDomain",
            Authentication = new BanRequestAuthentication()
            {
                Method = "ExternalLink",
                Data = "Unknown",
            },
            Action = new BanRequestAction()
            {
                Type = BanAction.Ban,
                UserIds = new List<long>() { 23456 },
            },
            Reason = new BanRequestReason()
            {
                Display = "Test Message 1",
                Private = "Test Message 2",
            },
        })).Result;
        var simpleResponse = response.Response;
        Assert.That(response.StatusCode, Is.EqualTo(401));
        Assert.That(simpleResponse.Status, Is.EqualTo("Unauthorized"));
    }
    
    [Test]
    public void TestHandleBanRequestInvalidRobloxId()
    {
        this._testResources.Configuration = new BansConfiguration()
        {
            Domains = new List<DomainConfiguration>()
            {
                new DomainConfiguration()
                {
                    Name = "TestDomain",
                    Rules = new List<AuthenticationRuleEntry>(),
                },
            },
        };
        var response = this._banController.HandleBanRequest(GetContext(new BanRequest()
        {
            Domain = "TestDomain",
            Authentication = new BanRequestAuthentication()
            {
                Method = "Roblox",
                Data = "Unknown",
            },
            Action = new BanRequestAction()
            {
                Type = BanAction.Ban,
                UserIds = new List<long>() { 23456 },
            },
            Reason = new BanRequestReason()
            {
                Display = "Test Message 1",
                Private = "Test Message 2",
            },
        })).Result;
        var simpleResponse = response.Response;
        Assert.That(response.StatusCode, Is.EqualTo(401));
        Assert.That(simpleResponse.Status, Is.EqualTo("Unauthorized"));
    }
    
    [Test]
    public void TestHandleBanRequestNullRules()
    {
        this._testResources.Configuration = new BansConfiguration()
        {
            Domains = new List<DomainConfiguration>()
            {
                new DomainConfiguration()
                {
                    Name = "TestDomain",
                    Rules = null,
                },
            },
        };
        var response = this._banController.HandleBanRequest(GetContext(this.PrepareValidResponse())).Result;
        var simpleResponse = response.Response;
        Assert.That(response.StatusCode, Is.EqualTo(503));
        Assert.That(simpleResponse.Status, Is.EqualTo("ServerConfigurationError"));
    }
    
    [Test]
    public void TestHandleBanRequestInvalidRule()
    {
        this._testResources.Configuration = new BansConfiguration()
        {
            Domains = new List<DomainConfiguration>()
            {
                new DomainConfiguration()
                {
                    Name = "TestDomain",
                    Rules = new List<AuthenticationRuleEntry>()
                    {
                        new AuthenticationRuleEntry()
                        {
                            Rule = "InvalidRule",
                        },
                    },
                },
            },
        };
        var response = this._banController.HandleBanRequest(GetContext(this.PrepareValidResponse())).Result;
        var simpleResponse = response.Response;
        Assert.That(response.StatusCode, Is.EqualTo(503));
        Assert.That(simpleResponse.Status, Is.EqualTo("ServerError"));
    }
    
    [Test]
    public void TestHandleBanRequestUnauthorized()
    {
        this._testResources.Configuration = new BansConfiguration()
        {
            Domains = new List<DomainConfiguration>()
            {
                new DomainConfiguration()
                {
                    Name = "TestDomain",
                    Rules = new List<AuthenticationRuleEntry>(),
                },
            },
        };
        var response = this._banController.HandleBanRequest(GetContext(this.PrepareValidResponse())).Result;
        var simpleResponse = response.Response;
        Assert.That(response.StatusCode, Is.EqualTo(403));
        Assert.That(simpleResponse.Status, Is.EqualTo("Forbidden"));
    }

    [Test]
    public void TestHandleBanRequestPermanentBan()
    {
        this.PrepareValidConfiguration();
        var response = this._banController.HandleBanRequest(GetContext(this.PrepareValidResponse())).Result;
        var banResponse = (BanResponse) response.Response;
        Assert.That(response.StatusCode, Is.EqualTo(200));
        Assert.That(banResponse.Status, Is.EqualTo("Success"));
        Assert.That(banResponse.BannedUserIds, Is.EqualTo(new List<long>() { 23456, }));
        Assert.That(banResponse.UnbannedUserIds, Is.EqualTo(new List<long>()));

        var latestBanEntry = this._testResources.GetBansContext().BanEntries.OrderBy(entry => entry.StartTime).Last();
        Assert.That(latestBanEntry.TargetRobloxUserId, Is.EqualTo(23456));
        Assert.That(latestBanEntry.Domain, Is.EqualTo("TestDomain"));
        Assert.That(latestBanEntry.Action, Is.EqualTo(BanAction.Ban));
        Assert.That(Math.Abs((DateTime.Now - latestBanEntry.StartTime).TotalSeconds), Is.LessThan(10));
        Assert.That(latestBanEntry.EndTime, Is.Null);
        Assert.That(latestBanEntry.ActingRobloxUserId, Is.EqualTo(12345));
        Assert.That(latestBanEntry.DisplayReason, Is.EqualTo("Test Message 1"));
        Assert.That(latestBanEntry.PrivateReason, Is.EqualTo("Test Message 2"));
    }

    [Test]
    public void TestHandleBanRequestTemporaryBan()
    {
        this.PrepareValidConfiguration();
        var request = this.PrepareValidResponse();
        request.Action!.Duration = 120;
        var response = this._banController.HandleBanRequest(GetContext(request)).Result;
        var banResponse = (BanResponse) response.Response;
        Assert.That(response.StatusCode, Is.EqualTo(200));
        Assert.That(banResponse.Status, Is.EqualTo("Success"));
        Assert.That(banResponse.BannedUserIds, Is.EqualTo(new List<long>() { 23456, }));
        Assert.That(banResponse.UnbannedUserIds, Is.EqualTo(new List<long>()));

        var latestBanEntry = this._testResources.GetBansContext().BanEntries.OrderBy(entry => entry.StartTime).Last();
        Assert.That(latestBanEntry.TargetRobloxUserId, Is.EqualTo(23456));
        Assert.That(latestBanEntry.Domain, Is.EqualTo("TestDomain"));
        Assert.That(latestBanEntry.Action, Is.EqualTo(BanAction.Ban));
        Assert.That(Math.Abs((DateTime.Now - latestBanEntry.StartTime).TotalSeconds), Is.LessThan(10));
        Assert.That(Math.Abs((latestBanEntry.EndTime!.Value - DateTime.Now).TotalSeconds - 120), Is.LessThan(10));
        Assert.That(latestBanEntry.ActingRobloxUserId, Is.EqualTo(12345));
        Assert.That(latestBanEntry.DisplayReason, Is.EqualTo("Test Message 1"));
        Assert.That(latestBanEntry.PrivateReason, Is.EqualTo("Test Message 2"));
    }

    [Test]
    public void TestHandleBanRequestUpdatedBan()
    {
        var context = this._testResources.GetBansContext();
        context.BanEntries.Add(new BanEntry()
        {
            TargetRobloxUserId = 23456,
            Domain = "TestDomain",
            Action = BanAction.Ban,
            StartTime = DateTime.Now.AddDays(-2),
            ActingRobloxUserId = 12345,
            DisplayReason = "Old Message 1",
            PrivateReason = "Old Message 2",
        });
        context.SaveChanges();
        
        this.PrepareValidConfiguration();
        var response = this._banController.HandleBanRequest(GetContext(this.PrepareValidResponse())).Result;
        var banResponse = (BanResponse) response.Response;
        Assert.That(response.StatusCode, Is.EqualTo(200));
        Assert.That(banResponse.Status, Is.EqualTo("Success"));
        Assert.That(banResponse.BannedUserIds, Is.EqualTo(new List<long>() { 23456, }));
        Assert.That(banResponse.UnbannedUserIds, Is.EqualTo(new List<long>()));

        var latestBanEntry = this._testResources.GetBansContext().BanEntries.OrderBy(entry => entry.StartTime).Last();
        Assert.That(latestBanEntry.TargetRobloxUserId, Is.EqualTo(23456));
        Assert.That(latestBanEntry.Domain, Is.EqualTo("TestDomain"));
        Assert.That(latestBanEntry.Action, Is.EqualTo(BanAction.Ban));
        Assert.That(Math.Abs((DateTime.Now - latestBanEntry.StartTime).TotalSeconds), Is.LessThan(10));
        Assert.That(latestBanEntry.EndTime, Is.Null);
        Assert.That(latestBanEntry.ActingRobloxUserId, Is.EqualTo(12345));
        Assert.That(latestBanEntry.DisplayReason, Is.EqualTo("Test Message 1"));
        Assert.That(latestBanEntry.PrivateReason, Is.EqualTo("Test Message 2"));
    }

    [Test]
    public void TestHandleBanRequestUnban()
    {
        var context = this._testResources.GetBansContext();
        context.BanEntries.Add(new BanEntry()
        {
            TargetRobloxUserId = 23456,
            Domain = "TestDomain",
            Action = BanAction.Ban,
            StartTime = DateTime.Now.AddDays(-2),
            ActingRobloxUserId = 12345,
            DisplayReason = "Old Message 1",
            PrivateReason = "Old Message 2",
        });
        context.SaveChanges();
        
        this.PrepareValidConfiguration();
        var response = this._banController.HandleBanRequest(GetContext(this.PrepareValidResponse(BanAction.Unban))).Result;
        var banResponse = (BanResponse) response.Response;
        Assert.That(response.StatusCode, Is.EqualTo(200));
        Assert.That(banResponse.Status, Is.EqualTo("Success"));
        Assert.That(banResponse.BannedUserIds, Is.EqualTo(new List<long>()));
        Assert.That(banResponse.UnbannedUserIds, Is.EqualTo(new List<long>() { 23456, }));

        var latestBanEntry = this._testResources.GetBansContext().BanEntries.OrderBy(entry => entry.StartTime).Last();
        Assert.That(latestBanEntry.TargetRobloxUserId, Is.EqualTo(23456));
        Assert.That(latestBanEntry.Domain, Is.EqualTo("TestDomain"));
        Assert.That(latestBanEntry.Action, Is.EqualTo(BanAction.Unban));
        Assert.That(Math.Abs((DateTime.Now - latestBanEntry.StartTime).TotalSeconds), Is.LessThan(10));
        Assert.That(latestBanEntry.EndTime, Is.Null);
        Assert.That(latestBanEntry.ActingRobloxUserId, Is.EqualTo(12345));
        Assert.That(latestBanEntry.DisplayReason, Is.EqualTo("Test Message 1"));
        Assert.That(latestBanEntry.PrivateReason, Is.EqualTo("Test Message 2"));
    }

    [Test]
    public void TestHandleBanRequestUnbanNoPreviousRecord()
    {
        this.PrepareValidConfiguration();
        var response = this._banController.HandleBanRequest(GetContext(this.PrepareValidResponse(BanAction.Unban))).Result;
        var banResponse = (BanResponse) response.Response;
        Assert.That(response.StatusCode, Is.EqualTo(200));
        Assert.That(banResponse.Status, Is.EqualTo("Success"));
        Assert.That(banResponse.BannedUserIds, Is.EqualTo(new List<long>()));
        Assert.That(banResponse.UnbannedUserIds, Is.EqualTo(new List<long>()));
    }

    [Test]
    public void TestHandleBanRequestUnbanAlreadyUnbanned()
    {
        var context = this._testResources.GetBansContext();
        context.BanEntries.Add(new BanEntry()
        {
            TargetRobloxUserId = 23456,
            Domain = "TestDomain",
            Action = BanAction.Unban,
            StartTime = DateTime.Now.AddDays(-2),
            ActingRobloxUserId = 12345,
            DisplayReason = "Old Message 1",
            PrivateReason = "Old Message 2",
        });
        context.SaveChanges();
        
        this.PrepareValidConfiguration();
        var response = this._banController.HandleBanRequest(GetContext(this.PrepareValidResponse(BanAction.Unban))).Result;
        var banResponse = (BanResponse) response.Response;
        Assert.That(response.StatusCode, Is.EqualTo(200));
        Assert.That(banResponse.Status, Is.EqualTo("Success"));
        Assert.That(banResponse.BannedUserIds, Is.EqualTo(new List<long>()));
        Assert.That(banResponse.UnbannedUserIds, Is.EqualTo(new List<long>()));
    }

    [Test]
    public void TestHandleBanRequestUnbanExpiredBan()
    {
        var context = this._testResources.GetBansContext();
        context.BanEntries.Add(new BanEntry()
        {
            TargetRobloxUserId = 23456,
            Domain = "TestDomain",
            Action = BanAction.Ban,
            StartTime = DateTime.Now.AddDays(-2),
            EndTime = DateTime.Now.AddDays(-1),
            ActingRobloxUserId = 12345,
            DisplayReason = "Old Message 1",
            PrivateReason = "Old Message 2",
        });
        context.SaveChanges();
        
        this.PrepareValidConfiguration();
        var response = this._banController.HandleBanRequest(GetContext(this.PrepareValidResponse(BanAction.Unban))).Result;
        var banResponse = (BanResponse) response.Response;
        Assert.That(response.StatusCode, Is.EqualTo(200));
        Assert.That(banResponse.Status, Is.EqualTo("Success"));
        Assert.That(banResponse.BannedUserIds, Is.EqualTo(new List<long>()));
        Assert.That(banResponse.UnbannedUserIds, Is.EqualTo(new List<long>()));
    }

    [Test]
    public void TestHandleBanRequestUnbanDifferentDomain()
    {
        var context = this._testResources.GetBansContext();
        context.BanEntries.Add(new BanEntry()
        {
            TargetRobloxUserId = 23456,
            Domain = "OtherDomain",
            Action = BanAction.Ban,
            StartTime = DateTime.Now.AddDays(-2),
            ActingRobloxUserId = 12345,
            DisplayReason = "Old Message 1",
            PrivateReason = "Old Message 2",
        });
        context.SaveChanges();
        
        this.PrepareValidConfiguration();
        var response = this._banController.HandleBanRequest(GetContext(this.PrepareValidResponse(BanAction.Unban))).Result;
        var banResponse = (BanResponse) response.Response;
        Assert.That(response.StatusCode, Is.EqualTo(200));
        Assert.That(banResponse.Status, Is.EqualTo("Success"));
        Assert.That(banResponse.BannedUserIds, Is.EqualTo(new List<long>()));
        Assert.That(banResponse.UnbannedUserIds, Is.EqualTo(new List<long>()));
    }

    [Test]
    public void TestHandleBanRequestLinkData()
    {
        var context = this._testResources.GetBansContext();
        context.ExternalAccountLinks.Add(new ExternalAccountLink()
        {
            RobloxUserId = 12345,
            Domain = "TestDomain",
            LinkMethod = "TestLink",
            LinkData = "TestData",
        });
        context.SaveChanges();
        
        this.PrepareValidConfiguration();
        var request = this.PrepareValidResponse();
        request.Authentication!.Method = "TestLink";
        request.Authentication!.Data = "TestData";
        var response = this._banController.HandleBanRequest(GetContext(request)).Result;
        var banResponse = (BanResponse) response.Response;
        Assert.That(response.StatusCode, Is.EqualTo(200));
        Assert.That(banResponse.Status, Is.EqualTo("Success"));
        Assert.That(banResponse.BannedUserIds, Is.EqualTo(new List<long>() { 23456, }));
        Assert.That(banResponse.UnbannedUserIds, Is.EqualTo(new List<long>()));

        var latestBanEntry = this._testResources.GetBansContext().BanEntries.OrderBy(entry => entry.StartTime).Last();
        Assert.That(latestBanEntry.TargetRobloxUserId, Is.EqualTo(23456));
        Assert.That(latestBanEntry.Domain, Is.EqualTo("TestDomain"));
        Assert.That(latestBanEntry.Action, Is.EqualTo(BanAction.Ban));
        Assert.That(Math.Abs((DateTime.Now - latestBanEntry.StartTime).TotalSeconds), Is.LessThan(10));
        Assert.That(latestBanEntry.EndTime, Is.Null);
        Assert.That(latestBanEntry.ActingRobloxUserId, Is.EqualTo(12345));
        Assert.That(latestBanEntry.DisplayReason, Is.EqualTo("Test Message 1"));
        Assert.That(latestBanEntry.PrivateReason, Is.EqualTo("Test Message 2"));
    }

    public void PrepareValidConfiguration()
    {
        this._testResources.Configuration = new BansConfiguration()
        {
            Domains = new List<DomainConfiguration>()
            {
                new DomainConfiguration()
                {
                    Name = "TestDomain",
                    Rules = new List<AuthenticationRuleEntry>()
                    {
                        new AuthenticationRuleEntry()
                        {
                            Rule = "IsUser(12345)",
                            Action = AuthenticationRuleAction.Allow,
                        }
                    },
                },
            },
        };
    }

    public BanRequest PrepareValidResponse(BanAction action = BanAction.Ban)
    {
        return new BanRequest()
        {
            Domain = "testDomain",
            Authentication = new BanRequestAuthentication()
            {
                Method = "Roblox",
                Data = "12345",
            },
            Action = new BanRequestAction()
            {
                Type = action,
                UserIds = new List<long>() { 23456 },
            },
            Reason = new BanRequestReason()
            {
                Display = "Test Message 1",
                Private = "Test Message 2",
            },
        };
    }

    public TestRequestContext GetContext(BanRequest request)
    {
        return TestRequestContext.FromRequest(request, BanRequestJsonContext.Default.BanRequest);
    }
}
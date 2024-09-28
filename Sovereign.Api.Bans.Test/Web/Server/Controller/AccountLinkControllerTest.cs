using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Sovereign.Api.Bans.Configuration;
using Sovereign.Api.Bans.Test.Web.Server.Controller.Shim;
using Sovereign.Api.Bans.Web.Server.Controller;
using Sovereign.Core.Database.Model.Api;
using Sovereign.Core.Model.Request.Api;
using Sovereign.Core.Model.Response;
using Sovereign.Core.Test.Model.Request.Authorization;

namespace Sovereign.Api.Bans.Test.Web.Server.Controller;

public class AccountLinkControllerTest
{
    private TestBanControllerResources _testResources;
    private AccountLinkController _accountLinkController;

    [SetUp]
    public void SetUp()
    {
        this._testResources = new TestBanControllerResources();
        this._accountLinkController = new AccountLinkController()
        {
            ControllerResources = this._testResources,
        };
    }
    
    [Test]
    public void TestHandleExternalLinkRequestAllFieldsMissing()
    {
        var response = this._accountLinkController.HandleExternalLinkRequest(this.GetContext(new ExternalLinkRequest())).Result;
        var validationErrorResponse = (ValidationErrorResponse) response.Response;
        Assert.That(response.StatusCode, Is.EqualTo(400));
        Assert.That(validationErrorResponse.Errors.Count, Is.EqualTo(4));
        Assert.That(validationErrorResponse.Errors[0].Path, Is.EqualTo("domain"));
        Assert.That(validationErrorResponse.Errors[0].Message, Is.EqualTo("domain was not provided."));
        Assert.That(validationErrorResponse.Errors[1].Path, Is.EqualTo("robloxUserId"));
        Assert.That(validationErrorResponse.Errors[1].Message, Is.EqualTo("robloxUserId was not provided."));
        Assert.That(validationErrorResponse.Errors[2].Path, Is.EqualTo("linkMethod"));
        Assert.That(validationErrorResponse.Errors[2].Message, Is.EqualTo("linkMethod was not provided."));
        Assert.That(validationErrorResponse.Errors[3].Path, Is.EqualTo("linkData"));
        Assert.That(validationErrorResponse.Errors[3].Message, Is.EqualTo("linkData was not provided."));
    }

    [Test]
    public void TestHandleExternalLinkRequestNoDomains()
    {
        this._testResources.Configuration = new BansConfiguration()
        {
            Domains = null,
        };
        var response = this._accountLinkController.HandleExternalLinkRequest(this.GetContext(this.PrepareValidRequest())).Result;
        var simpleResponse = (SimpleResponse) response.Response;
        Assert.That(response.StatusCode, Is.EqualTo(503));
        Assert.That(simpleResponse.Status, Is.EqualTo(ResponseStatus.ServerConfigurationError));
    }

    [Test]
    public void TestHandleExternalLinkRequestUnknownDomains()
    {
        this.PrepareValidConfiguration();
        var response = this._accountLinkController.HandleExternalLinkRequest(GetContext(new ExternalLinkRequest()
        {
            Domain = "UnknownDomain",
            RobloxUserId = 12345,
            LinkMethod = "TestMethod",
            LinkData = "TestData",
        })).Result;
        var simpleResponse = (SimpleResponse) response.Response;
        Assert.That(response.StatusCode, Is.EqualTo(401));
        Assert.That(simpleResponse.Status, Is.EqualTo(ResponseStatus.Unauthorized));
    }
    
    [Test]
    public void TestHandleBanRequestUnauthorizedHeader()
    {
        this.PrepareValidConfiguration();
        var context = this.GetContext(this.PrepareValidRequest());
        context.Authorized = false;
        var response = this._accountLinkController.HandleExternalLinkRequest(context).Result;
        var simpleResponse = response.Response;
        Assert.That(response.StatusCode, Is.EqualTo(401));
        Assert.That(simpleResponse.Status, Is.EqualTo(ResponseStatus.Unauthorized));
    }

    [Test]
    public void TestHandleExternalLinkRequestUnauthorizedUser()
    {
        this.PrepareValidConfiguration();
        var response = this._accountLinkController.HandleExternalLinkRequest(this.GetContext(new ExternalLinkRequest()
        {
            Domain = "testDomain",
            RobloxUserId = 23456,
            LinkMethod = "TestMethod",
            LinkData = "TestData",
        })).Result;
        var simpleResponse = (SimpleResponse) response.Response;
        Assert.That(response.StatusCode, Is.EqualTo(403));
        Assert.That(simpleResponse.Status, Is.EqualTo(ResponseStatus.Forbidden));
    }
    
    [Test]
    public void TestHandleBanRequestNewLink()
    {
        this.PrepareValidConfiguration();
        var response = this._accountLinkController.HandleExternalLinkRequest(this.GetContext(this.PrepareValidRequest())).Result;
        var simpleResponse = response.Response;
        Assert.That(response.StatusCode, Is.EqualTo(200));
        Assert.That(simpleResponse.Status, Is.EqualTo(ResponseStatus.Success));
        
        var accountLink = this._testResources.GetBansContext().ExternalAccountLinks.First();
        Assert.That(accountLink.Domain, Is.EqualTo("TestDomain"));
        Assert.That(accountLink.RobloxUserId, Is.EqualTo(12345));
        Assert.That(accountLink.LinkMethod, Is.EqualTo("TestMethod"));
        Assert.That(accountLink.LinkData, Is.EqualTo("TestData"));
    }
    
    [Test]
    public void TestHandleBanRequestUpdateLink()
    {
        var context = this._testResources.GetBansContext();
        context.ExternalAccountLinks.Add(new ExternalAccountLink()
        {
            Domain = "TestDomain",
            RobloxUserId = 12345,
            LinkMethod = "TestMethod",
            LinkData = "OldTestData",
        });
        context.SaveChanges();
        
        this.PrepareValidConfiguration();
        var response = this._accountLinkController.HandleExternalLinkRequest(this.GetContext(this.PrepareValidRequest())).Result;
        var simpleResponse = response.Response;
        Assert.That(response.StatusCode, Is.EqualTo(200));
        Assert.That(simpleResponse.Status, Is.EqualTo(ResponseStatus.Success));
        
        var accountLink = this._testResources.GetBansContext().ExternalAccountLinks.First();
        Assert.That(accountLink.Domain, Is.EqualTo("TestDomain"));
        Assert.That(accountLink.RobloxUserId, Is.EqualTo(12345));
        Assert.That(accountLink.LinkMethod, Is.EqualTo("TestMethod"));
        Assert.That(accountLink.LinkData, Is.EqualTo("TestData"));
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
    
    public ExternalLinkRequest PrepareValidRequest()
    {
        return new ExternalLinkRequest()
        {
            Domain = "testDomain",
            RobloxUserId = 12345,
            LinkMethod = "TestMethod",
            LinkData = "TestData",
        };
    }

    public TestRequestContext GetContext(ExternalLinkRequest request)
    {
        return TestRequestContext.FromRequest(request, ExternalLinkRequestJsonContext.Default.ExternalLinkRequest);
    }
}
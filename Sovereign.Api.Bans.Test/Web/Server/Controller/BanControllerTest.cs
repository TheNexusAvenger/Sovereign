using System.Collections.Generic;
using NUnit.Framework;
using Sovereign.Api.Bans.Web.Server.Controller;
using Sovereign.Api.Bans.Web.Server.Model;
using Sovereign.Core.Model;
using Sovereign.Core.Model.Response;

namespace Sovereign.Api.Bans.Test.Web.Server.Controller;

public class BanControllerTest
{
    [Test]
    public void HandleBanRequestAllFieldsMissing()
    {
        var response = BanController.HandleBanRequest(new BanRequest()).Result;
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
    public void HandleBanRequestAllSubFieldsMissing()
    {
        var response = BanController.HandleBanRequest(new BanRequest()
        {
            Domain = "Domain",
            Authentication = new BanRequestAuthentication(),
            Action = new BanRequestAction(),
            Reason = new BanRequestReason(),
        }).Result;
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
    public void HandleBanRequestAllInvalidSubFieldsMissing()
    {
        var response = BanController.HandleBanRequest(new BanRequest()
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
        }).Result;
        var validationErrorResponse = (ValidationErrorResponse) response.Response;
        Assert.That(response.StatusCode, Is.EqualTo(400));
        Assert.That(validationErrorResponse.Errors.Count, Is.EqualTo(2));
        Assert.That(validationErrorResponse.Errors[0].Path, Is.EqualTo("action.userIds"));
        Assert.That(validationErrorResponse.Errors[0].Message, Is.EqualTo("action.userIds was empty."));
        Assert.That(validationErrorResponse.Errors[1].Path, Is.EqualTo("action.duration"));
        Assert.That(validationErrorResponse.Errors[1].Message, Is.EqualTo("action.duration was not a positive number."));
    }
}
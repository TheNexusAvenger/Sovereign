using System;
using System.Collections.Generic;
using System.Linq;
using Discord;
using NUnit.Framework;
using Sovereign.Core.Model;
using Sovereign.Core.Model.Response;
using Sovereign.Core.Model.Response.Api;
using Sovereign.Discord.Discord.Commands;
using Sovereign.Discord.Test.Discord.Shim;

namespace Sovereign.Discord.Test.Discord.Commands;

public class ViewBanCommandsTest
{
    private TestInteractionContextWrapper _interactionContext;
    private ViewBanCommands _viewBanCommands;

    [SetUp]
    public void SetUp()
    {
        this._interactionContext = new TestInteractionContextWrapper();
        this._interactionContext.BanRecordResponse = new BanRecordResponse()
        {
            Total = 3,
            Entries = new List<BanRecordResponseEntry>()
            {
                new BanRecordResponseEntry()
                {
                    Action = new BanResponseEntryAction()
                    {
                        Type = BanAction.Ban,
                        StartTime = DateTime.Now.AddDays(-1),
                    },
                    Reason = new BanResponseEntryReason()
                    {
                        ActingUserId = 12345,
                        Display = "Test Display",
                        Private = "Test Private",
                    },
                },
            },
        };
        
        this._viewBanCommands = new ViewBanCommands();
        this._viewBanCommands.SetOverrideContextWrapper(this._interactionContext);
    }
    
    [Test]
    public void TestViewBanNotConfigured()
    {
        this._interactionContext.DiscordGuildId = 34567;
        this._viewBanCommands.ViewBan(12345).Wait();
        
        var response = this._interactionContext.LastMessage!;
        Assert.That(response.Text, Is.EqualTo("Sovereign is not configured for this server."));
        Assert.That(response.Embed, Is.Null);
        Assert.That(response.MessageComponent, Is.Null);
    }
    
    [Test]
    public void TestViewBanUnauthorized()
    {
        this._interactionContext.BanRecordResponse.Status = ResponseStatus.Unauthorized;
        this._viewBanCommands.ViewBan(12345).Wait();
        
        var response = this._interactionContext.LastMessage!;
        Assert.That(response.Text, Is.EqualTo("A configuration error occured when fetching the ban history."));
        Assert.That(response.Embed, Is.Null);
        Assert.That(response.MessageComponent, Is.Null);
    }
    
    [Test]
    public void TestViewBanInitialBan()
    {
        this._viewBanCommands.ViewBan(12345).Wait();
        
        var response = this._interactionContext.LastMessage!;
        Assert.That(response.Text, Is.Null);
        Assert.That(response.Embed, Is.Not.Null);
        Assert.That(response.MessageComponent, Is.Not.Null);
        
        var actionRowComponent = (ActionRowComponent) response.MessageComponent.Components.First();
        var previousButton = (ButtonComponent) actionRowComponent.Components.ToList()[0];
        Assert.That(previousButton.Label, Is.EqualTo("Previous"));
        Assert.That(previousButton.CustomId, Is.EqualTo("SovereignViewBan:12345:-1"));
        var nextButton = (ButtonComponent) actionRowComponent.Components.ToList()[1];
        Assert.That(nextButton.Label, Is.EqualTo("Next"));
        Assert.That(nextButton.CustomId, Is.EqualTo("SovereignViewBan:12345:1"));
    }
    
    [Test]
    public void TestCommendsLogsPreviousPageNotConfigured()
    {
        this._interactionContext.DiscordGuildId = 34567;
        this._viewBanCommands.CommendsLogsPreviousPage(12345, 1).Wait();
        
        var response = this._interactionContext.LastMessage!;
        Assert.That(response.Text, Is.EqualTo("Sovereign is not configured for this server."));
        Assert.That(response.Embed, Is.Null);
        Assert.That(response.MessageComponent, Is.Null);
    }
    
    [Test]
    public void TestCommendsLogsPreviousPageUnauthorized()
    {
        this._interactionContext.BanRecordResponse.Status = ResponseStatus.Unauthorized;
        this._viewBanCommands.CommendsLogsPreviousPage(12345, 1).Wait();
        
        var response = this._interactionContext.LastMessage!;
        Assert.That(response.Text, Is.EqualTo("A configuration error occured when fetching the ban history."));
        Assert.That(response.Embed, Is.Null);
        Assert.That(response.MessageComponent, Is.Null);
    }
    
    [Test]
    public void TestCommendsLogsPreviousPage()
    {
        this._viewBanCommands.CommendsLogsPreviousPage(12345, 1).Wait();
        
        var response = this._interactionContext.LastMessage!;
        Assert.That(response.Text, Is.Null);
        Assert.That(response.Embed, Is.Not.Null);
        Assert.That(response.MessageComponent, Is.Not.Null);
        
        var actionRowComponent = (ActionRowComponent) response.MessageComponent.Components.First();
        var previousButton = (ButtonComponent) actionRowComponent.Components.ToList()[0];
        Assert.That(previousButton.Label, Is.EqualTo("Previous"));
        Assert.That(previousButton.CustomId, Is.EqualTo("SovereignViewBan:12345:0"));
        var nextButton = (ButtonComponent) actionRowComponent.Components.ToList()[1];
        Assert.That(nextButton.Label, Is.EqualTo("Next"));
        Assert.That(nextButton.CustomId, Is.EqualTo("SovereignViewBan:12345:2"));
    }
}
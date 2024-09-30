using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Discord;
using NUnit.Framework;
using Sovereign.Core.Model;
using Sovereign.Core.Model.Response.Api;
using Sovereign.Core.Web.Client.Response;
using Sovereign.Discord.Discord.Component;

namespace Sovereign.Discord.Test.Discord.Component;

public class BanViewComponentTest
{
    private UserProfileResponse _userProfileResponse;
    private UserProfileResponse _banningUserProfileResponse;
    private BanRecordResponse _banRecordResponse;

    [SetUp]
    public void SetUp()
    {
        this. _userProfileResponse = new UserProfileResponse()
        {
            DisplayName = "TestDisplay1",
            Name = "TestName1",
        };
        this._banningUserProfileResponse = new UserProfileResponse()
        {
            DisplayName = "TestDisplay2",
            Name = "TestName2",
        };
        this._banRecordResponse = new BanRecordResponse()
        {
            Total = 5,
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
    }

    [Test]
    public void TestGetEmbedNoRecord()
    {
        this._banRecordResponse.Total = 0;
        this._banRecordResponse.Entries.Clear();

        var banViewComponent = new BanViewComponent(23456, this._banRecordResponse, 0, this._userProfileResponse, this._banningUserProfileResponse);
        var embed = banViewComponent.GetEmbed();
        Assert.That(embed.Title, Is.EqualTo("TestDisplay1 (@TestName1) [23456]"));
        Assert.That(embed.Description, Is.EqualTo("No ban record to show."));
        Assert.That(embed.Timestamp, Is.Not.Null);
        Assert.That(embed.Footer!.Value.Text, Is.EqualTo("No bans."));
    }

    [Test]
    public void TestGetEmbedPermanentBan()
    {
        var banEntry = this._banRecordResponse.Entries.First();
        var description = new StringBuilder();
        description.Append($"**Action**: Ban");
        description.Append($"\n**Display reason**: Test Display");
        description.Append($"\n**Private reason**: Test Private");
        description.Append($"\n**Handled by**: TestDisplay2 (@TestName2) [12345]");
        description.Append($"\n**Start time**: {banEntry.Action.StartTime}");
        
        var banViewComponent = new BanViewComponent(23456, this._banRecordResponse, 0, this._userProfileResponse, this._banningUserProfileResponse);
        var embed = banViewComponent.GetEmbed();
        Assert.That(embed.Title, Is.EqualTo("TestDisplay1 (@TestName1) [23456]"));
        Assert.That(embed.Description, Is.EqualTo(description.ToString()));
        Assert.That(embed.Timestamp, Is.Not.Null);
        Assert.That(embed.Footer!.Value.Text, Is.EqualTo("Entry 1/5"));
    }

    [Test]
    public void TestGetEmbedTemporaryBan()
    {
        var banEntry = this._banRecordResponse.Entries.First();
        banEntry.Action.EndTime = DateTime.Now.AddDays(1);
        var description = new StringBuilder();
        description.Append($"**Action**: Ban");
        description.Append($"\n**Display reason**: Test Display");
        description.Append($"\n**Private reason**: Test Private");
        description.Append($"\n**Handled by**: TestDisplay2 (@TestName2) [12345]");
        description.Append($"\n**Start time**: {banEntry.Action.StartTime}");
        description.Append($"\n**Expire time**: {banEntry.Action.EndTime}");
        
        var banViewComponent = new BanViewComponent(23456, this._banRecordResponse, 0, this._userProfileResponse, this._banningUserProfileResponse);
        var embed = banViewComponent.GetEmbed();
        Assert.That(embed.Title, Is.EqualTo("TestDisplay1 (@TestName1) [23456]"));
        Assert.That(embed.Description, Is.EqualTo(description.ToString()));
        Assert.That(embed.Timestamp, Is.Not.Null);
        Assert.That(embed.Footer!.Value.Text, Is.EqualTo("Entry 1/5"));
    }

    [Test]
    public void TestGetEmbedTemporaryBanExpired()
    {
        var banEntry = this._banRecordResponse.Entries.First();
        banEntry.Action.EndTime = DateTime.Now.AddHours(-1);
        var description = new StringBuilder();
        description.Append($"**Action**: Ban");
        description.Append($"\n**Display reason**: Test Display");
        description.Append($"\n**Private reason**: Test Private");
        description.Append($"\n**Handled by**: TestDisplay2 (@TestName2) [12345]");
        description.Append($"\n**Start time**: {banEntry.Action.StartTime}");
        description.Append($"\n**Expire time**: {banEntry.Action.EndTime} *(Expired)*");
        
        var banViewComponent = new BanViewComponent(23456, this._banRecordResponse, 0, this._userProfileResponse, this._banningUserProfileResponse);
        var embed = banViewComponent.GetEmbed();
        Assert.That(embed.Title, Is.EqualTo("TestDisplay1 (@TestName1) [23456]"));
        Assert.That(embed.Description, Is.EqualTo(description.ToString()));
        Assert.That(embed.Timestamp, Is.Not.Null);
        Assert.That(embed.Footer!.Value.Text, Is.EqualTo("Entry 1/5"));
    }

    [Test]
    public void TestGetMessageComponentNoRecord()
    {
        this._banRecordResponse.Total = 0;
        this._banRecordResponse.Entries.Clear();

        var banViewComponent = new BanViewComponent(23456, this._banRecordResponse, 0, this._userProfileResponse, this._banningUserProfileResponse);
        var messageComponent = banViewComponent.GetMessageComponent();
        var actionRowComponent = (ActionRowComponent) messageComponent.Components.First();
        var previousButton = (ButtonComponent) actionRowComponent.Components.ToList()[0];
        Assert.That(previousButton.Label, Is.EqualTo("Previous"));
        Assert.That(previousButton.CustomId, Is.EqualTo("SovereignViewBan:23456:-1"));
        Assert.That(previousButton.IsDisabled, Is.True);
        var nextButton = (ButtonComponent) actionRowComponent.Components.ToList()[1];
        Assert.That(nextButton.Label, Is.EqualTo("Next"));
        Assert.That(nextButton.CustomId, Is.EqualTo("SovereignViewBan:23456:1"));
        Assert.That(nextButton.IsDisabled, Is.True);
    }

    [Test]
    public void TestGetMessageComponentFirstRecord()
    {
        var banViewComponent = new BanViewComponent(23456, this._banRecordResponse, 0, this._userProfileResponse, this._banningUserProfileResponse);
        var messageComponent = banViewComponent.GetMessageComponent();
        var actionRowComponent = (ActionRowComponent) messageComponent.Components.First();
        var previousButton = (ButtonComponent) actionRowComponent.Components.ToList()[0];
        Assert.That(previousButton.Label, Is.EqualTo("Previous"));
        Assert.That(previousButton.CustomId, Is.EqualTo("SovereignViewBan:23456:-1"));
        Assert.That(previousButton.IsDisabled, Is.True);
        var nextButton = (ButtonComponent) actionRowComponent.Components.ToList()[1];
        Assert.That(nextButton.Label, Is.EqualTo("Next"));
        Assert.That(nextButton.CustomId, Is.EqualTo("SovereignViewBan:23456:1"));
        Assert.That(nextButton.IsDisabled, Is.False);
    }

    [Test]
    public void TestGetMessageComponentLastRecord()
    {
        var banViewComponent = new BanViewComponent(23456, this._banRecordResponse, 4, this._userProfileResponse, this._banningUserProfileResponse);
        var messageComponent = banViewComponent.GetMessageComponent();
        var actionRowComponent = (ActionRowComponent) messageComponent.Components.First();
        var previousButton = (ButtonComponent) actionRowComponent.Components.ToList()[0];
        Assert.That(previousButton.Label, Is.EqualTo("Previous"));
        Assert.That(previousButton.CustomId, Is.EqualTo("SovereignViewBan:23456:3"));
        Assert.That(previousButton.IsDisabled, Is.False);
        var nextButton = (ButtonComponent) actionRowComponent.Components.ToList()[1];
        Assert.That(nextButton.Label, Is.EqualTo("Next"));
        Assert.That(nextButton.CustomId, Is.EqualTo("SovereignViewBan:23456:5"));
        Assert.That(nextButton.IsDisabled, Is.True);
    }

    [Test]
    public void TestGetMessageComponentMiddleRecord()
    {
        var banViewComponent = new BanViewComponent(23456, this._banRecordResponse, 2, this._userProfileResponse, this._banningUserProfileResponse);
        var messageComponent = banViewComponent.GetMessageComponent();
        var actionRowComponent = (ActionRowComponent) messageComponent.Components.First();
        var previousButton = (ButtonComponent) actionRowComponent.Components.ToList()[0];
        Assert.That(previousButton.Label, Is.EqualTo("Previous"));
        Assert.That(previousButton.CustomId, Is.EqualTo("SovereignViewBan:23456:1"));
        Assert.That(previousButton.IsDisabled, Is.False);
        var nextButton = (ButtonComponent) actionRowComponent.Components.ToList()[1];
        Assert.That(nextButton.Label, Is.EqualTo("Next"));
        Assert.That(nextButton.CustomId, Is.EqualTo("SovereignViewBan:23456:3"));
        Assert.That(nextButton.IsDisabled, Is.False);
    }
}
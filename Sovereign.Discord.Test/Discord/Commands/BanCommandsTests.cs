using System.Collections.Generic;
using System.Linq;
using Discord;
using NUnit.Framework;
using Sovereign.Core.Model.Response;
using Sovereign.Core.Model.Response.Api;
using Sovereign.Discord.Configuration;
using Sovereign.Discord.Discord.Commands;
using Sovereign.Discord.Test.Discord.Shim;

namespace Sovereign.Discord.Test.Discord.Commands;

public class BanCommandsTests
{
    private TestInteractionContextWrapper _interactionContext;
    private BanCommands _banCommands;

    [SetUp]
    public void SetUp()
    {
        this._interactionContext = new TestInteractionContextWrapper();
        this._banCommands = new BanCommands();
        this._banCommands.SetOverrideContextWrapper(this._interactionContext);
    }
    
    [Test]
    public void TestStartBanInvalidIds()
    {
        this._banCommands.StartBan("test").Wait();
        
        var response = this._interactionContext.LastMessage!;
        Assert.That(response.Text, Is.EqualTo("The Roblox user id \"test\" could not be parsed."));
        Assert.That(response.Embed, Is.Null);
        Assert.That(response.MessageComponent, Is.Null);
    }
    
    [Test]
    public void TestStartBanNoBanOptions()
    {
        this._interactionContext.Configuration.Domains!.First().BanOptions!.Clear();
        this._banCommands.StartBan("12345").Wait();
        
        var response = this._interactionContext.LastMessage!;
        Assert.That(response.Text, Is.EqualTo("Ban options are not configured for this server."));
        Assert.That(response.Embed, Is.Null);
        Assert.That(response.MessageComponent, Is.Null);
    }

    [Test]
    public void TestStartBanModal()
    {
        this._interactionContext.Configuration.Domains!.First().BanOptions = new List<DiscordDomainBanOptionConfiguration>()
        {
            this._interactionContext.Configuration.Domains!.First().BanOptions!.First(),
        };
        this._banCommands.StartBan("123,456,123").Wait();
        
        var modal = this._interactionContext.LastModal!;
        Assert.That(modal.Title, Is.EqualTo("Ban (Exploiting)"));
        Assert.That(modal.CustomId, Is.EqualTo("SovereignBanPrompt"));
        var modalComponents = modal.Component.Components!.ToList().Select(component => (TextInputComponent) component.Components.First()).ToList();
        Assert.That(modalComponents[0].CustomId, Is.EqualTo("DisplayReason"));
        Assert.That(modalComponents[0].Label, Is.EqualTo("Display Reason"));
        Assert.That(modalComponents[0].MaxLength, Is.EqualTo(400));
        Assert.That(modalComponents[0].Value, Is.EqualTo("Banned for exploiting. Use the Discord server in the game's social links to appeal."));
        Assert.That(modalComponents[1].CustomId, Is.EqualTo("PrivateReason"));
        Assert.That(modalComponents[1].Label, Is.EqualTo("Private Reason"));
        Assert.That(modalComponents[1].MaxLength, Is.EqualTo(1000));
        Assert.That(modalComponents[1].Value, Is.Null);
        Assert.That(modalComponents[2].CustomId, Is.EqualTo("RobloxUserIds"));
        Assert.That(modalComponents[2].Label, Is.EqualTo("Roblox User Ids"));
        Assert.That(modalComponents[2].MaxLength, Is.Null);
        Assert.That(modalComponents[2].Value, Is.EqualTo("123, 456"));
        Assert.That(modalComponents[3].CustomId, Is.EqualTo("Duration"));
        Assert.That(modalComponents[3].Label, Is.EqualTo("Optional Duration (Hours)"));
        Assert.That(modalComponents[3].MaxLength, Is.Null);
        Assert.That(modalComponents[3].Value, Is.EqualTo(""));
    }

    [Test]
    public void TestStartBanMenu()
    {
        this._banCommands.StartBan("123,456,123").Wait();
        
        var response = this._interactionContext.LastMessage!;
        Assert.That(response.Text, Is.EqualTo("Select a ban option to ban the following: 123, 456"));
        Assert.That(response.Embed, Is.Null);
        var selectMenuComponent = (SelectMenuComponent) response.MessageComponent!.Components.First().Components!.First();
        Assert.That(selectMenuComponent.Placeholder, Is.EqualTo("Select a ban option"));
        Assert.That(selectMenuComponent.CustomId, Is.EqualTo("SovereignBanOptions"));
        Assert.That(selectMenuComponent.MinValues, Is.EqualTo(1));
        Assert.That(selectMenuComponent.MaxValues, Is.EqualTo(1));
        var selectMenuComponentOptions = selectMenuComponent.Options!.ToList();
        Assert.That(selectMenuComponentOptions[0].Label, Is.EqualTo("Exploiting"));
        Assert.That(selectMenuComponentOptions[0].Value, Is.EqualTo("Exploiting"));
        Assert.That(selectMenuComponentOptions[0].Description, Is.EqualTo("Please specify details in the private reason."));
        Assert.That(selectMenuComponentOptions[1].Label, Is.EqualTo("Harassment"));
        Assert.That(selectMenuComponentOptions[1].Value, Is.EqualTo("Harassment"));
        Assert.That(selectMenuComponentOptions[1].Description, Is.Null);
        Assert.That(selectMenuComponentOptions[2].Label, Is.EqualTo("Other"));
        Assert.That(selectMenuComponentOptions[2].Value, Is.EqualTo("Other"));
        Assert.That(selectMenuComponentOptions[2].Description, Is.Null);
    }
    
    [Test]
    public void TestStartUnbanInvalidIds()
    {
        this._banCommands.StartUnban("test").Wait();
        
        var response = this._interactionContext.LastMessage!;
        Assert.That(response.Text, Is.EqualTo("The Roblox user id \"test\" could not be parsed."));
        Assert.That(response.Embed, Is.Null);
        Assert.That(response.MessageComponent, Is.Null);
    }
    
    [Test]
    public void TestStartUnbanModal()
    {
        this._banCommands.StartUnban("123,456,123").Wait();
        
        var modal = this._interactionContext.LastModal!;
        Assert.That(modal.Title, Is.EqualTo("Unban"));
        Assert.That(modal.CustomId, Is.EqualTo("SovereignUnbanPrompt"));
        var modalComponents = modal.Component.Components!.ToList().Select(component => (TextInputComponent) component.Components.First()).ToList();
        Assert.That(modalComponents[0].CustomId, Is.EqualTo("PrivateReason"));
        Assert.That(modalComponents[0].Label, Is.EqualTo("Private Reason"));
        Assert.That(modalComponents[0].MaxLength, Is.EqualTo(1000));
        Assert.That(modalComponents[0].Value, Is.EqualTo("No information provided."));
        Assert.That(modalComponents[1].CustomId, Is.EqualTo("RobloxUserIds"));
        Assert.That(modalComponents[1].Label, Is.EqualTo("Roblox User Ids"));
        Assert.That(modalComponents[1].MaxLength, Is.Null);
        Assert.That(modalComponents[1].Value, Is.EqualTo("123, 456"));
    }

    [Test]
    public void TestSovereignBanOptionsMissingRole()
    {
        this._interactionContext.SourceMessage = "Select a ban option to ban the following: 123, 456";
        this._banCommands.SovereignBanOptions(new[] { "UnknownBan" }).Wait();
        
        var response = this._interactionContext.LastMessage!;
        Assert.That(response.Text, Is.EqualTo("The ban option is no longer configured on the server."));
        Assert.That(response.Embed, Is.Null);
        Assert.That(response.MessageComponent, Is.Null);
    }

    [Test]
    public void TestSovereignBanOptionsModal()
    {
        this._interactionContext.SourceMessage = "Select a ban option to ban the following: 123, 456";
        this._banCommands.SovereignBanOptions(new[] { "Exploiting" }).Wait();

        var modal = this._interactionContext.LastModal!;
        Assert.That(modal.Title, Is.EqualTo("Ban (Exploiting)"));
        Assert.That(modal.CustomId, Is.EqualTo("SovereignBanPrompt"));
        var modalComponents = modal.Component.Components!.ToList().Select(component => (TextInputComponent) component.Components.First()).ToList();
        Assert.That(modalComponents[0].CustomId, Is.EqualTo("DisplayReason"));
        Assert.That(modalComponents[0].Label, Is.EqualTo("Display Reason"));
        Assert.That(modalComponents[0].MaxLength, Is.EqualTo(400));
        Assert.That(modalComponents[0].Value, Is.EqualTo("Banned for exploiting. Use the Discord server in the game's social links to appeal."));
        Assert.That(modalComponents[1].CustomId, Is.EqualTo("PrivateReason"));
        Assert.That(modalComponents[1].Label, Is.EqualTo("Private Reason"));
        Assert.That(modalComponents[1].MaxLength, Is.EqualTo(1000));
        Assert.That(modalComponents[1].Value, Is.Null);
        Assert.That(modalComponents[2].CustomId, Is.EqualTo("RobloxUserIds"));
        Assert.That(modalComponents[2].Label, Is.EqualTo("Roblox User Ids"));
        Assert.That(modalComponents[2].MaxLength, Is.Null);
        Assert.That(modalComponents[2].Value, Is.EqualTo("123, 456"));
        Assert.That(modalComponents[3].CustomId, Is.EqualTo("Duration"));
        Assert.That(modalComponents[3].Label, Is.EqualTo("Optional Duration (Hours)"));
        Assert.That(modalComponents[3].MaxLength, Is.Null);
        Assert.That(modalComponents[3].Value, Is.EqualTo(""));
    }

    [Test]
    public void TestSovereignBanPromptInvalidIds()
    {
        var modal = new BanPromptModal()
        {
            RobloxUserIds = "123,test  ",
            Duration = "",
        };
        this._banCommands.SovereignBanPrompt(modal).Wait();
        
        var response = this._interactionContext.LastMessage!;
        Assert.That(response.Text, Is.EqualTo("The Roblox user id \"test\" could not be parsed."));
        Assert.That(response.Embed, Is.Null);
        Assert.That(response.MessageComponent, Is.Null);
    }

    [Test]
    public void TestSovereignBanPromptInvalidDuration()
    {
        var modal = new BanPromptModal()
        {
            RobloxUserIds = "123,456",
            Duration = "test",
        };
        this._banCommands.SovereignBanPrompt(modal).Wait();
        
        var response = this._interactionContext.LastMessage!;
        Assert.That(response.Text, Is.EqualTo("Duration \"test\" could not be parsed."));
        Assert.That(response.Embed, Is.Null);
        Assert.That(response.MessageComponent, Is.Null);
    }

    [Test]
    public void TestSovereignBanForbidden()
    {
        this._interactionContext.BanResponse.Status = ResponseStatus.Forbidden;
        this._interactionContext.BanResponse.BannedUserIds = new List<long>() { 23456, };
        var modal = new BanPromptModal()
        {
            DisplayReason = "Test Display",
            PrivateReason = "Test Private",
            RobloxUserIds = "123,456",
            Duration = "2",
        };
        this._banCommands.SovereignBanPrompt(modal).Wait();
        
        var response = this._interactionContext.LastMessage!;
        Assert.That(response.Text, Is.EqualTo("You are not authorized to ban users."));
        Assert.That(response.Embed, Is.Null);
        Assert.That(response.MessageComponent, Is.Null);
    }

    [Test]
    public void TestSovereignBanGroupRankPermissionError()
    {
        this._interactionContext.BanResponse.Status = ResponseStatus.GroupRankPermissionError;
        this._interactionContext.BanResponse.BannedUserIds = new List<long>() { 23456, };
        var modal = new BanPromptModal()
        {
            DisplayReason = "Test Display",
            PrivateReason = "Test Private",
            RobloxUserIds = "123,456",
            Duration = "2",
        };
        this._banCommands.SovereignBanPrompt(modal).Wait();
        
        var response = this._interactionContext.LastMessage!;
        Assert.That(response.Text, Is.EqualTo("You are not authorized to ban users with the same or higher rank in the configured group."));
        Assert.That(response.Embed, Is.Null);
        Assert.That(response.MessageComponent, Is.Null);
    }

    [Test]
    public void TestSovereignBanError()
    {
        this._interactionContext.BanResponse.Status = ResponseStatus.Unauthorized;
        this._interactionContext.BanResponse.BannedUserIds = new List<long>() { 23456, };
        var modal = new BanPromptModal()
        {
            DisplayReason = "Test Display",
            PrivateReason = "Test Private",
            RobloxUserIds = "123,456",
            Duration = "2",
        };
        this._banCommands.SovereignBanPrompt(modal).Wait();
        
        var response = this._interactionContext.LastMessage!;
        Assert.That(response.Text, Is.EqualTo("A configuration error occured when handling the bans."));
        Assert.That(response.Embed, Is.Null);
        Assert.That(response.MessageComponent, Is.Null);
    }

    [Test]
    public void TestSovereignBanSuccess()
    {
        this._interactionContext.BanResponse.Status = ResponseStatus.Success;
        this._interactionContext.BanResponse.BannedUserIds = new List<long>() { 23456, };
        var modal = new BanPromptModal()
        {
            DisplayReason = "Test Display",
            PrivateReason = "Test Private",
            RobloxUserIds = "123,456",
            Duration = "2",
        };
        this._banCommands.SovereignBanPrompt(modal).Wait();
        
        var response = this._interactionContext.LastMessage!;
        Assert.That(response.Text, Is.EqualTo("Banned 1 user(s)."));
        Assert.That(response.Embed, Is.Null);
        Assert.That(response.MessageComponent, Is.Null);
    }
    
    [Test]
    public void TestSovereignUnbanPromptInvalidIds()
    {
        var modal = new UnbanPromptModal()
        {
            RobloxUserIds = "123,test  ",
        };
        this._banCommands.SovereignUnbanPrompt(modal).Wait();
        
        var response = this._interactionContext.LastMessage!;
        Assert.That(response.Text, Is.EqualTo("The Roblox user id \"test\" could not be parsed."));
        Assert.That(response.Embed, Is.Null);
        Assert.That(response.MessageComponent, Is.Null);
    }

    [Test]
    public void TestSovereignUnbanForbidden()
    {
        this._interactionContext.BanResponse.Status = ResponseStatus.Forbidden;
        this._interactionContext.BanResponse.BannedUserIds = new List<long>() { 23456, };
        var modal = new UnbanPromptModal()
        {
            PrivateReason = "Test Private",
            RobloxUserIds = "123,456",
        };
        this._banCommands.SovereignUnbanPrompt(modal).Wait();
        
        var response = this._interactionContext.LastMessage!;
        Assert.That(response.Text, Is.EqualTo("You are not authorized to unban users."));
        Assert.That(response.Embed, Is.Null);
        Assert.That(response.MessageComponent, Is.Null);
    }

    [Test]
    public void TestSovereignUnbanError()
    {
        this._interactionContext.BanResponse.Status = ResponseStatus.Unauthorized;
        this._interactionContext.BanResponse.BannedUserIds = new List<long>() { 23456, };
        var modal = new UnbanPromptModal()
        {
            PrivateReason = "Test Private",
            RobloxUserIds = "123,456",
        };
        this._banCommands.SovereignUnbanPrompt(modal).Wait();
        
        var response = this._interactionContext.LastMessage!;
        Assert.That(response.Text, Is.EqualTo("A configuration error occured when handling the unbans."));
        Assert.That(response.Embed, Is.Null);
        Assert.That(response.MessageComponent, Is.Null);
    }

    [Test]
    public void TestSovereignUnbanSuccess()
    {
        this._interactionContext.BanResponse.Status = ResponseStatus.Success;
        this._interactionContext.BanResponse.UnbannedUserIds = new List<long>() { 23456, };
        var modal = new UnbanPromptModal()
        {
            PrivateReason = "Test Private",
            RobloxUserIds = "123,456",
        };
        this._banCommands.SovereignUnbanPrompt(modal).Wait();
        
        var response = this._interactionContext.LastMessage!;
        Assert.That(response.Text, Is.EqualTo("Unbanned 1 user(s)."));
        Assert.That(response.Embed, Is.Null);
        Assert.That(response.MessageComponent, Is.Null);
    }
    
    [Test]
    public void TestValidateBansAsyncNotConfigured()
    {
        this._interactionContext.DiscordGuildId = 34567;
        Assert.That(this._banCommands.ValidateBansAsync("12345").Result, Is.Null);
        
        var response = this._interactionContext.LastMessage!;
        Assert.That(response.Text, Is.EqualTo("Sovereign is not configured for this server."));
        Assert.That(response.Embed, Is.Null);
        Assert.That(response.MessageComponent, Is.Null);
    }
    
    [Test]
    public void TestValidateBansAsyncNoIds()
    {
        Assert.That(this._banCommands.ValidateBansAsync("").Result, Is.Null);
        
        var response = this._interactionContext.LastMessage!;
        Assert.That(response.Text, Is.EqualTo("No Roblox user ids could be parsed."));
        Assert.That(response.Embed, Is.Null);
        Assert.That(response.MessageComponent, Is.Null);
    }
    
    [Test]
    public void TestValidateBansAsyncInvalidId()
    {
        Assert.That(this._banCommands.ValidateBansAsync("123,test  ").Result, Is.Null);
        
        var response = this._interactionContext.LastMessage!;
        Assert.That(response.Text, Is.EqualTo("The Roblox user id \"test\" could not be parsed."));
        Assert.That(response.Embed, Is.Null);
        Assert.That(response.MessageComponent, Is.Null);
    }
    
    [Test]
    public void TestValidateBansAsyncInvalidAccountLink()
    {
        this._interactionContext.BanPermissionResponse.CanBan = false;
        this._interactionContext.BanPermissionResponse.BanPermissionIssue = BanPermissionIssue.InvalidAccountLink;
        Assert.That(this._banCommands.ValidateBansAsync("123").Result, Is.Null);
        
        var response = this._interactionContext.LastMessage!;
        Assert.That(response.Text, Is.EqualTo("Your account is not linked. Use /startlink to start it."));
        Assert.That(response.Embed, Is.Null);
        Assert.That(response.MessageComponent, Is.Null);
    }
    
    [Test]
    public void TestValidateBansAsyncMalformedRobloxId()
    {
        this._interactionContext.BanPermissionResponse.CanBan = false;
        this._interactionContext.BanPermissionResponse.BanPermissionIssue = BanPermissionIssue.MalformedRobloxId;
        Assert.That(this._banCommands.ValidateBansAsync("123").Result, Is.Null);
        
        var response = this._interactionContext.LastMessage!;
        Assert.That(response.Text, Is.EqualTo("Your account link is malformed. Use /startlink to reset it."));
        Assert.That(response.Embed, Is.Null);
        Assert.That(response.MessageComponent, Is.Null);
    }
    
    [Test]
    public void TestValidateBansAsyncForbidden()
    {
        this._interactionContext.BanPermissionResponse.CanBan = false;
        this._interactionContext.BanPermissionResponse.BanPermissionIssue = BanPermissionIssue.Forbidden;
        Assert.That(this._banCommands.ValidateBansAsync("123").Result, Is.Null);
        
        var response = this._interactionContext.LastMessage!;
        Assert.That(response.Text, Is.EqualTo("You are not authorized to ban users."));
        Assert.That(response.Embed, Is.Null);
        Assert.That(response.MessageComponent, Is.Null);
    }
    
    [Test]
    public void TestValidateBansAsyncValid()
    {
        Assert.That(this._banCommands.ValidateBansAsync("123,456,123").Result, Is.EqualTo(new List<long>() { 123, 456 }));
        Assert.That(this._interactionContext.LastMessage, Is.Null);
    }
}
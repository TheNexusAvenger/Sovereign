using NUnit.Framework;
using Sovereign.Core.Model.Response;
using Sovereign.Discord.Discord.Commands;
using Sovereign.Discord.Test.Discord.Shim;

namespace Sovereign.Discord.Test.Discord.Commands;

public class LinkCommandsTest
{
    private TestInteractionContextWrapper _interactionContext;
    private LinkCommands _linkCommands;

    [SetUp]
    public void SetUp()
    {
        this._interactionContext = new TestInteractionContextWrapper();
        this._linkCommands = new LinkCommands();
        this._linkCommands.SetOverrideContextWrapper(this._interactionContext);
    }

    [Test]
    public void TestStartLinkNotConfigured()
    {
        this._interactionContext.DiscordGuildId = 34567;
        this._linkCommands.StartLink(12345).Wait();
        
        var response = this._interactionContext.LastMessage!;
        Assert.That(response.Text, Is.EqualTo("Sovereign is not configured for this server."));
        Assert.That(response.Embed, Is.Null);
        Assert.That(response.MessageComponent, Is.Null);
    }

    [Test]
    public void TestStartLinkUnauthorized()
    {
        this._interactionContext.BanPermissionResponse.CanLink = false;
        this._linkCommands.StartLink(12345).Wait();
        
        var response = this._interactionContext.LastMessage!;
        Assert.That(response.Text, Is.EqualTo("You are not authorized to link your account."));
        Assert.That(response.Embed, Is.Null);
        Assert.That(response.MessageComponent, Is.Null);
    }

    [Test]
    public void TestStartLinkSuccess()
    {
        this._linkCommands.StartLink(12345).Wait();
        
        var response = this._interactionContext.LastMessage!;
        Assert.That(response.Text, Is.Null);
        Assert.That(response.Embed!.Title, Is.EqualTo("Sovereign Account Link (12345)"));
        Assert.That(response.MessageComponent, Is.Not.Null);
    }

    [Test]
    public void TestSovereignDescriptionLinkCompleteNotConfigured()
    {
        this._interactionContext.DiscordGuildId = 34567;
        this._linkCommands.SovereignDescriptionLinkComplete(12345, "TestCode").Wait();
        
        var response = this._interactionContext.LastMessage!;
        Assert.That(response.Text, Is.EqualTo("Sovereign is not configured for this server."));
        Assert.That(response.Embed, Is.Null);
        Assert.That(response.MessageComponent, Is.Null);
    }

    [Test]
    public void TestSovereignDescriptionLinkCodeNotFound()
    {
        this._linkCommands.SovereignDescriptionLinkComplete(12345, "TestCode").Wait();
        
        var response = this._interactionContext.LastMessage!;
        Assert.That(response.Text, Is.EqualTo("The link code was not found in your profile's description (https://www.roblox.com/users/12345/profile)."));
        Assert.That(response.Embed, Is.Null);
        Assert.That(response.MessageComponent, Is.Null);
    }

    [Test]
    public void TestSovereignDescriptionSuccess()
    {
        this._interactionContext.UserProfileResponse.Description = "TestDescription TestCode";
        this._linkCommands.SovereignDescriptionLinkComplete(12345, "TestCode").Wait();
        
        var response = this._interactionContext.LastMessage!;
        Assert.That(response.Text, Is.EqualTo("Successfully linked your account with the Roblox user 12345."));
        Assert.That(response.Embed, Is.Null);
        Assert.That(response.MessageComponent, Is.Null);
    }

    [Test]
    public void TestSovereignDescriptionForbidden()
    {
        this._interactionContext.UserProfileResponse.Description = "TestDescription TestCode";
        this._interactionContext.LinkDiscordAccountResponse.Status = ResponseStatus.Forbidden;
        this._linkCommands.SovereignDescriptionLinkComplete(12345, "TestCode").Wait();
        
        var response = this._interactionContext.LastMessage!;
        Assert.That(response.Text, Is.EqualTo("You are not authorized to link your account."));
        Assert.That(response.Embed, Is.Null);
        Assert.That(response.MessageComponent, Is.Null);
    }

    [Test]
    public void TestSovereignDescriptionOther()
    {
        this._interactionContext.UserProfileResponse.Description = "TestDescription TestCode";
        this._interactionContext.LinkDiscordAccountResponse.Status = ResponseStatus.Unauthorized;
        this._linkCommands.SovereignDescriptionLinkComplete(12345, "TestCode").Wait();
        
        var response = this._interactionContext.LastMessage!;
        Assert.That(response.Text, Is.EqualTo("Error linking your Discord and Roblox accounts. This might be a configuration error in Sovereign."));
        Assert.That(response.Embed, Is.Null);
        Assert.That(response.MessageComponent, Is.Null);
    }

    [Test]
    public void TestSovereignDescriptionRegenerateCode()
    {
        this._linkCommands.SovereignDescriptionRegenerateCode(12345, "TestCode").Wait();
        
        var response = this._interactionContext.LastMessage!;
        Assert.That(response.Text, Is.Null);
        Assert.That(response.Embed!.Title, Is.EqualTo("Sovereign Account Link (12345)"));
        Assert.That(response.Embed!.Description, Does.Not.Contain("TestCode"));
        Assert.That(response.MessageComponent, Is.Not.Null);
    }
}
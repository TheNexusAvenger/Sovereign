using System.Linq;
using System.Text.RegularExpressions;
using Discord;
using NUnit.Framework;
using Sovereign.Discord.Discord.Component;

namespace Sovereign.Discord.Test.Discord.Component;

public class ProfileDescriptionLinkComponentTest
{
    [Test]
    public void TestInitialCode()
    {
        var component = new ProfileDescriptionLinkComponent(1234);
        component.RegenerateLinkCode();
        Assert.That(Regex.IsMatch(component.LinkCode, @"[\d\w]{2}_[\d\w]{2}_[\d\w]{2}"), Is.True);
    }
    
    [Test]
    public void TestRegenerateCode()
    {
        var component = new ProfileDescriptionLinkComponent(12345, "TestCode");
        component.RegenerateLinkCode();
        Assert.That(Regex.IsMatch(component.LinkCode, @"[\d\w]{2}_[\d\w]{2}_[\d\w]{2}"), Is.True);
    }
    
    [Test]
    public void TestGetEmbed()
    {
        var component = new ProfileDescriptionLinkComponent(12345, "TestCode");
        var embed = component.GetEmbed();
        Assert.That(embed.Title, Is.EqualTo("Sovereign Account Link (12345)"));
        Assert.That(embed.Description, Is.EqualTo($"To link your Discord account to Roblox user 12345, visit your profile (https://www.roblox.com/users/12345/profile) and add the following link code to your profile description, then click \"Link\" below. Click \"Regenerate\" for a new code if the code gets moderated.\n\nLink code: TestCode"));
        Assert.That(embed.Timestamp, Is.Not.Null);
        Assert.That(embed.Footer!.Value.Text, Is.EqualTo("Run /startlink again to link to a different account."));
    }
    
    [Test]
    public void TestGetMessageComponent()
    {
        var component = new ProfileDescriptionLinkComponent(12345, "TestCode");
        var messageComponent = component.GetMessageComponent();
        var actionRowComponent = (ActionRowComponent) messageComponent.Components.First();
        var linkButton = (ButtonComponent) actionRowComponent.Components.ToList()[0];
        Assert.That(linkButton.Label, Is.EqualTo("Link"));
        Assert.That(linkButton.CustomId, Is.EqualTo("SovereignDescriptionLinkComplete:12345:TestCode"));
        Assert.That(linkButton.Style, Is.EqualTo(ButtonStyle.Primary));
        var regenerateButton = (ButtonComponent) actionRowComponent.Components.ToList()[1];
        Assert.That(regenerateButton.Label, Is.EqualTo("Regenerate"));
        Assert.That(regenerateButton.CustomId, Is.EqualTo("SovereignDescriptionRegenerateCode:12345:TestCode"));
        Assert.That(regenerateButton.Style, Is.EqualTo(ButtonStyle.Secondary));
    }
}
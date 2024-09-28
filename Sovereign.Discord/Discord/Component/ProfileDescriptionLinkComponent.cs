using System;
using Discord;

namespace Sovereign.Discord.Discord.Component;

public class ProfileDescriptionLinkComponent
{
    /// <summary>
    /// Random characters used for generating codes.
    /// </summary>
    public const string RandomCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";

    /// <summary>
    /// Random number generator for random characters.
    /// </summary>
    private static readonly Random RandomCharacterGenerator = new Random();
    
    /// <summary>
    /// Roblox user id that is being linked.
    /// </summary>
    public long RobloxUserId { get; private set; }

    /// <summary>
    /// Code used to link a Discord account to the Roblox account.
    /// </summary>
    public string LinkCode { get; private set; } = null!;

    /// <summary>
    /// Creates a profile description link component.
    /// </summary>
    /// <param name="robloxUserId">Roblox user id the link is for.</param>
    /// <param name="linkCode">Link code used to link the account.</param>
    public ProfileDescriptionLinkComponent(long robloxUserId, string linkCode)
    {
        this.RobloxUserId = robloxUserId;
        this.LinkCode = linkCode;
    }

    /// <summary>
    /// Creates a profile description link component.
    /// A random link code will be generated.
    /// </summary>
    /// <param name="robloxUserId">Roblox user id the link is for.</param>
    public ProfileDescriptionLinkComponent(long robloxUserId)
    {
        this.RobloxUserId = robloxUserId;
        this.RegenerateLinkCode();
    }

    /// <summary>
    /// Generates a random character.
    /// </summary>
    /// <returns>Random character used to generate codes.</returns>
    public static char GetRandomCharacter()
    {
        return RandomCharacters[RandomCharacterGenerator.Next(RandomCharacters.Length)];
    }
    
    /// <summary>
    /// Regenerates the link code.
    /// </summary>
    public void RegenerateLinkCode()
    {
        this.LinkCode = $"{GetRandomCharacter()}{GetRandomCharacter()}_{GetRandomCharacter()}{GetRandomCharacter()}_{GetRandomCharacter()}{GetRandomCharacter()}";
    }

    /// <summary>
    /// Returns the Discord embed for the component.
    /// </summary>
    /// <returns>The Discord embed for the component.</returns>
    public Embed GetEmbed()
    {
        var description = $"To link your Discord account to Roblox user {this.RobloxUserId}, visit your profile (https://www.roblox.com/users/{this.RobloxUserId}/profile) and add the following link code to your profile description, then click \"Link\" below. Click \"Regenerate\" for a new code if the code gets moderated.\n\nLink code: {this.LinkCode}";
        return new EmbedBuilder()
            .WithTitle($"Sovereign Account Link ({this.RobloxUserId})")
            .WithDescription(description)
            .WithTimestamp(DateTimeOffset.Now)
            .WithFooter(new EmbedFooterBuilder().WithText("Run /startlink again to link to a different account."))
            .Build();
    }

    /// <summary>
    /// Returns the Discord message component for the message.
    /// </summary>
    /// <returns>The Discord message component for the message.</returns>
    public MessageComponent GetMessageComponent()
    {
        return new ComponentBuilder()
            .WithButton("Link", customId: $"SovereignDescriptionLinkComplete:{this.RobloxUserId}:{this.LinkCode}", style: ButtonStyle.Primary)
            .WithButton("Regenerate", customId: $"SovereignDescriptionRegenerateCode:{this.RobloxUserId}:{this.LinkCode}", style: ButtonStyle.Secondary)
            .Build();
    }
}
using System;
using System.Threading.Tasks;
using Bouncer.State;
using Discord;
using Discord.WebSocket;
using Sovereign.Core.Model.Response;
using Sovereign.Core.Model.Response.Api;
using Sovereign.Core.Web.Client;
using Sovereign.Core.Web.Client.Response;
using Sovereign.Discord.Configuration;
using Sovereign.Discord.Web.Client;

namespace Sovereign.Discord.Discord.Shim;

public class InteractionContextWrapper : IInteractionContextWrapper
{
    /// <summary>
    /// Discord user id of the interaction.
    /// </summary>
    public ulong DiscordUserId => this._context.User.Id;
    
    /// <summary>
    /// Discord guild id of the interaction.
    /// </summary>
    public ulong DiscordGuildId => this._context.Guild.Id;
    
    /// <summary>
    /// Context used to interact with Discord.
    /// </summary>
    private readonly IInteractionContext _context;

    /// <summary>
    /// Creates an InteractionContext wrapper.
    /// </summary>
    /// <param name="context">Context to use with Discord.</param>
    public InteractionContextWrapper(IInteractionContext context)
    {
        this._context = context;
    }

    /// <summary>
    /// Returns the current Discord configuration.
    /// </summary>
    /// <returns>Current Discord configuration.</returns>
    public DiscordConfiguration GetConfiguration()
    {
        return Configurations.GetConfiguration<DiscordConfiguration>();
    }
    
    /// <summary>
    /// Fetches the Roblox user profile.
    /// The information is not cached due to the use case in the Discord bot relying on not caching.
    /// </summary>
    /// <param name="robloxUserId">Roblox user id to get.</param>
    /// <returns>User profile for the user id.</returns>
    public async Task<UserProfileResponse> GetRobloxProfileAsync(long robloxUserId)
    {
        var profileClient = new RobloxUserProfileClient();
        return await profileClient.GetRobloxProfileAsync(robloxUserId);
    }

    /// <summary>
    /// Returns the ban permissions for a Roblox user.
    /// </summary>
    /// <param name="domain">Domain of the bans to get the permissions for.</param>
    /// <param name="robloxUserId">Roblox user id to check the permissions for.</param>
    /// <returns>Response for the ban permissions.</returns>
    public async Task<BanPermissionResponse> GetPermissionsForRobloxUserAsync(string domain, long robloxUserId)
    {
        var sovereignBansApiClient = new SovereignBansApiClient();
        return await sovereignBansApiClient.GetPermissionsForRobloxUserAsync(domain, robloxUserId);
    }

    /// <summary>
    /// Attempts to link a Discord account to a Roblox account.
    /// </summary>
    /// <param name="domain">Domain to link the accounts in.</param>
    /// <param name="discordUserId">Discord user id to link.</param>
    /// <param name="robloxUserId">Roblox user id to link.</param>
    /// <returns>Response for the link request.</returns>
    public async Task<SimpleResponse> LinkDiscordAccountAsync(string domain, ulong discordUserId, long robloxUserId)
    {
        var sovereignBansApiClient = new SovereignBansApiClient();
        return await sovereignBansApiClient.LinkDiscordAccountAsync(domain, discordUserId, robloxUserId);
    }

    /// <summary>
    /// Responds to the context user in a message not seen to other users.
    /// Exceptions for failed messages are not thrown.
    /// </summary>
    /// <param name="text">Text of the message to send, if any.</param>
    /// <param name="embed">Embed of the message to send, if any.</param>
    /// <param name="components">Message components of the message to send, if any.</param>
    public async Task RespondAsync(string? text = null, Embed? embed = null, MessageComponent? components = null)
    {
        try
        {
            await this._context.Interaction.RespondAsync(text: text, embed: embed, components: components, ephemeral: true);
        }
        catch (Exception)
        {
            // Ignore exceptions, in case the response was too late.
        }
    }
    
    /// <summary>
    /// Updates the component of the interacted message.
    /// </summary>
    /// <param name="embed">New embed to replace with.</param>
    /// <param name="messageComponent">New message component to replace with.</param>
    public async Task UpdateComponentAsync(Embed? embed = null, MessageComponent? messageComponent = null)
    {
        var component = (SocketMessageComponent) this._context.Interaction;
        await component.UpdateAsync(message =>
        {
            message.Embed = embed;
            message.Components = messageComponent;
        });
    }
}
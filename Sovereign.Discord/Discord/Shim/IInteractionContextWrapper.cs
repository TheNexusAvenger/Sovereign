using System.Threading.Tasks;
using Discord;
using Sovereign.Core.Model.Response;
using Sovereign.Core.Model.Response.Api;
using Sovereign.Core.Web.Client.Response;
using Sovereign.Discord.Configuration;

namespace Sovereign.Discord.Discord.Shim;

public interface IInteractionContextWrapper
{
    /// <summary>
    /// Discord user id of the interaction.
    /// </summary>
    public ulong DiscordUserId { get; }
    
    /// <summary>
    /// Discord guild id of the interaction.
    /// </summary>
    public ulong DiscordGuildId { get; }
    
    /// <summary>
    /// Returns the current Discord configuration.
    /// </summary>
    /// <returns>Current Discord configuration.</returns>
    public DiscordConfiguration GetConfiguration();

    /// <summary>
    /// Fetches the Roblox user profile.
    /// The information is not cached due to the use case in the Discord bot relying on not caching.
    /// </summary>
    /// <param name="robloxUserId">Roblox user id to get.</param>
    /// <returns>User profile for the user id.</returns>
    public Task<UserProfileResponse> GetRobloxProfileAsync(long robloxUserId);

    /// <summary>
    /// Returns the ban permissions for a Roblox user.
    /// </summary>
    /// <param name="domain">Domain of the bans to get the permissions for.</param>
    /// <param name="robloxUserId">Roblox user id to check the permissions for.</param>
    /// <returns>Response for the ban permissions.</returns>
    public Task<BanPermissionResponse> GetPermissionsForRobloxUserAsync(string domain, long robloxUserId);

    /// <summary>
    /// Attempts to link a Discord account to a Roblox account.
    /// </summary>
    /// <param name="domain">Domain to link the accounts in.</param>
    /// <param name="discordUserId">Discord user id to link.</param>
    /// <param name="robloxUserId">Roblox user id to link.</param>
    /// <returns>Response for the link request.</returns>
    public Task<SimpleResponse> LinkDiscordAccountAsync(string domain, ulong discordUserId, long robloxUserId);
    
    /// <summary>
    /// Responds to the context user in a message not seen to other users.
    /// Exceptions for failed messages are not thrown.
    /// </summary>
    /// <param name="text">Text of the message to send, if any.</param>
    /// <param name="embed">Embed of the message to send, if any.</param>
    /// <param name="components">Message components of the message to send, if any.</param>
    public Task RespondAsync(string? text = null, Embed? embed = null, MessageComponent? components = null);

    /// <summary>
    /// Updates the component of the interacted message.
    /// </summary>
    /// <param name="embed">New embed to replace with.</param>
    /// <param name="messageComponent">New message component to replace with.</param>
    public Task UpdateComponentAsync(Embed? embed = null, MessageComponent? messageComponent = null);
}
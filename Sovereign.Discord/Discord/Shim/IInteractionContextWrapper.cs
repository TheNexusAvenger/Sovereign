using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Sovereign.Core.Model;
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
    /// Contents of the source message of the interaction.
    /// </summary>
    public string? SourceMessage { get; }
    
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
    /// Returns the ban permissions for a Discord user.
    /// </summary>
    /// <param name="domain">Domain of the bans to get the permissions for.</param>
    /// <param name="discordUserId">Discord user id to check the permissions for.</param>
    /// <returns>Response for the ban permissions.</returns>
    public Task<BanPermissionResponse> GetPermissionsForDiscordUserAsync(string domain, ulong discordUserId);
    
    /// <summary>
    /// Attempts to link a Discord account to a Roblox account.
    /// </summary>
    /// <param name="domain">Domain to link the accounts in.</param>
    /// <param name="discordUserId">Discord user id to link.</param>
    /// <param name="robloxUserId">Roblox user id to link.</param>
    /// <returns>Response for the link request.</returns>
    public Task<SimpleResponse> LinkDiscordAccountAsync(string domain, ulong discordUserId, long robloxUserId);
    
    /// <summary>
    /// Bans a list of Roblox user ids.
    /// </summary>
    /// <param name="domain">Domain to link the accounts in.</param>
    /// <param name="banAction">Action to perform for the ban.</param>
    /// <param name="discordUserId">Discord user id to ban the users as.</param>
    /// <param name="robloxUserIds">Roblox user ids to link.</param>
    /// <param name="displayReason">Reason to display to the users.</param>
    /// <param name="privateReason">Reason to store internally for the bans.</param>
    /// <param name="duration">Optional duration of the ban in seconds.</param>
    /// <returns>Response for the bans.</returns>
    public Task<BanResponse> BanAsync(string domain, BanAction banAction, ulong discordUserId, List<long> robloxUserIds, string displayReason, string privateReason, long? duration = null);

    /// <summary>
    /// Fetches a ban record for a Roblox user id.
    /// Due to the UI only showing 1 ban at a time, only 1 ban record at most is returned.
    /// </summary>
    /// <param name="domain">Domain of the bans to fetch.</param>
    /// <param name="robloxUserId">Roblox user id to fetch the bans of.</param>
    /// <param name="banIndex">Optional index of the ban to fetch.</param>
    /// <returns>Response of the ban record entry.</returns>
    public Task<BanRecordResponse> GetBanRecordAsync(string domain, long robloxUserId, int banIndex = 0);
    
    /// <summary>
    /// Responds to the context user in a message not seen to other users.
    /// Exceptions for failed messages are not thrown.
    /// </summary>
    /// <param name="text">Text of the message to send, if any.</param>
    /// <param name="embed">Embed of the message to send, if any.</param>
    /// <param name="components">Message components of the message to send, if any.</param>
    public Task RespondAsync(string? text = null, Embed? embed = null, MessageComponent? components = null);

    /// <summary>
    /// Responds to the context user with a modal.
    /// Exceptions for failed messages are not thrown.
    /// </summary>
    /// <param name="modal">Modal to display to the user.</param>
    public Task RespondAsync(Modal modal);

    /// <summary>
    /// Updates the component of the interacted message.
    /// </summary>
    /// <param name="embed">New embed to replace with.</param>
    /// <param name="messageComponent">New message component to replace with.</param>
    public Task UpdateComponentAsync(Embed? embed = null, MessageComponent? messageComponent = null);
}
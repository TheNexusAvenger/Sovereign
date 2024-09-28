using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Sovereign.Core.Model.Response;
using Sovereign.Core.Model.Response.Api;
using Sovereign.Core.Web.Client.Response;
using Sovereign.Discord.Configuration;
using Sovereign.Discord.Discord.Shim;

namespace Sovereign.Discord.Test.Discord.Shim;

public class RespondedMessage
{
    public string? Text { get; set; }
    public Embed? Embed { get; set; }
    public MessageComponent? MessageComponent { get; set; }
}

public class TestInteractionContextWrapper : IInteractionContextWrapper
{
    /// <summary>
    /// Discord user id of the interaction.
    /// </summary>
    public ulong DiscordUserId { set; get; } = 12345;

    /// <summary>
    /// Discord guild id of the interaction.
    /// </summary>
    public ulong DiscordGuildId { set; get; } = 23456;

    /// <summary>
    /// Response to return for the user profile request.
    /// </summary>
    public UserProfileResponse UserProfileResponse { get; set; } = new UserProfileResponse()
    {
        Name = "TestName",
        DisplayName = "TestDisplayName",
        Description = "TestDescription",
    };

    /// <summary>
    /// Response to return for the ban permission request.
    /// </summary>
    public BanPermissionResponse BanPermissionResponse { get; set; } = new BanPermissionResponse()
    {
        Status = ResponseStatus.Success,
        CanLink = true,
        CanBan = true,
    };

    /// <summary>
    /// Response to return for the Discord account link request.
    /// </summary>
    public SimpleResponse LinkDiscordAccountResponse { get; set; } = new SimpleResponse(ResponseStatus.Success);
    
    /// <summary>
    /// Discord configuration to test.
    /// </summary>
    public DiscordConfiguration Configuration { get; set; } = new DiscordConfiguration()
    {
        Discord = new DiscordBotConfiguration()
        {
            Servers = new List<DiscordServerConfiguration>()
            {
                new DiscordServerConfiguration()
                {
                    Id = 23456,
                    Domain = "TestDomain",  
                },
            },
        },
    };
    
    /// <summary>
    /// Last message that was sent to the user.
    /// </summary>
    public RespondedMessage? LastMessage { get; private set; }

    /// <summary>
    /// Returns the current Discord configuration.
    /// </summary>
    /// <returns>Current Discord configuration.</returns>
    public DiscordConfiguration GetConfiguration()
    {
        return this.Configuration;
    }
    
    /// <summary>
    /// Fetches the Roblox user profile.
    /// The information is not cached due to the use case in the Discord bot relying on not caching.
    /// </summary>
    /// <param name="robloxUserId">Roblox user id to get.</param>
    /// <returns>User profile for the user id.</returns>
    public Task<UserProfileResponse> GetRobloxProfileAsync(long robloxUserId)
    {
        return Task.FromResult(this.UserProfileResponse);
    }

    /// <summary>
    /// Returns the ban permissions for a Roblox user.
    /// </summary>
    /// <param name="domain">Domain of the bans to get the permissions for.</param>
    /// <param name="robloxUserId">Roblox user id to check the permissions for.</param>
    /// <returns>Response for the ban permissions.</returns>
    public Task<BanPermissionResponse> GetPermissionsForRobloxUserAsync(string domain, long robloxUserId)
    {
        return Task.FromResult(this.BanPermissionResponse);
    }

    /// <summary>
    /// Attempts to link a Discord account to a Roblox account.
    /// </summary>
    /// <param name="domain">Domain to link the accounts in.</param>
    /// <param name="discordUserId">Discord user id to link.</param>
    /// <param name="robloxUserId">Roblox user id to link.</param>
    /// <returns>Response for the link request.</returns>
    public Task<SimpleResponse> LinkDiscordAccountAsync(string domain, ulong discordUserId, long robloxUserId)
    {
        return Task.FromResult(this.LinkDiscordAccountResponse);
    }

    /// <summary>
    /// Responds to the context user in a message not seen to other users.
    /// Exceptions for failed messages are not thrown.
    /// </summary>
    /// <param name="text">Text of the message to send, if any.</param>
    /// <param name="embed">Embed of the message to send, if any.</param>
    /// <param name="components">Message components of the message to send, if any.</param>
    public Task RespondAsync(string? text = null, Embed? embed = null, MessageComponent? components = null)
    {
        this.LastMessage = new RespondedMessage()
        {
            Text = text,
            Embed = embed,
            MessageComponent = components,
        };
        return Task.CompletedTask;
    }
    
    /// <summary>
    /// Updates the component of the interacted message.
    /// </summary>
    /// <param name="embed">New embed to replace with.</param>
    /// <param name="messageComponent">New message component to replace with.</param>
    public Task UpdateComponentAsync(Embed? embed = null, MessageComponent? messageComponent = null)
    {
        this.LastMessage = new RespondedMessage()
        {
            Embed = embed,
            MessageComponent = messageComponent,
        };
        return Task.CompletedTask;
    }
}
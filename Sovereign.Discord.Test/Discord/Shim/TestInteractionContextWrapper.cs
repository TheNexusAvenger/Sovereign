using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Sovereign.Core.Model;
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
    public ulong DiscordUserId { get; set; } = 12345;

    /// <summary>
    /// Discord guild id of the interaction.
    /// </summary>
    public ulong DiscordGuildId { get; set; } = 23456;

    /// <summary>
    /// Contents of the source message of the interaction.
    /// </summary>
    public string? SourceMessage { get; set; } = null;

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
    /// Response to return for a ban request.
    /// </summary>
    public BanResponse BanResponse { get; set; } = new BanResponse();

    /// <summary>
    /// Response to return for a ban record request.
    /// </summary>
    public BanRecordResponse BanRecordResponse { get; set; } = new BanRecordResponse();
    
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
        Domains = new List<DiscordDomainConfiguration>()
        {
            new DiscordDomainConfiguration()
            {
                Name = "TestDomain",
                ApiSecretKey = "TestSecretKey",
                BanOptions = new List<DiscordDomainBanOptionConfiguration>()
                {
                    new DiscordDomainBanOptionConfiguration()
                    {
                        Name = "Exploiting",
                        Description = "Please specify details in the private reason.",
                        DefaultDisplayReason = "Banned for exploiting. Use the Discord server in the game's social links to appeal.",
                    },
                    new DiscordDomainBanOptionConfiguration()
                    {
                        Name = "Harassment",
                        DefaultDisplayReason = "Banned for harassment. Use the Discord server in the game's social links to appeal.",
                        DefaultPrivateReason = "No information given.",
                    },
                    new DiscordDomainBanOptionConfiguration()
                    {
                        Name = "Other",
                        DefaultDisplayReason = "You are banned. Use the Discord server in the game's social links to appeal.",
                    },
                },
            },
        },
    };
    
    /// <summary>
    /// Last message that was sent to the user.
    /// </summary>
    public RespondedMessage? LastMessage { get; private set; }
    
    /// <summary>
    /// Last modal that was sent to the user.
    /// </summary>
    public Modal? LastModal { get; private set; }

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
    /// Returns the ban permissions for a Discord user.
    /// </summary>
    /// <param name="domain">Domain of the bans to get the permissions for.</param>
    /// <param name="discordUserId">Discord user id to check the permissions for.</param>
    /// <returns>Response for the ban permissions.</returns>
    public Task<BanPermissionResponse> GetPermissionsForDiscordUserAsync(string domain, ulong discordUserId)
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
    /// Bans or unbans a list of Roblox user ids.
    /// </summary>
    /// <param name="domain">Domain to link the accounts in.</param>
    /// <param name="banAction">Action to perform for the ban.</param>
    /// <param name="discordUserId">Discord user id to ban the users as.</param>
    /// <param name="robloxUserIds">Roblox user ids to link.</param>
    /// <param name="displayReason">Reason to display to the users.</param>
    /// <param name="privateReason">Reason to store internally for the bans.</param>
    /// <param name="duration">Optional duration of the ban in seconds.</param>
    /// <returns>Response for the bans.</returns>
    public Task<BanResponse> BanAsync(string domain, BanAction banAction, ulong discordUserId, List<long> robloxUserIds, string displayReason, string privateReason, long? duration = null)
    {
        return Task.FromResult(this.BanResponse);
    }

    /// <summary>
    /// Fetches a ban record for a Roblox user id.
    /// Due to the UI only showing 1 ban at a time, only 1 ban record at most is returned.
    /// </summary>
    /// <param name="domain">Domain of the bans to fetch.</param>
    /// <param name="robloxUserId">Roblox user id to fetch the bans of.</param>
    /// <param name="banIndex">Optional index of the ban to fetch.</param>
    /// <returns>Response of the ban record entry.</returns>
    public Task<BanRecordResponse> GetBanRecordAsync(string domain, long robloxUserId, int banIndex = 0)
    {
        return Task.FromResult(this.BanRecordResponse);
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
    /// Responds to the context user with a modal.
    /// Exceptions for failed messages are not thrown.
    /// </summary>
    /// <param name="modal">Modal to display to the user.</param>
    public Task RespondAsync(Modal modal)
    {
        this.LastModal = modal;
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
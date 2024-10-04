using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Bouncer.Diagnostic;
using Discord;
using Discord.Interactions;
using Sovereign.Core.Model;
using Sovereign.Core.Model.Response;
using Sovereign.Core.Model.Response.Api;

namespace Sovereign.Discord.Discord.Commands;

public class BanPromptModal : IModal
{
    /// <summary>
    /// Dummy title of the prompt.
    /// </summary>
    public string Title => "Ban Prompt";

    /// <summary>
    /// Display reason for the ban.
    /// </summary>
    [ModalTextInput("DisplayReason")]
    public string DisplayReason { get; set; } = null!;
    
    /// <summary>
    /// Private reason for the ban.
    /// </summary>
    [ModalTextInput("PrivateReason")]
    public string PrivateReason { get; set; } = null!;
    
    /// <summary>
    /// User ids to ban.
    /// </summary>
    [ModalTextInput("RobloxUserIds")]
    public string RobloxUserIds { get; set; } = null!;
    
    /// <summary>
    /// Optional duration of the ban.
    /// </summary>
    [ModalTextInput("Duration")]
    public string Duration { get; set; } = null!;
}

public class UnbanPromptModal : IModal
{
    /// <summary>
    /// Dummy title of the prompt.
    /// </summary>
    public string Title => "Ban Prompt";
    
    /// <summary>
    /// Private reason for the ban.
    /// </summary>
    [ModalTextInput("PrivateReason")]
    public string PrivateReason { get; set; } = null!;
    
    /// <summary>
    /// User ids to ban.
    /// </summary>
    [ModalTextInput("RobloxUserIds")]
    public string RobloxUserIds { get; set; } = null!;
}

public partial class BanCommands : ExtendedInteractionModuleBase
{
    /// <summary>
    /// Max length of the display message.
    /// </summary>
    public const int MaxDisplayMessageLength = 400;
    
    /// <summary>
    /// Max length of the private message.
    /// </summary>
    public const int MaxPrivateMessageLength = 1000;

    /// <summary>
    /// Seconds in an hour.
    /// </summary>
    public const long SecondsPerHour = 60 * 60;
    
    /// <summary>
    /// Handles a command to ban users.
    /// </summary>
    [SlashCommand("startban", "Prompts banning users using Sovereign.")]
    public async Task StartBan([Summary("Roblox_User_Ids", "Roblox user ids to ban, separated by commas.")] string robloxUserIds)
    {
        try
        {
            // Validate the user and Roblox ids.
            var parsedRobloxUserIds = await this.ValidateBansAsync(robloxUserIds);
            if (parsedRobloxUserIds == null)
            {
                return;
            }
            
            // Return if there are no ban reasons.
            var context = this.GetContext();
            var domain = this.GetDomain()!;
            var configuration = context.GetConfiguration();
            var banOptions = configuration.Domains?
                .FirstOrDefault(entry => string.Equals(entry.Name, domain, StringComparison.CurrentCultureIgnoreCase))?
                .BanOptions;
            if (banOptions == null || banOptions.Count == 0)
            {
                Logger.Warn($"Domain {domain} is not configured with ban options.");
                await context.RespondAsync("Ban options are not configured for this server.");
                return;
            }

            if (banOptions.Count == 1)
            {
                // Show the prompt.
                await this.DisplayBanModalAsync(banOptions.First().Name!, string.Join(", ", parsedRobloxUserIds));
            }
            else
            {
                // Return the list of ban options.
                var menuBuilder = new SelectMenuBuilder()
                    .WithPlaceholder("Select a ban option")
                    .WithCustomId("SovereignBanOptions")
                    .WithMinValues(1)
                    .WithMaxValues(1);
                foreach (var banOption in banOptions)
                {
                    menuBuilder.AddOption(banOption.Name, banOption.Name, banOption.Description);
                }
                var builder = new ComponentBuilder()
                    .WithSelectMenu(menuBuilder);
                await context.RespondAsync($"Select a ban option to ban the following: {string.Join(", ", parsedRobloxUserIds)}", components: builder.Build());
            }
        }
        catch (Exception e)
        {
            Logger.Error($"Error processing /startban command.\n{e}");
        }
    }
    
    [SlashCommand("startunban", "Prompts unbanning users using Sovereign.")]
    public async Task StartUnban([Summary("Roblox_User_Ids", "Roblox user ids to unban, separated by commas.")] string robloxUserIds)
    {
        try
        {
            // Validate the user and Roblox ids.
            var parsedRobloxUserIds = await this.ValidateBansAsync(robloxUserIds);
            if (parsedRobloxUserIds == null)
            {
                return;
            }
            
            // Build and present the unban prompt.
            var context = this.GetContext();
            var modalBuilder = new ModalBuilder()
                .WithTitle($"Unban")
                .WithCustomId("SovereignUnbanPrompt")
                .AddTextInput("Private Reason", "PrivateReason", TextInputStyle.Paragraph, maxLength: MaxPrivateMessageLength, required: false, value: "No information provided.")
                .AddTextInput("Roblox User Ids", "RobloxUserIds", TextInputStyle.Paragraph, value: string.Join(", ", parsedRobloxUserIds));
            await context.RespondAsync(modalBuilder.Build());
        }
        catch (Exception e)
        {
            Logger.Error($"Error processing /startunban command.\n{e}");
        }
    }
    
    /// <summary>
    /// Handles a ban option being selected.
    /// </summary>
    [ComponentInteraction("SovereignBanOptions")]
    public async Task SovereignBanOptions(string[] selectedBanOptions)
    {
        var banOptionName = selectedBanOptions.First();
        try
        {
            var context = this.GetContext();
            await this.DisplayBanModalAsync(banOptionName, PreviousMessageUserIdRegex().Match(context.SourceMessage!).Groups[1].Value);
        }
        catch (Exception e)
        {
            Logger.Error($"Error processing ban option {banOptionName}.\n{e}");
        }
    }

    /// <summary>
    /// Handles a ban prompt being completed.
    /// </summary>
    [ModalInteraction("SovereignBanPrompt:*")]
    public async Task SovereignBanPrompt(string excludeAltAccounts, BanPromptModal modal)
    {
        try
        {
            // Validate the user ids again since they may have changed.
            var parsedRobloxUserIds = await this.ValidateBansAsync(modal.RobloxUserIds);
            if (parsedRobloxUserIds == null)
            {
                return;
            }
            
            // Parse the duration.
            var context = this.GetContext();
            var durationText = modal.Duration.Trim();
            long? durationSeconds = null;
            if (durationText != "")
            {
                if (!float.TryParse(durationText, out var durationHours))
                {
                    Logger.Debug($"Discord user {context.DiscordUserId} attempted to ban with a duration of \"{durationText}\" hours but could not be parsed to a float.");
                    await context.RespondAsync($"Duration \"{durationText}\" could not be parsed.");
                    return;
                }
                durationSeconds = (long) (SecondsPerHour * durationHours);
            }
            
            // Send the ban request.
            var domain = this.GetDomain()!;
            var displayMessage = modal.DisplayReason;
            var privateReason = modal.PrivateReason;
            try
            {
                var excludeAltAccountsBool = (excludeAltAccounts.Equals("true", StringComparison.CurrentCultureIgnoreCase));
                var response = await context.BanAsync(domain, BanAction.Ban, context.DiscordUserId, parsedRobloxUserIds, displayMessage, privateReason, excludeAltAccountsBool, durationSeconds);
                if (response.Status == ResponseStatus.Success)
                {
                    await context.RespondAsync($"Banned {response.BannedUserIds.Count} user(s).");
                }
                else if (response.Status == ResponseStatus.Forbidden)
                {
                    await context.RespondAsync("You are not authorized to ban users.");
                }
                else if (response.Status == ResponseStatus.GroupRankPermissionError)
                {
                    await context.RespondAsync("You are not authorized to ban users with the same or higher rank in the configured group.");
                }
                else
                {
                    await context.RespondAsync("A configuration error occured when handling the bans.");
                }
            }
            catch (Exception)
            {
                await context.RespondAsync("An error occured processing the ban.");
                throw;
            }
        }
        catch (Exception e)
        {
            Logger.Error($"Error processing ban prompt.\n{e}");
        }
    }
    
    /// <summary>
    /// Handles an unban prompt being completed.
    /// </summary>
    [ModalInteraction("SovereignUnbanPrompt")]
    public async Task SovereignUnbanPrompt(UnbanPromptModal modal)
    {
        try
        {
            // Validate the user ids again since they may have changed.
            var parsedRobloxUserIds = await this.ValidateBansAsync(modal.RobloxUserIds);
            if (parsedRobloxUserIds == null)
            {
                return;
            }
            
            // Send the unban request.
            var context = this.GetContext();
            var domain = this.GetDomain()!;
            var privateReason = modal.PrivateReason;
            try
            {
                var response = await context.BanAsync(domain, BanAction.Unban, context.DiscordUserId, parsedRobloxUserIds, "", privateReason);
                if (response.Status == ResponseStatus.Success)
                {
                    await context.RespondAsync($"Unbanned {response.UnbannedUserIds.Count} user(s).");
                }
                else if (response.Status == ResponseStatus.Forbidden)
                {
                    await context.RespondAsync("You are not authorized to unban users.");
                }
                else
                {
                    await context.RespondAsync("A configuration error occured when handling the unbans.");
                }
            }
            catch (Exception)
            {
                await context.RespondAsync("An error occured processing the unban.");
                throw;
            }
        }
        catch (Exception e)
        {
            Logger.Error($"Error processing ban prompt.\n{e}");
        }
    }

    /// <summary>
    /// Validates the user can ban.
    /// Returns the list of Roblox user ids if the ban can continue.
    /// </summary>
    /// <param name="robloxUserIds"></param>
    /// <returns>List of user ids to ban or unban, if the user can ban.</returns>
    public async Task<List<long>?> ValidateBansAsync(string robloxUserIds)
    {
        // Display a message if the domain is not configured.
        var context = this.GetContext();
        var domain = this.GetDomain();
        if (domain == null)
        {
            Logger.Warn($"Discord server {context.DiscordGuildId} is not configured with a domain.");
            await context.RespondAsync("Sovereign is not configured for this server.");
            return null;
        }
        
        // Parse the list of user ids.
        var parsedRobloxUserIds = new List<long>();
        foreach (var robloxUserId in robloxUserIds.Split(','))
        {
            var trimmedRobloxUserId = robloxUserId.Trim();
            if (string.IsNullOrEmpty(trimmedRobloxUserId)) continue;
            if (!long.TryParse(trimmedRobloxUserId, out var parsedRobloxUserId))
            {
                Logger.Debug($"Discord user {context.DiscordUserId} attempted to ban \"{trimmedRobloxUserId}\" but could not be parsed to a long.");
                await context.RespondAsync($"The Roblox user id \"{trimmedRobloxUserId}\" could not be parsed.");
                return null;
            }
            if (parsedRobloxUserIds.Contains(parsedRobloxUserId)) continue;
            parsedRobloxUserIds.Add(parsedRobloxUserId);
        }
        if (parsedRobloxUserIds.Count == 0)
        {
            Logger.Debug($"Discord user {context.DiscordUserId} attempted to ban 0 parsed Roblox user ids.");
            await context.RespondAsync($"No Roblox user ids could be parsed.");
            return null;
        }
            
        // Check if the user can ban.
        try
        {
            var userPermissions = await context.GetPermissionsForDiscordUserAsync(domain, context.DiscordUserId);
            if (!userPermissions.CanBan)
            {
                Logger.Debug($"Discord user {context.DiscordUserId} attempted to ban in server {context.DiscordGuildId} for domain {domain} but was not allowed to ban due to {userPermissions.BanPermissionIssue}.");
                if (userPermissions.BanPermissionIssue == BanPermissionIssue.InvalidAccountLink)
                {
                    await context.RespondAsync("Your account is not linked. Use /startlink to start it.");
                }
                else if (userPermissions.BanPermissionIssue == BanPermissionIssue.MalformedRobloxId)
                {
                    await context.RespondAsync("Your account link is malformed. Use /startlink to reset it.");
                }
                else
                {
                    await context.RespondAsync("You are not authorized to ban users.");
                }
                return null;
            }
        }
        catch (Exception)
        {
            await context.RespondAsync("Error occured when checking if your account can ban users.");
            throw;
        }
        
        // Return the Roblox user ids.
        return parsedRobloxUserIds;
    }

    /// <summary>
    /// Displays a ban modal.
    /// </summary>
    /// <param name="banOptionName">Ban option name to show the modal for.</param>
    /// <param name="banIdList">List of Roblox user ids to ban.</param>
    public async Task DisplayBanModalAsync(string banOptionName, string banIdList)
    {
        // Get the ban option.
        var context = this.GetContext();
        var domain = this.GetDomain()!;
        var configuration = context.GetConfiguration();
        var banOption = configuration.Domains?
            .FirstOrDefault(entry => string.Equals(entry.Name, domain, StringComparison.CurrentCultureIgnoreCase))?
            .BanOptions!.FirstOrDefault(option => option.Name == banOptionName);
        if (banOption == null)
        {
            await context.RespondAsync("The ban option is no longer configured on the server.");
            return;
        }
            
        // Build and present the ban prompt.
        var modalBuilder = new ModalBuilder()
            .WithTitle($"Ban ({banOptionName})")
            .WithCustomId($"SovereignBanPrompt:{banOption.ExcludeAltAccounts == true}")
            .AddTextInput("Display Reason", "DisplayReason", TextInputStyle.Paragraph, maxLength: MaxDisplayMessageLength, required: false, value: banOption.DefaultDisplayReason)
            .AddTextInput("Private Reason", "PrivateReason", TextInputStyle.Paragraph, maxLength: MaxPrivateMessageLength, required: false, value: banOption.DefaultPrivateReason)
            .AddTextInput("Roblox User Ids", "RobloxUserIds", TextInputStyle.Paragraph, value: banIdList)
            .AddTextInput("Optional Duration (Hours)", "Duration", required: false, value: "");
        await context.RespondAsync(modalBuilder.Build());
    }

    /// <summary>
    /// Regular expression for getting the Roblox user ids from the previous message.
    /// </summary>
    [GeneratedRegex(@": (.+)")]
    private static partial Regex PreviousMessageUserIdRegex();
}
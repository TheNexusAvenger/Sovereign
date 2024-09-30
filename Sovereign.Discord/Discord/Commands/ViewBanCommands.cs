using System;
using System.Threading.Tasks;
using Bouncer.Diagnostic;
using Discord.Interactions;
using Sovereign.Core.Model.Response;
using Sovereign.Discord.Discord.Component;

namespace Sovereign.Discord.Discord.Commands;

public class ViewBanCommands : ExtendedInteractionModuleBase
{
    /// <summary>
    /// Handles a command to view bans.
    /// </summary>
    [SlashCommand("viewban", "Shows the bans of a user in Sovereign.")]
    public async Task ViewBan([Summary("Roblox_User_Id", "Roblox user id to view the bans of.")] long robloxUserId)
    {
        try
        {
            // Display a message if the domain is not configured.
            var context = this.GetContext();
            var domain = this.GetDomain();
            if (domain == null)
            {
                Logger.Warn($"Discord server {context.DiscordGuildId} is not configured with a domain.");
                await context.RespondAsync("Sovereign is not configured for this server.");
                return;
            }
            
            // Get and display the latest ban.
            try
            {
                var discordUserId = context.DiscordUserId;
                var banRecordResponse = await context.GetBanRecordAsync(domain, robloxUserId);
                if (banRecordResponse.Status == ResponseStatus.Success)
                {
                    var component = new BanViewComponent(robloxUserId, banRecordResponse);
                    Logger.Info($"Returning ban record of user {robloxUserId} to Discord user {discordUserId}.");
                    await context.RespondAsync(embed: component.GetEmbed(), components: component.GetMessageComponent());
                }
                else
                {
                    await context.RespondAsync("A configuration error occured when fetching the ban history.");
                    Logger.Info($"Discord user {discordUserId} attempted to view bans for {robloxUserId} but got {banRecordResponse.Status}.");
                }
            }
            catch (Exception)
            {
                await context.RespondAsync("An error occured getting the ban.");
                throw;
            }
        }
        catch (Exception e)
        {
            Logger.Error($"Error processing /viewban command.\n{e}");
        }
    }
    
    /// <summary>
    /// Handles requesting a new page of a ban entry.
    /// </summary>
    [ComponentInteraction("SovereignViewBan:*:*")]
    public async Task CommendsLogsPreviousPage(long robloxUserId, int entryIndex)
    {
        try
        {
            // Display a message if the domain is not configured.
            var context = this.GetContext();
            var domain = this.GetDomain();
            if (domain == null)
            {
                Logger.Warn($"Discord server {context.DiscordGuildId} is not configured with a domain.");
                await context.RespondAsync("Sovereign is not configured for this server.");
                return;
            }
            
            // Get and display the latest ban.
            try
            {
                var discordUserId = context.DiscordUserId;
                var banRecordResponse = await context.GetBanRecordAsync(domain, robloxUserId, entryIndex);
                if (banRecordResponse.Status == ResponseStatus.Success)
                {
                    var component = new BanViewComponent(robloxUserId, banRecordResponse, entryIndex);
                    Logger.Info($"Returning ban record index {entryIndex} of user {robloxUserId} to Discord user {discordUserId}.");
                    await context.UpdateComponentAsync(embed: component.GetEmbed(), messageComponent: component.GetMessageComponent());
                }
                else
                {
                    await context.RespondAsync("A configuration error occured when fetching the ban history.");
                    Logger.Info($"Discord user {discordUserId} attempted to view ban entry {entryIndex} for {robloxUserId} but got {banRecordResponse.Status}.");
                }
            }
            catch (Exception)
            {
                await context.RespondAsync("An error occured getting the ban.");
                throw;
            }
        }
        catch (Exception e)
        {
            Logger.Error($"Error processing view ban request for {robloxUserId} with entry {entryIndex}.\n{e}");
        }
    }
}
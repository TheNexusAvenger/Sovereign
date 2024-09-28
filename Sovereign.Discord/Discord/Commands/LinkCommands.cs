using System;
using System.Threading.Tasks;
using Bouncer.Diagnostic;
using Discord.Interactions;
using Sovereign.Core.Model.Response;
using Sovereign.Discord.Discord.Component;

namespace Sovereign.Discord.Discord.Commands;

public class LinkCommands : ExtendedInteractionModuleBase
{
    /// <summary>
    /// Handles a command to start Discord account linking.
    /// TODO: Consider OAuth2 support with configuration to allow/block link methods.
    /// </summary>
    [SlashCommand("startlink", "Prompts linking your Discord account to your Roblox account.")]
    public async Task StartLink(long robloxUserId)
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
            
            // Check if the user can link.
            try
            {
                var userPermissions = await context.GetPermissionsForRobloxUserAsync(domain, robloxUserId);
                if (!userPermissions.CanLink)
                {
                    Logger.Debug($"Discord user {context.DiscordUserId} attempted to link in server {context.DiscordGuildId} for domain {domain} but was not allowed link.");
                    await context.RespondAsync("You are not authorized to link your account.");
                    return;
                }
            }
            catch (Exception)
            {
                await context.RespondAsync("Error occured when checking if the accounts could be linked.");
                throw;
            }
            
            // Show the initial link view.
            var linkComponent = new ProfileDescriptionLinkComponent(robloxUserId);
            await context.RespondAsync(embed: linkComponent.GetEmbed(), components: linkComponent.GetMessageComponent());
        }
        catch (Exception e)
        {
            Logger.Error($"Error processing /startlink command.\n{e}");
        }
    }
    
    /// <summary>
    /// Handles an interaction to complete description-based linking.
    /// </summary>
    [ComponentInteraction("SovereignDescriptionLinkComplete:*:*")]
    public async Task SovereignDescriptionLinkComplete(long robloxUserId, string linkCode)
    {
        try {
            // Display a message if the domain is not configured.
            var context = this.GetContext();
            var domain = this.GetDomain();
            if (domain == null)
            {
                Logger.Warn($"Discord server {context.DiscordGuildId} is not configured with a domain.");
                await context.RespondAsync("Sovereign is not configured for this server.");
                return;
            }
            
            // Verify that the user description contains the link code.
            try
            {
                var userProfile = await context.GetRobloxProfileAsync(robloxUserId);
                if (!userProfile.Description.Contains(linkCode))
                {
                    await context.RespondAsync($"The link code was not found in your profile's description (https://www.roblox.com/users/{robloxUserId}/profile).");
                    return;
                }
            }
            catch (Exception)
            {
                await context.RespondAsync("Error occured when checking the Roblox profile description.");
                throw;
            }
            
            // Set the account link.
            try
            {
                var discordUserId = context.DiscordUserId;
                Logger.Debug($"Attempting to link Discord user {discordUserId} to link with Roblox user {robloxUserId}.");
                var linkResponse = await context.LinkDiscordAccountAsync(domain, discordUserId, robloxUserId);
                if (linkResponse.Status == ResponseStatus.Success)
                {
                    await context.RespondAsync($"Successfully linked your account with the Roblox user {robloxUserId}.");
                    Logger.Info($"Discord user {discordUserId} attempted to link with Roblox user {robloxUserId} but was not authorized.");
                }
                else if (linkResponse.Status == ResponseStatus.Forbidden)
                {
                    await context.RespondAsync("You are not authorized to link your account.");
                    Logger.Warn($"Discord user {discordUserId} attempted to link with Roblox user {robloxUserId} but was not authorized.");
                }
                else
                {
                    await context.RespondAsync("Error linking your Discord and Roblox accounts. This might be a configuration error in Sovereign.");
                    Logger.Warn($"Discord user {discordUserId} attempted to link with Roblox user {robloxUserId} but got {linkResponse.Status}.");
                }
            }
            catch (Exception)
            {
                await context.RespondAsync("Error linking your Discord and Roblox accounts.");
                throw;
            }
        }
        catch (Exception e)
        {
            Logger.Error($"Error processing SovereignDescriptionLinkComplete interaction.\n{e}");
        }
    }
    
    /// <summary>
    /// Handles an interaction to regenerate the code for description-based linking.
    /// </summary>
    [ComponentInteraction("SovereignDescriptionRegenerateCode:*:*")]
    public async Task SovereignDescriptionRegenerateCode(long robloxUserId, string linkCode)
    {
        try {
            var context = this.GetContext();
            var linkComponent = new ProfileDescriptionLinkComponent(robloxUserId, linkCode);
            linkComponent.RegenerateLinkCode();
            Logger.Debug($"Regenerating link code for Discord user {context.DiscordUserId} attempting to link with {robloxUserId}.");
            await context.UpdateComponentAsync(linkComponent.GetEmbed(), linkComponent.GetMessageComponent());
        }
        catch (Exception e)
        {
            Logger.Error($"Error processing SovereignDescriptionRegenerateCode interaction.\n{e}");
        }
    }
}
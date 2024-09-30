using System;
using System.Linq;
using System.Text;
using Discord;
using Sovereign.Core.Model.Response.Api;
using Sovereign.Core.Web.Client;
using Sovereign.Core.Web.Client.Response;

namespace Sovereign.Discord.Discord.Component;

public class BanViewComponent
{
    /// <summary>
    /// Roblox user id for the (un)ban.
    /// </summary>
    private readonly long _robloxUserId;
    
    /// <summary>
    /// Ban entry to display, if one exists.
    /// </summary>
    private readonly BanRecordResponseEntry? _banEntry;

    /// <summary>
    /// Index of the ban that is displayed.
    /// </summary>
    private readonly int _banIndex;
    
    /// <summary>
    /// Total number of bans for the user.
    /// </summary>
    private readonly int _totalBans;

    /// <summary>
    /// User profile for the (un)banned Roblox user.
    /// </summary>
    private readonly UserProfileResponse _userProfileResponse;

    /// <summary>
    /// User profile for the (un)banning Roblox user.
    /// </summary>
    private readonly UserProfileResponse? _banningUerProfileResponse;
    
    /// <summary>
    /// Creates a ban view component.
    /// </summary>
    /// <param name="robloxUserId">Roblox user id for the (un)ban.</param>
    /// <param name="banRecordResponse">Ban entry to display, if one exists.</param>
    /// <param name="banIndex">Index of the ban that is displayed.</param>
    /// <param name="userProfileResponse">User profile for the (un)banned Roblox user. If not provided, it will be fetched.</param>
    /// <param name="banningUserProfileResponse">User profile for the (un)banning Roblox user. If not provided, it will be fetched.</param>
    public BanViewComponent(long robloxUserId, BanRecordResponse banRecordResponse, int banIndex = 0, UserProfileResponse? userProfileResponse = null, UserProfileResponse? banningUserProfileResponse = null)
    {
        this._robloxUserId = robloxUserId;
        this._banEntry = banRecordResponse.Entries.FirstOrDefault();
        this._banIndex = banIndex;
        this._totalBans = banRecordResponse.Total;

        var banningUserId = this._banEntry?.Reason?.ActingUserId;
        if (userProfileResponse == null || (banningUserId != null && banningUserProfileResponse == null))
        {
            var client = RobloxUserProfileClient.CachingClient;
            this._userProfileResponse = userProfileResponse ?? client.GetRobloxProfileAsync(robloxUserId).Result;
            if (banningUserId != null)
            {
                this._banningUerProfileResponse = banningUserProfileResponse ?? client.GetRobloxProfileAsync(banningUserId.Value).Result;
            }
        }
        else
        {
            this._userProfileResponse = userProfileResponse;
            this._banningUerProfileResponse = banningUserProfileResponse;
        }
    }

    /// <summary>
    /// Returns the Discord embed for the component.
    /// </summary>
    /// <returns>The Discord embed for the component.</returns>
    public Embed GetEmbed()
    {
        // Build the description.
        var description = "No ban record to show.";
        if (this._banEntry != null)
        {
            var newDescription = new StringBuilder();
            newDescription.Append($"**Action**: {this._banEntry.Action.Type}");
            newDescription.Append($"\n**Display reason**: {this._banEntry.Reason.Display}");
            newDescription.Append($"\n**Private reason**: {this._banEntry.Reason.Private}");
            newDescription.Append($"\n**Handled by**: {this._banningUerProfileResponse?.DisplayName} (@{this._banningUerProfileResponse?.Name}) [{this._banEntry.Reason.ActingUserId}]");
            newDescription.Append($"\n**Start time**: {this._banEntry.Action.StartTime}");
            if (this._banEntry.Action.EndTime != null)
            {
                newDescription.Append($"\n**Expire time**: {this._banEntry.Action.EndTime}");
                if (this._banEntry.Action.EndTime < DateTime.Now)
                {
                    newDescription.Append(" *(Expired)*");
                }
            }
            description = newDescription.ToString();
        }
        
        // Build the footer.
        var footer = "No bans.";
        if (this._totalBans > 0)
        {
            footer = $"Entry {this._banIndex + 1}/{this._totalBans}";
        }
        
        // Build the embed.
        return new EmbedBuilder()
            .WithTitle($"{this._userProfileResponse.DisplayName} (@{this._userProfileResponse.Name}) [{this._robloxUserId}]")
            .WithDescription(description)
            .WithTimestamp(DateTimeOffset.Now)
            .WithFooter(new EmbedFooterBuilder().WithText(footer))
            .Build();
    }

    /// <summary>
    /// Returns the Discord message component for the message.
    /// </summary>
    /// <returns>The Discord message component for the message.</returns>
    public MessageComponent GetMessageComponent()
    {
        // Determine if the buttons are enabled.
        var previousButtonEnabled = true;
        var nextButtonEnabled = true;
        if (this._banEntry == null)
        {
            previousButtonEnabled = false;
            nextButtonEnabled = false;
        }
        else
        {
            if (this._banIndex <= 0)
            {
                previousButtonEnabled = false;
            }
            if (this._banIndex + 1 >= this._totalBans)
            {
                nextButtonEnabled = false;
            }
        }
        
        // Build the buttons.
        return new ComponentBuilder()
            .WithButton("Previous", customId: $"SovereignViewBan:{this._robloxUserId}:{this._banIndex - 1}", disabled: !previousButtonEnabled)
            .WithButton("Next", customId: $"SovereignViewBan:{this._robloxUserId}:{this._banIndex + 1}", disabled: !nextButtonEnabled)
            .Build();
    }
}
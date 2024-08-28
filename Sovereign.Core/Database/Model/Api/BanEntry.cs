using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Sovereign.Core.Model;

namespace Sovereign.Core.Database.Model.Api;

public class BanEntry
{
    /// <summary>
    /// Arbitrary key for the record.
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }
    
    /// <summary>
    /// Roblox account that is banned or unbanned.
    /// </summary>
    [Required]
    public long TargetRobloxUserId { get; set; }

    /// <summary>
    /// Domain (games + groups) the ban applies to.
    /// </summary>
    [Required]
    public string Domain { get; set; } = null!;
    
    /// <summary>
    /// Action that was performed on the user.
    /// </summary>
    [Required]
    public BanAction Action { get; set; }
    
    /// <summary>
    /// Start time of the action.
    /// </summary>
    [Required]
    public DateTime StartTime { get; set; }
    
    /// <summary>
    /// End time of the action, if it is a temporary ban.
    /// </summary>
    public DateTime? EndTime { get; set; }
    
    /// <summary>
    /// Roblox account that issued the ban or unban.
    /// </summary>
    [Required]
    public long ActingRobloxUserId { get; set; }

    /// <summary>
    /// Ban reason that is displayed to the user.
    /// </summary>
    [Required]
    public string DisplayReason { get; set; } = null!;

    /// <summary>
    /// Private reason that is used internally by staff.
    /// </summary>
    [Required]
    public string PrivateReason { get; set; } = null!;
}
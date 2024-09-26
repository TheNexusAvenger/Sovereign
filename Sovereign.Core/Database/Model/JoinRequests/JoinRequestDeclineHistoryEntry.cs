using System;
using System.ComponentModel.DataAnnotations;

namespace Sovereign.Core.Database.Model.JoinRequests;

public class JoinRequestDeclineHistoryEntry
{
    /// <summary>
    /// Arbitrary key for the record.
    /// </summary>
    [Key]
    public long Id { get; set; }
    
    /// <summary>
    /// Id of the ban that was handled.
    /// </summary>
    [Required]
    public long BanId { get; set; }

    /// <summary>
    /// Domain the ban was handled for.
    /// </summary>
    [Required]
    public string Domain { get; set; } = null!;
    
    /// <summary>
    /// Group id the ban was handled for.
    /// </summary>
    [Required]
    public long GroupId { get; set; }
    
    /// <summary>
    /// Roblox user id the ban was handled for.
    /// </summary>
    [Required]
    public long UserId { get; set; }
    
    /// <summary>
    /// Time the ban was handled.
    /// </summary>
    [Required]
    public DateTime Time { get; set; } = DateTime.Now;
}
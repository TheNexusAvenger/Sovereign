using System;
using System.ComponentModel.DataAnnotations;

namespace Sovereign.Core.Database.Model.Bans;

public class GameBansHistoryEntry
{
    /// <summary>
    /// Arbitrary key for the record.
    /// </summary>
    [Key]
    public long Id { get; set; }

    /// <summary>
    /// Domain the ban was handled for.
    /// </summary>
    [Required]
    public string Domain { get; set; } = null!;
    
    /// <summary>
    /// Game id the ban was handled for.
    /// </summary>
    [Required]
    public long GameId { get; set; }
    
    /// <summary>
    /// Time the ban was handled.
    /// </summary>
    [Required]
    public DateTime Time { get; set; } = DateTime.Now;
}
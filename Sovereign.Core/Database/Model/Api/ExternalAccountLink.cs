using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sovereign.Core.Database.Model.Api;

public class ExternalAccountLink
{
    /// <summary>
    /// Arbitrary key for the record.
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    /// <summary>
    /// Domain (games + groups) the link applies to.
    /// </summary>
    [Required]
    public string Domain { get; set; } = null!;
    
    /// <summary>
    /// Roblox account linked to the external account.
    /// </summary>
    [Required]
    public long RobloxUserId { get; set; }

    /// <summary>
    /// Method used to link the account.
    /// The method is defined by the client and used later for requests.
    /// </summary>
    [Required]
    public string LinkMethod { get; set; } = null!;

    /// <summary>
    /// Data used to link the account.
    /// The method is defined by the client and used later for requests.
    /// </summary>
    [Required]
    public string LinkData { get; set; } = null!;
}
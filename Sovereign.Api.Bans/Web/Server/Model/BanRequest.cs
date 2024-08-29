using System.Collections.Generic;
using Sovereign.Core.Model;

namespace Sovereign.Api.Bans.Web.Server.Model;

public class BanRequestAuthentication
{
    /// <summary>
    /// Method to authenticate the user.
    /// </summary>
    public string? Method { get; set; }
    
    /// <summary>
    /// Data to use to authenticate the Roblox user.
    /// When the Method is Roblox, Data should be the Roblox user id.
    /// </summary>
    public string? Data { get; set; }
}

public class BanRequestAction
{
    /// <summary>
    /// Type of the action to perform.
    /// </summary>
    public BanAction? Type { get; set; }
    
    /// <summary>
    /// User ids to perform the action on.
    /// </summary>
    public List<long>? UserIds { get; set; }
    
    /// <summary>
    /// Optional duration of the ban.
    /// </summary>
    public long? Duration { get; set; }
}

public class BanRequestReason
{
    /// <summary>
    /// Message that is displayed to the user.
    /// This is only displayed for bans, but is recorded in the database.
    /// </summary>
    public string? Display { get; set; }
    
    /// <summary>
    /// Message that is kept internally for the ban.
    /// This is only stored on Roblox for bans, but is recorded in the database.
    /// </summary>
    public string? Private { get; set; }
}

public class BanRequest
{
    /// <summary>
    /// Domain of the request.
    /// </summary>
    public string? Domain { get; set; }
    
    /// <summary>
    /// User authorization to perform the ban or unban.
    /// </summary>
    public BanRequestAuthentication? Authentication { get; set; }
    
    /// <summary>
    /// Action to perform.
    /// </summary>
    public BanRequestAction? Action { get; set; }
    
    /// <summary>
    /// Reason for the ban or unban.
    /// </summary>
    public BanRequestReason? Reason { get; set; }
}
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Sovereign.Core.Model;

namespace Sovereign.Api.Bans.Web.Server.Model;

public class BanRequestAuthentication
{
    /// <summary>
    /// Method to authenticate the user.
    /// </summary>
    [JsonPropertyName("method")]
    public string? Method { get; set; }
    
    /// <summary>
    /// Data to use to authenticate the Roblox user.
    /// When the Method is Roblox, Data should be the Roblox user id.
    /// </summary>
    [JsonPropertyName("data")]
    public string? Data { get; set; }
}

public class BanRequestAction
{
    /// <summary>
    /// Type of the action to perform.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    [JsonPropertyName("type")]
    public BanAction? Type { get; set; }
    
    /// <summary>
    /// User ids to perform the action on.
    /// </summary>
    [JsonPropertyName("userIds")]
    public List<long>? UserIds { get; set; }

    /// <summary>
    /// If true, alt accounts will not be banned or unbanned.
    /// Not all banning outputs support this.
    /// </summary>
    [JsonPropertyName("excludeAltAccounts")]
    public bool ExcludeAltAccounts { get; set; } = false;
    
    /// <summary>
    /// Optional duration of the ban.
    /// </summary>
    [JsonPropertyName("duration")]
    public long? Duration { get; set; }
}

public class BanRequestReason
{
    /// <summary>
    /// Message that is displayed to the user.
    /// This is only displayed for bans, but is recorded in the database.
    /// </summary>
    [JsonPropertyName("display")]
    public string? Display { get; set; }
    
    /// <summary>
    /// Message that is kept internally for the ban.
    /// This is only stored on Roblox for bans, but is recorded in the database.
    /// </summary>
    [JsonPropertyName("private")]
    public string? Private { get; set; }
}

public class BanRequest
{
    /// <summary>
    /// Domain of the request.
    /// </summary>
    [JsonPropertyName("domain")]
    public string? Domain { get; set; }
    
    /// <summary>
    /// User authorization to perform the ban or unban.
    /// </summary>
    [JsonPropertyName("authentication")]
    public BanRequestAuthentication? Authentication { get; set; }
    
    /// <summary>
    /// Action to perform.
    /// </summary>
    [JsonPropertyName("action")]
    public BanRequestAction? Action { get; set; }
    
    /// <summary>
    /// Reason for the ban or unban.
    /// </summary>
    [JsonPropertyName("reason")]
    public BanRequestReason? Reason { get; set; }
}

[JsonSerializable(typeof(BanRequest))]
[JsonSourceGenerationOptions(WriteIndented = true, IncludeFields = true)]
public partial class BanRequestJsonContext : JsonSerializerContext
{
}
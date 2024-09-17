using System;
using System.Text.Json.Serialization;
using Bouncer.Web.Client.Response;

namespace Sovereign.Core.Web.Client.Response;

public class GameJoinRestrictionResponse
{
    /// <summary>
    /// Whether the game join restriction (ban) is active.
    /// </summary>
    [JsonPropertyName("active")]
    public bool Active;
    
    /// <summary>
    /// Time that the user restriction was started
    /// </summary>
    [JsonPropertyName("startTime")]
    public DateTime StartTime;

    /// <summary>
    /// Optional duration of the ban in the given units.
    /// </summary>
    [JsonPropertyName("duration")]
    public string? Duration;

    /// <summary>
    /// Private reason to store with the ban (not shown to the user).
    /// Must not be more than 1,000 characters.
    /// </summary>
    [JsonPropertyName("privateReason")]
    public string PrivateReason = null!;

    /// <summary>
    /// Display reason to store with the ban (filtered and displayed the user).
    /// Must not be more than 400 characters.
    /// </summary>
    [JsonPropertyName("displayReason")]
    public string DisplayReason = null!;

    /// <summary>
    /// If true, alt accounts will not be banned or unbanned.
    /// </summary>
    [JsonPropertyName("excludeAltAccounts")]
    public bool ExcludeAltAccounts;

    /// <summary>
    /// Whether the resource was inherited.
    /// </summary>
    [JsonPropertyName("inherited")]
    public bool Inherited;
}

public class UserRestrictionResponse : BaseRobloxOpenCloudResponse
{
    /// <summary>
    /// Path of the resource to set.
    /// </summary>
    [JsonPropertyName("path")]
    public string Path = null!;
    
    /// <summary>
    /// Time that the user restriction was updated.
    /// </summary>
    [JsonPropertyName("updateTime")]
    public DateTime UpdateTime;
    
    /// <summary>
    /// User that was updated.
    /// </summary>
    [JsonPropertyName("user")]
    public string User = null!;

    /// <summary>
    /// Game join request restriction data.
    /// </summary>
    [JsonPropertyName("gameJoinRestriction")]
    public GameJoinRestrictionResponse GameJoinRestriction = null!;
}

[JsonSerializable(typeof(UserRestrictionResponse))]
[JsonSourceGenerationOptions(WriteIndented = true, IncludeFields = true)]
public partial class UserRestrictionResponseJsonContext : JsonSerializerContext
{
}
using System.Text.Json.Serialization;

namespace Sovereign.Core.Web.Client.Request;

public class GameJoinRestrictionRequest
{
    /// <summary>
    /// Whether the game join restriction (ban) is active.
    /// </summary>
    [JsonPropertyName("active")]
    public bool Active;

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
    public bool ExcludeAltAccounts = false;
}

public class UserRestrictionRequest
{
    /// <summary>
    /// Game join request restriction data.
    /// </summary>
    [JsonPropertyName("gameJoinRestriction")]
    public GameJoinRestrictionRequest GameJoinRestriction = new GameJoinRestrictionRequest();
}

[JsonSerializable(typeof(UserRestrictionRequest))]
[JsonSourceGenerationOptions(WriteIndented = true, IncludeFields = true, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
public partial class UserRestrictionRequestJsonContext : JsonSerializerContext
{
}
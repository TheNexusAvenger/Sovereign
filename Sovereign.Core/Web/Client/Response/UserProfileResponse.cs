using System.Text.Json.Serialization;

namespace Sovereign.Core.Web.Client.Response;

public class UserProfileResponse
{
    /// <summary>
    /// Username of the Roblox user.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = null!;
    
    /// <summary>
    /// Display name of the Roblox user.
    /// </summary>
    [JsonPropertyName("displayName")]
    public string DisplayName { get; set; } = null!;
    
    /// <summary>
    /// Description of the Roblox user.
    /// </summary>
    [JsonPropertyName("description")]
    public string Description { get; set; } = null!;
}

[JsonSerializable(typeof(UserProfileResponse))]
[JsonSourceGenerationOptions(WriteIndented = true, IncludeFields = true, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
public partial class UserProfileResponseJsonContext : JsonSerializerContext
{
}
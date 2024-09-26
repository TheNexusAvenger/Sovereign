using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace Sovereign.Core.Model.Response.Api;

public enum BanPermissionIssue
{
    InvalidAccountLink,
    MalformedRobloxId,
    Forbidden,
}

public class BanPermissionResponse : BaseResponse
{
    /// <summary>
    /// Whether the user can ban.
    /// </summary>
    [JsonPropertyName("canBan")]
    public bool CanBan { get; set; } = true;
    
    /// <summary>
    /// If provided, the issue with the ban permisssion.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter<BanPermissionIssue>))]
    [JsonPropertyName("banPermissionIssue")]
    public BanPermissionIssue? BanPermissionIssue { get; set; }
    
    /// <summary>
    /// Returns the JSON type information of the response.
    /// </summary>
    /// <returns>The JSON type information of the response.</returns>
    public override JsonTypeInfo GetJsonTypeInfo()
    {
        return BanPermissionResponseJsonContext.Default.BanPermissionResponse;
    }
}

[JsonSerializable(typeof(BanPermissionResponse))]
[JsonSourceGenerationOptions(WriteIndented = true, IncludeFields = true)]
public partial class BanPermissionResponseJsonContext : JsonSerializerContext
{
}
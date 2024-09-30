using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace Sovereign.Core.Model.Response.Api;

public class BanResponse : BaseResponse
{
    /// <summary>
    /// List of users that were banned.
    /// </summary>
    [JsonPropertyName("bannedUserIds")]
    public List<long> BannedUserIds { get; set; } = null!;
    
    /// <summary>
    /// List of users that were unbanned.
    /// </summary>
    [JsonPropertyName("unbannedUserIds")]
    public List<long> UnbannedUserIds { get; set; } = null!;
    
    /// <summary>
    /// Returns the JSON type information of the response.
    /// </summary>
    /// <returns>The JSON type information of the response.</returns>
    public override JsonTypeInfo GetJsonTypeInfo()
    {
        return BanResponseJsonContext.Default.BanResponse;
    }
}

[JsonSerializable(typeof(BanResponse))]
[JsonSourceGenerationOptions(WriteIndented = true, IncludeFields = true, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
public partial class BanResponseJsonContext : JsonSerializerContext
{
}
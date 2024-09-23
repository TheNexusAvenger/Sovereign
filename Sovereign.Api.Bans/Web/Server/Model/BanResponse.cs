using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Sovereign.Core.Model.Response;

namespace Sovereign.Api.Bans.Web.Server.Model;

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
[JsonSourceGenerationOptions(WriteIndented = true, IncludeFields = true)]
public partial class BanResponseJsonContext : JsonSerializerContext
{
}
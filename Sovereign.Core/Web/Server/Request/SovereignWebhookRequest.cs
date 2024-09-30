using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Sovereign.Core.Web.Server.Request;

public class SovereignWebhookRequest
{
    /// <summary>
    /// Domain of bans to handle.
    /// </summary>
    [JsonPropertyName("domain")]
    public string? Domain { get; set; }
    
    /// <summary>
    /// Ids of the ban records to handle.
    /// </summary>
    [JsonPropertyName("ids")]
    public List<long>? Ids { get; set; }
}

[JsonSerializable(typeof(SovereignWebhookRequest))]
[JsonSourceGenerationOptions(WriteIndented = true, IncludeFields = true, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
public partial class SovereignWebhookRequestJsonContext : JsonSerializerContext
{
}
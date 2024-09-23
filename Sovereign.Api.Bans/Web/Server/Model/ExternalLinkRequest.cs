using System.Text.Json.Serialization;

namespace Sovereign.Api.Bans.Web.Server.Model;

public class ExternalLinkRequest
{
    /// <summary>
    /// Domain of the request.
    /// </summary>
    [JsonPropertyName("domain")]
    public string? Domain { get; set; }
    
    /// <summary>
    /// Roblox user id to link.
    /// </summary>
    [JsonPropertyName("robloxUserId")]
    public long? RobloxUserId { get; set; }

    /// <summary>
    /// Method used to link the account.
    /// </summary>
    [JsonPropertyName("linkMethod")]
    public string? LinkMethod { get; set; }

    /// <summary>
    /// Data used to link the account.
    /// </summary>
    [JsonPropertyName("linkData")]
    public string? LinkData { get; set; }
}

[JsonSerializable(typeof(ExternalLinkRequest))]
[JsonSourceGenerationOptions(WriteIndented = true, IncludeFields = true)]
public partial class ExternalLinkRequestJsonContext : JsonSerializerContext
{
}
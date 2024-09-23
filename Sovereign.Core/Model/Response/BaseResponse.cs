using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace Sovereign.Core.Model.Response;

public abstract class BaseResponse
{
    /// <summary>
    /// Status of the response.
    /// </summary>
    [JsonPropertyName("status")]
    public string Status { get; set; } = "Success";

    /// <summary>
    /// Returns the JSON type information of the response.
    /// </summary>
    /// <returns>The JSON type information of the response.</returns>
    public abstract JsonTypeInfo GetJsonTypeInfo();
}
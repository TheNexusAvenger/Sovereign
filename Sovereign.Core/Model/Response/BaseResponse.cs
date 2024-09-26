using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace Sovereign.Core.Model.Response;

public enum ResponseStatus
{
    // Generic success response.
    Success,
    
    // Generic client error response.
    MalformedRequest,
    ValidationError,
    Unauthorized,
    Forbidden,
    
    // Generic server error response.
    ServerProcessingError,
    
    // Client permission error response.
    GroupRankPermissionError,
    
    // Server configuration error response.
    ServerConfigurationError,
}

public abstract class BaseResponse
{
    /// <summary>
    /// Status of the response.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter<ResponseStatus>))]
    [JsonPropertyName("status")]
    public ResponseStatus Status { get; set; } = ResponseStatus.Success;

    /// <summary>
    /// Returns the JSON type information of the response.
    /// </summary>
    /// <returns>The JSON type information of the response.</returns>
    public abstract JsonTypeInfo GetJsonTypeInfo();
}
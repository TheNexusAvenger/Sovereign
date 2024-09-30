using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace Sovereign.Core.Model.Response;

public class SimpleResponse : BaseResponse
{
    /// <summary>
    /// Response for a successful request.
    /// </summary>
    public static readonly JsonResponse SuccessResponse = new JsonResponse(new SimpleResponse(ResponseStatus.Success), 200);
    
    /// <summary>
    /// Response for a malformed request.
    /// </summary>
    public static readonly JsonResponse MalformedRequestResponse = new JsonResponse(new SimpleResponse(ResponseStatus.MalformedRequest), 400);
    
    /// <summary>
    /// Response for an unauthorized response.
    /// Used for when a user can be confirmed.
    /// </summary>
    public static readonly JsonResponse UnauthorizedResponse = new JsonResponse(new SimpleResponse(ResponseStatus.Unauthorized), 401);
    
    /// <summary>
    /// Response for a forbidden response.
    /// Used for when a user can be confirmed but doesn't have access.
    /// </summary>
    public static readonly JsonResponse ForbiddenResponse = new JsonResponse(new SimpleResponse(ResponseStatus.Forbidden), 403);
    
    /// <summary>
    /// Creates a simple response.
    /// </summary>
    /// <param name="status">Status of the response.</param>
    public SimpleResponse(ResponseStatus status)
    {
        this.Status = status;
    }
    
    /// <summary>
    /// Returns the JSON type information of the response.
    /// </summary>
    /// <returns>The JSON type information of the response.</returns>
    public override JsonTypeInfo GetJsonTypeInfo()
    {
        return SimpleResponseJsonContext.Default.SimpleResponse;
    }
}

[JsonSerializable(typeof(SimpleResponse))]
[JsonSourceGenerationOptions(WriteIndented = true, IncludeFields = true, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
public partial class SimpleResponseJsonContext : JsonSerializerContext
{
}
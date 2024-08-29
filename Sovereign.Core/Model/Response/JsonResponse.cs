using Microsoft.AspNetCore.Http;

namespace Sovereign.Core.Model.Response;

public class JsonResponse
{
    /// <summary>
    /// Response body to return.
    /// </summary>
    public readonly BaseResponse Response;
    
    /// <summary>
    /// Status code to return.
    /// </summary>
    public readonly int StatusCode;

    /// <summary>
    /// Creates a JSON response.
    /// </summary>
    /// <param name="response">Response body to return.</param>
    /// <param name="statusCode">Status code to return for the response.</param>
    public JsonResponse(BaseResponse response, int statusCode)
    {
        this.Response = response;
        this.StatusCode = statusCode;
    }

    /// <summary>
    /// Returns the HTTP response for the JSON.
    /// </summary>
    /// <returns>The HTTP response for the JSON.</returns>
    public IResult GetResponse()
    {
        return Results.Json(this.Response, jsonTypeInfo: this.Response.GetJsonTypeInfo(), statusCode: this.StatusCode);
    }
}
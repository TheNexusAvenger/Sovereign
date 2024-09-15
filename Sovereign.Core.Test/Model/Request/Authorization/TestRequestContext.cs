using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Sovereign.Core.Model.Request.Authorization;

namespace Sovereign.Core.Test.Model.Request.Authorization;

public class TestRequestContext : BaseRequestContext
{
    /// <summary>
    /// Whether to return if the request was authorized or not.
    /// </summary>
    public bool Authorized { get; set; } = true;

    /// <summary>
    /// Creates a base request context.
    /// </summary>
    /// <param name="httpContext">HttpContext to read the request from.</param>
    public TestRequestContext(HttpContext httpContext) : base(httpContext)
    {
        
    }

    /// <summary>
    /// Creates a test request context from a request body.
    /// </summary>
    /// <param name="data">Data object to convert to JSON.</param>
    /// <param name="jsonTypeInfo">JSON type information for the request.</param>
    /// <typeparam name="T">Type of the request body.</typeparam>
    /// <returns>Test request context to use with tests.</returns>
    public static TestRequestContext FromRequest<T>(T data, JsonTypeInfo<T> jsonTypeInfo)
    {
        return new TestRequestContext(new DefaultHttpContext()
        {
            Request = {
                Body = new MemoryStream(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(data, jsonTypeInfo))),
            },
        });
    }

    /// <summary>
    /// Creates a test request context from a query string.
    /// </summary>
    /// <param name="queryString">Query string to use with the requests.</param>
    /// <returns>Test request context to use with tests.</returns>
    public static TestRequestContext FromQuery(string queryString)
    {
        return new TestRequestContext(new DefaultHttpContext()
        {
            Request = {
                QueryString = new QueryString(queryString),
                Query = new QueryCollection(QueryHelpers.ParseQuery(queryString)),
            },
        });
    }

    /// <summary>
    /// Returns if the request is authorized.
    /// </summary>
    /// <param name="apiKeys">List of API keys that are valid.</param>
    /// <param name="signatureSecrets">List of request signature secrets that are valid.</param>
    /// <returns>Whether the request is authorized or not.</returns>
    public override bool IsAuthorized(List<string>? apiKeys, List<string>? signatureSecrets)
    {
        return this.Authorized;
    }
}
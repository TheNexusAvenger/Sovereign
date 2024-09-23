using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Microsoft.AspNetCore.Http;

namespace Sovereign.Core.Model.Request.Authorization;

public abstract class BaseRequestContext
{
    /// <summary>
    /// Authorization header from the request.
    /// </summary>
    public readonly string? Authorization;
    
    /// <summary>
    /// Contents of the request body.
    /// </summary>
    public readonly string RequestBody;

    /// <summary>
    /// Raw query parameters part of the request.
    /// </summary>
    public readonly string RawQuery;

    /// <summary>
    /// Query parameters part of the request.
    /// </summary>
    public readonly IQueryCollection QueryParameters;

    /// <summary>
    /// Creates a base request context.
    /// </summary>
    /// <param name="httpContext">HttpContext to read the request from.</param>
    public BaseRequestContext(HttpContext httpContext)
    {
        this.Authorization = httpContext.Request.Headers.Authorization.FirstOrDefault();
        this.RequestBody = new StreamReader(httpContext.Request.Body).ReadToEndAsync().Result;
        this.RawQuery = httpContext.Request.QueryString.ToString();
        this.QueryParameters = httpContext.Request.Query;
    }

    /// <summary>
    /// Returns the request object from the body.
    /// </summary>
    /// <param name="jsonTypeInfo">JSON type information of the request.</param>
    /// <typeparam name="T">Type of the request to parse.</typeparam>
    /// <returns>Request body, if it could be parsed.</returns>
    public T? GetRequest<T>(JsonTypeInfo<T> jsonTypeInfo)
    {
        try
        {
            return JsonSerializer.Deserialize(this.RequestBody, jsonTypeInfo);
        }
        catch (Exception)
        {
            // JSON is malformed in this case.
            return default;
        }
    }

    /// <summary>
    /// Returns if the request is authorized.
    /// </summary>
    /// <param name="apiKeys">List of API keys that are valid.</param>
    /// <param name="signatureSecrets">List of request signature secrets that are valid.</param>
    /// <returns>Whether the request is authorized or not.</returns>
    public abstract bool IsAuthorized(List<string>? apiKeys, List<string>? signatureSecrets);
}
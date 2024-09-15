using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Sovereign.Core.Model.Request.Authorization;
using Sovereign.Core.Model.Response;

namespace Sovereign.Core.Extension;

public static class WebApplicationExtensions
{
    /// <summary>
    /// Maps a GET request for the web application.
    /// </summary>
    /// <param name="this">WebApplication to add the route to.</param>
    /// <param name="pattern">Pattern for the URL to map.</param>
    /// <param name="requestDelegate">Request handler, which takes in a RequestContext and returns a JsonResponse.</param>
    public static void MapGetWithContext(this WebApplication @this, string pattern, Func<RequestContext, Task<JsonResponse>> requestDelegate)
    {
        @this.MapGet(pattern, async (httpContext) =>
        {
            var requestContext = new RequestContext(httpContext, RequestAuthorizationSource.Query);
            var response = await requestDelegate(requestContext);
            await response.GetResponse().ExecuteAsync(httpContext);
        });
    }
    
    /// <summary>
    /// Maps a POST request for the web application.
    /// </summary>
    /// <param name="this">WebApplication to add the route to.</param>
    /// <param name="pattern">Pattern for the URL to map.</param>
    /// <param name="requestDelegate">Request handler, which takes in a RequestContext and returns a JsonResponse.</param>
    public static void MapPostWithContext(this WebApplication @this, string pattern, Func<RequestContext, Task<JsonResponse>> requestDelegate)
    {
        @this.MapPost(pattern, async (httpContext) =>
        {
            var requestContext = new RequestContext(httpContext);
            var response = await requestDelegate(requestContext);
            await response.GetResponse().ExecuteAsync(httpContext);
        });
    }
}
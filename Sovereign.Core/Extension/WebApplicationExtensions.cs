using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Bouncer.Diagnostic;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Sovereign.Core.Model.Request.Authorization;
using Sovereign.Core.Model.Response;
using Sovereign.Core.Web.Server.Request;

namespace Sovereign.Core.Extension;

public static class WebApplicationExtensions
{
    /// <summary>
    /// Maps a GET request for the web application.
    /// </summary>
    /// <param name="this">WebApplication to add the route to.</param>
    /// <param name="pattern">Pattern for the URL to map.</param>
    /// <param name="requestDelegate">Request handler, which takes in a RequestContext and returns a JsonResponse.</param>
    public static void MapGetWithContext(this IEndpointRouteBuilder @this, string pattern, Func<RequestContext, Task<JsonResponse>> requestDelegate)
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
    public static void MapPostWithContext(this IEndpointRouteBuilder @this, string pattern, Func<RequestContext, Task<JsonResponse>> requestDelegate)
    {
        @this.MapPost(pattern, async (httpContext) =>
        {
            var requestContext = new RequestContext(httpContext);
            var response = await requestDelegate(requestContext);
            await response.GetResponse().ExecuteAsync(httpContext);
        });
    }
    
    /// <summary>
    /// Maps a Sovereign internal webhook for the web application.
    /// </summary>
    /// <param name="this">WebApplication to add the route to.</param>
    /// <param name="requestDelegate">Request handler, which takes in a webhook body and RequestContext and returns a JsonResponse.</param>
    public static void MapInternalWebhook(this IEndpointRouteBuilder @this, Func<SovereignWebhookRequest, RequestContext, Task<JsonResponse>> requestDelegate)
    {
        @this.MapPostWithContext("/sovereign/webhook", async (requestContext) =>
        {
            // Return if the request could not be parsed.
            var request = requestContext.GetRequest(SovereignWebhookRequestJsonContext.Default.SovereignWebhookRequest);
            if (request == null)
            {
                Logger.Trace("Ignoring request to POST /sovereign/webhook due to unreadable JSON.");
                return SimpleResponse.MalformedRequestResponse;
            }
        
            // Return if the authorization header is invalid.
            var internalWebhookSecretKey = Environment.GetEnvironmentVariable("INTERNAL_WEBHOOK_SECRET_KEY");
            if (internalWebhookSecretKey == null)
            {
                Logger.Warn("INTERNAL_WEBHOOK_SECRET_KEY is unset. Webhooks can't be authenticated.");
                return SimpleResponse.UnauthorizedResponse;
            }
            if (!requestContext.IsAuthorized(null, new List<string>() { internalWebhookSecretKey }))
            {
                Logger.Trace("Ignoring request to POST /sovereign/webhook due to an invalid authorization header.");
                return SimpleResponse.UnauthorizedResponse;
            }
            
            // Perform the webhook.
            return await requestDelegate(request, requestContext);
        });
    }
}
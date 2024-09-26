using System.Threading.Tasks;
using Bouncer.Web.Server;
using Bouncer.Web.Server.Model;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Sovereign.Bans.JoinRequests.Loop;
using Sovereign.Bans.JoinRequests.Web.Server.Model;
using Sovereign.Core.Extension;
using Sovereign.Core.Web.Client.Request;

namespace Sovereign.Bans.JoinRequests.Web;

public class JoinRequestsWebServer : WebServer
{
    /// <summary>
    /// Creates the games web server.
    /// </summary>
    public JoinRequestsWebServer()
    {
        this.Port = 9001;
    }
    
    /// <summary>
    /// Starts the web server.
    /// </summary>
    /// <param name="joinRequestBanLoopCollection">Join request bans request loop collection to include in the health check.</param>
    public async Task StartAsync(JoinRequestBanLoopCollection joinRequestBanLoopCollection)
    {
        await this.StartAsync((builder) =>
        {
            // Add the JSON serializers.
            builder.Services.ConfigureHttpJsonOptions(options => {
                options.SerializerOptions.TypeInfoResolverChain.Add(JoinRequestBansHealthCheckResultJsonContext.Default);
                options.SerializerOptions.TypeInfoResolverChain.Add(UserRestrictionRequestJsonContext.Default);
            });
        }, (app) =>
        {
            // Add the health endpoint.
            app.MapGet("/health", async (httpContext) =>
            {
                var healthCheckResult = JoinRequestBansHealthCheckResult.FromLoopHealthResults(joinRequestBanLoopCollection.GetStatus());
                var statusCode = (healthCheckResult.Status == HealthCheckResultStatus.Up ? 200 : 503);
                var response = Results.Json(healthCheckResult, statusCode: statusCode, jsonTypeInfo: JoinRequestBansHealthCheckResultJsonContext.Default.JoinRequestBansHealthCheckResult);
                await response.ExecuteAsync(httpContext);
            });
            
            // Add the webhook endpoint.
            app.MapInternalWebhook(async (request, _) =>
                await joinRequestBanLoopCollection.HandleWebhookAsync(request));
        });
    }
}
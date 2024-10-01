using System.Threading.Tasks;
using Bouncer.Web.Server;
using Bouncer.Web.Server.Model;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Sovereign.Bans.Games.Loop;
using Sovereign.Bans.Games.Web.Server.Model;
using Sovereign.Core.Extension;
using Sovereign.Core.Web.Client.Request;

namespace Sovereign.Bans.Games.Web.Server;

public class GamesWebServer : WebServer
{
    /// <summary>
    /// Creates the game bans web server.
    /// </summary>
    public GamesWebServer()
    {
        this.Host = "*";
        this.Port = 9000;
    }
    
    /// <summary>
    /// Starts the web server.
    /// </summary>
    /// <param name="gameBanLoopCollection">Game bans request loop collection to include in the health check.</param>
    public async Task StartAsync(GameBanLoopCollection gameBanLoopCollection)
    {
        await this.StartAsync((builder) =>
        {
            // Add the JSON serializers.
            builder.Services.ConfigureHttpJsonOptions(options => {
                options.SerializerOptions.TypeInfoResolverChain.Add(GameBansHealthCheckResultJsonContext.Default);
                options.SerializerOptions.TypeInfoResolverChain.Add(UserRestrictionRequestJsonContext.Default);
            });
        }, (app) =>
        {
            // Add the health endpoint.
            app.MapGet("/health", async (httpContext) =>
            {
                var healthCheckResult = await gameBanLoopCollection.GetStatusAsync();
                var statusCode = (healthCheckResult.Status == HealthCheckResultStatus.Up ? 200 : 503);
                var response = Results.Json(healthCheckResult, statusCode: statusCode, jsonTypeInfo: GameBansHealthCheckResultJsonContext.Default.GameBansHealthCheckResult);
                await response.ExecuteAsync(httpContext);
            });
            
            // Add the webhook endpoint.
            app.MapInternalWebhook(async (request, _) =>
                await gameBanLoopCollection.HandleWebhookAsync(request));
        });
    }
}
using System.Threading.Tasks;
using Bouncer.Web.Server;
using Bouncer.Web.Server.Model;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Sovereign.Discord.Discord;
using Sovereign.Discord.Web.Server.Model;

namespace Sovereign.Discord.Web.Server;

public class DiscordWebServer : WebServer
{
    /// <summary>
    /// Creates the Discord bot server.
    /// </summary>
    public DiscordWebServer()
    {
        this.Port = 7000;
    }
    
    /// <summary>
    /// Starts the web server.
    /// </summary>
    /// <param name="discordBot">Discord bot to perform health checks with.</param>
    public async Task StartAsync(Bot discordBot)
    {
        await this.StartAsync((builder) =>
        {
            // Add the JSON serializers.
            builder.Services.ConfigureHttpJsonOptions(options => {
                options.SerializerOptions.TypeInfoResolverChain.Add(DiscordHealthCheckResultJsonContext.Default);
            });
        }, (app) =>
        {
            // Add the health endpoint.
            app.MapGet("/health", async (httpContext) =>
            {
                var healthCheckResult = await discordBot.PerformHealthCheckAsync();
                var statusCode = (healthCheckResult.Status == HealthCheckResultStatus.Up ? 200 : 503);
                var response = Results.Json(healthCheckResult, statusCode: statusCode, jsonTypeInfo: DiscordHealthCheckResultJsonContext.Default.DiscordHealthCheckResult);
                await response.ExecuteAsync(httpContext);
            });
        });
    }
}
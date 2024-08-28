using System.Threading.Tasks;
using Bouncer.Web.Server;
using Bouncer.Web.Server.Model;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Sovereign.Api.Bans.Web.Server.Model;

namespace Sovereign.Api.Bans.Web.Server;

public class BansWebServer : WebServer
{
    /// <summary>
    /// Starts the web server.
    /// </summary>
    public async Task StartAsync()
    {
        var healthCheckState = new BansHealthCheckState();
        healthCheckState.ConnectConfigurationChanges();
        await this.StartAsync((builder) =>
        {
            // Add the JSON serializers.
            builder.Services.ConfigureHttpJsonOptions(options => {
                options.SerializerOptions.TypeInfoResolverChain.Insert(0, BansHealthCheckResultJsonContext.Default);
            });
        }, (app) =>
        {
            // Add the health endpoint.
            app.MapGet("/health", () =>
            {
                var healthCheckResult = healthCheckState.GetHealthCheckResult();
                var statusCode = (healthCheckResult.Status == HealthCheckResultStatus.Up ? 200 : 503);
                return Results.Json(healthCheckResult, statusCode: statusCode);
            });
            
            // Add the ban endpoints.
            // TODO
        });
    }
}
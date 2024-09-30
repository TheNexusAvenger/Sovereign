using System.Threading.Tasks;
using Bouncer.Web.Server;
using Bouncer.Web.Server.Model;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Sovereign.Api.Bans.Web.Server.Controller;
using Sovereign.Api.Bans.Web.Server.Model;
using Sovereign.Core.Extension;

namespace Sovereign.Api.Bans.Web.Server;

public class BansWebServer : WebServer
{
    /// <summary>
    /// Creates the bans web server.
    /// </summary>
    public BansWebServer()
    {
        this.Host = "*";
    }
    
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
                options.SerializerOptions.TypeInfoResolverChain.Add(BansHealthCheckResultJsonContext.Default);
            });
        }, (app) =>
        {
            // Add the health endpoint.
            app.MapGet("/health", async (httpContext) =>
            {
                var healthCheckResult = healthCheckState.GetHealthCheckResult();
                var statusCode = (healthCheckResult.Status == HealthCheckResultStatus.Up ? 200 : 503);
                var response = Results.Json(healthCheckResult, statusCode: statusCode, jsonTypeInfo: BansHealthCheckResultJsonContext.Default.BansHealthCheckResult);
                await response.ExecuteAsync(httpContext);
            });
            
            // Add the ban endpoints.
            var banController = new BanController();
            app.MapPostWithContext("/bans/ban", async (requestContext) =>
                await banController.HandleBanRequest(requestContext));
            app.MapGetWithContext("/bans/list", async (requestContext) =>
                await banController.HandleListBansRequest(requestContext));
            app.MapGetWithContext("/bans/permissions", async (requestContext) =>
                await banController.HandleGetPermissionsRequest(requestContext));
            
            // Add the account endpoints.
            var accountLinkController = new AccountLinkController();
            app.MapPostWithContext("/accounts/link", async (requestContext) =>
                await accountLinkController.HandleExternalLinkRequest(requestContext));
        });
    }
}
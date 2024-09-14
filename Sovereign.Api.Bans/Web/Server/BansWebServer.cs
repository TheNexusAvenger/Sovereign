using System.Threading.Tasks;
using Bouncer.Web.Server;
using Bouncer.Web.Server.Model;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Sovereign.Api.Bans.Web.Server.Controller;
using Sovereign.Api.Bans.Web.Server.Model;
using Sovereign.Core.Model.Request.Authorization;

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
            app.MapGet("/health", async (httpContext) =>
            {
                var healthCheckResult = healthCheckState.GetHealthCheckResult();
                var statusCode = (healthCheckResult.Status == HealthCheckResultStatus.Up ? 200 : 503);
                var response = Results.Json(healthCheckResult, statusCode: statusCode, jsonTypeInfo: BansHealthCheckResultJsonContext.Default.HealthCheckResultStatus);
                await response.ExecuteAsync(httpContext);
            });
            
            // Add the ban endpoints.
            var banController = new BanController();
            app.MapPost("/bans/ban", async (httpContext) =>
            {
                var requestContext = new RequestContext(httpContext);
                var response = await banController.HandleBanRequest(requestContext);
                await response.GetResponse().ExecuteAsync(httpContext);
            });
            app.MapGet("/bans/list", async (httpContext) =>
            {
                var requestContext = new RequestContext(httpContext);
                var response = await banController.HandleListBansRequest(requestContext);
                await response.GetResponse().ExecuteAsync(httpContext);
            });
            
            // Add the account endpoints.
            var accountLinkController = new AccountLinkController();
            app.MapPost("/accounts/link", async (httpContext) =>
            {
                var requestContext = new RequestContext(httpContext);
                var response = await accountLinkController.HandleExternalLinkRequest(requestContext);
                await response.GetResponse().ExecuteAsync(httpContext);
            });
        });
    }
}
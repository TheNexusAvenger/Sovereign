using System.Threading.Tasks;
using Bouncer.Web.Server;

namespace Sovereign.Bans.Games.Web.Server;

public class GamesWebServer : WebServer
{
    /// <summary>
    /// Creates the games web server.
    /// </summary>
    public GamesWebServer()
    {
        this.Port = 9000;
    }
    
    /// <summary>
    /// Starts the web server.
    /// </summary>
    public async Task StartAsync()
    {
        await this.StartAsync((builder) =>
        {
            // Add the JSON serializers.
            // TODO: Add health check response.
        }, (app) =>
        {
            // Add the health endpoint.
            // TODO: Add health check route.
        });
    }
}
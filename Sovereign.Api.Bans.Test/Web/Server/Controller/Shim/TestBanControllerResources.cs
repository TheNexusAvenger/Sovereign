using System.IO;
using Bouncer.Test.Web.Client.Shim;
using Bouncer.Web.Client;
using Sovereign.Api.Bans.Configuration;
using Sovereign.Api.Bans.Web.Server.Controller.Shim;
using Sovereign.Core.Database;

namespace Sovereign.Api.Bans.Test.Web.Server.Controller.Shim;

public class TestBanControllerResources : IBanControllerResources
{
    /// <summary>
    /// Configuration to return for the tests.
    /// </summary>
    public BansConfiguration Configuration { get; set; } = new BansConfiguration();
    
    /// <summary>
    /// HTTP client used for tests.
    /// </summary>
    public TestHttpClient TestHttpClient { get; set; } = new TestHttpClient();

    /// <summary>
    /// Path to create a test database.
    /// </summary>
    private readonly string _databasePath = Path.GetTempFileName();
    
    /// <summary>
    /// Returns the current bans configuration.
    /// </summary>
    /// <returns>The current bans configuration.</returns>
    public BansConfiguration GetConfiguration()
    {
        return Configuration;
    }

    /// <summary>
    /// Returns the client used to for group rank checks.
    /// </summary>
    /// <returns>The client used to for group rank checks.</returns>
    public RobloxGroupClient GetRobloxGroupClient()
    {
        return new RobloxGroupClient(this.TestHttpClient, this.TestHttpClient);
    }
    
    /// <summary>
    /// Returns the database context to use.
    /// </summary>
    /// <returns>The database context to use.</returns>
    public BansContext GetBansContext()
    {
        var context = new BansContext(this._databasePath);
        context.MigrateAsync().Wait();
        return context;
    }
}
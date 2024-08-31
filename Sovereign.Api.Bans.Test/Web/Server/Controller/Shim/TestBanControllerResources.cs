using System.IO;
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
    /// Context to assign bans.
    /// </summary>
    public BansContext Context { get; set; } = new BansContext(Path.GetTempFileName());
    
    /// <summary>
    /// Returns the current bans configuration.
    /// </summary>
    /// <returns>The current bans configuration.</returns>
    public BansConfiguration GetConfiguration()
    {
        return Configuration;
    }
    
    /// <summary>
    /// Returns the database context to use.
    /// </summary>
    /// <returns>The database context to use.</returns>
    public BansContext GetBansContext()
    {
        this.Context.MigrateAsync().Wait();
        return this.Context;
    }
}
using Bouncer.Web.Client;
using Sovereign.Api.Bans.Configuration;
using Sovereign.Core.Database;

namespace Sovereign.Api.Bans.Web.Server.Controller.Shim;

public interface IBanControllerResources
{
    /// <summary>
    /// Returns the current bans configuration.
    /// </summary>
    /// <returns>The current bans configuration.</returns>
    public BansConfiguration GetConfiguration();
    
    /// <summary>
    /// Returns the client used to for group rank checks.
    /// </summary>
    /// <returns>The client used to for group rank checks.</returns>
    public RobloxGroupClient GetRobloxGroupClient();
    
    /// <summary>
    /// Returns the database context to use.
    /// </summary>
    /// <returns>The database context to use.</returns>
    public BansContext GetBansContext();
}
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bouncer.Diagnostic;
using Sovereign.Api.Bans.Web.Server.Controller.Shim;
using Sovereign.Api.Bans.Web.Server.Model;
using Sovereign.Core.Database.Model.Api;
using Sovereign.Core.Model.Request.Authorization;
using Sovereign.Core.Model.Response;

namespace Sovereign.Api.Bans.Web.Server.Controller;

public class AccountLinkController
{
    /// <summary>
    /// Resources for the controller.
    /// Meant to be replaced during unit tests.
    /// </summary>
    public IBanControllerResources ControllerResources { get; set; }= new BanControllerResources();

    /// <summary>
    /// Handles an external link request.
    /// </summary>
    public async Task<JsonResponse> HandleExternalLinkRequest(BaseRequestContext requestContext)
    {
        // Return if the request could not be parsed.
        var request = requestContext.GetRequest(ExternalLinkRequestJsonContext.Default.ExternalLinkRequest);
        if (request == null)
        {
            Logger.Trace("Ignoring request to POST /accounts/link due to unreadable JSON.");
            return SimpleResponse.MalformedRequestResponse;
        }
        
        // Return a validation error.
        var validationError = ValidationErrorResponse.GetValidationErrorResponse(new List<ValidationErrorCheck>()
        {
            new ValidationErrorCheck()
            {
                Path = "domain",
                Message = "domain was not provided.",
                IsValid = () => (request.Domain != null),
            },
            new ValidationErrorCheck()
            {
                Path = "robloxUserId",
                Message = "robloxUserId was not provided.",
                IsValid = () => (request.RobloxUserId != null),
            },
            new ValidationErrorCheck()
            {
                Path = "linkMethod",
                Message = "linkMethod was not provided.",
                IsValid = () => (request.LinkMethod != null),
            },
            new ValidationErrorCheck()
            {
                Path = "linkData",
                Message = "linkData was not provided.",
                IsValid = () => (request.LinkData != null),
            },
        });
        if (validationError != null)
        {
            Logger.Trace($"Ignoring request to POST /accounts/link due {validationError.Errors.Count} validation errors.");
            return new JsonResponse(validationError, 400);
        }
        
        // Get the domain for the action.
        var configuration = this.ControllerResources.GetConfiguration();
        var domain = request.Domain!.ToLower();
        var domains = configuration.Domains;
        if (domains == null)
        {
            Logger.Error("Domains was not configured in the configuration.");
            return new JsonResponse(new SimpleResponse("ServerConfigurationError"), 503);
        }
        var domainData = domains.FirstOrDefault(domainData => domainData.Name != null && domainData.Name.ToLower() == domain);
        if (domainData == null)
        {
            Logger.Trace("Ignoring request to POST /accounts/link due an invalid domain.");
            return SimpleResponse.UnauthorizedResponse;
        }
        
        // Return 401 if the authorization header was invalid for the request.
        if (!requestContext.IsAuthorized(domainData.ApiKeys, domainData.SecretKeys))
        {
            Logger.Trace("Ignoring request to POST /accounts/link due to an invalid or missing authorization header.");
            return SimpleResponse.UnauthorizedResponse;
        }
        
        // Add or update the account link.
        var robloxUserId = request.RobloxUserId!.Value;
        await using var bansContext = this.ControllerResources.GetBansContext();
        var existingBansContext = bansContext.ExternalAccountLinks.FirstOrDefault(link =>
            link.Domain.ToLower() == domain && link.RobloxUserId == robloxUserId &&
            link.LinkMethod.ToLower() == request.LinkMethod!.ToLower());
        if (existingBansContext != null)
        {
            // Update the link data.
            Logger.Info($"Updating external account link in domain {domain} for user {robloxUserId} with method {request.LinkMethod} to \"{request.LinkData}\"");
            existingBansContext.LinkData = request.LinkData!;
        }
        else
        {
            // Verify the user can create an external account link.
            // This is only done for new links to allow unauthorized used to clear or change their link data.
            var authorizationError = domainData.IsRobloxUserAuthorized(robloxUserId);
            if (authorizationError != null)
            {
                Logger.Trace($"Ignoring request to POST /accounts/link in domain {domain} due to user {robloxUserId} not be authorized for the domain.");
                return authorizationError;
            }
            
            // Add the link.
            Logger.Info($"Adding external account link in domain {domain} for user {robloxUserId} with method {request.LinkMethod} to \"{request.LinkData}\"");
            bansContext.ExternalAccountLinks.Add(new ExternalAccountLink()
            {
                Domain = domainData.Name!,
                RobloxUserId = robloxUserId,
                LinkMethod = request.LinkMethod!,
                LinkData = request.LinkData!,
            });
        }
        await bansContext.SaveChangesAsync();
        
        // Return success.
        return SimpleResponse.SuccessResponse;
    }
}
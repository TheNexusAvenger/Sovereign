using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bouncer.Diagnostic;
using Bouncer.Expression;
using Bouncer.Parser;
using Microsoft.EntityFrameworkCore;
using Sovereign.Api.Bans.Configuration;
using Sovereign.Api.Bans.Web.Server.Controller.Shim;
using Sovereign.Api.Bans.Web.Server.Model;
using Sovereign.Core.Database.Model.Api;
using Sovereign.Core.Model;
using Sovereign.Core.Model.Request.Authorization;
using Sovereign.Core.Model.Response;
using Sprache;

namespace Sovereign.Api.Bans.Web.Server.Controller;

public class BanController
{
    /// <summary>
    /// Resources for the controller.
    /// Meant to be replaced during unit tests.
    /// </summary>
    public IBanControllerResources ControllerResources { get; set; }= new BanControllerResources();
    
    /// <summary>
    /// Handles a ban request.
    /// </summary>
    public async Task<JsonResponse> HandleBanRequest(BaseRequestContext requestContext)
    {
        // Return if the request could not be parsed.
        var request = requestContext.GetRequest(BanRequestJsonContext.Default.BanRequest);
        if (request == null)
        {
            return SimpleResponse.MalformedRequestResponse;
        }
        
        // Return a validation error.
        var validationError = ValidationErrorResponse.GetValidationErrorResponse(new List<ValidationErrorCheck>()
        {
            // Domain
            new ValidationErrorCheck()
            {
                Path = "domain",
                Message = "domain was not provided.",
                IsValid = () => (request.Domain != null),
            },
            
            // Authentication
            new ValidationErrorCheck()
            {
                Path = "authentication",
                Message = "authentication was not provided.",
                IsValid = () => (request.Authentication != null),
            },
            new ValidationErrorCheck()
            {
                Path = "authentication.method",
                Message = "authentication.method was not provided.",
                IsValid = () => (request.Authentication == null || request.Authentication.Method != null),
            },
            new ValidationErrorCheck()
            {
                Path = "authentication.data",
                Message = "authentication.data was not provided.",
                IsValid = () => (request.Authentication == null || request.Authentication.Data != null),
            },
            
            // Action
            new ValidationErrorCheck()
            {
                Path = "action",
                Message = "action was not provided.",
                IsValid = () => (request.Action != null),
            },
            new ValidationErrorCheck()
            {
                Path = "action.type",
                Message = "action.type was not provided.",
                IsValid = () => (request.Action == null || request.Action.Type != null),
            },
            new ValidationErrorCheck()
            {
                Path = "action.userIds",
                Message = "action.userIds was not provided.",
                IsValid = () => (request.Action == null || request.Action.UserIds != null),
            },
            new ValidationErrorCheck()
            {
                Path = "action.userIds",
                Message = "action.userIds was empty.",
                IsValid = () => (request.Action == null || request.Action.UserIds == null || request.Action?.UserIds.Count > 0),
            },
            new ValidationErrorCheck()
            {
                Path = "action.duration",
                Message = "action.duration was not a positive number.",
                IsValid = () => (request.Action == null || request.Action?.Duration == null || request.Action?.Duration > 0),
            },
            
            // Reason
            new ValidationErrorCheck()
            {
                Path = "reason",
                Message = "reason was not provided.",
                IsValid = () => (request.Reason != null),
            },
            new ValidationErrorCheck()
            {
                Path = "reason.display",
                Message = "reason.display was not provided.",
                IsValid = () => (request.Reason == null || request.Reason?.Display != null),
            },
            new ValidationErrorCheck()
            {
                Path = "reason.private",
                Message = "reason.private was not provided.",
                IsValid = () => (request.Reason == null || request.Reason?.Private != null),
            },
        });
        if (validationError != null)
        {
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
            return SimpleResponse.UnauthorizedResponse;
        }
        
        // Return 401 if the authorization header was invalid for the request.
        if (!requestContext.IsAuthorized(domainData.ApiKeys, domainData.SecretKeys))
        {
            return SimpleResponse.UnauthorizedResponse;
        }
        
        // Convert the Roblox user authorization.
        // Return 401 if the linked Roblox account can be verified or the Roblox id is invalid.
        await using var bansContext = this.ControllerResources.GetBansContext();
        var authenticationMethod = request.Authentication!.Method!.ToLower();
        var authenticationData = request.Authentication!.Data!;
        if (authenticationMethod != "roblox")
        {
            var authenticationLink = await bansContext.ExternalAccountLinks.FirstOrDefaultAsync(link => link.Domain.ToLower() == domain && link.LinkMethod.ToLower() == authenticationMethod && link.LinkData == authenticationData);
            if (authenticationLink == null)
            {
                return SimpleResponse.UnauthorizedResponse;
            }
            authenticationData = authenticationLink.RobloxUserId.ToString();
        }
        if (!long.TryParse(authenticationData, out var actingRobloxId))
        {
            return SimpleResponse.UnauthorizedResponse;
        }
        
        // Verify the Roblox user can handle the ban.
        if (domainData.Rules == null)
        {
            Logger.Error($"Domain \"{domainData.Name}\" was does not have rules.");
            return new JsonResponse(new SimpleResponse("ServerConfigurationError"), 503);
        }
        var action = AuthenticationRuleAction.Deny;
        foreach (var rule in domainData.Rules)
        {
            try
            {
                if (!Condition.FromParsedCondition(ExpressionParser.FullExpressionParser.Parse(rule.Rule)).Evaluate(actingRobloxId)) continue;
                action = AuthenticationRuleAction.Allow;
                break;
            }
            catch (Exception e)
            {
                Logger.Error($"Error evaluating rule for {actingRobloxId} in domain \"{domainData.Name}\".\n{e}");
                return new JsonResponse(new SimpleResponse("ServerError"), 503); 
            }
        }
        if (action == AuthenticationRuleAction.Deny)
        {
            
            return SimpleResponse.ForbiddenResponse;
        }
        
        // Add the actions. Ignore unban requests for non-banned users.
        var bannedRobloxIds = new List<long>();
        var unbannedRobloxIds = new List<long>();
        var currentTime = DateTime.Now;
        var banDomain = domainData.Name!;
        foreach (var targetRobloxId in request.Action!.UserIds!)
        {
            // Check if the user is already unbanned while requesting an unban.
            // Requests to reban aren't checked to allow for changes.
            var isBanned = false;
            var latestBan = await bansContext.BanEntries.Where(entry => entry.Domain.ToLower() == domain && entry.TargetRobloxUserId == targetRobloxId)
                .OrderByDescending(entry => entry.StartTime).FirstOrDefaultAsync();
            if (latestBan != null && latestBan.Action == BanAction.Ban && (latestBan.EndTime == null || latestBan.EndTime >= DateTime.Now))
            {
                isBanned = true;
            }
            if (!isBanned && request.Action!.Type == BanAction.Unban)
            {
                Logger.Warn($"Ignoring request to unban user {targetRobloxId} in domain \"{banDomain}\" because they are already unbanned.");
                continue;
            }
            
            // Add the action.
            if (request.Action!.Type == BanAction.Ban)
            {
                Logger.Info($"Banning user {targetRobloxId} in domain \"{banDomain}\" on behalf of {actingRobloxId}.");
                DateTime? endTime = null;
                if (request.Action!.Duration != null)
                {
                    endTime = currentTime.AddSeconds(request.Action!.Duration.Value);
                }
                bansContext.BanEntries.Add(new BanEntry()
                {
                    TargetRobloxUserId = targetRobloxId,
                    Domain = banDomain,
                    Action = BanAction.Ban,
                    StartTime = currentTime,
                    EndTime = endTime,
                    ActingRobloxUserId = actingRobloxId,
                    DisplayReason = request.Reason!.Display!,
                    PrivateReason = request.Reason!.Private!,
                });
                bannedRobloxIds.Add(targetRobloxId);
            }
            else if (request.Action!.Type == BanAction.Unban)
            {
                Logger.Info($"Unbanning user {targetRobloxId} in domain \"{banDomain}\" on behalf of {actingRobloxId}.");
                bansContext.BanEntries.Add(new BanEntry()
                {
                    TargetRobloxUserId = targetRobloxId,
                    Domain = banDomain,
                    Action = BanAction.Unban,
                    StartTime = currentTime,
                    ActingRobloxUserId = actingRobloxId,
                    DisplayReason = request.Reason!.Display!,
                    PrivateReason = request.Reason!.Private!,
                });
                unbannedRobloxIds.Add(targetRobloxId);
            }
        }
        await bansContext.SaveChangesAsync();
        
        // Return the banned and unbanned users.
        return new JsonResponse(new BanResponse()
        {
            BannedUserIds = bannedRobloxIds,
            UnbannedUserIds = unbannedRobloxIds,
        }, 200);
    }
}
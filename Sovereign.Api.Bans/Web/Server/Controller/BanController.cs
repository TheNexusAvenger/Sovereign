using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bouncer.Diagnostic;
using Microsoft.EntityFrameworkCore;
using Sovereign.Api.Bans.Web.Server.Controller.Shim;
using Sovereign.Api.Bans.Web.Server.Model;
using Sovereign.Core.Database.Model.Api;
using Sovereign.Core.Model;
using Sovereign.Core.Model.Request.Authorization;
using Sovereign.Core.Model.Response;

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
            Logger.Trace("Ignoring request to POST /bans/ban due to unreadable JSON.");
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
            Logger.Trace($"Ignoring request to POST /bans/ban due {validationError.Errors.Count} validation errors.");
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
            Logger.Trace("Ignoring request to POST /bans/ban due an invalid domain.");
            return SimpleResponse.UnauthorizedResponse;
        }
        
        // Return 401 if the authorization header was invalid for the request.
        if (!requestContext.IsAuthorized(domainData.ApiKeys, domainData.SecretKeys))
        {
            Logger.Trace("Ignoring request to POST /bans/ban due to an invalid or missing authorization header.");
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
                Logger.Trace($"Ignoring request to POST /bans/ban due to no account link in domain {domain} for link method {authenticationMethod}");
                return SimpleResponse.UnauthorizedResponse;
            }
            authenticationData = authenticationLink.RobloxUserId.ToString();
        }
        if (!long.TryParse(authenticationData, out var actingRobloxId))
        {
            Logger.Trace($"Ignoring request to POST /bans/ban due to malformed Roblox user id \"{actingRobloxId}\"");
            return SimpleResponse.UnauthorizedResponse;
        }
        
        // Verify the Roblox user can handle the ban.
        var authorizationError = domainData.IsRobloxUserAuthorized(actingRobloxId);
        if (authorizationError != null)
        {
            Logger.Trace($"Ignoring request to POST /bans/ban due to no authorization in domain {domain} for link method {authenticationMethod}");
            return authorizationError;
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
                    ExcludeAltAccounts = request.Action!.ExcludeAltAccounts,
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
                    ExcludeAltAccounts = request.Action!.ExcludeAltAccounts,
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

    /// <summary>
    /// Handles a request to list bans.
    /// </summary>
    public async Task<JsonResponse> HandleListBansRequest(BaseRequestContext requestContext)
    {
        // Return a validation error.
        var robloxUserIdValid = long.TryParse(requestContext.QueryParameters["robloxUserId"].FirstOrDefault(), out var robloxUserId);
        var startIndexValid = int.TryParse(requestContext.QueryParameters["start"].FirstOrDefault() ?? "0", out var startIndex);
        var maxEntriesValid = int.TryParse(requestContext.QueryParameters["max"].FirstOrDefault() ?? "20", out var maxEntries);
        var validationError = ValidationErrorResponse.GetValidationErrorResponse(new List<ValidationErrorCheck>()
        {
            new ValidationErrorCheck()
            {
                Path = "domain",
                Message = "domain was not provided in the query parameters.",
                IsValid = () => requestContext.QueryParameters.ContainsKey("domain"),
            },
            new ValidationErrorCheck()
            {
                Path = "robloxUserId",
                Message = "robloxUserId was not provided in the query parameters.",
                IsValid = () => requestContext.QueryParameters.ContainsKey("robloxUserId"),
            },
            new ValidationErrorCheck()
            {
                Path = "robloxUserId",
                Message = "robloxUserId was not a number.",
                IsValid = () => !requestContext.QueryParameters.ContainsKey("robloxUserId") || robloxUserIdValid,
            },
            new ValidationErrorCheck()
            {
                Path = "start",
                Message = "start was not a number.",
                IsValid = () => startIndexValid,
            },
            new ValidationErrorCheck()
            {
                Path = "max",
                Message = "max was not a number.",
                IsValid = () => maxEntriesValid,
            },
            new ValidationErrorCheck()
            {
                Path = "start",
                Message = "start must be a positive integer or zero.",
                IsValid = () => !startIndexValid || startIndex >= 0,
            },
            new ValidationErrorCheck()
            {
                Path = "max",
                Message = "max must be a positive integer.",
                IsValid = () => !maxEntriesValid || maxEntries > 0,
            },
        });
        if (validationError != null)
        {
            Logger.Trace($"Ignoring request to POST /bans/list due {validationError.Errors.Count} validation errors.");
            return new JsonResponse(validationError, 400);
        }
        
        // Get the domain for the request.
        var configuration = this.ControllerResources.GetConfiguration();
        var domain = requestContext.QueryParameters["Domain"].First()!.ToLower();
        var domains = configuration.Domains;
        if (domains == null)
        {
            Logger.Error("Domains was not configured in the configuration.");
            return new JsonResponse(new SimpleResponse("ServerConfigurationError"), 503);
        }
        var domainData = domains.FirstOrDefault(domainData => domainData.Name != null && domainData.Name.ToLower() == domain);
        if (domainData == null)
        {
            Logger.Trace("Ignoring request to POST /bans/list due an invalid domain.");
            return SimpleResponse.UnauthorizedResponse;
        }
        
        // Return 401 if the authorization header was invalid for the request.
        if (!requestContext.IsAuthorized(domainData.ApiKeys, domainData.SecretKeys))
        {
            Logger.Trace("Ignoring request to GET /bans/list due to an invalid or missing authorization header.");
            return SimpleResponse.UnauthorizedResponse;
        }
        
        // List the bans and return the entries.
        await using var bansContext = this.ControllerResources.GetBansContext();
        var banEntries = await bansContext.BanEntries
            .Where(entry => entry.Domain.ToLower() == domain && entry.TargetRobloxUserId == robloxUserId)
            .OrderByDescending(entry => entry.StartTime)
            .Skip(startIndex)
            .Take(Math.Min(maxEntries, 100)).ToListAsync();
        Logger.Debug($"Returning {banEntries.Count} for domain {domain} user id {robloxUserId}.");
        return new JsonResponse(new BanRecordResponse()
        {
            Entries = banEntries.Select(entry => new BanRecordResponseEntry()
            {
                Action =
                {
                    Type = entry.Action,
                    ExcludeAltAccounts = entry.ExcludeAltAccounts,
                    StartTime = entry.StartTime,
                    EndTime = entry.EndTime,
                },
                Reason =
                {
                    ActingUserId = entry.ActingRobloxUserId,
                    Display = entry.DisplayReason,
                    Private = entry.PrivateReason,
                },
            }).ToList(),
        }, 200);
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Bouncer.Diagnostic;
using Microsoft.EntityFrameworkCore;
using Sovereign.Api.Bans.Web.Server.Controller.Shim;
using Sovereign.Api.Bans.Web.Server.Model;
using Sovereign.Core.Database.Model.Api;
using Sovereign.Core.Model;
using Sovereign.Core.Model.Request.Api;
using Sovereign.Core.Model.Request.Authorization;
using Sovereign.Core.Model.Response;
using Sovereign.Core.Model.Response.Api;
using Sovereign.Core.Web.Server.Request;

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
            return new JsonResponse(new SimpleResponse(ResponseStatus.ServerConfigurationError), 503);
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
                Logger.Trace($"Ignoring request to POST /bans/ban due to no account link in domain {domainData.Name} for link method \"{authenticationMethod}\".");
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
            Logger.Trace($"Ignoring request to POST /bans/ban due to no authorization in domain {domainData.Name} for link method \"{authenticationMethod}\".");
            return authorizationError;
        }
        
        // Verify that the user is not banning a user at or above their group level.
        // Ideally, at least the banning user should be cached in the authorization check.
        if (domainData.GroupIdRankChecks != null)
        {
            var robloxGroupClient = this.ControllerResources.GetRobloxGroupClient();
            foreach (var groupId in domainData.GroupIdRankChecks)
            {
                var banningUserRank = await robloxGroupClient.GetRankInGroupAsync(actingRobloxId, groupId);
                foreach (var targetRobloxId in request.Action!.UserIds!)
                {
                    var targetUserRank = await robloxGroupClient.GetRankInGroupAsync(targetRobloxId, groupId);
                    if (banningUserRank <= 0 && targetUserRank <= 0) continue;
                    if (banningUserRank > targetUserRank) continue;
                    Logger.Info($"Ignoring request to POST /bans/ban in domain {domainData.Name} due to {actingRobloxId} ({banningUserRank}) being above or the same rank as {targetRobloxId} ({targetUserRank}) in group {groupId}.");
                    return new JsonResponse(new SimpleResponse(ResponseStatus.GroupRankPermissionError), 403);
                }
            }
        }
        
        // Add the actions. Ignore unban requests for non-banned users.
        var bannedRobloxIds = new List<long>();
        var unbannedRobloxIds = new List<long>();
        var addedBanEntries = new List<BanEntry>();
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

                var banEntry = new BanEntry()
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
                };
                addedBanEntries.Add(banEntry);
                bansContext.BanEntries.Add(banEntry);
                bannedRobloxIds.Add(targetRobloxId);
            }
            else if (request.Action!.Type == BanAction.Unban)
            {
                Logger.Info($"Unbanning user {targetRobloxId} in domain \"{banDomain}\" on behalf of {actingRobloxId}.");

                var banEntry = new BanEntry()
                {
                    TargetRobloxUserId = targetRobloxId,
                    Domain = banDomain,
                    Action = BanAction.Unban,
                    ExcludeAltAccounts = request.Action!.ExcludeAltAccounts,
                    StartTime = currentTime,
                    ActingRobloxUserId = actingRobloxId,
                    DisplayReason = request.Reason!.Display!,
                    PrivateReason = request.Reason!.Private!,
                };
                addedBanEntries.Add(banEntry);
                bansContext.BanEntries.Add(banEntry);
                unbannedRobloxIds.Add(targetRobloxId);
            }
        }
        await bansContext.SaveChangesAsync();
        
        // Send the webhooks for quicker responses.
        var webhookEndpoints = Environment.GetEnvironmentVariable("INTERNAL_WEBHOOK_ENDPOINTS");
        if (webhookEndpoints != null)
        {
            // Get the webhook secret.
            var internalWebhookSecretKey = Environment.GetEnvironmentVariable("INTERNAL_WEBHOOK_SECRET_KEY");
            if (internalWebhookSecretKey != null)
            {
                // Prepare the webhook data.
                var banIds = addedBanEntries.Select(entry => entry.Id).ToList();
                var webhookBody = new SovereignWebhookRequest()
                {
                    Domain = domainData.Name!,
                    Ids = banIds,
                };
                var webhookContents = JsonSerializer.Serialize(webhookBody, SovereignWebhookRequestJsonContext.Default.SovereignWebhookRequest);

                // Prepare the webhook signature.
                using var sha256 = new HMACSHA256(Encoding.UTF8.GetBytes(internalWebhookSecretKey));
                var signature = Convert.ToBase64String(sha256.ComputeHash(Encoding.UTF8.GetBytes(webhookContents)));
                var authorizationHeader = $"Signature {signature}";
                
                // Send the webhooks.
                foreach (var endpoint in webhookEndpoints.Split(","))
                {
                    Logger.Debug($"Sending ban webhook to {endpoint}.");
                    var _ = Task.Run(async () =>
                    {
                        try
                        {
                            // Create and send the request.
                            var httpClient = new HttpClient();
                            var webhookRequest = new HttpRequestMessage()
                            {
                                RequestUri = new Uri(endpoint),
                                Headers =
                                {
                                    {"Authorization", authorizationHeader},
                                },
                                Method = HttpMethod.Post,
                                Content = JsonContent.Create(webhookBody, SovereignWebhookRequestJsonContext.Default.SovereignWebhookRequest),
                            };
                            var response = await httpClient.SendAsync(webhookRequest);
                            if (response.IsSuccessStatusCode)
                            {
                                Logger.Debug($"Webhook {endpoint} completed with HTTP {response.StatusCode}.");
                            }
                            else
                            {
                                Logger.Error($"Webhook {endpoint} completed with HTTP {response.StatusCode}.");
                            }
                        }
                        catch (Exception e)
                        {
                            Logger.Error($"Error sending webhook to {endpoint}.\n{e}");
                        }
                    });
                }
            }
            else
            {
                // Log if there is no secret.
                Logger.Warn("INTERNAL_WEBHOOK_SECRET_KEY is unset. Webhooks can't be authenticated.");
            }
        }
        
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
        var domain = requestContext.QueryParameters["domain"].First()!.ToLower();
        var domains = configuration.Domains;
        if (domains == null)
        {
            Logger.Error("Domains was not configured in the configuration.");
            return new JsonResponse(new SimpleResponse(ResponseStatus.ServerConfigurationError), 503);
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
        var userBanEntriesQuery = bansContext.BanEntries
            .Where(entry => entry.Domain.ToLower() == domain && entry.TargetRobloxUserId == robloxUserId);
        var banEntries = await userBanEntriesQuery
            .OrderByDescending(entry => entry.StartTime)
            .Skip(startIndex)
            .Take(Math.Min(maxEntries, 100)).ToListAsync();
        Logger.Debug($"Returning {banEntries.Count} for domain {domainData.Name} user id {robloxUserId}.");
        return new JsonResponse(new BanRecordResponse()
        {
            Total = await userBanEntriesQuery.CountAsync(),
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
    
    /// <summary>
    /// Handles a request to get user permissions.
    /// </summary>
    public async Task<JsonResponse> HandleGetPermissionsRequest(BaseRequestContext requestContext)
    {
        // Return a validation error.
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
                Path = "linkMethod",
                Message = "linkMethod was not provided in the query parameters.",
                IsValid = () => requestContext.QueryParameters.ContainsKey("linkMethod"),
            },
            new ValidationErrorCheck()
            {
                Path = "linkData",
                Message = "linkData was not provided in the query parameters.",
                IsValid = () => requestContext.QueryParameters.ContainsKey("linkData"),
            },
        });
        if (validationError != null)
        {
            Logger.Trace($"Ignoring request to GET /bans/permissions due {validationError.Errors.Count} validation errors.");
            return new JsonResponse(validationError, 400);
        }
        
        // Get the domain for the request.
        var configuration = this.ControllerResources.GetConfiguration();
        var domain = requestContext.QueryParameters["domain"].First()!.ToLower();
        var domains = configuration.Domains;
        if (domains == null)
        {
            Logger.Error("Domains was not configured in the configuration.");
            return new JsonResponse(new SimpleResponse(ResponseStatus.ServerConfigurationError), 503);
        }
        var domainData = domains.FirstOrDefault(domainData => domainData.Name != null && domainData.Name.ToLower() == domain);
        if (domainData == null)
        {
            Logger.Trace("Ignoring request to GET /bans/permissions due an invalid domain.");
            return SimpleResponse.UnauthorizedResponse;
        }
        
        // Return 401 if the authorization header was invalid for the request.
        if (!requestContext.IsAuthorized(domainData.ApiKeys, domainData.SecretKeys))
        {
            Logger.Trace("Ignoring request to GET /bans/permissions due to an invalid or missing authorization header.");
            return SimpleResponse.UnauthorizedResponse;
        }
        
        // Convert the Roblox user authorization and prepare the response.
        var response = new BanPermissionResponse();
        await using var bansContext = this.ControllerResources.GetBansContext();
        var authenticationMethod = requestContext.QueryParameters["linkMethod"].First()!.ToLower();
        var authenticationData = requestContext.QueryParameters["linkData"].First();
        if (authenticationMethod != "roblox")
        {
            var authenticationLink = await bansContext.ExternalAccountLinks.FirstOrDefaultAsync(link => link.Domain.ToLower() == domain && link.LinkMethod.ToLower() == authenticationMethod && link.LinkData == authenticationData);
            if (authenticationLink == null)
            {
                Logger.Trace($"Returning InvalidAccountLink ban permission issue in domain {domainData.Name} for link method \"{authenticationMethod}\".");
                response.CanBan = false;
                response.BanPermissionIssue = BanPermissionIssue.InvalidAccountLink;
            }
            else
            {
                authenticationData = authenticationLink.RobloxUserId.ToString();
            }
        }
        if (!long.TryParse(authenticationData, out var robloxUserId) && response.CanBan)
        {
            Logger.Trace($"Returning MalformedRobloxId ban permission issue in domain {domainData.Name} for link method \"{authenticationMethod}\".");
            response.CanBan = false;
            response.BanPermissionIssue = BanPermissionIssue.MalformedRobloxId;
        }
        
        // Verify the Roblox user can handle the ban.
        if (response.CanBan)
        {
            var authorizationError = domainData.IsRobloxUserAuthorized(robloxUserId);
            if (authorizationError != null)
            {
                Logger.Trace($"Returning Forbidden ban permission issue in in domain {domainData.Name} for link method \"{authenticationMethod}\".");
                response.CanBan = false;
                response.BanPermissionIssue = BanPermissionIssue.Forbidden;
            }
        }
        
        // Set the CanLink status.
        // Users can link if they can ban or have existing links.
        if (!response.CanBan && await bansContext.ExternalAccountLinks.AllAsync(entry => entry.RobloxUserId != robloxUserId))
        {
            response.CanLink = false;
        }
        
        // Return the response.
        return new JsonResponse(response, 200);
    }
}
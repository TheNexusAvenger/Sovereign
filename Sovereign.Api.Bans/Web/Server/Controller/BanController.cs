using System.Collections.Generic;
using System.Threading.Tasks;
using Sovereign.Api.Bans.Web.Server.Model;
using Sovereign.Core.Model.Response;

namespace Sovereign.Api.Bans.Web.Server.Controller;

public class BanController
{
    public static async Task<JsonResponse> HandleBanRequest(BanRequest request)
    {
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
        
        // TODO: Authentication error (401 - can't find authorization).
        // TODO: Authorization error (403 - user can't ban).
        // TODO: Handle request
        return null;
    }
}
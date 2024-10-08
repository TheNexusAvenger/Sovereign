﻿using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Http;

namespace Sovereign.Core.Model.Request.Authorization;

public enum RequestAuthorizationSource
{
    Body,
    Query,
}

public class RequestContext : BaseRequestContext
{
    /// <summary>
    /// Authorization source of the request to base the signature check off of.
    /// </summary>
    public readonly RequestAuthorizationSource RequestAuthorizationSource;
    
    /// <summary>
    /// Creates a request context.
    /// </summary>
    /// <param name="httpContext">HttpContext to read the request from.</param>
    /// <param name="authorizationSource">Source to base signatures on.</param>
    public RequestContext(HttpContext httpContext, RequestAuthorizationSource authorizationSource = RequestAuthorizationSource.Body) : base(httpContext)
    {
        this.RequestAuthorizationSource = authorizationSource;
    }

    /// <summary>
    /// Returns if the request is authorized.
    /// </summary>
    /// <param name="apiKeys">List of API keys that are valid.</param>
    /// <param name="signatureSecrets">List of request signature secrets that are valid.</param>
    /// <returns>Whether the request is authorized or not.</returns>
    public override bool IsAuthorized(List<string>? apiKeys, List<string>? signatureSecrets)
    {
        // Return false if there is no authorization header.
        if (this.Authorization == null)
        {
            return false;
        }
        
        // Verify the authorization header.
        var authorizationHeaderParts = this.Authorization.Split(" ", 2);
        if (authorizationHeaderParts.Length == 2)
        {
            var authorizationScheme = authorizationHeaderParts[0].ToLower();
            var authorizationData = authorizationHeaderParts[1];
            if (authorizationScheme == "apikey" && apiKeys != null)
            {
                // Return true if an API key matches.
                foreach (var apiKey in apiKeys)
                {
                    if (apiKey != authorizationData) continue;
                    return true;
                }
            }
            else if (authorizationScheme == "signature" && signatureSecrets != null)
            {
                // Return true if a signature matches.
                var compareData = (this.RequestAuthorizationSource == RequestAuthorizationSource.Body ? this.RequestBody : this.RawQuery);
                foreach (var signatureSecret in signatureSecrets)
                {
                    using var sha256 = new HMACSHA256(Encoding.UTF8.GetBytes(signatureSecret));
                    var newSignature = Convert.ToBase64String(sha256.ComputeHash(Encoding.UTF8.GetBytes(compareData)));
                    if (authorizationData != newSignature) continue;
                    return true;
                }
            }
        }
        
        // Return false (not authorized).
        return false;
    }
}
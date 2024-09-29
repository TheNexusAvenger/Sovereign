using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using System.Threading.Tasks;
using Bouncer.State;
using Bouncer.Web.Client.Shim;
using Sovereign.Core.Model;
using Sovereign.Core.Model.Request.Api;
using Sovereign.Core.Model.Response;
using Sovereign.Core.Model.Response.Api;
using Sovereign.Discord.Configuration;

namespace Sovereign.Discord.Web.Client;

public class SovereignBansApiClient
{
    /// <summary>
    /// HTTP client to send requests.
    /// </summary>
    private readonly IHttpClient _httpClient;

    /// <summary>
    /// Function to get the configuration.
    /// Intended to be replaced for unit tests.
    /// </summary>
    public Func<DiscordConfiguration> GetConfiguration { get; set; } = Configurations.GetConfiguration<DiscordConfiguration>;
    
    /// <summary>
    /// Creates a Sovereign API client.
    /// </summary>
    /// <param name="httpClient">HTTP client to send requests with.</param>
    public SovereignBansApiClient(IHttpClient httpClient)
    {
        this._httpClient = httpClient;
    }
    
    /// <summary>
    /// Creates a Sovereign API client.
    /// </summary>
    public SovereignBansApiClient() : this(new HttpClientImpl())
    {
        
    }

    /// <summary>
    /// Builds an authorization header for a request.
    /// </summary>
    /// <param name="domain">Domain of the request to get the secret key of.</param>
    /// <param name="data">Data used to create the authorization header.</param>
    /// <returns>Value of the authorization header.</returns>
    public string GetAuthorizationHeader(string domain, string data)
    {
        // Get the secret key.
        var configuration = this.GetConfiguration();
        var domainData = configuration.Domains!.FirstOrDefault(entry => string.Equals(entry.Name, domain, StringComparison.CurrentCultureIgnoreCase));
        if (domainData?.ApiSecretKey == null)
        {
            throw new InvalidDataException($"No apiSecretKey was configured for {domain}");
        }
        
        // Create the authorization header.
        using var sha256 = new HMACSHA256(Encoding.UTF8.GetBytes(domainData.ApiSecretKey));
        var signature = Convert.ToBase64String(sha256.ComputeHash(Encoding.UTF8.GetBytes(data)));
        return $"Signature {signature}";
    }

    /// <summary>
    /// Sends a GET request to Sovereign.
    /// </summary>
    /// <param name="domain">Sovereign ban domain to perform the request to.</param>
    /// <param name="urlPath">Additional path after the base URL to use with Sovereign.</param>
    /// <param name="query">Query parameters for the request, including the leading question mark.</param>
    /// <param name="jsonResponseTypeInfo">JSON type information for the response contents.</param>
    /// <typeparam name="TResponse">Type of the response.</typeparam>
    /// <returns>JSON response for the request.</returns>
    public async Task<TResponse> GetAsync<TResponse>(string domain, string urlPath, string query, JsonTypeInfo<TResponse> jsonResponseTypeInfo)
    {
        // Send the request.
        var authorizationHeader = this.GetAuthorizationHeader(domain, query);
        var baseUrl = Environment.GetEnvironmentVariable("SOVEREIGN_BANS_API_BASE_URL") ?? "http://localhost:8000";
        var request = new HttpRequestMessage()
        {
            RequestUri = new Uri(baseUrl + urlPath + query),
            Headers =
            {
                {"Authorization", authorizationHeader},
            },
            Method = HttpMethod.Get,
        };
        var response = await this._httpClient.SendAsync(request);
        
        // Parse and return the response.
        return JsonSerializer.Deserialize<TResponse>(response.Content, jsonResponseTypeInfo)!;
    }
    
    /// <summary>
    /// Sends a GET request to Sovereign.
    /// </summary>
    /// <param name="domain">Sovereign ban domain to perform the request to.</param>
    /// <param name="urlPath">Additional path after the base URL to use with Sovereign.</param>
    /// <param name="requestBody">JSON body to send.</param>
    /// <param name="jsonRequestTypeInfo">JSON type information for the request contents.</param>
    /// <param name="jsonResponseTypeInfo">JSON type information for the response contents.</param>
    /// <typeparam name="TRequest">Type of the request.</typeparam>
    /// <typeparam name="TResponse">Type of the response.</typeparam>
    /// <returns>JSON response for the request.</returns>
    public async Task<TResponse> PostAsync<TRequest, TResponse>(string domain, string urlPath, TRequest requestBody, JsonTypeInfo<TRequest> jsonRequestTypeInfo, JsonTypeInfo<TResponse> jsonResponseTypeInfo)
    {
        // Send the request.
        var requestData = await JsonContent.Create(requestBody, jsonRequestTypeInfo).ReadAsStringAsync();
        var authorizationHeader = this.GetAuthorizationHeader(domain, requestData);
        var baseUrl = Environment.GetEnvironmentVariable("SOVEREIGN_BANS_API_BASE_URL") ?? "http://localhost:8000";
        var request = new HttpRequestMessage()
        {
            RequestUri = new Uri(baseUrl + urlPath),
            Headers =
            {
                {"Authorization", authorizationHeader},
            },
            Method = HttpMethod.Post,
            Content = JsonContent.Create(requestBody, jsonRequestTypeInfo),
        };
        var response = await this._httpClient.SendAsync(request);
        
        // Parse and return the response.
        return JsonSerializer.Deserialize<TResponse>(response.Content, jsonResponseTypeInfo)!;
    }

    /// <summary>
    /// Returns the ban permissions for a Roblox user.
    /// </summary>
    /// <param name="domain">Domain of the bans to get the permissions for.</param>
    /// <param name="robloxUserId">Roblox user id to check the permissions for.</param>
    /// <returns>Response for the ban permissions.</returns>
    public async Task<BanPermissionResponse> GetPermissionsForRobloxUserAsync(string domain, long robloxUserId)
    {
        var query = $"?domain={domain}&linkMethod=Roblox&linkData={robloxUserId}";
        return await this.GetAsync(domain, "/bans/permissions/", query, BanPermissionResponseJsonContext.Default.BanPermissionResponse);
    }

    /// <summary>
    /// Returns the ban permissions for a Discord user.
    /// </summary>
    /// <param name="domain">Domain of the bans to get the permissions for.</param>
    /// <param name="discordUserId">Discord user id to check the permissions for.</param>
    /// <returns>Response for the ban permissions.</returns>
    public async Task<BanPermissionResponse> GetPermissionsForDiscordUserAsync(string domain, ulong discordUserId)
    {
        var query = $"?domain={domain}&linkMethod=Discord&linkData={discordUserId}";
        return await this.GetAsync(domain, "/bans/permissions/", query, BanPermissionResponseJsonContext.Default.BanPermissionResponse);
    }

    /// <summary>
    /// Attempts to link a Discord account to a Roblox account.
    /// </summary>
    /// <param name="domain">Domain to link the accounts in.</param>
    /// <param name="discordUserId">Discord user id to link.</param>
    /// <param name="robloxUserId">Roblox user id to link.</param>
    /// <returns>Response for the link request.</returns>
    public async Task<SimpleResponse> LinkDiscordAccountAsync(string domain, ulong discordUserId, long robloxUserId)
    {
        var requestBody = new ExternalLinkRequest()
        {
            Domain = domain,
            RobloxUserId = robloxUserId,
            LinkMethod = "Discord",
            LinkData = discordUserId.ToString(),
        };
        return await this.PostAsync(domain, "/accounts/link", requestBody, ExternalLinkRequestJsonContext.Default.ExternalLinkRequest, SimpleResponseJsonContext.Default.SimpleResponse);
    }
    
    /// <summary>
    /// Bans or unbans a list of Roblox user ids.
    /// </summary>
    /// <param name="domain">Domain to link the accounts in.</param>
    /// <param name="banAction">Action to perform for the ban.</param>
    /// <param name="discordUserId">Discord user id to ban the users as.</param>
    /// <param name="robloxUserIds">Roblox user ids to link.</param>
    /// <param name="displayReason">Reason to display to the users.</param>
    /// <param name="privateReason">Reason to store internally for the bans.</param>
    /// <param name="duration">Optional duration of the ban in seconds.</param>
    /// <returns>Response for the bans.</returns>
    public async Task<BanResponse> BanAsync(string domain, BanAction banAction, ulong discordUserId, List<long> robloxUserIds, string displayReason, string privateReason, long? duration = null)
    {
        var requestBody = new BanRequest()
        {
            Domain = domain,
            Authentication = new BanRequestAuthentication()
            {
                Method = "Discord",
                Data = discordUserId.ToString(),
            },
            Action = new BanRequestAction()
            {
                Type = banAction,
                UserIds = robloxUserIds,
                ExcludeAltAccounts = false,
                Duration = duration,
            },
            Reason = new BanRequestReason()
            {
                Display = displayReason,
                Private = privateReason,
            },
        };
        return await this.PostAsync(domain, "/bans/ban", requestBody, BanRequestJsonContext.Default.BanRequest, BanResponseJsonContext.Default.BanResponse);
    }
}
using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Bouncer.Web.Client.Shim;
using Sovereign.Core.Web.Client.Response;

namespace Sovereign.Core.Web.Client;

public class RobloxUserProfileClient
{
    /// <summary>
    /// HTTP client to send requests.
    /// </summary>
    private readonly IHttpClient _httpClient;

    /// <summary>
    /// Version of the RobloxUserProfileClient with caching.
    /// </summary>
    public static RobloxUserProfileClient CachingClient => new RobloxUserProfileClient(new CachingHttpClient());

    /// <summary>
    /// Version of the RobloxUserProfileClient with no caching.
    /// </summary>
    public static RobloxUserProfileClient NonCachingClient => new RobloxUserProfileClient(new HttpClientImpl());
    
    /// <summary>
    /// Creates a Roblox User Profile Client client.
    /// </summary>
    /// <param name="httpClient">HTTP client to send requests with.</param>
    public RobloxUserProfileClient(IHttpClient httpClient)
    {
        this._httpClient = httpClient;
    }

    /// <summary>
    /// Fetches the Roblox user profile.
    /// The information is not cached due to the use case in the Discord bot relying on not caching.
    /// </summary>
    /// <param name="robloxUserId">Roblox user id to get.</param>
    /// <returns>User profile for the user id.</returns>
    public async Task<UserProfileResponse> GetRobloxProfileAsync(long robloxUserId)
    {
        var response = await this._httpClient.SendAsync(new HttpRequestMessage()
        {
            RequestUri = new Uri($"https://users.roblox.com/v1/users/{robloxUserId}"),
            Method = HttpMethod.Get,
        });
        return JsonSerializer.Deserialize(response.Content, UserProfileResponseJsonContext.Default.UserProfileResponse)!;
    }
}
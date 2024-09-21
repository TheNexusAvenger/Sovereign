using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Bouncer.Web.Client;
using Bouncer.Web.Client.Shim;
using Sovereign.Core.Web.Client.Request;
using Sovereign.Core.Web.Client.Response;

namespace Sovereign.Core.Web.Client;

public class RobloxUserRestrictionClient
{
    /// <summary>
    /// Maximum length of the display ban reason.
    /// </summary>
    public const int MaxDisplayReasonLength = 400;
    
    /// <summary>
    /// Maximum length of the private ban reason.
    /// </summary>
    public const int MaxPrivateReasonLength = 1000;
    
    /// <summary>
    /// Roblox Open Cloud client to non-caching send requests.
    /// </summary>
    private readonly RobloxOpenCloudClient _robloxClient;
    
    /// <summary>
    /// Roblox Open Cloud API key.
    /// </summary>
    public string? OpenCloudApiKey {
        get => this._robloxClient.OpenCloudApiKey;
        set => this._robloxClient.OpenCloudApiKey = value;
    }
    
    /// <summary>
    /// Creates a Roblox User Restriction API client.
    /// </summary>
    /// <param name="httpClient">HTTP client to send requests with.</param>
    public RobloxUserRestrictionClient(IHttpClient httpClient)
    {
        this._robloxClient = new RobloxOpenCloudClient(httpClient);
    }
    
    /// <summary>
    /// Creates a Roblox Group API client.
    /// </summary>
    public RobloxUserRestrictionClient() : this(new HttpClientImpl())
    {
        
    }

    /// <summary>
    /// Bans a user.
    /// </summary>
    /// <param name="universeId">Id of the universe to ban the user from.</param>
    /// <param name="userId">Roblox user id to ban from the universe.</param>
    /// <param name="displayReason">Display reason to present to the user.</param>
    /// <param name="privateReason">Private reason to associate with the user.</param>
    /// <param name="duration">Optional duration of the ban (in seconds).</param>
    /// <param name="excludeAltAccounts">If true, alt accounts will not be banned or unbanned.</param>
    /// <returns>The response for the ban request.</returns>
    public async Task<UserRestrictionResponse> BanAsync(long universeId, long userId, string displayReason, string privateReason, long? duration = null, bool excludeAltAccounts = false)
    {
        return await this._robloxClient.RequestAsync(HttpMethod.Patch,
            $"https://apis.roblox.com/cloud/v2/universes/{universeId}/user-restrictions/{userId}",
            UserRestrictionResponseJsonContext.Default.UserRestrictionResponse, JsonContent.Create(
                new UserRestrictionRequest()
                {
                    GameJoinRestriction =
                    {
                        Active = true,
                        Duration = duration == null ? null : $"{duration}s",
                        PrivateReason = ClampString(privateReason, MaxPrivateReasonLength),
                        DisplayReason = ClampString(displayReason, MaxDisplayReasonLength),
                        ExcludeAltAccounts = excludeAltAccounts,
                    },
                }, UserRestrictionRequestJsonContext.Default.UserRestrictionRequest));
    }

    /// <summary>
    /// Unbans a user.
    /// </summary>
    /// <param name="universeId">Id of the universe to unban the user from.</param>
    /// <param name="userId">Roblox user id to unban from the universe.</param>
    /// <param name="excludeAltAccounts">If true, alt accounts will not be banned or unbanned.</param>
    /// <returns>The response for the unban request.</returns>
    public async Task<UserRestrictionResponse> UnbanAsync(long universeId, long userId, bool excludeAltAccounts = false)
    {
        return await this._robloxClient.RequestAsync(HttpMethod.Patch,
            $"https://apis.roblox.com/cloud/v2/universes/{universeId}/user-restrictions/{userId}",
            UserRestrictionResponseJsonContext.Default.UserRestrictionResponse, JsonContent.Create(
                new UserRestrictionRequest()
                {
                    GameJoinRestriction =
                    {
                        Active = false,
                        ExcludeAltAccounts = excludeAltAccounts,
                    },
                }, UserRestrictionRequestJsonContext.Default.UserRestrictionRequest));
    }

    /// <summary>
    /// Clamps a string to a given maximum length.
    /// </summary>
    /// <param name="inputString">Input string to clamp.</param>
    /// <param name="maxLength">Maximum length to clamp the string to.</param>
    /// <returns>Clamped version of the given string.</returns>
    public static string ClampString(string inputString, int maxLength)
    {
        return inputString.Length < maxLength ? inputString : inputString.Substring(0, maxLength);
    }
}
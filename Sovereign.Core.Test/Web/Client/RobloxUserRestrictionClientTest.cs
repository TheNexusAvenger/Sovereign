using System.Net;
using System.Text.Json;
using Bouncer.Test.Web.Client.Shim;
using Bouncer.Web.Client.Shim;
using NUnit.Framework;
using Sovereign.Core.Web.Client;
using Sovereign.Core.Web.Client.Request;

namespace Sovereign.Core.Test.Web.Client;

public class RobloxUserRestrictionClientTest
{
    private TestHttpClient _testHttpClient;
    private RobloxUserRestrictionClient _client;

    [SetUp]
    public void SetUp()
    {
        this._testHttpClient = new TestHttpClient();
        this._client = new RobloxUserRestrictionClient(this._testHttpClient);
    }
    
    [Test]
    public void TestBanAsyncDuration()
    {
        this._testHttpClient.SetResponseResolver($"https://apis.roblox.com/cloud/v2/universes/123/user-restrictions/456", (request) =>
        {
            var requestBody = request.Content!.ReadAsStringAsync().Result;
            var requestJson = JsonSerializer.Deserialize(requestBody, UserRestrictionRequestJsonContext.Default.UserRestrictionRequest)!;
            Assert.That(requestJson.GameJoinRestriction.Active, Is.True);
            Assert.That(requestJson.GameJoinRestriction.Duration, Is.EqualTo("120s"));
            Assert.That(requestJson.GameJoinRestriction.PrivateReason, Is.EqualTo(new string('B', 1000)));
            Assert.That(requestJson.GameJoinRestriction.DisplayReason, Is.EqualTo(new string('A', 400)));
            Assert.That(requestJson.GameJoinRestriction.ExcludeAltAccounts, Is.False);
            
            return new HttpStringResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = "{}",
            };
        });

        Assert.That(this._client.BanAsync(123, 456, new string('A', 2000), new string('B', 2000), 120).Result.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }
    
    [Test]
    public void TestBanAsyncNoDuration()
    {
        this._testHttpClient.SetResponseResolver($"https://apis.roblox.com/cloud/v2/universes/123/user-restrictions/456", (request) =>
        {
            var requestBody = request.Content!.ReadAsStringAsync().Result;
            var requestJson = JsonSerializer.Deserialize(requestBody, UserRestrictionRequestJsonContext.Default.UserRestrictionRequest)!;
            Assert.That(requestJson.GameJoinRestriction.Active, Is.True);
            Assert.That(requestJson.GameJoinRestriction.Duration, Is.Null);
            Assert.That(requestJson.GameJoinRestriction.PrivateReason, Is.EqualTo(new string('B', 1000)));
            Assert.That(requestJson.GameJoinRestriction.DisplayReason, Is.EqualTo(new string('A', 400)));
            Assert.That(requestJson.GameJoinRestriction.ExcludeAltAccounts, Is.False);
            
            return new HttpStringResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = "{}",
            };
        });

        Assert.That(this._client.BanAsync(123, 456, new string('A', 2000), new string('B', 2000)).Result.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public void TestUnbanAsync()
    {
        this._testHttpClient.SetResponseResolver($"https://apis.roblox.com/cloud/v2/universes/123/user-restrictions/456", (request) =>
        {
            var requestBody = request.Content!.ReadAsStringAsync().Result;
            var requestJson = JsonSerializer.Deserialize(requestBody, UserRestrictionRequestJsonContext.Default.UserRestrictionRequest)!;
            Assert.That(requestJson.GameJoinRestriction.Active, Is.False);
            Assert.That(requestJson.GameJoinRestriction.Duration, Is.Null);
            Assert.That(requestJson.GameJoinRestriction.PrivateReason, Is.Null);
            Assert.That(requestJson.GameJoinRestriction.DisplayReason, Is.Null);
            Assert.That(requestJson.GameJoinRestriction.ExcludeAltAccounts, Is.False);
            
            return new HttpStringResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = "{}",
            };
        });

        Assert.That(this._client.UnbanAsync(123, 456).Result.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public void TestGetUserRestriction()
    {
        this._testHttpClient.SetResponseResolver($"https://apis.roblox.com/cloud/v2/universes/123/user-restrictions/456", (request) =>
        {
            return new HttpStringResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = "{}",
            };
        });

        Assert.That(this._client.GetUserRestriction(123, 456).Result.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }
    
    [Test]
    public void TestClampString()
    {
        Assert.That(RobloxUserRestrictionClient.ClampString("Test", 3), Is.EqualTo("Tes"));
        Assert.That(RobloxUserRestrictionClient.ClampString("Test", 4), Is.EqualTo("Test"));
        Assert.That(RobloxUserRestrictionClient.ClampString("Test", 5), Is.EqualTo("Test"));
    }
}
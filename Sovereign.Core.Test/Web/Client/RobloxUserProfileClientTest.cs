using System.Net;
using Bouncer.Test.Web.Client.Shim;
using NUnit.Framework;
using Sovereign.Core.Web.Client;

namespace Sovereign.Core.Test.Web.Client;

public class RobloxUserProfileClientTest
{
    private TestHttpClient _testHttpClient;
    private RobloxUserProfileClient _client;

    [SetUp]
    public void SetUp()
    {
        this._testHttpClient = new TestHttpClient();
        this._client = new RobloxUserProfileClient(this._testHttpClient);
    }

    [Test]
    public void TestGetRobloxProfileAsync()
    {
        this._testHttpClient.SetResponse("https://users.roblox.com/v1/users/12345", HttpStatusCode.OK, "{\"name\":\"TestName\",\"displayName\":\"TestDisplayName\",\"description\":\"TestDescription\"}");
        var response = this._client.GetRobloxProfileAsync(12345).Result;
        Assert.That(response.Name, Is.EqualTo("TestName"));
        Assert.That(response.DisplayName, Is.EqualTo("TestDisplayName"));
        Assert.That(response.Description, Is.EqualTo("TestDescription"));
    }
}
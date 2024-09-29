using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using Bouncer.Test.Web.Client.Shim;
using NUnit.Framework;
using Sovereign.Core.Model;
using Sovereign.Core.Model.Response;
using Sovereign.Core.Model.Response.Api;
using Sovereign.Discord.Configuration;
using Sovereign.Discord.Web.Client;

namespace Sovereign.Discord.Test.Web.Client;

public class SovereignBansApiClientTest
{
    private TestHttpClient _testHttpClient;
    private SovereignBansApiClient _client;

    [SetUp]
    public void SetUp()
    {
        this._testHttpClient = new TestHttpClient();
        this._client = new SovereignBansApiClient(this._testHttpClient);
        this._client.GetConfiguration = () => new DiscordConfiguration()
        {
            Domains = new List<DiscordDomainConfiguration>()
            {
                new DiscordDomainConfiguration()
                {
                    Name = "TestDomain",
                    ApiSecretKey = "TestSecretKey",
                },
            },
        };
    }

    [Test]
    public void TestGetAuthorizationHeaderNoApiKey()
    {
        Assert.Throws<InvalidDataException>(() =>
        {
            this._client.GetAuthorizationHeader("UnknownDomain", "TestData");
        });
    }

    [Test]
    public void TestGetAuthorizationHeader()
    {
        using var sha256 = new HMACSHA256(Encoding.UTF8.GetBytes("TestSecretKey"));
        var signature = Convert.ToBase64String(sha256.ComputeHash(Encoding.UTF8.GetBytes("TestData")));
        Assert.That(this._client.GetAuthorizationHeader("TestDomain", "TestData"), Is.EqualTo($"Signature {signature}"));
    }

    [Test]
    public void TestGetPermissionsForRobloxUserAsync()
    {
        this._testHttpClient.SetResponse("http://localhost:8000/bans/permissions/?domain=TestDomain&linkMethod=Roblox&linkData=12345", HttpStatusCode.OK, "{\"status\":\"Success\",\"canBan\":false,\"banPermissionIssue\":\"Forbidden\"}");
        var response = this._client.GetPermissionsForRobloxUserAsync("TestDomain", 12345L).Result;
        Assert.That(response.Status, Is.EqualTo(ResponseStatus.Success));
        Assert.That(response.BanPermissionIssue, Is.EqualTo(BanPermissionIssue.Forbidden));
        Assert.That(response.CanBan, Is.False);
    }

    [Test]
    public void TestGetPermissionsForDiscordUserAsync()
    {
        this._testHttpClient.SetResponse("http://localhost:8000/bans/permissions/?domain=TestDomain&linkMethod=Discord&linkData=12345", HttpStatusCode.OK, "{\"status\":\"Success\",\"canBan\":false,\"banPermissionIssue\":\"Forbidden\"}");
        var response = this._client.GetPermissionsForDiscordUserAsync("TestDomain", 12345L).Result;
        Assert.That(response.Status, Is.EqualTo(ResponseStatus.Success));
        Assert.That(response.BanPermissionIssue, Is.EqualTo(BanPermissionIssue.Forbidden));
        Assert.That(response.CanBan, Is.False);
    }

    [Test]
    public void TestLinkDiscordAccountAsync()
    {
        this._testHttpClient.SetResponse("http://localhost:8000/accounts/link", HttpStatusCode.OK, "{\"status\":\"Success\"}");
        var response = this._client.LinkDiscordAccountAsync("TestDomain", 12345L, 23456L).Result;
        Assert.That(response.Status, Is.EqualTo(ResponseStatus.Success));
    }

    [Test]
    public void TestBanAsync()
    {
        this._testHttpClient.SetResponse("http://localhost:8000/bans/ban", HttpStatusCode.OK, "{\"status\":\"Success\"}");
        var response = this._client.BanAsync("TestDomain", BanAction.Ban, 12345L, new List<long>() { 23456 }, "Test Display", "Test Private").Result;
        Assert.That(response.Status, Is.EqualTo(ResponseStatus.Success));
    }
}
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text.Json;
using Bouncer.Test.Web.Client.Shim;
using Bouncer.Web.Client.Shim;
using NUnit.Framework;
using Sovereign.Bans.Games.Configuration;
using Sovereign.Bans.Games.Loop;
using Sovereign.Core.Database;
using Sovereign.Core.Database.Model.Api;
using Sovereign.Core.Model;
using Sovereign.Core.Web.Client;
using Sovereign.Core.Web.Client.Request;

namespace Sovereign.Bans.Games.Test.Loop;

public class GameBanLoopTest
{
    private string _bansContextPath;
    private GameConfiguration _gameConfiguration;
    private TestHttpClient _testHttpClient;
    private HandledBanCache _handledBanCache;
    private GameBanLoop _gameBanLoop;

    [SetUp]
    public void SetUp()
    {
        this._testHttpClient = new TestHttpClient();
        var gameBansContextPath = Path.GetTempFileName();
        using var gameBansContext = new GameBansContext(gameBansContextPath);
        gameBansContext.MigrateAsync().Wait();
        
        this._bansContextPath = Path.GetTempFileName();
        using var bansContext = new BansContext(this._bansContextPath);
        bansContext.MigrateAsync().Wait();
        
        this._gameConfiguration = new GameConfiguration()
        {
            Domain = "TestDomain",
            GameId = 12345,
            ApiKey = "TestApiKey",
        } ;
        this._handledBanCache = new HandledBanCache("TestDomain", 12345, gameBansContextPath);
        this._gameBanLoop = new GameBanLoop(this._gameConfiguration, this._handledBanCache, new RobloxUserRestrictionClient(this._testHttpClient));
        this._gameBanLoop.OverrideBansDatabasePath = this._bansContextPath;
    }

    [Test]
    public void TestHandleBanAsyncWrongDomain()
    {
        this._gameBanLoop.HandleBanAsync(new BanEntry()
        {
            Id = 1,
            Domain = "OtherDomain",
        }).Wait();
        Assert.That(this._handledBanCache.IsHandled(1), Is.False);
    }

    [Test]
    public void TestHandleBanAsyncAlreadyHandled()
    {
        this._handledBanCache.SetHandledAsync(new List<long>() {1}).Wait();
        this._gameBanLoop.HandleBanAsync(new BanEntry()
        {
            Id = 1,
            Domain = "TestDomain",
        }).Wait();
        // An except would be raised if it is handled due to no registered request handler.
    }

    [Test]
    public void TestHandleBanAsyncBanDryRun()
    {
        this._gameConfiguration.DryRun = true;
        this._gameBanLoop.HandleBanAsync(new BanEntry()
        {
            Id = 1,
            Domain = "TestDomain",
            Action = BanAction.Ban,
        }).Wait();
        Assert.That(this._handledBanCache.IsHandled(1), Is.False);
    }

    [Test]
    public void TestHandleBanAsyncUnbanDryRun()
    {
        this._gameConfiguration.DryRun = true;
        this._gameBanLoop.HandleBanAsync(new BanEntry()
        {
            Id = 1,
            Domain = "TestDomain",
            Action = BanAction.Unban,
        }).Wait();
        Assert.That(this._handledBanCache.IsHandled(1), Is.False);
    }

    [Test]
    public void TestHandleBanAsyncPermanentBan()
    {
        UserRestrictionRequest? requestData = null;
        this._testHttpClient.SetResponseResolver("https://apis.roblox.com/cloud/v2/universes/12345/user-restrictions/123",
            (request) =>
            {
                var requestBody = request.Content!.ReadAsStringAsync().Result;
                requestData = JsonSerializer.Deserialize(requestBody, UserRestrictionRequestJsonContext.Default.UserRestrictionRequest)!;

                return new HttpStringResponseMessage()
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = "{}",
                };
            });
        
        this._gameBanLoop.HandleBanAsync(new BanEntry()
        {
            Id = 1,
            Domain = "TestDomain",
            Action = BanAction.Ban,
            ExcludeAltAccounts = true,
            StartTime = DateTime.Now,
            TargetRobloxUserId = 123,
            DisplayReason = "Test Display",
            PrivateReason = "Test Private",
        }).Wait();
        
        Assert.That(this._handledBanCache.IsHandled(1), Is.True);
        Assert.That(requestData!.GameJoinRestriction.Active, Is.True);
        Assert.That(requestData!.GameJoinRestriction.Duration, Is.Null);
        Assert.That(requestData!.GameJoinRestriction.DisplayReason, Is.EqualTo("Test Display"));
        Assert.That(requestData!.GameJoinRestriction.PrivateReason, Is.EqualTo("Test Private"));
        Assert.That(requestData!.GameJoinRestriction.ExcludeAltAccounts, Is.True);
    }
    
    [Test]
    public void TestHandleBanAsyncTemporaryBan()
    {
        UserRestrictionRequest? requestData = null;
        this._testHttpClient.SetResponseResolver("https://apis.roblox.com/cloud/v2/universes/12345/user-restrictions/123",
            (request) =>
            {
                var requestBody = request.Content!.ReadAsStringAsync().Result;
                requestData = JsonSerializer.Deserialize(requestBody, UserRestrictionRequestJsonContext.Default.UserRestrictionRequest)!;

                return new HttpStringResponseMessage()
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = "{}",
                };
            });
        
        this._gameBanLoop.HandleBanAsync(new BanEntry()
        {
            Id = 1,
            Domain = "TestDomain",
            Action = BanAction.Ban,
            StartTime = DateTime.Now,
            EndTime = DateTime.Now + TimeSpan.FromSeconds(5),
            TargetRobloxUserId = 123,
            DisplayReason = "Test Display",
            PrivateReason = "Test Private",
        }).Wait();
        
        Assert.That(this._handledBanCache.IsHandled(1), Is.True);
        Assert.That(requestData!.GameJoinRestriction.Active, Is.True);
        Assert.That(requestData!.GameJoinRestriction.Duration, Is.EqualTo("5s"));
        Assert.That(requestData!.GameJoinRestriction.DisplayReason, Is.EqualTo("Test Display"));
        Assert.That(requestData!.GameJoinRestriction.PrivateReason, Is.EqualTo("Test Private"));
        Assert.That(requestData!.GameJoinRestriction.ExcludeAltAccounts, Is.False);
    }
    
    [Test]
    public void TestHandleBanAsyncUnban()
    {
        UserRestrictionRequest? requestData = null;
        this._testHttpClient.SetResponseResolver("https://apis.roblox.com/cloud/v2/universes/12345/user-restrictions/123",
            (request) =>
            {
                var requestBody = request.Content!.ReadAsStringAsync().Result;
                requestData = JsonSerializer.Deserialize(requestBody, UserRestrictionRequestJsonContext.Default.UserRestrictionRequest)!;

                return new HttpStringResponseMessage()
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = "{}",
                };
            });
        
        this._gameBanLoop.HandleBanAsync(new BanEntry()
        {
            Id = 1,
            Domain = "TestDomain",
            Action = BanAction.Unban,
            StartTime = DateTime.Now,
            TargetRobloxUserId = 123,
            DisplayReason = "Test Display",
            PrivateReason = "Test Private",
        }).Wait();
        
        Assert.That(this._handledBanCache.IsHandled(1), Is.True);
        Assert.That(requestData!.GameJoinRestriction.Active, Is.False);
        Assert.That(requestData!.GameJoinRestriction.Duration, Is.Null);
        Assert.That(requestData!.GameJoinRestriction.DisplayReason, Is.Null);
        Assert.That(requestData!.GameJoinRestriction.PrivateReason, Is.Null);
        Assert.That(requestData!.GameJoinRestriction.ExcludeAltAccounts, Is.False);
    }

    [Test]
    public void TestRunAsyncSingleBan()
    {
        this._testHttpClient.SetResponse("https://apis.roblox.com/cloud/v2/universes/12345/user-restrictions/123", HttpStatusCode.OK, "{}");

        var context = new BansContext(this._bansContextPath);
        context.BanEntries.Add(new BanEntry()
        {
            Id = 1,
            Domain = "TestDomain",
            Action = BanAction.Ban,
            StartTime = DateTime.Now,
            TargetRobloxUserId = 123,
            DisplayReason = "Test Display",
            PrivateReason = "Test Private",
        });
        context.SaveChanges();
        
        Assert.That(this._gameBanLoop.Status, Is.EqualTo(GameBanLoopStatus.NotStarted));
        this._gameBanLoop.RunAsync().Wait();
        Assert.That(this._gameBanLoop.Status, Is.EqualTo(GameBanLoopStatus.Complete));
        Assert.That(this._gameBanLoop.LastSuccessfulIndex, Is.Null);
        Assert.That(this._handledBanCache.IsHandled(1), Is.True);
    }

    [Test]
    public void TestRunAsyncResumeIncomplete()
    {
        this._testHttpClient.SetResponse("https://apis.roblox.com/cloud/v2/universes/12345/user-restrictions/123", HttpStatusCode.OK, "{}");
        this._testHttpClient.SetResponse("https://apis.roblox.com/cloud/v2/universes/12345/user-restrictions/124", HttpStatusCode.OK, "{}");
        this._testHttpClient.SetResponse("https://apis.roblox.com/cloud/v2/universes/12345/user-restrictions/125", HttpStatusCode.TooManyRequests, "{}");

        var context = new BansContext(this._bansContextPath);
        for (var i = 0; i < 5; i++)
        {
            context.BanEntries.Add(new BanEntry()
            {
                Id = i + 1,
                Domain = "TestDomain",
                Action = BanAction.Ban,
                StartTime = DateTime.Now,
                TargetRobloxUserId = 123 + i,
                DisplayReason = "Test Display",
                PrivateReason = "Test Private",
            });
        }
        context.SaveChanges();
        
        Assert.That(this._gameBanLoop.Status, Is.EqualTo(GameBanLoopStatus.NotStarted));
        this._gameBanLoop.RunAsync().Wait();
        Assert.That(this._gameBanLoop.Status, Is.EqualTo(GameBanLoopStatus.TooManyRequests));
        Assert.That(this._gameBanLoop.LastSuccessfulIndex, Is.EqualTo(1));
        Assert.That(this._handledBanCache.IsHandled(1), Is.True);
        Assert.That(this._handledBanCache.IsHandled(2), Is.True);
        Assert.That(this._handledBanCache.IsHandled(3), Is.False);
        Assert.That(this._handledBanCache.IsHandled(4), Is.False);
        Assert.That(this._handledBanCache.IsHandled(5), Is.False);
        
        this._testHttpClient.SetResponse("https://apis.roblox.com/cloud/v2/universes/12345/user-restrictions/123", HttpStatusCode.ServiceUnavailable, "{}");
        this._testHttpClient.SetResponse("https://apis.roblox.com/cloud/v2/universes/12345/user-restrictions/124", HttpStatusCode.ServiceUnavailable, "{}");
        this._testHttpClient.SetResponse("https://apis.roblox.com/cloud/v2/universes/12345/user-restrictions/125", HttpStatusCode.OK, "{}");
        this._testHttpClient.SetResponse("https://apis.roblox.com/cloud/v2/universes/12345/user-restrictions/126", HttpStatusCode.OK, "{}");
        this._testHttpClient.SetResponse("https://apis.roblox.com/cloud/v2/universes/12345/user-restrictions/127", HttpStatusCode.OK, "{}");
        this._gameBanLoop.RunAsync().Wait();
        Assert.That(this._gameBanLoop.Status, Is.EqualTo(GameBanLoopStatus.Complete));
        Assert.That(this._gameBanLoop.LastSuccessfulIndex, Is.Null);
        Assert.That(this._handledBanCache.IsHandled(1), Is.True);
        Assert.That(this._handledBanCache.IsHandled(2), Is.True);
        Assert.That(this._handledBanCache.IsHandled(3), Is.True);
        Assert.That(this._handledBanCache.IsHandled(4), Is.True);
        Assert.That(this._handledBanCache.IsHandled(5), Is.True);
    }
    
    [Test]
    public void TestRunAsyncResumeError()
    {
        this._testHttpClient.SetResponse("https://apis.roblox.com/cloud/v2/universes/12345/user-restrictions/123", HttpStatusCode.OK, "{}");
        this._testHttpClient.SetResponse("https://apis.roblox.com/cloud/v2/universes/12345/user-restrictions/124", HttpStatusCode.OK, "{}");
        this._testHttpClient.SetResponse("https://apis.roblox.com/cloud/v2/universes/12345/user-restrictions/125", HttpStatusCode.ServiceUnavailable, "{}");
        this._testHttpClient.SetResponse("https://apis.roblox.com/cloud/v2/universes/12345/user-restrictions/126", HttpStatusCode.OK, "{}");
        this._testHttpClient.SetResponse("https://apis.roblox.com/cloud/v2/universes/12345/user-restrictions/127", HttpStatusCode.OK, "{}");

        var context = new BansContext(this._bansContextPath);
        for (var i = 0; i < 5; i++)
        {
            context.BanEntries.Add(new BanEntry()
            {
                Id = i + 1,
                Domain = "TestDomain",
                Action = BanAction.Ban,
                StartTime = DateTime.Now,
                TargetRobloxUserId = 123 + i,
                DisplayReason = "Test Display",
                PrivateReason = "Test Private",
            });
        }
        context.SaveChanges();
        
        Assert.That(this._gameBanLoop.Status, Is.EqualTo(GameBanLoopStatus.NotStarted));
        Assert.Throws<AggregateException>(() => this._gameBanLoop.RunAsync().Wait());
        Assert.That(this._gameBanLoop.Status, Is.EqualTo(GameBanLoopStatus.Error));
        Assert.That(this._gameBanLoop.LastSuccessfulIndex, Is.EqualTo(2));
        Assert.That(this._handledBanCache.IsHandled(1), Is.True);
        Assert.That(this._handledBanCache.IsHandled(2), Is.True);
        Assert.That(this._handledBanCache.IsHandled(3), Is.False);
        Assert.That(this._handledBanCache.IsHandled(4), Is.False);
        Assert.That(this._handledBanCache.IsHandled(5), Is.False);
        
        this._gameBanLoop.RunAsync().Wait();
        Assert.That(this._gameBanLoop.Status, Is.EqualTo(GameBanLoopStatus.Complete));
        Assert.That(this._gameBanLoop.LastSuccessfulIndex, Is.Null);
        Assert.That(this._handledBanCache.IsHandled(1), Is.True);
        Assert.That(this._handledBanCache.IsHandled(2), Is.True);
        Assert.That(this._handledBanCache.IsHandled(3), Is.False);
        Assert.That(this._handledBanCache.IsHandled(4), Is.True);
        Assert.That(this._handledBanCache.IsHandled(5), Is.True);
    }
}
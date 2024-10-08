﻿using System;
using System.IO;
using System.Linq;
using System.Net;
using Bouncer.State.Loop;
using Bouncer.Test.Web.Client.Shim;
using Bouncer.Web.Client;
using Bouncer.Web.Client.Response.Group;
using NUnit.Framework;
using Sovereign.Bans.JoinRequests.Configuration;
using Sovereign.Bans.JoinRequests.Loop;
using Sovereign.Core.Database;
using Sovereign.Core.Database.Model.Api;
using Sovereign.Core.Model;

namespace Sovereign.Bans.JoinRequests.Test.Loop;

public class JoinRequestBanLoopTest
{
    private string _bansContextPath;
    private string _joinRequestBansContextPath;
    private TestHttpClient _testHttpClient;
    private JoinRequestsGroupConfiguration _groupConfiguration;
    private JoinRequestBanLoop _joinRequestBanLoopTest;
    
    [SetUp]
    public void SetUp()
    {
        this._testHttpClient = new TestHttpClient();
        this._bansContextPath = Path.GetTempFileName();
        this._joinRequestBansContextPath = Path.GetTempFileName();
        using var bansContext = new BansContext(this._bansContextPath);
        bansContext.MigrateAsync().Wait();
        using var joinRequestBansContext = new JoinRequestBansContext(this._joinRequestBansContextPath);
        joinRequestBansContext.MigrateAsync().Wait();
        
        this._groupConfiguration = new JoinRequestsGroupConfiguration()
        {
            Domain = "TestDomain",
            GroupId = 12345,
            ApiKey = "TestApiKey",
        } ;
        this._joinRequestBanLoopTest = new JoinRequestBanLoop(this._groupConfiguration, new RobloxGroupClient(this._testHttpClient, this._testHttpClient));
        this._joinRequestBanLoopTest.OverrideBansDatabasePath = this._bansContextPath;
        this._joinRequestBanLoopTest.OverrideJoinRequestBansDatabasePath = this._joinRequestBansContextPath;
    }

    [Test]
    public void TestHandleJoinRequestAsyncNoBan()
    {
        Assert.That(this._joinRequestBanLoopTest.HandleJoinRequestAsync(new GroupJoinRequestEntry()
        {
            User = "users/23456",
        }).Result, Is.False);
        
        using var joinRequestBansContext = new JoinRequestBansContext(this._joinRequestBansContextPath);
        Assert.That(joinRequestBansContext.JoinRequestDeclineHistory.FirstOrDefault(), Is.Null);
    }
    
    [Test]
    public void TestHandleJoinRequestAsyncUnrelatedBans()
    {
        using var bansContext = new BansContext(this._bansContextPath);
        bansContext.BanEntries.Add(new BanEntry()
        {
            TargetRobloxUserId = 23456,
            Domain = "OtherDomain",
            Action = BanAction.Ban,
            StartTime = DateTime.Now.AddDays(-1),
            ActingRobloxUserId = 12345,
            DisplayReason = "Test Display",
            PrivateReason = "Test Private",
        });
        bansContext.BanEntries.Add(new BanEntry()
        {
            TargetRobloxUserId = 34567,
            Domain = "TestDomain",
            Action = BanAction.Ban,
            StartTime = DateTime.Now.AddDays(-1),
            ActingRobloxUserId = 12345,
            DisplayReason = "Test Display",
            PrivateReason = "Test Private",
        });
        bansContext.SaveChanges();
        
        Assert.That(this._joinRequestBanLoopTest.HandleJoinRequestAsync(new GroupJoinRequestEntry()
        {
            User = "users/23456",
        }).Result, Is.False);
        
        using var joinRequestBansContext = new JoinRequestBansContext(this._joinRequestBansContextPath);
        Assert.That(joinRequestBansContext.JoinRequestDeclineHistory.FirstOrDefault(), Is.Null);
    }

    [Test]
    public void TestHandleJoinRequestAsyncUnbanned()
    {
        using var bansContext = new BansContext(this._bansContextPath);
        bansContext.BanEntries.Add(new BanEntry()
        {
            TargetRobloxUserId = 23456,
            Domain = "TestDomain",
            Action = BanAction.Ban,
            StartTime = DateTime.Now.AddDays(-2),
            ActingRobloxUserId = 12345,
            DisplayReason = "Test Display",
            PrivateReason = "Test Private",
        });
        bansContext.BanEntries.Add(new BanEntry()
        {
            TargetRobloxUserId = 23456,
            Domain = "TestDomain",
            Action = BanAction.Unban,
            StartTime = DateTime.Now.AddDays(-1),
            ActingRobloxUserId = 12345,
            DisplayReason = "Test Display",
            PrivateReason = "Test Private",
        });
        bansContext.SaveChanges();
        
        Assert.That(this._joinRequestBanLoopTest.HandleJoinRequestAsync(new GroupJoinRequestEntry()
        {
            User = "users/23456",
        }).Result, Is.False);
        
        using var joinRequestBansContext = new JoinRequestBansContext(this._joinRequestBansContextPath);
        Assert.That(joinRequestBansContext.JoinRequestDeclineHistory.FirstOrDefault(), Is.Null);
    }

    [Test]
    public void TestHandleJoinRequestAsyncExpiredBan()
    {
        using var bansContext = new BansContext(this._bansContextPath);
        bansContext.BanEntries.Add(new BanEntry()
        {
            TargetRobloxUserId = 23456,
            Domain = "TestDomain",
            Action = BanAction.Ban,
            StartTime = DateTime.Now.AddDays(-2),
            EndTime = DateTime.Now.AddDays(-1),
            ActingRobloxUserId = 12345,
            DisplayReason = "Test Display",
            PrivateReason = "Test Private",
        });
        bansContext.SaveChanges();
        
        Assert.That(this._joinRequestBanLoopTest.HandleJoinRequestAsync(new GroupJoinRequestEntry()
        {
            User = "users/23456",
        }).Result, Is.False);
        
        using var joinRequestBansContext = new JoinRequestBansContext(this._joinRequestBansContextPath);
        Assert.That(joinRequestBansContext.JoinRequestDeclineHistory.FirstOrDefault(), Is.Null);
    }

    [Test]
    public void TestHandleJoinRequestAsyncDryRun()
    {
        using var bansContext = new BansContext(this._bansContextPath);
        bansContext.BanEntries.Add(new BanEntry()
        {
            Id = 1,
            TargetRobloxUserId = 23456,
            Domain = "TestDomain",
            Action = BanAction.Ban,
            StartTime = DateTime.Now.AddDays(-1),
            ActingRobloxUserId = 12345,
            DisplayReason = "Test Display",
            PrivateReason = "Test Private",
        });
        bansContext.SaveChanges();
        this._groupConfiguration.DryRun = true;
        
        Assert.That(this._joinRequestBanLoopTest.HandleJoinRequestAsync(new GroupJoinRequestEntry()
        {
            User = "users/23456",
        }).Result, Is.True);
        
        using var joinRequestBansContext = new JoinRequestBansContext(this._joinRequestBansContextPath);
        Assert.That(joinRequestBansContext.JoinRequestDeclineHistory.FirstOrDefault(), Is.Null);
    }

    [Test]
    public void TestHandleJoinRequestAsyncActiveBan()
    {
        using var bansContext = new BansContext(this._bansContextPath);
        bansContext.BanEntries.Add(new BanEntry()
        {
            TargetRobloxUserId = 23456,
            Domain = "TestDomain",
            Action = BanAction.Ban,
            StartTime = DateTime.Now.AddDays(-1),
            ActingRobloxUserId = 12345,
            DisplayReason = "Test Display",
            PrivateReason = "Test Private",
        });
        bansContext.SaveChanges();
        
        this._testHttpClient.SetResponse("https://apis.roblox.com/cloud/v2/groups/12345/join-requests/23456:decline", HttpStatusCode.OK, "{}");
        Assert.That(this._joinRequestBanLoopTest.HandleJoinRequestAsync(new GroupJoinRequestEntry()
        {
            User = "users/23456",
        }).Result, Is.True);
        
        using var joinRequestBansContext = new JoinRequestBansContext(this._joinRequestBansContextPath);
        var joinRequestRecord = joinRequestBansContext.JoinRequestDeclineHistory.First();
        Assert.That(joinRequestRecord.BanId, Is.EqualTo(1));
        Assert.That(joinRequestRecord.Domain, Is.EqualTo("TestDomain"));
        Assert.That(joinRequestRecord.GroupId, Is.EqualTo(12345));
        Assert.That(joinRequestRecord.UserId, Is.EqualTo(23456));
        Assert.That((DateTime.Now - joinRequestRecord.Time).TotalSeconds, Is.LessThan(10));
    }
    
    
    [Test]
    public void HandleJoinRequestsFromBanAsyncWrongDomain()
    {
        using var bansContext = new BansContext(this._bansContextPath);
        var banEntry = new BanEntry()
        {
            TargetRobloxUserId = 23456,
            Domain = "UnknownDomain",
            Action = BanAction.Ban,
            StartTime = DateTime.Now.AddDays(-1),
            ActingRobloxUserId = 12345,
            DisplayReason = "Test Display",
            PrivateReason = "Test Private",
        };
        bansContext.BanEntries.Add(banEntry);
        bansContext.SaveChanges();
        
        this._testHttpClient.SetResponse("https://apis.roblox.com/cloud/v2/groups/12345/join-requests?maxPageSize=20&filter=user == 'users/23456'", HttpStatusCode.OK, "{\"groupJoinRequests\":[{\"user\":\"users/23456\"}]}");
        this._joinRequestBanLoopTest.HandleJoinRequestsFromBanAsync(banEntry).Wait();
        
        using var joinRequestBansContext = new JoinRequestBansContext(this._joinRequestBansContextPath);
        Assert.That(joinRequestBansContext.JoinRequestDeclineHistory.FirstOrDefault(), Is.Null);
    }
    
    [Test]
    public void HandleJoinRequestsFromBanAsyncUnban()
    {
        using var bansContext = new BansContext(this._bansContextPath);
        var banEntry = new BanEntry()
        {
            TargetRobloxUserId = 23456,
            Domain = "TestDomain",
            Action = BanAction.Unban,
            StartTime = DateTime.Now.AddDays(-1),
            ActingRobloxUserId = 12345,
            DisplayReason = "Test Display",
            PrivateReason = "Test Private",
        };
        bansContext.BanEntries.Add(banEntry);
        bansContext.SaveChanges();
        
        this._testHttpClient.SetResponse("https://apis.roblox.com/cloud/v2/groups/12345/join-requests?maxPageSize=20&filter=user == 'users/23456'", HttpStatusCode.OK, "{\"groupJoinRequests\":[{\"user\":\"users/23456\"}]}");
        this._joinRequestBanLoopTest.HandleJoinRequestsFromBanAsync(banEntry).Wait();
        
        using var joinRequestBansContext = new JoinRequestBansContext(this._joinRequestBansContextPath);
        Assert.That(joinRequestBansContext.JoinRequestDeclineHistory.FirstOrDefault(), Is.Null);
    }
    
    [Test]
    public void HandleJoinRequestsFromBanAsync()
    {
        using var bansContext = new BansContext(this._bansContextPath);
        var banEntry = new BanEntry()
        {
            TargetRobloxUserId = 23456,
            Domain = "TestDomain",
            Action = BanAction.Ban,
            StartTime = DateTime.Now.AddDays(-1),
            ActingRobloxUserId = 12345,
            DisplayReason = "Test Display",
            PrivateReason = "Test Private",
        };
        bansContext.BanEntries.Add(banEntry);
        bansContext.SaveChanges();
        
        this._testHttpClient.SetResponse("https://apis.roblox.com/cloud/v2/groups/12345/join-requests?maxPageSize=20&filter=user == 'users/23456'", HttpStatusCode.OK, "{\"groupJoinRequests\":[{\"user\":\"users/23456\"}]}");
        this._testHttpClient.SetResponse("https://apis.roblox.com/cloud/v2/groups/12345/join-requests/23456:decline", HttpStatusCode.OK, "{}");
        this._joinRequestBanLoopTest.HandleJoinRequestsFromBanAsync(banEntry).Wait();
        
        using var joinRequestBansContext = new JoinRequestBansContext(this._joinRequestBansContextPath);
        var joinRequestRecord = joinRequestBansContext.JoinRequestDeclineHistory.First();
        Assert.That(joinRequestRecord.BanId, Is.EqualTo(1));
        Assert.That(joinRequestRecord.Domain, Is.EqualTo("TestDomain"));
        Assert.That(joinRequestRecord.GroupId, Is.EqualTo(12345));
        Assert.That(joinRequestRecord.UserId, Is.EqualTo(23456));
        Assert.That((DateTime.Now - joinRequestRecord.Time).TotalSeconds, Is.LessThan(10));
    }

    [Test]
    public void TestRunAsync()
    {
        using var bansContext = new BansContext(this._bansContextPath);
        bansContext.BanEntries.Add(new BanEntry()
        {
            TargetRobloxUserId = 34567,
            Domain = "TestDomain",
            Action = BanAction.Ban,
            StartTime = DateTime.Now.AddDays(-1),
            ActingRobloxUserId = 12345,
            DisplayReason = "Test Display",
            PrivateReason = "Test Private",
        });
        bansContext.SaveChanges();
        
        this._testHttpClient.SetResponse("https://apis.roblox.com/cloud/v2/groups/12345/join-requests?maxPageSize=20", HttpStatusCode.OK, "{\"groupJoinRequests\":[{\"user\":\"users/23456\"}],\"nextPageToken\":\"TestToken\"}");
        this._testHttpClient.SetResponse("https://apis.roblox.com/cloud/v2/groups/12345/join-requests?maxPageSize=20&pageToken=TestToken", HttpStatusCode.OK, "{\"groupJoinRequests\":[{\"user\":\"users/34567\"}]}");
        this._testHttpClient.SetResponse("https://apis.roblox.com/cloud/v2/groups/12345/join-requests/34567:decline", HttpStatusCode.OK, "{}");
        this._joinRequestBanLoopTest.RunAsync().Wait();
        
        using var joinRequestBansContext = new JoinRequestBansContext(this._joinRequestBansContextPath);
        var joinRequestRecord = joinRequestBansContext.JoinRequestDeclineHistory.First();
        Assert.That(joinRequestRecord.BanId, Is.EqualTo(1));
        Assert.That(joinRequestRecord.Domain, Is.EqualTo("TestDomain"));
        Assert.That(joinRequestRecord.GroupId, Is.EqualTo(12345));
        Assert.That(joinRequestRecord.UserId, Is.EqualTo(34567));
        Assert.That((DateTime.Now - joinRequestRecord.Time).TotalSeconds, Is.LessThan(10));
    }
    
    [Test]
    public void TestRunAsyncTooManyRequests()
    {
        this._testHttpClient.SetResponse("https://apis.roblox.com/cloud/v2/groups/12345/join-requests?maxPageSize=20", HttpStatusCode.TooManyRequests, "{}");
        this._joinRequestBanLoopTest.RunAsync().Wait();
        Assert.That(this._joinRequestBanLoopTest.Status, Is.EqualTo(GroupJoinRequestLoopStatus.TooManyRequests));
        
        using var joinRequestBansContext = new JoinRequestBansContext(this._joinRequestBansContextPath);
        Assert.That(joinRequestBansContext.JoinRequestDeclineHistory.FirstOrDefault(), Is.Null);
    }
    
    [Test]
    public void TestRunAsyncInvalidApiKey()
    {
        this._testHttpClient.SetResponse("https://apis.roblox.com/cloud/v2/groups/12345/join-requests?maxPageSize=20", HttpStatusCode.Unauthorized, "{}");
        Assert.Throws<AggregateException>(() =>
        {
            this._joinRequestBanLoopTest.RunAsync().Wait();
        });
        Assert.That(this._joinRequestBanLoopTest.Status, Is.EqualTo(GroupJoinRequestLoopStatus.InvalidApiKey));
        
        using var joinRequestBansContext = new JoinRequestBansContext(this._joinRequestBansContextPath);
        Assert.That(joinRequestBansContext.JoinRequestDeclineHistory.FirstOrDefault(), Is.Null);
    }
    
    [Test]
    public void TestRunAsyncInvalidUnknownError()
    {
        this._testHttpClient.SetResponse("https://apis.roblox.com/cloud/v2/groups/12345/join-requests?maxPageSize=20", HttpStatusCode.InternalServerError, "{}");
        Assert.Throws<AggregateException>(() =>
        {
            this._joinRequestBanLoopTest.RunAsync().Wait();
        });
        Assert.That(this._joinRequestBanLoopTest.Status, Is.EqualTo(GroupJoinRequestLoopStatus.Error));
        
        using var joinRequestBansContext = new JoinRequestBansContext(this._joinRequestBansContextPath);
        Assert.That(joinRequestBansContext.JoinRequestDeclineHistory.FirstOrDefault(), Is.Null);
    }

    [Test]
    public void TestRunAsyncException()
    {
        Assert.Throws<AggregateException>(() =>
        {
            this._joinRequestBanLoopTest.RunAsync().Wait();
        });
        Assert.That(this._joinRequestBanLoopTest.Status, Is.EqualTo(GroupJoinRequestLoopStatus.Error));
        
        using var joinRequestBansContext = new JoinRequestBansContext(this._joinRequestBansContextPath);
        Assert.That(joinRequestBansContext.JoinRequestDeclineHistory.FirstOrDefault(), Is.Null);
    }
}
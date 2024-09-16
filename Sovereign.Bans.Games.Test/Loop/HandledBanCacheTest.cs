using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using Sovereign.Bans.Games.Loop;
using Sovereign.Core.Database;
using Sovereign.Core.Database.Model.Bans;

namespace Sovereign.Bans.Games.Test.Loop;

public class HandledBanCacheTest
{
    private string _databasePath;

    [SetUp]
    public void SetUp()
    {
        this._databasePath = Path.GetTempFileName();
        using var context = new GameBansContext(this._databasePath);
        context.MigrateAsync().Wait();
    }
    
    [Test]
    public void TestIsHandled()
    {
        using var context = new GameBansContext(this._databasePath);
        context.GameBansHistory.Add(new GameBansHistoryEntry()
        {
            Id = 1,
            Domain = "testDomain",
            GameId = 12345,
        });
        context.GameBansHistory.Add(new GameBansHistoryEntry()
        {
            Id = 2,
            Domain = "otherDomain",
            GameId = 12345,
        });
        context.GameBansHistory.Add(new GameBansHistoryEntry()
        {
            Id = 3,
            Domain = "testDomain",
            GameId = 23436,
        });
        context.SaveChanges();

        var handledBanCache = new HandledBanCache("TestDomain", 12345, this._databasePath);
        Assert.That(handledBanCache.IsHandled(1), Is.True);
        Assert.That(handledBanCache.IsHandled(2), Is.False);
        Assert.That(handledBanCache.IsHandled(3), Is.False);
        Assert.That(handledBanCache.IsHandled(4), Is.False);
    }

    [Test]
    public void TestSetHandledAsync()
    {
        using var context = new GameBansContext(this._databasePath);
        context.GameBansHistory.Add(new GameBansHistoryEntry()
        {
            Id = 1,
            Domain = "testDomain",
            GameId = 12345,
        });
        
        var handledBanCache = new HandledBanCache("TestDomain", 12345, this._databasePath);
        handledBanCache.SetHandledAsync(new List<long>() { 1, 2, 3 }).Wait();
        Assert.That(handledBanCache.IsHandled(1), Is.True);
        Assert.That(handledBanCache.IsHandled(2), Is.True);
        Assert.That(handledBanCache.IsHandled(3), Is.True);
        
        using var newContext = new GameBansContext(this._databasePath);
        var handledBans = context.GameBansHistory.ToList();
        Assert.That(handledBans.Count, Is.EqualTo(3));
        Assert.That(handledBans[0].Id, Is.EqualTo(1));
        Assert.That(handledBans[0].Domain, Is.EqualTo("testDomain"));
        Assert.That(handledBans[0].GameId, Is.EqualTo(12345));
        Assert.That(handledBans[1].Id, Is.EqualTo(2));
        Assert.That(handledBans[1].Domain, Is.EqualTo("TestDomain"));
        Assert.That(handledBans[1].GameId, Is.EqualTo(12345));
        Assert.That(handledBans[2].Id, Is.EqualTo(3));
        Assert.That(handledBans[2].Domain, Is.EqualTo("TestDomain"));
        Assert.That(handledBans[2].GameId, Is.EqualTo(12345));
    }
}
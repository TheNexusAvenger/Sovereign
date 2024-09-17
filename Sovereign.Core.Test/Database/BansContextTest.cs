using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using Sovereign.Core.Database;
using Sovereign.Core.Database.Model.Api;

namespace Sovereign.Core.Test.Database;

public class BansContextTest
{
    private BansContext _bansContext;

    [SetUp]
    public void SetUp()
    {
        this._bansContext = new BansContext(Path.GetTempFileName());
        this._bansContext.MigrateAsync().Wait();
    }

    [TearDown]
    public void TearDown()
    {
        this._bansContext.Dispose();
    }
    
    [Test]
    public void TestGetCurrentBans()
    {
        this._bansContext.BanEntries.Add(new BanEntry()
        {
            Domain = "TestDomain",
            TargetRobloxUserId = 12345,
            StartTime = DateTime.Now.AddDays(-4),
            DisplayReason = "Test Display 1",
            PrivateReason = "Test Private 1",
        });
        this._bansContext.BanEntries.Add(new BanEntry()
        {
            Domain = "TestDomain",
            TargetRobloxUserId = 23456,
            StartTime = DateTime.Now.AddDays(-3),
            DisplayReason = "Test Display 2",
            PrivateReason = "Test Private 2",
        });
        this._bansContext.BanEntries.Add(new BanEntry()
        {
            Domain = "TestDomain",
            TargetRobloxUserId = 23456,
            StartTime = DateTime.Now.AddDays(-2),
            DisplayReason = "Test Display 3",
            PrivateReason = "Test Private 3",
        });
        this._bansContext.BanEntries.Add(new BanEntry()
        {
            Domain = "OtherDomain",
            TargetRobloxUserId = 34567,
            StartTime = DateTime.Now.AddDays(-1),
            DisplayReason = "Test Display 4",
            PrivateReason = "Test Private 4",
        });
        this._bansContext.SaveChanges();

        var entries = this._bansContext.GetCurrentBans("testDomain").ToList();
        Assert.That(entries.Count, Is.EqualTo(2));
        Assert.That(entries[0].DisplayReason, Is.EqualTo("Test Display 1"));
        Assert.That(entries[1].DisplayReason, Is.EqualTo("Test Display 3"));
    }
}
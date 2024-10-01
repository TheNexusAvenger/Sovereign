using System.Collections.Generic;
using Bouncer.Web.Server.Model;
using NUnit.Framework;
using Sovereign.Bans.Games.Web.Server.Model;

namespace Sovereign.Bans.Games.Test.Web.Server.Model;

public class GameBansHealthCheckResultTest
{
    [Test]
    public void TestFromLoopHealthResultsUp()
    {
        var loopHealthResults = new List<GameBansGameLoopHealthCheckResult>()
        {
            new GameBansGameLoopHealthCheckResult()
            {
                Status = HealthCheckResultStatus.Up,
            },
            new GameBansGameLoopHealthCheckResult()
            {
                Status = HealthCheckResultStatus.Up,
            },
        };
        var openCloudHealthCheckResults = new List<GameBansOpenCloudHealthCheckResult>()
        {
            new GameBansOpenCloudHealthCheckResult()
            {
                Status = HealthCheckResultStatus.Up,
            },
            new GameBansOpenCloudHealthCheckResult()
            {
                Status = HealthCheckResultStatus.Up,
            },
        };

        var result = GameBansHealthCheckResult.FromLoopHealthResults(loopHealthResults, openCloudHealthCheckResults);
        Assert.That(result.Status, Is.EqualTo(HealthCheckResultStatus.Up));
        Assert.That(result.Games, Is.EqualTo(loopHealthResults));
        Assert.That(result.OpenCloudKeys, Is.EqualTo(openCloudHealthCheckResults));
    }
    
    [Test]
    public void TestFromLoopHealthResultsLoopsDown()
    {
        var loopHealthResults = new List<GameBansGameLoopHealthCheckResult>()
        {
            new GameBansGameLoopHealthCheckResult()
            {
                Status = HealthCheckResultStatus.Up,
            },
            new GameBansGameLoopHealthCheckResult()
            {
                Status = HealthCheckResultStatus.Down,
            },
        };
        var openCloudHealthCheckResults = new List<GameBansOpenCloudHealthCheckResult>()
        {
            new GameBansOpenCloudHealthCheckResult()
            {
                Status = HealthCheckResultStatus.Up,
            },
            new GameBansOpenCloudHealthCheckResult()
            {
                Status = HealthCheckResultStatus.Up,
            },
        };

        var result = GameBansHealthCheckResult.FromLoopHealthResults(loopHealthResults, openCloudHealthCheckResults);
        Assert.That(result.Status, Is.EqualTo(HealthCheckResultStatus.Down));
        Assert.That(result.Games, Is.EqualTo(loopHealthResults));
        Assert.That(result.OpenCloudKeys, Is.EqualTo(openCloudHealthCheckResults));
    }
    
    [Test]
    public void TestFromLoopHealthResultsOpenCloudKeysDown()
    {
        var loopHealthResults = new List<GameBansGameLoopHealthCheckResult>()
        {
            new GameBansGameLoopHealthCheckResult()
            {
                Status = HealthCheckResultStatus.Up,
            },
            new GameBansGameLoopHealthCheckResult()
            {
                Status = HealthCheckResultStatus.Up,
            },
        };
        var openCloudHealthCheckResults = new List<GameBansOpenCloudHealthCheckResult>()
        {
            new GameBansOpenCloudHealthCheckResult()
            {
                Status = HealthCheckResultStatus.Up,
            },
            new GameBansOpenCloudHealthCheckResult()
            {
                Status = HealthCheckResultStatus.Down,
            },
        };

        var result = GameBansHealthCheckResult.FromLoopHealthResults(loopHealthResults, openCloudHealthCheckResults);
        Assert.That(result.Status, Is.EqualTo(HealthCheckResultStatus.Down));
        Assert.That(result.Games, Is.EqualTo(loopHealthResults));
        Assert.That(result.OpenCloudKeys, Is.EqualTo(openCloudHealthCheckResults));
    }
}
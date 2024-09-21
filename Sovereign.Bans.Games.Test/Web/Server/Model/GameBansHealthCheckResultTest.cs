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

        var result = GameBansHealthCheckResult.FromLoopHealthResults(loopHealthResults);
        Assert.That(result.Status, Is.EqualTo(HealthCheckResultStatus.Up));
        Assert.That(result.Games, Is.EqualTo(loopHealthResults));
    }
    
    [Test]
    public void TestFromLoopHealthResultsDown()
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

        var result = GameBansHealthCheckResult.FromLoopHealthResults(loopHealthResults);
        Assert.That(result.Status, Is.EqualTo(HealthCheckResultStatus.Down));
        Assert.That(result.Games, Is.EqualTo(loopHealthResults));
    }
}
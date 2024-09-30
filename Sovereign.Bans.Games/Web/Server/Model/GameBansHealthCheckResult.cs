using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using Bouncer.Web.Server.Model;
using Sovereign.Bans.Games.Loop;

namespace Sovereign.Bans.Games.Web.Server.Model;

public class GameBansGameLoopHealthCheckResult
{
    /// <summary>
    /// Status of the game ban loop.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter<HealthCheckResultStatus>))]
    [JsonPropertyName("status")]
    public HealthCheckResultStatus Status { get; set; } = HealthCheckResultStatus.Up;
    
    /// <summary>
    /// Domain of the game ban loop.
    /// </summary>
    [JsonPropertyName("domain")]
    public string Domain { get; set; } = null!;
    
    /// <summary>
    /// Game id of the game ban loop.
    /// </summary>
    [JsonPropertyName("gameId")]
    public long GameId { get; set; }
    
    /// <summary>
    /// Status of the last step of the game ban loop.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter<GameBanLoopStatus>))]
    [JsonPropertyName("lastStepStatus")]
    public GameBanLoopStatus LastStepStatus { get; set; } = GameBanLoopStatus.NotStarted;
}

public class GameBansHealthCheckResult
{
    /// <summary>
    /// Status of the combined health check.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter<HealthCheckResultStatus>))]
    [JsonPropertyName("status")]
    public HealthCheckResultStatus Status { get; set; } = HealthCheckResultStatus.Up;

    /// <summary>
    /// Summary of the health checks for the game ban loops.
    /// </summary>
    [JsonPropertyName("games")]
    public List<GameBansGameLoopHealthCheckResult> Games = new List<GameBansGameLoopHealthCheckResult>();

    /// <summary>
    /// Creates a health check result from a list of loop health checks.
    /// </summary>
    /// <param name="loopHealthCheckResults">Health check results of the loops.</param>
    /// <returns>Health check result to return in the response.</returns>
    public static GameBansHealthCheckResult FromLoopHealthResults(List<GameBansGameLoopHealthCheckResult> loopHealthCheckResults)
    {
        return new GameBansHealthCheckResult()
        {
            Status = (loopHealthCheckResults.Any(result => result.Status == HealthCheckResultStatus.Down) ? HealthCheckResultStatus.Down : HealthCheckResultStatus.Up),
            Games = loopHealthCheckResults,
        };
    }
}

[JsonSerializable(typeof(GameBansHealthCheckResult))]
[JsonSourceGenerationOptions(WriteIndented = true, IncludeFields = true, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
public partial class GameBansHealthCheckResultJsonContext : JsonSerializerContext
{
}
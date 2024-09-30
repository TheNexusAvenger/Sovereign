using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using Bouncer.State.Loop;
using Bouncer.Web.Server.Model;

namespace Sovereign.Bans.JoinRequests.Web.Server.Model;

public class JoinRequestBansGroupLoopHealthCheckResult
{
    /// <summary>
    /// Status of the join request ban loop.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter<HealthCheckResultStatus>))]
    [JsonPropertyName("status")]
    public HealthCheckResultStatus Status { get; set; } = HealthCheckResultStatus.Up;
    
    /// <summary>
    /// Domain of the join request ban loop.
    /// </summary>
    [JsonPropertyName("domain")]
    public string Domain { get; set; } = null!;
    
    /// <summary>
    /// Group id of the join request ban loop.
    /// </summary>
    [JsonPropertyName("groupId")]
    public long GroupId { get; set; }
    
    /// <summary>
    /// Status of the last step of the join request ban loop.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter<GroupJoinRequestLoopStatus>))]
    [JsonPropertyName("lastStepStatus")]
    public GroupJoinRequestLoopStatus LastStepStatus { get; set; } = GroupJoinRequestLoopStatus.NotStarted;
}

public class JoinRequestBansHealthCheckResult
{
    /// <summary>
    /// Status of the combined health check.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter<HealthCheckResultStatus>))]
    [JsonPropertyName("status")]
    public HealthCheckResultStatus Status { get; set; } = HealthCheckResultStatus.Up;

    /// <summary>
    /// Summary of the health checks for the join request ban loops.
    /// </summary>
    [JsonPropertyName("groups")]
    public List<JoinRequestBansGroupLoopHealthCheckResult> Groups = new List<JoinRequestBansGroupLoopHealthCheckResult>();

    /// <summary>
    /// Creates a health check result from a list of loop health checks.
    /// </summary>
    /// <param name="loopHealthCheckResults">Health check results of the loops.</param>
    /// <returns>Health check result to return in the response.</returns>
    public static JoinRequestBansHealthCheckResult FromLoopHealthResults(List<JoinRequestBansGroupLoopHealthCheckResult> loopHealthCheckResults)
    {
        return new JoinRequestBansHealthCheckResult()
        {
            Status = (loopHealthCheckResults.Any(result => result.Status == HealthCheckResultStatus.Down) ? HealthCheckResultStatus.Down : HealthCheckResultStatus.Up),
            Groups = loopHealthCheckResults,
        };
    }
}

[JsonSerializable(typeof(JoinRequestBansHealthCheckResult))]
[JsonSourceGenerationOptions(WriteIndented = true, IncludeFields = true, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
public partial class JoinRequestBansHealthCheckResultJsonContext : JsonSerializerContext
{
}
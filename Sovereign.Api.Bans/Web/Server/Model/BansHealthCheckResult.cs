using System.Text.Json.Serialization;
using Bouncer.Web.Server.Model;

namespace Sovereign.Api.Bans.Web.Server.Model;

public class BansHealthCheckResult
{
    /// <summary>
    /// Status of the combined health check.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public HealthCheckResultStatus Status { get; set; } = HealthCheckResultStatus.Up;

    /// <summary>
    /// Summary of the health check for the configuration.
    /// </summary>
    public HealthCheckConfigurationProblems Configuration { get; set; } = new HealthCheckConfigurationProblems();
}

[JsonSerializable(typeof(BansHealthCheckResult))]
[JsonSourceGenerationOptions(WriteIndented = true, IncludeFields = true)]
public partial class BansHealthCheckResultJsonContext : JsonSerializerContext
{
}
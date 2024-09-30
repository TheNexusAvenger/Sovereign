using System.Text.Json.Serialization;
using Bouncer.Web.Server.Model;

namespace Sovereign.Discord.Web.Server.Model;

public class DiscordBotHealthCheckResult
{
    /// <summary>
    /// Status of the Discord bot.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter<HealthCheckResultStatus>))]
    [JsonPropertyName("status")]
    public HealthCheckResultStatus Status { get; set; } = HealthCheckResultStatus.Up;
}

public class SovereignApiHealthCheckResult
{
    /// <summary>
    /// Status of the Sovereign API.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter<HealthCheckResultStatus>))]
    [JsonPropertyName("status")]
    public HealthCheckResultStatus Status { get; set; } = HealthCheckResultStatus.Up;
}

public class DiscordHealthCheckResult
{
    /// <summary>
    /// Status of the combined health check.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter<HealthCheckResultStatus>))]
    [JsonPropertyName("status")]
    public HealthCheckResultStatus Status { get; set; } = HealthCheckResultStatus.Up;

    /// <summary>
    /// Summary of the health check for the Discord bot.
    /// </summary>
    [JsonPropertyName("discord")]
    public DiscordBotHealthCheckResult Discord { get; set; } = new DiscordBotHealthCheckResult();

    /// <summary>
    /// Summary of the health check for the Sovereign API.
    /// </summary>
    [JsonPropertyName("sovereign")]
    public SovereignApiHealthCheckResult Sovereign { get; set; } = new SovereignApiHealthCheckResult();
}

[JsonSerializable(typeof(DiscordHealthCheckResult))]
[JsonSourceGenerationOptions(WriteIndented = true, IncludeFields = true, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
public partial class DiscordHealthCheckResultJsonContext : JsonSerializerContext
{
}
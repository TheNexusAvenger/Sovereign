using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Sovereign.Core.Model;
using Sovereign.Core.Model.Response;

namespace Sovereign.Api.Bans.Web.Server.Model;

public class BanResponseEntryAction
{
    /// <summary>
    /// Type of the action to perform.
    /// </summary>
    [JsonPropertyName("type")]
    public BanAction Type { get; set; }
    
    /// <summary>
    /// If true, alt accounts will not be banned or unbanned.
    /// Not all banning outputs support this.
    /// </summary>
    [JsonPropertyName("excludeAltAccounts")]
    public bool ExcludeAltAccounts { get; set; } = false;
    
    /// <summary>
    /// Start time of the action.
    /// </summary>
    [JsonPropertyName("startTime")]
    public DateTime StartTime { get; set; }
    
    /// <summary>
    /// Optional end time of the ban.
    /// </summary>
    [JsonPropertyName("endTime")]
    public DateTime? EndTime { get; set; }
}

public class BanResponseEntryReason
{
    /// <summary>
    /// Roblox user id that authorized the action.
    /// </summary>
    [JsonPropertyName("actingUserId")]
    public long ActingUserId { get; set; }

    /// <summary>
    /// Message that is displayed to the user.
    /// This is only displayed for bans, but is recorded in the database.
    /// </summary>
    [JsonPropertyName("display")]
    public string Display { get; set; } = null!;
    
    /// <summary>
    /// Message that is kept internally for the ban.
    /// This is only stored on Roblox for bans, but is recorded in the database.
    /// </summary>
    [JsonPropertyName("private")]
    public string Private { get; set; } = null!;
}

public class BanRecordResponseEntry
{
    /// <summary>
    /// Action to perform.
    /// </summary>
    [JsonPropertyName("action")]
    public BanResponseEntryAction Action { get; set; } = new BanResponseEntryAction();

    /// <summary>
    /// Reason for the ban or unban.
    /// </summary>
    [JsonPropertyName("reason")]
    public BanResponseEntryReason Reason { get; set; } = new BanResponseEntryReason();
}

public class BanRecordResponse : BaseResponse
{
    /// <summary>
    /// List of ban entries.
    /// </summary>
    [JsonPropertyName("entries")]
    public List<BanRecordResponseEntry> Entries { get; set; } = null!;
    
    /// <summary>
    /// Returns the JSON type information of the response.
    /// </summary>
    /// <returns>The JSON type information of the response.</returns>
    public override JsonTypeInfo GetJsonTypeInfo()
    {
        return BanRecordResponseJsonContext.Default.BanRecordResponse;
    }
}

[JsonSerializable(typeof(BanRecordResponse))]
[JsonSourceGenerationOptions(WriteIndented = true, IncludeFields = true)]
public partial class BanRecordResponseJsonContext : JsonSerializerContext
{
}
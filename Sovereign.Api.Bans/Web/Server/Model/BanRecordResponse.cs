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
    public BanAction Type { get; set; }
    
    /// <summary>
    /// Start time of the action.
    /// </summary>
    public DateTime StartTime { get; set; }
    
    /// <summary>
    /// Optional end time of the ban.
    /// </summary>
    public DateTime? EndTime { get; set; }
}

public class BanResponseEntryReason
{
    /// <summary>
    /// Roblox user id that authorized the action.
    /// </summary>
    public long ActingUserId { get; set; }

    /// <summary>
    /// Message that is displayed to the user.
    /// This is only displayed for bans, but is recorded in the database.
    /// </summary>
    public string Display { get; set; } = null!;
    
    /// <summary>
    /// Message that is kept internally for the ban.
    /// This is only stored on Roblox for bans, but is recorded in the database.
    /// </summary>
    public string Private { get; set; } = null!;
}

public class BanRecordResponseEntry
{
    /// <summary>
    /// Action to perform.
    /// </summary>
    public BanResponseEntryAction Action { get; set; } = new BanResponseEntryAction();

    /// <summary>
    /// Reason for the ban or unban.
    /// </summary>
    public BanResponseEntryReason Reason { get; set; } = new BanResponseEntryReason();
}

public class BanRecordResponse : BaseResponse
{
    /// <summary>
    /// List of ban entries.
    /// </summary>
    public List<BanRecordResponseEntry> Entries { get; set; } = null!;
    
    /// <summary>
    /// Creates a ban record response.
    /// </summary>
    public BanRecordResponse()
    {
        this.Status = "Success";
    }
    
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
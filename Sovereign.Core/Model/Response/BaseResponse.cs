using System.Text.Json.Serialization;

namespace Sovereign.Core.Model.Response;

public class BaseResponse
{
    /// <summary>
    /// Status of the response.
    /// </summary>
    public string Status { get; set; } = "Success";
}

[JsonSerializable(typeof(BaseResponse))]
[JsonSourceGenerationOptions(WriteIndented = true, IncludeFields = true)]
public partial class BaseResponseJsonContext : JsonSerializerContext
{
}
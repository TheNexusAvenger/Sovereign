using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace Sovereign.Core.Model.Response;

public class ValidationError
{
    /// <summary>
    /// Path of the error.
    /// </summary>
    [JsonPropertyName("path")]
    public string? Path { get; set; }
    
    /// <summary>
    /// Message of the error.
    /// </summary>
    [JsonPropertyName("message")]
    public string? Message { get; set; }
}

public class ValidationErrorCheck
{
    /// <summary>
    /// Path of the error.
    /// </summary>
    public string Path { get; set; } = null!;

    /// <summary>
    /// Message of the error.
    /// </summary>
    public string Message { get; set; } = null!;

    /// <summary>
    /// Function for if the response is valid.
    /// </summary>
    public Func<bool> IsValid { get; set; } = null!;
}

public class ValidationErrorResponse : BaseResponse
{
    /// <summary>
    /// Creates a validation error response.
    /// </summary>
    public ValidationErrorResponse()
    {
        this.Status = "ValidationError";
    }

    /// <summary>
    /// Validation errors to return.
    /// </summary>
    [JsonPropertyName("errors")]
    public List<ValidationError> Errors { get; set; } = null!;

    /// <summary>
    /// Creates a validation error response for a set of validation checks.
    /// </summary>
    /// <param name="validations">Request validations to run.</param>
    /// <returns>A validation error response if there is at least 1 validation error.</returns>
    public static ValidationErrorResponse? GetValidationErrorResponse(List<ValidationErrorCheck> validations)
    {
        // Build the validation errors.
        var validationErrors = new List<ValidationError>();
        foreach (var validation in validations)
        {
            if (validation.IsValid.Invoke()) continue;
            validationErrors.Add(new ValidationError()
            {
                Path = validation.Path,
                Message = validation.Message,
            });
        }
        
        // Return a response if there are validation errors.
        if (validationErrors.Count == 0) return null;
        return new ValidationErrorResponse()
        {
            Errors = validationErrors,
        };
    }

    /// <summary>
    /// Returns the JSON type information of the response.
    /// </summary>
    /// <returns>The JSON type information of the response.</returns>
    public override JsonTypeInfo GetJsonTypeInfo()
    {
        return ValidationErrorResponseJsonContext.Default.ValidationErrorResponse;
    }
}

[JsonSerializable(typeof(ValidationErrorResponse))]
[JsonSourceGenerationOptions(WriteIndented = true, IncludeFields = true)]
public partial class ValidationErrorResponseJsonContext : JsonSerializerContext
{
}
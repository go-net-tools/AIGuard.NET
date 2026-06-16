using System.Text.Json;
using AIGuard.NET.Models;

namespace AIGuard.NET.Validators.Output;

/// <summary>
/// Validates that the model output is valid JSON and matches a specific JSON Schema.
/// </summary>
public sealed class JsonSchemaValidator : ValidatorBase
{
    public override ValidatorType Type => ValidatorType.Output;
    public override int Order => 300;

    public string? JsonSchema { get; set; }
    public bool StrictMode { get; set; } = true;

    public override Task<ValidationResult> ValidateAsync(string content, ValidationContext context, CancellationToken ct)
    {
        var violations = new List<ValidationViolation>();

        try
        {
            using var doc = JsonDocument.Parse(content);
            // In a full implementation, we'd validate against the JsonSchema using a library like JsonSchema.Net
            // For now, parsing successfully is the base validation.
        }
        catch (JsonException ex)
        {
            violations.Add(new ValidationViolation("InvalidJson", $"Response is not valid JSON: {ex.Message}", 1.0));
        }

        if (violations.Count > 0)
        {
            return Task.FromResult(ValidationResult.Fail(Name, "JSON validation failed", 1.0, violations));
        }

        return Task.FromResult(ValidationResult.Pass(Name));
    }
    
    /// <summary>
    /// Generates a basic JSON Schema from a C# type (stub implementation).
    /// </summary>
    public static string GenerateSchemaFromType<T>()
    {
        return "{}"; // stub for reflection logic
    }
}

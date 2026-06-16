using System.Text.Json;
using System.ComponentModel.DataAnnotations;
using AIGuard.NET.Models;

namespace AIGuard.NET.Validators.Output;

/// <summary>
/// Validates that the model output can be deserialized into type T and passes data annotations validation.
/// </summary>
public sealed class TypeSchemaValidator<T> : ValidatorBase
{
    public override ValidatorType Type => ValidatorType.Output;
    public override int Order => 310;

    public JsonSerializerOptions? JsonSerializerOptions { get; set; }
    public bool ValidateDataAnnotations { get; set; } = true;

    public override Task<global::AIGuard.NET.Models.ValidationResult> ValidateAsync(string content, global::AIGuard.NET.Models.ValidationContext context, CancellationToken ct)
    {
        var violations = new List<global::AIGuard.NET.Models.ValidationViolation>();

        try
        {
            var obj = JsonSerializer.Deserialize<T>(content, JsonSerializerOptions);
            if (obj == null)
            {
                violations.Add(new global::AIGuard.NET.Models.ValidationViolation("DeserializeFailed", "Deserialized object was null", 1.0));
            }
            else if (ValidateDataAnnotations)
            {
                var valContext = new System.ComponentModel.DataAnnotations.ValidationContext(obj);
                var validationResults = new List<System.ComponentModel.DataAnnotations.ValidationResult>();
                if (!Validator.TryValidateObject(obj, valContext, validationResults, true))
                {
                    foreach (var res in validationResults)
                    {
                        violations.Add(new global::AIGuard.NET.Models.ValidationViolation("DataAnnotation", res.ErrorMessage ?? "Validation failed", 1.0));
                    }
                }
            }
        }
        catch (JsonException ex)
        {
            violations.Add(new global::AIGuard.NET.Models.ValidationViolation("InvalidJson", $"Cannot deserialize to type {typeof(T).Name}: {ex.Message}", 1.0));
        }

        if (violations.Count > 0)
        {
            return Task.FromResult(global::AIGuard.NET.Models.ValidationResult.Fail(Name, $"Type schema validation failed for {typeof(T).Name}", 1.0, violations));
        }

        return Task.FromResult(global::AIGuard.NET.Models.ValidationResult.Pass(Name));
    }
}

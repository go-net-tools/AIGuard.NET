using AIGuard.NET.Models;
using System.Text.RegularExpressions;

namespace AIGuard.NET.Validators.Input;

public enum SensitivityLevel
{
    Low,
    Medium,
    High
}

/// <summary>
/// Detects prompt injection attacks in the input content.
/// </summary>
public sealed class PromptInjectionValidator : ValidatorBase
{
    public override ValidatorType Type => ValidatorType.Input;
    public override int Order => 100;
    
    public SensitivityLevel Sensitivity { get; set; } = SensitivityLevel.Medium;
    public List<string> AdditionalPatterns { get; set; } = [];

    private static readonly string[] InjectionPatterns = 
    [
        "ignore previous instructions",
        "disregard all prior",
        "forget everything above",
        "do not follow",
        "you are now",
        "pretend you are",
        "act as",
        "your new role is",
        "reveal your system prompt",
        "show me your instructions",
        "what are your rules",
        "repeat the above",
        "DAN mode",
        "developer mode",
        "no restrictions",
        "bypass safety",
        "unfiltered mode"
    ];

    public override Task<ValidationResult> ValidateAsync(string content, ValidationContext context, CancellationToken ct)
    {
        var violations = new List<ValidationViolation>();

        foreach (var pattern in InjectionPatterns)
        {
            if (content.Contains(pattern, StringComparison.OrdinalIgnoreCase))
            {
                violations.Add(new ValidationViolation("PromptInjection", $"Injection pattern detected: '{pattern}'", 0.9));
            }
        }

        foreach (var pattern in AdditionalPatterns)
        {
            if (content.Contains(pattern, StringComparison.OrdinalIgnoreCase))
            {
                violations.Add(new ValidationViolation("PromptInjection_Custom", $"Custom injection pattern detected: '{pattern}'", 0.9));
            }
        }

        if (violations.Count > 0)
        {
            return Task.FromResult(ValidationResult.Fail(Name, "Prompt injection detected", 0.95, violations));
        }

        return Task.FromResult(ValidationResult.Pass(Name));
    }
}

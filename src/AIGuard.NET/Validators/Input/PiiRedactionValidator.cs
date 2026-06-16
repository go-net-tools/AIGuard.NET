using System.Text.RegularExpressions;
using AIGuard.NET.Models;

namespace AIGuard.NET.Validators.Input;

/// <summary>
/// Detects and flags personally identifiable information (PII) in the input.
/// </summary>
public sealed partial class PiiRedactionValidator : ValidatorBase
{
    public override ValidatorType Type => ValidatorType.Input;
    public override int Order => 200;

    public char RedactionChar { get; set; } = '*';
    public HashSet<PiiEntityType> EnabledTypes { get; set; } = 
    [
        PiiEntityType.Email, PiiEntityType.Phone, PiiEntityType.SSN, 
        PiiEntityType.CreditCard, PiiEntityType.IpAddress
    ];

    [GeneratedRegex(@"\b[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Z|a-z]{2,}\b")]
    private static partial Regex EmailRegex();

    [GeneratedRegex(@"\b\d{3}-\d{2}-\d{4}\b")]
    private static partial Regex SsnRegex();

    public override Task<ValidationResult> ValidateAsync(string content, ValidationContext context, CancellationToken ct)
    {
        var violations = new List<ValidationViolation>();

        if (EnabledTypes.Contains(PiiEntityType.Email))
        {
            foreach (Match match in EmailRegex().Matches(content))
            {
                violations.Add(new ValidationViolation("PII_Email", "Email address detected", 0.8));
            }
        }

        if (EnabledTypes.Contains(PiiEntityType.SSN))
        {
            foreach (Match match in SsnRegex().Matches(content))
            {
                violations.Add(new ValidationViolation("PII_SSN", "SSN detected", 0.9));
            }
        }

        if (violations.Count > 0)
        {
            return Task.FromResult(ValidationResult.Fail(Name, "PII detected", 1.0, violations));
        }

        return Task.FromResult(ValidationResult.Pass(Name));
    }
}

using AIGuard.NET.Models;

namespace AIGuard.NET.Validators.Output;

/// <summary>
/// Validates that the response length falls within specified character or word limits.
/// </summary>
public sealed class ResponseLengthValidator : ValidatorBase
{
    public override ValidatorType Type => ValidatorType.Output;
    public override int Order => 100;
    
    public int? MinLength { get; set; }
    public int? MaxLength { get; set; }
    public int? MinWordCount { get; set; }
    public int? MaxWordCount { get; set; }

    public override Task<ValidationResult> ValidateAsync(string content, ValidationContext context, CancellationToken ct)
    {
        var violations = new List<ValidationViolation>();

        if (MinLength.HasValue && content.Length < MinLength.Value)
            violations.Add(new ValidationViolation("Length", $"Response length ({content.Length}) is less than minimum ({MinLength.Value})", 0.5));
            
        if (MaxLength.HasValue && content.Length > MaxLength.Value)
            violations.Add(new ValidationViolation("Length", $"Response length ({content.Length}) exceeds maximum ({MaxLength.Value})", 0.5));

        int wordCount = content.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;
        
        if (MinWordCount.HasValue && wordCount < MinWordCount.Value)
            violations.Add(new ValidationViolation("WordCount", $"Response word count ({wordCount}) is less than minimum ({MinWordCount.Value})", 0.5));
            
        if (MaxWordCount.HasValue && wordCount > MaxWordCount.Value)
            violations.Add(new ValidationViolation("WordCount", $"Response word count ({wordCount}) exceeds maximum ({MaxWordCount.Value})", 0.5));

        if (violations.Count > 0)
            return Task.FromResult(ValidationResult.Fail(Name, "Response length validation failed", 1.0, violations));

        return Task.FromResult(ValidationResult.Pass(Name));
    }
}

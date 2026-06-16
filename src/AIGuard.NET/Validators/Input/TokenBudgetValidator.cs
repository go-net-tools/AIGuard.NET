using AIGuard.NET.Models;

namespace AIGuard.NET.Validators.Input;

/// <summary>
/// Methods for estimating token counts.
/// </summary>
public enum EstimationMethod
{
    /// <summary>
    /// Uses character count divided by 4 as a rough estimation.
    /// </summary>
    CharBased,

    /// <summary>
    /// Uses word count multiplied by 1.3 as a rough estimation.
    /// </summary>
    WordBased
}

/// <summary>
/// Enforces a token limit on the input content.
/// </summary>
public sealed class TokenBudgetValidator : ValidatorBase
{
    public override ValidatorType Type => ValidatorType.Input;
    public override int Order => 50;
    
    public int MaxTokens { get; set; } = 4096;
    public EstimationMethod EstimationMethod { get; set; } = EstimationMethod.CharBased;

    public override Task<ValidationResult> ValidateAsync(string content, ValidationContext context, CancellationToken ct)
    {
        int estimatedTokens = EstimationMethod == EstimationMethod.CharBased 
            ? content.Length / 4 
            : (int)(content.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length * 1.3);

        if (estimatedTokens > MaxTokens)
        {
            var violation = new ValidationViolation(
                "TokenBudget", 
                $"Estimated tokens ({estimatedTokens}) exceed max ({MaxTokens})", 
                0.5);
            return Task.FromResult(ValidationResult.Fail(Name, "Token budget exceeded", 1.0, [violation]));
        }

        return Task.FromResult(ValidationResult.Pass(Name));
    }
}

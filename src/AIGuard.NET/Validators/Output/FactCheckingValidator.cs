using AIGuard.NET.Models;
using AIGuard.NET.Retry;

namespace AIGuard.NET.Validators.Output;

/// <summary>
/// Cross-references claims in the model output against external knowledge via an LLM judge.
/// </summary>
public sealed class FactCheckingValidator : ValidatorBase
{
    public override ValidatorType Type => ValidatorType.Output;
    public override int Order => 410;

    private readonly ILlmClient? _evaluatorClient;

    /// <summary>
    /// Initializes a new FactCheckingValidator.
    /// </summary>
    public FactCheckingValidator(ILlmClient? evaluatorClient = null)
    {
        _evaluatorClient = evaluatorClient;
    }

    public override async Task<ValidationResult> ValidateAsync(string content, ValidationContext context, CancellationToken ct)
    {
        if (_evaluatorClient == null)
        {
            return ValidationResult.Pass(Name);
        }

        string prompt = $@"
You are a factual consistency judge.
Review the following model response for factual inaccuracies. Do not assume context outside of common world knowledge.
If all factual claims are true, respond with 'PASS'.
If any factual claims are demonstrably false, respond with 'FAIL: [Explanation]'.

Model Response:
{content}
";

        try
        {
            string evaluation = await _evaluatorClient.GenerateContentAsync(prompt, ct);
            
            if (evaluation.TrimStart().StartsWith("FAIL", StringComparison.OrdinalIgnoreCase))
            {
                var violations = new List<ValidationViolation>
                {
                    new("FactualInconsistency", evaluation, 0.9)
                };
                return ValidationResult.Fail(Name, "Factual inaccuracies detected", 0.9, violations);
            }

            return ValidationResult.Pass(Name);
        }
        catch (Exception ex)
        {
            return ValidationResult.Fail(Name, $"Fact check evaluation failed: {ex.Message}");
        }
    }
}

using AIGuard.NET.Models;
using AIGuard.NET.Retry;

namespace AIGuard.NET.Validators.Output;

/// <summary>
/// Detects hallucinations by verifying if the output is grounded in the provided source documents.
/// Uses an ILlmClient to perform 'LLM-as-a-judge' evaluation.
/// </summary>
public sealed class HallucinationValidator : ValidatorBase
{
    public override ValidatorType Type => ValidatorType.Output;
    public override int Order => 400;

    private readonly ILlmClient? _evaluatorClient;

    /// <summary>
    /// Initializes a new HallucinationValidator with an optional LLM client for evaluation.
    /// </summary>
    public HallucinationValidator(ILlmClient? evaluatorClient = null)
    {
        _evaluatorClient = evaluatorClient;
    }

    public override async Task<ValidationResult> ValidateAsync(string content, ValidationContext context, CancellationToken ct)
    {
        if (context.SourceDocuments == null || context.SourceDocuments.Count == 0)
        {
            // Nothing to ground against, assume pass
            return ValidationResult.Pass(Name);
        }

        if (_evaluatorClient == null)
        {
            // Provide a soft pass if no evaluator is available
            return ValidationResult.Pass(Name);
        }

        string contextDocs = string.Join("\n---\n", context.SourceDocuments);
        string prompt = $@"
You are a strict hallucination detection judge.
Read the source documents and the model's response.
Determine if the model's response contains any claims that are NOT supported by the source documents.

Source Documents:
{contextDocs}

Model Response:
{content}

Respond strictly with 'PASS' if fully grounded, or 'FAIL: [Reason]' if hallucinations exist.
";

        try
        {
            string evaluation = await _evaluatorClient.GenerateContentAsync(prompt, ct);
            
            if (evaluation.TrimStart().StartsWith("FAIL", StringComparison.OrdinalIgnoreCase))
            {
                var violations = new List<ValidationViolation>
                {
                    new("Hallucination", evaluation, 1.0)
                };
                return ValidationResult.Fail(Name, "Hallucination detected in output", 1.0, violations);
            }

            return ValidationResult.Pass(Name);
        }
        catch (Exception ex)
        {
            return ValidationResult.Fail(Name, $"Hallucination evaluation failed: {ex.Message}");
        }
    }
}

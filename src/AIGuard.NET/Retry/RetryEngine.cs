using AIGuard.NET.Models;

namespace AIGuard.NET.Retry;

/// <summary>
/// Orchestrates an automatic feedback loop where invalid AI outputs are sent back
/// to the LLM with error details for self-correction.
/// </summary>
public sealed class RetryEngine
{
    private readonly ILlmClient _client;
    private readonly AIGuard _guard;

    /// <summary>
    /// Initializes a new RetryEngine.
    /// </summary>
    public RetryEngine(ILlmClient client, AIGuard guard)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _guard = guard ?? throw new ArgumentNullException(nameof(guard));
    }

    /// <summary>
    /// Executes a prompt, validating the output, and automatically reprompting the LLM
    /// if violations occur, up to the maximum number of retries.
    /// </summary>
    public async Task<RetryResult> ExecuteWithRetryAsync(
        string initialPrompt, 
        int maxRetries = 3, 
        CancellationToken ct = default)
    {
        var history = new List<GuardResult>();
        string currentPrompt = initialPrompt;
        int attempts = 0;

        while (attempts <= maxRetries)
        {
            attempts++;
            string output = await _client.GenerateContentAsync(currentPrompt, ct);
            
            var validationResult = await _guard.ValidateOutputAsync(output, ct);
            history.Add(validationResult);

            if (validationResult.IsSuccess)
            {
                return new RetryResult(true, output, validationResult, attempts, history.AsReadOnly());
            }

            if (attempts <= maxRetries)
            {
                currentPrompt = GenerateRetryPrompt(initialPrompt, output, validationResult);
            }
        }

        return new RetryResult(false, null, history[^1], attempts - 1, history.AsReadOnly());
    }

    private static string GenerateRetryPrompt(string originalPrompt, string failedOutput, GuardResult result)
    {
        var violations = result.OutputValidations.SelectMany(v => v.Violations);
        var violationList = string.Join("\n", violations.Select(v => $"- [{v.Rule}]: {v.Description}"));

        return $@"{originalPrompt}

=========================================
SYSTEM NOTIFICATION:
Your previous output failed validation due to the following violations:
{violationList}

Please correct these issues and provide a valid response.
=========================================";
    }
}

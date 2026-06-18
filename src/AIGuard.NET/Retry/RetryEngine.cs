using AIGuard.NET.Models;
using System.Text;

namespace AIGuard.NET.Retry;

/// <summary>
/// Orchestrates an automatic feedback loop where invalid AI outputs are sent back
/// to the LLM with error details for self-correction.
/// </summary>
public sealed class RetryEngine
{
    private readonly ILlmClient _client;
    private readonly AIGuard _guard;
    private readonly IRetryPolicy _retryPolicy;

    /// <summary>
    /// Initializes a new RetryEngine with default exponential backoff retry policy.
    /// </summary>
    public RetryEngine(ILlmClient client, AIGuard guard)
        : this(client, guard, new ExponentialBackoffRetryPolicy())
    {
    }

    /// <summary>
    /// Initializes a new RetryEngine with a custom retry policy.
    /// </summary>
    public RetryEngine(ILlmClient client, AIGuard guard, IRetryPolicy retryPolicy)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _guard = guard ?? throw new ArgumentNullException(nameof(guard));
        _retryPolicy = retryPolicy ?? throw new ArgumentNullException(nameof(retryPolicy));
    }

    /// <summary>
    /// Executes a prompt, validating the output, and automatically reprompting the LLM
    /// if violations occur, up to the maximum number of retries.
    /// </summary>
    /// <param name="initialPrompt">The initial prompt to send to the LLM.</param>
    /// <param name="maxRetries">Maximum number of retry attempts. Must be >= 0.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A RetryResult containing the final output, validation result, and attempt history.</returns>
    /// <exception cref="ArgumentException">Thrown if maxRetries is negative.</exception>
    /// <exception cref="ArgumentException">Thrown if initialPrompt is null or empty.</exception>
    public async Task<RetryResult> ExecuteWithRetryAsync(
        string initialPrompt,
        int maxRetries = 3,
        CancellationToken ct = default)
    {
        // Validation
        if (string.IsNullOrWhiteSpace(initialPrompt))
            throw new ArgumentException("Initial prompt cannot be null or empty.", nameof(initialPrompt));

        if (maxRetries < 0)
            throw new ArgumentException("Max retries cannot be negative.", nameof(maxRetries));

        var history = new List<GuardResult>();
        string currentPrompt = initialPrompt;
        int attempt = 0;

        while (attempt < maxRetries + 1)
        {
            attempt++;
            ct.ThrowIfCancellationRequested();

            // Generate content from LLM
            string output = await _client.GenerateContentAsync(currentPrompt, ct);

            // Validate output
            var validationResult = await _guard.ValidateOutputAsync(output, ct);
            history.Add(validationResult);

            // Success case
            if (validationResult.IsSuccess)
            {
                return new RetryResult(true, output, validationResult, attempt, history.AsReadOnly());
            }

            // If we've exhausted retries, return failure
            if (attempt >= maxRetries + 1)
            {
                break;
            }

            // Apply backoff before retry
            var backoffDelay = _retryPolicy.GetBackoffDelay(attempt, validationResult);
            if (backoffDelay > TimeSpan.Zero)
            {
                await Task.Delay(backoffDelay, ct);
            }

            // Generate retry prompt with violation details
            currentPrompt = GenerateRetryPrompt(initialPrompt, output, validationResult);
        }

        // All retries exhausted
        return new RetryResult(
            false,
            null,
            history[^1],
            attempt,
            history.AsReadOnly());
    }

    /// <summary>
    /// Generates a retry prompt that includes the original prompt and detailed violation information.
    /// </summary>
    private static string GenerateRetryPrompt(string originalPrompt, string failedOutput, GuardResult result)
    {
        var violations = result.OutputValidations?.SelectMany(v => v.Violations) ?? Enumerable.Empty<ValidationViolation>();

        if (!violations.Any())
        {
            return originalPrompt; // Fallback if no violations found
        }

        var violationBuilder = new StringBuilder();
        foreach (var violation in violations)
        {
            violationBuilder.AppendLine($"- [{violation.Rule}]: {violation.Description}");
        }

        return $@"{originalPrompt}

=========================================
SYSTEM NOTIFICATION:
Your previous output failed validation due to the following violations:
{violationBuilder.ToString().TrimEnd()}

Please correct these issues and provide a valid response.
=========================================";
    }
}

/// <summary>
/// Defines a strategy for calculating backoff delays between retry attempts.
/// </summary>
public interface IRetryPolicy
{
    /// <summary>
    /// Calculates the delay before the next retry attempt.
    /// </summary>
    /// <param name="attemptNumber">The current attempt number (1-indexed).</param>
    /// <param name="lastValidationResult">The validation result from the last attempt.</param>
    /// <returns>A TimeSpan representing the delay. Zero or negative means no delay.</returns>
    TimeSpan GetBackoffDelay(int attemptNumber, GuardResult lastValidationResult);
}

/// <summary>
/// Implements exponential backoff with a maximum delay cap.
/// </summary>
public sealed class ExponentialBackoffRetryPolicy : IRetryPolicy
{
    private readonly TimeSpan _initialDelay;
    private readonly TimeSpan _maxDelay;
    private readonly double _backoffMultiplier;

    /// <summary>
    /// Initializes a new ExponentialBackoffRetryPolicy with default settings.
    /// Initial delay: 100ms, Max delay: 10s, Multiplier: 2.0
    /// </summary>
    public ExponentialBackoffRetryPolicy()
        : this(TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(10), 2.0)
    {
    }

    /// <summary>
    /// Initializes a new ExponentialBackoffRetryPolicy with custom settings.
    /// </summary>
    public ExponentialBackoffRetryPolicy(TimeSpan initialDelay, TimeSpan maxDelay, double backoffMultiplier)
    {
        if (initialDelay <= TimeSpan.Zero)
            throw new ArgumentException("Initial delay must be positive.", nameof(initialDelay));

        if (maxDelay <= TimeSpan.Zero)
            throw new ArgumentException("Max delay must be positive.", nameof(maxDelay));

        if (backoffMultiplier <= 1.0)
            throw new ArgumentException("Backoff multiplier must be greater than 1.0.", nameof(backoffMultiplier));

        _initialDelay = initialDelay;
        _maxDelay = maxDelay;
        _backoffMultiplier = backoffMultiplier;
    }

    public TimeSpan GetBackoffDelay(int attemptNumber, GuardResult lastValidationResult)
    {
        // Calculate exponential backoff: initialDelay * (multiplier ^ (attemptNumber - 1))
        var delayMs = _initialDelay.TotalMilliseconds * Math.Pow(_backoffMultiplier, attemptNumber - 1);
        var delay = TimeSpan.FromMilliseconds(delayMs);

        // Cap at max delay
        return delay > _maxDelay ? _maxDelay : delay;
    }
}

/// <summary>
/// A no-op retry policy that never applies delay between retries.
/// </summary>
public sealed class NoDelayRetryPolicy : IRetryPolicy
{
    public TimeSpan GetBackoffDelay(int attemptNumber, GuardResult lastValidationResult) => TimeSpan.Zero;
}

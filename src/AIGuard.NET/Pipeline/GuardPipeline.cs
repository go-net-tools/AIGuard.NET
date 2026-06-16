// Copyright (c) go-net-tools 2026. Licensed under the MIT License.

using System.Diagnostics;
using AIGuard.NET.Models;
using AIGuard.NET.Validators;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace AIGuard.NET.Pipeline;

/// <summary>
/// Core pipeline engine that orchestrates the ordered execution of
/// <see cref="IGuardValidator"/> instances against textual content.
/// </summary>
/// <remarks>
/// <para>
/// Validators are partitioned by <see cref="ValidatorType"/> and executed in
/// ascending <see cref="IGuardValidator.Order"/>. Input validators run before
/// output validators when <see cref="ExecuteAsync"/> is called.
/// </para>
/// <para>
/// The pipeline respects <see cref="AIGuardOptions.StopOnFirstFailure"/>,
/// <see cref="AIGuardOptions.Timeout"/>, and <see cref="AIGuardOptions.MaxRetries"/>.
/// </para>
/// </remarks>
public sealed class GuardPipeline
{
    private readonly IReadOnlyList<IGuardValidator> _validators;
    private readonly AIGuardOptions _options;
    private readonly ILogger<GuardPipeline> _logger;

    /// <summary>
    /// Initializes a new <see cref="GuardPipeline"/>.
    /// </summary>
    /// <param name="validators">
    /// The ordered collection of validators to execute. Must not be <see langword="null"/>.
    /// </param>
    /// <param name="options">Pipeline configuration options.</param>
    /// <param name="logger">
    /// Optional logger instance. When <see langword="null"/>, a no-op logger is used.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="validators"/> or <paramref name="options"/> is <see langword="null"/>.
    /// </exception>
    public GuardPipeline(
        IReadOnlyList<IGuardValidator> validators,
        AIGuardOptions options,
        ILogger<GuardPipeline>? logger = null)
    {
        _validators = validators ?? throw new ArgumentNullException(nameof(validators));
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? NullLogger<GuardPipeline>.Instance;
    }

    /// <summary>
    /// Executes both input and output validators against the supplied content.
    /// Input validators run first (ordered by <see cref="IGuardValidator.Order"/>),
    /// followed by output validators.
    /// </summary>
    /// <param name="input">The text to validate.</param>
    /// <param name="context">
    /// Optional <see cref="ValidationContext"/> carrying additional metadata.
    /// A default context is created when <see langword="null"/>.
    /// </param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A <see cref="GuardResult"/> summarising the full pipeline run.</returns>
    public async Task<GuardResult> ExecuteAsync(
        string input,
        ValidationContext? context = null,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(input);

        var stopwatch = Stopwatch.StartNew();
        using var timeoutCts = CreateTimeoutCts(ct);
        var effectiveCt = timeoutCts.Token;
        var resolvedContext = context ?? ValidationContext.Default;

        _logger.LogDebug("Pipeline ExecuteAsync started with {ValidatorCount} validators", _validators.Count);

        var (inputResults, inputTerminated) = await RunValidatorsAsync(
            ValidatorType.Input, input, resolvedContext, effectiveCt).ConfigureAwait(false);

        List<ValidationResult> outputResults;
        if (inputTerminated)
        {
            _logger.LogInformation("Pipeline terminated early during input validation; skipping output validators");
            outputResults = [];
        }
        else
        {
            (outputResults, _) = await RunValidatorsAsync(
                ValidatorType.Output, input, resolvedContext, effectiveCt).ConfigureAwait(false);
        }

        stopwatch.Stop();

        var result = BuildGuardResult(input, inputResults, outputResults, stopwatch.Elapsed);

        if (_options.EnableAuditLog)
        {
            LogAudit(result);
        }

        _logger.LogDebug(
            "Pipeline ExecuteAsync completed in {Elapsed}ms — Success: {IsSuccess}",
            result.Elapsed.TotalMilliseconds,
            result.IsSuccess);

        return result;
    }

    /// <summary>
    /// Runs only the input-side validators (those with <see cref="ValidatorType.Input"/>
    /// or <see cref="ValidatorType.Both"/>).
    /// </summary>
    /// <param name="input">The text to validate as input.</param>
    /// <param name="context">Optional validation context.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A <see cref="GuardResult"/> with only input validation results populated.</returns>
    public async Task<GuardResult> ValidateInputAsync(
        string input,
        ValidationContext? context = null,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(input);

        var stopwatch = Stopwatch.StartNew();
        using var timeoutCts = CreateTimeoutCts(ct);
        var resolvedContext = context ?? ValidationContext.Default;

        _logger.LogDebug("Pipeline ValidateInputAsync started");

        var (inputResults, _) = await RunValidatorsAsync(
            ValidatorType.Input, input, resolvedContext, timeoutCts.Token).ConfigureAwait(false);

        stopwatch.Stop();

        var result = BuildGuardResult(input, inputResults, [], stopwatch.Elapsed);

        if (_options.EnableAuditLog)
        {
            LogAudit(result);
        }

        return result;
    }

    /// <summary>
    /// Runs only the output-side validators (those with <see cref="ValidatorType.Output"/>
    /// or <see cref="ValidatorType.Both"/>).
    /// </summary>
    /// <param name="output">The text to validate as output.</param>
    /// <param name="context">Optional validation context.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A <see cref="GuardResult"/> with only output validation results populated.</returns>
    public async Task<GuardResult> ValidateOutputAsync(
        string output,
        ValidationContext? context = null,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(output);

        var stopwatch = Stopwatch.StartNew();
        using var timeoutCts = CreateTimeoutCts(ct);
        var resolvedContext = context ?? ValidationContext.Default;

        _logger.LogDebug("Pipeline ValidateOutputAsync started");

        var (outputResults, _) = await RunValidatorsAsync(
            ValidatorType.Output, output, resolvedContext, timeoutCts.Token).ConfigureAwait(false);

        stopwatch.Stop();

        var result = BuildGuardResult(output, [], outputResults, stopwatch.Elapsed);

        if (_options.EnableAuditLog)
        {
            LogAudit(result);
        }

        return result;
    }

    // ── Private helpers ──────────────────────────────────────────────────

    /// <summary>
    /// Runs the validators matching <paramref name="phase"/> in ascending order,
    /// applying retry logic, confidence filtering, and early-termination.
    /// </summary>
    private async Task<(List<ValidationResult> Results, bool Terminated)> RunValidatorsAsync(
        ValidatorType phase,
        string content,
        ValidationContext context,
        CancellationToken ct)
    {
        var applicable = _validators
            .Where(v => v.Type == phase || v.Type == ValidatorType.Both)
            .OrderBy(v => v.Order)
            .ToList();

        var results = new List<ValidationResult>(applicable.Count);
        var terminated = false;

        foreach (var validator in applicable)
        {
            ct.ThrowIfCancellationRequested();

            _logger.LogTrace(
                "Running validator '{ValidatorName}' (Order={Order}, Phase={Phase})",
                validator.Name, validator.Order, phase);

            var result = await ExecuteWithRetryAsync(validator, content, context, ct).ConfigureAwait(false);

            // Confidence filter — flag low-confidence results as warnings.
            if (result.Confidence < _options.MinConfidenceThreshold)
            {
                _logger.LogWarning(
                    "Validator '{ValidatorName}' returned confidence {Confidence:F2} " +
                    "below threshold {Threshold:F2} — result treated as inconclusive",
                    validator.Name, result.Confidence, _options.MinConfidenceThreshold);
            }

            results.Add(result);

            // Invoke violation callback when violations are present.
            if (!result.IsValid && result.Violations.Count > 0)
            {
                try
                {
                    _options.OnViolationDetected?.Invoke(result);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "OnViolationDetected callback threw for validator '{ValidatorName}'", validator.Name);
                }
            }

            // Early termination on first failure (if configured).
            if (!result.IsValid && _options.StopOnFirstFailure)
            {
                _logger.LogInformation(
                    "StopOnFirstFailure is enabled — halting pipeline after '{ValidatorName}' failure",
                    validator.Name);
                terminated = true;
                break;
            }
        }

        return (results, terminated);
    }

    /// <summary>
    /// Executes a single validator with retry semantics.
    /// </summary>
    private async Task<ValidationResult> ExecuteWithRetryAsync(
        IGuardValidator validator,
        string content,
        ValidationContext context,
        CancellationToken ct)
    {
        var maxAttempts = _options.MaxRetries + 1;

        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                return await validator.ValidateAsync(content, context, ct).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                // Propagate genuine cancellation (timeout or caller-initiated).
                throw;
            }
            catch (Exception ex) when (attempt < maxAttempts)
            {
                _logger.LogWarning(
                    ex,
                    "Validator '{ValidatorName}' failed on attempt {Attempt}/{MaxAttempts} — retrying",
                    validator.Name, attempt, maxAttempts);

                // Exponential back-off: 100ms, 200ms, 400ms …
                var delay = TimeSpan.FromMilliseconds(100 * Math.Pow(2, attempt - 1));
                await Task.Delay(delay, ct).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Validator '{ValidatorName}' failed after {MaxAttempts} attempts",
                    validator.Name, maxAttempts);

                // Return a failure result so the pipeline keeps a record.
                return ValidationResult.Fail(
                    validator.Name,
                    $"Validator threw after {maxAttempts} attempt(s): {ex.Message}");
            }
        }

        // Unreachable, but satisfies the compiler.
        return ValidationResult.Fail(validator.Name, "Unexpected retry loop exit");
    }

    /// <summary>
    /// Creates a linked <see cref="CancellationTokenSource"/> that will cancel
    /// after the configured <see cref="AIGuardOptions.Timeout"/>.
    /// </summary>
    private CancellationTokenSource CreateTimeoutCts(CancellationToken externalToken)
    {
        var cts = CancellationTokenSource.CreateLinkedTokenSource(externalToken);
        cts.CancelAfter(_options.Timeout);
        return cts;
    }

    /// <summary>
    /// Builds the final <see cref="GuardResult"/> from the collected validation results.
    /// </summary>
    private static GuardResult BuildGuardResult(
        string originalInput,
        List<ValidationResult> inputResults,
        List<ValidationResult> outputResults,
        TimeSpan elapsed)
    {
        var allValid = inputResults.TrueForAll(r => r.IsValid)
                    && outputResults.TrueForAll(r => r.IsValid);

        return new GuardResult
        {
            IsSuccess = allValid,
            SanitizedInput = originalInput,
            ValidatedOutput = null,
            InputValidations = inputResults.AsReadOnly(),
            OutputValidations = outputResults.AsReadOnly(),
            Elapsed = elapsed,
            RetryCount = 0
        };
    }

    /// <summary>
    /// Emits a structured audit-log entry for a completed pipeline run.
    /// </summary>
    private void LogAudit(GuardResult result)
    {
        _logger.LogInformation(
            "[AUDIT] Pipeline run completed — Success={IsSuccess}, " +
            "InputValidations={InputCount}, OutputValidations={OutputCount}, " +
            "Elapsed={Elapsed}ms",
            result.IsSuccess,
            result.InputValidations.Count,
            result.OutputValidations.Count,
            result.Elapsed.TotalMilliseconds);
    }
}

// Copyright (c) go-net-tools 2026. Licensed under the MIT License.

using AIGuard.NET.Models;

namespace AIGuard.NET;

/// <summary>
/// Configuration options for the AIGuard validation pipeline.
/// </summary>
/// <remarks>
/// Configure these values via the builder's fluent API methods
/// such as WithStopOnFirstFailure, WithMaxRetries, etc.
/// All properties carry production-safe defaults.
/// </remarks>
public sealed class AIGuardOptions
{
    /// <summary>
    /// When <see langword="true"/>, the pipeline will stop executing remaining validators
    /// as soon as a validator returns an invalid result.
    /// Default: <see langword="false"/>.
    /// </summary>
    public bool StopOnFirstFailure { get; set; }

    /// <summary>
    /// The default action to take when a threat or policy violation is detected
    /// and no validator-specific action has been configured.
    /// Default: <see cref="ThreatAction.Block"/>.
    /// </summary>
    public ThreatAction DefaultThreatAction { get; set; } = ThreatAction.Block;

    /// <summary>
    /// Maximum number of automatic retries when a transient validator failure occurs.
    /// Default: <c>0</c> (no retries).
    /// </summary>
    /// <remarks>
    /// Retries apply to validators that throw exceptions, not to validation failures.
    /// The pipeline will wait a short back-off period between retries.
    /// </remarks>
    public int MaxRetries { get; set; }

    /// <summary>
    /// Maximum wall-clock time the pipeline is allowed to spend executing all validators
    /// before cancellation is requested.
    /// Default: <c>30 seconds</c>.
    /// </summary>
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// When <see langword="true"/>, the pipeline emits structured audit-log entries
    /// for every validation run.
    /// Default: <see langword="false"/>.
    /// </summary>
    public bool EnableAuditLog { get; set; }

    /// <summary>
    /// The minimum confidence score (0.0–1.0) a validator must report for a validation
    /// result to be considered meaningful. Results below this threshold are treated as
    /// inconclusive and logged as warnings.
    /// Default: <c>0.7</c>.
    /// </summary>
    public double MinConfidenceThreshold { get; set; } = 0.7;

    /// <summary>
    /// Optional callback invoked each time a <see cref="ValidationResult"/> reports
    /// one or more violations. Useful for telemetry, alerting, or external audit sinks.
    /// </summary>
    public Action<ValidationResult>? OnViolationDetected { get; set; }
}

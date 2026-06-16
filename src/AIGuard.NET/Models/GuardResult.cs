namespace AIGuard.NET.Models;

/// <summary>
/// Represents the aggregate result of running all validators in a guard pipeline.
/// </summary>
public sealed class GuardResult
{
    /// <summary>
    /// Gets or sets whether all validations passed successfully.
    /// </summary>
    public bool IsSuccess { get; set; }

    /// <summary>
    /// Gets or sets the sanitized input (if modified by validators like PII redaction).
    /// </summary>
    public string? SanitizedInput { get; set; }

    /// <summary>
    /// Gets or sets the validated output.
    /// </summary>
    public string? ValidatedOutput { get; set; }

    /// <summary>
    /// Gets or sets the input validation results.
    /// </summary>
    public IReadOnlyList<ValidationResult> InputValidations { get; set; } = [];

    /// <summary>
    /// Gets or sets the output validation results.
    /// </summary>
    public IReadOnlyList<ValidationResult> OutputValidations { get; set; } = [];

    /// <summary>
    /// Gets or sets the elapsed time for the validation.
    /// </summary>
    public TimeSpan Elapsed { get; set; }

    /// <summary>
    /// Gets or sets the total number of retries attempted.
    /// </summary>
    public int RetryCount { get; set; }

    /// <summary>
    /// Gets whether there are any input violations.
    /// </summary>
    public bool HasInputViolations => InputValidations.Any(v => !v.IsValid);

    /// <summary>
    /// Gets whether there are any output violations.
    /// </summary>
    public bool HasOutputViolations => OutputValidations.Any(v => !v.IsValid);

    /// <summary>
    /// Gets all violations across both input and output.
    /// </summary>
    public IReadOnlyList<ValidationViolation> AllViolations => 
        InputValidations.SelectMany(v => v.Violations)
        .Concat(OutputValidations.SelectMany(v => v.Violations))
        .ToList();
}

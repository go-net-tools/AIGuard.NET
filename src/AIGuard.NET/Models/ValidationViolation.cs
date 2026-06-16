namespace AIGuard.NET.Models;

/// <summary>
/// Represents a specific rule violation detected during validation.
/// </summary>
/// <param name="Rule">The name or identifier of the rule that was violated.</param>
/// <param name="Description">A human-readable description of the violation.</param>
/// <param name="Severity">
/// The severity of the violation on a continuous scale from 0.0 (informational) to 1.0 (critical).
/// Values outside the range are automatically clamped.
/// </param>
/// <param name="SuggestedFix">
/// An optional suggestion for how to fix or remediate the violation.
/// </param>
/// <param name="Position">
/// The optional zero-based character position in the content where the violation starts.
/// </param>
/// <param name="Length">
/// The optional length (in characters) of the offending content span.
/// </param>
public sealed record ValidationViolation(
    string Rule,
    string Description,
    double Severity,
    string? SuggestedFix = null,
    int? Position = null,
    int? Length = null)
{
    /// <summary>
    /// Gets the severity of the violation, clamped to the [0, 1] range.
    /// </summary>
    public double Severity { get; init; } = Math.Clamp(Severity, 0.0, 1.0);
}

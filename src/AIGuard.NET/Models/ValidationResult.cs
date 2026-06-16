namespace AIGuard.NET.Models;

/// <summary>
/// Represents the result of a validation operation performed by a single validator.
/// </summary>
/// <param name="IsValid">Whether the validation passed.</param>
/// <param name="Message">An optional summary message describing the result.</param>
/// <param name="Confidence">
/// A confidence score between 0.0 and 1.0 indicating how confident the validator is in its result.
/// A value of 1.0 means absolute certainty. Values outside the range are automatically clamped.
/// </param>
/// <param name="ValidatorName">The name of the validator that produced this result.</param>
/// <param name="Violations">The list of violations detected during validation, if any.</param>
public sealed record ValidationResult(
    bool IsValid,
    string? Message,
    double Confidence,
    string ValidatorName,
    IReadOnlyList<ValidationViolation> Violations)
{
    /// <summary>
    /// Gets the confidence score, clamped to the [0, 1] range.
    /// </summary>
    public double Confidence { get; init; } = Math.Clamp(Confidence, 0.0, 1.0);

    /// <summary>
    /// Creates a passing validation result with no violations and full confidence.
    /// </summary>
    /// <param name="validatorName">The name of the validator.</param>
    /// <returns>A passing <see cref="ValidationResult"/>.</returns>
    public static ValidationResult Pass(string validatorName) =>
        new(true, null, 1.0, validatorName, []);

    /// <summary>
    /// Creates a failing validation result with the specified message and violations.
    /// </summary>
    /// <param name="validatorName">The name of the validator.</param>
    /// <param name="message">A summary message describing the failure.</param>
    /// <param name="confidence">The confidence score for the failure determination (defaults to 1.0).</param>
    /// <param name="violations">The list of violations detected. If <c>null</c>, an empty list is used.</param>
    /// <returns>A failing <see cref="ValidationResult"/>.</returns>
    public static ValidationResult Fail(
        string validatorName,
        string message,
        double confidence = 1.0,
        IReadOnlyList<ValidationViolation>? violations = null) =>
        new(false, message, confidence, validatorName, violations ?? []);
}

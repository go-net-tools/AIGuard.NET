namespace AIGuard.NET.Models;

/// <summary>
/// Represents the result of a PII redaction operation, containing the sanitized text
/// and details of all detected PII entities.
/// </summary>
/// <param name="OriginalText">The original, unmodified input text.</param>
/// <param name="SanitizedText">The text after all detected PII entities have been redacted.</param>
/// <param name="DetectedEntities">The collection of PII entities that were detected and redacted.</param>
public sealed record PiiRedactionResult(
    string OriginalText,
    string SanitizedText,
    IReadOnlyList<PiiEntity> DetectedEntities)
{
    /// <summary>
    /// Gets a value indicating whether any PII entities were detected in the original text.
    /// </summary>
    public bool HasDetections => DetectedEntities.Count > 0;
}

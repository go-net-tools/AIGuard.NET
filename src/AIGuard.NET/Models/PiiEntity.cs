namespace AIGuard.NET.Models;

/// <summary>
/// Represents a single PII entity detected within content.
/// </summary>
/// <param name="Type">The classification of the PII entity.</param>
/// <param name="OriginalValue">The original, unredacted value found in the content.</param>
/// <param name="RedactedValue">
/// The redacted or masked replacement value (e.g., "***-**-1234" for an SSN).
/// </param>
/// <param name="StartIndex">The zero-based character index where the entity starts in the original text.</param>
/// <param name="Length">The character length of the original entity value.</param>
public sealed record PiiEntity(
    PiiEntityType Type,
    string OriginalValue,
    string RedactedValue,
    int StartIndex,
    int Length);

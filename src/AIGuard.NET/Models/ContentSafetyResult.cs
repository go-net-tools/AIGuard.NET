namespace AIGuard.NET.Models;

/// <summary>
/// Represents the result of a content safety analysis, with per-category scores
/// and a list of categories that exceeded the configured safety threshold.
/// </summary>
/// <param name="IsSafe">Whether the content is considered safe based on the configured threshold.</param>
/// <param name="CategoryScores">
/// Per-category safety scores, each normalized between 0.0 (safe) and 1.0 (most unsafe).
/// Keys are category names (e.g., "hate", "violence", "sexual", "self-harm").
/// </param>
/// <param name="FlaggedCategories">The set of category names that exceeded the safety threshold.</param>
/// <param name="OverallScore">
/// The overall safety risk score, normalized between 0.0 (safe) and 1.0 (most unsafe).
/// </param>
public sealed record ContentSafetyResult(
    bool IsSafe,
    IReadOnlyDictionary<string, double> CategoryScores,
    IReadOnlyList<string> FlaggedCategories,
    double OverallScore)
{
    /// <summary>
    /// Gets the overall score clamped to the [0, 1] range.
    /// </summary>
    public double OverallScore { get; init; } = Math.Clamp(OverallScore, 0.0, 1.0);
}

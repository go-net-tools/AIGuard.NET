namespace AIGuard.NET.Models;

/// <summary>
/// Specifies the action to take when a threat or policy violation is detected.
/// </summary>
public enum ThreatAction
{
    /// <summary>
    /// Allow the content to pass through without modification.
    /// </summary>
    Allow,

    /// <summary>
    /// Block the content entirely and prevent further processing.
    /// </summary>
    Block,

    /// <summary>
    /// Sanitize the content by removing or redacting the offending portions.
    /// </summary>
    Sanitize,

    /// <summary>
    /// Log the threat for auditing purposes but allow the content to proceed.
    /// </summary>
    Log
}

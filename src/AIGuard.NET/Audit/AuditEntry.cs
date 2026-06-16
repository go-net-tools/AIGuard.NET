using AIGuard.NET.Models;

namespace AIGuard.NET.Audit;

/// <summary>
/// Represents an entry in the AI guard audit log.
/// </summary>
public sealed record AuditEntry(
    DateTimeOffset Timestamp,
    string Action,
    string? Input,
    string? Output,
    GuardResult? Result,
    TimeSpan Elapsed,
    Dictionary<string, object?> Metadata
);

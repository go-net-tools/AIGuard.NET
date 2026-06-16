using AIGuard.NET.Models;

namespace AIGuard.NET.Retry;

/// <summary>
/// Represents the final outcome of a retry-enabled guard execution.
/// </summary>
public sealed record RetryResult(
    bool IsSuccess,
    string? FinalOutput,
    GuardResult FinalValidation,
    int TotalAttempts,
    IReadOnlyList<GuardResult> History
);

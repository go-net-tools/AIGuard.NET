namespace AIGuard.NET.Retry;

/// <summary>
/// Abstraction for calling a Large Language Model (LLM).
/// Used by the RetryEngine to request corrections.
/// </summary>
public interface ILlmClient
{
    /// <summary>
    /// Generates content from the given prompt.
    /// </summary>
    Task<string> GenerateContentAsync(string prompt, CancellationToken ct = default);
}

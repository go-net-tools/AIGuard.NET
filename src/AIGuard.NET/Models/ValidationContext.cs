namespace AIGuard.NET.Models;

/// <summary>
/// Provides contextual information to validators during validation execution.
/// </summary>
public sealed class ValidationContext
{
    /// <summary>
    /// Gets a default, empty validation context.
    /// </summary>
    public static readonly ValidationContext Default = new();

    /// <summary>
    /// Gets or sets the original input prompt sent to the AI model.
    /// </summary>
    public string? OriginalInput { get; set; }

    /// <summary>
    /// Gets or sets the model identifier (e.g., "gpt-4", "claude-3").
    /// </summary>
    public string? ModelId { get; set; }

    /// <summary>
    /// Gets or sets the source documents provided to the model (used for hallucination detection).
    /// </summary>
    public IReadOnlyList<string>? SourceDocuments { get; set; }

    /// <summary>
    /// Gets or sets the conversation or request identifier for correlation.
    /// </summary>
    public string? ConversationId { get; set; }

    /// <summary>
    /// Gets the additional metadata dictionary for extensibility.
    /// </summary>
    public Dictionary<string, object?> Metadata { get; } = [];

    /// <summary>
    /// Gets the items dictionary for passing data between validators in a pipeline.
    /// </summary>
    public Dictionary<string, object?> Items { get; } = [];
}

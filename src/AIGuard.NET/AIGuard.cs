using AIGuard.NET.Models;
using AIGuard.NET.Pipeline;

namespace AIGuard.NET;

/// <summary>
/// Main orchestrator class for AIGuard.NET validation.
/// </summary>
public sealed class AIGuard
{
    private readonly GuardPipeline _pipeline;
    private readonly AIGuardOptions _options;

    /// <summary>
    /// Initializes a new instance of the AIGuard class.
    /// </summary>
    public AIGuard(GuardPipeline pipeline, AIGuardOptions options)
    {
        _pipeline = pipeline ?? throw new ArgumentNullException(nameof(pipeline));
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    /// <summary>
    /// Creates a builder to construct an AIGuard instance.
    /// </summary>
    public static AIGuardBuilder CreateBuilder() => new();

    /// <summary>
    /// Validates both input and output.
    /// </summary>
    public Task<GuardResult> ExecuteAsync(string input, CancellationToken ct = default)
    {
        return _pipeline.ExecuteAsync(input, null, ct);
    }

    /// <summary>
    /// Validates the given input content against input validators.
    /// </summary>
    public Task<GuardResult> ValidateInputAsync(string input, CancellationToken ct = default)
    {
        return _pipeline.ValidateInputAsync(input, null, ct);
    }

    /// <summary>
    /// Validates the given output content against output validators.
    /// </summary>
    public Task<GuardResult> ValidateOutputAsync(string output, CancellationToken ct = default)
    {
        return _pipeline.ValidateOutputAsync(output, null, ct);
    }

    /// <summary>
    /// Convenience method for redacting PII from text.
    /// </summary>
    public async Task<PiiRedactionResult> RedactPiiAsync(string text, CancellationToken ct = default)
    {
        var result = await ValidateInputAsync(text, ct);
        // Extract PII specifics if the PiiRedactionValidator modified the text
        // For simplicity, we just return the sanitized text and empty entities here,
        // since PiiRedactionValidator would populate the context with specifics.
        return new PiiRedactionResult(
            OriginalText: text, 
            SanitizedText: result.SanitizedInput ?? text, 
            DetectedEntities: []);
    }

    /// <summary>
    /// Convenience method for checking content safety.
    /// </summary>
    public async Task<ContentSafetyResult> CheckContentSafetyAsync(string text, CancellationToken ct = default)
    {
        var result = await ValidateOutputAsync(text, ct);
        return new ContentSafetyResult(
            IsSafe: result.IsSuccess,
            OverallScore: 0.0,
            CategoryScores: new Dictionary<string, double>(),
            FlaggedCategories: []
        );
    }
}

using AIGuard.NET.Models;

namespace AIGuard.NET.Validators;

/// <summary>
/// Abstract base class for all validators in the AIGuard.NET pipeline.
/// Provides common structure including name, type, and execution order.
/// </summary>
public abstract class ValidatorBase
{
    /// <summary>
    /// Gets the display name of this validator. Defaults to the type name.
    /// </summary>
    public virtual string Name => GetType().Name;

    /// <summary>
    /// Gets the type of this validator, indicating whether it validates input, output, or both.
    /// </summary>
    public abstract ValidatorType Type { get; }

    /// <summary>
    /// Gets the execution order of this validator within the pipeline.
    /// Lower values execute first.
    /// </summary>
    public abstract int Order { get; }

    /// <summary>
    /// Validates the specified content asynchronously.
    /// </summary>
    /// <param name="content">The text content to validate.</param>
    /// <param name="context">The validation context providing additional metadata.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A <see cref="ValidationResult"/> indicating the outcome of the validation.</returns>
    public abstract Task<ValidationResult> ValidateAsync(
        string content,
        ValidationContext context,
        CancellationToken cancellationToken = default);
}

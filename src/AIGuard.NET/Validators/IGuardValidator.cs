using AIGuard.NET.Models;

namespace AIGuard.NET.Validators;

/// <summary>
/// Defines the contract for an AI guard validator.
/// </summary>
public interface IGuardValidator
{
    /// <summary>
    /// Gets the type of validator (Input, Output, or Both).
    /// </summary>
    ValidatorType Type { get; }

    /// <summary>
    /// Gets the name of the validator.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the execution order of the validator. Lower values run first.
    /// </summary>
    int Order { get; }

    /// <summary>
    /// Validates the given content.
    /// </summary>
    /// <param name="content">The content to validate.</param>
    /// <param name="context">The validation context.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A validation result.</returns>
    Task<ValidationResult> ValidateAsync(string content, ValidationContext context, CancellationToken ct);
}

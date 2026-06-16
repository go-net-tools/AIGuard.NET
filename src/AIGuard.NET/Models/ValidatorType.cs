namespace AIGuard.NET.Models;

/// <summary>
/// Specifies when a validator should be applied in the AI pipeline.
/// </summary>
public enum ValidatorType
{
    /// <summary>
    /// Validates input content before it is sent to the AI model.
    /// </summary>
    Input,

    /// <summary>
    /// Validates output content received from the AI model.
    /// </summary>
    Output,

    /// <summary>
    /// Validates both input and output content.
    /// </summary>
    Both
}

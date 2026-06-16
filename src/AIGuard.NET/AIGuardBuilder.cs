using AIGuard.NET.Models;
using AIGuard.NET.Pipeline;
using AIGuard.NET.Validators;

namespace AIGuard.NET;

/// <summary>
/// Fluent builder for constructing an AIGuard instance.
/// </summary>
public sealed class AIGuardBuilder
{
    private readonly List<IGuardValidator> _validators = [];
    private readonly AIGuardOptions _options = new();

    /// <summary>
    /// Adds a validator of the specified type with default configuration.
    /// </summary>
    public AIGuardBuilder AddValidator<T>() where T : IGuardValidator, new()
    {
        _validators.Add(new T());
        return this;
    }

    /// <summary>
    /// Adds a validator of the specified type and configures it.
    /// </summary>
    public AIGuardBuilder AddValidator<T>(Action<T> configure) where T : IGuardValidator, new()
    {
        var validator = new T();
        configure(validator);
        _validators.Add(validator);
        return this;
    }

    /// <summary>
    /// Adds a specific validator instance.
    /// </summary>
    public AIGuardBuilder AddValidator(IGuardValidator validator)
    {
        _validators.Add(validator);
        return this;
    }

    /// <summary>
    /// Configures the AIGuard options.
    /// </summary>
    public AIGuardBuilder WithOptions(Action<AIGuardOptions> configure)
    {
        configure(_options);
        return this;
    }

    /// <summary>
    /// Configures whether to stop on the first validation failure.
    /// </summary>
    public AIGuardBuilder WithStopOnFirstFailure(bool stop = true)
    {
        _options.StopOnFirstFailure = stop;
        return this;
    }

    /// <summary>
    /// Configures the maximum number of retries.
    /// </summary>
    public AIGuardBuilder WithMaxRetries(int retries)
    {
        _options.MaxRetries = retries;
        return this;
    }

    /// <summary>
    /// Configures the timeout for validation.
    /// </summary>
    public AIGuardBuilder WithTimeout(TimeSpan timeout)
    {
        _options.Timeout = timeout;
        return this;
    }

    /// <summary>
    /// Configures whether audit logging is enabled.
    /// </summary>
    public AIGuardBuilder WithAuditLog(bool enable = true)
    {
        _options.EnableAuditLog = enable;
        return this;
    }

    /// <summary>
    /// Configures the minimum confidence threshold for validation results.
    /// </summary>
    public AIGuardBuilder WithMinConfidence(double threshold)
    {
        _options.MinConfidenceThreshold = threshold;
        return this;
    }

    /// <summary>
    /// Builds the AIGuard instance.
    /// </summary>
    public AIGuard Build()
    {
        var pipeline = new GuardPipeline(_validators, _options);
        return new AIGuard(pipeline, _options);
    }
}

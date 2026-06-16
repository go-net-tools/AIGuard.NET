using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using AIGuard.NET.Pipeline;

namespace AIGuard.NET.AspNetCore.DependencyInjection;

/// <summary>
/// Extension methods for setting up AIGuard.NET services in an <see cref="IServiceCollection" />.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds AIGuard.NET services to the specified <see cref="IServiceCollection" />.
    /// </summary>
    public static IServiceCollection AddAIGuard(
        this IServiceCollection services,
        Action<AIGuardBuilder> configure)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configure);

        services.TryAddSingleton<AIGuard>(sp =>
        {
            var builder = AIGuard.CreateBuilder();
            
            // Allow user to configure validators and options
            configure(builder);

            return builder.Build();
        });

        return services;
    }
}

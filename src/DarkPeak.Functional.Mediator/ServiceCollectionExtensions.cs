using Mediator;
using Microsoft.Extensions.DependencyInjection;

namespace DarkPeak.Functional.Mediator;

/// <summary>
/// Extension methods for registering DarkPeak.Functional Mediator pipeline behaviors.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the DarkPeak.Functional Mediator pipeline behaviors in the recommended order:
    /// <list type="number">
    ///   <item><see cref="ResultExceptionHandler{TMessage, T}"/> — outermost, catches unhandled exceptions</item>
    ///   <item><see cref="ResultValidationBehavior{TMessage, T}"/> — innermost, short-circuits invalid messages</item>
    /// </list>
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddDarkPeakMediatorBehaviors(this IServiceCollection services)
    {
        services.AddSingleton(typeof(IPipelineBehavior<,>), typeof(ResultExceptionHandler<,>));
        services.AddSingleton(typeof(IPipelineBehavior<,>), typeof(ResultValidationBehavior<,>));
        return services;
    }
}

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using DarkPeak.Functional;

namespace DarkPeak.Functional.HealthChecks;

/// <summary>
/// Extension methods for registering DarkPeak.Functional health checks on <see cref="IHealthChecksBuilder"/>.
/// </summary>
public static class HealthChecksBuilderExtensions
{
    /// <summary>
    /// Adds a health check that monitors a <see cref="CircuitBreakerPolicy"/>.
    /// Reports Healthy (Closed), Degraded (HalfOpen), or Unhealthy (Open).
    /// </summary>
    /// <param name="builder">The health checks builder.</param>
    /// <param name="name">The name of the health check (e.g. "payment-gateway").</param>
    /// <param name="circuitBreaker">The circuit breaker policy to monitor.</param>
    /// <param name="tags">Optional tags for filtering health checks (e.g. "ready", "live").</param>
    /// <returns>The <see cref="IHealthChecksBuilder"/> for chaining.</returns>
    public static IHealthChecksBuilder AddCircuitBreakerHealthCheck(
        this IHealthChecksBuilder builder,
        string name,
        CircuitBreakerPolicy circuitBreaker,
        IEnumerable<string>? tags = null)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(circuitBreaker);

        return builder.Add(new HealthCheckRegistration(
            name,
            new CircuitBreakerHealthCheck(circuitBreaker),
            failureStatus: HealthStatus.Unhealthy,
            tags));
    }

    /// <summary>
    /// Adds a health check that probes an <see cref="ICacheProvider{TKey, TValue}"/> by
    /// attempting to retrieve a sentinel key. A successful <c>GetAsync</c> call (returning
    /// Some or None) indicates the provider is healthy.
    /// An exception indicates the provider is unreachable.
    /// </summary>
    /// <typeparam name="TKey">The cache key type.</typeparam>
    /// <typeparam name="TValue">The cache value type.</typeparam>
    /// <param name="builder">The health checks builder.</param>
    /// <param name="name">The name of the health check (e.g. "redis-cache").</param>
    /// <param name="cacheProvider">The cache provider to probe.</param>
    /// <param name="probeKey">
    /// A key used to test connectivity by calling <c>GetAsync</c>. The key does not need to map to
    /// an existing cache entry — a cache miss (returning None) still indicates the provider is reachable.
    /// </param>
    /// <param name="tags">Optional tags for filtering health checks (e.g. "ready", "live").</param>
    /// <returns>The <see cref="IHealthChecksBuilder"/> for chaining.</returns>
    public static IHealthChecksBuilder AddCacheProviderHealthCheck<TKey, TValue>(
        this IHealthChecksBuilder builder,
        string name,
        ICacheProvider<TKey, TValue> cacheProvider,
        TKey probeKey,
        IEnumerable<string>? tags = null)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(cacheProvider);

        return builder.Add(new HealthCheckRegistration(
            name,
            new CacheProviderHealthCheck<TKey, TValue>(cacheProvider, probeKey),
            failureStatus: HealthStatus.Unhealthy,
            tags));
    }
}

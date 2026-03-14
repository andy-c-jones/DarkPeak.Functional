using Microsoft.Extensions.Diagnostics.HealthChecks;
using DarkPeak.Functional;

namespace DarkPeak.Functional.HealthChecks;

/// <summary>
/// An <see cref="IHealthCheck"/> that probes an <see cref="ICacheProvider{TKey, TValue}"/>
/// for reachability by attempting to retrieve a sentinel key.
/// </summary>
/// <typeparam name="TKey">The cache key type.</typeparam>
/// <typeparam name="TValue">The cache value type.</typeparam>
public sealed class CacheProviderHealthCheck<TKey, TValue> : IHealthCheck
{
    private readonly ICacheProvider<TKey, TValue> _cacheProvider;
    private readonly TKey _probeKey;

    /// <summary>
    /// Creates a health check that probes the specified cache provider.
    /// </summary>
    /// <param name="cacheProvider">The cache provider to probe.</param>
    /// <param name="probeKey">
    /// A key used to test connectivity by calling <c>GetAsync</c>. The key does not need to map to
    /// an existing cache entry — a cache miss (returning None) still indicates the provider is reachable.
    /// </param>
    public CacheProviderHealthCheck(ICacheProvider<TKey, TValue> cacheProvider, TKey probeKey)
    {
        ArgumentNullException.ThrowIfNull(cacheProvider);
        _cacheProvider = cacheProvider;
        _probeKey = probeKey;
    }

    /// <inheritdoc />
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await _cacheProvider.GetAsync(_probeKey);
            return HealthCheckResult.Healthy("Cache provider is reachable");
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            return HealthCheckResult.Unhealthy("Cache provider is unreachable", ex);
        }
    }
}

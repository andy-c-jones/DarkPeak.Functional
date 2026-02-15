using System.Collections.Concurrent;

namespace DarkPeak.Functional;

/// <summary>
/// Provides factory methods to create memoized versions of functions that return
/// <see cref="Result{T, TError}"/>, caching only successful results.
/// Failed results pass through uncached so subsequent calls retry the computation.
/// </summary>
/// <remarks>
/// <para>
/// Unlike <see cref="Memoize"/>, which caches any return value unconditionally,
/// <see cref="MemoizeResult"/> inspects the <see cref="Result{T, TError}"/> and only
/// stores the success value. This prevents caching transient failures — if a call fails,
/// the next call will re-execute the function instead of returning the cached failure.
/// </para>
/// <para>
/// This is particularly useful for HTTP calls, external service requests, and other operations
/// where failures are often transient and should be retried on the next attempt.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Cache successful API responses for 5 minutes; failures are never cached
/// var cachedFetch = MemoizeResult.FuncAsync&lt;string, AppConfig, Error&gt;(
///     endpoint => httpClient.GetResultAsync&lt;AppConfig&gt;(endpoint),
///     opts => opts.WithExpiration(TimeSpan.FromMinutes(5)));
///
/// var result = await cachedFetch("/api/config");
/// // First call: fetches from API, caches on success
/// // Second call within 5 min: returns cached success immediately
/// // If first call failed: second call retries the API
/// </code>
/// </example>
public static class MemoizeResult
{
    /// <summary>
    /// Memoizes an async single-argument function that returns a <see cref="Result{T, TError}"/>,
    /// caching only successful results. Failed results are not cached.
    /// Concurrent calls for the same key share a single in-flight computation (thundering-herd protection).
    /// </summary>
    /// <typeparam name="TKey">The argument type (used as cache key).</typeparam>
    /// <typeparam name="TValue">The success value type.</typeparam>
    /// <typeparam name="TError">The error type.</typeparam>
    /// <param name="func">The function to memoize.</param>
    /// <param name="configure">Optional configuration for TTL, max size, and distributed cache.</param>
    /// <returns>A memoized version of the function that caches only successful results.</returns>
    public static Func<TKey, Task<Result<TValue, TError>>> FuncAsync<TKey, TValue, TError>(
        Func<TKey, Task<Result<TValue, TError>>> func,
        Func<MemoizeOptions, MemoizeOptions>? configure = null)
        where TKey : notnull
        where TError : Error
    {
        if (configure is not null)
        {
            var options = configure(new MemoizeOptions());
            var provider = options.CacheProvider as ICacheProvider<TKey, TValue>;
            var cache = new MemoizeCache<TKey, TValue>(options, provider);
            var inflight = new ConcurrentDictionary<TKey, Task<Result<TValue, TError>>>();

            return async key =>
            {
                // Check cache first (sync L1 path)
                var cached = cache.TryGet(key);
                if (cached is Some<TValue> some)
                    return Result.Success<TValue, TError>(some.Value);

                // Thundering-herd protection: share a single in-flight task per key
                var task = inflight.GetOrAdd(key, k => ExecuteAndCache(k, func, cache));
                try
                {
                    return await task;
                }
                finally
                {
                    inflight.TryRemove(key, out _);
                }
            };
        }
        else
        {
            var cache = new ConcurrentDictionary<TKey, TValue>();
            var inflight = new ConcurrentDictionary<TKey, Task<Result<TValue, TError>>>();

            return async key =>
            {
                if (cache.TryGetValue(key, out var cached))
                    return Result.Success<TValue, TError>(cached);

                var task = inflight.GetOrAdd(key, k => ExecuteAndCacheSimple(k, func, cache));
                try
                {
                    return await task;
                }
                finally
                {
                    inflight.TryRemove(key, out _);
                }
            };
        }
    }

    /// <summary>
    /// Memoizes a synchronous single-argument function that returns a <see cref="Result{T, TError}"/>,
    /// caching only successful results. Failed results are not cached.
    /// </summary>
    /// <typeparam name="TKey">The argument type (used as cache key).</typeparam>
    /// <typeparam name="TValue">The success value type.</typeparam>
    /// <typeparam name="TError">The error type.</typeparam>
    /// <param name="func">The function to memoize.</param>
    /// <param name="configure">Optional configuration for TTL, max size, and distributed cache.</param>
    /// <returns>A memoized version of the function that caches only successful results.</returns>
    public static Func<TKey, Result<TValue, TError>> Func<TKey, TValue, TError>(
        Func<TKey, Result<TValue, TError>> func,
        Func<MemoizeOptions, MemoizeOptions>? configure = null)
        where TKey : notnull
        where TError : Error
    {
        if (configure is not null)
        {
            var options = configure(new MemoizeOptions());
            var provider = options.CacheProvider as ICacheProvider<TKey, TValue>;
            var cache = new MemoizeCache<TKey, TValue>(options, provider);

            return key =>
            {
                var cached = cache.TryGet(key);
                if (cached is Some<TValue> some)
                    return Result.Success<TValue, TError>(some.Value);

                var result = func(key);
                result.Match(
                    success: value =>
                    {
                        cache.Add(key, value);
                        return value;
                    },
                    failure: _ => default!);
                return result;
            };
        }
        else
        {
            var cache = new ConcurrentDictionary<TKey, TValue>();

            return key =>
            {
                if (cache.TryGetValue(key, out var cached))
                    return Result.Success<TValue, TError>(cached);

                var result = func(key);
                result.Match(
                    success: value =>
                    {
                        cache.TryAdd(key, value);
                        return value;
                    },
                    failure: _ => default!);
                return result;
            };
        }
    }

    private static async Task<Result<TValue, TError>> ExecuteAndCache<TKey, TValue, TError>(
        TKey key,
        Func<TKey, Task<Result<TValue, TError>>> func,
        MemoizeCache<TKey, TValue> cache)
        where TKey : notnull
        where TError : Error
    {
        var result = await func(key);
        result.Match(
            success: value =>
            {
                cache.Add(key, value);
                return value;
            },
            failure: _ => default!);
        return result;
    }

    private static async Task<Result<TValue, TError>> ExecuteAndCacheSimple<TKey, TValue, TError>(
        TKey key,
        Func<TKey, Task<Result<TValue, TError>>> func,
        ConcurrentDictionary<TKey, TValue> cache)
        where TKey : notnull
        where TError : Error
    {
        var result = await func(key);
        result.Match(
            success: value =>
            {
                cache.TryAdd(key, value);
                return value;
            },
            failure: _ => default!);
        return result;
    }
}

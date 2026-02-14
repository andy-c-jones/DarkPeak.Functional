using System.Collections.Concurrent;

namespace DarkPeak.Functional;

/// <summary>
/// Configuration options for memoized functions.
/// </summary>
public sealed record MemoizeOptions
{
    /// <summary>
    /// Time-to-live for cache entries. Null means entries never expire.
    /// </summary>
    public TimeSpan? Expiration { get; init; }

    /// <summary>
    /// Maximum number of entries in the cache. Null means unbounded.
    /// When the limit is reached, the least recently used entry is evicted.
    /// </summary>
    public int? MaxSize { get; init; }

    /// <summary>
    /// Sets the expiration time for cache entries.
    /// </summary>
    public MemoizeOptions WithExpiration(TimeSpan expiration) =>
        this with { Expiration = expiration };

    /// <summary>
    /// Sets the maximum number of entries in the cache (LRU eviction).
    /// </summary>
    public MemoizeOptions WithMaxSize(int maxSize)
    {
        if (maxSize < 1)
            throw new ArgumentOutOfRangeException(nameof(maxSize), "Must be at least 1.");

        return this with { MaxSize = maxSize };
    }
}

/// <summary>
/// Provides factory methods to create memoized (cached) versions of functions.
/// Thread-safe. Supports TTL expiration and LRU eviction.
/// </summary>
public static class Memoize
{
    /// <summary>
    /// Memoizes a zero-argument function (lazy singleton).
    /// The function is called at most once; subsequent calls return the cached result.
    /// </summary>
    public static Func<TResult> Func<TResult>(Func<TResult> func)
    {
        var lazy = new Lazy<TResult>(func, LazyThreadSafetyMode.ExecutionAndPublication);
        return () => lazy.Value;
    }

    /// <summary>
    /// Memoizes a single-argument function with an unbounded, never-expiring cache.
    /// </summary>
    public static Func<T, TResult> Func<T, TResult>(Func<T, TResult> func) where T : notnull
    {
        var cache = new ConcurrentDictionary<T, TResult>();
        return arg => cache.GetOrAdd(arg, func);
    }

    /// <summary>
    /// Memoizes a single-argument function with configurable options (TTL, max size).
    /// </summary>
    public static Func<T, TResult> Func<T, TResult>(
        Func<T, TResult> func,
        Func<MemoizeOptions, MemoizeOptions> configure) where T : notnull
    {
        var options = configure(new MemoizeOptions());
        var cache = new MemoizeCache<T, TResult>(options);
        return arg => cache.GetOrAdd(arg, func);
    }

    /// <summary>
    /// Memoizes a two-argument function with an unbounded, never-expiring cache.
    /// </summary>
    public static Func<T1, T2, TResult> Func<T1, T2, TResult>(Func<T1, T2, TResult> func)
        where T1 : notnull where T2 : notnull
    {
        var cache = new ConcurrentDictionary<(T1, T2), TResult>();
        return (a, b) => cache.GetOrAdd((a, b), key => func(key.Item1, key.Item2));
    }

    /// <summary>
    /// Memoizes a two-argument function with configurable options (TTL, max size).
    /// </summary>
    public static Func<T1, T2, TResult> Func<T1, T2, TResult>(
        Func<T1, T2, TResult> func,
        Func<MemoizeOptions, MemoizeOptions> configure)
        where T1 : notnull where T2 : notnull
    {
        var options = configure(new MemoizeOptions());
        var cache = new MemoizeCache<(T1, T2), TResult>(options);
        return (a, b) => cache.GetOrAdd((a, b), key => func(key.Item1, key.Item2));
    }

    /// <summary>
    /// Memoizes an async single-argument function with an unbounded, never-expiring cache.
    /// Concurrent calls for the same key share the same Task (thundering-herd protection).
    /// </summary>
    public static Func<T, Task<TResult>> FuncAsync<T, TResult>(Func<T, Task<TResult>> func)
        where T : notnull
    {
        var cache = new ConcurrentDictionary<T, Lazy<Task<TResult>>>();
        return arg => cache.GetOrAdd(
            arg,
            key => new Lazy<Task<TResult>>(() => func(key), LazyThreadSafetyMode.ExecutionAndPublication)).Value;
    }

    /// <summary>
    /// Memoizes an async single-argument function with configurable options (TTL, max size).
    /// Concurrent calls for the same key share the same Task (thundering-herd protection).
    /// </summary>
    public static Func<T, Task<TResult>> FuncAsync<T, TResult>(
        Func<T, Task<TResult>> func,
        Func<MemoizeOptions, MemoizeOptions> configure) where T : notnull
    {
        var options = configure(new MemoizeOptions());
        var cache = new MemoizeCache<T, Lazy<Task<TResult>>>(options);
        return arg => cache.GetOrAdd(
            arg,
            key => new Lazy<Task<TResult>>(() => func(key), LazyThreadSafetyMode.ExecutionAndPublication)).Value;
    }
}

/// <summary>
/// Internal cache with TTL and LRU eviction support.
/// </summary>
internal sealed class MemoizeCache<TKey, TValue> where TKey : notnull
{
    private readonly Dictionary<TKey, CacheEntry> _cache = new();
    private readonly LinkedList<TKey> _accessOrder = new();
    private readonly Dictionary<TKey, LinkedListNode<TKey>> _nodes = new();
    private readonly MemoizeOptions _options;
    private readonly Lock _lock = new();

    internal MemoizeCache(MemoizeOptions options)
    {
        _options = options;
    }

    internal TValue GetOrAdd(TKey key, Func<TKey, TValue> factory)
    {
        lock (_lock)
        {
            if (_cache.TryGetValue(key, out var existing))
            {
                if (_options.Expiration is null ||
                    existing.CreatedAt + _options.Expiration.Value > DateTimeOffset.UtcNow)
                {
                    TouchAccessOrder(key);
                    return existing.Value;
                }

                // Expired — remove and recompute
                Remove(key);
            }

            var value = factory(key);
            _cache[key] = new CacheEntry(value, DateTimeOffset.UtcNow);
            TouchAccessOrder(key);
            EvictIfNeeded();

            return value;
        }
    }

    private void TouchAccessOrder(TKey key)
    {
        if (_nodes.TryGetValue(key, out var node))
            _accessOrder.Remove(node);

        _nodes[key] = _accessOrder.AddLast(key);
    }

    private void Remove(TKey key)
    {
        _cache.Remove(key);
        if (_nodes.Remove(key, out var node))
            _accessOrder.Remove(node);
    }

    private void EvictIfNeeded()
    {
        if (!_options.MaxSize.HasValue)
            return;

        while (_cache.Count > _options.MaxSize.Value && _accessOrder.First is not null)
        {
            Remove(_accessOrder.First.Value);
        }
    }

    private readonly record struct CacheEntry(TValue Value, DateTimeOffset CreatedAt);
}

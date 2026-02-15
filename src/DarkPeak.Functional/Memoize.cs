using System.Collections.Concurrent;

namespace DarkPeak.Functional;

/// <summary>
/// Abstraction for an external cache provider (e.g. Redis, SQL, etc.).
/// Implement this interface to plug a distributed cache into memoized functions.
/// The provider is responsible for its own serialization and transport concerns.
/// </summary>
public interface ICacheProvider<TKey, TValue>
{
    /// <summary>
    /// Attempts to retrieve a value from the cache.
    /// Returns Some if the key exists, None otherwise.
    /// </summary>
    Option<TValue> Get(TKey key);

    /// <summary>
    /// Attempts to retrieve a value from the cache asynchronously.
    /// Returns Some if the key exists, None otherwise.
    /// </summary>
    Task<Option<TValue>> GetAsync(TKey key);

    /// <summary>
    /// Stores a value in the cache with an optional expiration.
    /// </summary>
    void Set(TKey key, TValue value, TimeSpan? expiration);

    /// <summary>
    /// Stores a value in the cache asynchronously with an optional expiration.
    /// </summary>
    Task SetAsync(TKey key, TValue value, TimeSpan? expiration);

    /// <summary>
    /// Removes a value from the cache.
    /// </summary>
    void Remove(TKey key);

    /// <summary>
    /// Removes a value from the cache asynchronously.
    /// </summary>
    Task RemoveAsync(TKey key);
}

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
    /// External cache provider for distributed caching. Null means memory-only.
    /// </summary>
    internal object? CacheProvider { get; init; }

    /// <summary>
    /// Whether the in-memory L1 cache is enabled.
    /// True when MaxSize or Expiration is configured, or when no provider is set.
    /// </summary>
    internal bool UseMemoryCache => MaxSize.HasValue || Expiration.HasValue || CacheProvider is null;

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

    /// <summary>
    /// Sets an external cache provider for distributed caching.
    /// When combined with MaxSize or Expiration, acts as L2 behind an in-memory L1.
    /// When used alone, all caching is delegated to the provider.
    /// </summary>
    public MemoizeOptions WithCacheProvider<TKey, TValue>(ICacheProvider<TKey, TValue> provider) =>
        this with { CacheProvider = provider };
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
    /// Memoizes a single-argument function with configurable options (TTL, max size, distributed cache).
    /// </summary>
    public static Func<T, TResult> Func<T, TResult>(
        Func<T, TResult> func,
        Func<MemoizeOptions, MemoizeOptions> configure) where T : notnull
    {
        var options = configure(new MemoizeOptions());
        var provider = options.CacheProvider as ICacheProvider<T, TResult>;
        var cache = new MemoizeCache<T, TResult>(options, provider);
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
    /// Memoizes a two-argument function with configurable options (TTL, max size, distributed cache).
    /// </summary>
    public static Func<T1, T2, TResult> Func<T1, T2, TResult>(
        Func<T1, T2, TResult> func,
        Func<MemoizeOptions, MemoizeOptions> configure)
        where T1 : notnull where T2 : notnull
    {
        var options = configure(new MemoizeOptions());
        var provider = options.CacheProvider as ICacheProvider<(T1, T2), TResult>;
        var cache = new MemoizeCache<(T1, T2), TResult>(options, provider);
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
    /// Memoizes an async single-argument function with configurable options (TTL, max size, distributed cache).
    /// Concurrent calls for the same key share the same Task (thundering-herd protection).
    /// </summary>
    public static Func<T, Task<TResult>> FuncAsync<T, TResult>(
        Func<T, Task<TResult>> func,
        Func<MemoizeOptions, MemoizeOptions> configure) where T : notnull
    {
        var options = configure(new MemoizeOptions());
        var provider = options.CacheProvider as ICacheProvider<T, TResult>;
        if (provider is not null)
        {
            var cache = new MemoizeCache<T, TResult>(options, provider);
            return arg => cache.GetOrAddAsync(arg, async key => await func(key));
        }
        else
        {
            var cache = new MemoizeCache<T, Lazy<Task<TResult>>>(options, null);
            return arg => cache.GetOrAdd(
                arg,
                key => new Lazy<Task<TResult>>(() => func(key), LazyThreadSafetyMode.ExecutionAndPublication)).Value;
        }
    }
}

/// <summary>
/// Internal cache with TTL, LRU eviction, and optional external provider support.
/// </summary>
internal sealed class MemoizeCache<TKey, TValue> where TKey : notnull
{
    private readonly Dictionary<TKey, CacheEntry> _cache = new();
    private readonly LinkedList<TKey> _accessOrder = new();
    private readonly Dictionary<TKey, LinkedListNode<TKey>> _nodes = new();
    private readonly MemoizeOptions _options;
    private readonly ICacheProvider<TKey, TValue>? _provider;
    private readonly Lock _lock = new();

    internal MemoizeCache(MemoizeOptions options, ICacheProvider<TKey, TValue>? provider)
    {
        _options = options;
        _provider = provider;
    }

    internal TValue GetOrAdd(TKey key, Func<TKey, TValue> factory)
    {
        lock (_lock)
        {
            // L1: check in-memory cache
            if (_options.UseMemoryCache && _cache.TryGetValue(key, out var existing))
            {
                if (_options.Expiration is null ||
                    existing.CreatedAt + _options.Expiration.Value > DateTimeOffset.UtcNow)
                {
                    TouchAccessOrder(key);
                    return existing.Value;
                }

                // Expired in L1 — remove
                RemoveFromMemory(key);
            }

            // L2: check external provider
            if (_provider is not null)
            {
                var fromProvider = _provider.Get(key);
                if (fromProvider is Some<TValue> some)
                {
                    // Populate L1 from L2
                    if (_options.UseMemoryCache)
                    {
                        _cache[key] = new CacheEntry(some.Value, DateTimeOffset.UtcNow);
                        TouchAccessOrder(key);
                        EvictIfNeeded();
                    }

                    return some.Value;
                }
            }

            // Miss everywhere — compute
            var value = factory(key);

            // Write to L1
            if (_options.UseMemoryCache)
            {
                _cache[key] = new CacheEntry(value, DateTimeOffset.UtcNow);
                TouchAccessOrder(key);
                EvictIfNeeded();
            }

            // Write to L2
            _provider?.Set(key, value, _options.Expiration);

            return value;
        }
    }

    internal async Task<TValue> GetOrAddAsync(TKey key, Func<TKey, Task<TValue>> factory)
    {
        // L1: check in-memory cache (synchronous, under lock)
        lock (_lock)
        {
            if (_options.UseMemoryCache && _cache.TryGetValue(key, out var existing))
            {
                if (_options.Expiration is null ||
                    existing.CreatedAt + _options.Expiration.Value > DateTimeOffset.UtcNow)
                {
                    TouchAccessOrder(key);
                    return existing.Value;
                }

                RemoveFromMemory(key);
            }
        }

        // L2: check external provider (async, outside lock)
        if (_provider is not null)
        {
            var fromProvider = await _provider.GetAsync(key);
            if (fromProvider is Some<TValue> some)
            {
                lock (_lock)
                {
                    if (_options.UseMemoryCache)
                    {
                        _cache[key] = new CacheEntry(some.Value, DateTimeOffset.UtcNow);
                        TouchAccessOrder(key);
                        EvictIfNeeded();
                    }
                }

                return some.Value;
            }
        }

        // Miss everywhere — compute (async, outside lock)
        var value = await factory(key);

        // Write to L1
        lock (_lock)
        {
            if (_options.UseMemoryCache)
            {
                _cache[key] = new CacheEntry(value, DateTimeOffset.UtcNow);
                TouchAccessOrder(key);
                EvictIfNeeded();
            }
        }

        // Write to L2
        if (_provider is not null)
            await _provider.SetAsync(key, value, _options.Expiration);

        return value;
    }

    /// <summary>
    /// Attempts to retrieve a value from the cache without computing it.
    /// Checks L1 (in-memory) and L2 (provider) caches.
    /// </summary>
    internal Option<TValue> TryGet(TKey key)
    {
        lock (_lock)
        {
            if (_options.UseMemoryCache && _cache.TryGetValue(key, out var existing))
            {
                if (_options.Expiration is null ||
                    existing.CreatedAt + _options.Expiration.Value > DateTimeOffset.UtcNow)
                {
                    TouchAccessOrder(key);
                    return Option.Some(existing.Value);
                }

                RemoveFromMemory(key);
            }
        }

        if (_provider is not null)
        {
            var fromProvider = _provider.Get(key);
            if (fromProvider is Some<TValue> some)
            {
                lock (_lock)
                {
                    if (_options.UseMemoryCache)
                    {
                        _cache[key] = new CacheEntry(some.Value, DateTimeOffset.UtcNow);
                        TouchAccessOrder(key);
                        EvictIfNeeded();
                    }
                }

                return fromProvider;
            }
        }

        return Option.None<TValue>();
    }

    /// <summary>
    /// Adds a value to the cache unconditionally, writing to both L1 and L2.
    /// </summary>
    internal void Add(TKey key, TValue value)
    {
        lock (_lock)
        {
            if (_options.UseMemoryCache)
            {
                _cache[key] = new CacheEntry(value, DateTimeOffset.UtcNow);
                TouchAccessOrder(key);
                EvictIfNeeded();
            }
        }

        _provider?.Set(key, value, _options.Expiration);
    }

    private void TouchAccessOrder(TKey key)
    {
        if (_nodes.TryGetValue(key, out var node))
            _accessOrder.Remove(node);

        _nodes[key] = _accessOrder.AddLast(key);
    }

    private void RemoveFromMemory(TKey key)
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
            RemoveFromMemory(_accessOrder.First.Value);
        }
    }

    private readonly record struct CacheEntry(TValue Value, DateTimeOffset CreatedAt);
}

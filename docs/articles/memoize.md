# Memoize

The `Memoize` module provides function caching with support for TTL expiration, LRU eviction, and pluggable distributed cache providers.

## Basic Usage

Wrap any function to cache its results:

```csharp
var cachedParse = Memoize.Func<string, int>(int.Parse);

var a = cachedParse("42"); // computes
var b = cachedParse("42"); // cache hit — returns instantly
```

## Zero-Argument (Lazy Singleton)

```csharp
var loadConfig = Memoize.Func(() => LoadExpensiveConfig());

var config1 = loadConfig(); // computes once
var config2 = loadConfig(); // cached
```

## Multi-Argument

Two-argument functions use `ValueTuple` as the cache key:

```csharp
var add = Memoize.Func<int, int, int>((a, b) => a + b);

add(1, 2); // computes
add(1, 2); // cache hit
add(2, 1); // different key, computes
```

## TTL Expiration

Cache entries automatically expire after a given duration:

```csharp
var lookup = Memoize.Func<string, UserProfile>(
    LoadUserProfile,
    opts => opts.WithExpiration(TimeSpan.FromMinutes(5)));
```

## LRU Eviction

Limit the cache size with least-recently-used eviction:

```csharp
var compute = Memoize.Func<int, int>(
    ExpensiveCalculation,
    opts => opts.WithMaxSize(1000));
```

## Combined Options

TTL and LRU can be used together:

```csharp
var cached = Memoize.Func<string, ApiResponse>(
    FetchFromApi,
    opts => opts
        .WithExpiration(TimeSpan.FromMinutes(10))
        .WithMaxSize(500));
```

## Async Memoization

Async functions get thundering-herd protection — concurrent calls for the same key share a single in-flight computation:

```csharp
var cachedFetch = Memoize.FuncAsync<string, ApiResponse>(
    FetchFromApiAsync);

// 50 concurrent calls for "key" → only one HTTP request
var tasks = Enumerable.Range(0, 50)
    .Select(_ => cachedFetch("key"));
var results = await Task.WhenAll(tasks);
```

With options:

```csharp
var cachedFetch = Memoize.FuncAsync<string, ApiResponse>(
    FetchFromApiAsync,
    opts => opts.WithExpiration(TimeSpan.FromSeconds(30)));
```

## Distributed Cache Support

Plug in any external cache by implementing `ICacheProvider<TKey, TValue>`. For a ready-made Redis implementation, see the [Redis](redis.md) documentation.

```csharp
public interface ICacheProvider<TKey, TValue>
{
    Option<TValue> Get(TKey key);
    Task<Option<TValue>> GetAsync(TKey key);
    void Set(TKey key, TValue value, TimeSpan? expiration);
    Task SetAsync(TKey key, TValue value, TimeSpan? expiration);
    void Remove(TKey key);
    Task RemoveAsync(TKey key);
}
```

### Distributed-Only

All caching delegated to the provider:

```csharp
var cached = Memoize.Func<string, UserProfile>(
    LoadUser,
    opts => opts.WithCacheProvider(new RedisCacheProvider<string, UserProfile>()));
```

### L1 Memory + L2 Distributed

Combine in-memory caching with a distributed provider for a two-tier strategy:

```csharp
var cached = Memoize.Func<string, UserProfile>(
    LoadUser,
    opts => opts
        .WithMaxSize(100)                        // L1: in-memory LRU
        .WithExpiration(TimeSpan.FromMinutes(5))  // TTL for both layers
        .WithCacheProvider(myRedisProvider));      // L2: distributed
```

**Read path:** L1 memory → L2 provider → compute (populate both)

**Write path:** Write to both L1 and L2

When an entry is evicted from L1 (LRU), it still lives in L2 and will be restored on next access.

### Async Distributed

```csharp
var cached = Memoize.FuncAsync<string, ApiResponse>(
    FetchFromApiAsync,
    opts => opts
        .WithExpiration(TimeSpan.FromSeconds(30))
        .WithCacheProvider(myProvider));
```

## Thread Safety

All memoization is thread-safe:
- Simple (no-options) variants use `ConcurrentDictionary`
- Options-based variants use a `lock`-protected LRU cache
- Async variants prevent thundering-herd via `Lazy<Task<T>>`

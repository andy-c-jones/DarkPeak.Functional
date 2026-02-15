# Redis

The `DarkPeak.Functional.Redis` package provides a Redis-backed `ICacheProvider` implementation using StackExchange.Redis, enabling distributed caching with `Memoize` and `MemoizeResult`.

## Installation

```bash
dotnet add package DarkPeak.Functional.Redis
```

## Basic Usage

Create a `RedisCacheProvider` and pass it to any memoized function:

```csharp
using DarkPeak.Functional;
using DarkPeak.Functional.Redis;
using StackExchange.Redis;

var redis = ConnectionMultiplexer.Connect("localhost:6379");
var db = redis.GetDatabase();

var provider = new RedisCacheProvider<string, UserProfile>(db);

var cachedLookup = Memoize.FuncAsync<string, UserProfile>(
    LoadUserProfileAsync,
    opts => opts.WithCacheProvider(provider));

var profile = await cachedLookup("user:123");
// First call: computes and caches in Redis
// Second call: returns from Redis cache
```

## With TTL

Set an expiration time for cached entries:

```csharp
var provider = new RedisCacheProvider<string, UserProfile>(db);

var cached = Memoize.FuncAsync<string, UserProfile>(
    LoadUserProfileAsync,
    opts => opts
        .WithExpiration(TimeSpan.FromMinutes(10))
        .WithCacheProvider(provider));
```

The TTL is forwarded to Redis via `SETEX` / `PSETEX`.

## L1 Memory + L2 Redis

Combine in-memory caching with Redis for a two-tier strategy:

```csharp
var provider = new RedisCacheProvider<string, UserProfile>(db);

var cached = Memoize.FuncAsync<string, UserProfile>(
    LoadUserProfileAsync,
    opts => opts
        .WithMaxSize(100)                        // L1: in-memory LRU
        .WithExpiration(TimeSpan.FromMinutes(5))  // TTL for both layers
        .WithCacheProvider(provider));             // L2: Redis
```

**Read path:** L1 memory → L2 Redis → compute (populate both)

**Write path:** Write to both L1 and L2

When an entry is evicted from L1 (LRU), it still lives in Redis and will be restored on next access.

## Key Prefix

Namespace Redis keys to avoid collisions with other applications sharing the same Redis instance:

```csharp
var provider = new RedisCacheProvider<string, UserProfile>(
    db, keyPrefix: "myapp:users:");

// Keys in Redis will be: myapp:users:user:123, myapp:users:user:456, etc.
```

## Custom JSON Serialization

Pass `JsonSerializerOptions` for custom serialization:

```csharp
var jsonOptions = new JsonSerializerOptions
{
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
};

var provider = new RedisCacheProvider<string, UserProfile>(
    db, jsonOptions: jsonOptions);
```

## With MemoizeResult

Cache only successful `Result` values in Redis — failed results are never cached, so subsequent calls retry the computation:

```csharp
var provider = new RedisCacheProvider<string, ApiResponse>(
    db, keyPrefix: "api:");

var cachedFetch = MemoizeResult.FuncAsync<string, ApiResponse, Error>(
    endpoint => httpClient.GetResultAsync<ApiResponse>(endpoint),
    opts => opts
        .WithExpiration(TimeSpan.FromMinutes(5))
        .WithCacheProvider(provider));

var result = await cachedFetch("/api/config");
// Success: cached in Redis for 5 minutes
// Failure: NOT cached, next call retries the HTTP request
```

## Full Resilience Stack

Combine Redis caching with retry, circuit breaker, and Http extensions for production-grade resilience:

```csharp
var redis = ConnectionMultiplexer.Connect("localhost:6379");
var cacheProvider = new RedisCacheProvider<string, CatalogItem>(
    redis.GetDatabase(), keyPrefix: "catalog:");

var breaker = CircuitBreaker
    .WithFailureThreshold(5)
    .WithResetTimeout(TimeSpan.FromSeconds(30))
    .WithBreakWhen(error => error is ExternalServiceError or HttpRequestError)
    .OnStateChange((from, to) =>
        logger.LogWarning("Circuit: {From} -> {To}", from, to));

var cachedFetch = MemoizeResult.FuncAsync<string, CatalogItem, Error>(
    endpoint => Retry
        .WithMaxAttempts(3)
        .WithBackoff(Backoff.Exponential(
            TimeSpan.FromMilliseconds(200),
            maxDelay: TimeSpan.FromSeconds(5)))
        .WithRetryWhen(error =>
            error is ExternalServiceError or HttpRequestError or CircuitBreakerOpenError)
        .OnRetry((attempt, error) =>
            logger.LogWarning("Retry {Attempt}: {Error}", attempt, error.Message))
        .ExecuteAsync(() =>
            breaker.ExecuteAsync(
                () => httpClient.GetResultAsync<CatalogItem>(endpoint))),
    opts => opts
        .WithMaxSize(100)                          // L1: in-memory LRU
        .WithExpiration(TimeSpan.FromMinutes(10))   // TTL for both layers
        .WithCacheProvider(cacheProvider));          // L2: Redis

// Usage
var result = await cachedFetch("/api/catalog/item-42");
```

**Request flow:** L1 memory → L2 Redis → Retry loop → Circuit breaker → HTTP call

On success, the result is cached in both memory and Redis. On transient failure, the retry policy backs off and retries. If the dependency is consistently failing, the circuit opens and rejects requests immediately.

## Constructor Reference

```csharp
public RedisCacheProvider(
    IDatabase database,           // StackExchange.Redis IDatabase
    string keyPrefix = "",        // Optional prefix for all Redis keys
    JsonSerializerOptions? jsonOptions = null)  // Optional JSON serialization options
```

| Parameter | Description |
|-----------|-------------|
| `database` | The StackExchange.Redis `IDatabase` to use for cache operations |
| `keyPrefix` | Prepended to all Redis keys (e.g. `"myapp:cache:"`) |
| `jsonOptions` | `JsonSerializerOptions` for value serialization (defaults to `JsonSerializerOptions.Default`) |

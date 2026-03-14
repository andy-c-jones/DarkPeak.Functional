# Health Checks

The `DarkPeak.Functional.HealthChecks` package integrates DarkPeak.Functional's resilience policies with ASP.NET Core's health check system, exposing circuit breaker and cache provider status via standard `IHealthCheck` implementations.

## Installation

```bash
dotnet add package DarkPeak.Functional.HealthChecks
```

## Basic Usage

Register a circuit breaker health check with a single extension method call:

```csharp
using DarkPeak.Functional;
using DarkPeak.Functional.HealthChecks;

var breaker = CircuitBreaker
    .WithFailureThreshold(5)
    .WithResetTimeout(TimeSpan.FromSeconds(30));

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddHealthChecks()
    .AddCircuitBreakerHealthCheck("payment-gateway", breaker);

var app = builder.Build();
app.MapHealthChecks("/health");
app.Run();
```

## Circuit Breaker Health Check

The `CircuitBreakerHealthCheck` monitors a `CircuitBreakerPolicy` via `GetSnapshot()` and maps its state to ASP.NET Core health statuses:

| Circuit State | Health Status | Description |
|---------------|---------------|-------------|
| Closed | Healthy | Normal operation |
| HalfOpen | Degraded | Probe request allowed |
| Open | Unhealthy | Requests rejected |

```csharp
using DarkPeak.Functional;
using DarkPeak.Functional.HealthChecks;

var breaker = CircuitBreaker
    .WithFailureThreshold(3)
    .WithResetTimeout(TimeSpan.FromSeconds(60))
    .WithBreakWhen(error => error is ExternalServiceError);

builder.Services
    .AddHealthChecks()
    .AddCircuitBreakerHealthCheck("inventory-service", breaker);
```

## Cache Provider Health Check

The `CacheProviderHealthCheck<TKey, TValue>` probes an `ICacheProvider` by calling `GetAsync` with a sentinel key. A successful call (returning `Some` or `None`) indicates the provider is reachable. An exception marks it as unhealthy.

```csharp
using DarkPeak.Functional;
using DarkPeak.Functional.HealthChecks;
using DarkPeak.Functional.Redis;
using StackExchange.Redis;

var redis = ConnectionMultiplexer.Connect("localhost:6379");
var cacheProvider = new RedisCacheProvider<string, string>(
    redis.GetDatabase(), keyPrefix: "myapp:");

builder.Services
    .AddHealthChecks()
    .AddCacheProviderHealthCheck("redis-cache", cacheProvider, probeKey: "__health");
```

The probe key (`"__health"`) does not need to exist in the cache — a cache miss still confirms connectivity.

## Tags

Use tags to separate liveness and readiness checks. This lets Kubernetes (or any orchestrator) query different endpoints for different purposes:

```csharp
builder.Services
    .AddHealthChecks()
    .AddCircuitBreakerHealthCheck(
        "payment-gateway", breaker, tags: ["ready"])
    .AddCacheProviderHealthCheck(
        "redis-cache", cacheProvider, probeKey: "__health", tags: ["ready"]);

app.MapHealthChecks("/health/ready", new()
{
    Predicate = check => check.Tags.Contains("ready")
});

app.MapHealthChecks("/health/live", new()
{
    Predicate = check => !check.Tags.Contains("ready")
});
```

## Health Check Data

The circuit breaker health check includes structured data in its result:

| Key | Type | Description |
|-----|------|-------------|
| `state` | `string` | Current circuit state (`Closed`, `HalfOpen`, or `Open`) |
| `failureCount` | `int` | Number of consecutive failures |
| `resetTime` | `string` | ISO 8601 UTC timestamp when the circuit will transition to HalfOpen (only present when Open) |

When the circuit is open, the description includes the failure count and reset timestamp:

> Circuit breaker is Open. 5 consecutive failures. Resets at 2025-01-15 14:30:00Z

## Full Example

A complete minimal API combining circuit breaker, Redis cache, and health checks:

```csharp
using DarkPeak.Functional;
using DarkPeak.Functional.HealthChecks;
using DarkPeak.Functional.Http;
using DarkPeak.Functional.Redis;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// Redis
var redis = ConnectionMultiplexer.Connect("localhost:6379");
var cacheProvider = new RedisCacheProvider<string, CatalogItem>(
    redis.GetDatabase(), keyPrefix: "catalog:");

// Circuit breaker
var breaker = CircuitBreaker
    .WithFailureThreshold(5)
    .WithResetTimeout(TimeSpan.FromSeconds(30))
    .WithBreakWhen(error => error is ExternalServiceError or HttpRequestError);

// Memoized + resilient fetch
var httpClient = new HttpClient { BaseAddress = new Uri("https://api.example.com") };

var cachedFetch = MemoizeResult.FuncAsync<string, CatalogItem, Error>(
    endpoint => Retry
        .WithMaxAttempts(3)
        .WithBackoff(Backoff.Exponential(TimeSpan.FromMilliseconds(200)))
        .ExecuteAsync(() =>
            breaker.ExecuteAsync(
                () => httpClient.GetResultAsync<CatalogItem>(endpoint))),
    opts => opts
        .WithExpiration(TimeSpan.FromMinutes(10))
        .WithCacheProvider(cacheProvider));

// Health checks
builder.Services
    .AddHealthChecks()
    .AddCircuitBreakerHealthCheck("catalog-api", breaker, tags: ["ready"])
    .AddCacheProviderHealthCheck("redis-cache", cacheProvider,
        probeKey: "__health", tags: ["ready"]);

var app = builder.Build();

app.MapHealthChecks("/health");
app.MapHealthChecks("/health/ready", new()
{
    Predicate = check => check.Tags.Contains("ready")
});

app.MapGet("/catalog/{id}", async (string id) =>
{
    var result = await cachedFetch($"/api/catalog/{id}");
    return result.Match(
        success: item => Results.Ok(item),
        failure: error => Results.Problem(error.Message));
});

app.Run();

record CatalogItem(string Id, string Name, decimal Price);
```

**Request flow:** L1 memory → L2 Redis → Retry → Circuit breaker → HTTP call

Health checks expose the state of each resilience component independently, so your orchestrator can route traffic away from degraded instances before end users are affected.

## API Reference

| Method | Parameters | Description |
|--------|------------|-------------|
| `AddCircuitBreakerHealthCheck` | `string name`, `CircuitBreakerPolicy circuitBreaker`, `IEnumerable<string>? tags = null` | Registers a health check that monitors a circuit breaker. Reports Healthy (Closed), Degraded (HalfOpen), or Unhealthy (Open). |
| `AddCacheProviderHealthCheck<TKey, TValue>` | `string name`, `ICacheProvider<TKey, TValue> cacheProvider`, `TKey probeKey`, `IEnumerable<string>? tags = null` | Registers a health check that probes a cache provider by calling `GetAsync` with a sentinel key. Healthy if reachable, Unhealthy if an exception occurs. |

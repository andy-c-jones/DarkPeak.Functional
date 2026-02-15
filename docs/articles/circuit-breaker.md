# Circuit Breaker

The `CircuitBreaker` module prevents cascading failures by short-circuiting requests to a failing dependency. It integrates with `Result<T, TError>` — the circuit tracks failures and rejects requests when a threshold is reached.

## Basic Usage

```csharp
var breaker = CircuitBreaker.WithFailureThreshold(5)
    .WithResetTimeout(TimeSpan.FromSeconds(30));

var result = breaker.Execute(() => CallExternalService());
// After 5 consecutive failures, subsequent calls return CircuitBreakerOpenError immediately
```

## Builder API

Build circuit breaker policies fluently:

```csharp
var breaker = CircuitBreaker
    .WithFailureThreshold(5)
    .WithResetTimeout(TimeSpan.FromSeconds(30))
    .WithBreakWhen(error => error is ExternalServiceError)
    .OnStateChange((from, to) =>
        logger.LogWarning("Circuit breaker: {From} -> {To}", from, to));

var result = await breaker.ExecuteAsync(
    () => httpClient.GetResultAsync<Data>("/api/data"));
```

### Policy Options

| Method | Description |
|--------|-------------|
| `WithFailureThreshold(int)` | Consecutive failures before the circuit opens (at least 1) |
| `WithResetTimeout(TimeSpan)` | Duration the circuit stays open before transitioning to half-open |
| `WithBreakWhen(Func<Error, bool>)` | Predicate to filter which errors count toward the threshold |
| `OnStateChange(Action<CircuitBreakerState, CircuitBreakerState>)` | Callback for logging/observability on state transitions |

## State Transitions

The circuit breaker has three states:

```
  ┌──────────┐  failure threshold   ┌──────┐  reset timeout   ┌──────────┐
  │  Closed  │ ────────────────────> │ Open │ ────────────────> │ HalfOpen │
  └──────────┘                      └──────┘                   └──────────┘
       ^                                ^                           │
       │                                │                           │
       │          success               │        failure            │
       └────────────────────────────────┴───────────────────────────┘
```

- **Closed** — Normal operation. Failures increment a counter. When the counter reaches the failure threshold, the circuit opens.
- **Open** — All requests are immediately rejected with `CircuitBreakerOpenError`. After the reset timeout elapses, the circuit transitions to half-open.
- **HalfOpen** — One probe request is allowed through. On success the circuit closes and the failure counter resets; on failure it reopens.

## Sync and Async

Both synchronous and asynchronous execution are supported:

```csharp
// Synchronous
Result<Data, Error> result = breaker.Execute(() => LoadData());

// Asynchronous
Result<Data, Error> result = await breaker.ExecuteAsync(() => LoadDataAsync());
```

## Selective Breaking

Only count specific error types toward the failure threshold:

```csharp
var breaker = CircuitBreaker.WithFailureThreshold(3)
    .WithBreakWhen(error => error is ExternalServiceError);

// ValidationError or NotFoundError will NOT trip the breaker
var result = await breaker.ExecuteAsync(() => CallServiceAsync());
```

## The Open Error

When the circuit is open, `CircuitBreakerOpenError` is returned with a `RetryAfter` property indicating when the circuit will transition to half-open:

```csharp
var result = await breaker.ExecuteAsync(() => CallServiceAsync());

result.TapError(error =>
{
    if (error is CircuitBreakerOpenError openError)
    {
        logger.LogWarning(
            "Circuit open, retry after {RetryAfter}",
            openError.RetryAfter);
    }
});
```

## Observability

Monitor state transitions for logging, metrics, or alerting:

```csharp
var breaker = CircuitBreaker
    .WithFailureThreshold(5)
    .WithResetTimeout(TimeSpan.FromSeconds(30))
    .OnStateChange((from, to) =>
    {
        logger.LogWarning("Circuit breaker: {From} -> {To}", from, to);
        metrics.RecordCircuitStateChange(from, to);
    });
```

## Thread Safety

The circuit breaker is thread-safe. The policy configuration is immutable, while the internal state (failure count, current state, timestamps) is managed by a shared state tracker protected by a `Lock`. A single `CircuitBreakerPolicy` instance can be safely shared across multiple threads and concurrent calls to `Execute`/`ExecuteAsync`.

## Composition with Retry

Combine circuit breaker with retry for resilient service calls. Place the circuit breaker **inside** the retry so that open-circuit rejections are retried after the timeout:

```csharp
var breaker = CircuitBreaker
    .WithFailureThreshold(3)
    .WithResetTimeout(TimeSpan.FromSeconds(30))
    .WithBreakWhen(error => error is ExternalServiceError);

var result = await Retry
    .WithMaxAttempts(5)
    .WithBackoff(Backoff.Exponential(TimeSpan.FromMilliseconds(500)))
    .WithRetryWhen(error => error is ExternalServiceError or CircuitBreakerOpenError)
    .OnRetry((attempt, error) =>
        logger.LogWarning("Attempt {Attempt}: {Error}", attempt, error.Message))
    .ExecuteAsync(() =>
        breaker.ExecuteAsync(
            () => httpClient.GetResultAsync<Data>("/api/data")));
```

## Composition with Http and MemoizeResult

Build a full resilience stack with caching, circuit breaking, and retry:

```csharp
// Cache successful responses for 5 minutes
var cachedFetch = MemoizeResult.FuncAsync<string, Data, Error>(
    endpoint => Retry
        .WithMaxAttempts(3)
        .WithBackoff(Backoff.Exponential(TimeSpan.FromMilliseconds(200)))
        .ExecuteAsync(() =>
            breaker.ExecuteAsync(
                () => httpClient.GetResultAsync<Data>(endpoint))),
    opts => opts.WithExpiration(TimeSpan.FromMinutes(5)));

var result = await cachedFetch("/api/data");
// First call: cache miss → retry with circuit breaker → HTTP call
// Subsequent calls within 5 min: cache hit (if first call succeeded)
// If dependency is down: circuit opens after 3 failures, retries back off
```

# Retry

The `Retry` module provides configurable retry policies with backoff strategies. It integrates with `Result<T, TError>` — retries are driven by failures and produce a final Result.

## Basic Usage

```csharp
var result = Retry.WithMaxAttempts(3)
    .Execute(() => CallService());
// Retries up to 3 times, returns first Success or last Failure
```

## Builder API

Build retry policies fluently:

```csharp
var result = await Retry
    .WithMaxAttempts(5)
    .WithBackoff(Backoff.Exponential(TimeSpan.FromMilliseconds(100)))
    .WithRetryWhen(error => error is ExternalServiceError)
    .OnRetry((attempt, error) =>
        logger.LogWarning("Attempt {Attempt} failed: {Error}", attempt, error.Message))
    .ExecuteAsync(() => FetchFromApiAsync());
```

### Policy Options

| Method | Description |
|--------|-------------|
| `WithMaxAttempts(int)` | Maximum number of attempts (at least 1) |
| `WithBackoff(Func<int, TimeSpan>)` | Delay strategy between attempts |
| `WithRetryWhen(Func<Error, bool>)` | Predicate to filter retryable errors |
| `OnRetry(Action<int, Error>)` | Callback for logging/observability |

## Backoff Strategies

The `Backoff` class provides factory methods for common delay patterns:

```csharp
// No delay between retries
Backoff.None

// Same delay every time
Backoff.Constant(TimeSpan.FromSeconds(1))

// Linearly increasing: initial + (attempt - 1) * increment
Backoff.Linear(
    initial:   TimeSpan.FromMilliseconds(100),
    increment: TimeSpan.FromMilliseconds(200))
// Delays: 100ms, 300ms, 500ms, 700ms, ...

// Exponentially increasing: initial * multiplier^(attempt - 1)
Backoff.Exponential(TimeSpan.FromMilliseconds(100))
// Delays: 100ms, 200ms, 400ms, 800ms, ...

// Exponential with a cap
Backoff.Exponential(
    initial:    TimeSpan.FromMilliseconds(100),
    multiplier: 2.0,
    maxDelay:   TimeSpan.FromSeconds(5))
// Delays: 100ms, 200ms, 400ms, ..., capped at 5s
```

## Sync and Async

Both synchronous and asynchronous execution are supported:

```csharp
// Synchronous
Result<Data, Error> result = policy.Execute(() => LoadData());

// Asynchronous
Result<Data, Error> result = await policy.ExecuteAsync(() => LoadDataAsync());
```

## Selective Retry

Only retry specific error types:

```csharp
var result = await Retry.WithMaxAttempts(3)
    .WithRetryWhen(error => error is ExternalServiceError or InternalError)
    .ExecuteAsync(() => CallExternalServiceAsync());
// ValidationError or NotFoundError will NOT be retried
```

## Real-World Example

```csharp
var policy = Retry
    .WithMaxAttempts(5)
    .WithBackoff(Backoff.Exponential(
        TimeSpan.FromMilliseconds(200),
        multiplier: 2.0,
        maxDelay: TimeSpan.FromSeconds(10)))
    .WithRetryWhen(error => error is ExternalServiceError)
    .OnRetry((attempt, error) =>
        logger.LogWarning(
            "API call failed (attempt {Attempt}): {Error}",
            attempt, error.Message));

var result = await policy.ExecuteAsync(async () =>
{
    var response = await httpClient.GetAsync("/api/data");

    return response.IsSuccessStatusCode
        ? Result.Success<Data, ExternalServiceError>(
            await response.Content.ReadFromJsonAsync<Data>()!)
        : Result.Failure<Data, ExternalServiceError>(
            new ExternalServiceError { Message = $"HTTP {response.StatusCode}" });
});
```

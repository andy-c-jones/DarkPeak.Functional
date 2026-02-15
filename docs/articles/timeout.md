# Timeout

The `Timeout` module provides configurable timeout policies that wrap async operations and return `Result<T, TError>` on timeout instead of throwing exceptions. It integrates seamlessly with other resilience policies and supports cancellation token threading.

## Basic Usage

```csharp
var timeout = Timeout.Create()
    .WithTimeout(TimeSpan.FromSeconds(5));

var result = await timeout.ExecuteAsync<ApiResponse, Error>(
    async ct => await httpClient.GetResultAsync<ApiResponse>("/api/data", ct));
// Returns TimeoutError if operation takes longer than 5 seconds
```

## Builder API

Build timeout policies fluently:

```csharp
var timeout = Timeout.Create()
    .WithTimeout(TimeSpan.FromSeconds(10))
    .WithTimeoutError(elapsed => new ExternalServiceError
    {
        Message = $"Operation timed out after {elapsed.TotalSeconds}s",
        ServiceName = "PaymentGateway"
    });

var result = await timeout.ExecuteAsync<PaymentResult, Error>(
    async ct => await paymentService.ChargeAsync(request, ct));
```

### Policy Options

| Method | Description |
|--------|-------------|
| `WithTimeout(TimeSpan)` | Sets the maximum duration for the operation (must be greater than zero) |
| `WithTimeoutError(Func<TimeSpan, Error>)` | Sets a custom error factory to create the timeout error. Receives the elapsed time. |

## TimeoutError

When a timeout occurs, a `TimeoutError` is returned with details about the timeout:

```csharp
var result = await timeout.ExecuteAsync<Data, Error>(
    async ct => await SlowOperationAsync(ct));

result.TapError(error =>
{
    if (error is TimeoutError timeoutError)
    {
        logger.LogWarning(
            "Operation timed out. Allowed: {Timeout}, Elapsed: {Elapsed}",
            timeoutError.Timeout,
            timeoutError.Elapsed);
    }
});
```

The `TimeoutError` type includes:
- `Message` — Human-readable description
- `Timeout` — The configured timeout duration
- `Elapsed` — The actual time elapsed when timeout occurred

## Custom Timeout Errors

Use a custom error type instead of the default `TimeoutError`:

```csharp
var timeout = Timeout.Create()
    .WithTimeout(TimeSpan.FromSeconds(5))
    .WithTimeoutError(elapsed => new ExternalServiceError
    {
        Message = $"Payment gateway timeout after {elapsed.TotalSeconds:F2}s",
        ServiceName = "PaymentGateway",
        InnerMessage = "Request took too long to complete"
    });
```

## Cancellation Token Threading

The timeout policy properly threads cancellation tokens through to the wrapped operation:

```csharp
var timeout = Timeout.Create()
    .WithTimeout(TimeSpan.FromSeconds(10));

var cts = new CancellationTokenSource();

var result = await timeout.ExecuteAsync<Data, Error>(
    async ct =>
    {
        // ct will be cancelled when timeout is reached
        await httpClient.GetResultAsync<Data>("/api/data", ct);
    },
    cts.Token);

// External cancellation via cts.Token is also respected
```

If the operation is cancelled externally (not by the timeout), an `OperationCanceledException` is thrown as expected, rather than returning a `TimeoutError`.

## Plain Value Overload

For operations that don't already return `Result<T, TError>`, use the plain value overload:

```csharp
var timeout = Timeout.Create()
    .WithTimeout(TimeSpan.FromSeconds(5));

var result = await timeout.ExecuteAsync<string, Error>(
    async ct =>
    {
        // Returns plain value, wrapped in Result automatically
        return await File.ReadAllTextAsync("data.json", ct);
    });
// Returns Success<string> or Failure with TimeoutError
```

## Real-World Example

Using the `DarkPeak.Functional.Http` extensions, HTTP calls already return `Result<T, Error>`:

```csharp
public class ApiClient
{
    private readonly HttpClient _httpClient;
    private readonly TimeoutPolicy _timeout;

    public ApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _timeout = Timeout.Create()
            .WithTimeout(TimeSpan.FromSeconds(30))
            .WithTimeoutError(elapsed => new ExternalServiceError
            {
                Message = $"API call timed out after {elapsed.TotalSeconds:F1}s",
                ServiceName = "ExternalAPI"
            });
    }

    public async Task<Result<Order, Error>> GetOrderAsync(string orderId, CancellationToken ct = default)
    {
        return await _timeout.ExecuteAsync(
            ct => _httpClient.GetResultAsync<Order>($"/api/orders/{orderId}", ct),
            ct);
    }
}
```

## Composition with Retry

Timeout policies work seamlessly with retry:

```csharp
var retry = Retry.WithMaxAttempts(3)
    .WithBackoff(Backoff.Exponential(TimeSpan.FromMilliseconds(200)));

var timeout = Timeout.Create()
    .WithTimeout(TimeSpan.FromSeconds(5));

// Each retry attempt has a 5-second timeout
var result = await retry.ExecuteAsync(ct =>
    timeout.ExecuteAsync(
        ct => httpClient.GetResultAsync<Data>("/api/data", ct),
        ct));
```

Or use `ResiliencePolicy` for cleaner composition:

```csharp
var policy = ResiliencePolicy.Create<Error>()
    .WithTimeout(TimeSpan.FromSeconds(30))              // overall timeout
    .WithRetry(r => r
        .WithMaxAttempts(3)
        .WithBackoff(Backoff.Exponential(TimeSpan.FromMilliseconds(200)))
        .WithTimeout(TimeSpan.FromSeconds(5)))          // per-attempt timeout
    .Build();

var result = await policy.ExecuteAsync(
    ct => httpClient.GetResultAsync<Data>("/api/data", ct));
```

## Best Practices

### Choose Appropriate Timeouts

```csharp
// Short timeout for local services
var localTimeout = Timeout.Create()
    .WithTimeout(TimeSpan.FromSeconds(1));

// Longer timeout for external APIs
var externalTimeout = Timeout.Create()
    .WithTimeout(TimeSpan.FromSeconds(30));

// Very long timeout for batch operations
var batchTimeout = Timeout.Create()
    .WithTimeout(TimeSpan.FromMinutes(5));
```

### Timeout Error Handling

```csharp
var result = await timeout.ExecuteAsync<Data, Error>(operation);

result.Match(
    success: data => ProcessData(data),
    failure: error =>
    {
        if (error is TimeoutError timeoutError)
        {
            // Log timeout with details
            logger.LogWarning(
                "Timeout after {Elapsed}ms (limit: {Limit}ms)",
                timeoutError.Elapsed?.TotalMilliseconds,
                timeoutError.Timeout?.TotalMilliseconds);

            // Maybe trigger fallback behavior
            return GetCachedData();
        }
        // Handle other errors...
    });
```

### Timeout with Circuit Breaker

Timeouts can trigger circuit breakers:

```csharp
var breaker = CircuitBreaker.WithFailureThreshold(5)
    .WithResetTimeout(TimeSpan.FromMinutes(1))
    .WithBreakWhen(error => error is TimeoutError or ExternalServiceError);

var timeout = Timeout.Create()
    .WithTimeout(TimeSpan.FromSeconds(10));

var result = await breaker.ExecuteAsync(ct =>
    timeout.ExecuteAsync(
        ct => httpClient.GetResultAsync<Data>("/api/data", ct),
        ct));
```

## Thread Safety

Timeout policies are stateless and thread-safe. A single `TimeoutPolicy` instance can be safely shared across multiple threads and concurrent calls to `ExecuteAsync`.

## See Also

- [Retry](retry.md) — Retry failed operations with backoff
- [Circuit Breaker](circuit-breaker.md) — Prevent cascading failures
- [Bulkhead](bulkhead.md) — Limit concurrency to protect resources
- [Resilience Policy](resilience-policy.md) — Compose all resilience policies

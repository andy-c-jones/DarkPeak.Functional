# Resilience Policy

The `ResiliencePolicy` module provides a unified way to compose multiple resilience strategies (Timeout, Retry, Circuit Breaker, Bulkhead) into a single policy. Instead of manually nesting policies, use the fluent builder API to create a robust resilience stack.

## Why Compose Policies?

In cloud-native applications, resilience policies are rarely used in isolation. A typical scenario requires:

- **Overall Timeout** — Prevent operations from running indefinitely
- **Retry** — Handle transient failures automatically
- **Per-Attempt Timeout** — Limit each retry attempt
- **Circuit Breaker** — Stop calling failing dependencies
- **Bulkhead** — Limit concurrency to prevent resource exhaustion

Manual composition is verbose and error-prone. `ResiliencePolicy` handles the correct ordering and CancellationToken threading automatically.

## Basic Usage

```csharp
var policy = ResiliencePolicy.Create<Error>()
    .WithTimeout(TimeSpan.FromSeconds(30))
    .WithRetry(r => r
        .WithMaxAttempts(3)
        .WithBackoff(Backoff.Exponential(TimeSpan.FromMilliseconds(200))))
    .WithCircuitBreaker(cb => cb
        .WithFailureThreshold(5)
        .WithResetTimeout(TimeSpan.FromMinutes(1)))
    .WithBulkhead(b => b
        .WithMaxConcurrency(10))
    .Build();

var result = await policy.ExecuteAsync(
    async ct => await httpClient.GetResultAsync<Data>("/api/data", ct));
```

## Policy Ordering

Policies are applied in this order (outermost to innermost):

```
Overall Timeout
    └─> Retry
            └─> Per-Attempt Timeout
                    └─> Circuit Breaker
                            └─> Bulkhead
                                    └─> Your Operation
```

This ensures:
1. Overall timeout prevents infinite retries
2. Each retry attempt can have its own timeout
3. Circuit breaker protects downstream even during retries
4. Bulkhead limits concurrency at the innermost level

## Builder API

### Timeout Configuration

```csharp
// Overall timeout (wraps everything)
.WithTimeout(TimeSpan.FromSeconds(30))

// With custom error factory
.WithTimeout(
    TimeSpan.FromSeconds(30),
    elapsed => new ExternalServiceError
    {
        Message = $"Operation timed out after {elapsed.TotalSeconds}s",
        ServiceName = "PaymentGateway"
    })
```

### Retry Configuration

```csharp
.WithRetry(r => r
    .WithMaxAttempts(3)
    .WithBackoff(Backoff.Exponential(TimeSpan.FromMilliseconds(200)))
    .WithRetryWhen(error => error is ExternalServiceError or TimeoutError)
    .OnRetry((attempt, error) =>
        logger.LogWarning("Retry attempt {Attempt}: {Error}", attempt, error.Message))
    
    // Optional per-attempt timeout
    .WithTimeout(TimeSpan.FromSeconds(5)))
```

### Circuit Breaker Configuration

```csharp
.WithCircuitBreaker(cb => cb
    .WithFailureThreshold(5)
    .WithResetTimeout(TimeSpan.FromMinutes(1))
    .WithBreakWhen(error => error is ExternalServiceError)
    .OnStateChange((from, to) =>
        logger.LogWarning("Circuit breaker: {From} -> {To}", from, to)))
```

### Bulkhead Configuration

```csharp
.WithBulkhead(b => b
    .WithMaxConcurrency(10)
    .WithMaxQueueSize(20)
    .OnRejected(() =>
        logger.LogWarning("Bulkhead rejected request")))
```

## Real-World Example: Payment Gateway

A microservice calls an external payment provider that has occasional high latency and brief outages:

```csharp
public class PaymentGatewayClient
{
    private readonly HttpClient _httpClient;
    private readonly ResiliencePolicy<Error> _policy;

    public PaymentGatewayClient(HttpClient httpClient, ILogger<PaymentGatewayClient> logger)
    {
        _httpClient = httpClient;
        
        _policy = ResiliencePolicy.Create<Error>()
            // Overall timeout: prevent hanging forever
            .WithTimeout(TimeSpan.FromSeconds(30))
            
            // Retry: handle transient failures
            .WithRetry(r => r
                .WithMaxAttempts(3)
                .WithBackoff(Backoff.Exponential(TimeSpan.FromMilliseconds(500)))
                .WithTimeout(TimeSpan.FromSeconds(5))  // per-attempt timeout
                .WithRetryWhen(error => 
                    error is ExternalServiceError or TimeoutError)
                .OnRetry((attempt, error) =>
                    logger.LogWarning(
                        "Payment gateway retry {Attempt}: {Error}",
                        attempt, error.Message)))
            
            // Circuit breaker: stop calling if it's down
            .WithCircuitBreaker(cb => cb
                .WithFailureThreshold(10)
                .WithResetTimeout(TimeSpan.FromMinutes(2))
                .WithBreakWhen(error => error is ExternalServiceError)
                .OnStateChange((from, to) =>
                {
                    if (to == CircuitBreakerState.Open)
                        logger.LogError("Payment gateway circuit opened!");
                }))
            
            // Bulkhead: limit concurrent requests
            .WithBulkhead(b => b
                .WithMaxConcurrency(20)
                .WithMaxQueueSize(50)
                .OnRejected(() =>
                    logger.LogWarning("Payment gateway bulkhead full")))
            
            .Build();
    }

    public async Task<Result<PaymentResult, Error>> ChargeAsync(
        PaymentRequest request,
        CancellationToken ct = default)
    {
        return await _policy.ExecuteAsync(
            async token => await _httpClient.PostResultAsync<PaymentResult>(
                "/v1/charges", 
                request,
                token),
            ct);
    }
}
```

Without composition, the same logic requires deeply nested callbacks:

```csharp
// Manual nesting (verbose and error-prone)
var result = await overallTimeout.ExecuteAsync(async ct1 =>
    await retry.ExecuteAsync(async ct2 =>
        await perAttemptTimeout.ExecuteAsync(async ct3 =>
            await breaker.ExecuteAsync(async ct4 =>
                await bulkhead.ExecuteAsync(async ct5 =>
                    await httpClient.PostResultAsync<PaymentResult>("/v1/charges", request, ct5),
                    ct4),
                ct3),
            ct2),
        ct1),
    ct);
```

## Per-Attempt vs Overall Timeout

The distinction between per-attempt and overall timeout is important:

```csharp
var policy = ResiliencePolicy.Create<Error>()
    .WithTimeout(TimeSpan.FromSeconds(10))  // Overall: max 10s total
    .WithRetry(r => r
        .WithMaxAttempts(5)
        .WithBackoff(Backoff.Exponential(TimeSpan.FromMilliseconds(200)))
        .WithTimeout(TimeSpan.FromSeconds(2)))  // Per-attempt: max 2s each
    .Build();

// Scenario:
// - Attempt 1: times out after 2s → retry
// - Attempt 2: times out after 2s → retry
// - Attempt 3: times out after 2s → retry
// - Attempt 4: times out after 2s → retry
// - Overall timeout hits at ~9s → stops retrying
```

Without per-attempt timeout:

```csharp
var policy = ResiliencePolicy.Create<Error>()
    .WithTimeout(TimeSpan.FromSeconds(10))  // Overall only
    .WithRetry(r => r.WithMaxAttempts(5))
    .Build();

// Problem: Each attempt could take up to 10s
// Total time could be 10s * 5 = 50s if not for overall timeout
```

## CancellationToken Threading

The policy threads `CancellationToken` through all layers automatically:

```csharp
var policy = ResiliencePolicy.Create<Error>()
    .WithTimeout(TimeSpan.FromSeconds(30))
    .WithRetry(r => r.WithMaxAttempts(3))
    .WithCircuitBreaker(cb => cb.WithFailureThreshold(5))
    .WithBulkhead(b => b.WithMaxConcurrency(10))
    .Build();

var cts = new CancellationTokenSource();
cts.CancelAfter(5000); // Cancel after 5 seconds

var result = await policy.ExecuteAsync(
    async ct =>
    {
        // ct will be cancelled when:
        // - cts is cancelled
        // - Overall timeout is reached
        // - Per-attempt timeout is reached (if configured)
        await httpClient.GetResultAsync<Data>("/api/data", ct);
    },
    cts.Token);
```

## Partial Policy Composition

You don't need to configure all policies. Use only what you need:

```csharp
// Just timeout and retry
var policy = ResiliencePolicy.Create<Error>()
    .WithTimeout(TimeSpan.FromSeconds(10))
    .WithRetry(r => r.WithMaxAttempts(3))
    .Build();

// Just circuit breaker and bulkhead
var policy = ResiliencePolicy.Create<Error>()
    .WithCircuitBreaker(cb => cb.WithFailureThreshold(5))
    .WithBulkhead(b => b.WithMaxConcurrency(10))
    .Build();

// Just retry with per-attempt timeout
var policy = ResiliencePolicy.Create<Error>()
    .WithRetry(r => r
        .WithMaxAttempts(3)
        .WithTimeout(TimeSpan.FromSeconds(5)))
    .Build();
```

## Plain Value Overload

For operations that don't return `Result<T, TError>`:

```csharp
var policy = ResiliencePolicy.Create<Error>()
    .WithTimeout(TimeSpan.FromSeconds(10))
    .WithRetry(r => r.WithMaxAttempts(3))
    .Build();

var result = await policy.ExecuteAsync<string>(
    async ct =>
    {
        // Returns plain value, wrapped in Result automatically
        return await File.ReadAllTextAsync("data.json", ct);
    });
// Returns Success<string> or Failure with appropriate error
```

## Error Handling Across Policies

Each policy can produce its own error type:

- `TimeoutError` — From timeout policy
- `TimeoutError` — From per-attempt timeout in retry
- `CircuitBreakerOpenError` — From circuit breaker
- `BulkheadRejectedError` — From bulkhead

Configure retry to handle specific errors:

```csharp
var policy = ResiliencePolicy.Create<Error>()
    .WithRetry(r => r
        .WithMaxAttempts(3)
        .WithRetryWhen(error =>
            error is ExternalServiceError or
                     TimeoutError or
                     CircuitBreakerOpenError))  // Retry on circuit open
    .WithCircuitBreaker(cb => cb
        .WithFailureThreshold(5)
        .WithBreakWhen(error => error is ExternalServiceError))
    .Build();
```

## Best Practices

### Start Simple, Add Complexity

```csharp
// Phase 1: Just retry
var policy = ResiliencePolicy.Create<Error>()
    .WithRetry(r => r.WithMaxAttempts(3))
    .Build();

// Phase 2: Add timeouts
var policy = ResiliencePolicy.Create<Error>()
    .WithTimeout(TimeSpan.FromSeconds(30))
    .WithRetry(r => r
        .WithMaxAttempts(3)
        .WithTimeout(TimeSpan.FromSeconds(5)))
    .Build();

// Phase 3: Add circuit breaker after observing failures
var policy = ResiliencePolicy.Create<Error>()
    .WithTimeout(TimeSpan.FromSeconds(30))
    .WithRetry(r => r.WithMaxAttempts(3).WithTimeout(TimeSpan.FromSeconds(5)))
    .WithCircuitBreaker(cb => cb.WithFailureThreshold(10))
    .Build();
```

### Configure Based on SLAs

```csharp
// External API SLA: 99% of requests < 500ms, 99.9% < 2s
var policy = ResiliencePolicy.Create<Error>()
    .WithTimeout(TimeSpan.FromSeconds(30))      // Overall budget
    .WithRetry(r => r
        .WithMaxAttempts(3)
        .WithBackoff(Backoff.Exponential(TimeSpan.FromMilliseconds(100)))
        .WithTimeout(TimeSpan.FromSeconds(2)))  // Per-attempt at p99.9
    .WithCircuitBreaker(cb => cb
        .WithFailureThreshold(10)
        .WithResetTimeout(TimeSpan.FromMinutes(1)))
    .Build();
```

### Log at Every Layer

```csharp
var policy = ResiliencePolicy.Create<Error>()
    .WithTimeout(
        TimeSpan.FromSeconds(30),
        elapsed => 
        {
            logger.LogError("Overall timeout after {Elapsed}s", elapsed.TotalSeconds);
            return new TimeoutError { Message = "Overall timeout", Elapsed = elapsed };
        })
    .WithRetry(r => r
        .WithMaxAttempts(3)
        .OnRetry((attempt, error) =>
            logger.LogWarning("Retry {Attempt}: {Error}", attempt, error.Message)))
    .WithCircuitBreaker(cb => cb
        .WithFailureThreshold(5)
        .OnStateChange((from, to) =>
            logger.LogWarning("Circuit: {From} -> {To}", from, to)))
    .WithBulkhead(b => b
        .WithMaxConcurrency(10)
        .OnRejected(() =>
            logger.LogWarning("Bulkhead rejected")))
    .Build();
```

### Use Metrics and Observability

```csharp
public class InstrumentedPaymentClient
{
    private readonly ResiliencePolicy<Error> _policy;
    private readonly IMetrics _metrics;

    public InstrumentedPaymentClient(IMetrics metrics)
    {
        _metrics = metrics;
        
        _policy = ResiliencePolicy.Create<Error>()
            .WithRetry(r => r
                .WithMaxAttempts(3)
                .OnRetry((attempt, _) =>
                    _metrics.Increment("payment.retry", new { attempt })))
            .WithCircuitBreaker(cb => cb
                .WithFailureThreshold(5)
                .OnStateChange((from, to) =>
                {
                    _metrics.RecordStateChange("payment.circuit", from, to);
                    if (to == CircuitBreakerState.Open)
                        _metrics.Increment("payment.circuit.opened");
                }))
            .WithBulkhead(b => b
                .WithMaxConcurrency(10)
                .OnRejected(() =>
                    _metrics.Increment("payment.bulkhead.rejected")))
            .Build();
    }
}
```

## Thread Safety

Resilience policies are thread-safe. The policy configuration is immutable. Internal state (circuit breaker state, bulkhead concurrency) is managed by shared state trackers protected by locks. A single `ResiliencePolicy<TError>` instance can be safely shared across multiple threads and concurrent calls to `ExecuteAsync`.

## Performance Considerations

The resilience policy adds minimal overhead:
- No allocations beyond what individual policies require
- CancellationToken linking is efficient
- Policy composition is evaluated once at build time

For high-throughput scenarios, consider:
1. Reusing policy instances (don't build per-request)
2. Tuning bulkhead queue sizes to avoid memory pressure
3. Using appropriate timeout values to avoid unnecessary waiting

## Common Patterns

### Different Policies for Different Endpoints

```csharp
public class ApiClient
{
    private readonly ResiliencePolicy<Error> _readPolicy;
    private readonly ResiliencePolicy<Error> _writePolicy;

    public ApiClient()
    {
        // Reads: aggressive timeout, more retries
        _readPolicy = ResiliencePolicy.Create<Error>()
            .WithTimeout(TimeSpan.FromSeconds(10))
            .WithRetry(r => r.WithMaxAttempts(5))
            .WithCircuitBreaker(cb => cb.WithFailureThreshold(10))
            .Build();

        // Writes: longer timeout, fewer retries
        _writePolicy = ResiliencePolicy.Create<Error>()
            .WithTimeout(TimeSpan.FromSeconds(30))
            .WithRetry(r => r.WithMaxAttempts(2))
            .WithCircuitBreaker(cb => cb.WithFailureThreshold(5))
            .Build();
    }
}
```

### Fallback on Policy Failure

```csharp
var result = await policy.ExecuteAsync(operation);

return result.Match(
    success: data => data,
    failure: error =>
    {
        logger.LogWarning("Primary failed with {Error}, using fallback", error);
        return GetCachedData();
    });
```

## See Also

- [Timeout](timeout.md) — Time-limit operations
- [Retry](retry.md) — Retry failed operations with backoff
- [Circuit Breaker](circuit-breaker.md) — Prevent cascading failures
- [Bulkhead](bulkhead.md) — Limit concurrency to protect resources

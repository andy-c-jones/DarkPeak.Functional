# Bulkhead

The `Bulkhead` module provides concurrency limiting to prevent resource exhaustion. It integrates with `Result<T, TError>` — requests are queued when the limit is reached and rejected with `BulkheadRejectedError` when the queue is full.

## Basic Usage

```csharp
var bulkhead = Bulkhead.WithMaxConcurrency(10)
    .WithMaxQueueSize(20);

var result = await bulkhead.ExecuteAsync<Data, Error>(
    async ct => await httpClient.GetResultAsync<Data>("/api/data", ct));
// Limits to 10 concurrent operations, queues up to 20 more
```

## Builder API

Build bulkhead policies fluently:

```csharp
var bulkhead = Bulkhead
    .WithMaxConcurrency(5)
    .WithMaxQueueSize(10)
    .OnRejected(() =>
        logger.LogWarning("Request rejected - bulkhead capacity reached"));

var result = await bulkhead.ExecuteAsync<Response, Error>(
    async ct => await CallDownstreamServiceAsync(ct));
```

### Policy Options

| Method | Description |
|--------|-------------|
| `WithMaxConcurrency(int)` | Maximum number of concurrent operations (at least 1) |
| `WithMaxQueueSize(int)` | Maximum number of requests waiting for a slot (at least 0) |
| `OnRejected(Action)` | Callback invoked when a request is rejected |

## State Management

The bulkhead tracks:
- **Current Concurrency** — Number of operations currently executing
- **Queue Size** — Number of operations waiting for a slot

When a request arrives:
1. If current concurrency < max concurrency → Execute immediately
2. Else if queue size < max queue size → Queue and wait
3. Else → Reject with `BulkheadRejectedError`

## BulkheadRejectedError

When capacity is exceeded, a `BulkheadRejectedError` is returned:

```csharp
var result = await bulkhead.ExecuteAsync<Data, Error>(operation);

result.TapError(error =>
{
    if (error is BulkheadRejectedError rejectedError)
    {
        logger.LogWarning(
            "Bulkhead rejected: max concurrency={MaxConcurrency}, queue size={QueueSize}",
            rejectedError.MaxConcurrency,
            rejectedError.MaxQueueSize);
        
        // Maybe try again later or use a fallback
    }
});
```

The `BulkheadRejectedError` type includes:
- `Message` — Human-readable description
- `MaxConcurrency` — The configured maximum concurrency
- `MaxQueueSize` — The configured maximum queue size

## Cancellation Token Support

The bulkhead respects cancellation tokens:

```csharp
var bulkhead = Bulkhead.WithMaxConcurrency(5)
    .WithMaxQueueSize(10);

var cts = new CancellationTokenSource();

var result = await bulkhead.ExecuteAsync<Data, Error>(
    async ct =>
    {
        await Task.Delay(1000, ct); // Will be cancelled if cts is cancelled
        return Result.Success<Data, Error>(data);
    },
    cts.Token);

// If cancelled while queued, returns false without executing
```

## Plain Value Overload

For operations that don't already return `Result<T, TError>`:

```csharp
var bulkhead = Bulkhead.WithMaxConcurrency(3);

var result = await bulkhead.ExecuteAsync<string, Error>(
    async ct =>
    {
        // Returns plain value, wrapped in Result automatically
        return await File.ReadAllTextAsync("data.json", ct);
    });
// Returns Success<string> or Failure with BulkheadRejectedError
```

## Real-World Example: Database Connection Pool

Limit concurrent database operations to avoid exhausting the connection pool:

```csharp
public class OrderRepository
{
    private readonly BulkheadPolicy _bulkhead;
    private readonly IDbConnection _connection;

    public OrderRepository(IDbConnection connection)
    {
        _connection = connection;
        
        // Limit to 20 concurrent queries, queue up to 50
        _bulkhead = Bulkhead.WithMaxConcurrency(20)
            .WithMaxQueueSize(50)
            .OnRejected(() =>
                logger.LogWarning("Database bulkhead rejected request"));
    }

    public async Task<Result<Order, Error>> GetOrderAsync(string orderId, CancellationToken ct = default)
    {
        return await _bulkhead.ExecuteAsync(
            async ct =>
            {
                var order = await _connection.QuerySingleOrDefaultAsync<Order>(
                    "SELECT * FROM Orders WHERE Id = @Id",
                    new { Id = orderId });
                    
                return order != null
                    ? Result.Success<Order, Error>(order)
                    : Result.Failure<Order, Error>(new NotFoundError 
                    { 
                        Message = "Order not found",
                        ResourceId = orderId 
                    });
            },
            ct);
    }
}
```

## Real-World Example: Rate Limiting API Calls

Protect an external API from too many concurrent requests:

```csharp
public class WeatherApiClient
{
    private readonly HttpClient _httpClient;
    private readonly BulkheadPolicy _bulkhead;

    public WeatherApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
        
        // API allows max 5 concurrent requests
        _bulkhead = Bulkhead.WithMaxConcurrency(5)
            .WithMaxQueueSize(10)
            .OnRejected(() =>
                logger.LogWarning("Weather API rate limit reached"));
    }

    public async Task<Result<WeatherData, Error>> GetWeatherAsync(
        string city, 
        CancellationToken ct = default)
    {
        return await _bulkhead.ExecuteAsync(
            ct => _httpClient.GetResultAsync<WeatherData>($"/api/weather?city={city}", ct),
            ct);
    }
}
```

## Composition with Retry

Combine bulkhead with retry to handle temporary capacity issues:

```csharp
var bulkhead = Bulkhead.WithMaxConcurrency(5)
    .WithMaxQueueSize(10);

var retry = Retry.WithMaxAttempts(3)
    .WithBackoff(Backoff.Linear(TimeSpan.FromMilliseconds(100), TimeSpan.FromMilliseconds(100)))
    .WithRetryWhen(error => error is BulkheadRejectedError);

var result = await retry.ExecuteAsync(ct =>
    bulkhead.ExecuteAsync(
        ct => httpClient.GetResultAsync<Data>("/api/data", ct),
        ct));
```

Or use `ResiliencePolicy`:

```csharp
var policy = ResiliencePolicy.Create<Error>()
    .WithRetry(r => r
        .WithMaxAttempts(3)
        .WithBackoff(Backoff.Constant(TimeSpan.FromMilliseconds(100)))
        .WithRetryWhen(error => error is BulkheadRejectedError))
    .WithBulkhead(b => b
        .WithMaxConcurrency(10)
        .WithMaxQueueSize(20))
    .Build();

var result = await policy.ExecuteAsync(operation);
```

## Composition with Circuit Breaker

Use bulkhead inside a circuit breaker to protect resources:

```csharp
var policy = ResiliencePolicy.Create<Error>()
    .WithCircuitBreaker(cb => cb
        .WithFailureThreshold(10)
        .WithResetTimeout(TimeSpan.FromMinutes(1)))
    .WithBulkhead(b => b
        .WithMaxConcurrency(20)
        .WithMaxQueueSize(50))
    .Build();

// Circuit breaker prevents cascading failures
// Bulkhead limits resource usage
var result = await policy.ExecuteAsync(operation);
```

## Observability

Monitor bulkhead rejection rates:

```csharp
var rejections = 0;
var bulkhead = Bulkhead.WithMaxConcurrency(10)
    .WithMaxQueueSize(20)
    .OnRejected(() =>
    {
        Interlocked.Increment(ref rejections);
        metrics.RecordBulkheadRejection();
        logger.LogWarning("Bulkhead rejected request #{Count}", rejections);
    });
```

## Best Practices

### Choose Appropriate Limits

```csharp
// For CPU-bound operations, limit to processor count
var cpuBulkhead = Bulkhead.WithMaxConcurrency(Environment.ProcessorCount);

// For I/O-bound operations, can be higher
var ioBulkhead = Bulkhead.WithMaxConcurrency(100)
    .WithMaxQueueSize(200);

// For external APIs, respect their rate limits
var apiBulkhead = Bulkhead.WithMaxConcurrency(10)
    .WithMaxQueueSize(0); // No queue, immediate rejection
```

### Handle Rejections Gracefully

```csharp
var result = await bulkhead.ExecuteAsync<Data, Error>(operation);

result.Match(
    success: data => ProcessData(data),
    failure: error =>
    {
        if (error is BulkheadRejectedError)
        {
            // Return cached data or default value
            logger.LogWarning("Using cached data due to bulkhead rejection");
            return GetCachedData();
        }
        // Handle other errors...
    });
```

### Configure Queue Size Based on Load

```csharp
// For bursty traffic, larger queue
var burstyBulkhead = Bulkhead.WithMaxConcurrency(10)
    .WithMaxQueueSize(100);

// For steady traffic, smaller queue
var steadyBulkhead = Bulkhead.WithMaxConcurrency(10)
    .WithMaxQueueSize(10);

// For critical paths, no queue (fail fast)
var criticalBulkhead = Bulkhead.WithMaxConcurrency(5)
    .WithMaxQueueSize(0);
```

## Thread Safety

Bulkhead policies are thread-safe. The policy configuration is immutable, while internal state (current concurrency, queue) is managed by a shared state tracker protected by locks. A single `BulkheadPolicy` instance can be safely shared across multiple threads and concurrent calls to `ExecuteAsync`.

## Performance Considerations

The bulkhead uses an internal queue implemented with `TaskCompletionSource` for efficient async waiting. No polling or busy-waiting occurs.

### Memory Usage

Each queued request allocates a `TaskCompletionSource<bool>`. For applications with large queue sizes and high rejection rates, consider:

1. Reducing queue size
2. Implementing backpressure upstream
3. Using multiple bulkheads for different priorities

## Common Patterns

### Priority Bulkheads

Use separate bulkheads for different priority levels:

```csharp
public class ApiClient
{
    private readonly BulkheadPolicy _highPriority;
    private readonly BulkheadPolicy _lowPriority;

    public ApiClient()
    {
        _highPriority = Bulkhead.WithMaxConcurrency(20)
            .WithMaxQueueSize(10);
            
        _lowPriority = Bulkhead.WithMaxConcurrency(5)
            .WithMaxQueueSize(5);
    }

    public Task<Result<Data, Error>> GetCriticalDataAsync(CancellationToken ct) =>
        _highPriority.ExecuteAsync(ct => FetchDataAsync(ct), ct);

    public Task<Result<Data, Error>> GetNonCriticalDataAsync(CancellationToken ct) =>
        _lowPriority.ExecuteAsync(ct => FetchDataAsync(ct), ct);
}
```

### Bulkhead per Downstream Service

Isolate failures by using separate bulkheads for each dependency:

```csharp
public class AggregatorService
{
    private readonly BulkheadPolicy _authServiceBulkhead;
    private readonly BulkheadPolicy _paymentServiceBulkhead;
    private readonly BulkheadPolicy _inventoryServiceBulkhead;

    public AggregatorService()
    {
        _authServiceBulkhead = Bulkhead.WithMaxConcurrency(10);
        _paymentServiceBulkhead = Bulkhead.WithMaxConcurrency(5);
        _inventoryServiceBulkhead = Bulkhead.WithMaxConcurrency(15);
    }
    
    // Each service has its own concurrency limit
}
```

## See Also

- [Retry](retry.md) — Retry failed operations with backoff
- [Circuit Breaker](circuit-breaker.md) — Prevent cascading failures
- [Timeout](timeout.md) — Time-limit operations
- [Resilience Policy](resilience-policy.md) — Compose all resilience policies

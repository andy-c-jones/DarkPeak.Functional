# Http

The `DarkPeak.Functional.Http` package wraps `HttpClient` operations in `Result<T, Error>`, enabling railway-oriented HTTP communication without try/catch blocks.

## Installation

```bash
dotnet add package DarkPeak.Functional.Http
```

## Basic Usage

```csharp
using DarkPeak.Functional;
using DarkPeak.Functional.Http;

var result = await httpClient.GetResultAsync<Order>("/api/orders/123");

var message = result.Match(
    success: order => $"Order {order.Id} totals {order.Total:C}",
    failure: error => $"Failed: {error.Message}");
```

## JSON Methods

All JSON methods deserialize the response body using `System.Text.Json`:

```csharp
// GET
var order = await httpClient.GetResultAsync<Order>("/api/orders/123");

// POST
var created = await httpClient.PostResultAsync<Order>("/api/orders", newOrder);

// PUT
var updated = await httpClient.PutResultAsync<Order>("/api/orders/123", changes);

// PATCH
var patched = await httpClient.PatchResultAsync<Order>("/api/orders/123", patch);

// DELETE (no response body)
var deleted = await httpClient.DeleteResultAsync("/api/orders/123");

// DELETE (with response body)
var confirmation = await httpClient.DeleteResultAsync<DeletionResult>("/api/orders/123");

// Custom request
var request = new HttpRequestMessage(HttpMethod.Options, "/api/orders");
var options = await httpClient.SendResultAsync<OptionsResponse>(request);

// Custom request (no response body)
var head = new HttpRequestMessage(HttpMethod.Head, "/api/health");
var health = await httpClient.SendResultAsync(head);
```

### Custom JSON Options

All JSON methods accept optional `JsonSerializerOptions`:

```csharp
var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
var result = await httpClient.GetResultAsync<Order>("/api/orders/123", options);
```

## Non-JSON Response Types

For responses that are not JSON, use the typed GET methods:

```csharp
// String — plain text, XML, HTML
var html = await httpClient.GetStringResultAsync("/api/reports/summary");

// Stream — large files, binary data (uses ResponseHeadersRead for streaming)
var stream = await httpClient.GetStreamResultAsync("/api/exports/report.csv");

// Bytes — images, files, binary content
var image = await httpClient.GetBytesResultAsync("/api/images/logo.png");
```

The stream variant uses `HttpCompletionOption.ResponseHeadersRead` so the response body is not buffered — the caller is responsible for disposing the returned `Stream`.

### API Reference

| Method | Return Type | Use Case |
|--------|-------------|----------|
| `GetResultAsync<T>` | `Result<T, Error>` | JSON deserialization |
| `PostResultAsync<T>` | `Result<T, Error>` | JSON POST with response |
| `PutResultAsync<T>` | `Result<T, Error>` | JSON PUT with response |
| `PatchResultAsync<T>` | `Result<T, Error>` | JSON PATCH with response |
| `DeleteResultAsync` | `Result<Unit, Error>` | DELETE without body |
| `DeleteResultAsync<T>` | `Result<T, Error>` | DELETE with JSON response |
| `SendResultAsync<T>` | `Result<T, Error>` | Custom request, JSON response |
| `SendResultAsync` | `Result<Unit, Error>` | Custom request, no body |
| `GetStringResultAsync` | `Result<string, Error>` | Plain text / XML / HTML |
| `GetStreamResultAsync` | `Result<Stream, Error>` | Streaming binary data |
| `GetBytesResultAsync` | `Result<byte[], Error>` | Binary content as byte array |

## Request Configuration

All methods have an overload accepting `Action<HttpRequestMessage>` for per-request customization such as adding headers, authentication, or correlation IDs:

```csharp
var result = await httpClient.GetResultAsync<Order>("/api/orders/123", request =>
{
    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
    request.Headers.Add("X-Correlation-Id", correlationId);
});
```

This works with all method types:

```csharp
// POST with auth
var created = await httpClient.PostResultAsync<Order>("/api/orders", newOrder, request =>
{
    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
});

// DELETE with auth
var deleted = await httpClient.DeleteResultAsync("/api/orders/123", request =>
{
    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
});

// Stream with auth
var stream = await httpClient.GetStreamResultAsync("/api/exports/report.csv", request =>
{
    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
});

// PATCH with ETag
var patched = await httpClient.PatchResultAsync<Order>("/api/orders/123", patch, request =>
{
    request.Headers.Add("If-Match", etag);
});
```

## Error Mapping

Non-success HTTP status codes are automatically mapped to the most specific `Error` subtype:

| Status Code | Error Type |
|-------------|------------|
| 400 Bad Request | `BadRequestError` |
| 401 Unauthorized | `UnauthorizedError` |
| 403 Forbidden | `ForbiddenError` |
| 404 Not Found | `NotFoundError` |
| 409 Conflict | `ConflictError` |
| 422 Unprocessable Entity | `ValidationError` |
| 5xx Server Error | `ExternalServiceError` |
| Other | `HttpError` |

Transport-level failures (network errors, DNS failures, timeouts) are captured as `HttpRequestError`.

```csharp
var result = await httpClient.GetResultAsync<Order>("/api/orders/123");

result.TapError(error =>
{
    switch (error)
    {
        case NotFoundError:
            logger.LogWarning("Order not found");
            break;
        case UnauthorizedError:
            logger.LogWarning("Authentication required");
            break;
        case ExternalServiceError e:
            logger.LogError("Server error: {Message}", e.Message);
            break;
        case HttpRequestError e:
            logger.LogError("Network failure: {Type}", e.ExceptionType);
            break;
    }
});
```

## Chaining with Map and Bind

Results compose naturally with the core library's `Map` and `Bind`:

```csharp
// Transform the success value
var orderId = await httpClient
    .PostResultAsync<Order>("/api/orders", newOrder)
    .Map(order => order.Id);

// Chain dependent calls
var invoice = await httpClient
    .GetResultAsync<Order>("/api/orders/123")
    .BindAsync(order =>
        httpClient.GetResultAsync<Invoice>($"/api/invoices/{order.InvoiceId}"));
```

## Composition with Retry

Wrap HTTP calls in a retry policy for transient failure handling:

```csharp
var result = await Retry
    .WithMaxAttempts(3)
    .WithBackoff(Backoff.Exponential(TimeSpan.FromMilliseconds(200)))
    .WithRetryWhen(error => error is ExternalServiceError or HttpRequestError)
    .OnRetry((attempt, error) =>
        logger.LogWarning("Attempt {Attempt}: {Error}", attempt, error.Message))
    .ExecuteAsync(() =>
        httpClient.GetResultAsync<Data>("/api/data"));
```

## Composition with Circuit Breaker

Protect against cascading failures from a consistently failing dependency:

```csharp
var breaker = CircuitBreaker
    .WithFailureThreshold(5)
    .WithResetTimeout(TimeSpan.FromSeconds(30))
    .WithBreakWhen(error => error is ExternalServiceError or HttpRequestError);

var result = await breaker.ExecuteAsync(
    () => httpClient.GetResultAsync<Data>("/api/data"));
```

## Composition with MemoizeResult

Cache successful HTTP responses while allowing failed requests to be retried:

```csharp
var cachedGet = MemoizeResult.FuncAsync<string, UserProfile, Error>(
    endpoint => httpClient.GetResultAsync<UserProfile>(endpoint),
    opts => opts
        .WithExpiration(TimeSpan.FromMinutes(5))
        .WithMaxSize(1000));

var profile = await cachedGet("/api/users/123");
// First call: HTTP request, caches on success
// Second call within 5 min: returns cached result
// If first call failed: second call retries the HTTP request
```

## Full Resilience Stack

Combine caching, circuit breaking, retry, and Http extensions for production-grade resilience:

```csharp
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
        .WithExpiration(TimeSpan.FromMinutes(10))
        .WithMaxSize(500));

// Usage
var result = await cachedFetch("/api/catalog/item-42");
```

**Request flow:** Cache check → Retry loop → Circuit breaker → HTTP call

On success, the result is cached. On transient failure, the retry policy backs off and retries. If the dependency is consistently failing, the circuit opens and subsequent calls are rejected immediately until the reset timeout elapses.

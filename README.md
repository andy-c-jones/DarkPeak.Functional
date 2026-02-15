# DarkPeak.Functional

[![CI](https://github.com/andy-c-jones/DarkPeak.Functional/actions/workflows/ci.yml/badge.svg)](https://github.com/andy-c-jones/DarkPeak.Functional/actions/workflows/ci.yml)

A functional programming library for .NET providing monadic types and railway-oriented programming patterns.

## Packages

| Package | Version | Description |
|---------|---------|-------------|
| **DarkPeak.Functional** | [![NuGet](https://img.shields.io/nuget/v/DarkPeak.Functional.svg)](https://www.nuget.org/packages/DarkPeak.Functional/) | Core library providing monadic types (`Option`, `Result`, `Either`, `Validation`), `IAsyncEnumerable` functional extensions, retry policies with backoff strategies, circuit breaker, memoization with TTL/LRU support, and `MemoizeResult` for caching only successful `Result` values. |
| **DarkPeak.Functional.Http** | [![NuGet](https://img.shields.io/nuget/v/DarkPeak.Functional.Http.svg)](https://www.nuget.org/packages/DarkPeak.Functional.Http/) | Wraps `HttpClient` operations in `Result<T, Error>` for type-safe, exception-free HTTP communication. Supports JSON, string, stream, and byte array responses with per-request header customization. |
| **DarkPeak.Functional.AspNet** | [![NuGet](https://img.shields.io/nuget/v/DarkPeak.Functional.AspNet.svg)](https://www.nuget.org/packages/DarkPeak.Functional.AspNet/) | ASP.NET integration that converts `Result<T, Error>` to `IResult` and `ProblemDetails` for idiomatic minimal API error handling. |
| **DarkPeak.Functional.Redis** | [![NuGet](https://img.shields.io/nuget/v/DarkPeak.Functional.Redis.svg)](https://www.nuget.org/packages/DarkPeak.Functional.Redis/) | Redis distributed cache provider implementing `ICacheProvider<TKey, TValue>` for use with `Memoize` and `MemoizeResult`. |
| **DarkPeak.Functional.Dapper** | [![NuGet](https://img.shields.io/nuget/v/DarkPeak.Functional.Dapper.svg)](https://www.nuget.org/packages/DarkPeak.Functional.Dapper/) | Wraps Dapper queries and commands in `Result<T, Error>` with typed `DatabaseError` mapping and transaction support. |
| **DarkPeak.Functional.EntityFramework** | [![NuGet](https://img.shields.io/nuget/v/DarkPeak.Functional.EntityFramework.svg)](https://www.nuget.org/packages/DarkPeak.Functional.EntityFramework/) | Wraps EF Core operations in `Result<T, Error>` with typed errors for concurrency, save failures, and general database exceptions. |

All types support `Map`, `Bind`, `Match`, LINQ query syntax, and async variants.

## Example

```csharp
using DarkPeak.Functional;
using DarkPeak.Functional.Extensions;

// Cache successful fetches for 5 minutes — failures are never cached
var fetchUser = MemoizeResult.FuncAsync<int, User, Error>(
    id => httpClient.GetResultAsync<User>($"/users/{id}"),
    opts => opts.WithExpiration(TimeSpan.FromMinutes(5)));

var fetchOrder = MemoizeResult.FuncAsync<int, Order, Error>(
    id => httpClient.GetResultAsync<Order>($"/orders/{id}"),
    opts => opts.WithExpiration(TimeSpan.FromMinutes(5)));

// Run both fetches concurrently, then chain the result
var summary = await fetchUser(42)
    .Join(fetchOrder(7))                                     // concurrent via Task.WhenAll
    .Map((user, order) => new Summary(user.Name, order.Total))
    .Tap(s => logger.LogInformation("Built summary for {Name}", s.Name))
    .TapError(err => logger.LogError("Failed: {Msg}", err.Message));
```

## Building

```bash
dotnet build
dotnet test
```

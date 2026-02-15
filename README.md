# DarkPeak.Functional

[![CI](https://github.com/andy-c-jones/DarkPeak.Functional/actions/workflows/ci.yml/badge.svg)](https://github.com/andy-c-jones/DarkPeak.Functional/actions/workflows/ci.yml)

A functional programming library for .NET providing monadic types and railway-oriented programming patterns.

## Packages

| Package | Version | Description |
|---------|---------|-------------|
| **DarkPeak.Functional** | [![NuGet](https://img.shields.io/nuget/v/DarkPeak.Functional.svg)](https://www.nuget.org/packages/DarkPeak.Functional/) | Core library providing monadic types (`Option`, `Result`, `Either`, `Validation`), retry policies with backoff strategies, circuit breaker, memoization with TTL/LRU support, and `MemoizeResult` for caching only successful `Result` values. |
| **DarkPeak.Functional.Http** | [![NuGet](https://img.shields.io/nuget/v/DarkPeak.Functional.Http.svg)](https://www.nuget.org/packages/DarkPeak.Functional.Http/) | Wraps `HttpClient` operations in `Result<T, Error>` for type-safe, exception-free HTTP communication. Supports JSON, string, stream, and byte array responses with per-request header customization. |
| **DarkPeak.Functional.AspNet** | [![NuGet](https://img.shields.io/nuget/v/DarkPeak.Functional.AspNet.svg)](https://www.nuget.org/packages/DarkPeak.Functional.AspNet/) | ASP.NET integration that converts `Result<T, Error>` to `IResult` and `ProblemDetails` for idiomatic minimal API error handling. |

All types support `Map`, `Bind`, `Match`, LINQ query syntax, and async variants.

## Quick Start

```csharp
using DarkPeak.Functional;

var result = Option.Some("42")
    .Bind(s => Option.TryParse<int>(s))
    .Map(x => x * 2)
    .Match(
        some: x => $"Result: {x}",
        none: () => "No value");
// "Result: 84"
```

## Building

```bash
dotnet build
dotnet test
```

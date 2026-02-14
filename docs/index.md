# DarkPeak.Functional

A functional programming library for .NET providing monadic types and railway-oriented programming patterns.

## Features

- **[Option&lt;T&gt;](articles/option.md)** — Eliminates null reference exceptions with explicit presence/absence
- **[Result&lt;T, TError&gt;](articles/result.md)** — Railway-oriented error handling without exceptions
- **[Either&lt;TLeft, TRight&gt;](articles/either.md)** — Symmetric dual-value type for branching logic
- **[Validation&lt;T, TError&gt;](articles/validation.md)** — Accumulates multiple errors instead of short-circuiting
- **[Retry](articles/retry.md)** — Configurable retry policies with backoff strategies
- **[Memoize](articles/memoize.md)** — Function caching with TTL, LRU eviction, and distributed cache support

## Quick Start

```csharp
using DarkPeak.Functional;

// Option — no more nulls
Option<string> name = Option.Some("Alice");
var greeting = name.Map(n => $"Hello, {n}!");

// Result — railway-oriented programming
Result<int, ValidationError> parsed = Result.Success<int, ValidationError>(42);
var doubled = parsed.Map(x => x * 2);

// Fluent chaining
var result = await Option.Some("42")
    .ToResult(new ValidationError { Message = "Missing" })
    .Map(int.Parse)
    .Map(x => x * 2)
    .Match(
        success: x => $"Result: {x}",
        failure: e => $"Error: {e.Message}");
```

## Installation

```
dotnet add package DarkPeak.Functional
```

## Learn More

- [Getting Started](articles/getting-started.md) — Installation, concepts, and first steps
- [API Reference](api/index.md) — Full API documentation generated from source

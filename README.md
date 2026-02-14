# DarkPeak.Functional

A functional programming library for .NET providing monadic types and railway-oriented programming patterns.

## Features

- **Option\<T\>** — Type-safe alternative to null
- **Result\<T, TError\>** — Railway-oriented error handling
- **Either\<TLeft, TRight\>** — Symmetric dual-value type
- **Validation\<T, TError\>** — Error accumulation
- **Retry** — Configurable retry policies with backoff strategies
- **Memoize** — Function caching with TTL, LRU, and distributed cache support

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

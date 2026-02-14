# Getting Started

DarkPeak.Functional is a functional programming library for .NET that provides monadic types and railway-oriented programming patterns.

## Installation

```bash
dotnet add package DarkPeak.Functional
```

## Core Concepts

### Monadic Types

The library provides four core monadic types, each serving a different purpose:

| Type | Purpose | Error Handling |
|------|---------|----------------|
| `Option<T>` | Represents a value that may or may not exist | No error info — just presence/absence |
| `Result<T, TError>` | Represents success or failure | Single error, short-circuits on first failure |
| `Either<TLeft, TRight>` | Represents one of two possible values | No bias — both sides are equally valid |
| `Validation<T, TError>` | Represents valid or invalid with errors | Accumulates all errors |

### Railway-Oriented Programming

Each type supports a common set of operations that let you chain computations without checking for errors at every step:

- **Map** — Transform the value inside the container
- **Bind** — Chain operations that themselves return a container
- **Match** — Extract the final result by handling all cases

```csharp
using DarkPeak.Functional;

// Map transforms the inner value
var doubled = Option.Some(21).Map(x => x * 2); // Some(42)

// Bind chains operations that return Options
var result = Option.Some("42")
    .Bind(s => Option.TryParse<int>(s))
    .Map(x => x * 2); // Some(84)

// Match extracts the value by handling all cases
var message = result.Match(
    some: x => $"Got {x}",
    none: () => "Nothing"); // "Got 84"
```

### LINQ Support

All monadic types support LINQ query syntax:

```csharp
var result =
    from x in Option.Some(10)
    from y in Option.Some(20)
    where x + y > 25
    select x + y; // Some(30)
```

### Async Support

Every operation has an async variant suffixed with `Async`:

```csharp
var result = await Option.Some("https://api.example.com")
    .BindAsync(url => FetchDataAsync(url))
    .MapAsync(data => ParseAsync(data));
```

### Task Extensions

Fluent async chaining is available via task extensions — no intermediate `await` needed:

```csharp
using DarkPeak.Functional.Extensions;

var result = await FetchUserAsync(userId)     // Task<Result<User, Error>>
    .Map(user => user.Email)                   // Task<Result<string, Error>>
    .Bind(email => ValidateEmailAsync(email))  // Task<Result<Email, Error>>
    .Match(
        success: email => $"Valid: {email}",
        failure: err => $"Error: {err.Message}");
```

## Namespace Layout

```
DarkPeak.Functional              — Core types (Option, Result, Either, Validation, Error)
DarkPeak.Functional.Extensions   — Extension methods (type conversions, task extensions, etc.)
```

## Next Steps

- [Option](option.md) — Eliminating null reference exceptions
- [Result](result.md) — Railway-oriented error handling
- [Either](either.md) — Symmetric dual-value type
- [Validation](validation.md) — Error accumulation
- [Retry](retry.md) — Configurable retry policies
- [Memoize](memoize.md) — Function caching

# Copilot Instructions for DarkPeak.Functional

## Build and Test Commands

```bash
# Build
dotnet build

# Run all tests
dotnet test

# Run a single test by name
dotnet test --filter "FullyQualifiedName~OptionShould.Map_transforms_some_value"

# Run tests in a specific class
dotnet test --filter "FullyQualifiedName~OptionShould"
```

## Architecture Overview

This is a .NET 10 functional programming library providing monadic types for railway-oriented programming. The library targets nullable-enabled C# with strict null reference handling.

### Core Types

- **`Option<T>`** - Represents a value that may or may not exist (alternative to null). Implementations: `Some<T>`, `None<T>`
- **`Result<T, TError>`** - Represents an operation that can succeed with a value or fail with a typed error. Implementations: `Success<T, TError>`, `Failure<T, TError>`
- **`Either<TLeft, TRight>`** - Represents a value that can be one of two types (both valid states, not success/failure). Implementations: `Left<TLeft, TRight>`, `Right<TLeft, TRight>`
- **`Validation<T, TError>`** - Accumulates multiple errors instead of short-circuiting. Implementations: `Valid<T, TError>`, `Invalid<T, TError>`
- **`Error`** - Abstract base record for typed errors with HTTP-mapped subtypes (ValidationError, NotFoundError, UnauthorizedError, etc.)
- **`RetryPolicy`** - Configurable retry with backoff strategies (None, Constant, Linear, Exponential). Entry point: `Retry.WithMaxAttempts()`
- **`Memoize`** - Function caching with TTL, LRU eviction, and pluggable distributed cache via `ICacheProvider<TKey, TValue>`

### Extensions Namespace (`DarkPeak.Functional.Extensions`)

- **`TypeConversionExtensions`** - Cross-type conversions (Option↔Result↔Either)
- **`TaskOptionExtensions`** / **`TaskResultExtensions`** - Fluent async chaining on `Task<Option<T>>` and `Task<Result<T, TError>>`
- **`EitherExtensions`** - GetLeftOrDefault, GetRightOrDefault, Flatten, Merge, Partition
- **`ValidationExtensions`** - Apply (error accumulation), Combine, Sequence, ToResult/ToValidation interop
- **`OptionExtensions`** / **`ResultExtensions`** - Collection operations (Choose, Partition, etc.)

### Type Hierarchy Pattern

Each monadic type follows this structure:
1. Abstract base record defining the interface (e.g., `Option<T>`)
2. Concrete sealed records for each case (e.g., `Some<T>`, `None<T>`)
3. Static factory class for convenient construction (e.g., `Option.Some()`, `Option.None<T>()`)
4. Extension methods in the `Extensions` namespace for additional operations

## Key Conventions

### Exhaustive Pattern Matching
Use `Match()` to handle all cases explicitly rather than checking `IsSome`/`IsSuccess`/`IsLeft` and casting:
```csharp
var result = option.Match(
    some: value => $"Got {value}",
    none: () => "Nothing"
);
```

### LINQ Support
All monadic types support LINQ query syntax via `Select`, `SelectMany`, and `Where`:
```csharp
var result = from x in option
             from y in Option.Some(x * 2)
             where y > 5
             select y;
```

### Error Type Constraint
`Result<T, TError>` requires `TError : Error`. Create domain-specific errors by inheriting from `Error` or using provided types like `ValidationError`, `NotFoundError`.

### Async Variants
All operations have async counterparts suffixed with `Async` (e.g., `Map` → `MapAsync`, `Bind` → `BindAsync`).

### Testing Framework
Tests use TUnit (`[Test]` attribute) with async assertions (`await Assert.That(...)`).

### Commit Messages
This project uses [Conventional Commits](https://www.conventionalcommits.org/) (e.g., `feat:`, `fix:`, `docs:`, `test:`, `chore:`). Versions are derived automatically by GitVersion.

## CI/CD

- **GitVersion** (Mainline mode) derives SemVer from conventional commits
- **GitHub Actions** builds, tests, packs, and publishes on push to `main` (pre-release) and tag push (stable release)
- **Dependabot** monitors GitHub Actions and NuGet dependencies weekly
- **NuGet** package published to nuget.org (requires `NUGET_API_KEY` secret)

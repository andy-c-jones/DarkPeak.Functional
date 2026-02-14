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
- **`Unit`** - The functional programming equivalent of void; a type with only one value
- **`Error`** - Abstract base record for typed errors with HTTP-mapped subtypes (ValidationError, NotFoundError, UnauthorizedError, etc.)

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
This project uses [Conventional Commits](https://www.conventionalcommits.org/) (e.g., `feat:`, `fix:`, `docs:`, `test:`, `chore:`).

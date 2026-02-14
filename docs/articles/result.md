# Result&lt;T, TError&gt;

`Result<T, TError>` represents the outcome of an operation that can either succeed with a value of type `T` or fail with an error of type `TError`. The error type must inherit from the `Error` base record.

## Creating Results

```csharp
// Explicit construction
var success = Result.Success<int, ValidationError>(42);
var failure = Result.Failure<int, ValidationError>(
    new ValidationError { Message = "Invalid input" });

// Implicit conversion
Result<int, ValidationError> result = 42;  // Success
Result<int, ValidationError> result = new ValidationError { Message = "Oops" }; // Failure
```

## Error Types

The library provides a hierarchy of error types:

```csharp
public abstract record Error
{
    public required string Message { get; init; }
    public string? Code { get; init; }
    public Dictionary<string, object>? Metadata { get; init; }
}

// Built-in subtypes
public sealed record ValidationError : Error { ... }
public sealed record NotFoundError : Error { ... }
public sealed record UnauthorizedError : Error { ... }
public sealed record ForbiddenError : Error { ... }
public sealed record ConflictError : Error { ... }
public sealed record ExternalServiceError : Error { ... }
public sealed record BadRequestError : Error { ... }
public sealed record InternalError : Error { ... }
```

## Railway-Oriented Programming

### Map

Transform the success value. Failures pass through unchanged:

```csharp
var doubled = Result.Success<int, ValidationError>(21)
    .Map(x => x * 2); // Success(42)

var failed = Result.Failure<int, ValidationError>(error)
    .Map(x => x * 2); // Failure(error) — mapper never called
```

### MapError

Transform the error value. Successes pass through unchanged:

```csharp
var mapped = result.MapError(err =>
    new InternalError { Message = $"Wrapped: {err.Message}" });
```

### Bind

Chain operations that return Results. Short-circuits on first failure:

```csharp
Result<User, ValidationError> ValidateUser(UserDto dto) => /* ... */;
Result<UserId, ValidationError> SaveUser(User user) => /* ... */;

var result = ValidateUser(dto)
    .Bind(user => SaveUser(user)); // Success(userId) or first Failure
```

### Match

Exhaustively handle both cases:

```csharp
var message = result.Match(
    success: value => $"Created user {value}",
    failure: error => $"Failed: {error.Message}");
```

## Extracting Values

```csharp
// With a default
var value = result.GetValueOrDefault(0);
var value = result.GetValueOrDefault(() => ComputeDefault());

// Escape hatch — throws if failure
var value = result.GetValueOrThrow();

// Alternative result on failure
var value = result.OrElse(fallbackResult);
var value = result.OrElse(() => ComputeFallback());
```

## Side Effects

```csharp
result
    .Tap(value => logger.LogInformation("Success: {Value}", value))
    .TapError(error => logger.LogError("Failed: {Error}", error.Message));
```

## LINQ Support

```csharp
var result =
    from user in ValidateUser(dto)
    from saved in SaveUser(user)
    select saved;
```

## Async Operations

```csharp
var result = await ValidateUserAsync(dto)
    .BindAsync(user => SaveUserAsync(user))
    .MapAsync(id => EnrichAsync(id))
    .Match(
        success: x => $"Done: {x}",
        failure: e => $"Error: {e.Message}");
```

## Task Extensions

Chain operations on `Task<Result<T, TError>>` without intermediate awaits:

```csharp
using DarkPeak.Functional.Extensions;

var result = await FetchUserAsync(id)      // Task<Result<User, Error>>
    .Map(user => user.Email)                // Task<Result<string, Error>>
    .Bind(email => ValidateAsync(email))    // Task<Result<Email, Error>>
    .Tap(email => Log(email))               // side effect
    .Match(
        success: e => $"Valid: {e}",
        failure: e => $"Error: {e.Message}");
```

# Orchestration: Validation → Result Pipelines

Real-world applications rarely use a single monadic type in isolation. A common pattern is to **validate** input (accumulating all errors), then switch to a **Result** pipeline for sequential operations like saving, sending emails, or calling external services.

## The Pattern

```
Input ──► Validation (accumulate errors) ──► Result (sequential operations) ──► Output
```

1. **Validate** with `Validation<T, TError>` — collect every problem up front
2. **Convert** with `.ToResult()` — switch to fail-fast mode
3. **Chain** with `Bind`, `Map`, `Tap`, `TapError` — process the validated data

## Example: User Registration

### Domain Types

```csharp
public record CreateUserRequest(string Name, string Email, int Age);

public record User(string Name, string Email, int Age, Guid Id);
```

### Validators

Each validator returns a `Validation` — either valid with the cleaned value, or invalid with an error:

```csharp
using DarkPeak.Functional;

static Validation<string, ValidationError> ValidateName(string name) =>
    string.IsNullOrWhiteSpace(name)
        ? Validation.Invalid<string, ValidationError>(
            new ValidationError { Message = "Name is required" })
        : Validation.Valid<string, ValidationError>(name.Trim());

static Validation<string, ValidationError> ValidateEmail(string email) =>
    email?.Contains('@') is true
        ? Validation.Valid<string, ValidationError>(email.Trim().ToLower())
        : Validation.Invalid<string, ValidationError>(
            new ValidationError { Message = "Valid email is required" });

static Validation<int, ValidationError> ValidateAge(int age) =>
    age is >= 18 and <= 120
        ? Validation.Valid<int, ValidationError>(age)
        : Validation.Invalid<int, ValidationError>(
            new ValidationError { Message = "Age must be between 18 and 120" });
```

### The Orchestrator

```csharp
using DarkPeak.Functional.Extensions;

Result<User, ValidationError> RegisterUser(CreateUserRequest request)
{
    // Step 1: Validate — all errors are accumulated
    var validated = ValidateName(request.Name)
        .ZipWith(
            ValidateEmail(request.Email),
            ValidateAge(request.Age),
            (name, email, age) => new User(name, email, age, Guid.NewGuid()));

    // Step 2: Convert to Result — switch to fail-fast railway
    return validated
        .ToResult()
        .Tap(user => Console.WriteLine($"Validated user: {user.Name}"))
        .Bind(user => SaveUser(user))
        .Tap(user => Console.WriteLine($"Saved user: {user.Id}"))
        .TapError(err => Console.WriteLine($"Failed: {err.Message}"));
}
```

### Supporting Operations

```csharp
static Result<User, ValidationError> SaveUser(User user)
{
    // Simulate a save — in reality this would call a repository
    return Result.Success<User, ValidationError>(user);
}
```

### Using It

```csharp
// Valid input — all three validators pass
var result = RegisterUser(new CreateUserRequest("Alice", "alice@example.com", 30));
// Success(User { Name = "Alice", Email = "alice@example.com", Age = 30, Id = ... })

// Invalid input — all errors are reported at once
var result = RegisterUser(new CreateUserRequest("", "bad", 200));
// Failure(ValidationError { Message = "Name is required" })
// (Name is required, Valid email is required, and Age must be 18-120 were all caught by Validation,
//  but ToResult() surfaces the first error for the Result pipeline)
```

## Async Variant

The same pattern works with async operations using the task extensions:

```csharp
using DarkPeak.Functional.Extensions;

async Task<Result<User, ValidationError>> RegisterUserAsync(CreateUserRequest request)
{
    var validated = ValidateName(request.Name)
        .ZipWith(
            ValidateEmail(request.Email),
            ValidateAge(request.Age),
            (name, email, age) => new User(name, email, age, Guid.NewGuid()));

    return await Task.FromResult(validated.ToResult())
        .Bind(user => SaveUserAsync(user))
        .Map(user => user with { Name = user.Name.ToUpper() })
        .Tap(user => logger.LogInformation("Registered {Id}", user.Id))
        .TapError(err => logger.LogError("Registration failed: {Msg}", err.Message));
}
```

## When to Stay in Validation

If you need **all** errors at the end (e.g. to return a 400 response with every field error), stay in `Validation` and use `Match` directly instead of converting to `Result`:

```csharp
var validated = ValidateName(request.Name)
    .ZipWith(
        ValidateEmail(request.Email),
        ValidateAge(request.Age),
        (name, email, age) => new User(name, email, age, Guid.NewGuid()));

validated.Match(
    valid: user => Results.Ok(user),
    invalid: errors => Results.BadRequest(errors.Select(e => e.Message)));
```

See [Minimal Web API Example](example-web-api.md) for a complete example of this approach.

## Concurrency

When you have **independent** operations that can run at the same time, prefer the concurrent combinators over sequential chaining:

| Need | Use | Runs |
|---|---|---|
| Combine 2-8 independent `Task<Result>` | `Join` | Concurrent (`Task.WhenAll`) |
| Combine 2-8 independent `Task<Option>` | `Join` | Concurrent (`Task.WhenAll`) |
| Combine 2-8 independent `Task<Validation>` | `Join` / `ZipWithAsync` | Concurrent (`Task.WhenAll`) |
| Await a collection of `Task<Result>` | `SequenceParallel` / `TraverseParallel` | Concurrent (`Task.WhenAll`) |
| Await a collection of `Task<Option>` | `SequenceParallel` / `TraverseParallel` | Concurrent (`Task.WhenAll`) |
| Await a collection of `Task<Validation>` | `SequenceParallel` / `TraverseParallel` | Concurrent (`Task.WhenAll`) |

### Example: Concurrent Validation

```csharp
// Validators that call external services — run concurrently
Task<Validation<string, Error>> ValidateUsernameAsync(string name) => /* check DB */;
Task<Validation<string, Error>> ValidateEmailAsync(string email) => /* check mailbox */;
Task<Validation<int, Error>> ValidateAgeAsync(int age) => /* check against policy service */;

var validated = await ValidateUsernameAsync(request.Name)
    .Join(
        ValidateEmailAsync(request.Email),
        ValidateAgeAsync(request.Age));
// All three HTTP calls run concurrently.
// If any fail, errors are accumulated (Validation semantics).
```

### Sequential vs Concurrent

Use `Bind` chains and `Pipeline` when each step **depends on** the previous step's output. Use `Join` and `*Parallel` when the operations are **independent** — this can dramatically reduce wall-clock time.

```
Sequential (Bind/Pipeline):  A ──► B ──► C     total = tA + tB + tC
Concurrent (Join):           A ──┐
                             B ──┤ join    total = max(tA, tB, tC)
                             C ──┘
```

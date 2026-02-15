# Validation&lt;T, TError&gt;

`Validation<T, TError>` is like `Result`, but instead of short-circuiting on the first error, it **accumulates all errors**. This makes it ideal for form validation, input checking, and any scenario where you want to report every problem at once.

## Creating Validations

```csharp
var valid   = Validation.Valid<string, ValidationError>("Alice");
var invalid = Validation.Invalid<string, ValidationError>(
    new ValidationError { Message = "Name is required" });

// Multiple errors
var invalid = Validation.Invalid<string, ValidationError>(new[]
{
    new ValidationError { Message = "Too short" },
    new ValidationError { Message = "Contains invalid characters" }
});

// Implicit conversion from value
Validation<int, ValidationError> v = 42; // Valid(42)
```

## Validation vs Result

| | `Result<T, TError>` | `Validation<T, TError>` |
|---|---|---|
| Error count | Single error | Multiple errors |
| `Bind` behaviour | Short-circuits on first error | Short-circuits on first error |
| `Apply` behaviour | N/A | **Accumulates errors** |
| Best for | Sequential operations | Parallel validation rules |

## Accumulating Errors with Apply

`Apply` is the key differentiator. It runs all validations and collects every error:

```csharp
using DarkPeak.Functional.Extensions;

Validation<string, ValidationError> ValidateName(string name) =>
    string.IsNullOrWhiteSpace(name)
        ? Validation.Invalid<string, ValidationError>(
            new ValidationError { Message = "Name is required" })
        : Validation.Valid<string, ValidationError>(name);

Validation<int, ValidationError> ValidateAge(int age) =>
    age is < 0 or > 150
        ? Validation.Invalid<int, ValidationError>(
            new ValidationError { Message = "Age must be 0-150" })
        : Validation.Valid<int, ValidationError>(age);

// Combine with Apply — all errors are accumulated
var result = Validation.Valid<Func<string, int, User>, ValidationError>(
        (name, age) => new User(name, age))
    .Apply(ValidateName(""))
    .Apply(ValidateAge(-1));
// Invalid([{ Message = "Name is required" }, { Message = "Age must be 0-150" }])
```

## ZipWith

Combine multiple validations with a projection function:

```csharp
var result = ValidateName(dto.Name)
    .ZipWith(
        ValidateAge(dto.Age),
        (name, age) => new User(name, age));

// Three-way combine
var result = ValidateName(dto.Name)
    .ZipWith(
        ValidateAge(dto.Age),
        ValidateEmail(dto.Email),
        (name, age, email) => new User(name, age, email));
```

## Sequence

Convert a collection of validations into a single validation of a collection:

```csharp
var validations = new[]
{
    ValidateName("Alice"),
    ValidateName(""),
    ValidateName("Charlie")
};

var result = validations.Sequence();
// Invalid — collects errors from the second item
```

## Interop with Result

```csharp
using DarkPeak.Functional.Extensions;

// Validation → Result (takes first error)
Result<T, TError> result = validation.ToResult();

// Result → Validation
Validation<T, TError> validation = result.ToValidation();
```

## Standard Operations

Validation supports the same operations as the other monadic types:

```csharp
// Map
var upper = validation.Map(name => name.ToUpper());

// Bind (short-circuits, does NOT accumulate — use Apply for that)
var result = validation.Bind(name => ValidateLength(name));

// Match
var message = validation.Match(
    valid:   value  => $"OK: {value}",
    invalid: errors => string.Join(", ", errors.Select(e => e.Message)));

// Side effects
validation
    .Tap(value => Log($"Valid: {value}"))
    .TapInvalid(errors => Log($"Errors: {errors.Count}"));

// Extract value
var value = validation.GetValueOrDefault("fallback");
var value = validation.GetValueOrThrow();
```

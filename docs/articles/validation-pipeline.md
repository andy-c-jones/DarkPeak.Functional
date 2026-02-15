# Validation Pipeline

The `ValidationPipeline` builder constructs reusable multi-step workflows that **accumulate all errors** rather than short-circuiting on the first failure. It is the validation-aware counterpart to [Pipeline](pipeline.md).

## Pipeline vs ValidationPipeline

| | `Pipeline.Create` | `ValidationPipeline.Create` |
|---|---|---|
| Underlying type | `Result<T, TError>` | `Validation<T, TError>` |
| Error behaviour | Short-circuits on first error | **Accumulates all errors** |
| Step input | Output of previous step (sequential) | Original input (fan-out) |
| Step method | `Then` / `ThenAsync` | `Validate` / `ValidateAsync` |
| Best for | Sequential transformation chains | Independent validation rules |

## Basic Usage

Each `Validate` step receives the **original input** and returns a `Validation<T, TError>`. When you call `Build`, you provide a combiner function that merges the valid results:

```csharp
using DarkPeak.Functional;

Validation<string, ValidationError> ValidateName(string name) =>
    string.IsNullOrWhiteSpace(name)
        ? Validation.Invalid<string, ValidationError>(
            new ValidationError { Message = "Name is required" })
        : Validation.Valid<string, ValidationError>(name.Trim());

Validation<int, ValidationError> ValidateAge(int age) =>
    age is < 0 or > 150
        ? Validation.Invalid<int, ValidationError>(
            new ValidationError { Message = "Age must be 0-150" })
        : Validation.Valid<int, ValidationError>(age);

record UserForm(string Name, int Age);
record User(string Name, int Age);

var validate = ValidationPipeline.Create<UserForm, ValidationError>()
    .Validate(form => ValidateName(form.Name))
    .Validate(form => ValidateAge(form.Age))
    .Build((name, age) => new User(name, age));

validate(new UserForm("Alice", 30));  // Valid(User("Alice", 30))
validate(new UserForm("", -1));       // Invalid(["Name is required", "Age must be 0-150"])
```

Both errors are reported in the second call — nothing is short-circuited.

## Plain Mapping Steps

If a step cannot fail (e.g. preprocessing or extracting a field), pass a plain function. It is auto-wrapped in `Valid`:

```csharp
var validate = ValidationPipeline.Create<UserForm, ValidationError>()
    .Validate(form => ValidateName(form.Name))
    .Validate(form => form.Age)   // plain mapping — always Valid
    .Build((name, age) => new User(name, age));
```

This avoids boilerplate for non-failing steps while keeping the pipeline fully typed.

## Single-Step Pipelines

With a single step, `Build()` takes no combiner — the step's output type is the pipeline's output type:

```csharp
var validateName = ValidationPipeline.Create<string, ValidationError>()
    .Validate(name => ValidateName(name))
    .Build();

validateName("Alice"); // Valid("Alice")
validateName("");      // Invalid(["Name is required"])
```

## Async Steps

Call `ValidateAsync` to add a step that returns `Task<Validation<T, TError>>`. Once any async step is added, the pipeline transitions to async mode permanently and returns `Func<TInput, Task<Validation<TResult, TError>>>`:

```csharp
var validate = ValidationPipeline.Create<UserForm, ValidationError>()
    .Validate(form => ValidateName(form.Name))
    .ValidateAsync(form => CheckEmailUniqueAsync(form.Email))
    .Build((name, isUnique) => new User(name));

var result = await validate(new UserForm("Alice", "alice@test.com"));
```

### Concurrency

All async steps run **concurrently** via `Task.WhenAll`. If you have three async validation steps, they execute in parallel — not sequentially:

```csharp
var validate = ValidationPipeline.Create<RegistrationForm, ValidationError>()
    .ValidateAsync(form => CheckUsernameAsync(form.Username))   // concurrent
    .ValidateAsync(form => CheckEmailAsync(form.Email))         // concurrent
    .ValidateAsync(form => VerifyCaptchaAsync(form.CaptchaToken)) // concurrent
    .Build((username, email, _) => new Account(username, email));

// All three checks run at the same time
var result = await validate(form);
```

### Mixed Sync and Async

Sync and async steps can be freely mixed. Sync steps added after the first async step are wrapped in `Task.FromResult` internally:

```csharp
var validate = ValidationPipeline.Create<UserForm, ValidationError>()
    .Validate(form => ValidateName(form.Name))                  // sync
    .ValidateAsync(form => CheckEmailUniqueAsync(form.Email))   // async — transitions to async mode
    .Validate(form => ValidateAge(form.Age))                    // sync (wrapped internally)
    .Build((name, email, age) => new User(name, email, age));

var result = await validate(form);
```

## Reusability

The function returned by `Build` is a plain delegate — call it any number of times with different inputs:

```csharp
var validate = ValidationPipeline.Create<UserForm, ValidationError>()
    .Validate(form => ValidateName(form.Name))
    .Validate(form => ValidateAge(form.Age))
    .Build((name, age) => new User(name, age));

var result1 = validate(new UserForm("Alice", 30));
var result2 = validate(new UserForm("Bob", 25));
var result3 = validate(new UserForm("", -1));
```

## Arities

`ValidationPipeline` supports up to **8 steps**, matching the arity of `ZipWith`:

```csharp
var validate = ValidationPipeline.Create<Form, ValidationError>()
    .Validate(f => ValidateField1(f))
    .Validate(f => ValidateField2(f))
    .Validate(f => ValidateField3(f))
    .Validate(f => ValidateField4(f))
    .Validate(f => ValidateField5(f))
    .Validate(f => ValidateField6(f))
    .Validate(f => ValidateField7(f))
    .Validate(f => ValidateField8(f))
    .Build((f1, f2, f3, f4, f5, f6, f7, f8) => new Model(f1, f2, f3, f4, f5, f6, f7, f8));
```

## Composing Nested Pipelines

Each pipeline's `Build` returns a `Func<TInput, Validation<TResult, TError>>` — exactly the signature that `.Validate(...)` accepts. This means you can compose pipelines hierarchically, validating deeply nested objects without hitting the 8-step cap:

```csharp
record Address(string Street, string City, string PostCode);
record ContactInfo(string Email, string Phone, Address Address);
record RegistrationForm(string Name, int Age, ContactInfo Contact);

// Address pipeline (3 steps → 1 Validation<Address>)
var validateAddress = ValidationPipeline.Create<Address, ValidationError>()
    .Validate(a => ValidateStreet(a.Street))
    .Validate(a => ValidateCity(a.City))
    .Validate(a => ValidatePostCode(a.PostCode))
    .Build((street, city, postCode) => new Address(street, city, postCode));

// ContactInfo pipeline (uses validateAddress as a single step)
var validateContact = ValidationPipeline.Create<ContactInfo, ValidationError>()
    .Validate(c => ValidateEmail(c.Email))
    .Validate(c => ValidatePhone(c.Phone))
    .Validate(c => validateAddress(c.Address))
    .Build((email, phone, address) => new ContactInfo(email, phone, address));

// Top-level pipeline (uses validateContact as a single step)
var validate = ValidationPipeline.Create<RegistrationForm, ValidationError>()
    .Validate(f => ValidateName(f.Name))
    .Validate(f => ValidateAge(f.Age))
    .Validate(f => validateContact(f.Contact))
    .Build((name, age, contact) => new Registration(name, age, contact));

var result = validate(new RegistrationForm("", -1,
    new ContactInfo("bad", "", new Address("", "", "!!!"))));
// Invalid — collects errors from Name, Age, Email, Phone, Street, City, and PostCode
```

Errors accumulate across all levels. The top-level call surfaces every failure from every nested pipeline in a single `Invalid` result, regardless of depth.

## When to Use What

| Approach | Best for |
|---|---|
| `Validation.ZipWith` | Combining a few ad-hoc validations inline |
| `ValidationPipeline.Create` | Reusable, multi-step validation workflows |
| `Pipeline.Create` | Sequential transformations that short-circuit on first error |

> **Tip:** `ValidationPipeline` is a builder for fan-out validation — every step sees the same input. If you need sequential transformations where each step depends on the previous step's output, use [`Pipeline.Create`](pipeline.md) instead.

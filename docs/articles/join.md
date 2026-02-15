# Join

`Join` combines independent computations into tuples. It supports arities 2 through 8 for sync, and 2 through 8 for async.

## Semantics

| Type | Semantics |
|---|---|
| `Result` | Fail-fast — returns the first failure |
| `Option` | Fail-fast — returns None if any is None |
| `Validation` | Error-accumulating — collects all errors |

## Result

```csharp
using DarkPeak.Functional;
using DarkPeak.Functional.Extensions;

var name = Result.Success<string, Error>("Alice");
var age = Result.Success<int, Error>(30);

var joined = name.Join(age);
// Success(("Alice", 30))

// 3-arity
var email = Result.Success<string, Error>("alice@example.com");
var joined = name.Join(age, email);
// Success(("Alice", 30, "alice@example.com"))
```

Higher arities (up to 8) work the same way:

```csharp
var joined = v1.Join(v2, v3, v4, v5, v6, v7, v8);
// Success((v1, v2, v3, v4, v5, v6, v7, v8))
```

If any Result is a Failure, the first failure is returned (fail-fast):

```csharp
var name = Result.Failure<string, Error>(new ValidationError { Message = "Name required" });
var age = Result.Success<int, Error>(30);

var joined = name.Join(age);
// Failure("Name required")
```

## Option

```csharp
var name = Option.Some("Alice");
var age = Option.Some(30);

var joined = name.Join(age);
// Some(("Alice", 30))

// 3-arity
var email = Option.Some("alice@example.com");
var joined = name.Join(age, email);
// Some(("Alice", 30, "alice@example.com"))
```

If any Option is None, the result is None:

```csharp
var name = Option.None<string>();
var age = Option.Some(30);

var joined = name.Join(age);
// None
```

## Validation

Validation uses error-accumulating semantics — all errors from all inputs are collected:

```csharp
var name = Validation.Invalid<string, Error>(
    new ValidationError { Message = "Name required" });
var age = Validation.Invalid<int, Error>(
    new ValidationError { Message = "Age required" });

var joined = name.Join(age);
// Invalid([{ Message = "Name required" }, { Message = "Age required" }])

// 3-arity
var email = Validation.Invalid<string, Error>(
    new ValidationError { Message = "Email required" });
var joined = name.Join(age, email);
// Invalid — 3 errors accumulated
```

## Async Join

For `Task<Result<T, TError>>`, `Task<Option<T>>`, and `Task<Validation<T, TError>>`, async `Join` runs all tasks concurrently using `Task.WhenAll`. Arities 2 through 8 are supported:

```csharp
using DarkPeak.Functional.Extensions;

// Result — concurrent, fail-fast
var result = await FetchUserAsync(userId)
    .Join(FetchOrdersAsync(userId));
// Success((user, orders)) — both tasks run concurrently

// Option — concurrent, fail-fast
var result = await FindConfigAsync("key1")
    .Join(FindConfigAsync("key2"));
// Some((value1, value2)) or None

// Validation — concurrent, error-accumulating
var result = await ValidateNameAsync(name)
    .Join(ValidateAgeAsync(age), ValidateEmailAsync(email));
// Valid((name, age, email)) or Invalid with all accumulated errors
```

Higher-arity async join works the same way:

```csharp
var result = await task1.Join(task2, task3, task4, task5);
// All five tasks run concurrently via Task.WhenAll
```

## Join vs ZipWith

`Join` produces a tuple. `ZipWith` (on Validation) lets you provide a projection function:

```csharp
// Join — tuple output
var tuple = name.Join(age);
// Valid(("Alice", 30))

// ZipWith — projected output
var user = name.ZipWith(age, (n, a) => new User(n, a));
// Valid(User("Alice", 30))
```

Both accumulate errors the same way. Use `Join` when you want the raw tuple, `ZipWith` when you want to transform the result immediately.

For async Validation, use `ZipWithAsync` to combine async validations with a projection:

```csharp
var user = await ValidateNameAsync(name)
    .ZipWithAsync(
        ValidateAgeAsync(age),
        ValidateEmailAsync(email),
        (n, a, e) => new User(n, a, e));
// All tasks run concurrently, errors accumulated
```

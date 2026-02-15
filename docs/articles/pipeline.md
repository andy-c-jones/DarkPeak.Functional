# Pipeline & Composition

DarkPeak.Functional provides three levels of function composition:

1. **Pipe** — pass a value into a function (`value.Pipe(f)`)
2. **Compose** — combine two functions into one (`f.Compose(g)`)
3. **Pipeline Builder** — construct reusable multi-step Result-based workflows

## Pipe

`Pipe` feeds a value into a function, enabling left-to-right reading of transformations (like F#'s `|>` operator):

```csharp
using DarkPeak.Functional;

var result = "  hello world  "
    .Pipe(s => s.Trim())
    .Pipe(s => s.ToUpper())
    .Pipe(s => s.Split(' '))
    .Pipe(words => words.Length);
// 2
```

### Async Pipe

```csharp
var result = await userId
    .PipeAsync(id => FetchUserAsync(id));
```

## Compose

`Compose` merges two functions into a single function — the output of the first feeds into the input of the second:

```csharp
using DarkPeak.Functional;

Func<string, string> trim = s => s.Trim();
Func<string, string> upper = s => s.ToUpper();

var trimAndUpper = trim.Compose(upper);
trimAndUpper("  hello  "); // "HELLO"
```

### Async Compose

Compose a sync function with an async function:

```csharp
Func<string, string> parse = s => s.Trim();
Func<string, Task<User>> lookup = name => FindUserAsync(name);

var pipeline = parse.ComposeAsync(lookup);
var user = await pipeline("  alice  ");
```

### Result-aware Compose

Compose two Result-returning functions — short-circuits on failure:

```csharp
Func<string, Result<int, Error>> parse = s =>
    int.TryParse(s, out var n)
        ? Result.Success<int, Error>(n)
        : Result.Failure<int, Error>(new ValidationError { Message = "Not a number" });

Func<int, Result<int, Error>> validate = n =>
    n > 0
        ? Result.Success<int, Error>(n)
        : Result.Failure<int, Error>(new ValidationError { Message = "Must be positive" });

var parseAndValidate = parse.Compose(validate);
parseAndValidate("42");  // Success(42)
parseAndValidate("-1");  // Failure("Must be positive")
parseAndValidate("abc"); // Failure("Not a number")
```

### Option-aware Compose

Compose two Option-returning functions — short-circuits on None:

```csharp
Func<string, Option<int>> tryParse = s =>
    int.TryParse(s, out var n) ? Option.Some(n) : Option.None<int>();

Func<int, Option<string>> lookupName = id =>
    id == 1 ? Option.Some("Alice") : Option.None<string>();

var parseAndLookup = tryParse.Compose(lookupName);
parseAndLookup("1");   // Some("Alice")
parseAndLookup("2");   // None
parseAndLookup("abc"); // None
```

## Pipeline Builder

For complex, reusable workflows, use the fluent `Pipeline.Create<TInput, TError>()` builder. Each step is a `Then` call that takes the previous step's output and returns a `Result`:

```csharp
using DarkPeak.Functional;

var process = Pipeline.Create<string, ValidationError>()
    .Then(input => int.TryParse(input, out var n)
        ? Result.Success<int, ValidationError>(n)
        : Result.Failure<int, ValidationError>(
            new ValidationError { Message = "Not a number" }))
    .Then(n => n > 0
        ? Result.Success<int, ValidationError>(n)
        : Result.Failure<int, ValidationError>(
            new ValidationError { Message = "Must be positive" }))
    .Then(n => $"Value: {n}")  // plain mapping — auto-wrapped in Success
    .Build();

process("42");  // Success("Value: 42")
process("-1");  // Failure("Must be positive")
process("abc"); // Failure("Not a number")
```

### Mixed Sync and Async

Once you call `ThenAsync`, the pipeline transitions to async mode permanently:

```csharp
var pipeline = Pipeline.Create<int, Error>()
    .Then(id => id > 0
        ? Result.Success<int, Error>(id)
        : Result.Failure<int, Error>(
            new ValidationError { Message = "Invalid ID" }))
    .ThenAsync(id => FetchUserAsync(id))     // now async
    .Then(user => user.Email)                 // sync step still works
    .ThenAsync(email => SendWelcomeAsync(email))
    .Build();

var result = await pipeline(42);
```

The built function is reusable — call it multiple times with different inputs.

## When to Use What

| Approach | Best for |
|---|---|
| `Pipe` | Ad-hoc, inline transformations |
| `Compose` | Building small, composable function building blocks |
| `Pipeline.Create` | Complex, reusable multi-step workflows with error handling |

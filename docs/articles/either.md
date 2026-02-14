# Either&lt;TLeft, TRight&gt;

`Either<TLeft, TRight>` represents a value that is one of two possible types. Unlike `Result`, both sides are equally valid — there's no success/failure bias. Use it for branching logic where both paths represent legitimate outcomes.

## Creating Eithers

```csharp
// Explicit construction
var left  = Either.Left<string, int>("hello");
var right = Either.Right<string, int>(42);

// Implicit conversion
Either<string, int> fromLeft  = "hello";
Either<string, int> fromRight = 42;
```

## Transforming Values

### MapLeft / MapRight

Transform one side independently:

```csharp
var result = Either.Left<string, int>("hello")
    .MapLeft(s => s.Length);  // Left(5)

var result = Either.Right<string, int>(42)
    .MapRight(x => x * 2);   // Right(84)
```

### Map (both sides)

Transform both sides in one call:

```csharp
var result = either.Map(
    leftMapper:  s => s.Length,
    rightMapper: x => x * 2);
```

### Bind

Chain operations on the right side:

```csharp
var result = Either.Right<string, int>(42)
    .Bind(x => x > 0
        ? Either.Right<string, int>(x * 2)
        : Either.Left<string, int>("must be positive"));
```

## Pattern Matching

```csharp
var message = either.Match(
    left:  s => $"Text: {s}",
    right: n => $"Number: {n}");
```

## Side Effects

```csharp
either
    .IfLeft(s => Console.WriteLine($"Left: {s}"))
    .IfRight(n => Console.WriteLine($"Right: {n}"));
```

## Swap

Exchange left and right:

```csharp
Either<int, string> swapped = Either.Left<string, int>("hello").Swap();
// Right("hello")
```

## Extension Methods

```csharp
using DarkPeak.Functional.Extensions;

// Extract with defaults
var leftVal  = either.GetLeftOrDefault("fallback");
var rightVal = either.GetRightOrDefault(0);

// Merge when both sides are the same type
Either<string, string> e = Either.Left<string, string>("hello");
string merged = e.Merge(); // "hello"

// Flatten nested Eithers
Either<Either<string, int>, int> nested = /* ... */;
Either<string, int> flat = nested.Flatten();

// Partition a collection
var (lefts, rights) = items.Partition();
```

## LINQ Support

LINQ query syntax operates on the right side:

```csharp
var result =
    from x in Either.Right<string, int>(10)
    from y in Either.Right<string, int>(20)
    select x + y; // Right(30)
```

## Async Operations

```csharp
var result = await either
    .MapLeftAsync(async s => await TranslateAsync(s))
    .MapRightAsync(async n => await EnrichAsync(n))
    .MatchAsync(
        left:  async s => await FormatLeftAsync(s),
        right: async n => await FormatRightAsync(n));
```

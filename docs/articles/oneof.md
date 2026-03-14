# OneOf&lt;T1, ..., Tn&gt;

`OneOf<T1, ..., Tn>` represents a discriminated union with between 2 and 8 possible cases. Use it when a value can legitimately be one of several different types and you want that choice to be explicit in the type system.

Unlike `Result`, `OneOf` does not treat any case as success or failure by default. Unlike `Either`, it is not limited to two cases.

## Creating OneOf Values

```csharp
using DarkPeak.Functional;

// Explicit factory methods
var fromFactory = OneOf.Second<string, int>(42);

// Implicit conversion
OneOf<string, int> number = 42;
OneOf<string, int> text = "hello";

// Higher arities are supported up to 8 cases
OneOf<string, int, bool> flag = true;
var fourth = OneOf.Fourth<string, int, bool, decimal>(12.5m);
```

## Inspecting the Active Case

Each arity exposes `IsTn` and `AsTn` members for the available cases:

```csharp
OneOf<string, int, bool> value = true;

if (value.IsT3)
{
    Console.WriteLine(value.AsT3); // true
}
```

When you need to consume the value, prefer `Match` so every case is handled explicitly:

```csharp
var message = value.Match(
    t1 => $"Text: {t1}",
    t2 => $"Number: {t2}",
    t3 => $"Flag: {t3}");
```

Async pattern matching is also available:

```csharp
var result = await value.MatchAsync(
    t1 => Task.FromResult($"Text: {t1}"),
    t2 => Task.FromResult($"Number: {t2}"),
    t3 => Task.FromResult($"Flag: {t3}"));
```

## Transforming Individual Cases

Use the case-specific mapping methods to transform only the active branch:

```csharp
OneOf<string, int, bool> input = 42;

var mapped = input.MapSecond(x => x * 2);
// OneOf<string, int, bool> containing 84
```

For unions with 3 or more cases, `Reduce...` methods let you collapse one case into a smaller union:

```csharp
OneOf<string, int, bool> input = true;

OneOf<string, int> reduced = input.ReduceThird(flag =>
    flag ? "enabled" : 0);
```

`Map...` and `Reduce...` only execute the delegate for the active case. For inactive cases, the union is returned unchanged in shape and value.

## Error Behavior

`AsTn` accessors enforce case safety. Accessing the wrong case throws `InvalidOperationException` with a descriptive message (for example, `"Value is not T3."`).

`Match` and `MatchAsync` validate internal state and throw `InvalidOperationException` if an invalid index is encountered (for example, after malformed reflection-based construction in tests).

## LINQ Support

`OneOf` supports LINQ query syntax. Queries operate on the final generic argument, and the earlier cases short-circuit through the query unchanged.

```csharp
OneOf<string, int> input = 21;

var query =
    from x in input
    from y in (OneOf<string, int>)(x * 2)
    select y + 1;

var value = query.Match(
    t1 => t1,
    t2 => t2.ToString()); // "43"
```

## Interop with Either

For two-case unions, `DarkPeak.Functional.Extensions` provides conversion helpers between `Either<TLeft, TRight>` and `OneOf<TLeft, TRight>`:

```csharp
using DarkPeak.Functional.Extensions;

Either<string, int> either = 42;
OneOf<string, int> oneOf = either.ToOneOf();
Either<string, int> roundTrip = oneOf.ToEither();
```

## When to Use OneOf

Reach for `OneOf` when:

- an API can return several different successful shapes
- a workflow has multiple valid states that should stay type-safe
- `Either` is too limited because you need more than two cases

Prefer `Result<T, TError>` when you specifically want success/failure semantics, and prefer `Validation<T, TError>` when you need to accumulate multiple errors.

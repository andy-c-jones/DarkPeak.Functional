# Sequence & Traverse

`Sequence` and `Traverse` turn collections of monadic values "inside out". They work with `Option`, `Result`, and `Validation`.

## Sequence

Converts a collection of monadic values into a single monadic value containing a collection:

```
IEnumerable<Option<T>>       → Option<IEnumerable<T>>
IEnumerable<Result<T, E>>    → Result<IEnumerable<T>, E>
IEnumerable<Validation<T, E>> → Validation<IEnumerable<T>, E>
```

### Option

Returns `Some` with all values if every element is `Some`, or `None` if any element is `None`:

```csharp
using DarkPeak.Functional;
using DarkPeak.Functional.Extensions;

var allSome = new[] { Option.Some(1), Option.Some(2), Option.Some(3) }.Sequence();
// Some([1, 2, 3])

var hasNone = new[] { Option.Some(1), Option.None<int>(), Option.Some(3) }.Sequence();
// None
```

### Result

Returns `Success` with all values if all succeed, or the first `Failure` (fail-fast):

```csharp
var allOk = new[]
{
    Result.Success<int, Error>(1),
    Result.Success<int, Error>(2)
}.Sequence();
// Success([1, 2])

var hasFail = new[]
{
    Result.Success<int, Error>(1),
    Result.Failure<int, Error>(new ValidationError { Message = "bad" })
}.Sequence();
// Failure("bad")
```

### Validation

Returns `Valid` with all values if all succeed, or `Invalid` with **all accumulated errors**:

```csharp
var mixed = new[]
{
    Validation.Valid<int, Error>(1),
    Validation.Invalid<int, Error>(new ValidationError { Message = "err1" }),
    Validation.Invalid<int, Error>(new ValidationError { Message = "err2" })
}.Sequence();
// Invalid([err1, err2])
```

## Traverse

`Traverse` is `Map` + `Sequence` in one step — it applies a function to each element, then sequences the results. This is more efficient and reads more naturally than `.Select(f).Sequence()`.

### Option

```csharp
Option<int> TryParse(string s) =>
    int.TryParse(s, out var n) ? Option.Some(n) : Option.None<int>();

var result = new[] { "1", "2", "3" }.Traverse(TryParse);
// Some([1, 2, 3])

var result = new[] { "1", "abc", "3" }.Traverse(TryParse);
// None
```

### Result

```csharp
Result<User, Error> FindUser(int id) => /* ... */;

var result = new[] { 1, 2, 3 }.Traverse(FindUser);
// Success([user1, user2, user3]) or first Failure
```

### Validation

```csharp
Validation<string, ValidationError> ValidateName(string name) => /* ... */;

var result = new[] { "Alice", "", "Charlie" }.Traverse(ValidateName);
// Invalid — accumulates errors from the empty name
```

## Async Variants

Both `Sequence` and `Traverse` have sequential and parallel async variants for `Result` and `Option`.

### Sequential (one at a time)

```csharp
// SequenceAsync — await tasks one by one, fail-fast
var tasks = ids.Select(id => FetchUserAsync(id));
var result = await tasks.SequenceAsync();

// TraverseAsync — apply async function sequentially, fail-fast
var result = await ids.TraverseAsync(id => FetchUserAsync(id));
```

### Parallel (concurrent via Task.WhenAll)

```csharp
// SequenceParallel — await all tasks concurrently, then sequence
var tasks = ids.Select(id => FetchUserAsync(id));
var result = await tasks.SequenceParallel();

// TraverseParallel — apply async function concurrently, then sequence
var result = await ids.TraverseParallel(id => FetchUserAsync(id));
```

### Additional Async Collection Operations (Result only)

```csharp
// PartitionAsync / PartitionParallel — split into successes and failures
var (successes, failures) = await tasks.PartitionAsync();
var (successes, failures) = await tasks.PartitionParallel();

// ChooseAsync / ChooseParallel — keep only successes
var values = await tasks.ChooseAsync();
var values = await tasks.ChooseParallel();
```

## Summary

| Operation | Option | Result | Validation |
|---|---|---|---|
| `Sequence` | Any None → None | First failure (fail-fast) | Accumulates all errors |
| `Traverse` | Any None → None | First failure (fail-fast) | Accumulates all errors |
| `SequenceAsync` | Sequential | Sequential, fail-fast | — |
| `TraverseAsync` | Sequential | Sequential, fail-fast | — |
| `SequenceParallel` | Concurrent | Concurrent, fail-fast | — |
| `TraverseParallel` | Concurrent | Concurrent, fail-fast | — |
| `PartitionAsync` | — | Sequential | — |
| `ChooseAsync` | — | Sequential | — |
| `PartitionParallel` | — | Concurrent | — |
| `ChooseParallel` | — | Concurrent | — |

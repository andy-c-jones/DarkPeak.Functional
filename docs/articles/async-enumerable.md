# Async Enumerable Extensions

Functional extension methods for `IAsyncEnumerable<T>` and its combinations with `Option<T>`, `Result<T, TError>`, and `Validation<T, TError>`. These extensions bring the same compositional style you use with single monadic values to asynchronous streams.

## Plain Stream Operations

### Map / MapAsync

Transform each element. `Map` delegates to `Select`; `MapAsync` supports async mapping functions:

```csharp
using DarkPeak.Functional.Extensions;

var doubled = source.Map(x => x * 2);

var enriched = source.MapAsync(async x => await EnrichAsync(x));
```

### Filter / FilterAsync

Keep elements that match a predicate. `Filter` delegates to `Where`; `FilterAsync` supports async predicates:

```csharp
var positives = source.Filter(x => x > 0);

var valid = source.FilterAsync(async x => await IsValidAsync(x));
```

### Bind / BindAsync

Project each element to a sub-stream and flatten (monadic bind / flatMap). `Bind` delegates to `SelectMany`; `BindAsync` supports async projections:

```csharp
var expanded = source.Bind(x => GetChildrenAsync(x));

var resolved = source.BindAsync(async x =>
{
    var children = await FetchChildrenAsync(x);
    return children;
});
```

### Tap / TapAsync

Execute a side-effect for each element without modifying the stream. Useful for logging, metrics, or debugging:

```csharp
var logged = source
    .Tap(x => Console.WriteLine($"Processing: {x}"))
    .Map(x => Transform(x));

var audited = source.TapAsync(async x => await AuditAsync(x));
```

### Scan / ScanAsync

Apply an accumulator function and yield each intermediate result (like `Aggregate`, but streaming):

```csharp
// Running total
var runningSum = numbers.Scan(0, (acc, x) => acc + x);
// Input:  1, 2, 3, 4
// Output: 1, 3, 6, 10

var asyncScan = source.ScanAsync(
    initialState,
    async (state, item) => await ComputeNextStateAsync(state, item));
```

### Unfold / UnfoldAsync

Generate an async sequence from a seed value. The generator returns `Some((element, nextSeed))` to continue or `None` to stop:

```csharp
// Generate Fibonacci numbers up to 100
var fibs = AsyncEnumerableExtensions.Unfold(
    (0, 1),
    state =>
    {
        var (a, b) = state;
        return a <= 100
            ? Option.Some((a, (b, a + b)))
            : Option.None<(int, (int, int))>();
    });
// Output: 0, 1, 1, 2, 3, 5, 8, 13, 21, 34, 55, 89

// Async variant for paginated API calls
var pages = AsyncEnumerableExtensions.UnfoldAsync(
    firstPageUrl,
    async url =>
    {
        var page = await FetchPageAsync(url);
        return page.NextUrl is not null
            ? Option.Some((page.Items, page.NextUrl))
            : Option.None<(List<Item>, string)>();
    });
```

### Buffer

Batch elements into fixed-size chunks. The last batch may contain fewer elements:

```csharp
var batches = source.Buffer(100);

await foreach (var batch in batches)
{
    await BulkInsertAsync(batch); // batch is IReadOnlyList<T>
}
```

### ForEachAsync

Terminal operation that consumes the stream, executing an action for each element:

```csharp
await source.ForEachAsync(item => Process(item));

await source.ForEachAsync(async item => await ProcessAsync(item));
```

## Option Stream Operations

Extensions for `IAsyncEnumerable<Option<T>>` — streams where each element may or may not have a value.

### Choose

Filter out `None` values and unwrap the `Some` values:

```csharp
IAsyncEnumerable<Option<int>> optionStream = /* ... */;

IAsyncEnumerable<int> values = optionStream.Choose();
// Only yields the inner values from Some elements
```

### ChooseMap / ChooseMapAsync

Apply a function that returns an `Option` to each element, keeping only the `Some` results. More efficient than `Map` + `Choose`:

```csharp
// Sync: parse strings, keep only valid integers
var parsed = strings.ChooseMap(s =>
    int.TryParse(s, out var n) ? Option.Some(n) : Option.None<int>());

// Async
var lookedUp = ids.ChooseMapAsync(async id => await TryFindAsync(id));
```

### MapOption / MapOptionAsync

Transform the value inside each `Some`, passing `None` through unchanged:

```csharp
var doubled = optionStream.MapOption(x => x * 2);
// Some(3) → Some(6), None → None

var enriched = optionStream.MapOptionAsync(async x => await EnrichAsync(x));
```

### BindOption / BindOptionAsync

Apply a function returning an `Option` to each `Some` value, passing `None` through:

```csharp
var resolved = optionStream.BindOption(x => Lookup(x));
// Some(key) → Some(value) or None, None → None

var asyncResolved = optionStream.BindOptionAsync(
    async x => await TryLookupAsync(x));
```

### SequenceAsync (Option)

Collect all options into a single option. Returns `Some` with all values if every element is `Some`, or `None` if any element is `None` (short-circuits):

```csharp
var result = await optionStream.SequenceAsync();
// All Some → Some([v1, v2, v3])
// Any None → None
```

### Terminal Queries: FirstOrNoneAsync, SingleOrNoneAsync, LastOrNoneAsync

Safe terminal operations that return `Option<T>` instead of throwing on empty sequences:

```csharp
// Works on any IAsyncEnumerable<T>, not just Option streams
var first = await stream.FirstOrNoneAsync();
var firstMatch = await stream.FirstOrNoneAsync(x => x > 10);

var single = await stream.SingleOrNoneAsync();
// None if empty OR if multiple elements

var last = await stream.LastOrNoneAsync();
var lastMatch = await stream.LastOrNoneAsync(x => x.IsActive);
```

## Result Stream Operations

Extensions for `IAsyncEnumerable<Result<T, TError>>` — streams where each element is either a success or a failure.

### MapResult / MapResultAsync

Transform the success value inside each `Result`, passing failures through unchanged:

```csharp
var mapped = resultStream.MapResult(user => user.Name);
// Success(user) → Success("Alice"), Failure(err) → Failure(err)

var enriched = resultStream.MapResultAsync(async user =>
    await EnrichUserAsync(user));
```

### BindResult / BindResultAsync

Apply a function returning a `Result` to each success value:

```csharp
var validated = resultStream.BindResult(user => ValidateUser(user));

var asyncValidated = resultStream.BindResultAsync(
    async user => await ValidateUserAsync(user));
```

### TapResult / TapResultError

Execute side-effects for successes or failures without modifying the stream:

```csharp
var observed = resultStream
    .TapResult(user => _logger.LogInformation("Processed {User}", user.Name))
    .TapResultError(err => _logger.LogWarning("Failed: {Error}", err.Message));
```

### ChooseResults

Filter out failures and unwrap success values:

```csharp
IAsyncEnumerable<User> users = resultStream.ChooseResults();
// Keeps only Success values, discards Failures
```

### SequenceAsync (Result)

Collect all results into a single result. Returns `Success` with all values if all succeed, or the first `Failure` (short-circuits):

```csharp
var result = await resultStream.SequenceAsync();
// All Success → Success([v1, v2, v3])
// First Failure → Failure(error)
```

### PartitionAsync (Result)

Consume the stream and separate successes from failures:

```csharp
var (successes, failures) = await resultStream.PartitionAsync();
// successes: IReadOnlyList<T>
// failures: IReadOnlyList<TError>
```

## Validation Stream Operations

Extensions for `IAsyncEnumerable<Validation<T, TError>>` — streams where each element is either valid or carries accumulated errors.

### MapValid / MapValidAsync

Transform the value inside each `Valid`, passing `Invalid` through unchanged:

```csharp
var mapped = validationStream.MapValid(x => x.ToUpper());

var enriched = validationStream.MapValidAsync(
    async x => await NormalizeAsync(x));
```

### TapValid / TapInvalid

Execute side-effects for valid or invalid elements:

```csharp
var observed = validationStream
    .TapValid(x => _logger.LogInformation("Valid: {Value}", x))
    .TapInvalid(errors =>
        _logger.LogWarning("Invalid with {Count} errors", errors.Count));
```

### ChooseValid

Filter out invalid elements and unwrap valid values:

```csharp
IAsyncEnumerable<string> validNames = validationStream.ChooseValid();
```

### SequenceAsync (Validation)

Collect all validations into a single validation. Unlike Result, this does **not** short-circuit — it accumulates **all** errors:

```csharp
var result = await validationStream.SequenceAsync();
// All Valid → Valid([v1, v2, v3])
// Any Invalid → Invalid([err1, err2, ...]) — all errors collected
```

### PartitionAsync (Validation)

Consume the stream and separate valid values from errors:

```csharp
var (valid, errors) = await validationStream.PartitionAsync();
// valid: IReadOnlyList<T>
// errors: IReadOnlyList<TError> — all errors from all Invalid elements
```

## Stream Type Conversions

Convert between monadic types within a stream. These mirror the single-value `TypeConversionExtensions` but operate element-wise on async sequences.

### Option to Result

```csharp
// With a fixed error for None values
var results = optionStream.ToResultStream(new NotFoundError { Message = "Not found" });

// With a factory for lazy error creation
var results = optionStream.ToResultStream(() => new NotFoundError { Message = "Missing" });
```

### Result to Option

```csharp
// Discard error information: Success → Some, Failure → None
var options = resultStream.ToOptionStream();
```

### Result to Either

```csharp
// Success → Right, Failure → Left
var eithers = resultStream.ToEitherStream();
```

### Option to Either

```csharp
// Some → Right, None → Left with provided value
var eithers = optionStream.ToEitherStream("missing");
```

### Validation to Result

```csharp
// Valid → Success, Invalid → Failure (first error)
var results = validationStream.ToResultStream();
```

### Result to Validation

```csharp
// Success → Valid, Failure → Invalid (single error)
var validations = resultStream.ToValidationStream();
```

## Composition Patterns

These extensions compose naturally with each other and with existing DarkPeak.Functional operations.

### Pipeline: fetch, validate, and collect

```csharp
var result = await userIds
    .ToAsyncEnumerable()
    .MapAsync(async id => await _repository.FindAsync(id))     // IAsyncEnumerable<Option<User>>
    .ToResultStream(new NotFoundError { Message = "User not found" })  // → Result stream
    .MapResult(user => user with { LastAccessed = DateTime.UtcNow })
    .BindResultAsync(async user => await _validator.ValidateAsync(user))
    .SequenceAsync();
// Result<IReadOnlyList<User>, Error> — all users or first failure
```

### Side-effects with Tap

```csharp
await events
    .Tap(e => _metrics.RecordEvent(e))
    .FilterAsync(async e => await ShouldProcessAsync(e))
    .Buffer(50)
    .ForEachAsync(async batch => await _processor.ProcessBatchAsync(batch));
```

### Unfold for pagination

```csharp
var allItems = AsyncEnumerableExtensions.UnfoldAsync(
    "/api/items?page=1",
    async url =>
    {
        var response = await _httpClient.GetAsync<PagedResponse>(url);
        return response.Match(
            success: page => page.NextPageUrl is not null
                ? Option.Some((page.Items.AsEnumerable(), page.NextPageUrl))
                : Option.Some((page.Items.AsEnumerable(), (string)null!)),
            failure: _ => Option.None<(IEnumerable<Item>, string)>());
    })
    .Bind(items => items.ToAsyncEnumerable());
```

## Relationship to Sequence & Traverse

The `SequenceAsync` methods on async streams are the streaming counterpart of the `Sequence` / `SequenceAsync` methods on `IEnumerable<Task<Result<T, E>>>`. The key differences:

| Aspect | `IEnumerable` Sequence/Traverse | `IAsyncEnumerable` SequenceAsync |
|---|---|---|
| Input | Eagerly-available collection of monads | Lazily-produced async stream of monads |
| Evaluation | Sequential or parallel (`SequenceParallel`) | Always sequential (stream order) |
| Memory | All inputs in memory | Processes one element at a time |
| Use case | Known collection of tasks | Unknown/unbounded streams |

Use `IEnumerable` Sequence/Traverse when you have a fixed set of operations to run. Use `IAsyncEnumerable` extensions when consuming a stream (database cursors, paginated APIs, message queues, file processing).

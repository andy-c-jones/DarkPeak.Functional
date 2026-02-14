# Option&lt;T&gt;

`Option<T>` represents a value that may or may not be present. It's a type-safe alternative to `null` that makes the absence of a value explicit in your type signatures.

## Creating Options

```csharp
// Explicit construction
var some = Option.Some(42);          // Some(42)
var none = Option.None<int>();       // None

// From nullable references
string? name = GetName();
var option = Option.From(name);      // Some or None

// From nullable value types
int? age = GetAge();
var option = Option.From(age);       // Some or None

// Implicit conversion
Option<int> x = 42;                  // Some(42)
```

## Try and TryParse

Safely wrap operations that might throw:

```csharp
// Catch exceptions and return None
var parsed = Option.Try(() => int.Parse("not a number")); // None
var valid  = Option.Try(() => int.Parse("42"));            // Some(42)

// Async version
var data = await Option.TryAsync(() => FetchDataAsync());

// Type-safe parsing via IParsable<T>
var number = Option.TryParse<int>("42");           // Some(42)
var bad    = Option.TryParse<int>("abc");           // None
var date   = Option.TryParse<DateOnly>("2024-01-15"); // Some(2024-01-15)
```

## Transforming Values

### Map

Transform the value inside an Option. If the Option is None, the function is not called:

```csharp
var greeting = Option.Some("Alice")
    .Map(name => $"Hello, {name}!"); // Some("Hello, Alice!")

var nothing = Option.None<string>()
    .Map(name => $"Hello, {name}!"); // None
```

### Bind

Chain operations that themselves return Options (flatMap):

```csharp
Option<User> FindUser(int id) => /* ... */;
Option<Address> GetAddress(User user) => /* ... */;

var address = FindUser(123)
    .Bind(user => GetAddress(user)); // Some(address) or None
```

### Filter

Keep the value only if a predicate is satisfied:

```csharp
var adult = Option.Some(25).Filter(age => age >= 18); // Some(25)
var minor = Option.Some(15).Filter(age => age >= 18); // None
```

## Extracting Values

### Match (recommended)

Exhaustively handle both cases:

```csharp
var message = option.Match(
    some: value => $"Found: {value}",
    none: () => "Not found");
```

### GetValueOrDefault

Provide a fallback:

```csharp
var value = option.GetValueOrDefault(0);
var value = option.GetValueOrDefault(() => ComputeDefault());
```

### GetValueOrThrow

Escape hatch — throws `InvalidOperationException` if None:

```csharp
var value = option.GetValueOrThrow(); // throws if None
```

### OrElse

Provide an alternative Option:

```csharp
var result = primaryLookup.OrElse(fallbackLookup);
var result = primaryLookup.OrElse(() => ExpensiveFallback());
```

## Side Effects

```csharp
option
    .Tap(value => Console.WriteLine($"Got: {value}"))
    .TapNone(() => Console.WriteLine("Nothing found"));
```

## LINQ Support

```csharp
var result =
    from user in FindUser(123)
    from address in GetAddress(user)
    where address.City == "London"
    select address.PostCode;
```

## IEnumerable Support

`Option<T>` implements `IEnumerable<T>` — Some yields one element, None yields zero:

```csharp
var values = options.SelectMany(opt => opt); // flatten to present values
```

## Async Operations

Every operation has an async variant:

```csharp
var result = await option
    .MapAsync(async x => await TransformAsync(x))
    .BindAsync(async x => await LookupAsync(x))
    .MatchAsync(
        some: async x => await FormatAsync(x),
        none: () => Task.FromResult("Not found"));
```

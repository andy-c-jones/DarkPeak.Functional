# Entity Framework

The `DarkPeak.Functional.EntityFramework` package wraps Entity Framework Core operations in `Result<T, Error>`, enabling railway-oriented database access without try/catch blocks.

## Installation

```bash
dotnet add package DarkPeak.Functional.EntityFramework
```

## Basic Usage

```csharp
using DarkPeak.Functional;
using DarkPeak.Functional.EntityFramework;

// Save changes with error handling
dbContext.Products.Add(new Product { Name = "Widget", Price = 9.99m });
var result = await dbContext.SaveChangesResultAsync();

result.Match(
    success: count => logger.LogInformation("Saved {Count} entities", count),
    failure: error => logger.LogError("Save failed: {Error}", error.Message));
```

## Query Methods

Query extensions are added to `IQueryable<T>`, so they compose with standard LINQ:

```csharp
// Materialize to a list
var products = await dbContext.Set<Product>()
    .Where(p => p.Category == "Electronics")
    .OrderBy(p => p.Name)
    .ToListResultAsync();

// First row (fails if empty)
var latest = await dbContext.Set<Order>()
    .OrderByDescending(o => o.CreatedAt)
    .FirstResultAsync();

// Single row (fails if zero or multiple)
var setting = await dbContext.Set<Setting>()
    .Where(s => s.Key == "app.theme")
    .SingleResultAsync();

// Count
var count = await dbContext.Set<Order>()
    .Where(o => o.Status == OrderStatus.Pending)
    .CountResultAsync();

// Any
var exists = await dbContext.Set<User>()
    .Where(u => u.Email == "admin@example.com")
    .AnyResultAsync();
```

## Optional Results with Option<T>

Methods that may return no rows use `Result<Option<T>, Error>` instead of returning null:

```csharp
// Find by primary key
var result = await dbContext.FindResultAsync<Product>(42);

var message = result.Match(
    success: option => option.Match(
        some: product => $"Found: {product.Name}",
        none: () => "Product not found"),
    failure: error => $"Error: {error.Message}");

// First or none
var user = await dbContext.Set<User>()
    .Where(u => u.Email == "alice@example.com")
    .FirstOrDefaultResultAsync();

// Single or none (fails if multiple)
var setting = await dbContext.Set<Setting>()
    .Where(s => s.Key == "app.theme")
    .SingleOrDefaultResultAsync();
```

## SaveChanges

`SaveChangesResultAsync` wraps `DbContext.SaveChangesAsync`, returning the number of state entries written:

```csharp
dbContext.Orders.Add(new Order { CustomerId = 1, Total = 99.99m });
var result = await dbContext.SaveChangesResultAsync();

result.Match(
    success: count => logger.LogInformation("Saved {Count} entities", count),
    failure: error => logger.LogError("Save failed: {Error}", error.Message));
```

## Transactions

`ExecuteInTransactionAsync` manages the full transaction lifecycle using EF Core's `Database.BeginTransactionAsync`. The transaction is committed on success and rolled back on failure or exception:

```csharp
var result = await dbContext.ExecuteInTransactionAsync(async ctx =>
{
    var order = new Order { CustomerId = 1 };
    ctx.Orders.Add(order);
    var saveOrder = await ctx.SaveChangesResultAsync();

    return await saveOrder.BindAsync(async _ =>
    {
        ctx.OrderItems.Add(new OrderItem { OrderId = order.Id, ProductId = 5 });
        var saveItems = await ctx.SaveChangesResultAsync();
        return saveItems.Map(_ => order.Id);
    });
});

// result is Success: both saves committed
// result is Failure: both saves rolled back
```

The Unit overload is available for operations that do not return a meaningful value:

```csharp
var result = await dbContext.ExecuteInTransactionAsync(async ctx =>
{
    var items = await ctx.Set<OrderItem>()
        .Where(i => i.OrderId == orderId)
        .ToListAsync();
    ctx.RemoveRange(items);

    var order = await ctx.Set<Order>().FindAsync(orderId);
    if (order is not null) ctx.Remove(order);

    var save = await ctx.SaveChangesResultAsync();
    return save.Map(_ => Unit.Value);
});
```

## Error Handling

EF Core's own exception hierarchy is mapped to typed errors:

| Exception | Error Type | Description |
|-----------|-----------|-------------|
| `DbUpdateConcurrencyException` | `ConcurrencyError` | Optimistic concurrency conflict |
| `DbUpdateException` | `SaveChangesError` | Save operation failure |
| Other exceptions | `EntityFrameworkError` | General EF Core error |

All three types inherit from `EntityFrameworkError`, which inherits from `Error`.

```csharp
var result = await dbContext.SaveChangesResultAsync();

result.TapError(error =>
{
    switch (error)
    {
        case ConcurrencyError concurrency:
            logger.LogWarning("Concurrency conflict on: {Entities}",
                string.Join(", ", concurrency.ConflictingEntries));
            break;
        case SaveChangesError save:
            logger.LogError("Save failed (SQLSTATE {State}): {Message}",
                save.SqlState, save.Message);
            break;
        case EntityFrameworkError ef:
            logger.LogError("EF error: {Message}", ef.Message);
            break;
    }
});
```

### ConcurrencyError

Produced when a concurrency token or row version check fails. The `ConflictingEntries` property contains the type names of the entities involved:

```csharp
if (error is ConcurrencyError concurrency)
{
    // concurrency.ConflictingEntries: ["Order", "OrderItem"]
    // Retry with fresh data, merge changes, or inform the user
}
```

### SaveChangesError

Produced for `DbUpdateException` that is not a concurrency conflict. The `SqlState` property is populated from the inner `DbException` when available:

```csharp
if (error is SaveChangesError save)
{
    // save.SqlState: e.g. "23505" for unique violation in PostgreSQL
    // save.AffectedEntries: ["User"]
}
```

No provider-specific classification is performed. Consumers who need to distinguish constraint types can inspect `SqlState` in their application code where the database vendor is known.

## Chaining with Map and Bind

Results compose naturally with the core library:

```csharp
// Transform the success value
var message = await dbContext.Set<Product>()
    .CountResultAsync()
    .Map(count => $"Found {count} products");

// Chain dependent operations
var result = await dbContext.Set<Order>()
    .OrderByDescending(o => o.CreatedAt)
    .FirstResultAsync()
    .BindAsync(order =>
        dbContext.Set<OrderItem>()
            .Where(i => i.OrderId == order.Id)
            .ToListResultAsync());
```

## Composition with Retry

Wrap database calls in a retry policy for concurrency conflict recovery:

```csharp
var result = await Retry
    .WithMaxAttempts(3)
    .WithBackoff(Backoff.Exponential(TimeSpan.FromMilliseconds(100)))
    .WithRetryWhen(error => error is ConcurrencyError)
    .ExecuteAsync(async () =>
    {
        var product = await dbContext.Set<Product>()
            .Where(p => p.Id == 42)
            .SingleResultAsync();

        return product.Map(p =>
        {
            p.Price += 1.00m;
            return p;
        }).BindAsync(_ => dbContext.SaveChangesResultAsync());
    });
```

## API Reference

| Method | Return Type | Description |
|--------|-------------|-------------|
| `SaveChangesResultAsync` | `Result<int, Error>` | Save changes, returns count |
| `FindResultAsync<T>` | `Result<Option<T>, Error>` | Find by primary key |
| `FirstOrDefaultResultAsync<T>` | `Result<Option<T>, Error>` | First row or none |
| `SingleOrDefaultResultAsync<T>` | `Result<Option<T>, Error>` | Single row or none (fails on 2+) |
| `FirstResultAsync<T>` | `Result<T, Error>` | First row (fails if empty) |
| `SingleResultAsync<T>` | `Result<T, Error>` | Single row (fails on 0 or 2+) |
| `ToListResultAsync<T>` | `Result<List<T>, Error>` | Materialize to list |
| `CountResultAsync<T>` | `Result<int, Error>` | Count elements |
| `AnyResultAsync<T>` | `Result<bool, Error>` | Check if any elements exist |
| `ExecuteInTransactionAsync<T>` | `Result<T, Error>` | Execute in transaction |
| `ExecuteInTransactionAsync` | `Result<Unit, Error>` | Execute in transaction (void) |

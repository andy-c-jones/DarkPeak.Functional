# Dapper

The `DarkPeak.Functional.Dapper` package wraps Dapper's query and execute operations in `Result<T, Error>`, enabling railway-oriented database access without try/catch blocks.

## Installation

```bash
dotnet add package DarkPeak.Functional.Dapper
```

## Basic Usage

```csharp
using DarkPeak.Functional;
using DarkPeak.Functional.Dapper;
using Npgsql;

await using var connection = new NpgsqlConnection(connectionString);
await connection.OpenAsync();

var result = await connection.QueryResultAsync<Order>(
    "SELECT * FROM orders WHERE customer_id = @CustomerId",
    new { CustomerId = 42 });

var message = result.Match(
    success: orders => $"Found {orders.Count()} orders",
    failure: error => $"Failed: {error.Message}");
```

## Query Methods

All query methods return `Result<T, Error>`, capturing any `DbException` as a `DatabaseError`:

```csharp
// Multiple rows
var orders = await connection.QueryResultAsync<Order>(
    "SELECT * FROM orders WHERE status = @Status",
    new { Status = "pending" });

// Single row (fails if zero or multiple rows)
var user = await connection.QuerySingleResultAsync<User>(
    "SELECT * FROM users WHERE id = @Id",
    new { Id = 42 });

// First row (fails if empty)
var latest = await connection.QueryFirstResultAsync<Order>(
    "SELECT * FROM orders ORDER BY created_at DESC");
```

## Optional Results with Option<T>

Methods that may return no rows use `Result<Option<T>, Error>` instead of returning null:

```csharp
// Single or none (fails if multiple rows)
var result = await connection.QuerySingleOrDefaultResultAsync<User>(
    "SELECT * FROM users WHERE email = @Email",
    new { Email = "alice@example.com" });

var message = result.Match(
    success: option => option.Match(
        some: user => $"Found: {user.Name}",
        none: () => "User not found"),
    failure: error => $"Query failed: {error.Message}");

// First or none (returns first row if any)
var order = await connection.QueryFirstOrDefaultResultAsync<Order>(
    "SELECT * FROM orders WHERE id = @Id",
    new { Id = 999 });
```

## Execute Methods

```csharp
// Execute a command, returns rows affected
var result = await connection.ExecuteResultAsync(
    "UPDATE products SET price = @Price WHERE id = @Id",
    new { Price = 29.99m, Id = 42 });

result.Tap(rows => logger.LogInformation("Updated {Rows} rows", rows));

// Execute scalar, returns a single value
var count = await connection.ExecuteScalarResultAsync<long>(
    "SELECT COUNT(*) FROM orders WHERE status = @Status",
    new { Status = "pending" });
```

## Transactions

`ExecuteInTransactionAsync` manages the full transaction lifecycle. The delegate receives both the connection and the transaction. The transaction is committed on success and rolled back on failure or exception:

```csharp
var result = await connection.ExecuteInTransactionAsync(async (conn, tx) =>
{
    var debit = await conn.ExecuteResultAsync(
        "UPDATE accounts SET balance = balance - @Amount WHERE id = @Id",
        new { Amount = 100m, Id = fromAccount }, transaction: tx);

    return await debit.BindAsync(async _ =>
        await conn.ExecuteResultAsync(
            "UPDATE accounts SET balance = balance + @Amount WHERE id = @Id",
            new { Amount = 100m, Id = toAccount }, transaction: tx));
});

// result is Success: both updates committed
// result is Failure: both updates rolled back
```

The Unit overload is available for operations that do not return a meaningful value:

```csharp
var result = await connection.ExecuteInTransactionAsync(async (conn, tx) =>
{
    await conn.ExecuteResultAsync(
        "DELETE FROM order_items WHERE order_id = @Id", new { Id = 42 }, transaction: tx);

    var delete = await conn.ExecuteResultAsync(
        "DELETE FROM orders WHERE id = @Id", new { Id = 42 }, transaction: tx);

    return delete.Map(_ => Unit.Value);
});
```

You can control the isolation level:

```csharp
var result = await connection.ExecuteInTransactionAsync(
    async (conn, tx) => { /* ... */ },
    isolationLevel: IsolationLevel.Serializable);
```

## Error Handling

All exceptions are wrapped into a single `DatabaseError` type that preserves provider-agnostic information from `DbException`:

```csharp
var result = await connection.QueryResultAsync<Order>("SELECT * FROM orders");

result.TapError(error =>
{
    if (error is DatabaseError dbError)
        logger.LogError("SQL error (SQLSTATE {SqlState}, code {Code}): {Message}",
            dbError.SqlState, dbError.ErrorNumber, dbError.Message);
});
```

| Property | Description |
|----------|-------------|
| `Message` | The exception message (inherited from `Error`) |
| `SqlState` | The SQLSTATE code from `DbException.SqlState`, if available |
| `ErrorNumber` | The vendor-specific error number from `DbException.ErrorCode`, if available |

No provider-specific classification is performed. Consumers who need to distinguish constraint violations from deadlocks can inspect `SqlState` in their application code where the database vendor is known.

## Chaining with Map and Bind

Results compose naturally with the core library:

```csharp
// Transform the success value
var total = await connection
    .ExecuteScalarResultAsync<decimal>(
        "SELECT SUM(amount) FROM orders WHERE customer_id = @Id",
        new { Id = 42 })
    .Map(sum => $"Total: {sum:C}");

// Chain dependent queries
var result = await connection
    .QueryFirstResultAsync<Order>(
        "SELECT * FROM orders WHERE id = @Id", new { Id = 42 })
    .BindAsync(order =>
        connection.QueryResultAsync<OrderItem>(
            "SELECT * FROM order_items WHERE order_id = @Id",
            new { Id = order.Id }));
```

## Composition with Retry

Wrap database calls in a retry policy for transient failure handling:

```csharp
var result = await Retry
    .WithMaxAttempts(3)
    .WithBackoff(Backoff.Exponential(TimeSpan.FromMilliseconds(100)))
    .WithRetryWhen(error => error is DatabaseError { SqlState: "40001" }) // serialization failure
    .ExecuteAsync(() =>
        connection.QueryResultAsync<Order>("SELECT * FROM orders"));
```

## API Reference

| Method | Return Type | Description |
|--------|-------------|-------------|
| `QueryResultAsync<T>` | `Result<IEnumerable<T>, Error>` | Query multiple rows |
| `QuerySingleResultAsync<T>` | `Result<T, Error>` | Exactly one row (fails on 0 or 2+) |
| `QuerySingleOrDefaultResultAsync<T>` | `Result<Option<T>, Error>` | Zero or one row (fails on 2+) |
| `QueryFirstResultAsync<T>` | `Result<T, Error>` | First row (fails if empty) |
| `QueryFirstOrDefaultResultAsync<T>` | `Result<Option<T>, Error>` | First row or none |
| `ExecuteResultAsync` | `Result<int, Error>` | Execute command, returns rows affected |
| `ExecuteScalarResultAsync<T>` | `Result<T, Error>` | Execute scalar query |
| `ExecuteInTransactionAsync<T>` | `Result<T, Error>` | Execute in transaction |
| `ExecuteInTransactionAsync` | `Result<Unit, Error>` | Execute in transaction (void) |

using DarkPeak.Functional.Dapper;
using Npgsql;
using Testcontainers.PostgreSql;

namespace DarkPeak.Functional.Dapper.Tests;

public class DbConnectionTransactionExtensionsShould : IAsyncDisposable
{
    private readonly PostgreSqlContainer _container;
    private NpgsqlConnection? _connection;

    public DbConnectionTransactionExtensionsShould()
    {
        _container = new PostgreSqlBuilder("postgres:17-alpine")
            .Build();
    }

    private async Task<NpgsqlConnection> GetConnectionAsync()
    {
        if (_connection is not null) return _connection;

        await _container.StartAsync();
        _connection = new NpgsqlConnection(_container.GetConnectionString());
        await _connection.OpenAsync();

        await using var cmd = _connection.CreateCommand();
        cmd.CommandText = """
            CREATE TABLE accounts (
                id SERIAL PRIMARY KEY,
                name TEXT NOT NULL,
                balance NUMERIC(10,2) NOT NULL DEFAULT 0
            );

            INSERT INTO accounts (name, balance) VALUES
                ('Alice', 100.00),
                ('Bob', 50.00);
            """;
        await cmd.ExecuteNonQueryAsync();

        return _connection;
    }

    public async ValueTask DisposeAsync()
    {
        if (_connection is not null) await _connection.DisposeAsync();
        await _container.DisposeAsync();
    }

    // --- Commit on success ---

    [Test]
    public async Task Commit_transaction_on_success()
    {
        var conn = await GetConnectionAsync();

        var result = await conn.ExecuteInTransactionAsync(async (c, tx) =>
        {
            var debit = await c.ExecuteResultAsync(
                "UPDATE accounts SET balance = balance - 25 WHERE id = 1", transaction: tx);

            return await debit.BindAsync(async _ =>
                await c.ExecuteResultAsync(
                    "UPDATE accounts SET balance = balance + 25 WHERE id = 2", transaction: tx));
        });

        await Assert.That(result.IsSuccess).IsTrue();

        // Verify both updates were committed
        var alice = await conn.QuerySingleResultAsync<decimal>(
            "SELECT balance FROM accounts WHERE id = 1");
        var bob = await conn.QuerySingleResultAsync<decimal>(
            "SELECT balance FROM accounts WHERE id = 2");

        await Assert.That(alice.GetValueOrThrow()).IsEqualTo(75.00m);
        await Assert.That(bob.GetValueOrThrow()).IsEqualTo(75.00m);
    }

    // --- Rollback on failure ---

    [Test]
    public async Task Rollback_transaction_on_failure()
    {
        var conn = await GetConnectionAsync();

        var result = await conn.ExecuteInTransactionAsync(async (c, tx) =>
        {
            await c.ExecuteResultAsync(
                "UPDATE accounts SET balance = balance - 25 WHERE id = 1", transaction: tx);

            // Return a failure to trigger rollback
            return Result.Failure<int, Error>(new DatabaseError
            {
                Message = "Insufficient funds"
            });
        });

        await Assert.That(result.IsFailure).IsTrue();

        // Verify the debit was rolled back
        var alice = await conn.QuerySingleResultAsync<decimal>(
            "SELECT balance FROM accounts WHERE id = 1");
        await Assert.That(alice.GetValueOrThrow()).IsEqualTo(100.00m);
    }

    // --- Rollback on exception ---

    [Test]
    public async Task Rollback_transaction_on_exception()
    {
        var conn = await GetConnectionAsync();

        var result = await conn.ExecuteInTransactionAsync(async (c, tx) =>
        {
            await c.ExecuteResultAsync(
                "UPDATE accounts SET balance = balance - 25 WHERE id = 1", transaction: tx);

            // Execute invalid SQL to cause an exception
            return await c.ExecuteResultAsync("INVALID SQL STATEMENT", transaction: tx);
        });

        await Assert.That(result.IsFailure).IsTrue();

        // Verify the debit was rolled back
        var alice = await conn.QuerySingleResultAsync<decimal>(
            "SELECT balance FROM accounts WHERE id = 1");
        await Assert.That(alice.GetValueOrThrow()).IsEqualTo(100.00m);
    }

    // --- Unit overload ---

    [Test]
    public async Task Support_unit_overload_for_void_operations()
    {
        var conn = await GetConnectionAsync();

        var result = await conn.ExecuteInTransactionAsync(async (c, tx) =>
        {
            var insert = await c.ExecuteResultAsync(
                "INSERT INTO accounts (name, balance) VALUES ('Charlie', 200.00)",
                transaction: tx);

            return insert.Map(_ => Unit.Value);
        });

        await Assert.That(result.IsSuccess).IsTrue();

        var charlie = await conn.QuerySingleOrDefaultResultAsync<decimal>(
            "SELECT balance FROM accounts WHERE name = 'Charlie'");
        await Assert.That(charlie.IsSuccess).IsTrue();
        var option = charlie.GetValueOrThrow();
        await Assert.That(option.IsSome).IsTrue();
    }

    // --- Chained operations ---

    [Test]
    public async Task Support_chained_bind_operations_in_transaction()
    {
        var conn = await GetConnectionAsync();

        var result = await conn.ExecuteInTransactionAsync(async (c, tx) =>
        {
            var insert = await c.ExecuteScalarResultAsync<int>(
                "INSERT INTO accounts (name, balance) VALUES ('Dana', 300.00) RETURNING id",
                transaction: tx);

            return await insert.BindAsync(async id =>
                await c.QuerySingleResultAsync<AccountRow>(
                    "SELECT * FROM accounts WHERE id = @Id",
                    new { Id = id }, tx));
        });

        await Assert.That(result.IsSuccess).IsTrue();
        var account = result.GetValueOrThrow();
        await Assert.That(account.Name).IsEqualTo("Dana");
        await Assert.That(account.Balance).IsEqualTo(300.00m);
    }

    private record AccountRow(int Id, string Name, decimal Balance);
}

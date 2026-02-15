using System.Data;
using DarkPeak.Functional.Dapper;

namespace DarkPeak.Functional.Dapper.Tests;

[ClassDataSource<PostgresFixture>(Shared = SharedType.PerClass)]
[NotInParallel]
public class DbConnectionTransactionExtensionsShould(PostgresFixture fixture)
{
    // --- Commit on success ---

    [Test]
    public async Task Commit_transaction_on_success()
    {
        await using var conn = await fixture.OpenConnectionAsync();

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

        // Reset balances for other tests
        await conn.ExecuteResultAsync("UPDATE accounts SET balance = 100.00 WHERE id = 1");
        await conn.ExecuteResultAsync("UPDATE accounts SET balance = 50.00 WHERE id = 2");
    }

    // --- Rollback on failure ---

    [Test]
    public async Task Rollback_transaction_on_failure()
    {
        await using var conn = await fixture.OpenConnectionAsync();

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
        await using var conn = await fixture.OpenConnectionAsync();

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

    // --- Connection auto-open ---

    [Test]
    public async Task Auto_open_closed_connection_for_transaction()
    {
        await using var closedConnection = fixture.CreateClosedConnection();
        // Connection is in Closed state — ExecuteInTransactionAsync should open it

        var result = await closedConnection.ExecuteInTransactionAsync(async (c, tx) =>
        {
            return await c.QuerySingleResultAsync<decimal>(
                "SELECT balance FROM accounts WHERE id = 1", transaction: tx);
        });

        await Assert.That(result.IsSuccess).IsTrue();
    }

    // --- Isolation level ---

    [Test]
    public async Task Accept_custom_isolation_level()
    {
        await using var conn = await fixture.OpenConnectionAsync();

        var result = await conn.ExecuteInTransactionAsync(async (c, tx) =>
        {
            return await c.QuerySingleResultAsync<decimal>(
                "SELECT balance FROM accounts WHERE id = 1", transaction: tx);
        }, IsolationLevel.Serializable);

        await Assert.That(result.IsSuccess).IsTrue();
    }

    // --- Unit overload ---

    [Test]
    public async Task Support_unit_overload_for_void_operations()
    {
        await using var conn = await fixture.OpenConnectionAsync();

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
        await using var conn = await fixture.OpenConnectionAsync();

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

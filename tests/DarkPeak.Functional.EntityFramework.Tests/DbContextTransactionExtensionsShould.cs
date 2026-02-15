using DarkPeak.Functional.EntityFramework;
using Microsoft.EntityFrameworkCore;

namespace DarkPeak.Functional.EntityFramework.Tests;

[ClassDataSource<PostgresFixture>(Shared = SharedType.PerClass)]
[NotInParallel]
public class DbContextTransactionExtensionsShould(PostgresFixture fixture)
{
    // --- Commit on success ---

    [Test]
    public async Task Commit_transaction_on_success()
    {
        await using var db = fixture.CreateDbContext();

        var result = await db.ExecuteInTransactionAsync(async ctx =>
        {
            var alice = await ctx.Set<Account>()
                .Where(a => a.Name == "Alice")
                .SingleAsync();
            var bob = await ctx.Set<Account>()
                .Where(a => a.Name == "Bob")
                .SingleAsync();

            alice.Balance -= 25;
            bob.Balance += 25;

            return await ctx.SaveChangesResultAsync();
        });

        await Assert.That(result.IsSuccess).IsTrue();

        // Verify with fresh context
        await using var verifyDb = fixture.CreateDbContext();
        var aliceBalance = await verifyDb.Set<Account>()
            .Where(a => a.Name == "Alice")
            .Select(a => a.Balance)
            .SingleAsync();
        var bobBalance = await verifyDb.Set<Account>()
            .Where(a => a.Name == "Bob")
            .Select(a => a.Balance)
            .SingleAsync();

        await Assert.That(aliceBalance).IsEqualTo(75.00m);
        await Assert.That(bobBalance).IsEqualTo(75.00m);
    }

    // --- Rollback on failure ---

    [Test]
    public async Task Rollback_transaction_on_failure()
    {
        await using var db = fixture.CreateDbContext();

        // Record balance before the test
        var balanceBefore = await db.Set<Account>()
            .Where(a => a.Name == "Alice")
            .Select(a => a.Balance)
            .SingleAsync();

        var result = await db.ExecuteInTransactionAsync(async ctx =>
        {
            var alice = await ctx.Set<Account>()
                .Where(a => a.Name == "Alice")
                .SingleAsync();

            alice.Balance -= 25;
            await ctx.SaveChangesAsync();

            // Return a failure to trigger rollback
            return Result.Failure<int, Error>(new EntityFrameworkError
            {
                Message = "Insufficient funds"
            });
        });

        await Assert.That(result.IsFailure).IsTrue();

        // Verify the debit was rolled back
        await using var verifyDb = fixture.CreateDbContext();
        var aliceBalance = await verifyDb.Set<Account>()
            .Where(a => a.Name == "Alice")
            .Select(a => a.Balance)
            .SingleAsync();
        await Assert.That(aliceBalance).IsEqualTo(balanceBefore);
    }

    // --- Rollback on exception ---

    [Test]
    public async Task Rollback_transaction_on_exception()
    {
        await using var db = fixture.CreateDbContext();

        // Record current balance before this test
        var balanceBefore = await db.Set<Account>()
            .Where(a => a.Name == "Alice")
            .Select(a => a.Balance)
            .SingleAsync();

        var result = await db.ExecuteInTransactionAsync(async ctx =>
        {
            var alice = await ctx.Set<Account>()
                .Where(a => a.Name == "Alice")
                .SingleAsync();

            alice.Balance -= 10;
            await ctx.SaveChangesAsync();

            // Force an exception inside the delegate
            throw new InvalidOperationException("Simulated failure");

#pragma warning disable CS0162 // Unreachable code
            return Result.Success<int, Error>(1);
#pragma warning restore CS0162
        });

        await Assert.That(result.IsFailure).IsTrue();

        // Verify the debit was rolled back
        await using var verifyDb = fixture.CreateDbContext();
        var aliceBalance = await verifyDb.Set<Account>()
            .Where(a => a.Name == "Alice")
            .Select(a => a.Balance)
            .SingleAsync();
        await Assert.That(aliceBalance).IsEqualTo(balanceBefore);
    }

    // --- Unit overload ---

    [Test]
    public async Task Support_unit_overload_for_void_operations()
    {
        await using var db = fixture.CreateDbContext();

        var result = await db.ExecuteInTransactionAsync(async ctx =>
        {
            ctx.Set<Account>().Add(new Account { Name = "Charlie", Balance = 200.00m });
            var save = await ctx.SaveChangesResultAsync();
            return save.Map(_ => Unit.Value);
        });

        await Assert.That(result.IsSuccess).IsTrue();

        // Verify with fresh context
        await using var verifyDb = fixture.CreateDbContext();
        var charlie = await verifyDb.Set<Account>()
            .Where(a => a.Name == "Charlie")
            .SingleOrDefaultResultAsync();

        await Assert.That(charlie.IsSuccess).IsTrue();
        var option = charlie.GetValueOrThrow();
        await Assert.That(option.IsSome).IsTrue();
    }

    // --- Chained operations ---

    [Test]
    public async Task Support_chained_bind_operations_in_transaction()
    {
        await using var db = fixture.CreateDbContext();

        var result = await db.ExecuteInTransactionAsync(async ctx =>
        {
            var account = new Account { Name = "Dana", Balance = 300.00m };
            ctx.Set<Account>().Add(account);
            var save = await ctx.SaveChangesResultAsync();

            return save.Map(_ => account);
        });

        await Assert.That(result.IsSuccess).IsTrue();
        var created = result.GetValueOrThrow();
        await Assert.That(created.Name).IsEqualTo("Dana");
        await Assert.That(created.Balance).IsEqualTo(300.00m);
    }
}

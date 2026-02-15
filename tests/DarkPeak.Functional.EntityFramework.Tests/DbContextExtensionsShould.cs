using DarkPeak.Functional.EntityFramework;
using Microsoft.EntityFrameworkCore;

namespace DarkPeak.Functional.EntityFramework.Tests;

[ClassDataSource<PostgresFixture>(Shared = SharedType.PerClass)]
[NotInParallel]
public class DbContextExtensionsShould(PostgresFixture fixture)
{
    // --- SaveChangesResultAsync ---

    [Test]
    public async Task Return_success_with_count_on_save()
    {
        await using var db = fixture.CreateDbContext();

        db.Products.Add(new Product { Name = "SaveTest", Price = 1.99m });
        var result = await db.SaveChangesResultAsync();

        await Assert.That(result.IsSuccess).IsTrue();
        var count = result.GetValueOrThrow();
        await Assert.That(count).IsEqualTo(1);
    }

    [Test]
    public async Task Return_save_changes_error_for_constraint_violation()
    {
        await using var db = fixture.CreateDbContext();

        db.Users.Add(new User { Email = "alice@example.com", Name = "Alice Duplicate" });
        var result = await db.SaveChangesResultAsync();

        await Assert.That(result.IsFailure).IsTrue();
        var error = result.Match(_ => null!, e => e);
        await Assert.That(error).IsTypeOf<SaveChangesError>();
    }

    [Test]
    public async Task Return_save_changes_error_with_sql_state()
    {
        await using var db = fixture.CreateDbContext();

        db.Users.Add(new User { Email = "alice@example.com", Name = "Dupe" });
        var result = await db.SaveChangesResultAsync();

        await Assert.That(result.IsFailure).IsTrue();
        var error = result.Match(_ => null!, e => e) as SaveChangesError;
        await Assert.That(error).IsNotNull();
        await Assert.That(error!.SqlState).IsNotNull();
    }

    [Test]
    public async Task Return_save_changes_error_with_affected_entries()
    {
        await using var db = fixture.CreateDbContext();

        db.Users.Add(new User { Email = "alice@example.com", Name = "Dupe" });
        var result = await db.SaveChangesResultAsync();

        var error = result.Match(_ => null!, e => e) as SaveChangesError;
        await Assert.That(error).IsNotNull();
        await Assert.That(error!.AffectedEntries).Contains("User");
    }

    // --- FindResultAsync ---

    [Test]
    public async Task Return_some_when_entity_found()
    {
        await using var db = fixture.CreateDbContext();

        var result = await db.FindResultAsync<Product>(1);

        await Assert.That(result.IsSuccess).IsTrue();
        var option = result.GetValueOrThrow();
        await Assert.That(option.IsSome).IsTrue();
        var product = ((Some<Product>)option).Value;
        await Assert.That(product.Name).IsEqualTo("Widget");
    }

    [Test]
    public async Task Return_none_when_entity_not_found()
    {
        await using var db = fixture.CreateDbContext();

        var result = await db.FindResultAsync<Product>(9999);

        await Assert.That(result.IsSuccess).IsTrue();
        var option = result.GetValueOrThrow();
        await Assert.That(option.IsNone).IsTrue();
    }

    // --- FirstOrDefaultResultAsync ---

    [Test]
    public async Task Return_some_for_first_or_default_with_match()
    {
        await using var db = fixture.CreateDbContext();

        var result = await db.Set<User>()
            .Where(u => u.Email == "alice@example.com")
            .FirstOrDefaultResultAsync();

        await Assert.That(result.IsSuccess).IsTrue();
        var option = result.GetValueOrThrow();
        await Assert.That(option.IsSome).IsTrue();
        var user = ((Some<User>)option).Value;
        await Assert.That(user.Name).IsEqualTo("Alice");
    }

    [Test]
    public async Task Return_none_for_first_or_default_with_no_match()
    {
        await using var db = fixture.CreateDbContext();

        var result = await db.Set<User>()
            .Where(u => u.Email == "nobody@example.com")
            .FirstOrDefaultResultAsync();

        await Assert.That(result.IsSuccess).IsTrue();
        var option = result.GetValueOrThrow();
        await Assert.That(option.IsNone).IsTrue();
    }

    // --- SingleOrDefaultResultAsync ---

    [Test]
    public async Task Return_some_for_single_or_default_with_one_match()
    {
        await using var db = fixture.CreateDbContext();

        var result = await db.Set<User>()
            .Where(u => u.Email == "bob@example.com")
            .SingleOrDefaultResultAsync();

        await Assert.That(result.IsSuccess).IsTrue();
        var option = result.GetValueOrThrow();
        await Assert.That(option.IsSome).IsTrue();
        var user = ((Some<User>)option).Value;
        await Assert.That(user.Name).IsEqualTo("Bob");
    }

    [Test]
    public async Task Return_none_for_single_or_default_with_no_match()
    {
        await using var db = fixture.CreateDbContext();

        var result = await db.Set<User>()
            .Where(u => u.Email == "nobody@example.com")
            .SingleOrDefaultResultAsync();

        await Assert.That(result.IsSuccess).IsTrue();
        var option = result.GetValueOrThrow();
        await Assert.That(option.IsNone).IsTrue();
    }

    [Test]
    public async Task Return_failure_for_single_or_default_with_multiple_matches()
    {
        await using var db = fixture.CreateDbContext();

        var result = await db.Set<Product>()
            .Where(p => p.Name == "Widget" || p.Name == "Gadget")
            .SingleOrDefaultResultAsync();

        await Assert.That(result.IsFailure).IsTrue();
    }

    // --- FirstResultAsync ---

    [Test]
    public async Task Return_success_for_first_with_rows()
    {
        await using var db = fixture.CreateDbContext();

        var result = await db.Set<Product>()
            .OrderBy(p => p.Id)
            .FirstResultAsync();

        await Assert.That(result.IsSuccess).IsTrue();
        var product = result.GetValueOrThrow();
        await Assert.That(product.Name).IsEqualTo("Widget");
    }

    [Test]
    public async Task Return_failure_for_first_with_no_rows()
    {
        await using var db = fixture.CreateDbContext();

        var result = await db.Set<Product>()
            .Where(p => p.Price > 9999m)
            .FirstResultAsync();

        await Assert.That(result.IsFailure).IsTrue();
    }

    // --- SingleResultAsync ---

    [Test]
    public async Task Return_success_for_single_with_one_row()
    {
        await using var db = fixture.CreateDbContext();

        var result = await db.Set<User>()
            .Where(u => u.Email == "alice@example.com")
            .SingleResultAsync();

        await Assert.That(result.IsSuccess).IsTrue();
        var user = result.GetValueOrThrow();
        await Assert.That(user.Name).IsEqualTo("Alice");
    }

    [Test]
    public async Task Return_failure_for_single_with_no_rows()
    {
        await using var db = fixture.CreateDbContext();

        var result = await db.Set<User>()
            .Where(u => u.Email == "nobody@example.com")
            .SingleResultAsync();

        await Assert.That(result.IsFailure).IsTrue();
    }

    [Test]
    public async Task Return_failure_for_single_with_multiple_rows()
    {
        await using var db = fixture.CreateDbContext();

        var result = await db.Set<Product>()
            .Where(p => p.Name == "Widget" || p.Name == "Gadget")
            .SingleResultAsync();

        await Assert.That(result.IsFailure).IsTrue();
    }

    // --- ToListResultAsync ---

    [Test]
    public async Task Return_success_with_list_for_valid_query()
    {
        await using var db = fixture.CreateDbContext();

        var result = await db.Set<Product>()
            .Where(p => p.Name == "Widget" || p.Name == "Gadget" || p.Name == "Doohickey")
            .OrderBy(p => p.Id)
            .ToListResultAsync();

        await Assert.That(result.IsSuccess).IsTrue();
        var products = result.GetValueOrThrow();
        await Assert.That(products.Count).IsGreaterThanOrEqualTo(3);
        await Assert.That(products[0].Name).IsEqualTo("Widget");
    }

    [Test]
    public async Task Return_success_with_empty_list_when_no_matches()
    {
        await using var db = fixture.CreateDbContext();

        var result = await db.Set<Product>()
            .Where(p => p.Price > 9999m)
            .ToListResultAsync();

        await Assert.That(result.IsSuccess).IsTrue();
        var products = result.GetValueOrThrow();
        await Assert.That(products.Count).IsEqualTo(0);
    }

    // --- CountResultAsync ---

    [Test]
    public async Task Return_success_with_count()
    {
        await using var db = fixture.CreateDbContext();

        var result = await db.Set<Product>()
            .Where(p => p.Name == "Widget" || p.Name == "Gadget" || p.Name == "Doohickey")
            .CountResultAsync();

        await Assert.That(result.IsSuccess).IsTrue();
        var count = result.GetValueOrThrow();
        await Assert.That(count).IsEqualTo(3);
    }

    [Test]
    public async Task Return_zero_count_when_no_matches()
    {
        await using var db = fixture.CreateDbContext();

        var result = await db.Set<Product>()
            .Where(p => p.Price > 9999m)
            .CountResultAsync();

        await Assert.That(result.IsSuccess).IsTrue();
        var count = result.GetValueOrThrow();
        await Assert.That(count).IsEqualTo(0);
    }

    // --- AnyResultAsync ---

    [Test]
    public async Task Return_true_when_any_matches_exist()
    {
        await using var db = fixture.CreateDbContext();

        var result = await db.Set<User>()
            .Where(u => u.Email == "alice@example.com")
            .AnyResultAsync();

        await Assert.That(result.IsSuccess).IsTrue();
        var any = result.GetValueOrThrow();
        await Assert.That(any).IsTrue();
    }

    [Test]
    public async Task Return_false_when_no_matches_exist()
    {
        await using var db = fixture.CreateDbContext();

        var result = await db.Set<User>()
            .Where(u => u.Email == "nobody@example.com")
            .AnyResultAsync();

        await Assert.That(result.IsSuccess).IsTrue();
        var any = result.GetValueOrThrow();
        await Assert.That(any).IsFalse();
    }

    // --- Chaining ---

    [Test]
    public async Task Support_result_chaining_with_map()
    {
        await using var db = fixture.CreateDbContext();

        var result = await db.Set<Product>()
            .Where(p => p.Name == "Widget" || p.Name == "Gadget" || p.Name == "Doohickey")
            .CountResultAsync();
        var message = result.Map(count => $"Found {count} products");

        await Assert.That(message.IsSuccess).IsTrue();
        await Assert.That(message.GetValueOrThrow()).IsEqualTo("Found 3 products");
    }

    [Test]
    public async Task Support_result_chaining_with_bind()
    {
        await using var db = fixture.CreateDbContext();

        var result = await db.Set<Product>()
            .OrderBy(p => p.Id)
            .FirstResultAsync();

        var priceResult = result.Map(p => p.Price);

        await Assert.That(priceResult.IsSuccess).IsTrue();
        await Assert.That(priceResult.GetValueOrThrow()).IsEqualTo(9.99m);
    }

    // --- FindResultAsync with CancellationToken overload ---

    [Test]
    public async Task Return_some_when_entity_found_with_cancellation_token()
    {
        await using var db = fixture.CreateDbContext();

        var result = await db.FindResultAsync<Product>([1], CancellationToken.None);

        await Assert.That(result.IsSuccess).IsTrue();
        var option = result.GetValueOrThrow();
        await Assert.That(option.IsSome).IsTrue();
        var product = ((Some<Product>)option).Value;
        await Assert.That(product.Name).IsEqualTo("Widget");
    }

    [Test]
    public async Task Return_none_when_entity_not_found_with_cancellation_token()
    {
        await using var db = fixture.CreateDbContext();

        var result = await db.FindResultAsync<Product>([9999], CancellationToken.None);

        await Assert.That(result.IsSuccess).IsTrue();
        var option = result.GetValueOrThrow();
        await Assert.That(option.IsNone).IsTrue();
    }

    // --- Concurrency error ---

    [Test]
    public async Task Return_concurrency_error_on_optimistic_concurrency_conflict()
    {
        // Add a ConcurrentProduct using a dedicated context
        await using (var setupDb = fixture.CreateDbContext())
        {
            setupDb.ConcurrentProducts.Add(new ConcurrentProduct { Name = "Concurrent Widget", Price = 9.99m, Version = 1 });
            await setupDb.SaveChangesAsync();
        }

        // Load the same entity in two separate contexts
        await using var ctx1 = fixture.CreateDbContext();
        await using var ctx2 = fixture.CreateDbContext();

        var product1 = await ctx1.ConcurrentProducts.FirstAsync(p => p.Name == "Concurrent Widget");
        var product2 = await ctx2.ConcurrentProducts.FirstAsync(p => p.Name == "Concurrent Widget");

        // Modify and save in ctx1 — update the version
        product1.Price = 19.99m;
        product1.Version = 2;
        await ctx1.SaveChangesAsync();

        // Modify and try to save in ctx2 — stale Version value should cause concurrency error
        product2.Price = 29.99m;
        product2.Version = 3;
        var result = await ctx2.SaveChangesResultAsync();

        await Assert.That(result.IsFailure).IsTrue();
        var error = result.Match(_ => null!, e => e);
        await Assert.That(error).IsTypeOf<ConcurrencyError>();
    }

    [Test]
    public async Task Return_concurrency_error_with_conflicting_entries()
    {
        await using (var setupDb = fixture.CreateDbContext())
        {
            setupDb.ConcurrentProducts.Add(new ConcurrentProduct { Name = "Conflict Item", Price = 5.99m, Version = 1 });
            await setupDb.SaveChangesAsync();
        }

        await using var ctx1 = fixture.CreateDbContext();
        await using var ctx2 = fixture.CreateDbContext();

        var p1 = await ctx1.ConcurrentProducts.FirstAsync(p => p.Name == "Conflict Item");
        var p2 = await ctx2.ConcurrentProducts.FirstAsync(p => p.Name == "Conflict Item");

        p1.Price = 11.99m;
        p1.Version = 2;
        await ctx1.SaveChangesAsync();

        p2.Price = 15.99m;
        p2.Version = 3;
        var result = await ctx2.SaveChangesResultAsync();

        var error = result.Match(_ => null!, e => e) as ConcurrencyError;
        await Assert.That(error).IsNotNull();
        await Assert.That(error!.Code).IsEqualTo("CONCURRENCY");
        await Assert.That(error.ConflictingEntries).Contains("ConcurrentProduct");
    }

    // --- Error code verification ---

    [Test]
    public async Task Return_save_changes_error_with_save_failed_code()
    {
        await using var db = fixture.CreateDbContext();

        db.Users.Add(new User { Email = "alice@example.com", Name = "Dupe" });
        var result = await db.SaveChangesResultAsync();

        var error = result.Match(_ => null!, e => e) as SaveChangesError;
        await Assert.That(error).IsNotNull();
        await Assert.That(error!.Code).IsEqualTo("SAVE_FAILED");
    }

    [Test]
    public async Task Return_multiple_results_code_for_single_or_default_with_duplicates()
    {
        await using var db = fixture.CreateDbContext();

        var result = await db.Set<Product>()
            .Where(p => p.Name == "Widget" || p.Name == "Gadget")
            .SingleOrDefaultResultAsync();

        await Assert.That(result.IsFailure).IsTrue();
        var error = result.Match(_ => null!, e => e) as EntityFrameworkError;
        await Assert.That(error).IsNotNull();
        await Assert.That(error!.Code).IsEqualTo("MULTIPLE_RESULTS");
    }

    [Test]
    public async Task Return_empty_result_set_code_for_first_with_no_rows()
    {
        await using var db = fixture.CreateDbContext();

        var result = await db.Set<Product>()
            .Where(p => p.Price > 9999m)
            .FirstResultAsync();

        await Assert.That(result.IsFailure).IsTrue();
        var error = result.Match(_ => null!, e => e) as EntityFrameworkError;
        await Assert.That(error).IsNotNull();
        await Assert.That(error!.Code).IsEqualTo("EMPTY_RESULT_SET");
    }

    [Test]
    public async Task Return_invalid_result_set_code_for_single_with_no_rows()
    {
        await using var db = fixture.CreateDbContext();

        var result = await db.Set<User>()
            .Where(u => u.Email == "nobody@example.com")
            .SingleResultAsync();

        await Assert.That(result.IsFailure).IsTrue();
        var error = result.Match(_ => null!, e => e) as EntityFrameworkError;
        await Assert.That(error).IsNotNull();
        await Assert.That(error!.Code).IsEqualTo("INVALID_RESULT_SET");
    }

    [Test]
    public async Task Return_invalid_result_set_code_for_single_with_multiple_rows()
    {
        await using var db = fixture.CreateDbContext();

        var result = await db.Set<Product>()
            .Where(p => p.Name == "Widget" || p.Name == "Gadget")
            .SingleResultAsync();

        await Assert.That(result.IsFailure).IsTrue();
        var error = result.Match(_ => null!, e => e) as EntityFrameworkError;
        await Assert.That(error).IsNotNull();
        await Assert.That(error!.Code).IsEqualTo("INVALID_RESULT_SET");
    }

    // --- SaveChangesResultAsync with CancellationToken ---

    [Test]
    public async Task Accept_cancellation_token_on_save_changes()
    {
        await using var db = fixture.CreateDbContext();

        db.Products.Add(new Product { Name = "CancelTest", Price = 1.00m });
        var result = await db.SaveChangesResultAsync(CancellationToken.None);

        await Assert.That(result.IsSuccess).IsTrue();
    }

    [Test]
    public async Task Return_cancelled_error_when_cancellation_token_is_cancelled_on_save()
    {
        await using var db = fixture.CreateDbContext();

        db.Products.Add(new Product { Name = "Cancelled", Price = 1.00m });

        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var result = await db.SaveChangesResultAsync(cts.Token);

        await Assert.That(result.IsFailure).IsTrue();
        var error = result.Match(_ => null!, e => e) as EntityFrameworkError;
        await Assert.That(error).IsNotNull();
        await Assert.That(error!.Code).IsEqualTo("CANCELLED");
    }

    [Test]
    public async Task Return_cancelled_error_when_cancellation_token_is_cancelled_on_to_list()
    {
        await using var db = fixture.CreateDbContext();

        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var result = await db.Set<Product>()
            .ToListResultAsync(cts.Token);

        await Assert.That(result.IsFailure).IsTrue();
        var error = result.Match(_ => null!, e => e) as EntityFrameworkError;
        await Assert.That(error).IsNotNull();
        await Assert.That(error!.Code).IsEqualTo("CANCELLED");
    }

    [Test]
    public async Task Return_cancelled_error_when_cancellation_token_is_cancelled_on_count()
    {
        await using var db = fixture.CreateDbContext();

        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var result = await db.Set<Product>()
            .CountResultAsync(cts.Token);

        await Assert.That(result.IsFailure).IsTrue();
        var error = result.Match(_ => null!, e => e) as EntityFrameworkError;
        await Assert.That(error).IsNotNull();
        await Assert.That(error!.Code).IsEqualTo("CANCELLED");
    }

    [Test]
    public async Task Return_cancelled_error_when_cancellation_token_is_cancelled_on_any()
    {
        await using var db = fixture.CreateDbContext();

        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var result = await db.Set<Product>()
            .AnyResultAsync(cts.Token);

        await Assert.That(result.IsFailure).IsTrue();
        var error = result.Match(_ => null!, e => e) as EntityFrameworkError;
        await Assert.That(error).IsNotNull();
        await Assert.That(error!.Code).IsEqualTo("CANCELLED");
    }

    [Test]
    public async Task Return_cancelled_error_when_cancellation_token_is_cancelled_on_first_or_default()
    {
        await using var db = fixture.CreateDbContext();

        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var result = await db.Set<Product>()
            .FirstOrDefaultResultAsync(cts.Token);

        await Assert.That(result.IsFailure).IsTrue();
        var error = result.Match(_ => null!, e => e) as EntityFrameworkError;
        await Assert.That(error).IsNotNull();
        await Assert.That(error!.Code).IsEqualTo("CANCELLED");
    }

    [Test]
    public async Task Return_cancelled_error_when_cancellation_token_is_cancelled_on_single_or_default()
    {
        await using var db = fixture.CreateDbContext();

        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var result = await db.Set<Product>()
            .SingleOrDefaultResultAsync(cts.Token);

        await Assert.That(result.IsFailure).IsTrue();
        var error = result.Match(_ => null!, e => e) as EntityFrameworkError;
        await Assert.That(error).IsNotNull();
        await Assert.That(error!.Code).IsEqualTo("CANCELLED");
    }

    [Test]
    public async Task Return_cancelled_error_when_cancellation_token_is_cancelled_on_first()
    {
        await using var db = fixture.CreateDbContext();

        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var result = await db.Set<Product>()
            .FirstResultAsync(cts.Token);

        await Assert.That(result.IsFailure).IsTrue();
        var error = result.Match(_ => null!, e => e) as EntityFrameworkError;
        await Assert.That(error).IsNotNull();
        await Assert.That(error!.Code).IsEqualTo("CANCELLED");
    }

    [Test]
    public async Task Return_cancelled_error_when_cancellation_token_is_cancelled_on_single()
    {
        await using var db = fixture.CreateDbContext();

        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var result = await db.Set<Product>()
            .SingleResultAsync(cts.Token);

        await Assert.That(result.IsFailure).IsTrue();
        var error = result.Match(_ => null!, e => e) as EntityFrameworkError;
        await Assert.That(error).IsNotNull();
        await Assert.That(error!.Code).IsEqualTo("CANCELLED");
    }

    [Test]
    public async Task Return_cancelled_error_when_cancellation_token_is_cancelled_on_find()
    {
        await using var db = fixture.CreateDbContext();

        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var result = await db.FindResultAsync<Product>([1], cts.Token);

        await Assert.That(result.IsFailure).IsTrue();
        var error = result.Match(_ => null!, e => e) as EntityFrameworkError;
        await Assert.That(error).IsNotNull();
        await Assert.That(error!.Code).IsEqualTo("CANCELLED");
    }
}

using DarkPeak.Functional.EntityFramework;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Testcontainers.PostgreSql;

namespace DarkPeak.Functional.EntityFramework.Tests;

public class DbContextExtensionsShould : IAsyncDisposable
{
    private readonly PostgreSqlContainer _container;
    private TestDbContext? _dbContext;

    public DbContextExtensionsShould()
    {
        _container = new PostgreSqlBuilder("postgres:17-alpine")
            .Build();
    }

    private async Task<TestDbContext> GetDbContextAsync()
    {
        if (_dbContext is not null) return _dbContext;

        await _container.StartAsync();
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseNpgsql(_container.GetConnectionString())
            .Options;

        _dbContext = new TestDbContext(options);
        await _dbContext.Database.EnsureCreatedAsync();

        // Seed data
        _dbContext.Products.AddRange(
            new Product { Name = "Widget", Price = 9.99m },
            new Product { Name = "Gadget", Price = 24.99m },
            new Product { Name = "Doohickey", Price = 4.99m });

        _dbContext.Users.AddRange(
            new User { Email = "alice@example.com", Name = "Alice" },
            new User { Email = "bob@example.com", Name = "Bob" });

        await _dbContext.SaveChangesAsync();

        return _dbContext;
    }

    public async ValueTask DisposeAsync()
    {
        if (_dbContext is not null) await _dbContext.DisposeAsync();
        await _container.DisposeAsync();
    }

    // --- SaveChangesResultAsync ---

    [Test]
    public async Task Return_success_with_count_on_save()
    {
        var db = await GetDbContextAsync();

        db.Products.Add(new Product { Name = "New Item", Price = 1.99m });
        var result = await db.SaveChangesResultAsync();

        await Assert.That(result.IsSuccess).IsTrue();
        var count = result.GetValueOrThrow();
        await Assert.That(count).IsEqualTo(1);
    }

    [Test]
    public async Task Return_save_changes_error_for_constraint_violation()
    {
        var db = await GetDbContextAsync();

        db.Users.Add(new User { Email = "alice@example.com", Name = "Alice Duplicate" });
        var result = await db.SaveChangesResultAsync();

        await Assert.That(result.IsFailure).IsTrue();
        var error = result.Match(_ => null!, e => e);
        await Assert.That(error).IsTypeOf<SaveChangesError>();
    }

    [Test]
    public async Task Return_save_changes_error_with_sql_state()
    {
        var db = await GetDbContextAsync();

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
        var db = await GetDbContextAsync();

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
        var db = await GetDbContextAsync();

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
        var db = await GetDbContextAsync();

        var result = await db.FindResultAsync<Product>(9999);

        await Assert.That(result.IsSuccess).IsTrue();
        var option = result.GetValueOrThrow();
        await Assert.That(option.IsNone).IsTrue();
    }

    // --- FirstOrDefaultResultAsync ---

    [Test]
    public async Task Return_some_for_first_or_default_with_match()
    {
        var db = await GetDbContextAsync();

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
        var db = await GetDbContextAsync();

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
        var db = await GetDbContextAsync();

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
        var db = await GetDbContextAsync();

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
        var db = await GetDbContextAsync();

        var result = await db.Set<Product>()
            .Where(p => p.Price < 100m)
            .SingleOrDefaultResultAsync();

        await Assert.That(result.IsFailure).IsTrue();
    }

    // --- FirstResultAsync ---

    [Test]
    public async Task Return_success_for_first_with_rows()
    {
        var db = await GetDbContextAsync();

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
        var db = await GetDbContextAsync();

        var result = await db.Set<Product>()
            .Where(p => p.Price > 9999m)
            .FirstResultAsync();

        await Assert.That(result.IsFailure).IsTrue();
    }

    // --- SingleResultAsync ---

    [Test]
    public async Task Return_success_for_single_with_one_row()
    {
        var db = await GetDbContextAsync();

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
        var db = await GetDbContextAsync();

        var result = await db.Set<User>()
            .Where(u => u.Email == "nobody@example.com")
            .SingleResultAsync();

        await Assert.That(result.IsFailure).IsTrue();
    }

    [Test]
    public async Task Return_failure_for_single_with_multiple_rows()
    {
        var db = await GetDbContextAsync();

        var result = await db.Set<Product>()
            .Where(p => p.Price < 100m)
            .SingleResultAsync();

        await Assert.That(result.IsFailure).IsTrue();
    }

    // --- ToListResultAsync ---

    [Test]
    public async Task Return_success_with_list_for_valid_query()
    {
        var db = await GetDbContextAsync();

        var result = await db.Set<Product>()
            .OrderBy(p => p.Id)
            .ToListResultAsync();

        await Assert.That(result.IsSuccess).IsTrue();
        var products = result.GetValueOrThrow();
        await Assert.That(products.Count).IsEqualTo(3);
        await Assert.That(products[0].Name).IsEqualTo("Widget");
    }

    [Test]
    public async Task Return_success_with_empty_list_when_no_matches()
    {
        var db = await GetDbContextAsync();

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
        var db = await GetDbContextAsync();

        var result = await db.Set<Product>()
            .CountResultAsync();

        await Assert.That(result.IsSuccess).IsTrue();
        var count = result.GetValueOrThrow();
        await Assert.That(count).IsEqualTo(3);
    }

    [Test]
    public async Task Return_zero_count_when_no_matches()
    {
        var db = await GetDbContextAsync();

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
        var db = await GetDbContextAsync();

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
        var db = await GetDbContextAsync();

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
        var db = await GetDbContextAsync();

        var result = await db.Set<Product>().CountResultAsync();
        var message = result.Map(count => $"Found {count} products");

        await Assert.That(message.IsSuccess).IsTrue();
        await Assert.That(message.GetValueOrThrow()).IsEqualTo("Found 3 products");
    }

    [Test]
    public async Task Support_result_chaining_with_bind()
    {
        var db = await GetDbContextAsync();

        var result = await db.Set<Product>()
            .OrderBy(p => p.Id)
            .FirstResultAsync();

        var priceResult = result.Map(p => p.Price);

        await Assert.That(priceResult.IsSuccess).IsTrue();
        await Assert.That(priceResult.GetValueOrThrow()).IsEqualTo(9.99m);
    }
}

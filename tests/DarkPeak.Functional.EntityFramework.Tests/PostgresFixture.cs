using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;
using TUnit.Core.Interfaces;

namespace DarkPeak.Functional.EntityFramework.Tests;

public class PostgresFixture : IAsyncInitializer, IAsyncDisposable
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder("postgres:17-alpine")
        .Build();

    public string ConnectionString => _container.GetConnectionString();

    public async Task InitializeAsync()
    {
        await _container.StartAsync();

        await using var seedContext = CreateDbContext();
        await seedContext.Database.EnsureCreatedAsync();

        seedContext.Products.AddRange(
            new Product { Name = "Widget", Price = 9.99m },
            new Product { Name = "Gadget", Price = 24.99m },
            new Product { Name = "Doohickey", Price = 4.99m });

        seedContext.Users.AddRange(
            new User { Email = "alice@example.com", Name = "Alice" },
            new User { Email = "bob@example.com", Name = "Bob" });

        seedContext.Accounts.AddRange(
            new Account { Name = "Alice", Balance = 100.00m },
            new Account { Name = "Bob", Balance = 50.00m });

        await seedContext.SaveChangesAsync();
    }

    public TestDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseNpgsql(ConnectionString)
            .Options;

        return new TestDbContext(options);
    }

    public async ValueTask DisposeAsync()
    {
        await _container.DisposeAsync();
    }
}

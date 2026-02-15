using DarkPeak.Functional.Dapper;
using Npgsql;
using Testcontainers.PostgreSql;

namespace DarkPeak.Functional.Dapper.Tests;

public class DbConnectionExtensionsShould : IAsyncDisposable
{
    private readonly PostgreSqlContainer _container;
    private NpgsqlConnection? _connection;

    public DbConnectionExtensionsShould()
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

        // Create test schema
        await using var cmd = _connection.CreateCommand();
        cmd.CommandText = """
            CREATE TABLE products (
                id SERIAL PRIMARY KEY,
                name TEXT NOT NULL,
                price NUMERIC(10,2) NOT NULL
            );

            CREATE TABLE users (
                id SERIAL PRIMARY KEY,
                email TEXT NOT NULL UNIQUE,
                name TEXT NOT NULL
            );

            INSERT INTO products (name, price) VALUES
                ('Widget', 9.99),
                ('Gadget', 24.99),
                ('Doohickey', 4.99);

            INSERT INTO users (email, name) VALUES
                ('alice@example.com', 'Alice'),
                ('bob@example.com', 'Bob');
            """;
        await cmd.ExecuteNonQueryAsync();

        return _connection;
    }

    public async ValueTask DisposeAsync()
    {
        if (_connection is not null) await _connection.DisposeAsync();
        await _container.DisposeAsync();
    }

    // --- QueryResultAsync ---

    [Test]
    public async Task Return_success_with_rows_for_valid_query()
    {
        var conn = await GetConnectionAsync();

        var result = await conn.QueryResultAsync<ProductRow>("SELECT * FROM products ORDER BY id");

        await Assert.That(result.IsSuccess).IsTrue();
        var products = result.GetValueOrThrow().ToList();
        await Assert.That(products.Count).IsEqualTo(3);
        await Assert.That(products[0].Name).IsEqualTo("Widget");
    }

    [Test]
    public async Task Return_success_with_empty_sequence_when_no_rows_match()
    {
        var conn = await GetConnectionAsync();

        var result = await conn.QueryResultAsync<ProductRow>(
            "SELECT * FROM products WHERE price > @Price",
            new { Price = 9999m });

        await Assert.That(result.IsSuccess).IsTrue();
        var products = result.GetValueOrThrow().ToList();
        await Assert.That(products.Count).IsEqualTo(0);
    }

    [Test]
    public async Task Return_failure_with_database_error_for_invalid_sql()
    {
        var conn = await GetConnectionAsync();

        var result = await conn.QueryResultAsync<ProductRow>("SELECT * FROM nonexistent_table");

        await Assert.That(result.IsFailure).IsTrue();
        var error = result.Match(_ => null!, e => e);
        await Assert.That(error).IsTypeOf<DatabaseError>();
    }

    [Test]
    public async Task Return_database_error_with_sql_state_populated()
    {
        var conn = await GetConnectionAsync();

        var result = await conn.QueryResultAsync<ProductRow>("SELECT * FROM nonexistent_table");

        await Assert.That(result.IsFailure).IsTrue();
        var error = result.Match(_ => null!, e => e) as DatabaseError;
        await Assert.That(error).IsNotNull();
        await Assert.That(error!.SqlState).IsNotNull();
    }

    // --- QuerySingleResultAsync ---

    [Test]
    public async Task Return_success_for_query_single_with_one_row()
    {
        var conn = await GetConnectionAsync();

        var result = await conn.QuerySingleResultAsync<ProductRow>(
            "SELECT * FROM products WHERE id = @Id",
            new { Id = 1 });

        await Assert.That(result.IsSuccess).IsTrue();
        var product = result.GetValueOrThrow();
        await Assert.That(product.Name).IsEqualTo("Widget");
    }

    [Test]
    public async Task Return_failure_for_query_single_with_no_rows()
    {
        var conn = await GetConnectionAsync();

        var result = await conn.QuerySingleResultAsync<ProductRow>(
            "SELECT * FROM products WHERE id = @Id",
            new { Id = 9999 });

        await Assert.That(result.IsFailure).IsTrue();
    }

    [Test]
    public async Task Return_failure_for_query_single_with_multiple_rows()
    {
        var conn = await GetConnectionAsync();

        var result = await conn.QuerySingleResultAsync<ProductRow>("SELECT * FROM products");

        await Assert.That(result.IsFailure).IsTrue();
    }

    // --- QuerySingleOrDefaultResultAsync ---

    [Test]
    public async Task Return_some_for_query_single_or_default_with_one_row()
    {
        var conn = await GetConnectionAsync();

        var result = await conn.QuerySingleOrDefaultResultAsync<ProductRow>(
            "SELECT * FROM products WHERE id = @Id",
            new { Id = 1 });

        await Assert.That(result.IsSuccess).IsTrue();
        var option = result.GetValueOrThrow();
        await Assert.That(option.IsSome).IsTrue();
        var product = ((Some<ProductRow>)option).Value;
        await Assert.That(product.Name).IsEqualTo("Widget");
    }

    [Test]
    public async Task Return_none_for_query_single_or_default_with_no_rows()
    {
        var conn = await GetConnectionAsync();

        var result = await conn.QuerySingleOrDefaultResultAsync<ProductRow>(
            "SELECT * FROM products WHERE id = @Id",
            new { Id = 9999 });

        await Assert.That(result.IsSuccess).IsTrue();
        var option = result.GetValueOrThrow();
        await Assert.That(option.IsNone).IsTrue();
    }

    [Test]
    public async Task Return_failure_for_query_single_or_default_with_multiple_rows()
    {
        var conn = await GetConnectionAsync();

        var result = await conn.QuerySingleOrDefaultResultAsync<ProductRow>("SELECT * FROM products");

        await Assert.That(result.IsFailure).IsTrue();
    }

    // --- QueryFirstResultAsync ---

    [Test]
    public async Task Return_success_for_query_first()
    {
        var conn = await GetConnectionAsync();

        var result = await conn.QueryFirstResultAsync<ProductRow>(
            "SELECT * FROM products ORDER BY id");

        await Assert.That(result.IsSuccess).IsTrue();
        var product = result.GetValueOrThrow();
        await Assert.That(product.Name).IsEqualTo("Widget");
    }

    [Test]
    public async Task Return_failure_for_query_first_with_no_rows()
    {
        var conn = await GetConnectionAsync();

        var result = await conn.QueryFirstResultAsync<ProductRow>(
            "SELECT * FROM products WHERE id = @Id",
            new { Id = 9999 });

        await Assert.That(result.IsFailure).IsTrue();
    }

    // --- QueryFirstOrDefaultResultAsync ---

    [Test]
    public async Task Return_some_for_query_first_or_default_with_rows()
    {
        var conn = await GetConnectionAsync();

        var result = await conn.QueryFirstOrDefaultResultAsync<ProductRow>(
            "SELECT * FROM products ORDER BY id");

        await Assert.That(result.IsSuccess).IsTrue();
        var option = result.GetValueOrThrow();
        await Assert.That(option.IsSome).IsTrue();
    }

    [Test]
    public async Task Return_none_for_query_first_or_default_with_no_rows()
    {
        var conn = await GetConnectionAsync();

        var result = await conn.QueryFirstOrDefaultResultAsync<ProductRow>(
            "SELECT * FROM products WHERE id = @Id",
            new { Id = 9999 });

        await Assert.That(result.IsSuccess).IsTrue();
        var option = result.GetValueOrThrow();
        await Assert.That(option.IsNone).IsTrue();
    }

    // --- ExecuteResultAsync ---

    [Test]
    public async Task Return_rows_affected_for_execute()
    {
        var conn = await GetConnectionAsync();

        var result = await conn.ExecuteResultAsync(
            "UPDATE products SET price = price + 1 WHERE id = @Id",
            new { Id = 1 });

        await Assert.That(result.IsSuccess).IsTrue();
        var rowsAffected = result.GetValueOrThrow();
        await Assert.That(rowsAffected).IsEqualTo(1);
    }

    [Test]
    public async Task Return_zero_rows_affected_when_no_match()
    {
        var conn = await GetConnectionAsync();

        var result = await conn.ExecuteResultAsync(
            "UPDATE products SET price = 0 WHERE id = @Id",
            new { Id = 9999 });

        await Assert.That(result.IsSuccess).IsTrue();
        var rowsAffected = result.GetValueOrThrow();
        await Assert.That(rowsAffected).IsEqualTo(0);
    }

    [Test]
    public async Task Return_failure_for_constraint_violation_on_execute()
    {
        var conn = await GetConnectionAsync();

        var result = await conn.ExecuteResultAsync(
            "INSERT INTO users (email, name) VALUES (@Email, @Name)",
            new { Email = "alice@example.com", Name = "Alice Duplicate" });

        await Assert.That(result.IsFailure).IsTrue();
        var error = result.Match(_ => null!, e => e);
        await Assert.That(error).IsTypeOf<DatabaseError>();
    }

    // --- ExecuteScalarResultAsync ---

    [Test]
    public async Task Return_scalar_value()
    {
        var conn = await GetConnectionAsync();

        var result = await conn.ExecuteScalarResultAsync<long>(
            "SELECT COUNT(*) FROM products");

        await Assert.That(result.IsSuccess).IsTrue();
        var count = result.GetValueOrThrow();
        await Assert.That(count).IsEqualTo(3);
    }

    [Test]
    public async Task Return_failure_for_invalid_scalar_query()
    {
        var conn = await GetConnectionAsync();

        var result = await conn.ExecuteScalarResultAsync<int>("SELECT COUNT(*) FROM no_such_table");

        await Assert.That(result.IsFailure).IsTrue();
    }

    // --- Parameterised queries ---

    [Test]
    public async Task Support_parameterised_queries()
    {
        var conn = await GetConnectionAsync();

        var result = await conn.QueryResultAsync<ProductRow>(
            "SELECT * FROM products WHERE price < @MaxPrice ORDER BY price",
            new { MaxPrice = 10m });

        await Assert.That(result.IsSuccess).IsTrue();
        var products = result.GetValueOrThrow().ToList();
        await Assert.That(products.Count).IsEqualTo(2);
    }

    // --- Chaining ---

    [Test]
    public async Task Support_result_chaining_with_map()
    {
        var conn = await GetConnectionAsync();

        var result = await conn.ExecuteScalarResultAsync<long>("SELECT COUNT(*) FROM products");
        var message = result.Map(count => $"Found {count} products");

        await Assert.That(message.IsSuccess).IsTrue();
        await Assert.That(message.GetValueOrThrow()).IsEqualTo("Found 3 products");
    }

    [Test]
    public async Task Support_result_chaining_with_bind()
    {
        var conn = await GetConnectionAsync();

        var result = await conn.QueryFirstResultAsync<ProductRow>(
            "SELECT * FROM products WHERE id = @Id", new { Id = 1 });

        var priceResult = result.Map(p => p.Price);

        await Assert.That(priceResult.IsSuccess).IsTrue();
        await Assert.That(priceResult.GetValueOrThrow()).IsEqualTo(9.99m);
    }

    private record ProductRow(int Id, string Name, decimal Price);
}

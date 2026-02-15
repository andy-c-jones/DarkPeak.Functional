using DarkPeak.Functional.Dapper;
using Npgsql;

namespace DarkPeak.Functional.Dapper.Tests;

[ClassDataSource<PostgresFixture>(Shared = SharedType.PerClass)]
[NotInParallel]
public class DbConnectionExtensionsShould(PostgresFixture fixture)
{
    // --- QueryResultAsync ---

    [Test]
    public async Task Return_success_with_rows_for_valid_query()
    {
        await using var conn = await fixture.OpenConnectionAsync();

        var result = await conn.QueryResultAsync<ProductRow>("SELECT * FROM products ORDER BY id");

        await Assert.That(result.IsSuccess).IsTrue();
        var products = result.GetValueOrThrow().ToList();
        await Assert.That(products.Count).IsEqualTo(3);
        await Assert.That(products[0].Name).IsEqualTo("Widget");
    }

    [Test]
    public async Task Return_success_with_empty_sequence_when_no_rows_match()
    {
        await using var conn = await fixture.OpenConnectionAsync();

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
        await using var conn = await fixture.OpenConnectionAsync();

        var result = await conn.QueryResultAsync<ProductRow>("SELECT * FROM nonexistent_table");

        await Assert.That(result.IsFailure).IsTrue();
        var error = result.Match(_ => null!, e => e);
        await Assert.That(error).IsTypeOf<DatabaseError>();
    }

    [Test]
    public async Task Return_database_error_with_sql_state_populated()
    {
        await using var conn = await fixture.OpenConnectionAsync();

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
        await using var conn = await fixture.OpenConnectionAsync();

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
        await using var conn = await fixture.OpenConnectionAsync();

        var result = await conn.QuerySingleResultAsync<ProductRow>(
            "SELECT * FROM products WHERE id = @Id",
            new { Id = 9999 });

        await Assert.That(result.IsFailure).IsTrue();
    }

    [Test]
    public async Task Return_failure_for_query_single_with_multiple_rows()
    {
        await using var conn = await fixture.OpenConnectionAsync();

        var result = await conn.QuerySingleResultAsync<ProductRow>("SELECT * FROM products");

        await Assert.That(result.IsFailure).IsTrue();
    }

    // --- QuerySingleOrDefaultResultAsync ---

    [Test]
    public async Task Return_some_for_query_single_or_default_with_one_row()
    {
        await using var conn = await fixture.OpenConnectionAsync();

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
        await using var conn = await fixture.OpenConnectionAsync();

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
        await using var conn = await fixture.OpenConnectionAsync();

        var result = await conn.QuerySingleOrDefaultResultAsync<ProductRow>("SELECT * FROM products");

        await Assert.That(result.IsFailure).IsTrue();
    }

    // --- QueryFirstResultAsync ---

    [Test]
    public async Task Return_success_for_query_first()
    {
        await using var conn = await fixture.OpenConnectionAsync();

        var result = await conn.QueryFirstResultAsync<ProductRow>(
            "SELECT * FROM products ORDER BY id");

        await Assert.That(result.IsSuccess).IsTrue();
        var product = result.GetValueOrThrow();
        await Assert.That(product.Name).IsEqualTo("Widget");
    }

    [Test]
    public async Task Return_failure_for_query_first_with_no_rows()
    {
        await using var conn = await fixture.OpenConnectionAsync();

        var result = await conn.QueryFirstResultAsync<ProductRow>(
            "SELECT * FROM products WHERE id = @Id",
            new { Id = 9999 });

        await Assert.That(result.IsFailure).IsTrue();
    }

    // --- QueryFirstOrDefaultResultAsync ---

    [Test]
    public async Task Return_some_for_query_first_or_default_with_rows()
    {
        await using var conn = await fixture.OpenConnectionAsync();

        var result = await conn.QueryFirstOrDefaultResultAsync<ProductRow>(
            "SELECT * FROM products ORDER BY id");

        await Assert.That(result.IsSuccess).IsTrue();
        var option = result.GetValueOrThrow();
        await Assert.That(option.IsSome).IsTrue();
    }

    [Test]
    public async Task Return_none_for_query_first_or_default_with_no_rows()
    {
        await using var conn = await fixture.OpenConnectionAsync();

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
        await using var conn = await fixture.OpenConnectionAsync();

        var result = await conn.ExecuteResultAsync(
            "UPDATE products SET price = price + 1 WHERE id = @Id",
            new { Id = 1 });

        await Assert.That(result.IsSuccess).IsTrue();
        var rowsAffected = result.GetValueOrThrow();
        await Assert.That(rowsAffected).IsEqualTo(1);

        // Reset price to original value so other tests are not affected
        await conn.ExecuteResultAsync(
            "UPDATE products SET price = price - 1 WHERE id = @Id",
            new { Id = 1 });
    }

    [Test]
    public async Task Return_zero_rows_affected_when_no_match()
    {
        await using var conn = await fixture.OpenConnectionAsync();

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
        await using var conn = await fixture.OpenConnectionAsync();

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
        await using var conn = await fixture.OpenConnectionAsync();

        var result = await conn.ExecuteScalarResultAsync<long>(
            "SELECT COUNT(*) FROM products");

        await Assert.That(result.IsSuccess).IsTrue();
        var count = result.GetValueOrThrow();
        await Assert.That(count).IsEqualTo(3);
    }

    [Test]
    public async Task Return_failure_for_invalid_scalar_query()
    {
        await using var conn = await fixture.OpenConnectionAsync();

        var result = await conn.ExecuteScalarResultAsync<int>("SELECT COUNT(*) FROM no_such_table");

        await Assert.That(result.IsFailure).IsTrue();
    }

    // --- Error code verification ---

    [Test]
    public async Task Return_invalid_result_set_code_for_query_single_with_no_rows()
    {
        await using var conn = await fixture.OpenConnectionAsync();

        var result = await conn.QuerySingleResultAsync<ProductRow>(
            "SELECT * FROM products WHERE id = @Id",
            new { Id = 9999 });

        await Assert.That(result.IsFailure).IsTrue();
        var error = result.Match(_ => null!, e => e) as DatabaseError;
        await Assert.That(error).IsNotNull();
        await Assert.That(error!.Code).IsEqualTo("INVALID_RESULT_SET");
    }

    [Test]
    public async Task Return_invalid_result_set_code_for_query_single_with_multiple_rows()
    {
        await using var conn = await fixture.OpenConnectionAsync();

        var result = await conn.QuerySingleResultAsync<ProductRow>("SELECT * FROM products");

        await Assert.That(result.IsFailure).IsTrue();
        var error = result.Match(_ => null!, e => e) as DatabaseError;
        await Assert.That(error).IsNotNull();
        await Assert.That(error!.Code).IsEqualTo("INVALID_RESULT_SET");
    }

    [Test]
    public async Task Return_invalid_result_set_code_for_query_single_or_default_with_multiple_rows()
    {
        await using var conn = await fixture.OpenConnectionAsync();

        var result = await conn.QuerySingleOrDefaultResultAsync<ProductRow>("SELECT * FROM products");

        await Assert.That(result.IsFailure).IsTrue();
        var error = result.Match(_ => null!, e => e) as DatabaseError;
        await Assert.That(error).IsNotNull();
        await Assert.That(error!.Code).IsEqualTo("INVALID_RESULT_SET");
    }

    [Test]
    public async Task Return_empty_result_set_code_for_query_first_with_no_rows()
    {
        await using var conn = await fixture.OpenConnectionAsync();

        var result = await conn.QueryFirstResultAsync<ProductRow>(
            "SELECT * FROM products WHERE id = @Id",
            new { Id = 9999 });

        await Assert.That(result.IsFailure).IsTrue();
        var error = result.Match(_ => null!, e => e) as DatabaseError;
        await Assert.That(error).IsNotNull();
        await Assert.That(error!.Code).IsEqualTo("EMPTY_RESULT_SET");
    }

    // --- Database error on invalid SQL for each method ---

    [Test]
    public async Task Return_database_error_for_query_single_with_invalid_sql()
    {
        await using var conn = await fixture.OpenConnectionAsync();

        var result = await conn.QuerySingleResultAsync<ProductRow>("SELECT * FROM nonexistent");

        await Assert.That(result.IsFailure).IsTrue();
        var error = result.Match(_ => null!, e => e) as DatabaseError;
        await Assert.That(error).IsNotNull();
        await Assert.That(error!.Code).IsEqualTo("DATABASE");
    }

    [Test]
    public async Task Return_database_error_for_query_single_or_default_with_invalid_sql()
    {
        await using var conn = await fixture.OpenConnectionAsync();

        var result = await conn.QuerySingleOrDefaultResultAsync<ProductRow>("SELECT * FROM nonexistent");

        await Assert.That(result.IsFailure).IsTrue();
        var error = result.Match(_ => null!, e => e) as DatabaseError;
        await Assert.That(error).IsNotNull();
        await Assert.That(error!.Code).IsEqualTo("DATABASE");
    }

    [Test]
    public async Task Return_database_error_for_query_first_with_invalid_sql()
    {
        await using var conn = await fixture.OpenConnectionAsync();

        var result = await conn.QueryFirstResultAsync<ProductRow>("SELECT * FROM nonexistent");

        await Assert.That(result.IsFailure).IsTrue();
        var error = result.Match(_ => null!, e => e) as DatabaseError;
        await Assert.That(error).IsNotNull();
        await Assert.That(error!.Code).IsEqualTo("DATABASE");
    }

    [Test]
    public async Task Return_database_error_for_query_first_or_default_with_invalid_sql()
    {
        await using var conn = await fixture.OpenConnectionAsync();

        var result = await conn.QueryFirstOrDefaultResultAsync<ProductRow>("SELECT * FROM nonexistent");

        await Assert.That(result.IsFailure).IsTrue();
        var error = result.Match(_ => null!, e => e) as DatabaseError;
        await Assert.That(error).IsNotNull();
        await Assert.That(error!.Code).IsEqualTo("DATABASE");
    }

    [Test]
    public async Task Return_database_error_for_execute_with_invalid_sql()
    {
        await using var conn = await fixture.OpenConnectionAsync();

        var result = await conn.ExecuteResultAsync("INSERT INTO nonexistent (x) VALUES (1)");

        await Assert.That(result.IsFailure).IsTrue();
        var error = result.Match(_ => null!, e => e) as DatabaseError;
        await Assert.That(error).IsNotNull();
        await Assert.That(error!.Code).IsEqualTo("DATABASE");
    }

    // --- Parameterised queries ---

    [Test]
    public async Task Support_parameterised_queries()
    {
        await using var conn = await fixture.OpenConnectionAsync();

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
        await using var conn = await fixture.OpenConnectionAsync();

        var result = await conn.ExecuteScalarResultAsync<long>("SELECT COUNT(*) FROM products");
        var message = result.Map(count => $"Found {count} products");

        await Assert.That(message.IsSuccess).IsTrue();
        await Assert.That(message.GetValueOrThrow()).IsEqualTo("Found 3 products");
    }

    [Test]
    public async Task Support_result_chaining_with_bind()
    {
        await using var conn = await fixture.OpenConnectionAsync();

        var result = await conn.QueryFirstResultAsync<ProductRow>(
            "SELECT * FROM products WHERE id = @Id", new { Id = 1 });

        var priceResult = result.Map(p => p.Price);

        await Assert.That(priceResult.IsSuccess).IsTrue();
        await Assert.That(priceResult.GetValueOrThrow()).IsEqualTo(9.99m);
    }

    private record ProductRow(int Id, string Name, decimal Price);
}

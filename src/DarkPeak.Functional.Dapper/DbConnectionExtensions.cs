using System.Data;
using System.Data.Common;
using Dapper;

namespace DarkPeak.Functional.Dapper;

/// <summary>
/// Provides extension methods for <see cref="DbConnection"/> that wrap Dapper query and execute
/// operations in <see cref="Result{T, TError}"/>, enabling railway-oriented programming for
/// database access.
/// </summary>
/// <remarks>
/// <para>
/// These extensions eliminate the need for try/catch blocks around database calls by capturing
/// SQL exceptions as typed <see cref="Error"/> values. Each method delegates to the corresponding
/// Dapper method and maps exceptions to <see cref="DatabaseError"/>.
/// </para>
/// <para>
/// All <see cref="DbException"/> instances are wrapped into <see cref="DatabaseError"/>,
/// preserving the provider-agnostic <see cref="DbException.SqlState"/> and
/// <see cref="System.Runtime.InteropServices.ExternalException.ErrorCode"/> properties. Consumers who need provider-specific
/// classification (e.g. distinguishing constraint violations from deadlocks) can inspect
/// these properties in their application code where the database vendor is known.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// using var connection = new NpgsqlConnection(connectionString);
///
/// // Query multiple rows
/// var orders = await connection.QueryResultAsync&lt;Order&gt;(
///     "SELECT * FROM orders WHERE customer_id = @CustomerId",
///     new { CustomerId = 42 });
///
/// // Chain operations
/// var total = await connection
///     .QuerySingleResultAsync&lt;decimal&gt;(
///         "SELECT SUM(amount) FROM orders WHERE customer_id = @Id",
///         new { Id = 42 })
///     .Map(sum => $"Total: {sum:C}");
/// </code>
/// </example>
public static class DbConnectionExtensions
{
    /// <summary>
    /// Executes a query and returns the result sequence, wrapped in a <see cref="Result{T, TError}"/>.
    /// </summary>
    /// <typeparam name="T">The type to deserialize each row to.</typeparam>
    /// <param name="connection">The database connection to query on.</param>
    /// <param name="sql">The SQL query to execute.</param>
    /// <param name="param">Optional query parameters.</param>
    /// <param name="transaction">Optional transaction to associate with the command.</param>
    /// <param name="commandTimeout">Optional command timeout in seconds.</param>
    /// <param name="commandType">Optional command type (text, stored procedure, etc.).</param>
    /// <returns>
    /// A <see cref="Result{T, TError}"/> containing the query results on success,
    /// or a typed <see cref="Error"/> describing what went wrong.
    /// </returns>
    /// <example>
    /// <code>
    /// var result = await connection.QueryResultAsync&lt;Product&gt;(
    ///     "SELECT * FROM products WHERE category = @Cat",
    ///     new { Cat = "Electronics" });
    ///
    /// result.Match(
    ///     success: products => products.ToList(),
    ///     failure: error => { logger.LogError(error.Message); return []; });
    /// </code>
    /// </example>
    public static async Task<Result<IEnumerable<T>, Error>> QueryResultAsync<T>(
        this DbConnection connection,
        string sql,
        object? param = null,
        DbTransaction? transaction = null,
        int? commandTimeout = null,
        CommandType? commandType = null)
    {
        try
        {
            var results = await connection.QueryAsync<T>(sql, param, transaction, commandTimeout, commandType);
            return Result.Success<IEnumerable<T>, Error>(results);
        }
        catch (Exception ex)
        {
            return Result.Failure<IEnumerable<T>, Error>(DatabaseExceptionMapper.Map(ex));
        }
    }

    /// <summary>
    /// Executes a query and returns the first result, wrapped in a <see cref="Result{T, TError}"/>.
    /// Returns a failure if the sequence is empty or contains more than one element.
    /// </summary>
    /// <typeparam name="T">The type to deserialize the row to.</typeparam>
    /// <param name="connection">The database connection to query on.</param>
    /// <param name="sql">The SQL query to execute.</param>
    /// <param name="param">Optional query parameters.</param>
    /// <param name="transaction">Optional transaction to associate with the command.</param>
    /// <param name="commandTimeout">Optional command timeout in seconds.</param>
    /// <param name="commandType">Optional command type.</param>
    /// <returns>
    /// A <see cref="Result{T, TError}"/> containing the single result on success,
    /// or a typed <see cref="Error"/> if the query fails, returns no rows, or returns multiple rows.
    /// </returns>
    /// <example>
    /// <code>
    /// var result = await connection.QuerySingleResultAsync&lt;User&gt;(
    ///     "SELECT * FROM users WHERE id = @Id",
    ///     new { Id = 42 });
    /// </code>
    /// </example>
    public static async Task<Result<T, Error>> QuerySingleResultAsync<T>(
        this DbConnection connection,
        string sql,
        object? param = null,
        DbTransaction? transaction = null,
        int? commandTimeout = null,
        CommandType? commandType = null)
    {
        try
        {
            var result = await connection.QuerySingleAsync<T>(sql, param, transaction, commandTimeout, commandType);
            return Result.Success<T, Error>(result);
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure<T, Error>(new DatabaseError
            {
                Message = ex.Message,
                Code = "INVALID_RESULT_SET"
            });
        }
        catch (Exception ex)
        {
            return Result.Failure<T, Error>(DatabaseExceptionMapper.Map(ex));
        }
    }

    /// <summary>
    /// Executes a query and returns the single result as an <see cref="Option{T}"/>,
    /// wrapped in a <see cref="Result{T, TError}"/>. Returns <see cref="Option.None{T}"/>
    /// when the query returns no rows, avoiding null and default value pitfalls.
    /// </summary>
    /// <typeparam name="T">The type to deserialize the row to.</typeparam>
    /// <param name="connection">The database connection to query on.</param>
    /// <param name="sql">The SQL query to execute.</param>
    /// <param name="param">Optional query parameters.</param>
    /// <param name="transaction">Optional transaction to associate with the command.</param>
    /// <param name="commandTimeout">Optional command timeout in seconds.</param>
    /// <param name="commandType">Optional command type.</param>
    /// <returns>
    /// A <see cref="Result{T, TError}"/> containing <see cref="Option.Some{T}"/> with the row
    /// if found, <see cref="Option.None{T}"/> if not found, or a typed <see cref="Error"/> on failure.
    /// Returns a failure if the query returns more than one row.
    /// </returns>
    /// <example>
    /// <code>
    /// var result = await connection.QuerySingleOrDefaultResultAsync&lt;User&gt;(
    ///     "SELECT * FROM users WHERE email = @Email",
    ///     new { Email = "alice@example.com" });
    ///
    /// var message = result.Match(
    ///     success: option => option.Match(
    ///         some: user => $"Found: {user.Name}",
    ///         none: () => "User not found"),
    ///     failure: error => $"Query failed: {error.Message}");
    /// </code>
    /// </example>
    public static async Task<Result<Option<T>, Error>> QuerySingleOrDefaultResultAsync<T>(
        this DbConnection connection,
        string sql,
        object? param = null,
        DbTransaction? transaction = null,
        int? commandTimeout = null,
        CommandType? commandType = null)
    {
        try
        {
            var result = await connection.QuerySingleOrDefaultAsync<T>(sql, param, transaction, commandTimeout, commandType);
            var option = result is not null
                ? Option.Some(result)
                : Option.None<T>();
            return Result.Success<Option<T>, Error>(option);
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure<Option<T>, Error>(new DatabaseError
            {
                Message = ex.Message,
                Code = "INVALID_RESULT_SET"
            });
        }
        catch (Exception ex)
        {
            return Result.Failure<Option<T>, Error>(DatabaseExceptionMapper.Map(ex));
        }
    }

    /// <summary>
    /// Executes a query and returns the first result, wrapped in a <see cref="Result{T, TError}"/>.
    /// Returns a failure if the sequence is empty.
    /// </summary>
    /// <typeparam name="T">The type to deserialize the row to.</typeparam>
    /// <param name="connection">The database connection to query on.</param>
    /// <param name="sql">The SQL query to execute.</param>
    /// <param name="param">Optional query parameters.</param>
    /// <param name="transaction">Optional transaction to associate with the command.</param>
    /// <param name="commandTimeout">Optional command timeout in seconds.</param>
    /// <param name="commandType">Optional command type.</param>
    /// <returns>
    /// A <see cref="Result{T, TError}"/> containing the first row on success,
    /// or a typed <see cref="Error"/> if the query fails or returns no rows.
    /// </returns>
    /// <example>
    /// <code>
    /// var result = await connection.QueryFirstResultAsync&lt;Order&gt;(
    ///     "SELECT * FROM orders WHERE customer_id = @Id ORDER BY created_at DESC",
    ///     new { Id = 42 });
    /// </code>
    /// </example>
    public static async Task<Result<T, Error>> QueryFirstResultAsync<T>(
        this DbConnection connection,
        string sql,
        object? param = null,
        DbTransaction? transaction = null,
        int? commandTimeout = null,
        CommandType? commandType = null)
    {
        try
        {
            var result = await connection.QueryFirstAsync<T>(sql, param, transaction, commandTimeout, commandType);
            return Result.Success<T, Error>(result);
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure<T, Error>(new DatabaseError
            {
                Message = ex.Message,
                Code = "EMPTY_RESULT_SET"
            });
        }
        catch (Exception ex)
        {
            return Result.Failure<T, Error>(DatabaseExceptionMapper.Map(ex));
        }
    }

    /// <summary>
    /// Executes a query and returns the first result as an <see cref="Option{T}"/>,
    /// wrapped in a <see cref="Result{T, TError}"/>. Returns <see cref="Option.None{T}"/>
    /// when the query returns no rows.
    /// </summary>
    /// <typeparam name="T">The type to deserialize the row to.</typeparam>
    /// <param name="connection">The database connection to query on.</param>
    /// <param name="sql">The SQL query to execute.</param>
    /// <param name="param">Optional query parameters.</param>
    /// <param name="transaction">Optional transaction to associate with the command.</param>
    /// <param name="commandTimeout">Optional command timeout in seconds.</param>
    /// <param name="commandType">Optional command type.</param>
    /// <returns>
    /// A <see cref="Result{T, TError}"/> containing <see cref="Option.Some{T}"/> with the first
    /// row if found, <see cref="Option.None{T}"/> if not found, or a typed <see cref="Error"/> on failure.
    /// </returns>
    /// <example>
    /// <code>
    /// var result = await connection.QueryFirstOrDefaultResultAsync&lt;Order&gt;(
    ///     "SELECT * FROM orders WHERE id = @Id",
    ///     new { Id = 999 });
    ///
    /// var order = result
    ///     .Map(opt => opt.GetValueOrDefault(Order.Empty));
    /// </code>
    /// </example>
    public static async Task<Result<Option<T>, Error>> QueryFirstOrDefaultResultAsync<T>(
        this DbConnection connection,
        string sql,
        object? param = null,
        DbTransaction? transaction = null,
        int? commandTimeout = null,
        CommandType? commandType = null)
    {
        try
        {
            var result = await connection.QueryFirstOrDefaultAsync<T>(sql, param, transaction, commandTimeout, commandType);
            var option = result is not null
                ? Option.Some(result)
                : Option.None<T>();
            return Result.Success<Option<T>, Error>(option);
        }
        catch (Exception ex)
        {
            return Result.Failure<Option<T>, Error>(DatabaseExceptionMapper.Map(ex));
        }
    }

    /// <summary>
    /// Executes a command (INSERT, UPDATE, DELETE, etc.) and returns the number of rows affected,
    /// wrapped in a <see cref="Result{T, TError}"/>.
    /// </summary>
    /// <param name="connection">The database connection to execute on.</param>
    /// <param name="sql">The SQL command to execute.</param>
    /// <param name="param">Optional command parameters.</param>
    /// <param name="transaction">Optional transaction to associate with the command.</param>
    /// <param name="commandTimeout">Optional command timeout in seconds.</param>
    /// <param name="commandType">Optional command type.</param>
    /// <returns>
    /// A <see cref="Result{T, TError}"/> containing the number of rows affected on success,
    /// or a typed <see cref="Error"/> describing what went wrong.
    /// </returns>
    /// <example>
    /// <code>
    /// var result = await connection.ExecuteResultAsync(
    ///     "UPDATE products SET price = @Price WHERE id = @Id",
    ///     new { Price = 29.99m, Id = 42 });
    ///
    /// result.Tap(rows => logger.LogInformation("Updated {Rows} rows", rows));
    /// </code>
    /// </example>
    public static async Task<Result<int, Error>> ExecuteResultAsync(
        this DbConnection connection,
        string sql,
        object? param = null,
        DbTransaction? transaction = null,
        int? commandTimeout = null,
        CommandType? commandType = null)
    {
        try
        {
            var rowsAffected = await connection.ExecuteAsync(sql, param, transaction, commandTimeout, commandType);
            return Result.Success<int, Error>(rowsAffected);
        }
        catch (Exception ex)
        {
            return Result.Failure<int, Error>(DatabaseExceptionMapper.Map(ex));
        }
    }

    /// <summary>
    /// Executes a command and returns the first column of the first row as a scalar value,
    /// wrapped in a <see cref="Result{T, TError}"/>.
    /// </summary>
    /// <typeparam name="T">The type of the scalar value.</typeparam>
    /// <param name="connection">The database connection to execute on.</param>
    /// <param name="sql">The SQL command to execute.</param>
    /// <param name="param">Optional command parameters.</param>
    /// <param name="transaction">Optional transaction to associate with the command.</param>
    /// <param name="commandTimeout">Optional command timeout in seconds.</param>
    /// <param name="commandType">Optional command type.</param>
    /// <returns>
    /// A <see cref="Result{T, TError}"/> containing the scalar value on success,
    /// or a typed <see cref="Error"/> describing what went wrong.
    /// </returns>
    /// <example>
    /// <code>
    /// var result = await connection.ExecuteScalarResultAsync&lt;int&gt;(
    ///     "SELECT COUNT(*) FROM orders WHERE status = @Status",
    ///     new { Status = "pending" });
    ///
    /// var count = result.GetValueOrDefault(0);
    /// </code>
    /// </example>
    public static async Task<Result<T, Error>> ExecuteScalarResultAsync<T>(
        this DbConnection connection,
        string sql,
        object? param = null,
        DbTransaction? transaction = null,
        int? commandTimeout = null,
        CommandType? commandType = null)
    {
        try
        {
            var result = await connection.ExecuteScalarAsync<T>(sql, param, transaction, commandTimeout, commandType);
            return Result.Success<T, Error>(result!);
        }
        catch (Exception ex)
        {
            return Result.Failure<T, Error>(DatabaseExceptionMapper.Map(ex));
        }
    }
}

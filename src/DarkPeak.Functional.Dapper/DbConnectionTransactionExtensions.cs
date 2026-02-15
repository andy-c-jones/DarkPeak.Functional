using System.Data;
using System.Data.Common;

namespace DarkPeak.Functional.Dapper;

/// <summary>
/// Provides functional transaction support for <see cref="DbConnection"/>, enabling
/// railway-oriented transactional workflows that automatically commit on success
/// and roll back on failure or exception.
/// </summary>
/// <remarks>
/// <para>
/// These methods manage the full transaction lifecycle: opening the connection (if needed),
/// beginning a transaction, executing the provided delegate, and committing or rolling back
/// based on the result. If the delegate returns a <see cref="Failure{T, TError}"/>, the
/// transaction is rolled back and the failure is propagated. If an exception is thrown,
/// the transaction is rolled back and the exception is mapped to a typed <see cref="Error"/>.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// var result = await connection.ExecuteInTransactionAsync(async (conn, tx) =>
/// {
///     var debit = await conn.ExecuteResultAsync(
///         "UPDATE accounts SET balance = balance - @Amount WHERE id = @Id",
///         new { Amount = 100m, Id = fromAccount }, tx);
///
///     return await debit.BindAsync(async _ =>
///         await conn.ExecuteResultAsync(
///             "UPDATE accounts SET balance = balance + @Amount WHERE id = @Id",
///             new { Amount = 100m, Id = toAccount }, tx));
/// });
///
/// // result is Success: both updates committed
/// // result is Failure: both updates rolled back
/// </code>
/// </example>
public static class DbConnectionTransactionExtensions
{
    /// <summary>
    /// Executes the provided delegate within a database transaction, returning a
    /// <see cref="Result{T, TError}"/>. The transaction is committed if the delegate
    /// returns a success, and rolled back if it returns a failure or throws an exception.
    /// </summary>
    /// <typeparam name="T">The type of the success value.</typeparam>
    /// <param name="connection">The database connection to use.</param>
    /// <param name="operation">
    /// An asynchronous delegate that receives the connection and transaction,
    /// and returns a <see cref="Result{T, TError}"/>.
    /// </param>
    /// <param name="isolationLevel">
    /// The isolation level for the transaction. Defaults to <see cref="IsolationLevel.ReadCommitted"/>.
    /// </param>
    /// <returns>
    /// A <see cref="Result{T, TError}"/> containing the delegate's result on success
    /// (transaction committed), or a typed <see cref="Error"/> on failure (transaction rolled back).
    /// </returns>
    /// <remarks>
    /// <para>
    /// The connection is opened automatically if it is not already open. The caller retains
    /// ownership of the connection and is responsible for disposing it.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Transfer funds between accounts
    /// var result = await connection.ExecuteInTransactionAsync(async (conn, tx) =>
    /// {
    ///     var debit = await conn.ExecuteResultAsync(
    ///         "UPDATE accounts SET balance = balance - @Amount WHERE id = @From",
    ///         new { Amount = 50m, From = 1 }, tx);
    ///
    ///     return await debit.BindAsync(async _ =>
    ///         await conn.ExecuteResultAsync(
    ///             "UPDATE accounts SET balance = balance + @Amount WHERE id = @To",
    ///             new { Amount = 50m, To = 2 }, tx));
    /// });
    /// </code>
    /// </example>
    public static async Task<Result<T, Error>> ExecuteInTransactionAsync<T>(
        this DbConnection connection,
        Func<DbConnection, DbTransaction, Task<Result<T, Error>>> operation,
        IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
    {
        if (connection.State != ConnectionState.Open)
            await connection.OpenAsync();

        await using var transaction = await connection.BeginTransactionAsync(isolationLevel);

        try
        {
            var result = await operation(connection, transaction);

            return await result.MatchAsync<Result<T, Error>>(
                success: async value =>
                {
                    await transaction.CommitAsync();
                    return Result.Success<T, Error>(value);
                },
                failure: async error =>
                {
                    await transaction.RollbackAsync();
                    return Result.Failure<T, Error>(error);
                });
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return Result.Failure<T, Error>(DatabaseExceptionMapper.Map(ex));
        }
    }

    /// <summary>
    /// Executes the provided delegate within a database transaction, returning a
    /// <see cref="Result{T, TError}"/> with <see cref="Unit"/> as the success type.
    /// Use this overload for operations that do not produce a meaningful return value.
    /// </summary>
    /// <param name="connection">The database connection to use.</param>
    /// <param name="operation">
    /// An asynchronous delegate that receives the connection and transaction,
    /// and returns a <see cref="Result{T, TError}"/> with <see cref="Unit"/>.
    /// </param>
    /// <param name="isolationLevel">
    /// The isolation level for the transaction. Defaults to <see cref="IsolationLevel.ReadCommitted"/>.
    /// </param>
    /// <returns>
    /// A <see cref="Result{T, TError}"/> containing <see cref="Unit"/> on success
    /// (transaction committed), or a typed <see cref="Error"/> on failure (transaction rolled back).
    /// </returns>
    /// <example>
    /// <code>
    /// var result = await connection.ExecuteInTransactionAsync(async (conn, tx) =>
    /// {
    ///     await conn.ExecuteResultAsync(
    ///         "DELETE FROM order_items WHERE order_id = @Id", new { Id = 42 }, tx);
    ///
    ///     var delete = await conn.ExecuteResultAsync(
    ///         "DELETE FROM orders WHERE id = @Id", new { Id = 42 }, tx);
    ///
    ///     return delete.Map(_ => Unit.Value);
    /// });
    /// </code>
    /// </example>
    public static Task<Result<Unit, Error>> ExecuteInTransactionAsync(
        this DbConnection connection,
        Func<DbConnection, DbTransaction, Task<Result<Unit, Error>>> operation,
        IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
    {
        return connection.ExecuteInTransactionAsync<Unit>(operation, isolationLevel);
    }
}

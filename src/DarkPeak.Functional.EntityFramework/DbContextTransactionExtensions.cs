using Microsoft.EntityFrameworkCore;

namespace DarkPeak.Functional.EntityFramework;

/// <summary>
/// Provides functional transaction support for <see cref="DbContext"/>, enabling
/// railway-oriented transactional workflows that automatically commit on success
/// and roll back on failure or exception.
/// </summary>
/// <remarks>
/// <para>
/// These methods manage the full transaction lifecycle using
/// <see cref="Microsoft.EntityFrameworkCore.Infrastructure.DatabaseFacade.BeginTransactionAsync"/>.
/// If the delegate returns a <see cref="Failure{T, TError}"/>, the transaction is rolled back
/// and the failure is propagated. If an exception is thrown, the transaction is rolled back
/// and the exception is mapped to a typed <see cref="Error"/>.
/// </para>
/// <para>
/// <strong>Note:</strong> The delegate receives the same <see cref="DbContext"/> instance,
/// and <see cref="DbContext.SaveChangesAsync(CancellationToken)"/> must be called within the delegate for changes
/// to be persisted. The transaction wraps all calls to <c>SaveChangesAsync</c> made within
/// the delegate.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// var result = await dbContext.ExecuteInTransactionAsync(async ctx =>
/// {
///     ctx.Orders.Add(new Order { CustomerId = 1, Total = 99.99m });
///     var saveOrder = await ctx.SaveChangesResultAsync();
///
///     return await saveOrder.BindAsync(async _ =>
///     {
///         ctx.AuditLogs.Add(new AuditLog { Action = "OrderCreated" });
///         return await ctx.SaveChangesResultAsync();
///     });
/// });
///
/// // result is Success: both saves committed
/// // result is Failure: both saves rolled back
/// </code>
/// </example>
public static class DbContextTransactionExtensions
{
    /// <summary>
    /// Executes the provided delegate within a database transaction, returning a
    /// <see cref="Result{T, TError}"/>. The transaction is committed if the delegate
    /// returns a success, and rolled back if it returns a failure or throws an exception.
    /// </summary>
    /// <typeparam name="T">The type of the success value.</typeparam>
    /// <param name="context">The <see cref="DbContext"/> to use.</param>
    /// <param name="operation">
    /// An asynchronous delegate that receives the <see cref="DbContext"/>
    /// and returns a <see cref="Result{T, TError}"/>.
    /// </param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>
    /// A <see cref="Result{T, TError}"/> containing the delegate's result on success
    /// (transaction committed), or a typed <see cref="Error"/> on failure (transaction rolled back).
    /// </returns>
    /// <remarks>
    /// <para>
    /// The caller is responsible for calling <see cref="DbContext.SaveChangesAsync(CancellationToken)"/> within
    /// the delegate. The transaction wraps all save operations made during the delegate's execution.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Create an order with line items in a single transaction
    /// var result = await dbContext.ExecuteInTransactionAsync(async ctx =>
    /// {
    ///     var order = new Order { CustomerId = 1 };
    ///     ctx.Orders.Add(order);
    ///     var saveOrder = await ctx.SaveChangesResultAsync();
    ///
    ///     return await saveOrder.BindAsync(async _ =>
    ///     {
    ///         ctx.OrderItems.Add(new OrderItem { OrderId = order.Id, ProductId = 5 });
    ///         var saveItems = await ctx.SaveChangesResultAsync();
    ///         return saveItems.Map(_ => order.Id);
    ///     });
    /// });
    /// </code>
    /// </example>
    public static async Task<Result<T, Error>> ExecuteInTransactionAsync<T>(
        this DbContext context,
        Func<DbContext, Task<Result<T, Error>>> operation,
        CancellationToken cancellationToken = default)
    {
        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            var result = await operation(context);

            return await result.MatchAsync<Result<T, Error>>(
                success: async value =>
                {
                    await transaction.CommitAsync(cancellationToken);
                    return Result.Success<T, Error>(value);
                },
                failure: async error =>
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return Result.Failure<T, Error>(error);
                });
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            return Result.Failure<T, Error>(EfExceptionMapper.Map(ex));
        }
    }

    /// <summary>
    /// Executes the provided delegate within a database transaction, returning a
    /// <see cref="Result{T, TError}"/> with <see cref="Unit"/> as the success type.
    /// Use this overload for operations that do not produce a meaningful return value.
    /// </summary>
    /// <param name="context">The <see cref="DbContext"/> to use.</param>
    /// <param name="operation">
    /// An asynchronous delegate that receives the <see cref="DbContext"/>
    /// and returns a <see cref="Result{T, TError}"/> with <see cref="Unit"/>.
    /// </param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>
    /// A <see cref="Result{T, TError}"/> containing <see cref="Unit"/> on success
    /// (transaction committed), or a typed <see cref="Error"/> on failure (transaction rolled back).
    /// </returns>
    /// <example>
    /// <code>
    /// var result = await dbContext.ExecuteInTransactionAsync(async ctx =>
    /// {
    ///     var items = await ctx.Set&lt;OrderItem&gt;()
    ///         .Where(i => i.OrderId == orderId)
    ///         .ToListAsync();
    ///     ctx.RemoveRange(items);
    ///
    ///     var order = await ctx.Set&lt;Order&gt;().FindAsync(orderId);
    ///     if (order is not null) ctx.Remove(order);
    ///
    ///     var save = await ctx.SaveChangesResultAsync();
    ///     return save.Map(_ => Unit.Value);
    /// });
    /// </code>
    /// </example>
    public static Task<Result<Unit, Error>> ExecuteInTransactionAsync(
        this DbContext context,
        Func<DbContext, Task<Result<Unit, Error>>> operation,
        CancellationToken cancellationToken = default)
    {
        return context.ExecuteInTransactionAsync<Unit>(operation, cancellationToken);
    }
}

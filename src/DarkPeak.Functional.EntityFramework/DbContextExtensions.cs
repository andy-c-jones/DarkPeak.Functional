using Microsoft.EntityFrameworkCore;

namespace DarkPeak.Functional.EntityFramework;

/// <summary>
/// Provides extension methods for <see cref="DbContext"/> that wrap Entity Framework Core
/// query and save operations in <see cref="Result{T, TError}"/>, enabling railway-oriented
/// programming for database access.
/// </summary>
/// <remarks>
/// <para>
/// These extensions eliminate the need for try/catch blocks around EF Core calls by capturing
/// exceptions as typed <see cref="Error"/> values. Each method delegates to the corresponding
/// EF Core method and maps exceptions to the appropriate error type.
/// </para>
/// <para>
/// <strong>Exception mapping:</strong>
/// </para>
/// <list type="table">
///   <listheader>
///     <term>Exception</term>
///     <description>Error Type</description>
///   </listheader>
///   <item><term><see cref="DbUpdateConcurrencyException"/></term><description><see cref="ConcurrencyError"/></description></item>
///   <item><term><see cref="DbUpdateException"/></term><description><see cref="SaveChangesError"/></description></item>
///   <item><term>Other exceptions</term><description><see cref="EntityFrameworkError"/></description></item>
/// </list>
/// </remarks>
/// <example>
/// <code>
/// // Save changes with error handling
/// dbContext.Products.Add(new Product { Name = "Widget", Price = 9.99m });
/// var result = await dbContext.SaveChangesResultAsync();
///
/// // Find an entity by primary key
/// var product = await dbContext.FindResultAsync&lt;Product&gt;(42);
///
/// // Query with LINQ
/// var expensive = await dbContext.ToListResultAsync(
///     dbContext.Products.Where(p => p.Price > 100));
/// </code>
/// </example>
public static class DbContextExtensions
{
    /// <summary>
    /// Saves all changes made in this context to the database, wrapped in a
    /// <see cref="Result{T, TError}"/>. Returns the number of state entries written
    /// to the database on success.
    /// </summary>
    /// <param name="context">The <see cref="DbContext"/> to save changes for.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>
    /// A <see cref="Result{T, TError}"/> containing the number of state entries written
    /// on success, or a typed <see cref="Error"/> describing what went wrong.
    /// </returns>
    /// <example>
    /// <code>
    /// dbContext.Orders.Add(new Order { CustomerId = 1, Total = 99.99m });
    /// var result = await dbContext.SaveChangesResultAsync();
    /// result.Match(
    ///     success: count => logger.LogInformation("Saved {Count} entities", count),
    ///     failure: error => logger.LogError("Save failed: {Error}", error.Message));
    /// </code>
    /// </example>
    public static async Task<Result<int, Error>> SaveChangesResultAsync(
        this DbContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var count = await context.SaveChangesAsync(cancellationToken);
            return Result.Success<int, Error>(count);
        }
        catch (Exception ex)
        {
            return Result.Failure<int, Error>(EfExceptionMapper.Map(ex));
        }
    }

    /// <summary>
    /// Finds an entity with the given primary key values, returning the result as an
    /// <see cref="Option{T}"/> wrapped in a <see cref="Result{T, TError}"/>. Returns
    /// <see cref="Option.None{T}"/> when the entity is not found, avoiding null pitfalls.
    /// </summary>
    /// <typeparam name="T">The entity type to find.</typeparam>
    /// <param name="context">The <see cref="DbContext"/> to search in.</param>
    /// <param name="keyValues">The primary key values of the entity to find.</param>
    /// <returns>
    /// A <see cref="Result{T, TError}"/> containing <see cref="Option.Some{T}"/> with the entity
    /// if found, <see cref="Option.None{T}"/> if not found, or a typed <see cref="Error"/> on failure.
    /// </returns>
    /// <example>
    /// <code>
    /// var result = await dbContext.FindResultAsync&lt;Product&gt;(42);
    ///
    /// var message = result.Match(
    ///     success: option => option.Match(
    ///         some: product => $"Found: {product.Name}",
    ///         none: () => "Product not found"),
    ///     failure: error => $"Error: {error.Message}");
    /// </code>
    /// </example>
    public static async Task<Result<Option<T>, Error>> FindResultAsync<T>(
        this DbContext context,
        params object?[] keyValues) where T : class
    {
        try
        {
            var entity = await context.Set<T>().FindAsync(keyValues);
            var option = entity is not null
                ? Option.Some(entity)
                : Option.None<T>();
            return Result.Success<Option<T>, Error>(option);
        }
        catch (Exception ex)
        {
            return Result.Failure<Option<T>, Error>(EfExceptionMapper.Map(ex));
        }
    }

    /// <summary>
    /// Finds an entity with the given primary key values, supporting cancellation.
    /// Returns the result as an <see cref="Option{T}"/> wrapped in a <see cref="Result{T, TError}"/>.
    /// </summary>
    /// <typeparam name="T">The entity type to find.</typeparam>
    /// <param name="context">The <see cref="DbContext"/> to search in.</param>
    /// <param name="keyValues">The primary key values of the entity to find.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>
    /// A <see cref="Result{T, TError}"/> containing <see cref="Option.Some{T}"/> with the entity
    /// if found, <see cref="Option.None{T}"/> if not found, or a typed <see cref="Error"/> on failure.
    /// </returns>
    /// <example>
    /// <code>
    /// var result = await dbContext.FindResultAsync&lt;Product&gt;([42], cancellationToken);
    /// </code>
    /// </example>
    public static async Task<Result<Option<T>, Error>> FindResultAsync<T>(
        this DbContext context,
        object?[] keyValues,
        CancellationToken cancellationToken) where T : class
    {
        try
        {
            var entity = await context.Set<T>().FindAsync(keyValues, cancellationToken);
            var option = entity is not null
                ? Option.Some(entity)
                : Option.None<T>();
            return Result.Success<Option<T>, Error>(option);
        }
        catch (Exception ex)
        {
            return Result.Failure<Option<T>, Error>(EfExceptionMapper.Map(ex));
        }
    }

    /// <summary>
    /// Asynchronously returns the first element of a query as an <see cref="Option{T}"/>,
    /// wrapped in a <see cref="Result{T, TError}"/>. Returns <see cref="Option.None{T}"/>
    /// when the query returns no results.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="query">The LINQ query to execute.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>
    /// A <see cref="Result{T, TError}"/> containing <see cref="Option.Some{T}"/> with the first
    /// element if found, <see cref="Option.None{T}"/> if empty, or a typed <see cref="Error"/> on failure.
    /// </returns>
    /// <example>
    /// <code>
    /// var result = await dbContext.Set&lt;User&gt;()
    ///     .Where(u => u.Email == "alice@example.com")
    ///     .FirstOrDefaultResultAsync();
    ///
    /// var user = result.Map(opt => opt.GetValueOrDefault(User.Guest));
    /// </code>
    /// </example>
    public static async Task<Result<Option<T>, Error>> FirstOrDefaultResultAsync<T>(
        this IQueryable<T> query,
        CancellationToken cancellationToken = default) where T : class
    {
        try
        {
            var entity = await query.FirstOrDefaultAsync(cancellationToken);
            var option = entity is not null
                ? Option.Some(entity)
                : Option.None<T>();
            return Result.Success<Option<T>, Error>(option);
        }
        catch (Exception ex)
        {
            return Result.Failure<Option<T>, Error>(EfExceptionMapper.Map(ex));
        }
    }

    /// <summary>
    /// Asynchronously returns the only element of a query as an <see cref="Option{T}"/>,
    /// wrapped in a <see cref="Result{T, TError}"/>. Returns <see cref="Option.None{T}"/>
    /// when the query returns no results. Returns a failure if the query returns more than one element.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="query">The LINQ query to execute.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>
    /// A <see cref="Result{T, TError}"/> containing <see cref="Option.Some{T}"/> with the single
    /// element if found, <see cref="Option.None{T}"/> if empty, or a typed <see cref="Error"/>
    /// if the query returns multiple elements or on failure.
    /// </returns>
    /// <example>
    /// <code>
    /// var result = await dbContext.Set&lt;User&gt;()
    ///     .Where(u => u.Id == 42)
    ///     .SingleOrDefaultResultAsync();
    /// </code>
    /// </example>
    public static async Task<Result<Option<T>, Error>> SingleOrDefaultResultAsync<T>(
        this IQueryable<T> query,
        CancellationToken cancellationToken = default) where T : class
    {
        try
        {
            var entity = await query.SingleOrDefaultAsync(cancellationToken);
            var option = entity is not null
                ? Option.Some(entity)
                : Option.None<T>();
            return Result.Success<Option<T>, Error>(option);
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure<Option<T>, Error>(new EntityFrameworkError
            {
                Message = ex.Message,
                Code = "MULTIPLE_RESULTS"
            });
        }
        catch (Exception ex)
        {
            return Result.Failure<Option<T>, Error>(EfExceptionMapper.Map(ex));
        }
    }

    /// <summary>
    /// Asynchronously returns the first element of a query, wrapped in a
    /// <see cref="Result{T, TError}"/>. Returns a failure if the query is empty.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="query">The LINQ query to execute.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>
    /// A <see cref="Result{T, TError}"/> containing the first element on success,
    /// or a typed <see cref="Error"/> if the query is empty or on failure.
    /// </returns>
    /// <example>
    /// <code>
    /// var result = await dbContext.Set&lt;Order&gt;()
    ///     .OrderByDescending(o => o.CreatedAt)
    ///     .FirstResultAsync();
    /// </code>
    /// </example>
    public static async Task<Result<T, Error>> FirstResultAsync<T>(
        this IQueryable<T> query,
        CancellationToken cancellationToken = default) where T : class
    {
        try
        {
            var entity = await query.FirstAsync(cancellationToken);
            return Result.Success<T, Error>(entity);
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure<T, Error>(new EntityFrameworkError
            {
                Message = ex.Message,
                Code = "EMPTY_RESULT_SET"
            });
        }
        catch (Exception ex)
        {
            return Result.Failure<T, Error>(EfExceptionMapper.Map(ex));
        }
    }

    /// <summary>
    /// Asynchronously returns the only element of a query, wrapped in a
    /// <see cref="Result{T, TError}"/>. Returns a failure if the query is empty
    /// or contains more than one element.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="query">The LINQ query to execute.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>
    /// A <see cref="Result{T, TError}"/> containing the single element on success,
    /// or a typed <see cref="Error"/> if the query returns zero or multiple elements, or on failure.
    /// </returns>
    /// <example>
    /// <code>
    /// var result = await dbContext.Set&lt;Setting&gt;()
    ///     .Where(s => s.Key == "app.theme")
    ///     .SingleResultAsync();
    /// </code>
    /// </example>
    public static async Task<Result<T, Error>> SingleResultAsync<T>(
        this IQueryable<T> query,
        CancellationToken cancellationToken = default) where T : class
    {
        try
        {
            var entity = await query.SingleAsync(cancellationToken);
            return Result.Success<T, Error>(entity);
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure<T, Error>(new EntityFrameworkError
            {
                Message = ex.Message,
                Code = "INVALID_RESULT_SET"
            });
        }
        catch (Exception ex)
        {
            return Result.Failure<T, Error>(EfExceptionMapper.Map(ex));
        }
    }

    /// <summary>
    /// Asynchronously materializes a query to a <see cref="List{T}"/>, wrapped in a
    /// <see cref="Result{T, TError}"/>.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="query">The LINQ query to execute.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>
    /// A <see cref="Result{T, TError}"/> containing the list of results on success,
    /// or a typed <see cref="Error"/> on failure.
    /// </returns>
    /// <example>
    /// <code>
    /// var result = await dbContext.Set&lt;Product&gt;()
    ///     .Where(p => p.Category == "Electronics")
    ///     .OrderBy(p => p.Name)
    ///     .ToListResultAsync();
    ///
    /// result.Tap(products =>
    ///     logger.LogInformation("Found {Count} electronics", products.Count));
    /// </code>
    /// </example>
    public static async Task<Result<List<T>, Error>> ToListResultAsync<T>(
        this IQueryable<T> query,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var list = await query.ToListAsync(cancellationToken);
            return Result.Success<List<T>, Error>(list);
        }
        catch (Exception ex)
        {
            return Result.Failure<List<T>, Error>(EfExceptionMapper.Map(ex));
        }
    }

    /// <summary>
    /// Asynchronously returns the number of elements in a query, wrapped in a
    /// <see cref="Result{T, TError}"/>.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="query">The LINQ query to count.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>
    /// A <see cref="Result{T, TError}"/> containing the count on success,
    /// or a typed <see cref="Error"/> on failure.
    /// </returns>
    /// <example>
    /// <code>
    /// var result = await dbContext.Set&lt;Order&gt;()
    ///     .Where(o => o.Status == OrderStatus.Pending)
    ///     .CountResultAsync();
    ///
    /// var count = result.GetValueOrDefault(0);
    /// </code>
    /// </example>
    public static async Task<Result<int, Error>> CountResultAsync<T>(
        this IQueryable<T> query,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var count = await query.CountAsync(cancellationToken);
            return Result.Success<int, Error>(count);
        }
        catch (Exception ex)
        {
            return Result.Failure<int, Error>(EfExceptionMapper.Map(ex));
        }
    }

    /// <summary>
    /// Asynchronously determines whether a query contains any elements, wrapped in a
    /// <see cref="Result{T, TError}"/>.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="query">The LINQ query to check.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>
    /// A <see cref="Result{T, TError}"/> containing <c>true</c> if any elements exist,
    /// <c>false</c> otherwise, or a typed <see cref="Error"/> on failure.
    /// </returns>
    /// <example>
    /// <code>
    /// var result = await dbContext.Set&lt;User&gt;()
    ///     .Where(u => u.Email == "admin@example.com")
    ///     .AnyResultAsync();
    ///
    /// var exists = result.GetValueOrDefault(false);
    /// </code>
    /// </example>
    public static async Task<Result<bool, Error>> AnyResultAsync<T>(
        this IQueryable<T> query,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var any = await query.AnyAsync(cancellationToken);
            return Result.Success<bool, Error>(any);
        }
        catch (Exception ex)
        {
            return Result.Failure<bool, Error>(EfExceptionMapper.Map(ex));
        }
    }
}

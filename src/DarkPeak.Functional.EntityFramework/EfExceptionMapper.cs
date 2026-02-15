using System.Data.Common;
using Microsoft.EntityFrameworkCore;

namespace DarkPeak.Functional.EntityFramework;

/// <summary>
/// Internal helper that maps Entity Framework Core exceptions to typed
/// <see cref="Error"/> subtypes.
/// </summary>
/// <remarks>
/// <para>
/// Only EF Core's own exception hierarchy is used for classification:
/// <see cref="DbUpdateConcurrencyException"/> maps to <see cref="ConcurrencyError"/>,
/// <see cref="DbUpdateException"/> maps to <see cref="SaveChangesError"/>, and everything
/// else maps to <see cref="EntityFrameworkError"/>. No provider-specific classification
/// is performed.
/// </para>
/// </remarks>
internal static class EfExceptionMapper
{
    /// <summary>
    /// Maps an exception to the most specific <see cref="Error"/> subtype
    /// within the EF Core exception hierarchy.
    /// </summary>
    /// <param name="ex">The exception to classify.</param>
    /// <returns>A typed <see cref="Error"/> instance.</returns>
    internal static Error Map(Exception ex) => ex switch
    {
        // DbUpdateConcurrencyException must be checked before DbUpdateException
        // because it derives from it.
        DbUpdateConcurrencyException concurrencyEx => new ConcurrencyError
        {
            Message = concurrencyEx.Message,
            Code = "CONCURRENCY",
            ConflictingEntries = concurrencyEx.Entries
                .Select(e => e.Entity.GetType().Name)
                .ToList()
        },
        DbUpdateException updateEx => MapDbUpdateException(updateEx),
        OperationCanceledException cancelEx => new EntityFrameworkError
        {
            Message = cancelEx.Message,
            Code = "CANCELLED"
        },
        _ => new EntityFrameworkError
        {
            Message = ex.Message,
            Code = "UNKNOWN"
        }
    };

    private static SaveChangesError MapDbUpdateException(DbUpdateException ex)
    {
        var affectedEntries = ex.Entries
            .Select(e => e.Entity.GetType().Name)
            .ToList();

        // Extract SQLSTATE from the inner DbException if present — this is the only
        // provider-facing property we surface, and it comes from the base DbException class.
        var sqlState = (ex.InnerException as DbException)?.SqlState;
        var message = ex.InnerException?.Message ?? ex.Message;

        return new SaveChangesError
        {
            Message = message,
            Code = "SAVE_FAILED",
            SqlState = sqlState,
            AffectedEntries = affectedEntries
        };
    }
}

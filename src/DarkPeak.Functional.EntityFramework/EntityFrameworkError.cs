using System.Data.Common;
using Microsoft.EntityFrameworkCore;

namespace DarkPeak.Functional.EntityFramework;

/// <summary>
/// Represents a general Entity Framework Core error that occurred during a database operation.
/// Wraps exceptions in a provider-agnostic way, preserving whatever information the
/// underlying exception exposes.
/// </summary>
/// <remarks>
/// <para>
/// This is the default error type for unclassified EF Core exceptions. For exceptions that
/// EF Core itself distinguishes — <see cref="DbUpdateConcurrencyException"/> and
/// <see cref="DbUpdateException"/> — the more specific <see cref="ConcurrencyError"/> and
/// <see cref="SaveChangesError"/> subtypes are used. These distinctions are part of EF Core's
/// own API surface and are not provider-specific.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// var result = await dbContext.SaveChangesResultAsync();
/// result.TapError(error =>
/// {
///     if (error is EntityFrameworkError efError)
///         logger.LogError("EF error: {Message}", efError.Message);
/// });
/// </code>
/// </example>
public record EntityFrameworkError : Error;

/// <summary>
/// Represents an optimistic concurrency conflict, produced when EF Core throws a
/// <see cref="DbUpdateConcurrencyException"/>. This is an EF Core concept — not
/// provider-specific — and occurs when a concurrency token or row version check fails
/// during <see cref="DbContext.SaveChangesAsync(CancellationToken)"/>.
/// </summary>
/// <remarks>
/// <para>
/// The <see cref="ConflictingEntries"/> property contains the type names of the entities
/// involved in the conflict. Common resolution strategies include retrying with fresh data,
/// merging changes, or informing the user.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// var result = await dbContext.SaveChangesResultAsync();
/// result.TapError(error =>
/// {
///     if (error is ConcurrencyError concurrency)
///         logger.LogWarning("Concurrency conflict on: {Entities}",
///             string.Join(", ", concurrency.ConflictingEntries));
/// });
/// </code>
/// </example>
public sealed record ConcurrencyError : EntityFrameworkError
{
    /// <summary>
    /// Gets or sets the type names of the entities involved in the concurrency conflict.
    /// </summary>
    public IReadOnlyList<string> ConflictingEntries { get; init; } = [];
}

/// <summary>
/// Represents an error that occurred while saving entity changes to the database,
/// wrapping a <see cref="DbUpdateException"/> that is not a concurrency conflict.
/// This is an EF Core concept — <see cref="DbUpdateException"/> is thrown by EF Core
/// itself, not by the database provider directly.
/// </summary>
/// <remarks>
/// <para>
/// When the <see cref="DbUpdateException"/> has an inner <see cref="DbException"/>,
/// the <see cref="SqlState"/> property is populated from <see cref="DbException.SqlState"/>
/// for informational purposes. No provider-specific classification is performed — consumers
/// who need to distinguish constraint types can inspect <see cref="SqlState"/> in their
/// own application code where the database vendor is known.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// var result = await dbContext.SaveChangesResultAsync();
/// result.TapError(error =>
/// {
///     if (error is SaveChangesError save)
///         logger.LogError("Save failed (SQLSTATE {State}): {Message}",
///             save.SqlState, save.Message);
/// });
/// </code>
/// </example>
public sealed record SaveChangesError : EntityFrameworkError
{
    /// <summary>
    /// Gets or sets the SQLSTATE code from the inner <see cref="DbException"/>, if available.
    /// </summary>
    /// <remarks>
    /// This is populated from <see cref="DbException.SqlState"/> on the inner exception.
    /// It is <c>null</c> when the inner exception is not a <see cref="DbException"/> or
    /// when the provider does not populate SQLSTATE.
    /// </remarks>
    public string? SqlState { get; init; }

    /// <summary>
    /// Gets or sets the type names of the entities that failed to save.
    /// </summary>
    public IReadOnlyList<string> AffectedEntries { get; init; } = [];
}

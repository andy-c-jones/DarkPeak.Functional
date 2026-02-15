using System.Data.Common;

namespace DarkPeak.Functional.Dapper;

/// <summary>
/// Represents a database error that occurred during a Dapper SQL operation.
/// Wraps the information available from <see cref="DbException"/> and other
/// ADO.NET exceptions in a provider-agnostic way.
/// </summary>
/// <remarks>
/// <para>
/// This is a single, generic error type that captures whatever the underlying ADO.NET
/// provider exposes through the <see cref="DbException"/> base class — namely
/// <see cref="Exception.Message"/>, <see cref="DbException.SqlState"/>, and
/// <see cref="System.Runtime.InteropServices.ExternalException.ErrorCode"/>. No provider-specific subtyping or classification
/// is performed, keeping this package independent of any particular database vendor.
/// </para>
/// <para>
/// Consumers who need provider-specific classification (e.g. distinguishing constraint
/// violations from deadlocks) can inspect <see cref="SqlState"/> and <see cref="ErrorNumber"/>
/// in their own application code where the database vendor is known.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// var result = await connection.QueryResultAsync&lt;Order&gt;("SELECT * FROM orders");
/// result.TapError(error =>
/// {
///     if (error is DatabaseError dbError)
///         logger.LogError("SQL error (SQLSTATE {SqlState}, code {Code}): {Message}",
///             dbError.SqlState, dbError.ErrorNumber, dbError.Message);
/// });
/// </code>
/// </example>
public sealed record DatabaseError : Error
{
    /// <summary>
    /// Gets or sets the SQLSTATE code returned by the database, if available.
    /// </summary>
    /// <remarks>
    /// SQLSTATE is a five-character code defined by the SQL standard. The format and values
    /// are database-vendor specific but follow standard classes (e.g. <c>"23505"</c> for
    /// unique violation in PostgreSQL, <c>"23000"</c> in SQL Server). This value is
    /// <c>null</c> when the underlying provider does not populate it or the exception
    /// is not a <see cref="DbException"/>.
    /// </remarks>
    public string? SqlState { get; init; }

    /// <summary>
    /// Gets or sets the vendor-specific error number returned by the database, if available.
    /// </summary>
    /// <remarks>
    /// This corresponds to <see cref="System.Runtime.InteropServices.ExternalException.ErrorCode"/>. The meaning is entirely
    /// vendor-specific (e.g. SQL Server error numbers, PostgreSQL error codes).
    /// </remarks>
    public int? ErrorNumber { get; init; }
}

using System.Data.Common;

namespace DarkPeak.Functional.Dapper;

/// <summary>
/// Internal helper that maps database exceptions to <see cref="DatabaseError"/>.
/// </summary>
/// <remarks>
/// <para>
/// All <see cref="DbException"/> instances are wrapped into a single <see cref="DatabaseError"/>
/// type, preserving the <see cref="DbException.SqlState"/> and <see cref="System.Runtime.InteropServices.ExternalException.ErrorCode"/>
/// from the base class. No provider-specific classification is performed — consumers who need
/// to distinguish constraint violations, deadlocks, or other vendor-specific conditions can
/// inspect these properties in their own application code.
/// </para>
/// </remarks>
internal static class DatabaseExceptionMapper
{
    /// <summary>
    /// Maps an exception to a <see cref="DatabaseError"/>.
    /// </summary>
    /// <param name="ex">The exception to wrap.</param>
    /// <returns>A <see cref="DatabaseError"/> instance.</returns>
    internal static DatabaseError Map(Exception ex) => ex switch
    {
        DbException dbEx => new DatabaseError
        {
            Message = dbEx.Message,
            Code = "DATABASE",
            SqlState = dbEx.SqlState,
            ErrorNumber = dbEx.ErrorCode
        },
        TimeoutException timeoutEx => new DatabaseError
        {
            Message = timeoutEx.Message,
            Code = "TIMEOUT"
        },
        _ => new DatabaseError
        {
            Message = ex.Message,
            Code = "DATABASE"
        }
    };
}

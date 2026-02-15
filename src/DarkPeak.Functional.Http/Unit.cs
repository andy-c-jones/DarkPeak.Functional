namespace DarkPeak.Functional.Http;

/// <summary>
/// Represents a valueless success type for HTTP operations that do not return a response body.
/// Used as the success type parameter in <see cref="Result{T, TError}"/> when the operation
/// succeeds but produces no meaningful value (e.g. DELETE, HEAD requests).
/// </summary>
/// <remarks>
/// <para>
/// <see cref="Unit"/> serves the same purpose as <c>void</c> but can be used as a type parameter,
/// which <c>void</c> cannot. This allows DELETE and other bodyless operations to return
/// <c>Result&lt;Unit, Error&gt;</c> instead of requiring a separate non-generic Result type.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// Result&lt;Unit, Error&gt; result = await httpClient.DeleteResultAsync("/api/orders/123");
/// result.Match(
///     success: _ => Console.WriteLine("Deleted"),
///     failure: error => Console.WriteLine($"Failed: {error.Message}")
/// );
/// </code>
/// </example>
public readonly record struct Unit
{
    /// <summary>
    /// Gets the single value of the <see cref="Unit"/> type.
    /// </summary>
    public static readonly Unit Value = default;
}

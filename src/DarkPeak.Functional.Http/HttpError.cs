using System.Net;

namespace DarkPeak.Functional.Http;

/// <summary>
/// Represents an error that originated from an HTTP response.
/// Contains the status code, reason phrase, and optional response body
/// to provide full context about the failed HTTP operation.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="HttpError"/> is used by <see cref="HttpClientExtensions"/> when an HTTP response
/// indicates a non-success status code. The error is automatically mapped to the most specific
/// <see cref="Error"/> subtype based on the HTTP status code (e.g. 404 becomes <see cref="NotFoundError"/>).
/// </para>
/// <para>
/// When the status code does not map to a well-known error type, the raw <see cref="HttpError"/>
/// is returned with full details for custom handling.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// var result = await client.GetResultAsync&lt;Order&gt;("/api/orders/123");
/// result.TapError(error =>
/// {
///     if (error is HttpError httpError)
///     {
///         Console.WriteLine($"HTTP {httpError.StatusCode}: {httpError.ReasonPhrase}");
///         Console.WriteLine($"Body: {httpError.ResponseBody}");
///     }
/// });
/// </code>
/// </example>
public sealed record HttpError : Error
{
    /// <summary>
    /// Gets the HTTP status code returned by the server.
    /// </summary>
    public required HttpStatusCode StatusCode { get; init; }

    /// <summary>
    /// Gets the reason phrase from the HTTP response (e.g. "Not Found", "Internal Server Error").
    /// </summary>
    public string? ReasonPhrase { get; init; }

    /// <summary>
    /// Gets the response body content, if it was readable.
    /// This can be useful for extracting server-provided error details.
    /// </summary>
    public string? ResponseBody { get; init; }
}

/// <summary>
/// Represents an error that occurred during the HTTP request itself,
/// before a response was received. This covers network failures,
/// DNS resolution errors, timeouts, and other transport-level problems.
/// </summary>
/// <remarks>
/// <para>
/// This error type wraps exceptions thrown by <see cref="HttpClient"/> such as
/// <see cref="HttpRequestException"/> (network errors) and <see cref="TaskCanceledException"/>
/// (timeouts). The original exception message is preserved in <see cref="Error.Message"/>.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// var result = await client.GetResultAsync&lt;Order&gt;("https://unreachable-host/api/orders");
/// result.TapError(error =>
/// {
///     if (error is HttpRequestError requestError)
///     {
///         Console.WriteLine($"Request failed: {requestError.ExceptionType}");
///     }
/// });
/// </code>
/// </example>
public sealed record HttpRequestError : Error
{
    /// <summary>
    /// Gets the fully-qualified type name of the exception that caused the request failure.
    /// </summary>
    public string? ExceptionType { get; init; }
}

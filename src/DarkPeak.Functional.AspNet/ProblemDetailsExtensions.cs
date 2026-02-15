using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DarkPeak.Functional.AspNet;

/// <summary>
/// Provides extension methods for converting <see cref="Error"/> instances to
/// <see cref="ProblemDetails"/> objects following the RFC 9457 (Problem Details for HTTP APIs) standard.
/// </summary>
/// <remarks>
/// <para>
/// These extensions enable fine-grained control over error responses when you need to
/// customize the <see cref="ProblemDetails"/> before returning it, or when working with
/// MVC controllers that return <see cref="ProblemDetails"/> directly.
/// </para>
/// <para>
/// For simple cases in minimal APIs, prefer <see cref="ResultExtensions.ToIResult{T}(Result{T, Error})"/>
/// which handles the full mapping automatically.
/// </para>
/// <para>
/// <strong>Metadata handling:</strong> Any entries in <see cref="Error.Metadata"/> are added
/// to the <see cref="ProblemDetails.Extensions"/> dictionary, making them available in the
/// JSON response for client-side diagnostics.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Convert an error to ProblemDetails for custom handling:
/// var problemDetails = error.ToProblemDetails();
/// problemDetails.Instance = HttpContext.Request.Path;
/// return Results.Problem(problemDetails);
///
/// // Use in an MVC controller:
/// [HttpGet("{id}")]
/// public IActionResult GetOrder(int id)
/// {
///     var result = _service.GetOrder(id);
///     return result.Match(
///         success: Ok,
///         failure: error => new ObjectResult(error.ToProblemDetails())
///         {
///             StatusCode = error.ToProblemDetails().Status
///         }
///     );
/// }
/// </code>
/// </example>
public static class ProblemDetailsExtensions
{
    /// <summary>
    /// Converts an <see cref="Error"/> to a <see cref="ProblemDetails"/> object.
    /// The error type determines the HTTP status code and title, while the error message
    /// is used as the detail.
    /// </summary>
    /// <param name="error">The error to convert.</param>
    /// <returns>
    /// A <see cref="ProblemDetails"/> object with the appropriate status code, title, and detail.
    /// If the error has <see cref="Error.Metadata"/>, entries are added to <see cref="ProblemDetails.Extensions"/>.
    /// </returns>
    /// <example>
    /// <code>
    /// var error = new NotFoundError
    /// {
    ///     Message = "Order not found",
    ///     ResourceType = "Order",
    ///     ResourceId = "123"
    /// };
    ///
    /// var problemDetails = error.ToProblemDetails();
    /// // problemDetails.Status == 404
    /// // problemDetails.Title == "Not Found"
    /// // problemDetails.Detail == "Order not found"
    /// </code>
    /// </example>
    public static ProblemDetails ToProblemDetails(this Error error)
    {
        var (statusCode, title) = GetStatusAndTitle(error);

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = error.Message
        };

        if (error.Code is not null)
        {
            problemDetails.Extensions["errorCode"] = error.Code;
        }

        if (error.Metadata is not null)
        {
            foreach (var (key, value) in error.Metadata)
            {
                problemDetails.Extensions[key] = value;
            }
        }

        return problemDetails;
    }

    /// <summary>
    /// Converts a <see cref="ValidationError"/> to an <see cref="HttpValidationProblemDetails"/> object.
    /// This specialized conversion preserves field-level validation errors in the standard format
    /// expected by HTTP clients.
    /// </summary>
    /// <param name="error">The validation error to convert.</param>
    /// <returns>
    /// An <see cref="HttpValidationProblemDetails"/> object with status 422, validation errors,
    /// and any metadata from the error.
    /// </returns>
    /// <example>
    /// <code>
    /// var error = new ValidationError
    /// {
    ///     Message = "Validation failed",
    ///     Errors = new Dictionary&lt;string, string[]&gt;
    ///     {
    ///         ["Name"] = ["Name is required"],
    ///         ["Email"] = ["Email is not valid"]
    ///     }
    /// };
    ///
    /// var problemDetails = error.ToValidationProblemDetails();
    /// // problemDetails.Status == 422
    /// // problemDetails.Errors["Name"] == ["Name is required"]
    /// </code>
    /// </example>
    public static HttpValidationProblemDetails ToValidationProblemDetails(this ValidationError error)
    {
        var problemDetails = new HttpValidationProblemDetails(
            error.Errors ?? new Dictionary<string, string[]>())
        {
            Status = StatusCodes.Status422UnprocessableEntity,
            Title = "Validation Failed",
            Detail = error.Message
        };

        if (error.Code is not null)
        {
            problemDetails.Extensions["errorCode"] = error.Code;
        }

        if (error.Metadata is not null)
        {
            foreach (var (key, value) in error.Metadata)
            {
                problemDetails.Extensions[key] = value;
            }
        }

        return problemDetails;
    }

    /// <summary>
    /// Determines the HTTP status code and title for a given error type.
    /// </summary>
    /// <param name="error">The error to map.</param>
    /// <returns>A tuple of (HTTP status code, human-readable title).</returns>
    private static (int StatusCode, string Title) GetStatusAndTitle(Error error) => error switch
    {
        ValidationError => (StatusCodes.Status422UnprocessableEntity, "Validation Failed"),
        BadRequestError => (StatusCodes.Status400BadRequest, "Bad Request"),
        NotFoundError => (StatusCodes.Status404NotFound, "Not Found"),
        UnauthorizedError => (StatusCodes.Status401Unauthorized, "Unauthorized"),
        ForbiddenError => (StatusCodes.Status403Forbidden, "Forbidden"),
        ConflictError => (StatusCodes.Status409Conflict, "Conflict"),
        ExternalServiceError => (StatusCodes.Status502BadGateway, "Bad Gateway"),
        InternalError => (StatusCodes.Status500InternalServerError, "Internal Server Error"),
        _ => (StatusCodes.Status500InternalServerError, "Internal Server Error")
    };
}

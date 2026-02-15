using Microsoft.AspNetCore.Http;

namespace DarkPeak.Functional.AspNet;

/// <summary>
/// Provides extension methods for converting <see cref="Result{T, TError}"/> to ASP.NET
/// <see cref="IResult"/> responses for use in minimal APIs.
/// </summary>
/// <remarks>
/// <para>
/// These extensions bridge the gap between domain-level <see cref="Result{T, TError}"/> values
/// and HTTP responses, applying the correct HTTP status codes based on the error type.
/// </para>
/// <para>
/// <strong>Error type to HTTP status mapping:</strong>
/// </para>
/// <list type="table">
///   <listheader>
///     <term>Error Type</term>
///     <description>HTTP Response</description>
///   </listheader>
///   <item><term><see cref="ValidationError"/></term><description>422 Unprocessable Entity with validation problem details</description></item>
///   <item><term><see cref="BadRequestError"/></term><description>400 Bad Request with problem details</description></item>
///   <item><term><see cref="NotFoundError"/></term><description>404 Not Found with problem details</description></item>
///   <item><term><see cref="UnauthorizedError"/></term><description>401 Unauthorized</description></item>
///   <item><term><see cref="ForbiddenError"/></term><description>403 Forbidden with problem details</description></item>
///   <item><term><see cref="ConflictError"/></term><description>409 Conflict with problem details</description></item>
///   <item><term><see cref="ExternalServiceError"/></term><description>502 Bad Gateway with problem details</description></item>
///   <item><term><see cref="InternalError"/></term><description>500 Internal Server Error with problem details</description></item>
///   <item><term>Other <see cref="Error"/></term><description>500 Internal Server Error with problem details</description></item>
/// </list>
/// </remarks>
/// <example>
/// <code>
/// // In a minimal API endpoint:
/// app.MapGet("/api/orders/{id}", async (int id, OrderService service) =>
/// {
///     var result = await service.GetOrderAsync(id);
///     return result.ToIResult();
/// });
///
/// // With async Result:
/// app.MapPost("/api/orders", async (CreateOrderRequest request, OrderService service) =>
///     await service.CreateOrderAsync(request).ToIResult()
/// );
/// </code>
/// </example>
public static class ResultExtensions
{
    /// <summary>
    /// Converts a <see cref="Result{T, TError}"/> to an ASP.NET <see cref="IResult"/>.
    /// Success values are returned as <c>200 OK</c> with the value serialized as JSON.
    /// Failure values are mapped to the appropriate HTTP error response based on the error type.
    /// </summary>
    /// <typeparam name="T">The type of the success value.</typeparam>
    /// <param name="result">The result to convert.</param>
    /// <returns>
    /// An <see cref="IResult"/> with the appropriate HTTP status code and body.
    /// </returns>
    /// <example>
    /// <code>
    /// app.MapGet("/api/products/{id}", async (int id, ProductService service) =>
    /// {
    ///     Result&lt;Product, Error&gt; result = await service.GetProductAsync(id);
    ///     return result.ToIResult();
    ///     // Success: 200 OK { "id": 1, "name": "Widget" }
    ///     // NotFoundError: 404 Not Found (ProblemDetails)
    ///     // ValidationError: 422 Unprocessable Entity (ValidationProblemDetails)
    /// });
    /// </code>
    /// </example>
    public static IResult ToIResult<T>(this Result<T, Error> result) =>
        result.Match(
            success: value => Results.Ok(value),
            failure: MapErrorToResult
        );

    /// <summary>
    /// Converts an asynchronous <see cref="Result{T, TError}"/> to an ASP.NET <see cref="IResult"/>.
    /// This is a convenience method that awaits the task and then applies the same mapping
    /// as <see cref="ToIResult{T}(Result{T, Error})"/>.
    /// </summary>
    /// <typeparam name="T">The type of the success value.</typeparam>
    /// <param name="resultTask">The task producing the result to convert.</param>
    /// <returns>
    /// A task containing an <see cref="IResult"/> with the appropriate HTTP status code and body.
    /// </returns>
    /// <example>
    /// <code>
    /// app.MapPost("/api/orders", async (CreateOrderRequest request, OrderService service) =>
    ///     await service.CreateOrderAsync(request).ToIResult()
    /// );
    /// </code>
    /// </example>
    public static async Task<IResult> ToIResult<T>(this Task<Result<T, Error>> resultTask) =>
        (await resultTask).ToIResult();

    /// <summary>
    /// Converts a <see cref="Result{T, TError}"/> to an ASP.NET <see cref="IResult"/>,
    /// using <c>201 Created</c> with a location header for successful results.
    /// Failure values are mapped identically to <see cref="ToIResult{T}(Result{T, Error})"/>.
    /// </summary>
    /// <typeparam name="T">The type of the success value.</typeparam>
    /// <param name="result">The result to convert.</param>
    /// <param name="locationUri">
    /// The URI of the newly created resource, used for the <c>Location</c> response header.
    /// </param>
    /// <returns>
    /// An <see cref="IResult"/> with <c>201 Created</c> on success or the appropriate error response.
    /// </returns>
    /// <example>
    /// <code>
    /// app.MapPost("/api/orders", async (CreateOrderRequest request, OrderService service) =>
    /// {
    ///     var result = await service.CreateOrderAsync(request);
    ///     return result.ToCreatedResult(order => $"/api/orders/{order.Id}");
    /// });
    /// </code>
    /// </example>
    public static IResult ToCreatedResult<T>(
        this Result<T, Error> result,
        Func<T, string> locationUri) =>
        result.Match(
            success: value => Results.Created(locationUri(value), value),
            failure: MapErrorToResult
        );

    /// <summary>
    /// Converts an asynchronous <see cref="Result{T, TError}"/> to an ASP.NET <see cref="IResult"/>,
    /// using <c>201 Created</c> with a location header for successful results.
    /// </summary>
    /// <typeparam name="T">The type of the success value.</typeparam>
    /// <param name="resultTask">The task producing the result to convert.</param>
    /// <param name="locationUri">
    /// The URI of the newly created resource, used for the <c>Location</c> response header.
    /// </param>
    /// <returns>
    /// A task containing an <see cref="IResult"/> with <c>201 Created</c> on success or the appropriate error response.
    /// </returns>
    public static async Task<IResult> ToCreatedResult<T>(
        this Task<Result<T, Error>> resultTask,
        Func<T, string> locationUri) =>
        (await resultTask).ToCreatedResult(locationUri);

    /// <summary>
    /// Converts a <see cref="Result{T, TError}"/> to an ASP.NET <see cref="IResult"/>,
    /// returning <c>204 No Content</c> for successful results.
    /// Failure values are mapped identically to <see cref="ToIResult{T}(Result{T, Error})"/>.
    /// </summary>
    /// <typeparam name="T">The type of the success value (ignored in the response).</typeparam>
    /// <param name="result">The result to convert.</param>
    /// <returns>
    /// An <see cref="IResult"/> with <c>204 No Content</c> on success or the appropriate error response.
    /// </returns>
    /// <example>
    /// <code>
    /// app.MapDelete("/api/orders/{id}", async (int id, OrderService service) =>
    /// {
    ///     var result = await service.DeleteOrderAsync(id);
    ///     return result.ToNoContentResult();
    /// });
    /// </code>
    /// </example>
    public static IResult ToNoContentResult<T>(this Result<T, Error> result) =>
        result.Match(
            success: _ => Results.NoContent(),
            failure: MapErrorToResult
        );

    /// <summary>
    /// Converts an asynchronous <see cref="Result{T, TError}"/> to an ASP.NET <see cref="IResult"/>,
    /// returning <c>204 No Content</c> for successful results.
    /// </summary>
    /// <typeparam name="T">The type of the success value (ignored in the response).</typeparam>
    /// <param name="resultTask">The task producing the result to convert.</param>
    /// <returns>
    /// A task containing an <see cref="IResult"/> with <c>204 No Content</c> on success or the appropriate error response.
    /// </returns>
    public static async Task<IResult> ToNoContentResult<T>(this Task<Result<T, Error>> resultTask) =>
        (await resultTask).ToNoContentResult();

    /// <summary>
    /// Maps an <see cref="Error"/> to the appropriate ASP.NET <see cref="IResult"/> response.
    /// Uses pattern matching to select the most specific HTTP status code for each error type.
    /// </summary>
    /// <param name="error">The error to map.</param>
    /// <returns>An <see cref="IResult"/> with the appropriate HTTP status code and problem details body.</returns>
    private static IResult MapErrorToResult(Error error) => error switch
    {
        ValidationError validationError => Results.ValidationProblem(
            errors: validationError.Errors ?? new Dictionary<string, string[]>(),
            detail: validationError.Message,
            title: "Validation Failed",
            statusCode: StatusCodes.Status422UnprocessableEntity),

        BadRequestError badRequest => Results.Problem(
            detail: badRequest.Message,
            title: "Bad Request",
            statusCode: StatusCodes.Status400BadRequest),

        NotFoundError notFound => Results.Problem(
            detail: notFound.Message,
            title: "Not Found",
            statusCode: StatusCodes.Status404NotFound),

        UnauthorizedError => Results.Unauthorized(),

        ForbiddenError forbidden => Results.Problem(
            detail: forbidden.Message,
            title: "Forbidden",
            statusCode: StatusCodes.Status403Forbidden),

        ConflictError conflict => Results.Problem(
            detail: conflict.Message,
            title: "Conflict",
            statusCode: StatusCodes.Status409Conflict),

        ExternalServiceError external => Results.Problem(
            detail: external.Message,
            title: "Bad Gateway",
            statusCode: StatusCodes.Status502BadGateway),

        InternalError internal_ => Results.Problem(
            detail: internal_.Message,
            title: "Internal Server Error",
            statusCode: StatusCodes.Status500InternalServerError),

        _ => Results.Problem(
            detail: error.Message,
            title: "Internal Server Error",
            statusCode: StatusCodes.Status500InternalServerError)
    };
}

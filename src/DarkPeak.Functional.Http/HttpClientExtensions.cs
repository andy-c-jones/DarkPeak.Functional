using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace DarkPeak.Functional.Http;

/// <summary>
/// Provides extension methods for <see cref="HttpClient"/> that wrap HTTP operations
/// in <see cref="Result{T, TError}"/>, enabling railway-oriented programming for HTTP communication.
/// </summary>
/// <remarks>
/// <para>
/// These extensions eliminate the need for try/catch blocks around HTTP calls by capturing
/// both HTTP error responses and transport-level exceptions as typed <see cref="Error"/> values.
/// </para>
/// <para>
/// <strong>HTTP status code mapping:</strong> Non-success responses are mapped to the most
/// specific <see cref="Error"/> subtype based on the status code:
/// </para>
/// <list type="table">
///   <listheader>
///     <term>Status Code</term>
///     <description>Error Type</description>
///   </listheader>
///   <item><term>400 Bad Request</term><description><see cref="BadRequestError"/></description></item>
///   <item><term>401 Unauthorized</term><description><see cref="UnauthorizedError"/></description></item>
///   <item><term>403 Forbidden</term><description><see cref="ForbiddenError"/></description></item>
///   <item><term>404 Not Found</term><description><see cref="NotFoundError"/></description></item>
///   <item><term>409 Conflict</term><description><see cref="ConflictError"/></description></item>
///   <item><term>422 Unprocessable Entity</term><description><see cref="ValidationError"/></description></item>
///   <item><term>5xx Server Error</term><description><see cref="ExternalServiceError"/></description></item>
///   <item><term>Other</term><description><see cref="HttpError"/></description></item>
/// </list>
/// <para>
/// <strong>Transport errors:</strong> Network failures, DNS errors, and timeouts are captured
/// as <see cref="HttpRequestError"/> with the original exception details preserved.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Simple GET request
/// var result = await httpClient.GetResultAsync&lt;Order&gt;("/api/orders/123");
/// var message = result.Match(
///     success: order => $"Order {order.Id} totals {order.Total:C}",
///     failure: error => $"Failed: {error.Message}"
/// );
///
/// // POST with chaining
/// var created = await httpClient
///     .PostResultAsync&lt;Order&gt;("/api/orders", newOrder)
///     .Map(order => order.Id);
///
/// // Custom request with SendResultAsync
/// var request = new HttpRequestMessage(HttpMethod.Patch, "/api/orders/123");
/// request.Content = JsonContent.Create(patch);
/// var patched = await httpClient.SendResultAsync&lt;Order&gt;(request);
/// </code>
/// </example>
public static class HttpClientExtensions
{
    /// <summary>
    /// Sends a GET request to the specified URI and deserializes the JSON response body
    /// as the specified type, returning the result wrapped in a <see cref="Result{T, TError}"/>.
    /// </summary>
    /// <typeparam name="T">The type to deserialize the response body to.</typeparam>
    /// <param name="client">The <see cref="HttpClient"/> to send the request with.</param>
    /// <param name="requestUri">The URI the request is sent to.</param>
    /// <param name="options">Optional <see cref="JsonSerializerOptions"/> for deserialization.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>
    /// A <see cref="Result{T, TError}"/> containing either the deserialized response body on success,
    /// or a typed <see cref="Error"/> describing what went wrong.
    /// </returns>
    /// <example>
    /// <code>
    /// var result = await httpClient.GetResultAsync&lt;WeatherForecast&gt;("/api/weather");
    /// result.Match(
    ///     success: forecast => Console.WriteLine($"Temperature: {forecast.TemperatureC}C"),
    ///     failure: error => Console.WriteLine($"Error: {error.Message}")
    /// );
    /// </code>
    /// </example>
    public static async Task<Result<T, Error>> GetResultAsync<T>(
        this HttpClient client,
        string requestUri,
        JsonSerializerOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await client.GetAsync(requestUri, cancellationToken);
            return await HandleResponse<T>(response, options, cancellationToken);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            return CreateRequestError(ex);
        }
    }

    /// <summary>
    /// Sends a POST request with a JSON-serialized body to the specified URI and deserializes
    /// the JSON response body as the specified type, returning the result wrapped in a <see cref="Result{T, TError}"/>.
    /// </summary>
    /// <typeparam name="T">The type to deserialize the response body to.</typeparam>
    /// <param name="client">The <see cref="HttpClient"/> to send the request with.</param>
    /// <param name="requestUri">The URI the request is sent to.</param>
    /// <param name="content">The object to serialize as JSON for the request body.</param>
    /// <param name="options">Optional <see cref="JsonSerializerOptions"/> for serialization and deserialization.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>
    /// A <see cref="Result{T, TError}"/> containing either the deserialized response body on success,
    /// or a typed <see cref="Error"/> describing what went wrong.
    /// </returns>
    /// <example>
    /// <code>
    /// var newOrder = new CreateOrderRequest { ProductId = "ABC", Quantity = 2 };
    /// var result = await httpClient.PostResultAsync&lt;Order&gt;("/api/orders", newOrder);
    /// </code>
    /// </example>
    public static async Task<Result<T, Error>> PostResultAsync<T>(
        this HttpClient client,
        string requestUri,
        object content,
        JsonSerializerOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await client.PostAsJsonAsync(requestUri, content, options ?? JsonSerializerOptions.Default, cancellationToken);
            return await HandleResponse<T>(response, options, cancellationToken);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            return CreateRequestError(ex);
        }
    }

    /// <summary>
    /// Sends a PUT request with a JSON-serialized body to the specified URI and deserializes
    /// the JSON response body as the specified type, returning the result wrapped in a <see cref="Result{T, TError}"/>.
    /// </summary>
    /// <typeparam name="T">The type to deserialize the response body to.</typeparam>
    /// <param name="client">The <see cref="HttpClient"/> to send the request with.</param>
    /// <param name="requestUri">The URI the request is sent to.</param>
    /// <param name="content">The object to serialize as JSON for the request body.</param>
    /// <param name="options">Optional <see cref="JsonSerializerOptions"/> for serialization and deserialization.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>
    /// A <see cref="Result{T, TError}"/> containing either the deserialized response body on success,
    /// or a typed <see cref="Error"/> describing what went wrong.
    /// </returns>
    /// <example>
    /// <code>
    /// var updated = new UpdateOrderRequest { Quantity = 5 };
    /// var result = await httpClient.PutResultAsync&lt;Order&gt;("/api/orders/123", updated);
    /// </code>
    /// </example>
    public static async Task<Result<T, Error>> PutResultAsync<T>(
        this HttpClient client,
        string requestUri,
        object content,
        JsonSerializerOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await client.PutAsJsonAsync(requestUri, content, options ?? JsonSerializerOptions.Default, cancellationToken);
            return await HandleResponse<T>(response, options, cancellationToken);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            return CreateRequestError(ex);
        }
    }

    /// <summary>
    /// Sends a PATCH request with a JSON-serialized body to the specified URI and deserializes
    /// the JSON response body as the specified type, returning the result wrapped in a <see cref="Result{T, TError}"/>.
    /// </summary>
    /// <typeparam name="T">The type to deserialize the response body to.</typeparam>
    /// <param name="client">The <see cref="HttpClient"/> to send the request with.</param>
    /// <param name="requestUri">The URI the request is sent to.</param>
    /// <param name="content">The object to serialize as JSON for the request body.</param>
    /// <param name="options">Optional <see cref="JsonSerializerOptions"/> for serialization and deserialization.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>
    /// A <see cref="Result{T, TError}"/> containing either the deserialized response body on success,
    /// or a typed <see cref="Error"/> describing what went wrong.
    /// </returns>
    /// <example>
    /// <code>
    /// var patch = new { Status = "Shipped" };
    /// var result = await httpClient.PatchResultAsync&lt;Order&gt;("/api/orders/123", patch);
    /// </code>
    /// </example>
    public static async Task<Result<T, Error>> PatchResultAsync<T>(
        this HttpClient client,
        string requestUri,
        object content,
        JsonSerializerOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var jsonContent = JsonContent.Create(content, options: options);
            var response = await client.PatchAsync(requestUri, jsonContent, cancellationToken);
            return await HandleResponse<T>(response, options, cancellationToken);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            return CreateRequestError(ex);
        }
    }

    /// <summary>
    /// Sends a DELETE request to the specified URI, returning a <see cref="Result{T, TError}"/>
    /// that indicates success or failure without a response body.
    /// </summary>
    /// <param name="client">The <see cref="HttpClient"/> to send the request with.</param>
    /// <param name="requestUri">The URI the request is sent to.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>
    /// A <see cref="Result{T, TError}"/> containing <see cref="Unit"/> on success,
    /// or a typed <see cref="Error"/> describing what went wrong.
    /// </returns>
    /// <example>
    /// <code>
    /// var result = await httpClient.DeleteResultAsync("/api/orders/123");
    /// result.Match(
    ///     success: _ => Console.WriteLine("Deleted successfully"),
    ///     failure: error => Console.WriteLine($"Delete failed: {error.Message}")
    /// );
    /// </code>
    /// </example>
    public static async Task<Result<Unit, Error>> DeleteResultAsync(
        this HttpClient client,
        string requestUri,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await client.DeleteAsync(requestUri, cancellationToken);
            return await HandleResponseNoBody(response, cancellationToken);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            return CreateRequestError(ex);
        }
    }

    /// <summary>
    /// Sends a DELETE request to the specified URI and deserializes the JSON response body
    /// as the specified type, returning the result wrapped in a <see cref="Result{T, TError}"/>.
    /// </summary>
    /// <typeparam name="T">The type to deserialize the response body to.</typeparam>
    /// <param name="client">The <see cref="HttpClient"/> to send the request with.</param>
    /// <param name="requestUri">The URI the request is sent to.</param>
    /// <param name="options">Optional <see cref="JsonSerializerOptions"/> for deserialization.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>
    /// A <see cref="Result{T, TError}"/> containing either the deserialized response body on success,
    /// or a typed <see cref="Error"/> describing what went wrong.
    /// </returns>
    /// <example>
    /// <code>
    /// var result = await httpClient.DeleteResultAsync&lt;DeletionConfirmation&gt;("/api/orders/123");
    /// </code>
    /// </example>
    public static async Task<Result<T, Error>> DeleteResultAsync<T>(
        this HttpClient client,
        string requestUri,
        JsonSerializerOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await client.DeleteAsync(requestUri, cancellationToken);
            return await HandleResponse<T>(response, options, cancellationToken);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            return CreateRequestError(ex);
        }
    }

    /// <summary>
    /// Sends an arbitrary <see cref="HttpRequestMessage"/> and deserializes the JSON response body
    /// as the specified type, returning the result wrapped in a <see cref="Result{T, TError}"/>.
    /// Use this for custom HTTP methods, headers, or request configurations not covered by the
    /// convenience methods.
    /// </summary>
    /// <typeparam name="T">The type to deserialize the response body to.</typeparam>
    /// <param name="client">The <see cref="HttpClient"/> to send the request with.</param>
    /// <param name="request">The <see cref="HttpRequestMessage"/> to send.</param>
    /// <param name="options">Optional <see cref="JsonSerializerOptions"/> for deserialization.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>
    /// A <see cref="Result{T, TError}"/> containing either the deserialized response body on success,
    /// or a typed <see cref="Error"/> describing what went wrong.
    /// </returns>
    /// <example>
    /// <code>
    /// var request = new HttpRequestMessage(HttpMethod.Options, "/api/orders");
    /// request.Headers.Add("X-Custom-Header", "value");
    /// var result = await httpClient.SendResultAsync&lt;OptionsResponse&gt;(request);
    /// </code>
    /// </example>
    public static async Task<Result<T, Error>> SendResultAsync<T>(
        this HttpClient client,
        HttpRequestMessage request,
        JsonSerializerOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await client.SendAsync(request, cancellationToken);
            return await HandleResponse<T>(response, options, cancellationToken);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            return CreateRequestError(ex);
        }
    }

    /// <summary>
    /// Sends an arbitrary <see cref="HttpRequestMessage"/> without expecting a response body,
    /// returning a <see cref="Result{T, TError}"/> that indicates success or failure.
    /// </summary>
    /// <param name="client">The <see cref="HttpClient"/> to send the request with.</param>
    /// <param name="request">The <see cref="HttpRequestMessage"/> to send.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>
    /// A <see cref="Result{T, TError}"/> containing <see cref="Unit"/> on success,
    /// or a typed <see cref="Error"/> describing what went wrong.
    /// </returns>
    /// <example>
    /// <code>
    /// var request = new HttpRequestMessage(HttpMethod.Head, "/api/health");
    /// var result = await httpClient.SendResultAsync(request);
    /// </code>
    /// </example>
    public static async Task<Result<Unit, Error>> SendResultAsync(
        this HttpClient client,
        HttpRequestMessage request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await client.SendAsync(request, cancellationToken);
            return await HandleResponseNoBody(response, cancellationToken);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            return CreateRequestError(ex);
        }
    }

    /// <summary>
    /// Processes an <see cref="HttpResponseMessage"/> and deserializes the body on success,
    /// or maps the status code to a typed error on failure.
    /// </summary>
    private static async Task<Result<T, Error>> HandleResponse<T>(
        HttpResponseMessage response,
        JsonSerializerOptions? options,
        CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
        {
            var value = await response.Content.ReadFromJsonAsync<T>(options ?? JsonSerializerOptions.Default, cancellationToken);
            return value is not null
                ? Result.Success<T, Error>(value)
                : Result.Failure<T, Error>(new HttpError
                {
                    Message = "Response body deserialized to null.",
                    StatusCode = response.StatusCode,
                    ReasonPhrase = response.ReasonPhrase
                });
        }

        return await CreateResponseError<T>(response, cancellationToken);
    }

    /// <summary>
    /// Processes an <see cref="HttpResponseMessage"/> for operations that do not return a body.
    /// Maps success to <see cref="Unit"/> or the status code to a typed error on failure.
    /// </summary>
    private static async Task<Result<Unit, Error>> HandleResponseNoBody(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
        {
            return Result.Success<Unit, Error>(Unit.Value);
        }

        return await CreateResponseError<Unit>(response, cancellationToken);
    }

    /// <summary>
    /// Maps an HTTP error response to the most specific <see cref="Error"/> subtype
    /// based on the status code.
    /// </summary>
    private static async Task<Result<T, Error>> CreateResponseError<T>(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        var body = await ReadResponseBody(response, cancellationToken);
        var statusCode = response.StatusCode;
        var reasonPhrase = response.ReasonPhrase ?? statusCode.ToString();

        Error error = statusCode switch
        {
            HttpStatusCode.BadRequest => new BadRequestError
            {
                Message = body ?? reasonPhrase,
                Code = ((int)statusCode).ToString()
            },
            HttpStatusCode.Unauthorized => new UnauthorizedError
            {
                Message = body ?? reasonPhrase,
                Code = ((int)statusCode).ToString()
            },
            HttpStatusCode.Forbidden => new ForbiddenError
            {
                Message = body ?? reasonPhrase,
                Code = ((int)statusCode).ToString()
            },
            HttpStatusCode.NotFound => new NotFoundError
            {
                Message = body ?? reasonPhrase,
                Code = ((int)statusCode).ToString()
            },
            HttpStatusCode.Conflict => new ConflictError
            {
                Message = body ?? reasonPhrase,
                Code = ((int)statusCode).ToString()
            },
            HttpStatusCode.UnprocessableEntity => new ValidationError
            {
                Message = body ?? reasonPhrase,
                Code = ((int)statusCode).ToString()
            },
            _ when (int)statusCode >= 500 => new ExternalServiceError
            {
                Message = body ?? reasonPhrase,
                Code = ((int)statusCode).ToString(),
                InnerMessage = body
            },
            _ => new HttpError
            {
                Message = body ?? reasonPhrase,
                StatusCode = statusCode,
                ReasonPhrase = reasonPhrase,
                ResponseBody = body,
                Code = ((int)statusCode).ToString()
            }
        };

        return Result.Failure<T, Error>(error);
    }

    /// <summary>
    /// Creates an <see cref="HttpRequestError"/> from a transport-level exception.
    /// </summary>
    private static HttpRequestError CreateRequestError(Exception ex) =>
        new()
        {
            Message = ex.Message,
            ExceptionType = ex.GetType().FullName
        };

    /// <summary>
    /// Safely reads the response body as a string, returning null if reading fails.
    /// </summary>
    private static async Task<string?> ReadResponseBody(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        try
        {
            return await response.Content.ReadAsStringAsync(cancellationToken);
        }
        catch
        {
            return null;
        }
    }
}

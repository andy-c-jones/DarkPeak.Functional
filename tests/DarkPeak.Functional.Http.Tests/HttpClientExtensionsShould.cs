using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using DarkPeak.Functional.Http;
using TUnit.Assertions;
using TUnit.Assertions.Extensions;
using TUnit.Core;

namespace DarkPeak.Functional.Http.Tests;

/// <summary>
/// Tests for <see cref="HttpClientExtensions"/> covering all HTTP methods,
/// status code mappings, transport errors, and edge cases.
/// </summary>
public class HttpClientExtensionsShould
{
    #region Test Helpers

    private record TestPayload(int Id, string Name);

    private static HttpClient CreateClient(HttpStatusCode statusCode, string? content = null,
        string contentType = "application/json")
    {
        var handler = new FakeHttpMessageHandler(statusCode, content, contentType);
        return new HttpClient(handler) { BaseAddress = new Uri("https://api.example.com") };
    }

    private static HttpClient CreateThrowingClient(Exception exception)
    {
        var handler = new ThrowingHttpMessageHandler(exception);
        return new HttpClient(handler) { BaseAddress = new Uri("https://api.example.com") };
    }

    #endregion

    #region GetResultAsync Tests

    [Test]
    public async Task GetResultAsync_returns_success_on_200()
    {
        var json = """{"Id":1,"Name":"Widget"}""";
        using var client = CreateClient(HttpStatusCode.OK, json);

        var result = await client.GetResultAsync<TestPayload>("/api/items/1");

        await Assert.That(result.IsSuccess).IsTrue();
        var value = result.Match(v => v, _ => null!);
        await Assert.That(value.Id).IsEqualTo(1);
        await Assert.That(value.Name).IsEqualTo("Widget");
    }

    [Test]
    public async Task GetResultAsync_returns_failure_on_null_deserialization()
    {
        using var client = CreateClient(HttpStatusCode.OK, "null");

        var result = await client.GetResultAsync<TestPayload>("/api/items/1");

        await Assert.That(result.IsFailure).IsTrue();
        var error = result.Match(_ => null!, e => e);
        await Assert.That(error).IsAssignableTo<HttpError>();
        await Assert.That(error.Message).Contains("null");
    }

    [Test]
    public async Task GetResultAsync_returns_NotFoundError_on_404()
    {
        using var client = CreateClient(HttpStatusCode.NotFound, "Resource not found");

        var result = await client.GetResultAsync<TestPayload>("/api/items/999");

        await Assert.That(result.IsFailure).IsTrue();
        var error = result.Match(_ => null!, e => e);
        await Assert.That(error).IsAssignableTo<NotFoundError>();
        await Assert.That(error.Message).Contains("Resource not found");
    }

    [Test]
    public async Task GetResultAsync_returns_UnauthorizedError_on_401()
    {
        using var client = CreateClient(HttpStatusCode.Unauthorized, "Invalid token");

        var result = await client.GetResultAsync<TestPayload>("/api/items/1");

        await Assert.That(result.IsFailure).IsTrue();
        var error = result.Match(_ => null!, e => e);
        await Assert.That(error).IsAssignableTo<UnauthorizedError>();
    }

    [Test]
    public async Task GetResultAsync_returns_ForbiddenError_on_403()
    {
        using var client = CreateClient(HttpStatusCode.Forbidden, "Access denied");

        var result = await client.GetResultAsync<TestPayload>("/api/items/1");

        await Assert.That(result.IsFailure).IsTrue();
        var error = result.Match(_ => null!, e => e);
        await Assert.That(error).IsAssignableTo<ForbiddenError>();
    }

    [Test]
    public async Task GetResultAsync_returns_BadRequestError_on_400()
    {
        using var client = CreateClient(HttpStatusCode.BadRequest, "Invalid parameters");

        var result = await client.GetResultAsync<TestPayload>("/api/items");

        await Assert.That(result.IsFailure).IsTrue();
        var error = result.Match(_ => null!, e => e);
        await Assert.That(error).IsAssignableTo<BadRequestError>();
    }

    [Test]
    public async Task GetResultAsync_returns_ConflictError_on_409()
    {
        using var client = CreateClient(HttpStatusCode.Conflict, "Resource already exists");

        var result = await client.GetResultAsync<TestPayload>("/api/items/1");

        await Assert.That(result.IsFailure).IsTrue();
        var error = result.Match(_ => null!, e => e);
        await Assert.That(error).IsAssignableTo<ConflictError>();
    }

    [Test]
    public async Task GetResultAsync_returns_ValidationError_on_422()
    {
        using var client = CreateClient(HttpStatusCode.UnprocessableEntity, "Validation failed");

        var result = await client.GetResultAsync<TestPayload>("/api/items");

        await Assert.That(result.IsFailure).IsTrue();
        var error = result.Match(_ => null!, e => e);
        await Assert.That(error).IsAssignableTo<ValidationError>();
    }

    [Test]
    public async Task GetResultAsync_returns_ExternalServiceError_on_500()
    {
        using var client = CreateClient(HttpStatusCode.InternalServerError, "Server error");

        var result = await client.GetResultAsync<TestPayload>("/api/items/1");

        await Assert.That(result.IsFailure).IsTrue();
        var error = result.Match(_ => null!, e => e);
        await Assert.That(error).IsAssignableTo<ExternalServiceError>();
    }

    [Test]
    public async Task GetResultAsync_returns_ExternalServiceError_on_503()
    {
        using var client = CreateClient(HttpStatusCode.ServiceUnavailable, "Service unavailable");

        var result = await client.GetResultAsync<TestPayload>("/api/items/1");

        await Assert.That(result.IsFailure).IsTrue();
        var error = result.Match(_ => null!, e => e);
        await Assert.That(error).IsAssignableTo<ExternalServiceError>();
    }

    [Test]
    public async Task GetResultAsync_returns_HttpError_on_unmapped_status()
    {
        using var client = CreateClient(HttpStatusCode.Gone, "Resource gone");

        var result = await client.GetResultAsync<TestPayload>("/api/items/1");

        await Assert.That(result.IsFailure).IsTrue();
        var error = result.Match(_ => null!, e => e);
        await Assert.That(error).IsAssignableTo<HttpError>();
        var httpError = (HttpError)error;
        await Assert.That(httpError.StatusCode).IsEqualTo(HttpStatusCode.Gone);
        await Assert.That(httpError.ResponseBody).Contains("Resource gone");
    }

    [Test]
    public async Task GetResultAsync_returns_HttpRequestError_on_network_failure()
    {
        using var client = CreateThrowingClient(new HttpRequestException("Connection refused"));

        var result = await client.GetResultAsync<TestPayload>("/api/items/1");

        await Assert.That(result.IsFailure).IsTrue();
        var error = result.Match(_ => null!, e => e);
        await Assert.That(error).IsAssignableTo<HttpRequestError>();
        await Assert.That(error.Message).Contains("Connection refused");
    }

    [Test]
    public async Task GetResultAsync_returns_HttpRequestError_on_timeout()
    {
        using var client = CreateThrowingClient(new TaskCanceledException("Request timed out"));

        var result = await client.GetResultAsync<TestPayload>("/api/items/1");

        await Assert.That(result.IsFailure).IsTrue();
        var error = result.Match(_ => null!, e => e);
        await Assert.That(error).IsAssignableTo<HttpRequestError>();
        await Assert.That(error.Message).Contains("timed out");
    }

    [Test]
    public async Task GetResultAsync_uses_custom_json_options()
    {
        var json = """{"id":1,"name":"Widget"}""";
        using var client = CreateClient(HttpStatusCode.OK, json);
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        var result = await client.GetResultAsync<TestPayload>("/api/items/1", options);

        await Assert.That(result.IsSuccess).IsTrue();
        var value = result.Match(v => v, _ => null!);
        await Assert.That(value.Name).IsEqualTo("Widget");
    }

    #endregion

    #region PostResultAsync Tests

    [Test]
    public async Task PostResultAsync_returns_success_on_201()
    {
        var json = """{"Id":1,"Name":"Created"}""";
        using var client = CreateClient(HttpStatusCode.Created, json);
        var payload = new TestPayload(0, "New");

        var result = await client.PostResultAsync<TestPayload>("/api/items", payload);

        await Assert.That(result.IsSuccess).IsTrue();
        var value = result.Match(v => v, _ => null!);
        await Assert.That(value.Id).IsEqualTo(1);
    }

    [Test]
    public async Task PostResultAsync_returns_failure_on_400()
    {
        using var client = CreateClient(HttpStatusCode.BadRequest, "Invalid payload");
        var payload = new TestPayload(0, "Bad");

        var result = await client.PostResultAsync<TestPayload>("/api/items", payload);

        await Assert.That(result.IsFailure).IsTrue();
        var error = result.Match(_ => null!, e => e);
        await Assert.That(error).IsAssignableTo<BadRequestError>();
    }

    [Test]
    public async Task PostResultAsync_returns_HttpRequestError_on_network_failure()
    {
        using var client = CreateThrowingClient(new HttpRequestException("DNS resolution failed"));

        var result = await client.PostResultAsync<TestPayload>("/api/items", new TestPayload(0, "X"));

        await Assert.That(result.IsFailure).IsTrue();
        var error = result.Match(_ => null!, e => e);
        await Assert.That(error).IsAssignableTo<HttpRequestError>();
    }

    #endregion

    #region PutResultAsync Tests

    [Test]
    public async Task PutResultAsync_returns_success_on_200()
    {
        var json = """{"Id":1,"Name":"Updated"}""";
        using var client = CreateClient(HttpStatusCode.OK, json);
        var payload = new TestPayload(1, "Updated");

        var result = await client.PutResultAsync<TestPayload>("/api/items/1", payload);

        await Assert.That(result.IsSuccess).IsTrue();
        var value = result.Match(v => v, _ => null!);
        await Assert.That(value.Name).IsEqualTo("Updated");
    }

    [Test]
    public async Task PutResultAsync_returns_failure_on_404()
    {
        using var client = CreateClient(HttpStatusCode.NotFound, "Item not found");

        var result = await client.PutResultAsync<TestPayload>("/api/items/999", new TestPayload(999, "X"));

        await Assert.That(result.IsFailure).IsTrue();
        var error = result.Match(_ => null!, e => e);
        await Assert.That(error).IsAssignableTo<NotFoundError>();
    }

    [Test]
    public async Task PutResultAsync_returns_HttpRequestError_on_network_failure()
    {
        using var client = CreateThrowingClient(new HttpRequestException("Connection reset"));

        var result = await client.PutResultAsync<TestPayload>("/api/items/1", new TestPayload(1, "X"));

        await Assert.That(result.IsFailure).IsTrue();
        var error = result.Match(_ => null!, e => e);
        await Assert.That(error).IsAssignableTo<HttpRequestError>();
    }

    #endregion

    #region PatchResultAsync Tests

    [Test]
    public async Task PatchResultAsync_returns_success_on_200()
    {
        var json = """{"Id":1,"Name":"Patched"}""";
        using var client = CreateClient(HttpStatusCode.OK, json);

        var result = await client.PatchResultAsync<TestPayload>("/api/items/1", new { Name = "Patched" });

        await Assert.That(result.IsSuccess).IsTrue();
        var value = result.Match(v => v, _ => null!);
        await Assert.That(value.Name).IsEqualTo("Patched");
    }

    [Test]
    public async Task PatchResultAsync_returns_failure_on_422()
    {
        using var client = CreateClient(HttpStatusCode.UnprocessableEntity, "Invalid patch");

        var result = await client.PatchResultAsync<TestPayload>("/api/items/1", new { Name = "" });

        await Assert.That(result.IsFailure).IsTrue();
        var error = result.Match(_ => null!, e => e);
        await Assert.That(error).IsAssignableTo<ValidationError>();
    }

    [Test]
    public async Task PatchResultAsync_returns_HttpRequestError_on_network_failure()
    {
        using var client = CreateThrowingClient(new HttpRequestException("Timeout"));

        var result = await client.PatchResultAsync<TestPayload>("/api/items/1", new { Name = "X" });

        await Assert.That(result.IsFailure).IsTrue();
        var error = result.Match(_ => null!, e => e);
        await Assert.That(error).IsAssignableTo<HttpRequestError>();
    }

    #endregion

    #region DeleteResultAsync Tests

    [Test]
    public async Task DeleteResultAsync_returns_success_on_204()
    {
        using var client = CreateClient(HttpStatusCode.NoContent);

        var result = await client.DeleteResultAsync("/api/items/1");

        await Assert.That(result.IsSuccess).IsTrue();
    }

    [Test]
    public async Task DeleteResultAsync_returns_success_on_200()
    {
        using var client = CreateClient(HttpStatusCode.OK);

        var result = await client.DeleteResultAsync("/api/items/1");

        await Assert.That(result.IsSuccess).IsTrue();
    }

    [Test]
    public async Task DeleteResultAsync_returns_failure_on_404()
    {
        using var client = CreateClient(HttpStatusCode.NotFound, "Not found");

        var result = await client.DeleteResultAsync("/api/items/999");

        await Assert.That(result.IsFailure).IsTrue();
        var error = result.Match(_ => null!, e => e);
        await Assert.That(error).IsAssignableTo<NotFoundError>();
    }

    [Test]
    public async Task DeleteResultAsync_returns_HttpRequestError_on_network_failure()
    {
        using var client = CreateThrowingClient(new HttpRequestException("Connection refused"));

        var result = await client.DeleteResultAsync("/api/items/1");

        await Assert.That(result.IsFailure).IsTrue();
        var error = result.Match(_ => null!, e => e);
        await Assert.That(error).IsAssignableTo<HttpRequestError>();
    }

    [Test]
    public async Task DeleteResultAsync_generic_returns_success_with_body()
    {
        var json = """{"Id":1,"Name":"Deleted"}""";
        using var client = CreateClient(HttpStatusCode.OK, json);

        var result = await client.DeleteResultAsync<TestPayload>("/api/items/1");

        await Assert.That(result.IsSuccess).IsTrue();
        var value = result.Match(v => v, _ => null!);
        await Assert.That(value.Name).IsEqualTo("Deleted");
    }

    [Test]
    public async Task DeleteResultAsync_generic_returns_failure_on_404()
    {
        using var client = CreateClient(HttpStatusCode.NotFound, "Not found");

        var result = await client.DeleteResultAsync<TestPayload>("/api/items/999");

        await Assert.That(result.IsFailure).IsTrue();
        var error = result.Match(_ => null!, e => e);
        await Assert.That(error).IsAssignableTo<NotFoundError>();
    }

    #endregion

    #region SendResultAsync Tests

    [Test]
    public async Task SendResultAsync_returns_success_for_custom_request()
    {
        var json = """{"Id":1,"Name":"Custom"}""";
        using var client = CreateClient(HttpStatusCode.OK, json);
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/items/1");

        var result = await client.SendResultAsync<TestPayload>(request);

        await Assert.That(result.IsSuccess).IsTrue();
        var value = result.Match(v => v, _ => null!);
        await Assert.That(value.Name).IsEqualTo("Custom");
    }

    [Test]
    public async Task SendResultAsync_returns_failure_for_error_response()
    {
        using var client = CreateClient(HttpStatusCode.InternalServerError, "Server failed");
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/items/1");

        var result = await client.SendResultAsync<TestPayload>(request);

        await Assert.That(result.IsFailure).IsTrue();
        var error = result.Match(_ => null!, e => e);
        await Assert.That(error).IsAssignableTo<ExternalServiceError>();
    }

    [Test]
    public async Task SendResultAsync_returns_HttpRequestError_on_network_failure()
    {
        using var client = CreateThrowingClient(new HttpRequestException("Network error"));
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/items/1");

        var result = await client.SendResultAsync<TestPayload>(request);

        await Assert.That(result.IsFailure).IsTrue();
        var error = result.Match(_ => null!, e => e);
        await Assert.That(error).IsAssignableTo<HttpRequestError>();
    }

    [Test]
    public async Task SendResultAsync_no_body_returns_success_on_200()
    {
        using var client = CreateClient(HttpStatusCode.OK);
        var request = new HttpRequestMessage(HttpMethod.Head, "/api/health");

        var result = await client.SendResultAsync(request);

        await Assert.That(result.IsSuccess).IsTrue();
    }

    [Test]
    public async Task SendResultAsync_no_body_returns_failure_on_error()
    {
        using var client = CreateClient(HttpStatusCode.ServiceUnavailable, "Unavailable");
        var request = new HttpRequestMessage(HttpMethod.Head, "/api/health");

        var result = await client.SendResultAsync(request);

        await Assert.That(result.IsFailure).IsTrue();
        var error = result.Match(_ => null!, e => e);
        await Assert.That(error).IsAssignableTo<ExternalServiceError>();
    }

    [Test]
    public async Task SendResultAsync_no_body_returns_HttpRequestError_on_network_failure()
    {
        using var client = CreateThrowingClient(new HttpRequestException("DNS failed"));
        var request = new HttpRequestMessage(HttpMethod.Head, "/api/health");

        var result = await client.SendResultAsync(request);

        await Assert.That(result.IsFailure).IsTrue();
        var error = result.Match(_ => null!, e => e);
        await Assert.That(error).IsAssignableTo<HttpRequestError>();
    }

    #endregion

    #region Error Code Mapping Tests

    [Test]
    public async Task Error_code_contains_numeric_status_code()
    {
        using var client = CreateClient(HttpStatusCode.NotFound, "Not found");

        var result = await client.GetResultAsync<TestPayload>("/api/items/1");

        var error = result.Match(_ => null!, e => e);
        await Assert.That(error.Code).IsEqualTo("404");
    }

    [Test]
    public async Task HttpRequestError_preserves_exception_type()
    {
        using var client = CreateThrowingClient(new HttpRequestException("Connection refused"));

        var result = await client.GetResultAsync<TestPayload>("/api/items/1");

        var error = result.Match(_ => null!, e => e);
        await Assert.That(error).IsAssignableTo<HttpRequestError>();
        var requestError = (HttpRequestError)error;
        await Assert.That(requestError.ExceptionType).IsEqualTo(typeof(HttpRequestException).FullName);
    }

    #endregion

    #region Unit Tests

    [Test]
    public async Task Unit_value_is_default()
    {
        var unit = Unit.Value;

        await Assert.That(unit).IsEqualTo(default(Unit));
    }

    [Test]
    public async Task Unit_values_are_equal()
    {
        var unit1 = Unit.Value;
        var unit2 = Unit.Value;

        await Assert.That(unit1).IsEqualTo(unit2);
    }

    #endregion
}

/// <summary>
/// A fake <see cref="HttpMessageHandler"/> that returns a predetermined response
/// for testing HTTP client extensions without making real network calls.
/// </summary>
internal class FakeHttpMessageHandler : HttpMessageHandler
{
    private readonly HttpStatusCode _statusCode;
    private readonly string? _content;
    private readonly string _contentType;

    public FakeHttpMessageHandler(HttpStatusCode statusCode, string? content = null,
        string contentType = "application/json")
    {
        _statusCode = statusCode;
        _content = content;
        _contentType = contentType;
    }

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var response = new HttpResponseMessage(_statusCode);

        if (_content is not null)
        {
            response.Content = new StringContent(_content, System.Text.Encoding.UTF8, _contentType);
        }

        return Task.FromResult(response);
    }
}

/// <summary>
/// A fake <see cref="HttpMessageHandler"/> that throws a specified exception,
/// simulating network failures and timeouts for testing.
/// </summary>
internal class ThrowingHttpMessageHandler : HttpMessageHandler
{
    private readonly Exception _exception;

    public ThrowingHttpMessageHandler(Exception exception)
    {
        _exception = exception;
    }

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken) =>
        throw _exception;
}

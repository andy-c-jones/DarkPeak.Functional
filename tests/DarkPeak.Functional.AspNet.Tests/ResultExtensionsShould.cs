using DarkPeak.Functional.AspNet;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using TUnit.Assertions;
using TUnit.Assertions.Extensions;
using TUnit.Core;

namespace DarkPeak.Functional.AspNet.Tests;

/// <summary>
/// Tests for <see cref="ResultExtensions"/> verifying correct mapping of
/// <see cref="Result{T, TError}"/> to ASP.NET <see cref="IResult"/> responses.
/// </summary>
public class ResultExtensionsShould
{
    #region Test Helpers

    private record TestItem(int Id, string Name);

    #endregion

    #region ToIResult Success Tests

    [Test]
    public async Task ToIResult_returns_Ok_on_success()
    {
        Result<TestItem, Error> result = new Success<TestItem, Error>(new TestItem(1, "Widget"));

        var httpResult = result.ToIResult();

        await Assert.That(httpResult).IsAssignableTo<Ok<TestItem>>();
        var ok = (Ok<TestItem>)httpResult;
        await Assert.That(ok.Value!.Id).IsEqualTo(1);
        await Assert.That(ok.Value!.Name).IsEqualTo("Widget");
    }

    [Test]
    public async Task ToIResult_async_returns_Ok_on_success()
    {
        var resultTask = Task.FromResult<Result<TestItem, Error>>(
            new Success<TestItem, Error>(new TestItem(1, "Widget")));

        var httpResult = await resultTask.ToIResult();

        await Assert.That(httpResult).IsAssignableTo<Ok<TestItem>>();
    }

    #endregion

    #region ToIResult Error Mapping Tests

    [Test]
    public async Task ToIResult_returns_ValidationProblem_on_ValidationError()
    {
        var errors = new Dictionary<string, string[]>
        {
            ["Name"] = ["Name is required"],
            ["Email"] = ["Email is invalid"]
        };
        Result<TestItem, Error> result = new Failure<TestItem, Error>(
            new ValidationError { Message = "Validation failed", Errors = errors });

        var httpResult = result.ToIResult();

        await Assert.That(httpResult).IsAssignableTo<ProblemHttpResult>();
    }

    [Test]
    public async Task ToIResult_returns_BadRequest_on_BadRequestError()
    {
        Result<TestItem, Error> result = new Failure<TestItem, Error>(
            new BadRequestError { Message = "Invalid input", Parameter = "id" });

        var httpResult = result.ToIResult();

        await Assert.That(httpResult).IsAssignableTo<ProblemHttpResult>();
        var problem = (ProblemHttpResult)httpResult;
        await Assert.That(problem.StatusCode).IsEqualTo(StatusCodes.Status400BadRequest);
        await Assert.That(problem.ProblemDetails.Detail).IsEqualTo("Invalid input");
    }

    [Test]
    public async Task ToIResult_returns_NotFound_on_NotFoundError()
    {
        Result<TestItem, Error> result = new Failure<TestItem, Error>(
            new NotFoundError { Message = "Item not found", ResourceType = "TestItem", ResourceId = "1" });

        var httpResult = result.ToIResult();

        await Assert.That(httpResult).IsAssignableTo<ProblemHttpResult>();
        var problem = (ProblemHttpResult)httpResult;
        await Assert.That(problem.StatusCode).IsEqualTo(StatusCodes.Status404NotFound);
    }

    [Test]
    public async Task ToIResult_returns_Unauthorized_on_UnauthorizedError()
    {
        Result<TestItem, Error> result = new Failure<TestItem, Error>(
            new UnauthorizedError { Message = "Invalid credentials" });

        var httpResult = result.ToIResult();

        await Assert.That(httpResult).IsAssignableTo<UnauthorizedHttpResult>();
    }

    [Test]
    public async Task ToIResult_returns_Forbidden_on_ForbiddenError()
    {
        Result<TestItem, Error> result = new Failure<TestItem, Error>(
            new ForbiddenError { Message = "Access denied", Resource = "admin" });

        var httpResult = result.ToIResult();

        await Assert.That(httpResult).IsAssignableTo<ProblemHttpResult>();
        var problem = (ProblemHttpResult)httpResult;
        await Assert.That(problem.StatusCode).IsEqualTo(StatusCodes.Status403Forbidden);
    }

    [Test]
    public async Task ToIResult_returns_Conflict_on_ConflictError()
    {
        Result<TestItem, Error> result = new Failure<TestItem, Error>(
            new ConflictError { Message = "Duplicate resource", ConflictingResource = "item-1" });

        var httpResult = result.ToIResult();

        await Assert.That(httpResult).IsAssignableTo<ProblemHttpResult>();
        var problem = (ProblemHttpResult)httpResult;
        await Assert.That(problem.StatusCode).IsEqualTo(StatusCodes.Status409Conflict);
    }

    [Test]
    public async Task ToIResult_returns_BadGateway_on_ExternalServiceError()
    {
        Result<TestItem, Error> result = new Failure<TestItem, Error>(
            new ExternalServiceError { Message = "Upstream failed", ServiceName = "PaymentGateway" });

        var httpResult = result.ToIResult();

        await Assert.That(httpResult).IsAssignableTo<ProblemHttpResult>();
        var problem = (ProblemHttpResult)httpResult;
        await Assert.That(problem.StatusCode).IsEqualTo(StatusCodes.Status502BadGateway);
    }

    [Test]
    public async Task ToIResult_returns_InternalServerError_on_InternalError()
    {
        Result<TestItem, Error> result = new Failure<TestItem, Error>(
            new InternalError { Message = "Unexpected error", ExceptionType = "NullReferenceException" });

        var httpResult = result.ToIResult();

        await Assert.That(httpResult).IsAssignableTo<ProblemHttpResult>();
        var problem = (ProblemHttpResult)httpResult;
        await Assert.That(problem.StatusCode).IsEqualTo(StatusCodes.Status500InternalServerError);
    }

    [Test]
    public async Task ToIResult_returns_InternalServerError_on_unknown_error_type()
    {
        Result<TestItem, Error> result = new Failure<TestItem, Error>(
            new CustomTestError { Message = "Something custom" });

        var httpResult = result.ToIResult();

        await Assert.That(httpResult).IsAssignableTo<ProblemHttpResult>();
        var problem = (ProblemHttpResult)httpResult;
        await Assert.That(problem.StatusCode).IsEqualTo(StatusCodes.Status500InternalServerError);
    }

    #endregion

    #region ToCreatedResult Tests

    [Test]
    public async Task ToCreatedResult_returns_Created_on_success()
    {
        Result<TestItem, Error> result = new Success<TestItem, Error>(new TestItem(42, "NewItem"));

        var httpResult = result.ToCreatedResult(item => $"/api/items/{item.Id}");

        await Assert.That(httpResult).IsAssignableTo<Created<TestItem>>();
        var created = (Created<TestItem>)httpResult;
        await Assert.That(created.Location).IsEqualTo("/api/items/42");
        await Assert.That(created.Value!.Name).IsEqualTo("NewItem");
    }

    [Test]
    public async Task ToCreatedResult_returns_error_on_failure()
    {
        Result<TestItem, Error> result = new Failure<TestItem, Error>(
            new BadRequestError { Message = "Invalid" });

        var httpResult = result.ToCreatedResult(item => $"/api/items/{item.Id}");

        await Assert.That(httpResult).IsAssignableTo<ProblemHttpResult>();
        var problem = (ProblemHttpResult)httpResult;
        await Assert.That(problem.StatusCode).IsEqualTo(StatusCodes.Status400BadRequest);
    }

    [Test]
    public async Task ToCreatedResult_async_returns_Created_on_success()
    {
        var resultTask = Task.FromResult<Result<TestItem, Error>>(
            new Success<TestItem, Error>(new TestItem(42, "NewItem")));

        var httpResult = await resultTask.ToCreatedResult(item => $"/api/items/{item.Id}");

        await Assert.That(httpResult).IsAssignableTo<Created<TestItem>>();
    }

    #endregion

    #region ToNoContentResult Tests

    [Test]
    public async Task ToNoContentResult_returns_NoContent_on_success()
    {
        Result<TestItem, Error> result = new Success<TestItem, Error>(new TestItem(1, "Deleted"));

        var httpResult = result.ToNoContentResult();

        await Assert.That(httpResult).IsAssignableTo<NoContent>();
    }

    [Test]
    public async Task ToNoContentResult_returns_error_on_failure()
    {
        Result<TestItem, Error> result = new Failure<TestItem, Error>(
            new NotFoundError { Message = "Not found" });

        var httpResult = result.ToNoContentResult();

        await Assert.That(httpResult).IsAssignableTo<ProblemHttpResult>();
        var problem = (ProblemHttpResult)httpResult;
        await Assert.That(problem.StatusCode).IsEqualTo(StatusCodes.Status404NotFound);
    }

    [Test]
    public async Task ToNoContentResult_async_returns_NoContent_on_success()
    {
        var resultTask = Task.FromResult<Result<TestItem, Error>>(
            new Success<TestItem, Error>(new TestItem(1, "Deleted")));

        var httpResult = await resultTask.ToNoContentResult();

        await Assert.That(httpResult).IsAssignableTo<NoContent>();
    }

    #endregion

    #region Async Error Mapping Tests

    [Test]
    public async Task ToIResult_async_returns_error_on_failure()
    {
        var resultTask = Task.FromResult<Result<TestItem, Error>>(
            new Failure<TestItem, Error>(new NotFoundError { Message = "Not found" }));

        var httpResult = await resultTask.ToIResult();

        await Assert.That(httpResult).IsAssignableTo<ProblemHttpResult>();
        var problem = (ProblemHttpResult)httpResult;
        await Assert.That(problem.StatusCode).IsEqualTo(StatusCodes.Status404NotFound);
    }

    #endregion
}

/// <summary>
/// A custom error type for testing that unknown error types fall through to 500.
/// </summary>
internal sealed record CustomTestError : Error;

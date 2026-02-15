using DarkPeak.Functional.AspNet;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TUnit.Assertions;
using TUnit.Assertions.Extensions;
using TUnit.Core;

namespace DarkPeak.Functional.AspNet.Tests;

/// <summary>
/// Tests for <see cref="ProblemDetailsExtensions"/> verifying correct conversion of
/// <see cref="Error"/> types to <see cref="ProblemDetails"/> and <see cref="HttpValidationProblemDetails"/>.
/// </summary>
public class ProblemDetailsExtensionsShould
{
    #region ToProblemDetails - Status Code Mapping

    [Test]
    public async Task ToProblemDetails_maps_ValidationError_to_422()
    {
        var error = new ValidationError { Message = "Validation failed" };

        var problemDetails = error.ToProblemDetails();

        await Assert.That(problemDetails.Status).IsEqualTo(StatusCodes.Status422UnprocessableEntity);
        await Assert.That(problemDetails.Title).IsEqualTo("Validation Failed");
        await Assert.That(problemDetails.Detail).IsEqualTo("Validation failed");
    }

    [Test]
    public async Task ToProblemDetails_maps_BadRequestError_to_400()
    {
        var error = new BadRequestError { Message = "Invalid parameters", Parameter = "id" };

        var problemDetails = error.ToProblemDetails();

        await Assert.That(problemDetails.Status).IsEqualTo(StatusCodes.Status400BadRequest);
        await Assert.That(problemDetails.Title).IsEqualTo("Bad Request");
        await Assert.That(problemDetails.Detail).IsEqualTo("Invalid parameters");
    }

    [Test]
    public async Task ToProblemDetails_maps_NotFoundError_to_404()
    {
        var error = new NotFoundError { Message = "Order not found", ResourceType = "Order", ResourceId = "123" };

        var problemDetails = error.ToProblemDetails();

        await Assert.That(problemDetails.Status).IsEqualTo(StatusCodes.Status404NotFound);
        await Assert.That(problemDetails.Title).IsEqualTo("Not Found");
    }

    [Test]
    public async Task ToProblemDetails_maps_UnauthorizedError_to_401()
    {
        var error = new UnauthorizedError { Message = "Token expired" };

        var problemDetails = error.ToProblemDetails();

        await Assert.That(problemDetails.Status).IsEqualTo(StatusCodes.Status401Unauthorized);
        await Assert.That(problemDetails.Title).IsEqualTo("Unauthorized");
    }

    [Test]
    public async Task ToProblemDetails_maps_ForbiddenError_to_403()
    {
        var error = new ForbiddenError { Message = "Insufficient permissions", Resource = "admin" };

        var problemDetails = error.ToProblemDetails();

        await Assert.That(problemDetails.Status).IsEqualTo(StatusCodes.Status403Forbidden);
        await Assert.That(problemDetails.Title).IsEqualTo("Forbidden");
    }

    [Test]
    public async Task ToProblemDetails_maps_ConflictError_to_409()
    {
        var error = new ConflictError { Message = "Duplicate entry", ConflictingResource = "user-email" };

        var problemDetails = error.ToProblemDetails();

        await Assert.That(problemDetails.Status).IsEqualTo(StatusCodes.Status409Conflict);
        await Assert.That(problemDetails.Title).IsEqualTo("Conflict");
    }

    [Test]
    public async Task ToProblemDetails_maps_ExternalServiceError_to_502()
    {
        var error = new ExternalServiceError { Message = "Payment gateway timeout", ServiceName = "Stripe" };

        var problemDetails = error.ToProblemDetails();

        await Assert.That(problemDetails.Status).IsEqualTo(StatusCodes.Status502BadGateway);
        await Assert.That(problemDetails.Title).IsEqualTo("Bad Gateway");
    }

    [Test]
    public async Task ToProblemDetails_maps_InternalError_to_500()
    {
        var error = new InternalError { Message = "Unexpected failure", ExceptionType = "NullReferenceException" };

        var problemDetails = error.ToProblemDetails();

        await Assert.That(problemDetails.Status).IsEqualTo(StatusCodes.Status500InternalServerError);
        await Assert.That(problemDetails.Title).IsEqualTo("Internal Server Error");
    }

    [Test]
    public async Task ToProblemDetails_maps_unknown_error_type_to_500()
    {
        var error = new CustomTestError2 { Message = "Something custom" };

        var problemDetails = error.ToProblemDetails();

        await Assert.That(problemDetails.Status).IsEqualTo(StatusCodes.Status500InternalServerError);
        await Assert.That(problemDetails.Title).IsEqualTo("Internal Server Error");
    }

    #endregion

    #region ToProblemDetails - Code and Metadata

    [Test]
    public async Task ToProblemDetails_includes_error_code_in_extensions()
    {
        var error = new BadRequestError { Message = "Bad", Code = "BR001" };

        var problemDetails = error.ToProblemDetails();

        await Assert.That(problemDetails.Extensions.ContainsKey("errorCode")).IsTrue();
        await Assert.That(problemDetails.Extensions["errorCode"]).IsEqualTo("BR001");
    }

    [Test]
    public async Task ToProblemDetails_excludes_error_code_when_null()
    {
        var error = new BadRequestError { Message = "Bad" };

        var problemDetails = error.ToProblemDetails();

        await Assert.That(problemDetails.Extensions.ContainsKey("errorCode")).IsFalse();
    }

    [Test]
    public async Task ToProblemDetails_includes_metadata_in_extensions()
    {
        var error = new NotFoundError
        {
            Message = "Not found",
            Metadata = new Dictionary<string, object>
            {
                ["traceId"] = "abc-123",
                ["timestamp"] = "2025-01-01T00:00:00Z"
            }
        };

        var problemDetails = error.ToProblemDetails();

        await Assert.That(problemDetails.Extensions.ContainsKey("traceId")).IsTrue();
        await Assert.That(problemDetails.Extensions["traceId"]).IsEqualTo("abc-123");
        await Assert.That(problemDetails.Extensions.ContainsKey("timestamp")).IsTrue();
        await Assert.That(problemDetails.Extensions["timestamp"]).IsEqualTo("2025-01-01T00:00:00Z");
    }

    [Test]
    public async Task ToProblemDetails_without_metadata_has_no_extra_extensions()
    {
        var error = new NotFoundError { Message = "Not found" };

        var problemDetails = error.ToProblemDetails();

        // Should only potentially have errorCode, but since Code is null, nothing
        await Assert.That(problemDetails.Extensions.Count).IsEqualTo(0);
    }

    [Test]
    public async Task ToProblemDetails_includes_both_code_and_metadata()
    {
        var error = new InternalError
        {
            Message = "Crash",
            Code = "INT500",
            Metadata = new Dictionary<string, object> { ["requestId"] = "req-456" }
        };

        var problemDetails = error.ToProblemDetails();

        await Assert.That(problemDetails.Extensions.ContainsKey("errorCode")).IsTrue();
        await Assert.That(problemDetails.Extensions["errorCode"]).IsEqualTo("INT500");
        await Assert.That(problemDetails.Extensions.ContainsKey("requestId")).IsTrue();
        await Assert.That(problemDetails.Extensions["requestId"]).IsEqualTo("req-456");
    }

    #endregion

    #region ToValidationProblemDetails Tests

    [Test]
    public async Task ToValidationProblemDetails_includes_field_errors()
    {
        var error = new ValidationError
        {
            Message = "Validation failed",
            Errors = new Dictionary<string, string[]>
            {
                ["Name"] = ["Name is required", "Name must be at least 2 characters"],
                ["Email"] = ["Email is not valid"]
            }
        };

        var problemDetails = error.ToValidationProblemDetails();

        await Assert.That(problemDetails.Status).IsEqualTo(StatusCodes.Status422UnprocessableEntity);
        await Assert.That(problemDetails.Title).IsEqualTo("Validation Failed");
        await Assert.That(problemDetails.Detail).IsEqualTo("Validation failed");
        await Assert.That(problemDetails.Errors.ContainsKey("Name")).IsTrue();
        await Assert.That(problemDetails.Errors["Name"].Length).IsEqualTo(2);
        await Assert.That(problemDetails.Errors.ContainsKey("Email")).IsTrue();
    }

    [Test]
    public async Task ToValidationProblemDetails_handles_null_errors_dictionary()
    {
        var error = new ValidationError { Message = "Validation failed" };

        var problemDetails = error.ToValidationProblemDetails();

        await Assert.That(problemDetails.Status).IsEqualTo(StatusCodes.Status422UnprocessableEntity);
        await Assert.That(problemDetails.Errors.Count).IsEqualTo(0);
    }

    [Test]
    public async Task ToValidationProblemDetails_includes_error_code()
    {
        var error = new ValidationError { Message = "Failed", Code = "VAL001" };

        var problemDetails = error.ToValidationProblemDetails();

        await Assert.That(problemDetails.Extensions.ContainsKey("errorCode")).IsTrue();
        await Assert.That(problemDetails.Extensions["errorCode"]).IsEqualTo("VAL001");
    }

    [Test]
    public async Task ToValidationProblemDetails_includes_metadata()
    {
        var error = new ValidationError
        {
            Message = "Failed",
            Metadata = new Dictionary<string, object> { ["field"] = "email" }
        };

        var problemDetails = error.ToValidationProblemDetails();

        await Assert.That(problemDetails.Extensions.ContainsKey("field")).IsTrue();
        await Assert.That(problemDetails.Extensions["field"]).IsEqualTo("email");
    }

    [Test]
    public async Task ToValidationProblemDetails_excludes_code_when_null()
    {
        var error = new ValidationError { Message = "Failed" };

        var problemDetails = error.ToValidationProblemDetails();

        await Assert.That(problemDetails.Extensions.ContainsKey("errorCode")).IsFalse();
    }

    #endregion
}

/// <summary>
/// A custom error type for testing unknown error fallback behavior.
/// </summary>
internal sealed record CustomTestError2 : Error;

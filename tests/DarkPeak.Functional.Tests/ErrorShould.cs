using DarkPeak.Functional;

namespace DarkPeak.Functional.Tests;

/// <summary>
/// Tests for Error type hierarchy.
/// </summary>
public class ErrorShould
{
    [Test]
    public async Task Create_validation_error_with_field_errors()
    {
        var error = new ValidationError
        {
            Message = "Validation failed",
            Errors = new Dictionary<string, string[]>
            {
                ["Name"] = ["Name is required"],
                ["Age"] = ["Must be positive"]
            }
        };

        await Assert.That(error.Message).IsEqualTo("Validation failed");
        await Assert.That(error.Errors).IsNotNull();
        await Assert.That(error.Errors!.Keys).Count().IsEqualTo(2);
    }

    [Test]
    public async Task Create_not_found_error_with_resource_info()
    {
        var error = new NotFoundError
        {
            Message = "User not found",
            ResourceType = "User",
            ResourceId = "123"
        };

        await Assert.That(error.Message).IsEqualTo("User not found");
        await Assert.That(error.ResourceType).IsEqualTo("User");
        await Assert.That(error.ResourceId).IsEqualTo("123");
    }

    [Test]
    public async Task Create_unauthorized_error_with_reason()
    {
        var error = new UnauthorizedError
        {
            Message = "Not authenticated",
            Reason = "Token expired"
        };

        await Assert.That(error.Message).IsEqualTo("Not authenticated");
        await Assert.That(error.Reason).IsEqualTo("Token expired");
    }

    [Test]
    public async Task Create_forbidden_error_with_resource()
    {
        var error = new ForbiddenError
        {
            Message = "Access denied",
            Resource = "/admin/settings"
        };

        await Assert.That(error.Message).IsEqualTo("Access denied");
        await Assert.That(error.Resource).IsEqualTo("/admin/settings");
    }

    [Test]
    public async Task Create_conflict_error_with_conflicting_resource()
    {
        var error = new ConflictError
        {
            Message = "Duplicate entry",
            ConflictingResource = "user@example.com"
        };

        await Assert.That(error.Message).IsEqualTo("Duplicate entry");
        await Assert.That(error.ConflictingResource).IsEqualTo("user@example.com");
    }

    [Test]
    public async Task Create_external_service_error_with_details()
    {
        var error = new ExternalServiceError
        {
            Message = "Service unavailable",
            ServiceName = "PaymentGateway",
            InnerMessage = "Connection timeout"
        };

        await Assert.That(error.Message).IsEqualTo("Service unavailable");
        await Assert.That(error.ServiceName).IsEqualTo("PaymentGateway");
        await Assert.That(error.InnerMessage).IsEqualTo("Connection timeout");
    }

    [Test]
    public async Task Create_bad_request_error_with_parameter()
    {
        var error = new BadRequestError
        {
            Message = "Invalid input",
            Parameter = "email"
        };

        await Assert.That(error.Message).IsEqualTo("Invalid input");
        await Assert.That(error.Parameter).IsEqualTo("email");
    }

    [Test]
    public async Task Create_internal_error_with_exception_type()
    {
        var error = new InternalError
        {
            Message = "Something went wrong",
            ExceptionType = "NullReferenceException"
        };

        await Assert.That(error.Message).IsEqualTo("Something went wrong");
        await Assert.That(error.ExceptionType).IsEqualTo("NullReferenceException");
    }

    [Test]
    public async Task Support_optional_code_on_all_errors()
    {
        var error = new NotFoundError
        {
            Message = "Not found",
            Code = "ERR_404"
        };

        await Assert.That(error.Code).IsEqualTo("ERR_404");
    }

    [Test]
    public async Task Support_optional_metadata_on_all_errors()
    {
        var error = new InternalError
        {
            Message = "Error",
            Metadata = new Dictionary<string, object>
            {
                ["RequestId"] = "abc-123",
                ["Timestamp"] = 1234567890L
            }
        };

        await Assert.That(error.Metadata).IsNotNull();
        await Assert.That(error.Metadata!.Keys).Count().IsEqualTo(2);
    }
}

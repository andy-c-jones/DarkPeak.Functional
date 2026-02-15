# AspNet

The `DarkPeak.Functional.AspNet` package provides extensions that convert `Result<T, Error>` to ASP.NET Core `IResult` and `ProblemDetails`, enabling idiomatic minimal API error handling with zero boilerplate.

## Installation

```bash
dotnet add package DarkPeak.Functional.AspNet
```

## Basic Usage

Convert any `Result<T, Error>` to an HTTP response in a single call:

```csharp
using DarkPeak.Functional;
using DarkPeak.Functional.AspNet;

app.MapGet("/orders/{id}", async (int id, IOrderService service) =>
    (await service.GetOrderAsync(id)).ToIResult());
```

On success, returns `200 OK` with the value as JSON. On failure, returns an error-specific HTTP status code with a ProblemDetails body.

## ToIResult

Maps `Result<T, Error>` to an `IResult`:

```csharp
// Sync
app.MapGet("/orders/{id}", (int id, IOrderService service) =>
    service.GetOrder(id).ToIResult());

// Async
app.MapGet("/orders/{id}", async (int id, IOrderService service) =>
    await service.GetOrderAsync(id).ToIResult());
```

Success produces `200 OK`. Failure maps to the appropriate HTTP status code.

## ToCreatedResult

Maps success to `201 Created` with a `Location` header:

```csharp
app.MapPost("/orders", async (CreateOrderRequest request, IOrderService service) =>
    await service.CreateOrderAsync(request)
        .ToCreatedResult(order => $"/orders/{order.Id}"));
```

The lambda receives the success value and returns the location URI.

## ToNoContentResult

Maps success to `204 No Content` (useful for updates and deletes):

```csharp
app.MapDelete("/orders/{id}", async (int id, IOrderService service) =>
    await service.DeleteOrderAsync(id).ToNoContentResult());
```

## Error-to-Status Mapping

Errors are automatically mapped to HTTP status codes based on their type:

| Error Type | Status Code | Title |
|------------|-------------|-------|
| `ValidationError` | 422 Unprocessable Entity | Validation Failed |
| `BadRequestError` | 400 Bad Request | Bad Request |
| `NotFoundError` | 404 Not Found | Not Found |
| `UnauthorizedError` | 401 Unauthorized | Unauthorized |
| `ForbiddenError` | 403 Forbidden | Forbidden |
| `ConflictError` | 409 Conflict | Conflict |
| `ExternalServiceError` | 502 Bad Gateway | Bad Gateway |
| `InternalError` | 500 Internal Server Error | Internal Server Error |
| Any other `Error` | 500 Internal Server Error | Internal Server Error |

`ValidationError` produces a `ValidationProblem` response with field-level errors preserved. `UnauthorizedError` produces a bare `401 Unauthorized` response with no body (matching ASP.NET conventions). All other errors produce a `Problem` response with ProblemDetails.

## ProblemDetails Conversion

Convert any `Error` to an RFC 9457 ProblemDetails object directly:

```csharp
var error = new NotFoundError { Message = "Order 123 not found", Code = "ORDER_NOT_FOUND" };
var problem = error.ToProblemDetails();
// { Status: 404, Title: "Not Found", Detail: "Order 123 not found",
//   Extensions: { "errorCode": "ORDER_NOT_FOUND" } }
```

### ValidationError to HttpValidationProblemDetails

`ValidationError` gets a specialized conversion that preserves field-level errors:

```csharp
var error = new ValidationError
{
    Message = "Input validation failed",
    Errors = new Dictionary<string, string[]>
    {
        ["email"] = ["Email is required"],
        ["age"] = ["Must be between 18 and 120"]
    }
};

var problem = error.ToValidationProblemDetails();
// Status: 422, Title: "Validation Failed"
// Errors: { "email": ["Email is required"], "age": ["Must be between 18 and 120"] }
```

### Error Code and Metadata

Error `Code` and `Metadata` are included in `ProblemDetails.Extensions`:

```csharp
var error = new NotFoundError
{
    Message = "Order not found",
    Code = "ORDER_NOT_FOUND",
    Metadata = new Dictionary<string, object> { ["orderId"] = 123 }
};

var problem = error.ToProblemDetails();
// Extensions: { "errorCode": "ORDER_NOT_FOUND", "orderId": 123 }
```

## Minimal API Example

A complete user registration API using `Validation`, `Result`, and the AspNet extensions:

```csharp
using DarkPeak.Functional;
using DarkPeak.Functional.AspNet;
using DarkPeak.Functional.Extensions;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<IUserRepository, InMemoryUserRepository>();

var app = builder.Build();

app.MapPost("/users", (CreateUserRequest request, IUserRepository repo) =>
    ValidateName(request.Name)
        .ZipWith(
            ValidateEmail(request.Email),
            ValidateAge(request.Age),
            (name, email, age) => new User(name, email, age, Guid.NewGuid()))
        .ToResult()
        .Bind(user => repo.Save(user))
        .ToCreatedResult(user => $"/users/{user.Id}"));

app.Run();
```

### Domain Types

```csharp
public record CreateUserRequest(string Name, string Email, int Age);
public record User(string Name, string Email, int Age, Guid Id);
```

### Validators

```csharp
static Validation<string, ValidationError> ValidateName(string name) =>
    string.IsNullOrWhiteSpace(name)
        ? Validation.Invalid<string, ValidationError>(
            new ValidationError { Message = "Name is required", Code = "name" })
        : name.Length > 100
            ? Validation.Invalid<string, ValidationError>(
                new ValidationError { Message = "Name must be 100 characters or fewer", Code = "name" })
            : Validation.Valid<string, ValidationError>(name.Trim());

static Validation<string, ValidationError> ValidateEmail(string email) =>
    string.IsNullOrWhiteSpace(email)
        ? Validation.Invalid<string, ValidationError>(
            new ValidationError { Message = "Email is required", Code = "email" })
        : !email.Contains('@')
            ? Validation.Invalid<string, ValidationError>(
                new ValidationError { Message = "Email must be a valid address", Code = "email" })
            : Validation.Valid<string, ValidationError>(email.Trim().ToLower());

static Validation<int, ValidationError> ValidateAge(int age) =>
    age is < 18 or > 120
        ? Validation.Invalid<int, ValidationError>(
            new ValidationError { Message = "Age must be between 18 and 120", Code = "age" })
        : Validation.Valid<int, ValidationError>(age);
```

### Repository

```csharp
public interface IUserRepository
{
    Result<User, Error> Save(User user);
}

public class InMemoryUserRepository : IUserRepository
{
    private readonly Dictionary<Guid, User> _users = new();

    public Result<User, Error> Save(User user)
    {
        if (_users.Values.Any(u => u.Email == user.Email))
            return Result.Failure<User, Error>(
                new ConflictError { Message = $"User with email {user.Email} already exists" });

        _users[user.Id] = user;
        return Result.Success<User, Error>(user);
    }
}
```

### HTTP Responses

**Success — 201 Created:**

```http
POST /users
Content-Type: application/json

{ "name": "Alice", "email": "alice@example.com", "age": 30 }
```

```http
HTTP/1.1 201 Created
Location: /users/3fa85f64-5717-4562-b3fc-2c963f66afa6

{
  "name": "Alice",
  "email": "alice@example.com",
  "age": 30,
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6"
}
```

**Validation Failure — 422 Unprocessable Entity:**

```http
POST /users
Content-Type: application/json

{ "name": "", "email": "bad", "age": 200 }
```

```http
HTTP/1.1 422 Unprocessable Entity

{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "One or more validation errors occurred.",
  "errors": {
    "name": ["Name is required"],
    "email": ["Email must be a valid address"],
    "age": ["Age must be between 18 and 120"]
  }
}
```

**Conflict — 409:**

```http
HTTP/1.1 409 Conflict

{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.10",
  "title": "Conflict",
  "detail": "User with email alice@example.com already exists",
  "status": 409
}
```

Compare this to the manual approach — instead of pattern matching on error types and calling `Results.Conflict()`, `Results.Problem()`, etc., the AspNet extensions handle the mapping automatically via `.ToCreatedResult()`.

## Composition with Http Extensions

Use the Http library to call external APIs and convert the result directly to an HTTP response:

```csharp
app.MapGet("/proxy/orders/{id}", async (int id, HttpClient httpClient) =>
    await httpClient
        .GetResultAsync<Order>($"https://api.example.com/orders/{id}")
        .ToIResult());
```

Add retry and circuit breaker for resilient API proxying:

```csharp
var breaker = CircuitBreaker
    .WithFailureThreshold(5)
    .WithResetTimeout(TimeSpan.FromSeconds(30));

app.MapGet("/proxy/orders/{id}", async (int id, HttpClient httpClient) =>
    await Retry
        .WithMaxAttempts(3)
        .WithBackoff(Backoff.Exponential(TimeSpan.FromMilliseconds(200)))
        .ExecuteAsync(() =>
            breaker.ExecuteAsync(
                () => httpClient.GetResultAsync<Order>(
                    $"https://api.example.com/orders/{id}")))
        .ToIResult());
```

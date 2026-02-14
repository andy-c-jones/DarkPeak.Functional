# Example: Minimal Web API

This example shows how to use `Validation` and `Result` in an ASP.NET Core Minimal API to handle user registration with proper error responses.

## The Endpoint

```csharp
using DarkPeak.Functional;
using DarkPeak.Functional.Extensions;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<IUserRepository, InMemoryUserRepository>();

var app = builder.Build();

app.MapPost("/users", (CreateUserRequest request, IUserRepository repo) =>
{
    return ValidateName(request.Name)
        .Combine(
            ValidateEmail(request.Email),
            ValidateAge(request.Age),
            (name, email, age) => new User(name, email, age, Guid.NewGuid()))
        .Match(
            valid: user => repo.Save(user).Match(
                success: saved => Results.Created($"/users/{saved.Id}", saved),
                failure: err => err switch
                {
                    ConflictError => Results.Conflict(new { err.Message }),
                    _ => Results.Problem(err.Message)
                }),
            invalid: errors => Results.ValidationProblem(
                errors
                    .GroupBy(e => e.Code ?? "general")
                    .ToDictionary(g => g.Key, g => g.Select(e => e.Message).ToArray())));
});

app.Run();
```

## Domain Types

```csharp
public record CreateUserRequest(string Name, string Email, int Age);
public record User(string Name, string Email, int Age, Guid Id);
```

## Validators

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

## Repository

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

## HTTP Responses

### Success — 201 Created

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

### Validation Failure — 400

```http
POST /users
Content-Type: application/json

{ "name": "", "email": "bad", "age": 200 }
```

```http
HTTP/1.1 400 Bad Request

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

### Conflict — 409

```http
HTTP/1.1 409 Conflict

{ "message": "User with email alice@example.com already exists" }
```

## Async Variant

For async repositories, use `MatchAsync` and the task extensions:

```csharp
app.MapPost("/users", async (CreateUserRequest request, IUserRepository repo) =>
{
    return await ValidateName(request.Name)
        .Combine(
            ValidateEmail(request.Email),
            ValidateAge(request.Age),
            (name, email, age) => new User(name, email, age, Guid.NewGuid()))
        .Match<Task<IResult>>(
            valid: async user => (await repo.SaveAsync(user)).Match(
                success: saved => Results.Created($"/users/{saved.Id}", saved),
                failure: err => Results.Problem(err.Message)),
            invalid: errors => Task.FromResult(Results.ValidationProblem(
                errors
                    .GroupBy(e => e.Code ?? "general")
                    .ToDictionary(g => g.Key, g => g.Select(e => e.Message).ToArray()))));
});
```

## Key Takeaways

- **Validation** collects all input errors — the user sees every problem in one response
- **Result** handles sequential operations (save, external calls) with fail-fast behaviour
- Pattern matching on error types (`ConflictError`, etc.) maps naturally to HTTP status codes
- `Results.ValidationProblem()` produces a standard RFC 7807 problem details response

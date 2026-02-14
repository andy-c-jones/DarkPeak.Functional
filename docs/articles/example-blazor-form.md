# Example: Blazor User Creation Form

This example shows how to use `Validation` in a Blazor component to validate a user creation form, displaying accumulated field errors inline.

## The Form Component

```razor
@page "/users/create"
@using DarkPeak.Functional
@using DarkPeak.Functional.Extensions

<h3>Create User</h3>

<EditForm Model="@_request" OnValidSubmit="HandleSubmit">
    <div class="mb-3">
        <label class="form-label">Name</label>
        <InputText @bind-Value="_request.Name" class="form-control" />
        @if (FieldErrors("name") is { } nameErrors)
        {
            @foreach (var error in nameErrors)
            {
                <div class="text-danger">@error</div>
            }
        }
    </div>

    <div class="mb-3">
        <label class="form-label">Email</label>
        <InputText @bind-Value="_request.Email" class="form-control" />
        @if (FieldErrors("email") is { } emailErrors)
        {
            @foreach (var error in emailErrors)
            {
                <div class="text-danger">@error</div>
            }
        }
    </div>

    <div class="mb-3">
        <label class="form-label">Age</label>
        <InputNumber @bind-Value="_request.Age" class="form-control" />
        @if (FieldErrors("age") is { } ageErrors)
        {
            @foreach (var error in ageErrors)
            {
                <div class="text-danger">@error</div>
            }
        }
    </div>

    <button type="submit" class="btn btn-primary" disabled="@_submitting">
        @(_submitting ? "Creating..." : "Create User")
    </button>

    @if (_successMessage is not null)
    {
        <div class="alert alert-success mt-3">@_successMessage</div>
    }

    @if (_generalError is not null)
    {
        <div class="alert alert-danger mt-3">@_generalError</div>
    }
</EditForm>
```

## The Code-Behind

```csharp
@code {
    [Inject] private IUserService UserService { get; set; } = default!;

    private CreateUserRequest _request = new("", "", 0);
    private Dictionary<string, List<string>> _fieldErrors = new();
    private string? _successMessage;
    private string? _generalError;
    private bool _submitting;

    private IEnumerable<string>? FieldErrors(string field) =>
        _fieldErrors.TryGetValue(field, out var errors) && errors.Count > 0
            ? errors
            : null;

    private async Task HandleSubmit()
    {
        _fieldErrors.Clear();
        _successMessage = null;
        _generalError = null;
        _submitting = true;

        var validation = UserValidation.Validate(_request);

        await validation.Match(
            valid: async user =>
            {
                var result = await UserService.CreateAsync(user);
                result
                    .Tap(u => _successMessage = $"User {u.Name} created successfully!")
                    .TapError(err => _generalError = err.Message);
            },
            invalid: errors =>
            {
                foreach (var error in errors)
                {
                    var field = error.Code ?? "general";
                    if (!_fieldErrors.ContainsKey(field))
                        _fieldErrors[field] = new List<string>();
                    _fieldErrors[field].Add(error.Message);
                }
                return Task.CompletedTask;
            });

        _submitting = false;
    }
}
```

## Validation Logic

Keep validation logic separate from the component — it's reusable and testable:

```csharp
using DarkPeak.Functional;
using DarkPeak.Functional.Extensions;

public record CreateUserRequest(string Name, string Email, int Age);
public record User(string Name, string Email, int Age, Guid Id);

public static class UserValidation
{
    public static Validation<User, ValidationError> Validate(CreateUserRequest request) =>
        ValidateName(request.Name)
            .Combine(
                ValidateEmail(request.Email),
                ValidateAge(request.Age),
                (name, email, age) => new User(name, email, age, Guid.NewGuid()));

    private static Validation<string, ValidationError> ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Validation.Invalid<string, ValidationError>(
                new ValidationError { Message = "Name is required", Code = "name" });

        if (name.Length < 2)
            return Validation.Invalid<string, ValidationError>(
                new ValidationError { Message = "Name must be at least 2 characters", Code = "name" });

        if (name.Length > 100)
            return Validation.Invalid<string, ValidationError>(
                new ValidationError { Message = "Name must be 100 characters or fewer", Code = "name" });

        return Validation.Valid<string, ValidationError>(name.Trim());
    }

    private static Validation<string, ValidationError> ValidateEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return Validation.Invalid<string, ValidationError>(
                new ValidationError { Message = "Email is required", Code = "email" });

        if (!email.Contains('@') || !email.Contains('.'))
            return Validation.Invalid<string, ValidationError>(
                new ValidationError { Message = "Email must be a valid address", Code = "email" });

        return Validation.Valid<string, ValidationError>(email.Trim().ToLower());
    }

    private static Validation<int, ValidationError> ValidateAge(int age)
    {
        if (age < 18)
            return Validation.Invalid<int, ValidationError>(
                new ValidationError { Message = "Must be at least 18 years old", Code = "age" });

        if (age > 120)
            return Validation.Invalid<int, ValidationError>(
                new ValidationError { Message = "Age must be 120 or less", Code = "age" });

        return Validation.Valid<int, ValidationError>(age);
    }
}
```

## User Service

The service returns a `Result`, keeping the error handling consistent:

```csharp
public interface IUserService
{
    Task<Result<User, Error>> CreateAsync(User user);
}

public class UserService : IUserService
{
    private readonly HttpClient _http;

    public UserService(HttpClient http) => _http = http;

    public async Task<Result<User, Error>> CreateAsync(User user)
    {
        try
        {
            var response = await _http.PostAsJsonAsync("/api/users", user);

            return response.IsSuccessStatusCode
                ? Result.Success<User, Error>(user)
                : Result.Failure<User, Error>(
                    new ExternalServiceError
                    {
                        Message = $"Server returned {response.StatusCode}",
                        ServiceName = "UserAPI"
                    });
        }
        catch (HttpRequestException ex)
        {
            return Result.Failure<User, Error>(
                new ExternalServiceError
                {
                    Message = ex.Message,
                    ServiceName = "UserAPI"
                });
        }
    }
}
```

## How It Works

1. User fills in the form and clicks **Create User**
2. `UserValidation.Validate()` runs all three validators via `Combine` — errors are **accumulated**, not short-circuited
3. On **invalid**: the `ValidationError.Code` (e.g. `"name"`, `"email"`, `"age"`) maps errors to the correct field in the UI
4. On **valid**: the `UserService` is called, returning a `Result`. `Tap` sets the success message; `TapError` sets the error message
5. The component re-renders with either field errors, a success banner, or a general error

## Key Takeaways

- **Validation accumulates errors** — the user sees all problems at once, not one at a time
- **`ValidationError.Code`** serves as a field identifier for mapping errors to form inputs
- **Validation logic is separate** from the component — easy to unit test
- **Result handles the service call** — network errors, server errors, etc. are handled uniformly with `TapError`

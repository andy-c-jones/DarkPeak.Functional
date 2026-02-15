using DarkPeak.Functional;
using DarkPeak.Functional.Extensions;

namespace DarkPeak.Functional.Tests;

/// <summary>
/// Tests for ValidationPipeline (fluent validation builder with error accumulation).
/// </summary>
public class ValidationPipelineShould
{
    // ──────────────────────────────────────────────────────────────
    //  Helpers
    // ──────────────────────────────────────────────────────────────

    private record UserDto(string Name, int Age, string Email);

    private record User(string Name, int Age, string Email);

    private static Validation<string, Error> ValidateName(string name) =>
        string.IsNullOrWhiteSpace(name)
            ? Validation.Invalid<string, Error>(new ValidationError { Message = "Name is required" })
            : Validation.Valid<string, Error>(name);

    private static Validation<int, Error> ValidateAge(int age) =>
        age is < 0 or > 150
            ? Validation.Invalid<int, Error>(new ValidationError { Message = "Age must be between 0 and 150" })
            : Validation.Valid<int, Error>(age);

    private static Validation<string, Error> ValidateEmail(string email) =>
        !email.Contains('@')
            ? Validation.Invalid<string, Error>(new ValidationError { Message = "Email must contain @" })
            : Validation.Valid<string, Error>(email);

    private static Task<Validation<string, Error>> ValidateNameAsync(string name) =>
        Task.FromResult(ValidateName(name));

    private static Task<Validation<int, Error>> ValidateAgeAsync(int age) =>
        Task.FromResult(ValidateAge(age));

    private static Task<Validation<string, Error>> ValidateEmailAsync(string email) =>
        Task.FromResult(ValidateEmail(email));

    #region Sync — Single step (arity 1)

    [Test]
    public async Task Single_step_valid_returns_valid()
    {
        var pipeline = ValidationPipeline.Create<string, Error>()
            .Validate(input => ValidateName(input))
            .Build();

        var result = pipeline("Alice");

        await Assert.That(result.IsValid).IsTrue();
        await Assert.That(result.GetValueOrThrow()).IsEqualTo("Alice");
    }

    [Test]
    public async Task Single_step_invalid_returns_invalid()
    {
        var pipeline = ValidationPipeline.Create<string, Error>()
            .Validate(input => ValidateName(input))
            .Build();

        var result = pipeline("");

        await Assert.That(result.IsInvalid).IsTrue();
        var errors = result.Match(_ => Array.Empty<Error>(), errs => errs.ToArray());
        await Assert.That(errors).Count().IsEqualTo(1);
    }

    #endregion

    #region Sync — Two steps (arity 2)

    [Test]
    public async Task Two_steps_all_valid_produces_valid_result()
    {
        var pipeline = ValidationPipeline.Create<UserDto, Error>()
            .Validate(dto => ValidateName(dto.Name))
            .Validate(dto => ValidateAge(dto.Age))
            .Build((name, age) => (name, age));

        var result = pipeline(new UserDto("Alice", 30, "alice@test.com"));

        await Assert.That(result.IsValid).IsTrue();
        var value = result.GetValueOrThrow();
        await Assert.That(value.name).IsEqualTo("Alice");
        await Assert.That(value.age).IsEqualTo(30);
    }

    [Test]
    public async Task Two_steps_first_invalid_accumulates_error()
    {
        var pipeline = ValidationPipeline.Create<UserDto, Error>()
            .Validate(dto => ValidateName(dto.Name))
            .Validate(dto => ValidateAge(dto.Age))
            .Build((name, age) => (name, age));

        var result = pipeline(new UserDto("", 30, "alice@test.com"));

        await Assert.That(result.IsInvalid).IsTrue();
        var errors = result.Match(_ => Array.Empty<Error>(), errs => errs.ToArray());
        await Assert.That(errors).Count().IsEqualTo(1);
        await Assert.That(errors[0].Message).IsEqualTo("Name is required");
    }

    [Test]
    public async Task Two_steps_both_invalid_accumulates_all_errors()
    {
        var pipeline = ValidationPipeline.Create<UserDto, Error>()
            .Validate(dto => ValidateName(dto.Name))
            .Validate(dto => ValidateAge(dto.Age))
            .Build((name, age) => (name, age));

        var result = pipeline(new UserDto("", -1, "alice@test.com"));

        await Assert.That(result.IsInvalid).IsTrue();
        var errors = result.Match(_ => Array.Empty<Error>(), errs => errs.ToArray());
        await Assert.That(errors).Count().IsEqualTo(2);
    }

    #endregion

    #region Sync — Three steps (arity 3)

    [Test]
    public async Task Three_steps_all_valid_produces_valid_result()
    {
        var pipeline = ValidationPipeline.Create<UserDto, Error>()
            .Validate(dto => ValidateName(dto.Name))
            .Validate(dto => ValidateAge(dto.Age))
            .Validate(dto => ValidateEmail(dto.Email))
            .Build((name, age, email) => new User(name, age, email));

        var result = pipeline(new UserDto("Alice", 30, "alice@test.com"));

        await Assert.That(result.IsValid).IsTrue();
        var user = result.GetValueOrThrow();
        await Assert.That(user.Name).IsEqualTo("Alice");
        await Assert.That(user.Age).IsEqualTo(30);
        await Assert.That(user.Email).IsEqualTo("alice@test.com");
    }

    [Test]
    public async Task Three_steps_all_invalid_accumulates_all_errors()
    {
        var pipeline = ValidationPipeline.Create<UserDto, Error>()
            .Validate(dto => ValidateName(dto.Name))
            .Validate(dto => ValidateAge(dto.Age))
            .Validate(dto => ValidateEmail(dto.Email))
            .Build((name, age, email) => new User(name, age, email));

        var result = pipeline(new UserDto("", -1, "bademail"));

        await Assert.That(result.IsInvalid).IsTrue();
        var errors = result.Match(_ => Array.Empty<Error>(), errs => errs.ToArray());
        await Assert.That(errors).Count().IsEqualTo(3);
    }

    #endregion

    #region Sync — Four steps (arity 4)

    [Test]
    public async Task Four_steps_all_valid_produces_valid_result()
    {
        var pipeline = ValidationPipeline.Create<(string A, string B, string C, string D), Error>()
            .Validate(x => ValidateName(x.A))
            .Validate(x => ValidateName(x.B))
            .Validate(x => ValidateName(x.C))
            .Validate(x => ValidateName(x.D))
            .Build((a, b, c, d) => $"{a},{b},{c},{d}");

        var result = pipeline(("A", "B", "C", "D"));

        await Assert.That(result.IsValid).IsTrue();
        await Assert.That(result.GetValueOrThrow()).IsEqualTo("A,B,C,D");
    }

    [Test]
    public async Task Four_steps_all_invalid_accumulates_all_errors()
    {
        var pipeline = ValidationPipeline.Create<(string A, string B, string C, string D), Error>()
            .Validate(x => ValidateName(x.A))
            .Validate(x => ValidateName(x.B))
            .Validate(x => ValidateName(x.C))
            .Validate(x => ValidateName(x.D))
            .Build((a, b, c, d) => $"{a},{b},{c},{d}");

        var result = pipeline(("", "", "", ""));

        await Assert.That(result.IsInvalid).IsTrue();
        var errors = result.Match(_ => Array.Empty<Error>(), errs => errs.ToArray());
        await Assert.That(errors).Count().IsEqualTo(4);
    }

    #endregion

    #region Sync — Five steps (arity 5)

    [Test]
    public async Task Five_steps_all_valid_produces_valid_result()
    {
        var pipeline = ValidationPipeline.Create<(string A, string B, string C, string D, string E), Error>()
            .Validate(x => ValidateName(x.A))
            .Validate(x => ValidateName(x.B))
            .Validate(x => ValidateName(x.C))
            .Validate(x => ValidateName(x.D))
            .Validate(x => ValidateName(x.E))
            .Build((a, b, c, d, e) => $"{a},{b},{c},{d},{e}");

        var result = pipeline(("A", "B", "C", "D", "E"));

        await Assert.That(result.IsValid).IsTrue();
        await Assert.That(result.GetValueOrThrow()).IsEqualTo("A,B,C,D,E");
    }

    [Test]
    public async Task Five_steps_all_invalid_accumulates_all_errors()
    {
        var pipeline = ValidationPipeline.Create<(string A, string B, string C, string D, string E), Error>()
            .Validate(x => ValidateName(x.A))
            .Validate(x => ValidateName(x.B))
            .Validate(x => ValidateName(x.C))
            .Validate(x => ValidateName(x.D))
            .Validate(x => ValidateName(x.E))
            .Build((a, b, c, d, e) => $"{a},{b},{c},{d},{e}");

        var result = pipeline(("", "", "", "", ""));

        await Assert.That(result.IsInvalid).IsTrue();
        var errors = result.Match(_ => Array.Empty<Error>(), errs => errs.ToArray());
        await Assert.That(errors).Count().IsEqualTo(5);
    }

    #endregion

    #region Sync — Six steps (arity 6)

    [Test]
    public async Task Six_steps_all_valid_produces_valid_result()
    {
        var pipeline = ValidationPipeline.Create<(string A, string B, string C, string D, string E, string F), Error>()
            .Validate(x => ValidateName(x.A))
            .Validate(x => ValidateName(x.B))
            .Validate(x => ValidateName(x.C))
            .Validate(x => ValidateName(x.D))
            .Validate(x => ValidateName(x.E))
            .Validate(x => ValidateName(x.F))
            .Build((a, b, c, d, e, f) => $"{a},{b},{c},{d},{e},{f}");

        var result = pipeline(("A", "B", "C", "D", "E", "F"));

        await Assert.That(result.IsValid).IsTrue();
        await Assert.That(result.GetValueOrThrow()).IsEqualTo("A,B,C,D,E,F");
    }

    [Test]
    public async Task Six_steps_all_invalid_accumulates_all_errors()
    {
        var pipeline = ValidationPipeline.Create<(string A, string B, string C, string D, string E, string F), Error>()
            .Validate(x => ValidateName(x.A))
            .Validate(x => ValidateName(x.B))
            .Validate(x => ValidateName(x.C))
            .Validate(x => ValidateName(x.D))
            .Validate(x => ValidateName(x.E))
            .Validate(x => ValidateName(x.F))
            .Build((a, b, c, d, e, f) => $"{a},{b},{c},{d},{e},{f}");

        var result = pipeline(("", "", "", "", "", ""));

        await Assert.That(result.IsInvalid).IsTrue();
        var errors = result.Match(_ => Array.Empty<Error>(), errs => errs.ToArray());
        await Assert.That(errors).Count().IsEqualTo(6);
    }

    #endregion

    #region Sync — Seven steps (arity 7)

    [Test]
    public async Task Seven_steps_all_valid_produces_valid_result()
    {
        var pipeline = ValidationPipeline.Create<(string A, string B, string C, string D, string E, string F, string G), Error>()
            .Validate(x => ValidateName(x.A))
            .Validate(x => ValidateName(x.B))
            .Validate(x => ValidateName(x.C))
            .Validate(x => ValidateName(x.D))
            .Validate(x => ValidateName(x.E))
            .Validate(x => ValidateName(x.F))
            .Validate(x => ValidateName(x.G))
            .Build((a, b, c, d, e, f, g) => $"{a},{b},{c},{d},{e},{f},{g}");

        var result = pipeline(("A", "B", "C", "D", "E", "F", "G"));

        await Assert.That(result.IsValid).IsTrue();
        await Assert.That(result.GetValueOrThrow()).IsEqualTo("A,B,C,D,E,F,G");
    }

    [Test]
    public async Task Seven_steps_all_invalid_accumulates_all_errors()
    {
        var pipeline = ValidationPipeline.Create<(string A, string B, string C, string D, string E, string F, string G), Error>()
            .Validate(x => ValidateName(x.A))
            .Validate(x => ValidateName(x.B))
            .Validate(x => ValidateName(x.C))
            .Validate(x => ValidateName(x.D))
            .Validate(x => ValidateName(x.E))
            .Validate(x => ValidateName(x.F))
            .Validate(x => ValidateName(x.G))
            .Build((a, b, c, d, e, f, g) => $"{a},{b},{c},{d},{e},{f},{g}");

        var result = pipeline(("", "", "", "", "", "", ""));

        await Assert.That(result.IsInvalid).IsTrue();
        var errors = result.Match(_ => Array.Empty<Error>(), errs => errs.ToArray());
        await Assert.That(errors).Count().IsEqualTo(7);
    }

    #endregion

    #region Sync — Eight steps (arity 8)

    [Test]
    public async Task Eight_steps_all_valid_produces_valid_result()
    {
        var pipeline = ValidationPipeline.Create<(string A, string B, string C, string D, string E, string F, string G, string H), Error>()
            .Validate(x => ValidateName(x.A))
            .Validate(x => ValidateName(x.B))
            .Validate(x => ValidateName(x.C))
            .Validate(x => ValidateName(x.D))
            .Validate(x => ValidateName(x.E))
            .Validate(x => ValidateName(x.F))
            .Validate(x => ValidateName(x.G))
            .Validate(x => ValidateName(x.H))
            .Build((a, b, c, d, e, f, g, h) => $"{a},{b},{c},{d},{e},{f},{g},{h}");

        var result = pipeline(("A", "B", "C", "D", "E", "F", "G", "H"));

        await Assert.That(result.IsValid).IsTrue();
        await Assert.That(result.GetValueOrThrow()).IsEqualTo("A,B,C,D,E,F,G,H");
    }

    [Test]
    public async Task Eight_steps_all_invalid_accumulates_all_errors()
    {
        var pipeline = ValidationPipeline.Create<(string A, string B, string C, string D, string E, string F, string G, string H), Error>()
            .Validate(x => ValidateName(x.A))
            .Validate(x => ValidateName(x.B))
            .Validate(x => ValidateName(x.C))
            .Validate(x => ValidateName(x.D))
            .Validate(x => ValidateName(x.E))
            .Validate(x => ValidateName(x.F))
            .Validate(x => ValidateName(x.G))
            .Validate(x => ValidateName(x.H))
            .Build((a, b, c, d, e, f, g, h) => $"{a},{b},{c},{d},{e},{f},{g},{h}");

        var result = pipeline(("", "", "", "", "", "", "", ""));

        await Assert.That(result.IsInvalid).IsTrue();
        var errors = result.Match(_ => Array.Empty<Error>(), errs => errs.ToArray());
        await Assert.That(errors).Count().IsEqualTo(8);
    }

    #endregion

    #region Sync — Plain mapping steps

    [Test]
    public async Task Plain_mapping_step_auto_wraps_in_valid()
    {
        var pipeline = ValidationPipeline.Create<string, Error>()
            .Validate(input => input.Trim())
            .Build();

        var result = pipeline("  hello  ");

        await Assert.That(result.IsValid).IsTrue();
        await Assert.That(result.GetValueOrThrow()).IsEqualTo("hello");
    }

    [Test]
    public async Task Plain_mapping_step_combined_with_validation_step()
    {
        var pipeline = ValidationPipeline.Create<UserDto, Error>()
            .Validate(dto => dto.Name.Trim())
            .Validate(dto => ValidateAge(dto.Age))
            .Build((name, age) => (name, age));

        var result = pipeline(new UserDto("  Alice  ", 30, "test@test.com"));

        await Assert.That(result.IsValid).IsTrue();
        var value = result.GetValueOrThrow();
        await Assert.That(value.name).IsEqualTo("Alice");
        await Assert.That(value.age).IsEqualTo(30);
    }

    [Test]
    public async Task Plain_mapping_step_combined_with_invalid_validation_step()
    {
        var pipeline = ValidationPipeline.Create<UserDto, Error>()
            .Validate(dto => dto.Name.Trim())
            .Validate(dto => ValidateAge(dto.Age))
            .Build((name, age) => (name, age));

        var result = pipeline(new UserDto("Alice", -1, "test@test.com"));

        await Assert.That(result.IsInvalid).IsTrue();
        var errors = result.Match(_ => Array.Empty<Error>(), errs => errs.ToArray());
        await Assert.That(errors).Count().IsEqualTo(1);
        await Assert.That(errors[0].Message).IsEqualTo("Age must be between 0 and 150");
    }

    #endregion

    #region Sync — Mixed valid/invalid

    [Test]
    public async Task Mixed_valid_and_invalid_accumulates_only_invalid_errors()
    {
        var pipeline = ValidationPipeline.Create<UserDto, Error>()
            .Validate(dto => ValidateName(dto.Name))
            .Validate(dto => ValidateAge(dto.Age))
            .Validate(dto => ValidateEmail(dto.Email))
            .Build((name, age, email) => new User(name, age, email));

        // Name is valid, Age is invalid, Email is invalid
        var result = pipeline(new UserDto("Alice", -1, "bademail"));

        await Assert.That(result.IsInvalid).IsTrue();
        var errors = result.Match(_ => Array.Empty<Error>(), errs => errs.ToArray());
        await Assert.That(errors).Count().IsEqualTo(2);
    }

    #endregion

    #region Sync — Reusability

    [Test]
    public async Task Pipeline_is_reusable_across_multiple_invocations()
    {
        var pipeline = ValidationPipeline.Create<UserDto, Error>()
            .Validate(dto => ValidateName(dto.Name))
            .Validate(dto => ValidateAge(dto.Age))
            .Build((name, age) => (name, age));

        var validResult = pipeline(new UserDto("Alice", 30, ""));
        var invalidResult = pipeline(new UserDto("", -1, ""));

        await Assert.That(validResult.IsValid).IsTrue();
        await Assert.That(invalidResult.IsInvalid).IsTrue();

        var errors = invalidResult.Match(_ => Array.Empty<Error>(), errs => errs.ToArray());
        await Assert.That(errors).Count().IsEqualTo(2);
    }

    #endregion

    #region Sync — Different output types per step

    [Test]
    public async Task Steps_can_produce_different_types()
    {
        var pipeline = ValidationPipeline.Create<UserDto, Error>()
            .Validate(dto => ValidateName(dto.Name))     // string
            .Validate(dto => ValidateAge(dto.Age))        // int
            .Validate(dto => ValidateEmail(dto.Email))    // string
            .Build((name, age, email) => new User(name, age, email));

        var result = pipeline(new UserDto("Alice", 30, "alice@test.com"));

        await Assert.That(result.IsValid).IsTrue();
        var user = result.GetValueOrThrow();
        await Assert.That(user.Name).IsEqualTo("Alice");
        await Assert.That(user.Age).IsEqualTo(30);
        await Assert.That(user.Email).IsEqualTo("alice@test.com");
    }

    #endregion

    #region Async — Single step (arity 1)

    [Test]
    public async Task Async_single_step_valid_returns_valid()
    {
        var pipeline = ValidationPipeline.Create<string, Error>()
            .ValidateAsync(input => ValidateNameAsync(input))
            .Build();

        var result = await pipeline("Alice");

        await Assert.That(result.IsValid).IsTrue();
        await Assert.That(result.GetValueOrThrow()).IsEqualTo("Alice");
    }

    [Test]
    public async Task Async_single_step_invalid_returns_invalid()
    {
        var pipeline = ValidationPipeline.Create<string, Error>()
            .ValidateAsync(input => ValidateNameAsync(input))
            .Build();

        var result = await pipeline("");

        await Assert.That(result.IsInvalid).IsTrue();
    }

    #endregion

    #region Async — Two steps (arity 2)

    [Test]
    public async Task Async_two_steps_all_valid_produces_valid_result()
    {
        var pipeline = ValidationPipeline.Create<UserDto, Error>()
            .ValidateAsync(dto => ValidateNameAsync(dto.Name))
            .ValidateAsync(dto => ValidateAgeAsync(dto.Age))
            .Build((name, age) => (name, age));

        var result = await pipeline(new UserDto("Alice", 30, "alice@test.com"));

        await Assert.That(result.IsValid).IsTrue();
        var value = result.GetValueOrThrow();
        await Assert.That(value.name).IsEqualTo("Alice");
        await Assert.That(value.age).IsEqualTo(30);
    }

    [Test]
    public async Task Async_two_steps_both_invalid_accumulates_all_errors()
    {
        var pipeline = ValidationPipeline.Create<UserDto, Error>()
            .ValidateAsync(dto => ValidateNameAsync(dto.Name))
            .ValidateAsync(dto => ValidateAgeAsync(dto.Age))
            .Build((name, age) => (name, age));

        var result = await pipeline(new UserDto("", -1, "alice@test.com"));

        await Assert.That(result.IsInvalid).IsTrue();
        var errors = result.Match(_ => Array.Empty<Error>(), errs => errs.ToArray());
        await Assert.That(errors).Count().IsEqualTo(2);
    }

    #endregion

    #region Async — Three steps (arity 3)

    [Test]
    public async Task Async_three_steps_all_valid_produces_valid_result()
    {
        var pipeline = ValidationPipeline.Create<UserDto, Error>()
            .ValidateAsync(dto => ValidateNameAsync(dto.Name))
            .ValidateAsync(dto => ValidateAgeAsync(dto.Age))
            .ValidateAsync(dto => ValidateEmailAsync(dto.Email))
            .Build((name, age, email) => new User(name, age, email));

        var result = await pipeline(new UserDto("Alice", 30, "alice@test.com"));

        await Assert.That(result.IsValid).IsTrue();
        var user = result.GetValueOrThrow();
        await Assert.That(user.Name).IsEqualTo("Alice");
    }

    [Test]
    public async Task Async_three_steps_all_invalid_accumulates_all_errors()
    {
        var pipeline = ValidationPipeline.Create<UserDto, Error>()
            .ValidateAsync(dto => ValidateNameAsync(dto.Name))
            .ValidateAsync(dto => ValidateAgeAsync(dto.Age))
            .ValidateAsync(dto => ValidateEmailAsync(dto.Email))
            .Build((name, age, email) => new User(name, age, email));

        var result = await pipeline(new UserDto("", -1, "bademail"));

        await Assert.That(result.IsInvalid).IsTrue();
        var errors = result.Match(_ => Array.Empty<Error>(), errs => errs.ToArray());
        await Assert.That(errors).Count().IsEqualTo(3);
    }

    #endregion

    #region Async — Four steps (arity 4)

    [Test]
    public async Task Async_four_steps_all_valid_produces_valid_result()
    {
        var pipeline = ValidationPipeline.Create<(string A, string B, string C, string D), Error>()
            .ValidateAsync(x => ValidateNameAsync(x.A))
            .ValidateAsync(x => ValidateNameAsync(x.B))
            .ValidateAsync(x => ValidateNameAsync(x.C))
            .ValidateAsync(x => ValidateNameAsync(x.D))
            .Build((a, b, c, d) => $"{a},{b},{c},{d}");

        var result = await pipeline(("A", "B", "C", "D"));

        await Assert.That(result.IsValid).IsTrue();
        await Assert.That(result.GetValueOrThrow()).IsEqualTo("A,B,C,D");
    }

    [Test]
    public async Task Async_four_steps_all_invalid_accumulates_all_errors()
    {
        var pipeline = ValidationPipeline.Create<(string A, string B, string C, string D), Error>()
            .ValidateAsync(x => ValidateNameAsync(x.A))
            .ValidateAsync(x => ValidateNameAsync(x.B))
            .ValidateAsync(x => ValidateNameAsync(x.C))
            .ValidateAsync(x => ValidateNameAsync(x.D))
            .Build((a, b, c, d) => $"{a},{b},{c},{d}");

        var result = await pipeline(("", "", "", ""));

        await Assert.That(result.IsInvalid).IsTrue();
        var errors = result.Match(_ => Array.Empty<Error>(), errs => errs.ToArray());
        await Assert.That(errors).Count().IsEqualTo(4);
    }

    #endregion

    #region Async — Five steps (arity 5)

    [Test]
    public async Task Async_five_steps_all_valid_produces_valid_result()
    {
        var pipeline = ValidationPipeline.Create<(string A, string B, string C, string D, string E), Error>()
            .ValidateAsync(x => ValidateNameAsync(x.A))
            .ValidateAsync(x => ValidateNameAsync(x.B))
            .ValidateAsync(x => ValidateNameAsync(x.C))
            .ValidateAsync(x => ValidateNameAsync(x.D))
            .ValidateAsync(x => ValidateNameAsync(x.E))
            .Build((a, b, c, d, e) => $"{a},{b},{c},{d},{e}");

        var result = await pipeline(("A", "B", "C", "D", "E"));

        await Assert.That(result.IsValid).IsTrue();
        await Assert.That(result.GetValueOrThrow()).IsEqualTo("A,B,C,D,E");
    }

    [Test]
    public async Task Async_five_steps_all_invalid_accumulates_all_errors()
    {
        var pipeline = ValidationPipeline.Create<(string A, string B, string C, string D, string E), Error>()
            .ValidateAsync(x => ValidateNameAsync(x.A))
            .ValidateAsync(x => ValidateNameAsync(x.B))
            .ValidateAsync(x => ValidateNameAsync(x.C))
            .ValidateAsync(x => ValidateNameAsync(x.D))
            .ValidateAsync(x => ValidateNameAsync(x.E))
            .Build((a, b, c, d, e) => $"{a},{b},{c},{d},{e}");

        var result = await pipeline(("", "", "", "", ""));

        await Assert.That(result.IsInvalid).IsTrue();
        var errors = result.Match(_ => Array.Empty<Error>(), errs => errs.ToArray());
        await Assert.That(errors).Count().IsEqualTo(5);
    }

    #endregion

    #region Async — Six steps (arity 6)

    [Test]
    public async Task Async_six_steps_all_valid_produces_valid_result()
    {
        var pipeline = ValidationPipeline.Create<(string A, string B, string C, string D, string E, string F), Error>()
            .ValidateAsync(x => ValidateNameAsync(x.A))
            .ValidateAsync(x => ValidateNameAsync(x.B))
            .ValidateAsync(x => ValidateNameAsync(x.C))
            .ValidateAsync(x => ValidateNameAsync(x.D))
            .ValidateAsync(x => ValidateNameAsync(x.E))
            .ValidateAsync(x => ValidateNameAsync(x.F))
            .Build((a, b, c, d, e, f) => $"{a},{b},{c},{d},{e},{f}");

        var result = await pipeline(("A", "B", "C", "D", "E", "F"));

        await Assert.That(result.IsValid).IsTrue();
        await Assert.That(result.GetValueOrThrow()).IsEqualTo("A,B,C,D,E,F");
    }

    [Test]
    public async Task Async_six_steps_all_invalid_accumulates_all_errors()
    {
        var pipeline = ValidationPipeline.Create<(string A, string B, string C, string D, string E, string F), Error>()
            .ValidateAsync(x => ValidateNameAsync(x.A))
            .ValidateAsync(x => ValidateNameAsync(x.B))
            .ValidateAsync(x => ValidateNameAsync(x.C))
            .ValidateAsync(x => ValidateNameAsync(x.D))
            .ValidateAsync(x => ValidateNameAsync(x.E))
            .ValidateAsync(x => ValidateNameAsync(x.F))
            .Build((a, b, c, d, e, f) => $"{a},{b},{c},{d},{e},{f}");

        var result = await pipeline(("", "", "", "", "", ""));

        await Assert.That(result.IsInvalid).IsTrue();
        var errors = result.Match(_ => Array.Empty<Error>(), errs => errs.ToArray());
        await Assert.That(errors).Count().IsEqualTo(6);
    }

    #endregion

    #region Async — Seven steps (arity 7)

    [Test]
    public async Task Async_seven_steps_all_valid_produces_valid_result()
    {
        var pipeline = ValidationPipeline.Create<(string A, string B, string C, string D, string E, string F, string G), Error>()
            .ValidateAsync(x => ValidateNameAsync(x.A))
            .ValidateAsync(x => ValidateNameAsync(x.B))
            .ValidateAsync(x => ValidateNameAsync(x.C))
            .ValidateAsync(x => ValidateNameAsync(x.D))
            .ValidateAsync(x => ValidateNameAsync(x.E))
            .ValidateAsync(x => ValidateNameAsync(x.F))
            .ValidateAsync(x => ValidateNameAsync(x.G))
            .Build((a, b, c, d, e, f, g) => $"{a},{b},{c},{d},{e},{f},{g}");

        var result = await pipeline(("A", "B", "C", "D", "E", "F", "G"));

        await Assert.That(result.IsValid).IsTrue();
        await Assert.That(result.GetValueOrThrow()).IsEqualTo("A,B,C,D,E,F,G");
    }

    [Test]
    public async Task Async_seven_steps_all_invalid_accumulates_all_errors()
    {
        var pipeline = ValidationPipeline.Create<(string A, string B, string C, string D, string E, string F, string G), Error>()
            .ValidateAsync(x => ValidateNameAsync(x.A))
            .ValidateAsync(x => ValidateNameAsync(x.B))
            .ValidateAsync(x => ValidateNameAsync(x.C))
            .ValidateAsync(x => ValidateNameAsync(x.D))
            .ValidateAsync(x => ValidateNameAsync(x.E))
            .ValidateAsync(x => ValidateNameAsync(x.F))
            .ValidateAsync(x => ValidateNameAsync(x.G))
            .Build((a, b, c, d, e, f, g) => $"{a},{b},{c},{d},{e},{f},{g}");

        var result = await pipeline(("", "", "", "", "", "", ""));

        await Assert.That(result.IsInvalid).IsTrue();
        var errors = result.Match(_ => Array.Empty<Error>(), errs => errs.ToArray());
        await Assert.That(errors).Count().IsEqualTo(7);
    }

    #endregion

    #region Async — Eight steps (arity 8)

    [Test]
    public async Task Async_eight_steps_all_valid_produces_valid_result()
    {
        var pipeline = ValidationPipeline.Create<(string A, string B, string C, string D, string E, string F, string G, string H), Error>()
            .ValidateAsync(x => ValidateNameAsync(x.A))
            .ValidateAsync(x => ValidateNameAsync(x.B))
            .ValidateAsync(x => ValidateNameAsync(x.C))
            .ValidateAsync(x => ValidateNameAsync(x.D))
            .ValidateAsync(x => ValidateNameAsync(x.E))
            .ValidateAsync(x => ValidateNameAsync(x.F))
            .ValidateAsync(x => ValidateNameAsync(x.G))
            .ValidateAsync(x => ValidateNameAsync(x.H))
            .Build((a, b, c, d, e, f, g, h) => $"{a},{b},{c},{d},{e},{f},{g},{h}");

        var result = await pipeline(("A", "B", "C", "D", "E", "F", "G", "H"));

        await Assert.That(result.IsValid).IsTrue();
        await Assert.That(result.GetValueOrThrow()).IsEqualTo("A,B,C,D,E,F,G,H");
    }

    [Test]
    public async Task Async_eight_steps_all_invalid_accumulates_all_errors()
    {
        var pipeline = ValidationPipeline.Create<(string A, string B, string C, string D, string E, string F, string G, string H), Error>()
            .ValidateAsync(x => ValidateNameAsync(x.A))
            .ValidateAsync(x => ValidateNameAsync(x.B))
            .ValidateAsync(x => ValidateNameAsync(x.C))
            .ValidateAsync(x => ValidateNameAsync(x.D))
            .ValidateAsync(x => ValidateNameAsync(x.E))
            .ValidateAsync(x => ValidateNameAsync(x.F))
            .ValidateAsync(x => ValidateNameAsync(x.G))
            .ValidateAsync(x => ValidateNameAsync(x.H))
            .Build((a, b, c, d, e, f, g, h) => $"{a},{b},{c},{d},{e},{f},{g},{h}");

        var result = await pipeline(("", "", "", "", "", "", "", ""));

        await Assert.That(result.IsInvalid).IsTrue();
        var errors = result.Match(_ => Array.Empty<Error>(), errs => errs.ToArray());
        await Assert.That(errors).Count().IsEqualTo(8);
    }

    #endregion

    #region Mixed sync/async steps

    [Test]
    public async Task Mixed_sync_then_async_steps_accumulate_errors()
    {
        var pipeline = ValidationPipeline.Create<UserDto, Error>()
            .Validate(dto => ValidateName(dto.Name))
            .ValidateAsync(dto => ValidateAgeAsync(dto.Age))
            .Build((name, age) => (name, age));

        var result = await pipeline(new UserDto("", -1, ""));

        await Assert.That(result.IsInvalid).IsTrue();
        var errors = result.Match(_ => Array.Empty<Error>(), errs => errs.ToArray());
        await Assert.That(errors).Count().IsEqualTo(2);
    }

    [Test]
    public async Task Mixed_sync_then_async_steps_valid_produces_result()
    {
        var pipeline = ValidationPipeline.Create<UserDto, Error>()
            .Validate(dto => ValidateName(dto.Name))
            .ValidateAsync(dto => ValidateAgeAsync(dto.Age))
            .Build((name, age) => (name, age));

        var result = await pipeline(new UserDto("Alice", 30, ""));

        await Assert.That(result.IsValid).IsTrue();
        var value = result.GetValueOrThrow();
        await Assert.That(value.name).IsEqualTo("Alice");
        await Assert.That(value.age).IsEqualTo(30);
    }

    [Test]
    public async Task Mixed_sync_async_sync_steps_three_arity()
    {
        var pipeline = ValidationPipeline.Create<UserDto, Error>()
            .Validate(dto => ValidateName(dto.Name))
            .ValidateAsync(dto => ValidateAgeAsync(dto.Age))
            .Validate(dto => ValidateEmail(dto.Email))
            .Build((name, age, email) => new User(name, age, email));

        var result = await pipeline(new UserDto("Alice", 30, "alice@test.com"));

        await Assert.That(result.IsValid).IsTrue();
        var user = result.GetValueOrThrow();
        await Assert.That(user.Name).IsEqualTo("Alice");
    }

    [Test]
    public async Task Mixed_steps_all_invalid_accumulates_errors()
    {
        var pipeline = ValidationPipeline.Create<UserDto, Error>()
            .Validate(dto => ValidateName(dto.Name))
            .ValidateAsync(dto => ValidateAgeAsync(dto.Age))
            .Validate(dto => ValidateEmail(dto.Email))
            .Build((name, age, email) => new User(name, age, email));

        var result = await pipeline(new UserDto("", -1, "bademail"));

        await Assert.That(result.IsInvalid).IsTrue();
        var errors = result.Match(_ => Array.Empty<Error>(), errs => errs.ToArray());
        await Assert.That(errors).Count().IsEqualTo(3);
    }

    [Test]
    public async Task Async_pipeline_with_sync_validate_added_after()
    {
        var pipeline = ValidationPipeline.Create<UserDto, Error>()
            .ValidateAsync(dto => ValidateNameAsync(dto.Name))
            .Validate(dto => ValidateAge(dto.Age))
            .Build((name, age) => (name, age));

        var result = await pipeline(new UserDto("Alice", 30, ""));

        await Assert.That(result.IsValid).IsTrue();
        var value = result.GetValueOrThrow();
        await Assert.That(value.name).IsEqualTo("Alice");
        await Assert.That(value.age).IsEqualTo(30);
    }

    [Test]
    public async Task Async_pipeline_with_plain_mapping_step()
    {
        var pipeline = ValidationPipeline.Create<UserDto, Error>()
            .ValidateAsync(dto => ValidateNameAsync(dto.Name))
            .Validate(dto => dto.Age)
            .Build((name, age) => (name, age));

        var result = await pipeline(new UserDto("Alice", 30, ""));

        await Assert.That(result.IsValid).IsTrue();
        var value = result.GetValueOrThrow();
        await Assert.That(value.name).IsEqualTo("Alice");
        await Assert.That(value.age).IsEqualTo(30);
    }

    #endregion

    #region Async — Concurrency

    [Test]
    public async Task Async_steps_execute_concurrently()
    {
        var tcs1 = new TaskCompletionSource<Validation<string, Error>>();
        var tcs2 = new TaskCompletionSource<Validation<int, Error>>();
        var step1Started = new TaskCompletionSource<bool>();
        var step2Started = new TaskCompletionSource<bool>();

        var pipeline = ValidationPipeline.Create<UserDto, Error>()
            .ValidateAsync(async dto =>
            {
                step1Started.SetResult(true);
                return await tcs1.Task;
            })
            .ValidateAsync(async dto =>
            {
                step2Started.SetResult(true);
                return await tcs2.Task;
            })
            .Build((name, age) => (name, age));

        var pipelineTask = pipeline(new UserDto("Alice", 30, ""));

        // Both steps should start before either completes
        await Task.WhenAll(
            step1Started.Task.WaitAsync(TimeSpan.FromSeconds(5)),
            step2Started.Task.WaitAsync(TimeSpan.FromSeconds(5)));

        await Assert.That(step1Started.Task.IsCompleted).IsTrue();
        await Assert.That(step2Started.Task.IsCompleted).IsTrue();

        // Complete both
        tcs1.SetResult(Validation.Valid<string, Error>("Alice"));
        tcs2.SetResult(Validation.Valid<int, Error>(30));

        var result = await pipelineTask;
        await Assert.That(result.IsValid).IsTrue();
    }

    [Test]
    public async Task Async_steps_execute_concurrently_and_accumulate_errors()
    {
        var tcs1 = new TaskCompletionSource<Validation<string, Error>>();
        var tcs2 = new TaskCompletionSource<Validation<int, Error>>();

        var pipeline = ValidationPipeline.Create<UserDto, Error>()
            .ValidateAsync(async _ => await tcs1.Task)
            .ValidateAsync(async _ => await tcs2.Task)
            .Build((name, age) => (name, age));

        var pipelineTask = pipeline(new UserDto("", -1, ""));

        // Both fail
        tcs1.SetResult(Validation.Invalid<string, Error>(new ValidationError { Message = "Error 1" }));
        tcs2.SetResult(Validation.Invalid<int, Error>(new ValidationError { Message = "Error 2" }));

        var result = await pipelineTask;
        await Assert.That(result.IsInvalid).IsTrue();
        var errors = result.Match(_ => Array.Empty<Error>(), errs => errs.ToArray());
        await Assert.That(errors).Count().IsEqualTo(2);
    }

    #endregion

    #region Async — Reusability

    [Test]
    public async Task Async_pipeline_is_reusable_across_multiple_invocations()
    {
        var pipeline = ValidationPipeline.Create<UserDto, Error>()
            .ValidateAsync(dto => ValidateNameAsync(dto.Name))
            .ValidateAsync(dto => ValidateAgeAsync(dto.Age))
            .Build((name, age) => (name, age));

        var validResult = await pipeline(new UserDto("Alice", 30, ""));
        var invalidResult = await pipeline(new UserDto("", -1, ""));

        await Assert.That(validResult.IsValid).IsTrue();
        await Assert.That(invalidResult.IsInvalid).IsTrue();
        var errors = invalidResult.Match(_ => Array.Empty<Error>(), errs => errs.ToArray());
        await Assert.That(errors).Count().IsEqualTo(2);
    }

    #endregion

    #region Transition from sync to async

    [Test]
    public async Task Sync_pipeline_transitions_to_async_on_first_async_validate()
    {
        // Start sync, add async — should produce async pipeline
        var pipeline = ValidationPipeline.Create<UserDto, Error>()
            .Validate(dto => ValidateName(dto.Name))
            .Validate(dto => ValidateAge(dto.Age))
            .ValidateAsync(dto => ValidateEmailAsync(dto.Email))
            .Build((name, age, email) => new User(name, age, email));

        // Return type is Func<UserDto, Task<Validation<User, Error>>>
        var result = await pipeline(new UserDto("Alice", 30, "alice@test.com"));

        await Assert.That(result.IsValid).IsTrue();
        var user = result.GetValueOrThrow();
        await Assert.That(user.Name).IsEqualTo("Alice");
        await Assert.That(user.Age).IsEqualTo(30);
        await Assert.That(user.Email).IsEqualTo("alice@test.com");
    }

    [Test]
    public async Task Sync_pipeline_transitions_to_async_errors_accumulate()
    {
        var pipeline = ValidationPipeline.Create<UserDto, Error>()
            .Validate(dto => ValidateName(dto.Name))
            .Validate(dto => ValidateAge(dto.Age))
            .ValidateAsync(dto => ValidateEmailAsync(dto.Email))
            .Build((name, age, email) => new User(name, age, email));

        var result = await pipeline(new UserDto("", -1, "bademail"));

        await Assert.That(result.IsInvalid).IsTrue();
        var errors = result.Match(_ => Array.Empty<Error>(), errs => errs.ToArray());
        await Assert.That(errors).Count().IsEqualTo(3);
    }

    #endregion

    #region Start with ValidateAsync

    [Test]
    public async Task Start_with_async_validate_followed_by_sync_validate()
    {
        var pipeline = ValidationPipeline.Create<UserDto, Error>()
            .ValidateAsync(dto => ValidateNameAsync(dto.Name))
            .Validate(dto => ValidateAge(dto.Age))
            .Validate(dto => ValidateEmail(dto.Email))
            .Build((name, age, email) => new User(name, age, email));

        var result = await pipeline(new UserDto("Alice", 30, "alice@test.com"));

        await Assert.That(result.IsValid).IsTrue();
    }

    [Test]
    public async Task Start_with_async_validate_followed_by_sync_validate_accumulates_errors()
    {
        var pipeline = ValidationPipeline.Create<UserDto, Error>()
            .ValidateAsync(dto => ValidateNameAsync(dto.Name))
            .Validate(dto => ValidateAge(dto.Age))
            .Validate(dto => ValidateEmail(dto.Email))
            .Build((name, age, email) => new User(name, age, email));

        var result = await pipeline(new UserDto("", -1, "bademail"));

        await Assert.That(result.IsInvalid).IsTrue();
        var errors = result.Match(_ => Array.Empty<Error>(), errs => errs.ToArray());
        await Assert.That(errors).Count().IsEqualTo(3);
    }

    #endregion

    #region Real-world scenario

    [Test]
    public async Task Real_world_form_validation_accumulates_all_errors()
    {
        // Build a reusable pipeline for validating user registration
        var validateRegistration = ValidationPipeline.Create<UserDto, Error>()
            .Validate(dto => ValidateName(dto.Name))
            .Validate(dto => ValidateAge(dto.Age))
            .Validate(dto => ValidateEmail(dto.Email))
            .Build((name, age, email) => new User(name, age, email));

        // Valid submission
        var validResult = validateRegistration(new UserDto("Alice", 30, "alice@example.com"));
        await Assert.That(validResult.IsValid).IsTrue();
        var user = validResult.GetValueOrThrow();
        await Assert.That(user.Name).IsEqualTo("Alice");
        await Assert.That(user.Age).IsEqualTo(30);
        await Assert.That(user.Email).IsEqualTo("alice@example.com");

        // Invalid submission — all fields fail
        var invalidResult = validateRegistration(new UserDto("", -1, "invalid"));
        await Assert.That(invalidResult.IsInvalid).IsTrue();

        var errors = invalidResult.Match(
            _ => Array.Empty<Error>(),
            errs => errs.ToArray());
        await Assert.That(errors).Count().IsEqualTo(3);
        await Assert.That(errors[0].Message).IsEqualTo("Name is required");
        await Assert.That(errors[1].Message).IsEqualTo("Age must be between 0 and 150");
        await Assert.That(errors[2].Message).IsEqualTo("Email must contain @");
    }

    [Test]
    public async Task Real_world_async_validation_with_concurrent_checks()
    {
        // Simulate async validation steps (e.g. database lookups)
        static async Task<Validation<string, Error>> CheckNameUniqueAsync(string name)
        {
            await Task.Delay(10);
            return name == "taken"
                ? Validation.Invalid<string, Error>(new ValidationError { Message = "Name already taken" })
                : Validation.Valid<string, Error>(name);
        }

        static async Task<Validation<string, Error>> CheckEmailUniqueAsync(string email)
        {
            await Task.Delay(10);
            return email == "taken@test.com"
                ? Validation.Invalid<string, Error>(new ValidationError { Message = "Email already registered" })
                : Validation.Valid<string, Error>(email);
        }

        var validateRegistration = ValidationPipeline.Create<UserDto, Error>()
            .Validate(dto => ValidateName(dto.Name))
            .ValidateAsync(dto => CheckNameUniqueAsync(dto.Name))
            .Validate(dto => ValidateAge(dto.Age))
            .ValidateAsync(dto => CheckEmailUniqueAsync(dto.Email))
            .Build((name, uniqueName, age, email) => new User(uniqueName, age, email));

        // All checks pass
        var validResult = await validateRegistration(new UserDto("Alice", 30, "alice@test.com"));
        await Assert.That(validResult.IsValid).IsTrue();

        // Multiple failures across sync and async steps
        var invalidResult = await validateRegistration(new UserDto("taken", -1, "taken@test.com"));
        await Assert.That(invalidResult.IsInvalid).IsTrue();

        var errors = invalidResult.Match(
            _ => Array.Empty<Error>(),
            errs => errs.ToArray());
        // "Age must be between 0 and 150", "Name already taken", "Email already registered"
        await Assert.That(errors).Count().IsEqualTo(3);
    }

    #endregion

    #region Edge cases

    [Test]
    public async Task Pipeline_with_single_plain_mapping_step()
    {
        var pipeline = ValidationPipeline.Create<string, Error>()
            .Validate(s => s.Length)
            .Build();

        var result = pipeline("hello");

        await Assert.That(result.IsValid).IsTrue();
        await Assert.That(result.GetValueOrThrow()).IsEqualTo(5);
    }

    [Test]
    public async Task Pipeline_start_with_async_then_plain_mapping()
    {
        var pipeline = ValidationPipeline.Create<string, Error>()
            .ValidateAsync(s => ValidateNameAsync(s))
            .Validate(s => s.Length)
            .Build((name, len) => (name, len));

        var result = await pipeline("hello");

        await Assert.That(result.IsValid).IsTrue();
        var value = result.GetValueOrThrow();
        await Assert.That(value.name).IsEqualTo("hello");
        await Assert.That(value.len).IsEqualTo(5);
    }

    [Test]
    public async Task Start_with_plain_mapping_from_start_interface()
    {
        var pipeline = ValidationPipeline.Create<string, Error>()
            .Validate(s => s.Trim())
            .Validate(s => ValidateName(s))
            .Build((trimmed, validated) => validated);

        var result = pipeline("  Alice  ");

        await Assert.That(result.IsValid).IsTrue();
        await Assert.That(result.GetValueOrThrow()).IsEqualTo("  Alice  ");
    }

    [Test]
    public async Task Start_with_async_validate_from_start_interface()
    {
        var pipeline = ValidationPipeline.Create<string, Error>()
            .ValidateAsync(s => ValidateNameAsync(s))
            .Build();

        var result = await pipeline("Alice");

        await Assert.That(result.IsValid).IsTrue();
        await Assert.That(result.GetValueOrThrow()).IsEqualTo("Alice");
    }

    #endregion
}

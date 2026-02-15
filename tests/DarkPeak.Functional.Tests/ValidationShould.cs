using DarkPeak.Functional;
using DarkPeak.Functional.Extensions;

namespace DarkPeak.Functional.Tests;

public class ValidationShould
{
    #region Creation

    [Test]
    public async Task Create_valid_with_value()
    {
        var validation = Validation.Valid<int, Error>(42);

        await Assert.That(validation.IsValid).IsTrue();
        await Assert.That(validation.IsInvalid).IsFalse();
    }

    [Test]
    public async Task Create_invalid_with_single_error()
    {
        var validation = Validation.Invalid<int, Error>(
            new ValidationError { Message = "Required" });

        await Assert.That(validation.IsInvalid).IsTrue();
        await Assert.That(validation.IsValid).IsFalse();
    }

    [Test]
    public async Task Create_invalid_with_multiple_errors()
    {
        var validation = Validation.Invalid<int, Error>(new Error[]
        {
            new ValidationError { Message = "Too short" },
            new ValidationError { Message = "Missing digit" }
        });

        await Assert.That(validation.IsInvalid).IsTrue();
        var errors = validation.Match(_ => Array.Empty<Error>(), errs => errs.ToArray());
        await Assert.That(errors.Length).IsEqualTo(2);
    }

    [Test]
    public async Task Implicit_conversion_from_value_creates_valid()
    {
        Validation<int, Error> validation = 42;

        await Assert.That(validation.IsValid).IsTrue();
        await Assert.That(validation.GetValueOrThrow()).IsEqualTo(42);
    }

    #endregion

    #region Match

    [Test]
    public async Task Match_valid_calls_valid_function()
    {
        var validation = Validation.Valid<int, Error>(42);

        var result = validation.Match(
            valid: v => $"Value: {v}",
            invalid: errs => $"Errors: {errs.Count}");

        await Assert.That(result).IsEqualTo("Value: 42");
    }

    [Test]
    public async Task Match_invalid_calls_invalid_function()
    {
        var validation = Validation.Invalid<int, Error>(
            new ValidationError { Message = "err" });

        var result = validation.Match(
            valid: v => $"Value: {v}",
            invalid: errs => $"Errors: {errs.Count}");

        await Assert.That(result).IsEqualTo("Errors: 1");
    }

    [Test]
    public async Task MatchAsync_valid_calls_async_function()
    {
        var validation = Validation.Valid<int, Error>(42);

        var result = await validation.MatchAsync(
            valid: async v => { await Task.Delay(1); return $"Value: {v}"; },
            invalid: async errs => { await Task.Delay(1); return $"Errors: {errs.Count}"; });

        await Assert.That(result).IsEqualTo("Value: 42");
    }

    [Test]
    public async Task MatchAsync_invalid_calls_async_function()
    {
        var validation = Validation.Invalid<int, Error>(
            new ValidationError { Message = "err" });

        var result = await validation.MatchAsync(
            valid: async v => { await Task.Delay(1); return $"Value: {v}"; },
            invalid: async errs => { await Task.Delay(1); return $"Errors: {errs.Count}"; });

        await Assert.That(result).IsEqualTo("Errors: 1");
    }

    #endregion

    #region Map

    [Test]
    public async Task Map_transforms_valid_value()
    {
        var result = Validation.Valid<int, Error>(5).Map(x => x * 2);

        await Assert.That(result.IsValid).IsTrue();
        await Assert.That(result.GetValueOrThrow()).IsEqualTo(10);
    }

    [Test]
    public async Task Map_preserves_invalid()
    {
        var result = Validation.Invalid<int, Error>(
            new ValidationError { Message = "err" }).Map(x => x * 2);

        await Assert.That(result.IsInvalid).IsTrue();
    }

    [Test]
    public async Task MapAsync_transforms_valid_value()
    {
        var result = await Validation.Valid<int, Error>(5).MapAsync(async x =>
        {
            await Task.Delay(1);
            return x * 2;
        });

        await Assert.That(result.IsValid).IsTrue();
        await Assert.That(result.GetValueOrThrow()).IsEqualTo(10);
    }

    #endregion

    #region Bind

    [Test]
    public async Task Bind_chains_valid_validations()
    {
        var result = Validation.Valid<int, Error>(42)
            .Bind(x => Validation.Valid<string, Error>($"Value: {x}"));

        await Assert.That(result.IsValid).IsTrue();
        await Assert.That(result.GetValueOrThrow()).IsEqualTo("Value: 42");
    }

    [Test]
    public async Task Bind_short_circuits_on_invalid()
    {
        var result = Validation.Invalid<int, Error>(
            new ValidationError { Message = "err" })
            .Bind(x => Validation.Valid<string, Error>($"Value: {x}"));

        await Assert.That(result.IsInvalid).IsTrue();
    }

    [Test]
    public async Task BindAsync_chains_async_operations()
    {
        var result = await Validation.Valid<int, Error>(42).BindAsync(async x =>
        {
            await Task.Delay(1);
            return Validation.Valid<string, Error>($"Value: {x}");
        });

        await Assert.That(result.IsValid).IsTrue();
        await Assert.That(result.GetValueOrThrow()).IsEqualTo("Value: 42");
    }

    #endregion

    #region Tap

    [Test]
    public async Task Tap_executes_action_on_valid()
    {
        var executed = false;

        Validation.Valid<int, Error>(42).Tap(x => executed = true);

        await Assert.That(executed).IsTrue();
    }

    [Test]
    public async Task Tap_does_not_execute_on_invalid()
    {
        var executed = false;

        Validation.Invalid<int, Error>(new ValidationError { Message = "err" })
            .Tap(x => executed = true);

        await Assert.That(executed).IsFalse();
    }

    [Test]
    public async Task TapInvalid_executes_action_on_invalid()
    {
        var executed = false;

        Validation.Invalid<int, Error>(new ValidationError { Message = "err" })
            .TapInvalid(errs => executed = true);

        await Assert.That(executed).IsTrue();
    }

    [Test]
    public async Task TapInvalid_does_not_execute_on_valid()
    {
        var executed = false;

        Validation.Valid<int, Error>(42).TapInvalid(errs => executed = true);

        await Assert.That(executed).IsFalse();
    }

    #endregion

    #region GetValueOrDefault / GetValueOrThrow

    [Test]
    public async Task GetValueOrDefault_returns_value_on_valid()
    {
        var value = Validation.Valid<int, Error>(42).GetValueOrDefault(0);

        await Assert.That(value).IsEqualTo(42);
    }

    [Test]
    public async Task GetValueOrDefault_returns_default_on_invalid()
    {
        var value = Validation.Invalid<int, Error>(
            new ValidationError { Message = "err" }).GetValueOrDefault(99);

        await Assert.That(value).IsEqualTo(99);
    }

    [Test]
    public async Task GetValueOrDefault_with_factory_returns_default_on_invalid()
    {
        var value = Validation.Invalid<int, Error>(
            new ValidationError { Message = "err" }).GetValueOrDefault(() => 99);

        await Assert.That(value).IsEqualTo(99);
    }

    [Test]
    public async Task GetValueOrThrow_returns_value_on_valid()
    {
        var value = Validation.Valid<int, Error>(42).GetValueOrThrow();

        await Assert.That(value).IsEqualTo(42);
    }

    [Test]
    public void GetValueOrThrow_throws_on_invalid()
    {
        var validation = Validation.Invalid<int, Error>(
            new ValidationError { Message = "err" });

        Assert.Throws<InvalidOperationException>(() => validation.GetValueOrThrow());
    }

    #endregion

    #region Apply

    [Test]
    public async Task Apply_both_valid_produces_valid_result()
    {
        var funcValidation = Validation.Valid<Func<int, string>, Error>(x => $"Value: {x}");
        var valueValidation = Validation.Valid<int, Error>(42);

        var result = funcValidation.Apply(valueValidation);

        await Assert.That(result.IsValid).IsTrue();
        await Assert.That(result.GetValueOrThrow()).IsEqualTo("Value: 42");
    }

    [Test]
    public async Task Apply_invalid_func_returns_func_errors()
    {
        var funcValidation = Validation.Invalid<Func<int, string>, Error>(
            new ValidationError { Message = "func err" });
        var valueValidation = Validation.Valid<int, Error>(42);

        var result = funcValidation.Apply(valueValidation);

        await Assert.That(result.IsInvalid).IsTrue();
        var errors = result.Match(_ => Array.Empty<Error>(), errs => errs.ToArray());
        await Assert.That(errors.Length).IsEqualTo(1);
        await Assert.That(errors[0].Message).IsEqualTo("func err");
    }

    [Test]
    public async Task Apply_invalid_value_returns_value_errors()
    {
        var funcValidation = Validation.Valid<Func<int, string>, Error>(x => $"Value: {x}");
        var valueValidation = Validation.Invalid<int, Error>(
            new ValidationError { Message = "value err" });

        var result = funcValidation.Apply(valueValidation);

        await Assert.That(result.IsInvalid).IsTrue();
        var errors = result.Match(_ => Array.Empty<Error>(), errs => errs.ToArray());
        await Assert.That(errors.Length).IsEqualTo(1);
        await Assert.That(errors[0].Message).IsEqualTo("value err");
    }

    [Test]
    public async Task Apply_both_invalid_accumulates_all_errors()
    {
        var funcValidation = Validation.Invalid<Func<int, string>, Error>(
            new ValidationError { Message = "func err" });
        var valueValidation = Validation.Invalid<int, Error>(
            new ValidationError { Message = "value err" });

        var result = funcValidation.Apply(valueValidation);

        await Assert.That(result.IsInvalid).IsTrue();
        var errors = result.Match(_ => Array.Empty<Error>(), errs => errs.ToArray());
        await Assert.That(errors.Length).IsEqualTo(2);
        await Assert.That(errors[0].Message).IsEqualTo("func err");
        await Assert.That(errors[1].Message).IsEqualTo("value err");
    }

    #endregion

    #region ZipWith (two values)

    [Test]
    public async Task ZipWith_two_valid_produces_combined_result()
    {
        var name = Validation.Valid<string, Error>("Alice");
        var age = Validation.Valid<int, Error>(30);

        var result = name.ZipWith(age, (n, a) => $"{n} is {a}");

        await Assert.That(result.IsValid).IsTrue();
        await Assert.That(result.GetValueOrThrow()).IsEqualTo("Alice is 30");
    }

    [Test]
    public async Task ZipWith_two_invalid_accumulates_errors()
    {
        var name = Validation.Invalid<string, Error>(new ValidationError { Message = "Name required" });
        var age = Validation.Invalid<int, Error>(new ValidationError { Message = "Age required" });

        var result = name.ZipWith(age, (n, a) => $"{n} is {a}");

        await Assert.That(result.IsInvalid).IsTrue();
        var errors = result.Match(_ => Array.Empty<Error>(), errs => errs.ToArray());
        await Assert.That(errors.Length).IsEqualTo(2);
    }

    [Test]
    public async Task ZipWith_first_invalid_returns_first_error()
    {
        var name = Validation.Invalid<string, Error>(new ValidationError { Message = "Name required" });
        var age = Validation.Valid<int, Error>(30);

        var result = name.ZipWith(age, (n, a) => $"{n} is {a}");

        await Assert.That(result.IsInvalid).IsTrue();
        var errors = result.Match(_ => Array.Empty<Error>(), errs => errs.ToArray());
        await Assert.That(errors.Length).IsEqualTo(1);
        await Assert.That(errors[0].Message).IsEqualTo("Name required");
    }

    #endregion

    #region ZipWith (three values)

    [Test]
    public async Task ZipWith_three_valid_produces_combined_result()
    {
        var first = Validation.Valid<string, Error>("Alice");
        var second = Validation.Valid<int, Error>(30);
        var third = Validation.Valid<string, Error>("London");

        var result = first.ZipWith(second, third, (n, a, c) => $"{n}, {a}, {c}");

        await Assert.That(result.IsValid).IsTrue();
        await Assert.That(result.GetValueOrThrow()).IsEqualTo("Alice, 30, London");
    }

    [Test]
    public async Task ZipWith_three_all_invalid_accumulates_all_errors()
    {
        var first = Validation.Invalid<string, Error>(new ValidationError { Message = "err1" });
        var second = Validation.Invalid<int, Error>(new ValidationError { Message = "err2" });
        var third = Validation.Invalid<string, Error>(new ValidationError { Message = "err3" });

        var result = first.ZipWith(second, third, (a, b, c) => "");

        await Assert.That(result.IsInvalid).IsTrue();
        var errors = result.Match(_ => Array.Empty<Error>(), errs => errs.ToArray());
        await Assert.That(errors.Length).IsEqualTo(3);
    }

    #endregion

    #region Sequence

    [Test]
    public async Task Sequence_all_valid()
    {
        var validations = new[]
        {
            Validation.Valid<int, Error>(1),
            Validation.Valid<int, Error>(2),
            Validation.Valid<int, Error>(3),
        };

        var result = validations.Sequence();

        await Assert.That(result.IsValid).IsTrue();
        var values = result.GetValueOrThrow().ToList();
        await Assert.That(values.Count).IsEqualTo(3);
        await Assert.That(values[0]).IsEqualTo(1);
        await Assert.That(values[1]).IsEqualTo(2);
        await Assert.That(values[2]).IsEqualTo(3);
    }

    [Test]
    public async Task Sequence_accumulates_all_errors()
    {
        var validations = new[]
        {
            Validation.Valid<int, Error>(1),
            Validation.Invalid<int, Error>(new ValidationError { Message = "err1" }),
            Validation.Valid<int, Error>(3),
            Validation.Invalid<int, Error>(new ValidationError { Message = "err2" }),
        };

        var result = validations.Sequence();

        await Assert.That(result.IsInvalid).IsTrue();
        var errors = result.Match(_ => Array.Empty<Error>(), errs => errs.ToArray());
        await Assert.That(errors.Length).IsEqualTo(2);
        await Assert.That(errors[0].Message).IsEqualTo("err1");
        await Assert.That(errors[1].Message).IsEqualTo("err2");
    }

    [Test]
    public async Task Sequence_empty_returns_valid_empty()
    {
        var validations = Array.Empty<Validation<int, Error>>();

        var result = validations.Sequence();

        await Assert.That(result.IsValid).IsTrue();
        var values = result.GetValueOrThrow().ToList();
        await Assert.That(values.Count).IsEqualTo(0);
    }

    #endregion

    #region ToResult / ToValidation

    [Test]
    public async Task ToResult_converts_valid_to_success()
    {
        var result = Validation.Valid<int, Error>(42).ToResult();

        await Assert.That(result.IsSuccess).IsTrue();
        await Assert.That(result.GetValueOrThrow()).IsEqualTo(42);
    }

    [Test]
    public async Task ToResult_converts_invalid_to_failure_with_first_error()
    {
        var result = Validation.Invalid<int, Error>(new Error[]
        {
            new ValidationError { Message = "first" },
            new ValidationError { Message = "second" },
        }).ToResult();

        await Assert.That(result.IsFailure).IsTrue();
        var error = result.Match(_ => (Error)null!, e => e);
        await Assert.That(error.Message).IsEqualTo("first");
    }

    [Test]
    public async Task ToValidation_converts_success_to_valid()
    {
        var validation = Result.Success<int, Error>(42).ToValidation();

        await Assert.That(validation.IsValid).IsTrue();
        await Assert.That(validation.GetValueOrThrow()).IsEqualTo(42);
    }

    [Test]
    public async Task ToValidation_converts_failure_to_invalid()
    {
        var validation = Result.Failure<int, Error>(
            new ValidationError { Message = "err" }).ToValidation();

        await Assert.That(validation.IsInvalid).IsTrue();
        var errors = validation.Match(_ => Array.Empty<Error>(), errs => errs.ToArray());
        await Assert.That(errors.Length).IsEqualTo(1);
        await Assert.That(errors[0].Message).IsEqualTo("err");
    }

    #endregion

    #region Real-world scenario

    [Test]
    public async Task Validates_form_input_accumulating_all_errors()
    {
        // Simulate validating a user registration form
        static Validation<string, Error> ValidateName(string name) =>
            string.IsNullOrWhiteSpace(name)
                ? Validation.Invalid<string, Error>(new ValidationError { Message = "Name is required" })
                : Validation.Valid<string, Error>(name);

        static Validation<int, Error> ValidateAge(int age) =>
            age < 0 || age > 150
                ? Validation.Invalid<int, Error>(new ValidationError { Message = "Age must be between 0 and 150" })
                : Validation.Valid<int, Error>(age);

        static Validation<string, Error> ValidateEmail(string email) =>
            !email.Contains('@')
                ? Validation.Invalid<string, Error>(new ValidationError { Message = "Invalid email" })
                : Validation.Valid<string, Error>(email);

        // All invalid — should accumulate 3 errors
        var result = ValidateName("")
            .ZipWith(ValidateAge(-1), ValidateEmail("not-an-email"),
                (name, age, email) => $"{name}, {age}, {email}");

        await Assert.That(result.IsInvalid).IsTrue();
        var errors = result.Match(_ => Array.Empty<Error>(), errs => errs.ToArray());
        await Assert.That(errors.Length).IsEqualTo(3);

        // All valid — should produce combined result
        var valid = ValidateName("Alice")
            .ZipWith(ValidateAge(30), ValidateEmail("alice@example.com"),
                (name, age, email) => $"{name}, {age}, {email}");

        await Assert.That(valid.IsValid).IsTrue();
        await Assert.That(valid.GetValueOrThrow()).IsEqualTo("Alice, 30, alice@example.com");
    }

    #endregion

    #region Traverse

    [Test]
    public async Task Traverse_all_valid_returns_mapped_values()
    {
        var source = new[] { 1, 2, 3 };

        var result = source.Traverse(x =>
            Validation.Valid<string, Error>($"v{x}"));

        await Assert.That(result.IsValid).IsTrue();
        var values = result.GetValueOrThrow().ToList();
        await Assert.That(values.Count).IsEqualTo(3);
        await Assert.That(values[0]).IsEqualTo("v1");
        await Assert.That(values[1]).IsEqualTo("v2");
        await Assert.That(values[2]).IsEqualTo("v3");
    }

    [Test]
    public async Task Traverse_accumulates_all_errors()
    {
        var source = new[] { 1, 2, 3 };

        var result = source.Traverse(x =>
            x == 2
                ? Validation.Invalid<string, Error>(new ValidationError { Message = "bad2" })
                : x == 3
                    ? Validation.Invalid<string, Error>(new ValidationError { Message = "bad3" })
                    : Validation.Valid<string, Error>($"v{x}"));

        await Assert.That(result.IsInvalid).IsTrue();
        var errors = result.Match(_ => Array.Empty<Error>(), errs => errs.ToArray());
        await Assert.That(errors.Length).IsEqualTo(2);
        await Assert.That(errors[0].Message).IsEqualTo("bad2");
        await Assert.That(errors[1].Message).IsEqualTo("bad3");
    }

    [Test]
    public async Task Traverse_empty_returns_valid_empty()
    {
        var source = Array.Empty<int>();

        var result = source.Traverse(x =>
            Validation.Valid<string, Error>($"v{x}"));

        await Assert.That(result.IsValid).IsTrue();
        var values = result.GetValueOrThrow().ToList();
        await Assert.That(values.Count).IsEqualTo(0);
    }

    #endregion

    #region Join (two values)

    [Test]
    public async Task Join_two_valid_returns_tuple()
    {
        var first = Validation.Valid<int, Error>(1);
        var second = Validation.Valid<string, Error>("two");

        var joined = first.Join(second);

        await Assert.That(joined.IsValid).IsTrue();
        var (v1, v2) = joined.GetValueOrThrow();
        await Assert.That(v1).IsEqualTo(1);
        await Assert.That(v2).IsEqualTo("two");
    }

    [Test]
    public async Task Join_two_both_invalid_accumulates_errors()
    {
        var first = Validation.Invalid<int, Error>(new ValidationError { Message = "err1" });
        var second = Validation.Invalid<string, Error>(new ValidationError { Message = "err2" });

        var joined = first.Join(second);

        await Assert.That(joined.IsInvalid).IsTrue();
        var errors = joined.Match(_ => Array.Empty<Error>(), errs => errs.ToArray());
        await Assert.That(errors.Length).IsEqualTo(2);
        await Assert.That(errors[0].Message).IsEqualTo("err1");
        await Assert.That(errors[1].Message).IsEqualTo("err2");
    }

    [Test]
    public async Task Join_two_first_invalid_returns_error()
    {
        var first = Validation.Invalid<int, Error>(new ValidationError { Message = "err1" });
        var second = Validation.Valid<string, Error>("two");

        var joined = first.Join(second);

        await Assert.That(joined.IsInvalid).IsTrue();
        var errors = joined.Match(_ => Array.Empty<Error>(), errs => errs.ToArray());
        await Assert.That(errors.Length).IsEqualTo(1);
        await Assert.That(errors[0].Message).IsEqualTo("err1");
    }

    #endregion

    #region Join (three values)

    [Test]
    public async Task Join_three_valid_returns_tuple()
    {
        var first = Validation.Valid<int, Error>(1);
        var second = Validation.Valid<string, Error>("two");
        var third = Validation.Valid<bool, Error>(true);

        var joined = first.Join(second, third);

        await Assert.That(joined.IsValid).IsTrue();
        var (v1, v2, v3) = joined.GetValueOrThrow();
        await Assert.That(v1).IsEqualTo(1);
        await Assert.That(v2).IsEqualTo("two");
        await Assert.That(v3).IsTrue();
    }

    [Test]
    public async Task Join_three_all_invalid_accumulates_all_errors()
    {
        var first = Validation.Invalid<int, Error>(new ValidationError { Message = "err1" });
        var second = Validation.Invalid<string, Error>(new ValidationError { Message = "err2" });
        var third = Validation.Invalid<bool, Error>(new ValidationError { Message = "err3" });

        var joined = first.Join(second, third);

        await Assert.That(joined.IsInvalid).IsTrue();
        var errors = joined.Match(_ => Array.Empty<Error>(), errs => errs.ToArray());
        await Assert.That(errors.Length).IsEqualTo(3);
    }

    #endregion

    #region Invalid Async Coverage

    [Test]
    public async Task MapAsync_invalid_returns_invalid()
    {
        var validation = Validation.Invalid<int, Error>(
            new ValidationError { Message = "err" });

        var mapped = await validation.MapAsync(async v => { await Task.Yield(); return v * 2; });

        await Assert.That(mapped.IsInvalid).IsTrue();
    }

    [Test]
    public async Task BindAsync_invalid_returns_invalid()
    {
        var validation = Validation.Invalid<int, Error>(
            new ValidationError { Message = "err" });

        var bound = await validation.BindAsync(async v =>
        {
            await Task.Yield();
            return Validation.Valid<string, Error>(v.ToString());
        });

        await Assert.That(bound.IsInvalid).IsTrue();
    }

    #endregion
}

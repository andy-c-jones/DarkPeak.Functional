using DarkPeak.Functional;
using DarkPeak.Functional.Extensions;

namespace DarkPeak.Functional.Tests;

public class TypeConversionShould
{
    #region Option → Result

    [Test]
    public async Task Convert_some_to_success_result()
    {
        var option = Option.Some(42);

        var result = option.ToResult(new NotFoundError { Message = "Not found" });

        await Assert.That(result.IsSuccess).IsTrue();
        var value = result.Match(v => v, _ => 0);
        await Assert.That(value).IsEqualTo(42);
    }

    [Test]
    public async Task Convert_none_to_failure_result()
    {
        var option = Option.None<int>();

        var result = option.ToResult(new NotFoundError { Message = "Not found" });

        await Assert.That(result.IsFailure).IsTrue();
        var error = result.Match(_ => (Error)new InternalError { Message = "" }, e => e);
        await Assert.That(error.Message).IsEqualTo("Not found");
    }

    [Test]
    public async Task Convert_none_to_failure_result_with_factory()
    {
        var option = Option.None<int>();

        var result = option.ToResult(() => new NotFoundError { Message = "Lazy error" });

        await Assert.That(result.IsFailure).IsTrue();
        var error = result.Match(_ => (Error)new InternalError { Message = "" }, e => e);
        await Assert.That(error.Message).IsEqualTo("Lazy error");
    }

    [Test]
    public async Task Convert_some_to_success_result_with_factory_does_not_invoke_factory()
    {
        var option = Option.Some(42);
        var factoryCalled = false;

        var result = option.ToResult(() =>
        {
            factoryCalled = true;
            return new NotFoundError { Message = "Not found" };
        });

        await Assert.That(result.IsSuccess).IsTrue();
        await Assert.That(factoryCalled).IsFalse();
    }

    #endregion

    #region Option → Either

    [Test]
    public async Task Convert_some_to_right_either()
    {
        var option = Option.Some(42);

        var either = option.ToEither("missing");

        await Assert.That(either.IsRight).IsTrue();
        var value = either.Match(_ => 0, v => v);
        await Assert.That(value).IsEqualTo(42);
    }

    [Test]
    public async Task Convert_none_to_left_either()
    {
        var option = Option.None<int>();

        var either = option.ToEither("missing");

        await Assert.That(either.IsLeft).IsTrue();
        var value = either.Match(s => s, _ => "");
        await Assert.That(value).IsEqualTo("missing");
    }

    [Test]
    public async Task Convert_none_to_left_either_with_factory()
    {
        var option = Option.None<int>();

        var either = option.ToEither(() => "lazy missing");

        await Assert.That(either.IsLeft).IsTrue();
        var value = either.Match(s => s, _ => "");
        await Assert.That(value).IsEqualTo("lazy missing");
    }

    [Test]
    public async Task Convert_some_to_right_either_with_factory_does_not_invoke_factory()
    {
        var option = Option.Some(42);
        var factoryCalled = false;

        var either = option.ToEither(() =>
        {
            factoryCalled = true;
            return "missing";
        });

        await Assert.That(either.IsRight).IsTrue();
        await Assert.That(factoryCalled).IsFalse();
    }

    #endregion

    #region Result → Either

    [Test]
    public async Task Convert_success_result_to_right_either()
    {
        var result = Result.Success<int, ValidationError>(42);

        var either = result.ToEither();

        await Assert.That(either.IsRight).IsTrue();
        var value = either.Match(_ => 0, v => v);
        await Assert.That(value).IsEqualTo(42);
    }

    [Test]
    public async Task Convert_failure_result_to_left_either()
    {
        var result = Result.Failure<int, ValidationError>(new ValidationError { Message = "Invalid" });

        var either = result.ToEither();

        await Assert.That(either.IsLeft).IsTrue();
        var error = either.Match(e => e.Message, _ => "");
        await Assert.That(error).IsEqualTo("Invalid");
    }

    #endregion

    #region Either → Option

    [Test]
    public async Task Convert_right_either_to_some_option()
    {
        var either = Either.Right<string, int>(42);

        var option = either.RightToOption();

        await Assert.That(option.IsSome).IsTrue();
        var value = option.Match(v => v, () => 0);
        await Assert.That(value).IsEqualTo(42);
    }

    [Test]
    public async Task Convert_left_either_to_none_option_via_right()
    {
        var either = Either.Left<string, int>("error");

        var option = either.RightToOption();

        await Assert.That(option.IsNone).IsTrue();
    }

    [Test]
    public async Task Convert_left_either_to_some_option()
    {
        var either = Either.Left<string, int>("value");

        var option = either.LeftToOption();

        await Assert.That(option.IsSome).IsTrue();
        var value = option.Match(v => v, () => "");
        await Assert.That(value).IsEqualTo("value");
    }

    [Test]
    public async Task Convert_right_either_to_none_option_via_left()
    {
        var either = Either.Right<string, int>(42);

        var option = either.LeftToOption();

        await Assert.That(option.IsNone).IsTrue();
    }

    #endregion

    #region Either → Result

    [Test]
    public async Task Convert_right_either_to_success_result()
    {
        var either = Either.Right<ValidationError, int>(42);

        var result = either.ToResult();

        await Assert.That(result.IsSuccess).IsTrue();
        var value = result.Match(v => v, _ => 0);
        await Assert.That(value).IsEqualTo(42);
    }

    [Test]
    public async Task Convert_left_either_to_failure_result()
    {
        var error = new ValidationError { Message = "Invalid" };
        var either = Either.Left<ValidationError, int>(error);

        var result = either.ToResult();

        await Assert.That(result.IsFailure).IsTrue();
        var errorMsg = result.Match(_ => "", e => e.Message);
        await Assert.That(errorMsg).IsEqualTo("Invalid");
    }

    #endregion

    #region Round-trip Conversions

    [Test]
    public async Task Option_to_result_to_either_round_trip_preserves_value()
    {
        var option = Option.Some(42);

        var result = option.ToResult(new NotFoundError { Message = "Not found" });
        var either = result.ToEither();
        var backToOption = either.RightToOption();

        await Assert.That(backToOption.IsSome).IsTrue();
        var value = backToOption.Match(v => v, () => 0);
        await Assert.That(value).IsEqualTo(42);
    }

    [Test]
    public async Task Result_to_option_loses_error_information()
    {
        var result = Result.Failure<int, ValidationError>(
            new ValidationError { Message = "Invalid" });

        var option = result.AsOption();
        var backToResult = option.ToResult(new InternalError { Message = "Unknown" });

        await Assert.That(option.IsNone).IsTrue();
        await Assert.That(backToResult.IsFailure).IsTrue();
        var error = backToResult.Match(_ => (Error)new InternalError { Message = "" }, e => e);
        await Assert.That(error.Message).IsEqualTo("Unknown");
    }

    #endregion
}

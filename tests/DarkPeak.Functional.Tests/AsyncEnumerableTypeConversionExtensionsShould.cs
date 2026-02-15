using DarkPeak.Functional;
using DarkPeak.Functional.Extensions;

namespace DarkPeak.Functional.Tests;

/// <summary>
/// Tests for AsyncEnumerableTypeConversionExtensions stream-level type conversions
/// between Option, Result, Validation, and Either.
/// </summary>
public class AsyncEnumerableTypeConversionExtensionsShould
{
    // ── Helpers ──

    private static async IAsyncEnumerable<T> ToAsyncEnumerable<T>(params T[] items)
    {
        foreach (var item in items)
        {
            await Task.Yield();
            yield return item;
        }
    }

    private static async IAsyncEnumerable<T> EmptyAsyncEnumerable<T>()
    {
        await Task.CompletedTask;
        yield break;
    }

    // ── ToResultStream (Option → Result, error instance) ──

    [Test]
    public async Task To_result_stream_converts_some_to_success_and_none_to_failure()
    {
        // Arrange
        var source = ToAsyncEnumerable<Option<int>>(
            new Some<int>(1),
            new None<int>(),
            new Some<int>(3));
        var error = new NotFoundError { Message = "Not found" };

        // Act
        var result = await source.ToResultStream(error).ToListAsync();

        // Assert
        await Assert.That(result.Count).IsEqualTo(3);

        await Assert.That(result[0].IsSuccess).IsTrue();
        var value0 = result[0].Match(v => v, _ => 0);
        await Assert.That(value0).IsEqualTo(1);

        await Assert.That(result[1].IsFailure).IsTrue();
        var error1 = result[1].Match(_ => (Error)new InternalError { Message = "" }, e => e);
        await Assert.That(error1.Message).IsEqualTo("Not found");

        await Assert.That(result[2].IsSuccess).IsTrue();
        var value2 = result[2].Match(v => v, _ => 0);
        await Assert.That(value2).IsEqualTo(3);
    }

    [Test]
    public async Task To_result_stream_from_options_returns_empty_for_empty_source()
    {
        // Arrange
        var source = EmptyAsyncEnumerable<Option<int>>();
        var error = new NotFoundError { Message = "Not found" };

        // Act
        var result = await source.ToResultStream(error).ToListAsync();

        // Assert
        await Assert.That(result.Count).IsEqualTo(0);
    }

    // ── ToResultStream (Option → Result, error factory) ──

    [Test]
    public async Task To_result_stream_with_factory_uses_factory_for_none()
    {
        // Arrange
        var source = ToAsyncEnumerable<Option<int>>(
            new Some<int>(10),
            new None<int>());
        var factoryCallCount = 0;

        // Act
        var result = await source.ToResultStream(() =>
        {
            factoryCallCount++;
            return new NotFoundError { Message = $"Error {factoryCallCount}" };
        }).ToListAsync();

        // Assert
        await Assert.That(result.Count).IsEqualTo(2);

        await Assert.That(result[0].IsSuccess).IsTrue();
        var value0 = result[0].Match(v => v, _ => 0);
        await Assert.That(value0).IsEqualTo(10);

        await Assert.That(result[1].IsFailure).IsTrue();
        var error1 = result[1].Match(_ => (Error)new InternalError { Message = "" }, e => e);
        await Assert.That(error1.Message).IsEqualTo("Error 1");

        await Assert.That(factoryCallCount).IsEqualTo(1);
    }

    // ── ToOptionStream (Result → Option) ──

    [Test]
    public async Task To_option_stream_converts_success_to_some_and_failure_to_none()
    {
        // Arrange
        var source = ToAsyncEnumerable<Result<int, NotFoundError>>(
            new Success<int, NotFoundError>(42),
            new Failure<int, NotFoundError>(new NotFoundError { Message = "Missing" }),
            new Success<int, NotFoundError>(99));

        // Act
        var result = await source.ToOptionStream().ToListAsync();

        // Assert
        await Assert.That(result.Count).IsEqualTo(3);

        await Assert.That(result[0].IsSome).IsTrue();
        var value0 = result[0].Match(v => v, () => 0);
        await Assert.That(value0).IsEqualTo(42);

        await Assert.That(result[1].IsNone).IsTrue();

        await Assert.That(result[2].IsSome).IsTrue();
        var value2 = result[2].Match(v => v, () => 0);
        await Assert.That(value2).IsEqualTo(99);
    }

    [Test]
    public async Task To_option_stream_returns_empty_for_empty_source()
    {
        // Arrange
        var source = EmptyAsyncEnumerable<Result<int, NotFoundError>>();

        // Act
        var result = await source.ToOptionStream().ToListAsync();

        // Assert
        await Assert.That(result.Count).IsEqualTo(0);
    }

    // ── ToEitherStream (Result → Either) ──

    [Test]
    public async Task To_either_stream_from_results_converts_success_to_right_and_failure_to_left()
    {
        // Arrange
        var source = ToAsyncEnumerable<Result<int, NotFoundError>>(
            new Success<int, NotFoundError>(5),
            new Failure<int, NotFoundError>(new NotFoundError { Message = "Gone" }));

        // Act
        var result = await source.ToEitherStream().ToListAsync();

        // Assert
        await Assert.That(result.Count).IsEqualTo(2);

        await Assert.That(result[0].IsRight).IsTrue();
        var value0 = result[0].Match(_ => 0, v => v);
        await Assert.That(value0).IsEqualTo(5);

        await Assert.That(result[1].IsLeft).IsTrue();
        var error1 = result[1].Match(e => e.Message, _ => "");
        await Assert.That(error1).IsEqualTo("Gone");
    }

    [Test]
    public async Task To_either_stream_from_results_returns_empty_for_empty_source()
    {
        // Arrange
        var source = EmptyAsyncEnumerable<Result<int, NotFoundError>>();

        // Act
        var result = await source.ToEitherStream().ToListAsync();

        // Assert
        await Assert.That(result.Count).IsEqualTo(0);
    }

    // ── ToEitherStream (Option → Either) ──

    [Test]
    public async Task To_either_stream_from_options_converts_some_to_right_and_none_to_left()
    {
        // Arrange
        var source = ToAsyncEnumerable<Option<int>>(
            new Some<int>(7),
            new None<int>());
        var leftValue = "missing";

        // Act
        var result = await source.ToEitherStream(leftValue).ToListAsync();

        // Assert
        await Assert.That(result.Count).IsEqualTo(2);

        await Assert.That(result[0].IsRight).IsTrue();
        var value0 = result[0].Match(_ => 0, v => v);
        await Assert.That(value0).IsEqualTo(7);

        await Assert.That(result[1].IsLeft).IsTrue();
        var left1 = result[1].Match(l => l, _ => "");
        await Assert.That(left1).IsEqualTo("missing");
    }

    // ── ToResultStream (Validation → Result) ──

    [Test]
    public async Task To_result_stream_from_validations_converts_valid_to_success_and_invalid_to_failure_with_first_error()
    {
        // Arrange
        var source = ToAsyncEnumerable<Validation<int, ValidationError>>(
            new Valid<int, ValidationError>(100),
            new Invalid<int, ValidationError>([
                new ValidationError { Message = "First error" },
                new ValidationError { Message = "Second error" }
            ]));

        // Act
        var result = await source.ToResultStream().ToListAsync();

        // Assert
        await Assert.That(result.Count).IsEqualTo(2);

        await Assert.That(result[0].IsSuccess).IsTrue();
        var value0 = result[0].Match(v => v, _ => 0);
        await Assert.That(value0).IsEqualTo(100);

        await Assert.That(result[1].IsFailure).IsTrue();
        var error1 = result[1].Match(_ => (Error)new InternalError { Message = "" }, e => e);
        await Assert.That(error1.Message).IsEqualTo("First error");
    }

    [Test]
    public async Task To_result_stream_from_validations_returns_empty_for_empty_source()
    {
        // Arrange
        var source = EmptyAsyncEnumerable<Validation<int, ValidationError>>();

        // Act
        var result = await source.ToResultStream().ToListAsync();

        // Assert
        await Assert.That(result.Count).IsEqualTo(0);
    }

    // ── ToValidationStream (Result → Validation) ──

    [Test]
    public async Task To_validation_stream_converts_success_to_valid_and_failure_to_invalid_with_single_error()
    {
        // Arrange
        var source = ToAsyncEnumerable<Result<int, ValidationError>>(
            new Success<int, ValidationError>(50),
            new Failure<int, ValidationError>(new ValidationError { Message = "Bad input" }));

        // Act
        var result = await source.ToValidationStream().ToListAsync();

        // Assert
        await Assert.That(result.Count).IsEqualTo(2);

        await Assert.That(result[0].IsValid).IsTrue();
        var value0 = result[0].Match(v => v, _ => 0);
        await Assert.That(value0).IsEqualTo(50);

        await Assert.That(result[1].IsInvalid).IsTrue();
        var errors1 = result[1].Match(_ => Array.Empty<ValidationError>(), errors => errors.ToArray());
        await Assert.That(errors1.Length).IsEqualTo(1);
        await Assert.That(errors1[0].Message).IsEqualTo("Bad input");
    }

    [Test]
    public async Task To_validation_stream_returns_empty_for_empty_source()
    {
        // Arrange
        var source = EmptyAsyncEnumerable<Result<int, ValidationError>>();

        // Act
        var result = await source.ToValidationStream().ToListAsync();

        // Assert
        await Assert.That(result.Count).IsEqualTo(0);
    }
}

using DarkPeak.Functional;
using DarkPeak.Functional.Extensions;

namespace DarkPeak.Functional.Tests;

/// <summary>
/// Tests for AsyncEnumerableValidationExtensions operations on IAsyncEnumerable{Validation{T, TError}}.
/// </summary>
public class AsyncEnumerableValidationExtensionsShould
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

    // ── MapValid ──

    [Test]
    public async Task Map_valid_transforms_valid_values_and_passes_invalid_through()
    {
        // Arrange
        var error = new ValidationError { Message = "bad" };
        var source = ToAsyncEnumerable<Validation<int, ValidationError>>(
            new Valid<int, ValidationError>(1),
            new Invalid<int, ValidationError>([error]),
            new Valid<int, ValidationError>(3));

        // Act
        var result = await source.MapValid(x => x * 10).ToListAsync();

        // Assert
        await Assert.That(result.Count).IsEqualTo(3);

        await Assert.That(result[0].IsValid).IsTrue();
        await Assert.That(result[0].GetValueOrThrow()).IsEqualTo(10);

        await Assert.That(result[1].IsInvalid).IsTrue();
        var errors = result[1].Match(_ => Array.Empty<ValidationError>(), errs => errs.ToArray());
        await Assert.That(errors[0].Message).IsEqualTo("bad");

        await Assert.That(result[2].IsValid).IsTrue();
        await Assert.That(result[2].GetValueOrThrow()).IsEqualTo(30);
    }

    [Test]
    public async Task Map_valid_returns_empty_for_empty_source()
    {
        // Arrange
        var source = EmptyAsyncEnumerable<Validation<int, ValidationError>>();

        // Act
        var result = await source.MapValid(x => x * 10).ToListAsync();

        // Assert
        await Assert.That(result.Count).IsEqualTo(0);
    }

    // ── MapValidAsync ──

    [Test]
    public async Task Map_valid_async_transforms_valid_values_with_async_mapper()
    {
        // Arrange
        var error = new ValidationError { Message = "err" };
        var source = ToAsyncEnumerable<Validation<int, ValidationError>>(
            new Valid<int, ValidationError>(2),
            new Invalid<int, ValidationError>([error]),
            new Valid<int, ValidationError>(4));

        // Act
        var result = await source.MapValidAsync(async x =>
        {
            await Task.Yield();
            return x * 5;
        }).ToListAsync();

        // Assert
        await Assert.That(result.Count).IsEqualTo(3);

        await Assert.That(result[0].IsValid).IsTrue();
        await Assert.That(result[0].GetValueOrThrow()).IsEqualTo(10);

        await Assert.That(result[1].IsInvalid).IsTrue();

        await Assert.That(result[2].IsValid).IsTrue();
        await Assert.That(result[2].GetValueOrThrow()).IsEqualTo(20);
    }

    // ── TapValid ──

    [Test]
    public async Task Tap_valid_calls_action_only_for_valid_values_and_preserves_sequence()
    {
        // Arrange
        var error = new ValidationError { Message = "oops" };
        var source = ToAsyncEnumerable<Validation<int, ValidationError>>(
            new Valid<int, ValidationError>(1),
            new Invalid<int, ValidationError>([error]),
            new Valid<int, ValidationError>(3));
        var tapped = new List<int>();

        // Act
        var result = await source.TapValid(x => tapped.Add(x)).ToListAsync();

        // Assert
        await Assert.That(tapped.Count).IsEqualTo(2);
        await Assert.That(tapped[0]).IsEqualTo(1);
        await Assert.That(tapped[1]).IsEqualTo(3);

        await Assert.That(result.Count).IsEqualTo(3);
        await Assert.That(result[0].IsValid).IsTrue();
        await Assert.That(result[1].IsInvalid).IsTrue();
        await Assert.That(result[2].IsValid).IsTrue();
    }

    // ── TapInvalid ──

    [Test]
    public async Task Tap_invalid_calls_action_only_for_invalid_values_with_error_list()
    {
        // Arrange
        var error1 = new ValidationError { Message = "e1" };
        var error2 = new ValidationError { Message = "e2" };
        var source = ToAsyncEnumerable<Validation<int, ValidationError>>(
            new Valid<int, ValidationError>(1),
            new Invalid<int, ValidationError>([error1, error2]),
            new Valid<int, ValidationError>(3));
        var tappedErrors = new List<IReadOnlyList<ValidationError>>();

        // Act
        var result = await source.TapInvalid(errs => tappedErrors.Add(errs)).ToListAsync();

        // Assert
        await Assert.That(tappedErrors.Count).IsEqualTo(1);
        await Assert.That(tappedErrors[0].Count).IsEqualTo(2);
        await Assert.That(tappedErrors[0][0].Message).IsEqualTo("e1");
        await Assert.That(tappedErrors[0][1].Message).IsEqualTo("e2");

        await Assert.That(result.Count).IsEqualTo(3);
        await Assert.That(result[0].IsValid).IsTrue();
        await Assert.That(result[1].IsInvalid).IsTrue();
        await Assert.That(result[2].IsValid).IsTrue();
    }

    // ── ChooseValid ──

    [Test]
    public async Task Choose_valid_returns_only_valid_values_from_mixed_sequence()
    {
        // Arrange
        var error = new ValidationError { Message = "bad" };
        var source = ToAsyncEnumerable<Validation<int, ValidationError>>(
            new Valid<int, ValidationError>(10),
            new Invalid<int, ValidationError>([error]),
            new Valid<int, ValidationError>(30),
            new Invalid<int, ValidationError>([error]));

        // Act
        var result = await source.ChooseValid().ToListAsync();

        // Assert
        await Assert.That(result.Count).IsEqualTo(2);
        await Assert.That(result[0]).IsEqualTo(10);
        await Assert.That(result[1]).IsEqualTo(30);
    }

    [Test]
    public async Task Choose_valid_returns_empty_when_all_invalid()
    {
        // Arrange
        var error = new ValidationError { Message = "fail" };
        var source = ToAsyncEnumerable<Validation<int, ValidationError>>(
            new Invalid<int, ValidationError>([error]),
            new Invalid<int, ValidationError>([error]));

        // Act
        var result = await source.ChooseValid().ToListAsync();

        // Assert
        await Assert.That(result.Count).IsEqualTo(0);
    }

    [Test]
    public async Task Choose_valid_returns_empty_for_empty_source()
    {
        // Arrange
        var source = EmptyAsyncEnumerable<Validation<int, ValidationError>>();

        // Act
        var result = await source.ChooseValid().ToListAsync();

        // Assert
        await Assert.That(result.Count).IsEqualTo(0);
    }

    // ── SequenceAsync ──

    [Test]
    public async Task Sequence_async_returns_valid_with_all_values_when_all_valid()
    {
        // Arrange
        var source = ToAsyncEnumerable<Validation<int, ValidationError>>(
            new Valid<int, ValidationError>(1),
            new Valid<int, ValidationError>(2),
            new Valid<int, ValidationError>(3));

        // Act
        var result = await source.SequenceAsync();

        // Assert
        await Assert.That(result.IsValid).IsTrue();
        var values = result.GetValueOrThrow();
        await Assert.That(values.Count).IsEqualTo(3);
        await Assert.That(values[0]).IsEqualTo(1);
        await Assert.That(values[1]).IsEqualTo(2);
        await Assert.That(values[2]).IsEqualTo(3);
    }

    [Test]
    public async Task Sequence_async_returns_invalid_with_errors_when_single_invalid()
    {
        // Arrange
        var error = new ValidationError { Message = "not valid" };
        var source = ToAsyncEnumerable<Validation<int, ValidationError>>(
            new Valid<int, ValidationError>(1),
            new Invalid<int, ValidationError>([error]),
            new Valid<int, ValidationError>(3));

        // Act
        var result = await source.SequenceAsync();

        // Assert
        await Assert.That(result.IsInvalid).IsTrue();
        var errors = result.Match(_ => Array.Empty<ValidationError>(), errs => errs.ToArray());
        await Assert.That(errors.Length).IsEqualTo(1);
        await Assert.That(errors[0].Message).IsEqualTo("not valid");
    }

    [Test]
    public async Task Sequence_async_accumulates_all_errors_from_multiple_invalids()
    {
        // Arrange
        var error1 = new ValidationError { Message = "err1" };
        var error2 = new ValidationError { Message = "err2" };
        var error3 = new ValidationError { Message = "err3" };
        var source = ToAsyncEnumerable<Validation<int, ValidationError>>(
            new Invalid<int, ValidationError>([error1]),
            new Valid<int, ValidationError>(2),
            new Invalid<int, ValidationError>([error2, error3]));

        // Act
        var result = await source.SequenceAsync();

        // Assert
        await Assert.That(result.IsInvalid).IsTrue();
        var errors = result.Match(_ => Array.Empty<ValidationError>(), errs => errs.ToArray());
        await Assert.That(errors.Length).IsEqualTo(3);
        await Assert.That(errors[0].Message).IsEqualTo("err1");
        await Assert.That(errors[1].Message).IsEqualTo("err2");
        await Assert.That(errors[2].Message).IsEqualTo("err3");
    }

    [Test]
    public async Task Sequence_async_returns_valid_with_empty_list_for_empty_source()
    {
        // Arrange
        var source = EmptyAsyncEnumerable<Validation<int, ValidationError>>();

        // Act
        var result = await source.SequenceAsync();

        // Assert
        await Assert.That(result.IsValid).IsTrue();
        var values = result.GetValueOrThrow();
        await Assert.That(values.Count).IsEqualTo(0);
    }

    // ── PartitionAsync ──

    [Test]
    public async Task Partition_async_separates_valid_values_and_accumulates_errors()
    {
        // Arrange
        var error1 = new ValidationError { Message = "e1" };
        var error2 = new ValidationError { Message = "e2" };
        var source = ToAsyncEnumerable<Validation<int, ValidationError>>(
            new Valid<int, ValidationError>(10),
            new Invalid<int, ValidationError>([error1]),
            new Valid<int, ValidationError>(30),
            new Invalid<int, ValidationError>([error2]));

        // Act
        var (valid, errors) = await source.PartitionAsync();

        // Assert
        await Assert.That(valid.Count).IsEqualTo(2);
        await Assert.That(valid[0]).IsEqualTo(10);
        await Assert.That(valid[1]).IsEqualTo(30);

        await Assert.That(errors.Count).IsEqualTo(2);
        await Assert.That(errors[0].Message).IsEqualTo("e1");
        await Assert.That(errors[1].Message).IsEqualTo("e2");
    }

    [Test]
    public async Task Partition_async_returns_all_values_and_empty_errors_when_all_valid()
    {
        // Arrange
        var source = ToAsyncEnumerable<Validation<int, ValidationError>>(
            new Valid<int, ValidationError>(1),
            new Valid<int, ValidationError>(2));

        // Act
        var (valid, errors) = await source.PartitionAsync();

        // Assert
        await Assert.That(valid.Count).IsEqualTo(2);
        await Assert.That(valid[0]).IsEqualTo(1);
        await Assert.That(valid[1]).IsEqualTo(2);
        await Assert.That(errors.Count).IsEqualTo(0);
    }

    [Test]
    public async Task Partition_async_returns_empty_values_and_all_errors_when_all_invalid()
    {
        // Arrange
        var error1 = new ValidationError { Message = "a" };
        var error2 = new ValidationError { Message = "b" };
        var source = ToAsyncEnumerable<Validation<int, ValidationError>>(
            new Invalid<int, ValidationError>([error1]),
            new Invalid<int, ValidationError>([error2]));

        // Act
        var (valid, errors) = await source.PartitionAsync();

        // Assert
        await Assert.That(valid.Count).IsEqualTo(0);
        await Assert.That(errors.Count).IsEqualTo(2);
        await Assert.That(errors[0].Message).IsEqualTo("a");
        await Assert.That(errors[1].Message).IsEqualTo("b");
    }

    [Test]
    public async Task Partition_async_returns_both_empty_for_empty_source()
    {
        // Arrange
        var source = EmptyAsyncEnumerable<Validation<int, ValidationError>>();

        // Act
        var (valid, errors) = await source.PartitionAsync();

        // Assert
        await Assert.That(valid.Count).IsEqualTo(0);
        await Assert.That(errors.Count).IsEqualTo(0);
    }
}

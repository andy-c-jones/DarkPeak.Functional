using DarkPeak.Functional;
using DarkPeak.Functional.Extensions;
using TUnit.Assertions;
using TUnit.Assertions.Extensions;
using TUnit.Core;

namespace DarkPeak.Functional.Tests;

public class AsyncEnumerableResultExtensionsShould
{
    #region Helpers

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

    #endregion

    #region MapResult

    [Test]
    public async Task Map_result_maps_successes_and_passes_failures_through()
    {
        // Arrange
        var source = ToAsyncEnumerable(
            Result.Success<int, NotFoundError>(1),
            Result.Failure<int, NotFoundError>(new NotFoundError { Message = "not found" }),
            Result.Success<int, NotFoundError>(3));

        // Act
        var results = await source.MapResult(x => x * 10).ToListAsync();

        // Assert
        await Assert.That(results).Count().IsEqualTo(3);
        await Assert.That(results[0] is Success<int, NotFoundError> s0 && s0.Value == 10).IsTrue();
        await Assert.That(results[1].IsFailure).IsTrue();
        await Assert.That(results[2] is Success<int, NotFoundError> s2 && s2.Value == 30).IsTrue();
    }

    [Test]
    public async Task Map_result_on_empty_returns_empty()
    {
        // Arrange
        var source = EmptyAsyncEnumerable<Result<int, NotFoundError>>();

        // Act
        var results = await source.MapResult(x => x * 10).ToListAsync();

        // Assert
        await Assert.That(results).Count().IsEqualTo(0);
    }

    #endregion

    #region MapResultAsync

    [Test]
    public async Task Map_result_async_maps_successes_with_async_mapper()
    {
        // Arrange
        var source = ToAsyncEnumerable(
            Result.Success<int, NotFoundError>(2),
            Result.Failure<int, NotFoundError>(new NotFoundError { Message = "missing" }),
            Result.Success<int, NotFoundError>(4));

        // Act
        var results = await source.MapResultAsync(async x =>
        {
            await Task.Yield();
            return x * 5;
        }).ToListAsync();

        // Assert
        await Assert.That(results).Count().IsEqualTo(3);
        await Assert.That(results[0] is Success<int, NotFoundError> s0 && s0.Value == 10).IsTrue();
        await Assert.That(results[1].IsFailure).IsTrue();
        await Assert.That(results[2] is Success<int, NotFoundError> s2 && s2.Value == 20).IsTrue();
    }

    #endregion

    #region BindResult

    [Test]
    public async Task Bind_result_succeeds_when_binder_returns_success()
    {
        // Arrange
        var source = ToAsyncEnumerable(
            Result.Success<int, NotFoundError>(5),
            Result.Success<int, NotFoundError>(10));

        // Act
        var results = await source.BindResult(x =>
            Result.Success<string, NotFoundError>(x.ToString())).ToListAsync();

        // Assert
        await Assert.That(results).Count().IsEqualTo(2);
        await Assert.That(results[0] is Success<string, NotFoundError> s0 && s0.Value == "5").IsTrue();
        await Assert.That(results[1] is Success<string, NotFoundError> s1 && s1.Value == "10").IsTrue();
    }

    [Test]
    public async Task Bind_result_fails_when_binder_returns_failure()
    {
        // Arrange
        var source = ToAsyncEnumerable(
            Result.Success<int, NotFoundError>(5));

        // Act
        var results = await source.BindResult(x =>
            Result.Failure<string, NotFoundError>(new NotFoundError { Message = "bind failed" })).ToListAsync();

        // Assert
        await Assert.That(results).Count().IsEqualTo(1);
        await Assert.That(results[0].IsFailure).IsTrue();
        var error = results[0].Match(_ => null!, e => e);
        await Assert.That(error.Message).IsEqualTo("bind failed");
    }

    [Test]
    public async Task Bind_result_passes_failures_through()
    {
        // Arrange
        var source = ToAsyncEnumerable(
            Result.Failure<int, NotFoundError>(new NotFoundError { Message = "original error" }));

        // Act
        var results = await source.BindResult(x =>
            Result.Success<string, NotFoundError>("should not reach")).ToListAsync();

        // Assert
        await Assert.That(results).Count().IsEqualTo(1);
        await Assert.That(results[0].IsFailure).IsTrue();
        var error = results[0].Match(_ => null!, e => e);
        await Assert.That(error.Message).IsEqualTo("original error");
    }

    #endregion

    #region BindResultAsync

    [Test]
    public async Task Bind_result_async_applies_async_binder()
    {
        // Arrange
        var source = ToAsyncEnumerable(
            Result.Success<int, NotFoundError>(7),
            Result.Failure<int, NotFoundError>(new NotFoundError { Message = "err" }),
            Result.Success<int, NotFoundError>(3));

        // Act
        var results = await source.BindResultAsync(async x =>
        {
            await Task.Yield();
            return x > 5
                ? Result.Success<string, NotFoundError>(x.ToString())
                : Result.Failure<string, NotFoundError>(new NotFoundError { Message = "too small" });
        }).ToListAsync();

        // Assert
        await Assert.That(results).Count().IsEqualTo(3);
        await Assert.That(results[0] is Success<string, NotFoundError> s0 && s0.Value == "7").IsTrue();
        await Assert.That(results[1].IsFailure).IsTrue();
        await Assert.That(results[2].IsFailure).IsTrue();
        var error = results[2].Match(_ => null!, e => e);
        await Assert.That(error.Message).IsEqualTo("too small");
    }

    #endregion

    #region TapResult

    [Test]
    public async Task Tap_result_calls_action_only_for_successes()
    {
        // Arrange
        var tapped = new List<int>();
        var source = ToAsyncEnumerable(
            Result.Success<int, NotFoundError>(1),
            Result.Failure<int, NotFoundError>(new NotFoundError { Message = "fail" }),
            Result.Success<int, NotFoundError>(2));

        // Act
        var results = await source.TapResult(x => tapped.Add(x)).ToListAsync();

        // Assert
        await Assert.That(tapped).Count().IsEqualTo(2);
        await Assert.That(tapped[0]).IsEqualTo(1);
        await Assert.That(tapped[1]).IsEqualTo(2);

        // Sequence is unchanged
        await Assert.That(results).Count().IsEqualTo(3);
        await Assert.That(results[0].IsSuccess).IsTrue();
        await Assert.That(results[1].IsFailure).IsTrue();
        await Assert.That(results[2].IsSuccess).IsTrue();
    }

    #endregion

    #region TapResultError

    [Test]
    public async Task Tap_result_error_calls_action_only_for_failures()
    {
        // Arrange
        var tappedErrors = new List<string>();
        var source = ToAsyncEnumerable(
            Result.Success<int, NotFoundError>(1),
            Result.Failure<int, NotFoundError>(new NotFoundError { Message = "err1" }),
            Result.Failure<int, NotFoundError>(new NotFoundError { Message = "err2" }),
            Result.Success<int, NotFoundError>(2));

        // Act
        var results = await source.TapResultError(e => tappedErrors.Add(e.Message)).ToListAsync();

        // Assert
        await Assert.That(tappedErrors).Count().IsEqualTo(2);
        await Assert.That(tappedErrors[0]).IsEqualTo("err1");
        await Assert.That(tappedErrors[1]).IsEqualTo("err2");

        // Sequence is unchanged
        await Assert.That(results).Count().IsEqualTo(4);
        await Assert.That(results[0].IsSuccess).IsTrue();
        await Assert.That(results[1].IsFailure).IsTrue();
        await Assert.That(results[2].IsFailure).IsTrue();
        await Assert.That(results[3].IsSuccess).IsTrue();
    }

    #endregion

    #region ChooseResults

    [Test]
    public async Task Choose_results_returns_only_success_values()
    {
        // Arrange
        var source = ToAsyncEnumerable(
            Result.Success<int, NotFoundError>(10),
            Result.Failure<int, NotFoundError>(new NotFoundError { Message = "skip" }),
            Result.Success<int, NotFoundError>(20),
            Result.Failure<int, NotFoundError>(new NotFoundError { Message = "skip" }),
            Result.Success<int, NotFoundError>(30));

        // Act
        var values = await source.ChooseResults().ToListAsync();

        // Assert
        await Assert.That(values).Count().IsEqualTo(3);
        await Assert.That(values[0]).IsEqualTo(10);
        await Assert.That(values[1]).IsEqualTo(20);
        await Assert.That(values[2]).IsEqualTo(30);
    }

    [Test]
    public async Task Choose_results_returns_empty_when_all_failures()
    {
        // Arrange
        var source = ToAsyncEnumerable(
            Result.Failure<int, NotFoundError>(new NotFoundError { Message = "a" }),
            Result.Failure<int, NotFoundError>(new NotFoundError { Message = "b" }));

        // Act
        var values = await source.ChooseResults().ToListAsync();

        // Assert
        await Assert.That(values).Count().IsEqualTo(0);
    }

    [Test]
    public async Task Choose_results_returns_empty_when_source_is_empty()
    {
        // Arrange
        var source = EmptyAsyncEnumerable<Result<int, NotFoundError>>();

        // Act
        var values = await source.ChooseResults().ToListAsync();

        // Assert
        await Assert.That(values).Count().IsEqualTo(0);
    }

    #endregion

    #region SequenceAsync

    [Test]
    public async Task Sequence_async_returns_success_with_all_values_when_all_succeed()
    {
        // Arrange
        var source = ToAsyncEnumerable(
            Result.Success<int, NotFoundError>(1),
            Result.Success<int, NotFoundError>(2),
            Result.Success<int, NotFoundError>(3));

        // Act
        var result = await source.SequenceAsync();

        // Assert
        await Assert.That(result.IsSuccess).IsTrue();
        var values = result.Match(v => v, _ => null!);
        await Assert.That(values).Count().IsEqualTo(3);
        await Assert.That(values[0]).IsEqualTo(1);
        await Assert.That(values[1]).IsEqualTo(2);
        await Assert.That(values[2]).IsEqualTo(3);
    }

    [Test]
    public async Task Sequence_async_returns_first_failure_when_any_fail()
    {
        // Arrange
        var source = ToAsyncEnumerable(
            Result.Success<int, NotFoundError>(1),
            Result.Failure<int, NotFoundError>(new NotFoundError { Message = "first error" }),
            Result.Failure<int, NotFoundError>(new NotFoundError { Message = "second error" }));

        // Act
        var result = await source.SequenceAsync();

        // Assert
        await Assert.That(result.IsFailure).IsTrue();
        var error = result.Match(_ => null!, e => e);
        await Assert.That(error.Message).IsEqualTo("first error");
    }

    [Test]
    public async Task Sequence_async_returns_success_with_empty_list_when_source_is_empty()
    {
        // Arrange
        var source = EmptyAsyncEnumerable<Result<int, NotFoundError>>();

        // Act
        var result = await source.SequenceAsync();

        // Assert
        await Assert.That(result.IsSuccess).IsTrue();
        var values = result.Match(v => v, _ => null!);
        await Assert.That(values).Count().IsEqualTo(0);
    }

    #endregion

    #region PartitionAsync

    [Test]
    public async Task Partition_async_separates_successes_and_failures()
    {
        // Arrange
        var source = ToAsyncEnumerable(
            Result.Success<int, NotFoundError>(1),
            Result.Failure<int, NotFoundError>(new NotFoundError { Message = "err1" }),
            Result.Success<int, NotFoundError>(2),
            Result.Failure<int, NotFoundError>(new NotFoundError { Message = "err2" }));

        // Act
        var (successes, failures) = await source.PartitionAsync();

        // Assert
        await Assert.That(successes).Count().IsEqualTo(2);
        await Assert.That(successes[0]).IsEqualTo(1);
        await Assert.That(successes[1]).IsEqualTo(2);
        await Assert.That(failures).Count().IsEqualTo(2);
        await Assert.That(failures[0].Message).IsEqualTo("err1");
        await Assert.That(failures[1].Message).IsEqualTo("err2");
    }

    [Test]
    public async Task Partition_async_returns_empty_failures_when_all_succeed()
    {
        // Arrange
        var source = ToAsyncEnumerable(
            Result.Success<int, NotFoundError>(1),
            Result.Success<int, NotFoundError>(2));

        // Act
        var (successes, failures) = await source.PartitionAsync();

        // Assert
        await Assert.That(successes).Count().IsEqualTo(2);
        await Assert.That(failures).Count().IsEqualTo(0);
    }

    [Test]
    public async Task Partition_async_returns_empty_successes_when_all_fail()
    {
        // Arrange
        var source = ToAsyncEnumerable(
            Result.Failure<int, NotFoundError>(new NotFoundError { Message = "a" }),
            Result.Failure<int, NotFoundError>(new NotFoundError { Message = "b" }));

        // Act
        var (successes, failures) = await source.PartitionAsync();

        // Assert
        await Assert.That(successes).Count().IsEqualTo(0);
        await Assert.That(failures).Count().IsEqualTo(2);
    }

    [Test]
    public async Task Partition_async_returns_both_empty_when_source_is_empty()
    {
        // Arrange
        var source = EmptyAsyncEnumerable<Result<int, NotFoundError>>();

        // Act
        var (successes, failures) = await source.PartitionAsync();

        // Assert
        await Assert.That(successes).Count().IsEqualTo(0);
        await Assert.That(failures).Count().IsEqualTo(0);
    }

    #endregion
}

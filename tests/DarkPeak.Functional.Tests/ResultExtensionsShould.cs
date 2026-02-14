using DarkPeak.Functional;
using DarkPeak.Functional.Extensions;

namespace DarkPeak.Functional.Tests;

/// <summary>
/// Tests for ResultExtensions.
/// </summary>
public class ResultExtensionsShould
{
    // ToResult

    [Test]
    public async Task Wrap_successful_function_in_success()
    {
        var result = ResultExtensions.ToResult(() => 42);

        await Assert.That(result.IsSuccess).IsTrue();
        await Assert.That(result.Match(v => v, _ => 0)).IsEqualTo(42);
    }

    [Test]
    public async Task Wrap_throwing_function_in_failure()
    {
        var result = ResultExtensions.ToResult<int>(() => throw new InvalidOperationException("boom"));

        await Assert.That(result.IsFailure).IsTrue();
        var error = result.Match<Error?>(
            success: _ => null,
            failure: e => e
        );
        await Assert.That(error).IsNotNull();
        await Assert.That(error!.Message).IsEqualTo("boom");
        await Assert.That(error).IsTypeOf<InternalError>();
        await Assert.That(((InternalError)error).ExceptionType).IsEqualTo("InvalidOperationException");
    }

    // ToResultAsync

    [Test]
    public async Task Wrap_successful_async_function_in_success()
    {
        var result = await ResultExtensions.ToResultAsync(async () =>
        {
            await Task.Yield();
            return 42;
        });

        await Assert.That(result.IsSuccess).IsTrue();
        await Assert.That(result.Match(v => v, _ => 0)).IsEqualTo(42);
    }

    [Test]
    public async Task Wrap_throwing_async_function_in_failure()
    {
        var result = await ResultExtensions.ToResultAsync<int>(async () =>
        {
            await Task.Yield();
            throw new ArgumentException("bad arg");
        });

        await Assert.That(result.IsFailure).IsTrue();
        var error = result.Match<Error?>(
            success: _ => null,
            failure: e => e
        );
        await Assert.That(error).IsNotNull();
        await Assert.That(error!.Message).IsEqualTo("bad arg");
        await Assert.That(error).IsTypeOf<InternalError>();
        await Assert.That(((InternalError)error).ExceptionType).IsEqualTo("ArgumentException");
    }

    // Combine

    [Test]
    public async Task Combine_all_successes_into_list()
    {
        var results = new[]
        {
            Result.Success<int, InternalError>(1),
            Result.Success<int, InternalError>(2),
            Result.Success<int, InternalError>(3)
        };

        var combined = results.Combine();

        await Assert.That(combined.IsSuccess).IsTrue();
        var values = combined.Match(v => v.ToList(), _ => []);
        await Assert.That(values).Count().IsEqualTo(3);
        await Assert.That(values[0]).IsEqualTo(1);
        await Assert.That(values[1]).IsEqualTo(2);
        await Assert.That(values[2]).IsEqualTo(3);
    }

    [Test]
    public async Task Combine_returns_first_failure()
    {
        var results = new[]
        {
            Result.Success<int, InternalError>(1),
            Result.Failure<int, InternalError>(new InternalError { Message = "first error" }),
            Result.Success<int, InternalError>(3),
            Result.Failure<int, InternalError>(new InternalError { Message = "second error" })
        };

        var combined = results.Combine();

        await Assert.That(combined.IsFailure).IsTrue();
        var error = combined.Match<string>(v => "", e => e.Message);
        await Assert.That(error).IsEqualTo("first error");
    }

    // CollectErrors

    [Test]
    public async Task CollectErrors_returns_success_when_all_succeed()
    {
        var results = new[]
        {
            Result.Success<int, ValidationError>(1),
            Result.Success<int, ValidationError>(2)
        };

        var collected = results.CollectErrors();

        await Assert.That(collected.IsSuccess).IsTrue();
        var values = collected.Match(v => v.ToList(), _ => []);
        await Assert.That(values).Count().IsEqualTo(2);
    }

    [Test]
    public async Task CollectErrors_aggregates_all_validation_errors()
    {
        var results = new[]
        {
            Result.Success<int, ValidationError>(1),
            Result.Failure<int, ValidationError>(new ValidationError
            {
                Message = "Error 1",
                Errors = new Dictionary<string, string[]>
                {
                    ["Name"] = ["Required"]
                }
            }),
            Result.Failure<int, ValidationError>(new ValidationError
            {
                Message = "Error 2",
                Errors = new Dictionary<string, string[]>
                {
                    ["Age"] = ["Must be positive"]
                }
            })
        };

        var collected = results.CollectErrors();

        await Assert.That(collected.IsFailure).IsTrue();
        var error = collected.Match<ValidationError?>(
            success: _ => null,
            failure: e => e
        );
        await Assert.That(error).IsNotNull();
        await Assert.That(error!.Errors).IsNotNull();
        await Assert.That(error.Errors!.Keys).Count().IsEqualTo(2);
    }

    // Choose

    [Test]
    public async Task Choose_filters_failures_and_unwraps_successes()
    {
        var results = new[]
        {
            Result.Success<int, InternalError>(1),
            Result.Failure<int, InternalError>(new InternalError { Message = "err" }),
            Result.Success<int, InternalError>(3)
        };

        var chosen = results.Choose().ToList();

        await Assert.That(chosen).Count().IsEqualTo(2);
        await Assert.That(chosen[0]).IsEqualTo(1);
        await Assert.That(chosen[1]).IsEqualTo(3);
    }

    // Partition

    [Test]
    public async Task Partition_splits_into_successes_and_failures()
    {
        var results = new[]
        {
            Result.Success<int, InternalError>(1),
            Result.Failure<int, InternalError>(new InternalError { Message = "err1" }),
            Result.Success<int, InternalError>(3),
            Result.Failure<int, InternalError>(new InternalError { Message = "err2" })
        };

        var (successes, failures) = results.Partition();
        var successList = successes.ToList();
        var failureList = failures.ToList();

        await Assert.That(successList).Count().IsEqualTo(2);
        await Assert.That(successList[0]).IsEqualTo(1);
        await Assert.That(successList[1]).IsEqualTo(3);
        await Assert.That(failureList).Count().IsEqualTo(2);
        await Assert.That(failureList[0].Message).IsEqualTo("err1");
        await Assert.That(failureList[1].Message).IsEqualTo("err2");
    }
}

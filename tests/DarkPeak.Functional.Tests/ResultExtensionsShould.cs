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

    // Sequence

    [Test]
    public async Task Sequence_all_successes_into_list()
    {
        var results = new[]
        {
            Result.Success<int, InternalError>(1),
            Result.Success<int, InternalError>(2),
            Result.Success<int, InternalError>(3)
        };

        var combined = results.Sequence();

        await Assert.That(combined.IsSuccess).IsTrue();
        var values = combined.Match(v => v.ToList(), _ => []);
        await Assert.That(values).Count().IsEqualTo(3);
        await Assert.That(values[0]).IsEqualTo(1);
        await Assert.That(values[1]).IsEqualTo(2);
        await Assert.That(values[2]).IsEqualTo(3);
    }

    [Test]
    public async Task Sequence_returns_first_failure()
    {
        var results = new[]
        {
            Result.Success<int, InternalError>(1),
            Result.Failure<int, InternalError>(new InternalError { Message = "first error" }),
            Result.Success<int, InternalError>(3),
            Result.Failure<int, InternalError>(new InternalError { Message = "second error" })
        };

        var combined = results.Sequence();

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

    // Traverse

    [Test]
    public async Task Traverse_all_succeed_returns_mapped_values()
    {
        var source = new[] { 1, 2, 3 };

        var result = source.Traverse(x =>
            Result.Success<string, InternalError>($"v{x}"));

        await Assert.That(result.IsSuccess).IsTrue();
        var values = result.Match(v => v.ToList(), _ => []);
        await Assert.That(values).Count().IsEqualTo(3);
        await Assert.That(values[0]).IsEqualTo("v1");
        await Assert.That(values[1]).IsEqualTo("v2");
        await Assert.That(values[2]).IsEqualTo("v3");
    }

    [Test]
    public async Task Traverse_first_failure_returns_fail_fast()
    {
        var source = new[] { 1, 2, 3 };

        var result = source.Traverse(x =>
            x == 2
                ? Result.Failure<string, InternalError>(new InternalError { Message = "bad" })
                : Result.Success<string, InternalError>($"v{x}"));

        await Assert.That(result.IsFailure).IsTrue();
        var error = result.Match<string>(_ => "", e => e.Message);
        await Assert.That(error).IsEqualTo("bad");
    }

    // Join (2-arity)

    [Test]
    public async Task Join_two_successes_returns_tuple()
    {
        var first = Result.Success<int, InternalError>(1);
        var second = Result.Success<string, InternalError>("two");

        var joined = first.Join(second);

        await Assert.That(joined.IsSuccess).IsTrue();
        var (v1, v2) = joined.Match(v => v, _ => default);
        await Assert.That(v1).IsEqualTo(1);
        await Assert.That(v2).IsEqualTo("two");
    }

    [Test]
    public async Task Join_two_first_failure_returns_failure()
    {
        var first = Result.Failure<int, InternalError>(new InternalError { Message = "err1" });
        var second = Result.Success<string, InternalError>("two");

        var joined = first.Join(second);

        await Assert.That(joined.IsFailure).IsTrue();
        var error = joined.Match<string>(_ => "", e => e.Message);
        await Assert.That(error).IsEqualTo("err1");
    }

    [Test]
    public async Task Join_two_second_failure_returns_failure()
    {
        var first = Result.Success<int, InternalError>(1);
        var second = Result.Failure<string, InternalError>(new InternalError { Message = "err2" });

        var joined = first.Join(second);

        await Assert.That(joined.IsFailure).IsTrue();
        var error = joined.Match<string>(_ => "", e => e.Message);
        await Assert.That(error).IsEqualTo("err2");
    }

    // Join (3-arity)

    [Test]
    public async Task Join_three_successes_returns_tuple()
    {
        var first = Result.Success<int, InternalError>(1);
        var second = Result.Success<string, InternalError>("two");
        var third = Result.Success<bool, InternalError>(true);

        var joined = first.Join(second, third);

        await Assert.That(joined.IsSuccess).IsTrue();
        var (v1, v2, v3) = joined.Match(v => v, _ => default);
        await Assert.That(v1).IsEqualTo(1);
        await Assert.That(v2).IsEqualTo("two");
        await Assert.That(v3).IsTrue();
    }

    [Test]
    public async Task Join_three_first_failure_fails_fast()
    {
        var first = Result.Failure<int, InternalError>(new InternalError { Message = "err1" });
        var second = Result.Success<string, InternalError>("two");
        var third = Result.Success<bool, InternalError>(true);

        var joined = first.Join(second, third);

        await Assert.That(joined.IsFailure).IsTrue();
        var error = joined.Match<string>(_ => "", e => e.Message);
        await Assert.That(error).IsEqualTo("err1");
    }

    // Join (4-arity)

    [Test]
    public async Task Join_four_successes_returns_tuple()
    {
        var result = Result.Success<int, InternalError>(1)
            .Join(
                Result.Success<string, InternalError>("two"),
                Result.Success<bool, InternalError>(true),
                Result.Success<double, InternalError>(4.0));

        await Assert.That(result.IsSuccess).IsTrue();
        var (v1, v2, v3, v4) = result.Match(v => v, _ => default);
        await Assert.That(v1).IsEqualTo(1);
        await Assert.That(v2).IsEqualTo("two");
        await Assert.That(v3).IsTrue();
        await Assert.That(v4).IsEqualTo(4.0);
    }

    [Test]
    public async Task Join_four_first_failure_fails_fast()
    {
        var result = Result.Failure<int, InternalError>(new InternalError { Message = "err" })
            .Join(
                Result.Success<string, InternalError>("two"),
                Result.Success<bool, InternalError>(true),
                Result.Success<double, InternalError>(4.0));

        await Assert.That(result.IsFailure).IsTrue();
    }

    // Join (5-arity)

    [Test]
    public async Task Join_five_successes_returns_tuple()
    {
        var result = Result.Success<int, InternalError>(1)
            .Join(
                Result.Success<string, InternalError>("two"),
                Result.Success<bool, InternalError>(true),
                Result.Success<double, InternalError>(4.0),
                Result.Success<char, InternalError>('e'));

        await Assert.That(result.IsSuccess).IsTrue();
        var (v1, v2, v3, v4, v5) = result.Match(v => v, _ => default);
        await Assert.That(v1).IsEqualTo(1);
        await Assert.That(v5).IsEqualTo('e');
    }

    [Test]
    public async Task Join_five_third_failure_fails_fast()
    {
        var result = Result.Success<int, InternalError>(1)
            .Join(
                Result.Success<string, InternalError>("two"),
                Result.Failure<bool, InternalError>(new InternalError { Message = "err3" }),
                Result.Success<double, InternalError>(4.0),
                Result.Success<char, InternalError>('e'));

        await Assert.That(result.IsFailure).IsTrue();
        var error = result.Match<string>(_ => "", e => e.Message);
        await Assert.That(error).IsEqualTo("err3");
    }

    // Join (6-arity)

    [Test]
    public async Task Join_six_successes_returns_tuple()
    {
        var result = Result.Success<int, InternalError>(1)
            .Join(
                Result.Success<string, InternalError>("two"),
                Result.Success<bool, InternalError>(true),
                Result.Success<double, InternalError>(4.0),
                Result.Success<char, InternalError>('e'),
                Result.Success<long, InternalError>(6L));

        await Assert.That(result.IsSuccess).IsTrue();
        var (v1, v2, v3, v4, v5, v6) = result.Match(v => v, _ => default);
        await Assert.That(v1).IsEqualTo(1);
        await Assert.That(v6).IsEqualTo(6L);
    }

    [Test]
    public async Task Join_six_last_failure_fails_fast()
    {
        var result = Result.Success<int, InternalError>(1)
            .Join(
                Result.Success<string, InternalError>("two"),
                Result.Success<bool, InternalError>(true),
                Result.Success<double, InternalError>(4.0),
                Result.Success<char, InternalError>('e'),
                Result.Failure<long, InternalError>(new InternalError { Message = "err6" }));

        await Assert.That(result.IsFailure).IsTrue();
        var error = result.Match<string>(_ => "", e => e.Message);
        await Assert.That(error).IsEqualTo("err6");
    }

    // Join (7-arity)

    [Test]
    public async Task Join_seven_successes_returns_tuple()
    {
        var result = Result.Success<int, InternalError>(1)
            .Join(
                Result.Success<string, InternalError>("two"),
                Result.Success<bool, InternalError>(true),
                Result.Success<double, InternalError>(4.0),
                Result.Success<char, InternalError>('e'),
                Result.Success<long, InternalError>(6L),
                Result.Success<float, InternalError>(7.0f));

        await Assert.That(result.IsSuccess).IsTrue();
        var (v1, v2, v3, v4, v5, v6, v7) = result.Match(v => v, _ => default);
        await Assert.That(v1).IsEqualTo(1);
        await Assert.That(v7).IsEqualTo(7.0f);
    }

    [Test]
    public async Task Join_seven_failure_fails_fast()
    {
        var result = Result.Failure<int, InternalError>(new InternalError { Message = "err" })
            .Join(
                Result.Success<string, InternalError>("two"),
                Result.Success<bool, InternalError>(true),
                Result.Success<double, InternalError>(4.0),
                Result.Success<char, InternalError>('e'),
                Result.Success<long, InternalError>(6L),
                Result.Success<float, InternalError>(7.0f));

        await Assert.That(result.IsFailure).IsTrue();
    }

    // Join (8-arity)

    [Test]
    public async Task Join_eight_successes_returns_tuple()
    {
        var result = Result.Success<int, InternalError>(1)
            .Join(
                Result.Success<string, InternalError>("two"),
                Result.Success<bool, InternalError>(true),
                Result.Success<double, InternalError>(4.0),
                Result.Success<char, InternalError>('e'),
                Result.Success<long, InternalError>(6L),
                Result.Success<float, InternalError>(7.0f),
                Result.Success<byte, InternalError>((byte)8));

        await Assert.That(result.IsSuccess).IsTrue();
        var (v1, v2, v3, v4, v5, v6, v7, v8) = result.Match(v => v, _ => default);
        await Assert.That(v1).IsEqualTo(1);
        await Assert.That(v8).IsEqualTo((byte)8);
    }

    [Test]
    public async Task Join_eight_failure_fails_fast()
    {
        var result = Result.Success<int, InternalError>(1)
            .Join(
                Result.Success<string, InternalError>("two"),
                Result.Success<bool, InternalError>(true),
                Result.Failure<double, InternalError>(new InternalError { Message = "err4" }),
                Result.Success<char, InternalError>('e'),
                Result.Success<long, InternalError>(6L),
                Result.Success<float, InternalError>(7.0f),
                Result.Success<byte, InternalError>((byte)8));

        await Assert.That(result.IsFailure).IsTrue();
        var error = result.Match<string>(_ => "", e => e.Message);
        await Assert.That(error).IsEqualTo("err4");
    }

    // SequenceAsync (sequential)

    [Test]
    public async Task SequenceAsync_all_successes_returns_values()
    {
        var tasks = new[]
        {
            Task.FromResult(Result.Success<int, InternalError>(1)),
            Task.FromResult(Result.Success<int, InternalError>(2)),
            Task.FromResult(Result.Success<int, InternalError>(3))
        };

        var result = await tasks.SequenceAsync();

        await Assert.That(result.IsSuccess).IsTrue();
        var values = result.Match(v => v.ToList(), _ => []);
        await Assert.That(values).Count().IsEqualTo(3);
        await Assert.That(values[0]).IsEqualTo(1);
        await Assert.That(values[1]).IsEqualTo(2);
        await Assert.That(values[2]).IsEqualTo(3);
    }

    [Test]
    public async Task SequenceAsync_first_failure_fails_fast()
    {
        var tasks = new[]
        {
            Task.FromResult(Result.Success<int, InternalError>(1)),
            Task.FromResult(Result.Failure<int, InternalError>(new InternalError { Message = "err" })),
            Task.FromResult(Result.Success<int, InternalError>(3))
        };

        var result = await tasks.SequenceAsync();

        await Assert.That(result.IsFailure).IsTrue();
        var error = result.Match<string>(_ => "", e => e.Message);
        await Assert.That(error).IsEqualTo("err");
    }

    // TraverseAsync (sequential)

    [Test]
    public async Task TraverseAsync_all_succeed_returns_mapped_values()
    {
        var source = new[] { 1, 2, 3 };

        var result = await source.TraverseAsync(async x =>
        {
            await Task.Yield();
            return Result.Success<string, InternalError>($"v{x}");
        });

        await Assert.That(result.IsSuccess).IsTrue();
        var values = result.Match(v => v.ToList(), _ => []);
        await Assert.That(values).Count().IsEqualTo(3);
        await Assert.That(values[0]).IsEqualTo("v1");
    }

    [Test]
    public async Task TraverseAsync_failure_fails_fast()
    {
        var source = new[] { 1, 2, 3 };

        var result = await source.TraverseAsync(async x =>
        {
            await Task.Yield();
            return x == 2
                ? Result.Failure<string, InternalError>(new InternalError { Message = "bad" })
                : Result.Success<string, InternalError>($"v{x}");
        });

        await Assert.That(result.IsFailure).IsTrue();
    }

    // PartitionAsync (sequential)

    [Test]
    public async Task PartitionAsync_splits_async_results()
    {
        var tasks = new[]
        {
            Task.FromResult(Result.Success<int, InternalError>(1)),
            Task.FromResult(Result.Failure<int, InternalError>(new InternalError { Message = "err1" })),
            Task.FromResult(Result.Success<int, InternalError>(3))
        };

        var (successes, failures) = await tasks.PartitionAsync();
        var successList = successes.ToList();
        var failureList = failures.ToList();

        await Assert.That(successList).Count().IsEqualTo(2);
        await Assert.That(successList[0]).IsEqualTo(1);
        await Assert.That(successList[1]).IsEqualTo(3);
        await Assert.That(failureList).Count().IsEqualTo(1);
        await Assert.That(failureList[0].Message).IsEqualTo("err1");
    }

    // ChooseAsync (sequential)

    [Test]
    public async Task ChooseAsync_filters_failures_from_async_results()
    {
        var tasks = new[]
        {
            Task.FromResult(Result.Success<int, InternalError>(1)),
            Task.FromResult(Result.Failure<int, InternalError>(new InternalError { Message = "err" })),
            Task.FromResult(Result.Success<int, InternalError>(3))
        };

        var chosen = (await tasks.ChooseAsync()).ToList();

        await Assert.That(chosen).Count().IsEqualTo(2);
        await Assert.That(chosen[0]).IsEqualTo(1);
        await Assert.That(chosen[1]).IsEqualTo(3);
    }

    // SequenceParallel

    [Test]
    public async Task SequenceParallel_all_successes_returns_values()
    {
        var tasks = new[]
        {
            Task.FromResult(Result.Success<int, InternalError>(1)),
            Task.FromResult(Result.Success<int, InternalError>(2)),
            Task.FromResult(Result.Success<int, InternalError>(3))
        };

        var result = await tasks.SequenceParallel();

        await Assert.That(result.IsSuccess).IsTrue();
        var values = result.Match(v => v.ToList(), _ => []);
        await Assert.That(values).Count().IsEqualTo(3);
    }

    [Test]
    public async Task SequenceParallel_with_failure_returns_failure()
    {
        var tasks = new[]
        {
            Task.FromResult(Result.Success<int, InternalError>(1)),
            Task.FromResult(Result.Failure<int, InternalError>(new InternalError { Message = "err" })),
            Task.FromResult(Result.Success<int, InternalError>(3))
        };

        var result = await tasks.SequenceParallel();

        await Assert.That(result.IsFailure).IsTrue();
    }

    // TraverseParallel

    [Test]
    public async Task TraverseParallel_all_succeed_returns_mapped_values()
    {
        var source = new[] { 1, 2, 3 };

        var result = await source.TraverseParallel(async x =>
        {
            await Task.Yield();
            return Result.Success<string, InternalError>($"v{x}");
        });

        await Assert.That(result.IsSuccess).IsTrue();
        var values = result.Match(v => v.ToList(), _ => []);
        await Assert.That(values).Count().IsEqualTo(3);
    }

    [Test]
    public async Task TraverseParallel_with_failure_returns_failure()
    {
        var source = new[] { 1, 2, 3 };

        var result = await source.TraverseParallel(async x =>
        {
            await Task.Yield();
            return x == 2
                ? Result.Failure<string, InternalError>(new InternalError { Message = "bad" })
                : Result.Success<string, InternalError>($"v{x}");
        });

        await Assert.That(result.IsFailure).IsTrue();
    }

    // PartitionParallel

    [Test]
    public async Task PartitionParallel_splits_concurrent_results()
    {
        var tasks = new[]
        {
            Task.FromResult(Result.Success<int, InternalError>(1)),
            Task.FromResult(Result.Failure<int, InternalError>(new InternalError { Message = "err" })),
            Task.FromResult(Result.Success<int, InternalError>(3))
        };

        var (successes, failures) = await tasks.PartitionParallel();

        await Assert.That(successes.ToList()).Count().IsEqualTo(2);
        await Assert.That(failures.ToList()).Count().IsEqualTo(1);
    }

    // ChooseParallel

    [Test]
    public async Task ChooseParallel_filters_failures_from_concurrent_results()
    {
        var tasks = new[]
        {
            Task.FromResult(Result.Success<int, InternalError>(1)),
            Task.FromResult(Result.Failure<int, InternalError>(new InternalError { Message = "err" })),
            Task.FromResult(Result.Success<int, InternalError>(3))
        };

        var chosen = (await tasks.ChooseParallel()).ToList();

        await Assert.That(chosen).Count().IsEqualTo(2);
        await Assert.That(chosen[0]).IsEqualTo(1);
        await Assert.That(chosen[1]).IsEqualTo(3);
    }

    // Concurrency barrier tests for *Parallel methods

    [Test]
    public async Task SequenceParallel_runs_tasks_concurrently()
    {
        var task1Started = new TaskCompletionSource();
        var task2Started = new TaskCompletionSource();

        var tasks = new[]
        {
            Task.Run(async () =>
            {
                task1Started.SetResult();
                await task2Started.Task;
                return Result.Success<int, InternalError>(1);
            }),
            Task.Run(async () =>
            {
                task2Started.SetResult();
                await task1Started.Task;
                return Result.Success<int, InternalError>(2);
            })
        };

        var result = await tasks.SequenceParallel();

        await Assert.That(result.IsSuccess).IsTrue();
    }

    [Test]
    public async Task TraverseParallel_runs_tasks_concurrently()
    {
        var task1Started = new TaskCompletionSource();
        var task2Started = new TaskCompletionSource();
        var items = new[] { 1, 2 };

        var result = await items.TraverseParallel(async x =>
        {
            if (x == 1)
            {
                task1Started.SetResult();
                await task2Started.Task;
            }
            else
            {
                task2Started.SetResult();
                await task1Started.Task;
            }
            return Result.Success<int, InternalError>(x);
        });

        await Assert.That(result.IsSuccess).IsTrue();
    }

    [Test]
    public async Task PartitionParallel_runs_tasks_concurrently()
    {
        var task1Started = new TaskCompletionSource();
        var task2Started = new TaskCompletionSource();

        var tasks = new[]
        {
            Task.Run(async () =>
            {
                task1Started.SetResult();
                await task2Started.Task;
                return Result.Success<int, InternalError>(1);
            }),
            Task.Run(async () =>
            {
                task2Started.SetResult();
                await task1Started.Task;
                return Result.Success<int, InternalError>(2);
            })
        };

        var (successes, _) = await tasks.PartitionParallel();

        await Assert.That(successes.ToList()).Count().IsEqualTo(2);
    }

    [Test]
    public async Task ChooseParallel_runs_tasks_concurrently()
    {
        var task1Started = new TaskCompletionSource();
        var task2Started = new TaskCompletionSource();

        var tasks = new[]
        {
            Task.Run(async () =>
            {
                task1Started.SetResult();
                await task2Started.Task;
                return Result.Success<int, InternalError>(1);
            }),
            Task.Run(async () =>
            {
                task2Started.SetResult();
                await task1Started.Task;
                return Result.Success<int, InternalError>(2);
            })
        };

        var chosen = (await tasks.ChooseParallel()).ToList();

        await Assert.That(chosen).Count().IsEqualTo(2);
    }
}

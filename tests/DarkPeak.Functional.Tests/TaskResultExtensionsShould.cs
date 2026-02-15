using DarkPeak.Functional;
using DarkPeak.Functional.Extensions;

namespace DarkPeak.Functional.Tests;

public class TaskResultExtensionsShould
{
    private static Task<Result<T, TError>> SuccessAsync<T, TError>(T value) where TError : Error =>
        Task.FromResult<Result<T, TError>>(Result.Success<T, TError>(value));

    private static Task<Result<T, TError>> FailureAsync<T, TError>(TError error) where TError : Error =>
        Task.FromResult<Result<T, TError>>(Result.Failure<T, TError>(error));

    #region Map

    [Test]
    public async Task Map_transforms_success_value()
    {
        var result = await SuccessAsync<int, Error>(5).Map(x => x * 2);

        await Assert.That(result.IsSuccess).IsTrue();
        await Assert.That(result.GetValueOrThrow()).IsEqualTo(10);
    }

    [Test]
    public async Task Map_preserves_failure()
    {
        var result = await FailureAsync<int, Error>(new ValidationError { Message = "err" })
            .Map(x => x * 2);

        await Assert.That(result.IsFailure).IsTrue();
    }

    [Test]
    public async Task MapAsync_transforms_success_value()
    {
        var result = await SuccessAsync<int, Error>(5).MapAsync(async x =>
        {
            await Task.Delay(1);
            return x * 2;
        });

        await Assert.That(result.IsSuccess).IsTrue();
        await Assert.That(result.GetValueOrThrow()).IsEqualTo(10);
    }

    #endregion

    #region MapError

    [Test]
    public async Task MapError_transforms_failure_error()
    {
        var result = await FailureAsync<int, ValidationError>(
            new ValidationError { Message = "Invalid" })
            .MapError(e => new NotFoundError { Message = $"Not found: {e.Message}" });

        await Assert.That(result.IsFailure).IsTrue();
        var error = result.Match(_ => (NotFoundError)null!, e => e);
        await Assert.That(error.Message).IsEqualTo("Not found: Invalid");
    }

    [Test]
    public async Task MapError_preserves_success()
    {
        var result = await SuccessAsync<int, ValidationError>(42)
            .MapError(e => new NotFoundError { Message = e.Message });

        await Assert.That(result.IsSuccess).IsTrue();
        await Assert.That(result.GetValueOrThrow()).IsEqualTo(42);
    }

    #endregion

    #region Bind

    [Test]
    public async Task Bind_chains_success_results()
    {
        var result = await SuccessAsync<int, Error>(42)
            .Bind(x => Result.Success<string, Error>($"Value: {x}"));

        await Assert.That(result.IsSuccess).IsTrue();
        await Assert.That(result.GetValueOrThrow()).IsEqualTo("Value: 42");
    }

    [Test]
    public async Task Bind_preserves_failure()
    {
        var result = await FailureAsync<int, Error>(new ValidationError { Message = "err" })
            .Bind(x => Result.Success<string, Error>($"Value: {x}"));

        await Assert.That(result.IsFailure).IsTrue();
    }

    [Test]
    public async Task BindAsync_chains_async_operations()
    {
        var result = await SuccessAsync<int, Error>(42).BindAsync(async x =>
        {
            await Task.Delay(1);
            return Result.Success<string, Error>($"Value: {x}");
        });

        await Assert.That(result.IsSuccess).IsTrue();
        await Assert.That(result.GetValueOrThrow()).IsEqualTo("Value: 42");
    }

    #endregion

    #region Match

    [Test]
    public async Task Match_success_calls_success_function()
    {
        var output = await SuccessAsync<int, Error>(42).Match(
            success: v => $"Success: {v}",
            failure: e => $"Failure: {e.Message}");

        await Assert.That(output).IsEqualTo("Success: 42");
    }

    [Test]
    public async Task Match_failure_calls_failure_function()
    {
        var output = await FailureAsync<int, Error>(new ValidationError { Message = "err" })
            .Match(
                success: v => $"Success: {v}",
                failure: e => $"Failure: {e.Message}");

        await Assert.That(output).IsEqualTo("Failure: err");
    }

    [Test]
    public async Task MatchAsync_success_calls_async_function()
    {
        var output = await SuccessAsync<int, Error>(42).MatchAsync(
            success: async v => { await Task.Delay(1); return $"Success: {v}"; },
            failure: async e => { await Task.Delay(1); return $"Failure: {e.Message}"; });

        await Assert.That(output).IsEqualTo("Success: 42");
    }

    #endregion

    #region Tap

    [Test]
    public async Task Tap_executes_action_on_success()
    {
        var executed = false;

        var result = await SuccessAsync<int, Error>(42).Tap(x => executed = true);

        await Assert.That(executed).IsTrue();
        await Assert.That(result.IsSuccess).IsTrue();
    }

    [Test]
    public async Task Tap_does_not_execute_on_failure()
    {
        var executed = false;

        await FailureAsync<int, Error>(new ValidationError { Message = "err" })
            .Tap(x => executed = true);

        await Assert.That(executed).IsFalse();
    }

    [Test]
    public async Task TapError_executes_action_on_failure()
    {
        var executed = false;

        await FailureAsync<int, Error>(new ValidationError { Message = "err" })
            .TapError(e => executed = true);

        await Assert.That(executed).IsTrue();
    }

    [Test]
    public async Task TapError_does_not_execute_on_success()
    {
        var executed = false;

        await SuccessAsync<int, Error>(42).TapError(e => executed = true);

        await Assert.That(executed).IsFalse();
    }

    #endregion

    #region GetValueOrDefault

    [Test]
    public async Task GetValueOrDefault_returns_value_on_success()
    {
        var value = await SuccessAsync<int, Error>(42).GetValueOrDefault(0);

        await Assert.That(value).IsEqualTo(42);
    }

    [Test]
    public async Task GetValueOrDefault_returns_default_on_failure()
    {
        var value = await FailureAsync<int, Error>(new ValidationError { Message = "err" })
            .GetValueOrDefault(99);

        await Assert.That(value).IsEqualTo(99);
    }

    [Test]
    public async Task GetValueOrDefault_with_factory_returns_value_on_success()
    {
        var value = await SuccessAsync<int, Error>(42).GetValueOrDefault(() => 0);

        await Assert.That(value).IsEqualTo(42);
    }

    [Test]
    public async Task GetValueOrDefault_with_factory_returns_default_on_failure()
    {
        var value = await FailureAsync<int, Error>(new ValidationError { Message = "err" })
            .GetValueOrDefault(() => 99);

        await Assert.That(value).IsEqualTo(99);
    }

    #endregion

    #region GetValueOrThrow

    [Test]
    public async Task GetValueOrThrow_returns_value_on_success()
    {
        var value = await SuccessAsync<int, Error>(42).GetValueOrThrow();

        await Assert.That(value).IsEqualTo(42);
    }

    [Test]
    public async Task GetValueOrThrow_throws_on_failure()
    {
        await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await FailureAsync<int, Error>(
                new ValidationError { Message = "err" }).GetValueOrThrow());
    }

    #endregion

    #region OrElse

    [Test]
    public async Task OrElse_returns_original_on_success()
    {
        var result = await SuccessAsync<int, Error>(42)
            .OrElse(Result.Success<int, Error>(99));

        await Assert.That(result.GetValueOrThrow()).IsEqualTo(42);
    }

    [Test]
    public async Task OrElse_returns_alternative_on_failure()
    {
        var result = await FailureAsync<int, Error>(new ValidationError { Message = "err" })
            .OrElse(Result.Success<int, Error>(99));

        await Assert.That(result.GetValueOrThrow()).IsEqualTo(99);
    }

    [Test]
    public async Task OrElse_with_factory_returns_original_on_success()
    {
        var result = await SuccessAsync<int, Error>(42)
            .OrElse(() => Result.Success<int, Error>(99));

        await Assert.That(result.GetValueOrThrow()).IsEqualTo(42);
    }

    [Test]
    public async Task OrElse_with_factory_returns_alternative_on_failure()
    {
        var result = await FailureAsync<int, Error>(new ValidationError { Message = "err" })
            .OrElse(() => Result.Success<int, Error>(99));

        await Assert.That(result.GetValueOrThrow()).IsEqualTo(99);
    }

    #endregion

    #region Chaining

    [Test]
    public async Task Fluent_chain_across_multiple_async_operations()
    {
        var output = await SuccessAsync<int, Error>(5)
            .Map(x => x * 2)
            .Bind(x => x > 5
                ? Result.Success<string, Error>($"Value: {x}")
                : Result.Failure<string, Error>(new ValidationError { Message = "Too small" }))
            .GetValueOrDefault("none");

        await Assert.That(output).IsEqualTo("Value: 10");
    }

    [Test]
    public async Task Fluent_chain_short_circuits_on_failure()
    {
        var output = await SuccessAsync<int, Error>(2)
            .Map(x => x * 2)
            .Bind(x => x > 5
                ? Result.Success<string, Error>($"Value: {x}")
                : Result.Failure<string, Error>(new ValidationError { Message = "Too small" }))
            .GetValueOrDefault("none");

        await Assert.That(output).IsEqualTo("none");
    }

    #endregion

    #region Join

    [Test]
    public async Task Join_two_successes_returns_tuple()
    {
        var result = await SuccessAsync<int, Error>(1).Join(SuccessAsync<string, Error>("two"));

        await Assert.That(result.IsSuccess).IsTrue();
        var (v1, v2) = result.GetValueOrThrow();
        await Assert.That(v1).IsEqualTo(1);
        await Assert.That(v2).IsEqualTo("two");
    }

    [Test]
    public async Task Join_first_failure_returns_failure()
    {
        var result = await FailureAsync<int, Error>(new ValidationError { Message = "err1" })
            .Join(SuccessAsync<string, Error>("two"));

        await Assert.That(result.IsFailure).IsTrue();
        var error = result.Match<string>(_ => "", e => e.Message);
        await Assert.That(error).IsEqualTo("err1");
    }

    [Test]
    public async Task Join_second_failure_returns_failure()
    {
        var result = await SuccessAsync<int, Error>(1)
            .Join(FailureAsync<string, Error>(new ValidationError { Message = "err2" }));

        await Assert.That(result.IsFailure).IsTrue();
        var error = result.Match<string>(_ => "", e => e.Message);
        await Assert.That(error).IsEqualTo("err2");
    }

    [Test]
    public async Task Join_runs_tasks_concurrently()
    {
        var task1Started = new TaskCompletionSource();
        var task2Started = new TaskCompletionSource();

        var first = Task.Run(async () =>
        {
            task1Started.SetResult();
            await task2Started.Task; // Wait for task2 to have started
            return Result.Success<int, Error>(1);
        });

        var second = Task.Run(async () =>
        {
            task2Started.SetResult();
            await task1Started.Task; // Wait for task1 to have started
            return Result.Success<string, Error>("two");
        });

        var result = await first.Join(second);

        await Assert.That(result.IsSuccess).IsTrue();
    }

    [Test]
    public async Task Join_three_successes_returns_tuple()
    {
        var result = await SuccessAsync<int, Error>(1)
            .Join(SuccessAsync<string, Error>("two"), SuccessAsync<bool, Error>(true));

        await Assert.That(result.IsSuccess).IsTrue();
        var (v1, v2, v3) = result.GetValueOrThrow();
        await Assert.That(v1).IsEqualTo(1);
        await Assert.That(v2).IsEqualTo("two");
        await Assert.That(v3).IsTrue();
    }

    [Test]
    public async Task Join_three_any_failure_returns_failure()
    {
        var result = await SuccessAsync<int, Error>(1)
            .Join(
                FailureAsync<string, Error>(new ValidationError { Message = "err2" }),
                SuccessAsync<bool, Error>(true));

        await Assert.That(result.IsFailure).IsTrue();
    }

    [Test]
    public async Task Join_four_successes_returns_tuple()
    {
        var result = await SuccessAsync<int, Error>(1)
            .Join(
                SuccessAsync<string, Error>("two"),
                SuccessAsync<bool, Error>(true),
                SuccessAsync<double, Error>(4.0));

        await Assert.That(result.IsSuccess).IsTrue();
        var (v1, v2, v3, v4) = result.GetValueOrThrow();
        await Assert.That(v1).IsEqualTo(1);
        await Assert.That(v4).IsEqualTo(4.0);
    }

    [Test]
    public async Task Join_four_any_failure_returns_failure()
    {
        var result = await SuccessAsync<int, Error>(1)
            .Join(
                SuccessAsync<string, Error>("two"),
                FailureAsync<bool, Error>(new ValidationError { Message = "err3" }),
                SuccessAsync<double, Error>(4.0));

        await Assert.That(result.IsFailure).IsTrue();
    }

    [Test]
    public async Task Join_five_successes_returns_tuple()
    {
        var result = await SuccessAsync<int, Error>(1)
            .Join(
                SuccessAsync<string, Error>("two"),
                SuccessAsync<bool, Error>(true),
                SuccessAsync<double, Error>(4.0),
                SuccessAsync<char, Error>('e'));

        await Assert.That(result.IsSuccess).IsTrue();
        var (v1, v2, v3, v4, v5) = result.GetValueOrThrow();
        await Assert.That(v1).IsEqualTo(1);
        await Assert.That(v5).IsEqualTo('e');
    }

    [Test]
    public async Task Join_five_any_failure_returns_failure()
    {
        var result = await FailureAsync<int, Error>(new ValidationError { Message = "err1" })
            .Join(
                SuccessAsync<string, Error>("two"),
                SuccessAsync<bool, Error>(true),
                SuccessAsync<double, Error>(4.0),
                SuccessAsync<char, Error>('e'));

        await Assert.That(result.IsFailure).IsTrue();
    }

    [Test]
    public async Task Join_six_successes_returns_tuple()
    {
        var result = await SuccessAsync<int, Error>(1)
            .Join(
                SuccessAsync<string, Error>("two"),
                SuccessAsync<bool, Error>(true),
                SuccessAsync<double, Error>(4.0),
                SuccessAsync<char, Error>('e'),
                SuccessAsync<long, Error>(6L));

        await Assert.That(result.IsSuccess).IsTrue();
        var (v1, v2, v3, v4, v5, v6) = result.GetValueOrThrow();
        await Assert.That(v1).IsEqualTo(1);
        await Assert.That(v6).IsEqualTo(6L);
    }

    [Test]
    public async Task Join_six_any_failure_returns_failure()
    {
        var result = await SuccessAsync<int, Error>(1)
            .Join(
                SuccessAsync<string, Error>("two"),
                SuccessAsync<bool, Error>(true),
                SuccessAsync<double, Error>(4.0),
                SuccessAsync<char, Error>('e'),
                FailureAsync<long, Error>(new ValidationError { Message = "err6" }));

        await Assert.That(result.IsFailure).IsTrue();
    }

    [Test]
    public async Task Join_seven_successes_returns_tuple()
    {
        var result = await SuccessAsync<int, Error>(1)
            .Join(
                SuccessAsync<string, Error>("two"),
                SuccessAsync<bool, Error>(true),
                SuccessAsync<double, Error>(4.0),
                SuccessAsync<char, Error>('e'),
                SuccessAsync<long, Error>(6L),
                SuccessAsync<float, Error>(7.0f));

        await Assert.That(result.IsSuccess).IsTrue();
        var (v1, v2, v3, v4, v5, v6, v7) = result.GetValueOrThrow();
        await Assert.That(v1).IsEqualTo(1);
        await Assert.That(v7).IsEqualTo(7.0f);
    }

    [Test]
    public async Task Join_seven_any_failure_returns_failure()
    {
        var result = await SuccessAsync<int, Error>(1)
            .Join(
                SuccessAsync<string, Error>("two"),
                FailureAsync<bool, Error>(new ValidationError { Message = "err3" }),
                SuccessAsync<double, Error>(4.0),
                SuccessAsync<char, Error>('e'),
                SuccessAsync<long, Error>(6L),
                SuccessAsync<float, Error>(7.0f));

        await Assert.That(result.IsFailure).IsTrue();
    }

    [Test]
    public async Task Join_eight_successes_returns_tuple()
    {
        var result = await SuccessAsync<int, Error>(1)
            .Join(
                SuccessAsync<string, Error>("two"),
                SuccessAsync<bool, Error>(true),
                SuccessAsync<double, Error>(4.0),
                SuccessAsync<char, Error>('e'),
                SuccessAsync<long, Error>(6L),
                SuccessAsync<float, Error>(7.0f),
                SuccessAsync<byte, Error>((byte)8));

        await Assert.That(result.IsSuccess).IsTrue();
        var (v1, v2, v3, v4, v5, v6, v7, v8) = result.GetValueOrThrow();
        await Assert.That(v1).IsEqualTo(1);
        await Assert.That(v8).IsEqualTo((byte)8);
    }

    [Test]
    public async Task Join_eight_any_failure_returns_failure()
    {
        var result = await SuccessAsync<int, Error>(1)
            .Join(
                SuccessAsync<string, Error>("two"),
                SuccessAsync<bool, Error>(true),
                FailureAsync<double, Error>(new ValidationError { Message = "err4" }),
                SuccessAsync<char, Error>('e'),
                SuccessAsync<long, Error>(6L),
                SuccessAsync<float, Error>(7.0f),
                SuccessAsync<byte, Error>((byte)8));

        await Assert.That(result.IsFailure).IsTrue();
    }

    [Test]
    public async Task Join_three_runs_tasks_concurrently()
    {
        var task1Started = new TaskCompletionSource();
        var task2Started = new TaskCompletionSource();
        var task3Started = new TaskCompletionSource();

        var first = Task.Run(async () =>
        {
            task1Started.SetResult();
            await task2Started.Task;
            await task3Started.Task;
            return Result.Success<int, Error>(1);
        });

        var second = Task.Run(async () =>
        {
            task2Started.SetResult();
            await task1Started.Task;
            await task3Started.Task;
            return Result.Success<string, Error>("two");
        });

        var third = Task.Run(async () =>
        {
            task3Started.SetResult();
            await task1Started.Task;
            await task2Started.Task;
            return Result.Success<bool, Error>(true);
        });

        var result = await first.Join(second, third);

        await Assert.That(result.IsSuccess).IsTrue();
    }

    #endregion
}

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
}

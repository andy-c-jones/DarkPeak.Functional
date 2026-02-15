using DarkPeak.Functional;
using DarkPeak.Functional.Extensions;

namespace DarkPeak.Functional.Tests;

/// <summary>
/// Tests for Pipeline (Pipe, Compose, Pipeline builder).
/// </summary>
public class PipelineShould
{
    #region Pipe

    [Test]
    public async Task Pipe_applies_function_to_value()
    {
        var result = 5.Pipe(x => x * 2);

        await Assert.That(result).IsEqualTo(10);
    }

    [Test]
    public async Task Pipe_chains_multiple_functions()
    {
        var result = "hello"
            .Pipe(s => s.ToUpper())
            .Pipe(s => s.Length);

        await Assert.That(result).IsEqualTo(5);
    }

    [Test]
    public async Task Pipe_into_result_returning_function()
    {
        var result = 42.Pipe(x =>
            x > 0
                ? Result.Success<int, InternalError>(x)
                : Result.Failure<int, InternalError>(new InternalError { Message = "must be positive" }));

        await Assert.That(result.IsSuccess).IsTrue();
        await Assert.That(result.GetValueOrThrow()).IsEqualTo(42);
    }

    [Test]
    public async Task Pipe_into_option_returning_function()
    {
        var result = "hello".Pipe(s => s.Length > 0 ? Option.Some(s) : Option.None<string>());

        await Assert.That(result.IsSome).IsTrue();
    }

    [Test]
    public async Task PipeAsync_applies_async_function()
    {
        var result = await 5.PipeAsync(async x =>
        {
            await Task.Delay(1);
            return x * 3;
        });

        await Assert.That(result).IsEqualTo(15);
    }

    #endregion

    #region Compose

    [Test]
    public async Task Compose_creates_forward_composition()
    {
        Func<int, int> doubleIt = x => x * 2;
        Func<int, string> toString = x => x.ToString();

        var composed = doubleIt.Compose(toString);

        await Assert.That(composed(21)).IsEqualTo("42");
    }

    [Test]
    public async Task Compose_chains_multiple_functions()
    {
        Func<string, string> trim = s => s.Trim();
        Func<string, string> upper = s => s.ToUpper();
        Func<string, int> length = s => s.Length;

        var composed = trim.Compose(upper).Compose(length);

        await Assert.That(composed("  hello  ")).IsEqualTo(5);
    }

    [Test]
    public async Task ComposeAsync_creates_async_composition()
    {
        Func<int, int> doubleIt = x => x * 2;
        Func<int, Task<string>> toStringAsync = async x =>
        {
            await Task.Delay(1);
            return x.ToString();
        };

        var composed = doubleIt.ComposeAsync(toStringAsync);

        await Assert.That(await composed(21)).IsEqualTo("42");
    }

    [Test]
    public async Task Compose_result_aware_chains_successes()
    {
        Func<int, Result<int, InternalError>> validate = x =>
            x > 0
                ? Result.Success<int, InternalError>(x)
                : Result.Failure<int, InternalError>(new InternalError { Message = "must be positive" });

        Func<int, Result<string, InternalError>> format = x =>
            Result.Success<string, InternalError>($"Value: {x}");

        var composed = validate.Compose(format);

        var result = composed(42);
        await Assert.That(result.IsSuccess).IsTrue();
        await Assert.That(result.GetValueOrThrow()).IsEqualTo("Value: 42");
    }

    [Test]
    public async Task Compose_result_aware_short_circuits_on_failure()
    {
        Func<int, Result<int, InternalError>> validate = x =>
            x > 0
                ? Result.Success<int, InternalError>(x)
                : Result.Failure<int, InternalError>(new InternalError { Message = "must be positive" });

        Func<int, Result<string, InternalError>> format = x =>
            Result.Success<string, InternalError>($"Value: {x}");

        var composed = validate.Compose(format);

        var result = composed(-1);
        await Assert.That(result.IsFailure).IsTrue();
    }

    [Test]
    public async Task ComposeAsync_result_aware_chains_async_successes()
    {
        Func<int, Task<Result<int, InternalError>>> validate = async x =>
        {
            await Task.Delay(1);
            return x > 0
                ? Result.Success<int, InternalError>(x)
                : Result.Failure<int, InternalError>(new InternalError { Message = "must be positive" });
        };

        Func<int, Task<Result<string, InternalError>>> format = async x =>
        {
            await Task.Delay(1);
            return Result.Success<string, InternalError>($"Value: {x}");
        };

        var composed = validate.ComposeAsync(format);

        var result = await composed(42);
        await Assert.That(result.IsSuccess).IsTrue();
        await Assert.That(result.GetValueOrThrow()).IsEqualTo("Value: 42");
    }

    [Test]
    public async Task Compose_option_aware_chains_somes()
    {
        Func<string, Option<int>> tryParse = s =>
            int.TryParse(s, out var n) ? Option.Some(n) : Option.None<int>();

        Func<int, Option<string>> format = n =>
            n > 0 ? Option.Some($"Positive: {n}") : Option.None<string>();

        var composed = tryParse.Compose(format);

        var result = composed("42");
        await Assert.That(result.IsSome).IsTrue();
        await Assert.That(result.GetValueOrThrow()).IsEqualTo("Positive: 42");
    }

    [Test]
    public async Task Compose_option_aware_short_circuits_on_none()
    {
        Func<string, Option<int>> tryParse = s =>
            int.TryParse(s, out var n) ? Option.Some(n) : Option.None<int>();

        Func<int, Option<string>> format = n =>
            n > 0 ? Option.Some($"Positive: {n}") : Option.None<string>();

        var composed = tryParse.Compose(format);

        var result = composed("not a number");
        await Assert.That(result.IsSome).IsFalse();
    }

    #endregion

    #region Pipeline Builder

    [Test]
    public async Task Pipeline_executes_all_steps_on_success()
    {
        var pipeline = Pipeline.Create<int, InternalError>()
            .Then(x => Result.Success<int, InternalError>(x * 2))
            .Then(x => Result.Success<string, InternalError>($"Value: {x}"))
            .Build();

        var result = pipeline(21);

        await Assert.That(result.IsSuccess).IsTrue();
        await Assert.That(result.GetValueOrThrow()).IsEqualTo("Value: 42");
    }

    [Test]
    public async Task Pipeline_short_circuits_on_failure()
    {
        var lastStepCalled = false;

        var pipeline = Pipeline.Create<int, InternalError>()
            .Then<int>(_ => Result.Failure<int, InternalError>(new InternalError { Message = "fail" }))
            .Then(x =>
            {
                lastStepCalled = true;
                return Result.Success<string, InternalError>($"Value: {x}");
            })
            .Build();

        var result = pipeline(21);

        await Assert.That(result.IsFailure).IsTrue();
        await Assert.That(lastStepCalled).IsFalse();
    }

    [Test]
    public async Task Pipeline_supports_plain_mapping_steps()
    {
        var pipeline = Pipeline.Create<string, InternalError>()
            .Then(s => s.Trim())
            .Then(s => s.ToUpper())
            .Then(s => s.Length)
            .Build();

        var result = pipeline("  hello  ");

        await Assert.That(result.IsSuccess).IsTrue();
        await Assert.That(result.GetValueOrThrow()).IsEqualTo(5);
    }

    [Test]
    public async Task Pipeline_async_executes_all_steps()
    {
        var pipeline = Pipeline.Create<int, InternalError>()
            .ThenAsync(async x =>
            {
                await Task.Delay(1);
                return Result.Success<int, InternalError>(x * 2);
            })
            .Then(x => Result.Success<string, InternalError>($"Value: {x}"))
            .Build();

        var result = await pipeline(21);

        await Assert.That(result.IsSuccess).IsTrue();
        await Assert.That(result.GetValueOrThrow()).IsEqualTo("Value: 42");
    }

    [Test]
    public async Task Pipeline_async_short_circuits_on_failure()
    {
        var lastStepCalled = false;

        var pipeline = Pipeline.Create<int, InternalError>()
            .ThenAsync<int>(async _ =>
            {
                await Task.Delay(1);
                return Result.Failure<int, InternalError>(new InternalError { Message = "fail" });
            })
            .Then(x =>
            {
                lastStepCalled = true;
                return Result.Success<string, InternalError>($"Value: {x}");
            })
            .Build();

        var result = await pipeline(21);

        await Assert.That(result.IsFailure).IsTrue();
        await Assert.That(lastStepCalled).IsFalse();
    }

    [Test]
    public async Task Pipeline_mixed_sync_and_async_steps()
    {
        var pipeline = Pipeline.Create<string, InternalError>()
            .Then(s => s.Trim())
            .ThenAsync(async s =>
            {
                await Task.Delay(1);
                return Result.Success<string, InternalError>(s.ToUpper());
            })
            .Then(s => s.Length)
            .Build();

        var result = await pipeline("  hello  ");

        await Assert.That(result.IsSuccess).IsTrue();
        await Assert.That(result.GetValueOrThrow()).IsEqualTo(5);
    }

    [Test]
    public async Task Pipeline_async_then_async_chains()
    {
        var pipeline = Pipeline.Create<int, InternalError>()
            .ThenAsync(async x =>
            {
                await Task.Delay(1);
                return Result.Success<int, InternalError>(x + 10);
            })
            .ThenAsync(async x =>
            {
                await Task.Delay(1);
                return Result.Success<string, InternalError>($"Result: {x}");
            })
            .Build();

        var result = await pipeline(5);

        await Assert.That(result.IsSuccess).IsTrue();
        await Assert.That(result.GetValueOrThrow()).IsEqualTo("Result: 15");
    }

    [Test]
    public async Task Pipeline_async_plain_mapping_step()
    {
        var pipeline = Pipeline.Create<int, InternalError>()
            .ThenAsync(async x =>
            {
                await Task.Delay(1);
                return Result.Success<int, InternalError>(x * 2);
            })
            .Then(x => x.ToString())
            .Build();

        var result = await pipeline(21);

        await Assert.That(result.IsSuccess).IsTrue();
        await Assert.That(result.GetValueOrThrow()).IsEqualTo("42");
    }

    #endregion
}

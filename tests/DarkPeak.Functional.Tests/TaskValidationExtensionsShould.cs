using DarkPeak.Functional;
using DarkPeak.Functional.Extensions;

namespace DarkPeak.Functional.Tests;

public class TaskValidationExtensionsShould
{
    private static Task<Validation<T, TError>> ValidAsync<T, TError>(T value) where TError : Error =>
        Task.FromResult<Validation<T, TError>>(Validation.Valid<T, TError>(value));

    private static Task<Validation<T, TError>> InvalidAsync<T, TError>(TError error) where TError : Error =>
        Task.FromResult<Validation<T, TError>>(Validation.Invalid<T, TError>(error));

    #region Map

    [Test]
    public async Task Map_transforms_valid_value()
    {
        var result = await ValidAsync<int, Error>(5).Map(x => x * 2);

        await Assert.That(result.IsValid).IsTrue();
        await Assert.That(result.GetValueOrThrow()).IsEqualTo(10);
    }

    [Test]
    public async Task Map_preserves_invalid()
    {
        var result = await InvalidAsync<int, Error>(new ValidationError { Message = "err" })
            .Map(x => x * 2);

        await Assert.That(result.IsInvalid).IsTrue();
    }

    [Test]
    public async Task MapAsync_transforms_valid_value()
    {
        var result = await ValidAsync<int, Error>(5).MapAsync(async x =>
        {
            await Task.Delay(1);
            return x * 2;
        });

        await Assert.That(result.IsValid).IsTrue();
        await Assert.That(result.GetValueOrThrow()).IsEqualTo(10);
    }

    [Test]
    public async Task MapAsync_preserves_invalid()
    {
        var result = await InvalidAsync<int, Error>(new ValidationError { Message = "err" })
            .MapAsync(async x => { await Task.Delay(1); return x * 2; });

        await Assert.That(result.IsInvalid).IsTrue();
    }

    #endregion

    #region Bind

    [Test]
    public async Task Bind_chains_valid_validations()
    {
        var result = await ValidAsync<int, Error>(42)
            .Bind(x => Validation.Valid<string, Error>($"Value: {x}"));

        await Assert.That(result.IsValid).IsTrue();
        await Assert.That(result.GetValueOrThrow()).IsEqualTo("Value: 42");
    }

    [Test]
    public async Task Bind_preserves_invalid()
    {
        var result = await InvalidAsync<int, Error>(new ValidationError { Message = "err" })
            .Bind(x => Validation.Valid<string, Error>($"Value: {x}"));

        await Assert.That(result.IsInvalid).IsTrue();
    }

    [Test]
    public async Task BindAsync_chains_async_operations()
    {
        var result = await ValidAsync<int, Error>(42).BindAsync(async x =>
        {
            await Task.Delay(1);
            return Validation.Valid<string, Error>($"Value: {x}");
        });

        await Assert.That(result.IsValid).IsTrue();
        await Assert.That(result.GetValueOrThrow()).IsEqualTo("Value: 42");
    }

    [Test]
    public async Task BindAsync_preserves_invalid()
    {
        var result = await InvalidAsync<int, Error>(new ValidationError { Message = "err" })
            .BindAsync(async x =>
            {
                await Task.Delay(1);
                return Validation.Valid<string, Error>($"Value: {x}");
            });

        await Assert.That(result.IsInvalid).IsTrue();
    }

    #endregion

    #region Match

    [Test]
    public async Task Match_valid_calls_valid_function()
    {
        var output = await ValidAsync<int, Error>(42).Match(
            valid: v => $"Value: {v}",
            invalid: errs => $"Errors: {errs.Count}");

        await Assert.That(output).IsEqualTo("Value: 42");
    }

    [Test]
    public async Task Match_invalid_calls_invalid_function()
    {
        var output = await InvalidAsync<int, Error>(new ValidationError { Message = "err" })
            .Match(
                valid: v => $"Value: {v}",
                invalid: errs => $"Errors: {errs.Count}");

        await Assert.That(output).IsEqualTo("Errors: 1");
    }

    [Test]
    public async Task MatchAsync_valid_calls_async_function()
    {
        var output = await ValidAsync<int, Error>(42).MatchAsync(
            valid: async v => { await Task.Delay(1); return $"Value: {v}"; },
            invalid: async errs => { await Task.Delay(1); return $"Errors: {errs.Count}"; });

        await Assert.That(output).IsEqualTo("Value: 42");
    }

    [Test]
    public async Task MatchAsync_invalid_calls_async_function()
    {
        var output = await InvalidAsync<int, Error>(new ValidationError { Message = "err" })
            .MatchAsync(
                valid: async v => { await Task.Delay(1); return $"Value: {v}"; },
                invalid: async errs => { await Task.Delay(1); return $"Errors: {errs.Count}"; });

        await Assert.That(output).IsEqualTo("Errors: 1");
    }

    #endregion

    #region Tap

    [Test]
    public async Task Tap_executes_action_on_valid()
    {
        var executed = false;

        var result = await ValidAsync<int, Error>(42).Tap(x => executed = true);

        await Assert.That(executed).IsTrue();
        await Assert.That(result.IsValid).IsTrue();
    }

    [Test]
    public async Task Tap_does_not_execute_on_invalid()
    {
        var executed = false;

        await InvalidAsync<int, Error>(new ValidationError { Message = "err" })
            .Tap(x => executed = true);

        await Assert.That(executed).IsFalse();
    }

    [Test]
    public async Task TapInvalid_executes_action_on_invalid()
    {
        var executed = false;

        await InvalidAsync<int, Error>(new ValidationError { Message = "err" })
            .TapInvalid(errs => executed = true);

        await Assert.That(executed).IsTrue();
    }

    [Test]
    public async Task TapInvalid_does_not_execute_on_valid()
    {
        var executed = false;

        await ValidAsync<int, Error>(42).TapInvalid(errs => executed = true);

        await Assert.That(executed).IsFalse();
    }

    #endregion

    #region GetValueOrDefault / GetValueOrThrow

    [Test]
    public async Task GetValueOrDefault_returns_value_on_valid()
    {
        var value = await ValidAsync<int, Error>(42).GetValueOrDefault(0);

        await Assert.That(value).IsEqualTo(42);
    }

    [Test]
    public async Task GetValueOrDefault_returns_default_on_invalid()
    {
        var value = await InvalidAsync<int, Error>(new ValidationError { Message = "err" })
            .GetValueOrDefault(99);

        await Assert.That(value).IsEqualTo(99);
    }

    [Test]
    public async Task GetValueOrDefault_with_factory_returns_value_on_valid()
    {
        var value = await ValidAsync<int, Error>(42).GetValueOrDefault(() => 0);

        await Assert.That(value).IsEqualTo(42);
    }

    [Test]
    public async Task GetValueOrDefault_with_factory_returns_default_on_invalid()
    {
        var value = await InvalidAsync<int, Error>(new ValidationError { Message = "err" })
            .GetValueOrDefault(() => 99);

        await Assert.That(value).IsEqualTo(99);
    }

    [Test]
    public async Task GetValueOrThrow_returns_value_on_valid()
    {
        var value = await ValidAsync<int, Error>(42).GetValueOrThrow();

        await Assert.That(value).IsEqualTo(42);
    }

    [Test]
    public async Task GetValueOrThrow_throws_on_invalid()
    {
        await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await InvalidAsync<int, Error>(
                new ValidationError { Message = "err" }).GetValueOrThrow());
    }

    #endregion

    #region ToResult

    [Test]
    public async Task ToResult_converts_valid_to_success()
    {
        var result = await ValidAsync<int, Error>(42).ToResult();

        await Assert.That(result.IsSuccess).IsTrue();
        await Assert.That(result.GetValueOrThrow()).IsEqualTo(42);
    }

    [Test]
    public async Task ToResult_converts_invalid_to_failure()
    {
        var result = await InvalidAsync<int, Error>(new ValidationError { Message = "err" }).ToResult();

        await Assert.That(result.IsFailure).IsTrue();
    }

    #endregion

    #region ZipWithAsync (2-arity)

    [Test]
    public async Task ZipWithAsync_two_valid_produces_combined_result()
    {
        var result = await ValidAsync<string, Error>("Alice")
            .ZipWithAsync(
                ValidAsync<int, Error>(30),
                (n, a) => $"{n} is {a}");

        await Assert.That(result.IsValid).IsTrue();
        await Assert.That(result.GetValueOrThrow()).IsEqualTo("Alice is 30");
    }

    [Test]
    public async Task ZipWithAsync_two_invalid_accumulates_errors()
    {
        var result = await InvalidAsync<string, Error>(new ValidationError { Message = "e1" })
            .ZipWithAsync(
                InvalidAsync<int, Error>(new ValidationError { Message = "e2" }),
                (n, a) => $"{n} is {a}");

        await Assert.That(result.IsInvalid).IsTrue();
        var errors = result.Match(_ => Array.Empty<Error>(), errs => errs.ToArray());
        await Assert.That(errors.Length).IsEqualTo(2);
    }

    #endregion

    #region ZipWithAsync (3-arity)

    [Test]
    public async Task ZipWithAsync_three_valid_produces_combined_result()
    {
        var result = await ValidAsync<string, Error>("Alice")
            .ZipWithAsync(
                ValidAsync<int, Error>(30),
                ValidAsync<string, Error>("London"),
                (n, a, c) => $"{n}, {a}, {c}");

        await Assert.That(result.IsValid).IsTrue();
        await Assert.That(result.GetValueOrThrow()).IsEqualTo("Alice, 30, London");
    }

    [Test]
    public async Task ZipWithAsync_three_invalid_accumulates_errors()
    {
        var result = await InvalidAsync<string, Error>(new ValidationError { Message = "e1" })
            .ZipWithAsync(
                InvalidAsync<int, Error>(new ValidationError { Message = "e2" }),
                InvalidAsync<string, Error>(new ValidationError { Message = "e3" }),
                (a, b, c) => "");

        await Assert.That(result.IsInvalid).IsTrue();
        var errors = result.Match(_ => Array.Empty<Error>(), errs => errs.ToArray());
        await Assert.That(errors.Length).IsEqualTo(3);
    }

    #endregion

    #region ZipWithAsync (4-arity)

    [Test]
    public async Task ZipWithAsync_four_valid_combines_values()
    {
        var result = await ValidAsync<int, Error>(1)
            .ZipWithAsync(
                ValidAsync<int, Error>(2),
                ValidAsync<int, Error>(3),
                ValidAsync<int, Error>(4),
                (a, b, c, d) => a + b + c + d);

        await Assert.That(result.IsValid).IsTrue();
        await Assert.That(result.GetValueOrThrow()).IsEqualTo(10);
    }

    [Test]
    public async Task ZipWithAsync_four_invalid_accumulates_errors()
    {
        var result = await InvalidAsync<int, Error>(new ValidationError { Message = "e1" })
            .ZipWithAsync(
                InvalidAsync<int, Error>(new ValidationError { Message = "e2" }),
                InvalidAsync<int, Error>(new ValidationError { Message = "e3" }),
                InvalidAsync<int, Error>(new ValidationError { Message = "e4" }),
                (a, b, c, d) => a + b + c + d);

        await Assert.That(result.IsInvalid).IsTrue();
        var errors = result.Match(_ => Array.Empty<Error>(), errs => errs.ToArray());
        await Assert.That(errors.Length).IsEqualTo(4);
    }

    #endregion

    #region ZipWithAsync (5-arity)

    [Test]
    public async Task ZipWithAsync_five_valid_combines_values()
    {
        var result = await ValidAsync<int, Error>(1)
            .ZipWithAsync(
                ValidAsync<int, Error>(2),
                ValidAsync<int, Error>(3),
                ValidAsync<int, Error>(4),
                ValidAsync<int, Error>(5),
                (a, b, c, d, e) => a + b + c + d + e);

        await Assert.That(result.IsValid).IsTrue();
        await Assert.That(result.GetValueOrThrow()).IsEqualTo(15);
    }

    [Test]
    public async Task ZipWithAsync_five_invalid_accumulates_errors()
    {
        var result = await InvalidAsync<int, Error>(new ValidationError { Message = "e1" })
            .ZipWithAsync(
                InvalidAsync<int, Error>(new ValidationError { Message = "e2" }),
                InvalidAsync<int, Error>(new ValidationError { Message = "e3" }),
                InvalidAsync<int, Error>(new ValidationError { Message = "e4" }),
                InvalidAsync<int, Error>(new ValidationError { Message = "e5" }),
                (a, b, c, d, e) => a + b + c + d + e);

        await Assert.That(result.IsInvalid).IsTrue();
        var errors = result.Match(_ => Array.Empty<Error>(), errs => errs.ToArray());
        await Assert.That(errors.Length).IsEqualTo(5);
    }

    #endregion

    #region ZipWithAsync (6-arity)

    [Test]
    public async Task ZipWithAsync_six_valid_combines_values()
    {
        var result = await ValidAsync<int, Error>(1)
            .ZipWithAsync(
                ValidAsync<int, Error>(2),
                ValidAsync<int, Error>(3),
                ValidAsync<int, Error>(4),
                ValidAsync<int, Error>(5),
                ValidAsync<int, Error>(6),
                (a, b, c, d, e, f) => a + b + c + d + e + f);

        await Assert.That(result.IsValid).IsTrue();
        await Assert.That(result.GetValueOrThrow()).IsEqualTo(21);
    }

    [Test]
    public async Task ZipWithAsync_six_invalid_accumulates_errors()
    {
        var result = await InvalidAsync<int, Error>(new ValidationError { Message = "e1" })
            .ZipWithAsync(
                InvalidAsync<int, Error>(new ValidationError { Message = "e2" }),
                InvalidAsync<int, Error>(new ValidationError { Message = "e3" }),
                InvalidAsync<int, Error>(new ValidationError { Message = "e4" }),
                InvalidAsync<int, Error>(new ValidationError { Message = "e5" }),
                InvalidAsync<int, Error>(new ValidationError { Message = "e6" }),
                (a, b, c, d, e, f) => a + b + c + d + e + f);

        await Assert.That(result.IsInvalid).IsTrue();
        var errors = result.Match(_ => Array.Empty<Error>(), errs => errs.ToArray());
        await Assert.That(errors.Length).IsEqualTo(6);
    }

    #endregion

    #region ZipWithAsync (7-arity)

    [Test]
    public async Task ZipWithAsync_seven_valid_combines_values()
    {
        var result = await ValidAsync<int, Error>(1)
            .ZipWithAsync(
                ValidAsync<int, Error>(2),
                ValidAsync<int, Error>(3),
                ValidAsync<int, Error>(4),
                ValidAsync<int, Error>(5),
                ValidAsync<int, Error>(6),
                ValidAsync<int, Error>(7),
                (a, b, c, d, e, f, g) => a + b + c + d + e + f + g);

        await Assert.That(result.IsValid).IsTrue();
        await Assert.That(result.GetValueOrThrow()).IsEqualTo(28);
    }

    [Test]
    public async Task ZipWithAsync_seven_invalid_accumulates_errors()
    {
        var result = await InvalidAsync<int, Error>(new ValidationError { Message = "e1" })
            .ZipWithAsync(
                InvalidAsync<int, Error>(new ValidationError { Message = "e2" }),
                InvalidAsync<int, Error>(new ValidationError { Message = "e3" }),
                InvalidAsync<int, Error>(new ValidationError { Message = "e4" }),
                InvalidAsync<int, Error>(new ValidationError { Message = "e5" }),
                InvalidAsync<int, Error>(new ValidationError { Message = "e6" }),
                InvalidAsync<int, Error>(new ValidationError { Message = "e7" }),
                (a, b, c, d, e, f, g) => a + b + c + d + e + f + g);

        await Assert.That(result.IsInvalid).IsTrue();
        var errors = result.Match(_ => Array.Empty<Error>(), errs => errs.ToArray());
        await Assert.That(errors.Length).IsEqualTo(7);
    }

    #endregion

    #region ZipWithAsync (8-arity)

    [Test]
    public async Task ZipWithAsync_eight_valid_combines_values()
    {
        var result = await ValidAsync<int, Error>(1)
            .ZipWithAsync(
                ValidAsync<int, Error>(2),
                ValidAsync<int, Error>(3),
                ValidAsync<int, Error>(4),
                ValidAsync<int, Error>(5),
                ValidAsync<int, Error>(6),
                ValidAsync<int, Error>(7),
                ValidAsync<int, Error>(8),
                (a, b, c, d, e, f, g, h) => a + b + c + d + e + f + g + h);

        await Assert.That(result.IsValid).IsTrue();
        await Assert.That(result.GetValueOrThrow()).IsEqualTo(36);
    }

    [Test]
    public async Task ZipWithAsync_eight_invalid_accumulates_errors()
    {
        var result = await InvalidAsync<int, Error>(new ValidationError { Message = "e1" })
            .ZipWithAsync(
                InvalidAsync<int, Error>(new ValidationError { Message = "e2" }),
                InvalidAsync<int, Error>(new ValidationError { Message = "e3" }),
                InvalidAsync<int, Error>(new ValidationError { Message = "e4" }),
                InvalidAsync<int, Error>(new ValidationError { Message = "e5" }),
                InvalidAsync<int, Error>(new ValidationError { Message = "e6" }),
                InvalidAsync<int, Error>(new ValidationError { Message = "e7" }),
                InvalidAsync<int, Error>(new ValidationError { Message = "e8" }),
                (a, b, c, d, e, f, g, h) => a + b + c + d + e + f + g + h);

        await Assert.That(result.IsInvalid).IsTrue();
        var errors = result.Match(_ => Array.Empty<Error>(), errs => errs.ToArray());
        await Assert.That(errors.Length).IsEqualTo(8);
    }

    #endregion

    #region ZipWithAsync concurrency

    [Test]
    public async Task ZipWithAsync_two_runs_tasks_concurrently()
    {
        var task1Started = new TaskCompletionSource();
        var task2Started = new TaskCompletionSource();

        var first = Task.Run(async () =>
        {
            task1Started.SetResult();
            await task2Started.Task;
            return Validation.Valid<int, Error>(1);
        });

        var second = Task.Run(async () =>
        {
            task2Started.SetResult();
            await task1Started.Task;
            return Validation.Valid<int, Error>(2);
        });

        var result = await first.ZipWithAsync(second, (a, b) => a + b);

        await Assert.That(result.IsValid).IsTrue();
        await Assert.That(result.GetValueOrThrow()).IsEqualTo(3);
    }

    #endregion

    #region Join (2-arity)

    [Test]
    public async Task Join_two_valid_returns_tuple()
    {
        var result = await ValidAsync<int, Error>(1).Join(ValidAsync<string, Error>("two"));

        await Assert.That(result.IsValid).IsTrue();
        var (v1, v2) = result.GetValueOrThrow();
        await Assert.That(v1).IsEqualTo(1);
        await Assert.That(v2).IsEqualTo("two");
    }

    [Test]
    public async Task Join_two_both_invalid_accumulates_errors()
    {
        var result = await InvalidAsync<int, Error>(new ValidationError { Message = "e1" })
            .Join(InvalidAsync<string, Error>(new ValidationError { Message = "e2" }));

        await Assert.That(result.IsInvalid).IsTrue();
        var errors = result.Match(_ => Array.Empty<Error>(), errs => errs.ToArray());
        await Assert.That(errors.Length).IsEqualTo(2);
    }

    #endregion

    #region Join (3-arity)

    [Test]
    public async Task Join_three_valid_returns_tuple()
    {
        var result = await ValidAsync<int, Error>(1)
            .Join(ValidAsync<string, Error>("two"), ValidAsync<bool, Error>(true));

        await Assert.That(result.IsValid).IsTrue();
        var (v1, v2, v3) = result.GetValueOrThrow();
        await Assert.That(v1).IsEqualTo(1);
        await Assert.That(v2).IsEqualTo("two");
        await Assert.That(v3).IsTrue();
    }

    [Test]
    public async Task Join_three_all_invalid_accumulates_errors()
    {
        var result = await InvalidAsync<int, Error>(new ValidationError { Message = "e1" })
            .Join(
                InvalidAsync<string, Error>(new ValidationError { Message = "e2" }),
                InvalidAsync<bool, Error>(new ValidationError { Message = "e3" }));

        await Assert.That(result.IsInvalid).IsTrue();
        var errors = result.Match(_ => Array.Empty<Error>(), errs => errs.ToArray());
        await Assert.That(errors.Length).IsEqualTo(3);
    }

    #endregion

    #region Join (4-arity)

    [Test]
    public async Task Join_four_valid_returns_tuple()
    {
        var result = await ValidAsync<int, Error>(1)
            .Join(
                ValidAsync<string, Error>("two"),
                ValidAsync<bool, Error>(true),
                ValidAsync<double, Error>(4.0));

        await Assert.That(result.IsValid).IsTrue();
        var (v1, v2, v3, v4) = result.GetValueOrThrow();
        await Assert.That(v1).IsEqualTo(1);
        await Assert.That(v4).IsEqualTo(4.0);
    }

    [Test]
    public async Task Join_four_all_invalid_accumulates_errors()
    {
        var result = await InvalidAsync<int, Error>(new ValidationError { Message = "e1" })
            .Join(
                InvalidAsync<string, Error>(new ValidationError { Message = "e2" }),
                InvalidAsync<bool, Error>(new ValidationError { Message = "e3" }),
                InvalidAsync<double, Error>(new ValidationError { Message = "e4" }));

        await Assert.That(result.IsInvalid).IsTrue();
        var errors = result.Match(_ => Array.Empty<Error>(), errs => errs.ToArray());
        await Assert.That(errors.Length).IsEqualTo(4);
    }

    #endregion

    #region Join (5-arity)

    [Test]
    public async Task Join_five_valid_returns_tuple()
    {
        var result = await ValidAsync<int, Error>(1)
            .Join(
                ValidAsync<string, Error>("two"),
                ValidAsync<bool, Error>(true),
                ValidAsync<double, Error>(4.0),
                ValidAsync<char, Error>('e'));

        await Assert.That(result.IsValid).IsTrue();
        var (v1, v2, v3, v4, v5) = result.GetValueOrThrow();
        await Assert.That(v1).IsEqualTo(1);
        await Assert.That(v5).IsEqualTo('e');
    }

    [Test]
    public async Task Join_five_all_invalid_accumulates_errors()
    {
        var result = await InvalidAsync<int, Error>(new ValidationError { Message = "e1" })
            .Join(
                InvalidAsync<string, Error>(new ValidationError { Message = "e2" }),
                InvalidAsync<bool, Error>(new ValidationError { Message = "e3" }),
                InvalidAsync<double, Error>(new ValidationError { Message = "e4" }),
                InvalidAsync<char, Error>(new ValidationError { Message = "e5" }));

        await Assert.That(result.IsInvalid).IsTrue();
        var errors = result.Match(_ => Array.Empty<Error>(), errs => errs.ToArray());
        await Assert.That(errors.Length).IsEqualTo(5);
    }

    #endregion

    #region Join (6-arity)

    [Test]
    public async Task Join_six_valid_returns_tuple()
    {
        var result = await ValidAsync<int, Error>(1)
            .Join(
                ValidAsync<string, Error>("two"),
                ValidAsync<bool, Error>(true),
                ValidAsync<double, Error>(4.0),
                ValidAsync<char, Error>('e'),
                ValidAsync<long, Error>(6L));

        await Assert.That(result.IsValid).IsTrue();
        var (v1, v2, v3, v4, v5, v6) = result.GetValueOrThrow();
        await Assert.That(v1).IsEqualTo(1);
        await Assert.That(v6).IsEqualTo(6L);
    }

    [Test]
    public async Task Join_six_all_invalid_accumulates_errors()
    {
        var result = await InvalidAsync<int, Error>(new ValidationError { Message = "e1" })
            .Join(
                InvalidAsync<string, Error>(new ValidationError { Message = "e2" }),
                InvalidAsync<bool, Error>(new ValidationError { Message = "e3" }),
                InvalidAsync<double, Error>(new ValidationError { Message = "e4" }),
                InvalidAsync<char, Error>(new ValidationError { Message = "e5" }),
                InvalidAsync<long, Error>(new ValidationError { Message = "e6" }));

        await Assert.That(result.IsInvalid).IsTrue();
        var errors = result.Match(_ => Array.Empty<Error>(), errs => errs.ToArray());
        await Assert.That(errors.Length).IsEqualTo(6);
    }

    #endregion

    #region Join (7-arity)

    [Test]
    public async Task Join_seven_valid_returns_tuple()
    {
        var result = await ValidAsync<int, Error>(1)
            .Join(
                ValidAsync<string, Error>("two"),
                ValidAsync<bool, Error>(true),
                ValidAsync<double, Error>(4.0),
                ValidAsync<char, Error>('e'),
                ValidAsync<long, Error>(6L),
                ValidAsync<float, Error>(7.0f));

        await Assert.That(result.IsValid).IsTrue();
        var (v1, v2, v3, v4, v5, v6, v7) = result.GetValueOrThrow();
        await Assert.That(v1).IsEqualTo(1);
        await Assert.That(v7).IsEqualTo(7.0f);
    }

    [Test]
    public async Task Join_seven_all_invalid_accumulates_errors()
    {
        var result = await InvalidAsync<int, Error>(new ValidationError { Message = "e1" })
            .Join(
                InvalidAsync<string, Error>(new ValidationError { Message = "e2" }),
                InvalidAsync<bool, Error>(new ValidationError { Message = "e3" }),
                InvalidAsync<double, Error>(new ValidationError { Message = "e4" }),
                InvalidAsync<char, Error>(new ValidationError { Message = "e5" }),
                InvalidAsync<long, Error>(new ValidationError { Message = "e6" }),
                InvalidAsync<float, Error>(new ValidationError { Message = "e7" }));

        await Assert.That(result.IsInvalid).IsTrue();
        var errors = result.Match(_ => Array.Empty<Error>(), errs => errs.ToArray());
        await Assert.That(errors.Length).IsEqualTo(7);
    }

    #endregion

    #region Join (8-arity)

    [Test]
    public async Task Join_eight_valid_returns_tuple()
    {
        var result = await ValidAsync<int, Error>(1)
            .Join(
                ValidAsync<string, Error>("two"),
                ValidAsync<bool, Error>(true),
                ValidAsync<double, Error>(4.0),
                ValidAsync<char, Error>('e'),
                ValidAsync<long, Error>(6L),
                ValidAsync<float, Error>(7.0f),
                ValidAsync<byte, Error>((byte)8));

        await Assert.That(result.IsValid).IsTrue();
        var (v1, v2, v3, v4, v5, v6, v7, v8) = result.GetValueOrThrow();
        await Assert.That(v1).IsEqualTo(1);
        await Assert.That(v8).IsEqualTo((byte)8);
    }

    [Test]
    public async Task Join_eight_all_invalid_accumulates_errors()
    {
        var result = await InvalidAsync<int, Error>(new ValidationError { Message = "e1" })
            .Join(
                InvalidAsync<string, Error>(new ValidationError { Message = "e2" }),
                InvalidAsync<bool, Error>(new ValidationError { Message = "e3" }),
                InvalidAsync<double, Error>(new ValidationError { Message = "e4" }),
                InvalidAsync<char, Error>(new ValidationError { Message = "e5" }),
                InvalidAsync<long, Error>(new ValidationError { Message = "e6" }),
                InvalidAsync<float, Error>(new ValidationError { Message = "e7" }),
                InvalidAsync<byte, Error>(new ValidationError { Message = "e8" }));

        await Assert.That(result.IsInvalid).IsTrue();
        var errors = result.Match(_ => Array.Empty<Error>(), errs => errs.ToArray());
        await Assert.That(errors.Length).IsEqualTo(8);
    }

    #endregion

    #region Join concurrency

    [Test]
    public async Task Join_two_runs_tasks_concurrently()
    {
        var task1Started = new TaskCompletionSource();
        var task2Started = new TaskCompletionSource();

        var first = Task.Run(async () =>
        {
            task1Started.SetResult();
            await task2Started.Task;
            return Validation.Valid<int, Error>(1);
        });

        var second = Task.Run(async () =>
        {
            task2Started.SetResult();
            await task1Started.Task;
            return Validation.Valid<string, Error>("two");
        });

        var result = await first.Join(second);

        await Assert.That(result.IsValid).IsTrue();
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
            return Validation.Valid<int, Error>(1);
        });

        var second = Task.Run(async () =>
        {
            task2Started.SetResult();
            await task1Started.Task;
            await task3Started.Task;
            return Validation.Valid<string, Error>("two");
        });

        var third = Task.Run(async () =>
        {
            task3Started.SetResult();
            await task1Started.Task;
            await task2Started.Task;
            return Validation.Valid<bool, Error>(true);
        });

        var result = await first.Join(second, third);

        await Assert.That(result.IsValid).IsTrue();
    }

    #endregion

    #region SequenceAsync

    [Test]
    public async Task SequenceAsync_all_valid_returns_values()
    {
        var tasks = new[]
        {
            Task.FromResult(Validation.Valid<int, Error>(1)),
            Task.FromResult(Validation.Valid<int, Error>(2)),
            Task.FromResult(Validation.Valid<int, Error>(3))
        };

        var result = await tasks.SequenceAsync();

        await Assert.That(result.IsValid).IsTrue();
        var values = result.GetValueOrThrow().ToList();
        await Assert.That(values.Count).IsEqualTo(3);
        await Assert.That(values[0]).IsEqualTo(1);
        await Assert.That(values[1]).IsEqualTo(2);
        await Assert.That(values[2]).IsEqualTo(3);
    }

    [Test]
    public async Task SequenceAsync_accumulates_all_errors()
    {
        var tasks = new[]
        {
            Task.FromResult(Validation.Valid<int, Error>(1)),
            Task.FromResult(Validation.Invalid<int, Error>(new ValidationError { Message = "e1" })),
            Task.FromResult(Validation.Valid<int, Error>(3)),
            Task.FromResult(Validation.Invalid<int, Error>(new ValidationError { Message = "e2" }))
        };

        var result = await tasks.SequenceAsync();

        await Assert.That(result.IsInvalid).IsTrue();
        var errors = result.Match(_ => Array.Empty<Error>(), errs => errs.ToArray());
        await Assert.That(errors.Length).IsEqualTo(2);
        await Assert.That(errors[0].Message).IsEqualTo("e1");
        await Assert.That(errors[1].Message).IsEqualTo("e2");
    }

    [Test]
    public async Task SequenceAsync_empty_returns_valid_empty()
    {
        var tasks = Array.Empty<Task<Validation<int, Error>>>();

        var result = await tasks.SequenceAsync();

        await Assert.That(result.IsValid).IsTrue();
        var values = result.GetValueOrThrow().ToList();
        await Assert.That(values.Count).IsEqualTo(0);
    }

    #endregion

    #region TraverseAsync

    [Test]
    public async Task TraverseAsync_all_valid_returns_mapped_values()
    {
        var source = new[] { 1, 2, 3 };

        var result = await source.TraverseAsync(async x =>
        {
            await Task.Yield();
            return Validation.Valid<string, Error>($"v{x}");
        });

        await Assert.That(result.IsValid).IsTrue();
        var values = result.GetValueOrThrow().ToList();
        await Assert.That(values.Count).IsEqualTo(3);
        await Assert.That(values[0]).IsEqualTo("v1");
    }

    [Test]
    public async Task TraverseAsync_accumulates_all_errors()
    {
        var source = new[] { 1, 2, 3 };

        var result = await source.TraverseAsync(async x =>
        {
            await Task.Yield();
            return x == 2
                ? Validation.Invalid<string, Error>(new ValidationError { Message = "bad2" })
                : x == 3
                    ? Validation.Invalid<string, Error>(new ValidationError { Message = "bad3" })
                    : Validation.Valid<string, Error>($"v{x}");
        });

        await Assert.That(result.IsInvalid).IsTrue();
        var errors = result.Match(_ => Array.Empty<Error>(), errs => errs.ToArray());
        await Assert.That(errors.Length).IsEqualTo(2);
    }

    #endregion

    #region SequenceParallel

    [Test]
    public async Task SequenceParallel_all_valid_returns_values()
    {
        var tasks = new[]
        {
            Task.FromResult(Validation.Valid<int, Error>(1)),
            Task.FromResult(Validation.Valid<int, Error>(2)),
            Task.FromResult(Validation.Valid<int, Error>(3))
        };

        var result = await tasks.SequenceParallel();

        await Assert.That(result.IsValid).IsTrue();
        var values = result.GetValueOrThrow().ToList();
        await Assert.That(values.Count).IsEqualTo(3);
    }

    [Test]
    public async Task SequenceParallel_accumulates_all_errors()
    {
        var tasks = new[]
        {
            Task.FromResult(Validation.Valid<int, Error>(1)),
            Task.FromResult(Validation.Invalid<int, Error>(new ValidationError { Message = "e1" })),
            Task.FromResult(Validation.Invalid<int, Error>(new ValidationError { Message = "e2" }))
        };

        var result = await tasks.SequenceParallel();

        await Assert.That(result.IsInvalid).IsTrue();
        var errors = result.Match(_ => Array.Empty<Error>(), errs => errs.ToArray());
        await Assert.That(errors.Length).IsEqualTo(2);
    }

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
                return Validation.Valid<int, Error>(1);
            }),
            Task.Run(async () =>
            {
                task2Started.SetResult();
                await task1Started.Task;
                return Validation.Valid<int, Error>(2);
            })
        };

        var result = await tasks.SequenceParallel();

        await Assert.That(result.IsValid).IsTrue();
    }

    #endregion

    #region TraverseParallel

    [Test]
    public async Task TraverseParallel_all_valid_returns_mapped_values()
    {
        var source = new[] { 1, 2, 3 };

        var result = await source.TraverseParallel(async x =>
        {
            await Task.Yield();
            return Validation.Valid<string, Error>($"v{x}");
        });

        await Assert.That(result.IsValid).IsTrue();
        var values = result.GetValueOrThrow().ToList();
        await Assert.That(values.Count).IsEqualTo(3);
    }

    [Test]
    public async Task TraverseParallel_accumulates_all_errors()
    {
        var source = new[] { 1, 2, 3 };

        var result = await source.TraverseParallel(async x =>
        {
            await Task.Yield();
            return x == 2
                ? Validation.Invalid<string, Error>(new ValidationError { Message = "bad" })
                : Validation.Valid<string, Error>($"v{x}");
        });

        await Assert.That(result.IsInvalid).IsTrue();
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
            return Validation.Valid<int, Error>(x);
        });

        await Assert.That(result.IsValid).IsTrue();
    }

    #endregion

    #region Chaining

    [Test]
    public async Task Fluent_chain_across_multiple_async_operations()
    {
        var output = await ValidAsync<int, Error>(5)
            .Map(x => x * 2)
            .Bind(x => x > 5
                ? Validation.Valid<string, Error>($"Value: {x}")
                : Validation.Invalid<string, Error>(new ValidationError { Message = "Too small" }))
            .GetValueOrDefault("none");

        await Assert.That(output).IsEqualTo("Value: 10");
    }

    [Test]
    public async Task Fluent_chain_short_circuits_on_invalid()
    {
        var output = await ValidAsync<int, Error>(2)
            .Map(x => x * 2)
            .Bind(x => x > 5
                ? Validation.Valid<string, Error>($"Value: {x}")
                : Validation.Invalid<string, Error>(new ValidationError { Message = "Too small" }))
            .GetValueOrDefault("none");

        await Assert.That(output).IsEqualTo("none");
    }

    #endregion
}

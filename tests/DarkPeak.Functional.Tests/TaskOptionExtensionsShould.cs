using DarkPeak.Functional;
using DarkPeak.Functional.Extensions;

namespace DarkPeak.Functional.Tests;

public class TaskOptionExtensionsShould
{
    private static Task<Option<T>> SomeAsync<T>(T value) =>
        Task.FromResult<Option<T>>(Option.Some(value));

    private static Task<Option<T>> NoneAsync<T>() =>
        Task.FromResult<Option<T>>(Option.None<T>());

    #region Map

    [Test]
    public async Task Map_transforms_some_value()
    {
        var result = await SomeAsync(5).Map(x => x * 2);

        await Assert.That(result.IsSome).IsTrue();
        await Assert.That(result.GetValueOrThrow()).IsEqualTo(10);
    }

    [Test]
    public async Task Map_preserves_none()
    {
        var result = await NoneAsync<int>().Map(x => x * 2);

        await Assert.That(result.IsNone).IsTrue();
    }

    [Test]
    public async Task MapAsync_transforms_some_value()
    {
        var result = await SomeAsync(5).MapAsync(async x =>
        {
            await Task.Delay(1);
            return x * 2;
        });

        await Assert.That(result.IsSome).IsTrue();
        await Assert.That(result.GetValueOrThrow()).IsEqualTo(10);
    }

    #endregion

    #region Bind

    [Test]
    public async Task Bind_chains_some_operations()
    {
        var result = await SomeAsync(10).Bind(x =>
            x > 5 ? Option.Some(x * 2) : Option.None<int>());

        await Assert.That(result.IsSome).IsTrue();
        await Assert.That(result.GetValueOrThrow()).IsEqualTo(20);
    }

    [Test]
    public async Task Bind_preserves_none()
    {
        var result = await NoneAsync<int>().Bind(x => Option.Some(x * 2));

        await Assert.That(result.IsNone).IsTrue();
    }

    [Test]
    public async Task BindAsync_chains_async_operations()
    {
        var result = await SomeAsync(10).BindAsync(async x =>
        {
            await Task.Delay(1);
            return x > 5 ? Option.Some(x * 2) : Option.None<int>();
        });

        await Assert.That(result.IsSome).IsTrue();
        await Assert.That(result.GetValueOrThrow()).IsEqualTo(20);
    }

    #endregion

    #region Match

    [Test]
    public async Task Match_some_calls_some_function()
    {
        var result = await SomeAsync("hello").Match(
            some: v => v.ToUpper(),
            none: () => "default");

        await Assert.That(result).IsEqualTo("HELLO");
    }

    [Test]
    public async Task Match_none_calls_none_function()
    {
        var result = await NoneAsync<string>().Match(
            some: v => v.ToUpper(),
            none: () => "default");

        await Assert.That(result).IsEqualTo("default");
    }

    [Test]
    public async Task MatchAsync_some_calls_async_function()
    {
        var result = await SomeAsync("hello").MatchAsync(
            some: async v => { await Task.Delay(1); return v.ToUpper(); },
            none: async () => { await Task.Delay(1); return "default"; });

        await Assert.That(result).IsEqualTo("HELLO");
    }

    #endregion

    #region Filter

    [Test]
    public async Task Filter_keeps_value_when_predicate_true()
    {
        var result = await SomeAsync(10).Filter(x => x > 5);

        await Assert.That(result.IsSome).IsTrue();
    }

    [Test]
    public async Task Filter_returns_none_when_predicate_false()
    {
        var result = await SomeAsync(3).Filter(x => x > 5);

        await Assert.That(result.IsNone).IsTrue();
    }

    #endregion

    #region GetValueOrDefault

    [Test]
    public async Task GetValueOrDefault_returns_value_for_some()
    {
        var result = await SomeAsync(42).GetValueOrDefault(0);

        await Assert.That(result).IsEqualTo(42);
    }

    [Test]
    public async Task GetValueOrDefault_returns_default_for_none()
    {
        var result = await NoneAsync<int>().GetValueOrDefault(99);

        await Assert.That(result).IsEqualTo(99);
    }

    [Test]
    public async Task GetValueOrDefault_with_factory_returns_value_for_some()
    {
        var result = await SomeAsync(42).GetValueOrDefault(() => 0);

        await Assert.That(result).IsEqualTo(42);
    }

    [Test]
    public async Task GetValueOrDefault_with_factory_returns_default_for_none()
    {
        var result = await NoneAsync<int>().GetValueOrDefault(() => 99);

        await Assert.That(result).IsEqualTo(99);
    }

    #endregion

    #region GetValueOrThrow

    [Test]
    public async Task GetValueOrThrow_returns_value_for_some()
    {
        var result = await SomeAsync(42).GetValueOrThrow();

        await Assert.That(result).IsEqualTo(42);
    }

    [Test]
    public async Task GetValueOrThrow_throws_for_none()
    {
        await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await NoneAsync<int>().GetValueOrThrow());
    }

    #endregion

    #region Tap

    [Test]
    public async Task Tap_executes_action_on_some()
    {
        var executed = false;

        var result = await SomeAsync(42).Tap(x => executed = true);

        await Assert.That(executed).IsTrue();
        await Assert.That(result.IsSome).IsTrue();
    }

    [Test]
    public async Task Tap_does_not_execute_on_none()
    {
        var executed = false;

        await NoneAsync<int>().Tap(x => executed = true);

        await Assert.That(executed).IsFalse();
    }

    [Test]
    public async Task TapNone_executes_action_on_none()
    {
        var executed = false;

        await NoneAsync<int>().TapNone(() => executed = true);

        await Assert.That(executed).IsTrue();
    }

    [Test]
    public async Task TapNone_does_not_execute_on_some()
    {
        var executed = false;

        await SomeAsync(42).TapNone(() => executed = true);

        await Assert.That(executed).IsFalse();
    }

    #endregion

    #region OrElse

    [Test]
    public async Task OrElse_returns_original_for_some()
    {
        var result = await SomeAsync(42).OrElse(Option.Some(99));

        await Assert.That(result.GetValueOrThrow()).IsEqualTo(42);
    }

    [Test]
    public async Task OrElse_returns_alternative_for_none()
    {
        var result = await NoneAsync<int>().OrElse(Option.Some(99));

        await Assert.That(result.GetValueOrThrow()).IsEqualTo(99);
    }

    [Test]
    public async Task OrElse_with_factory_returns_original_for_some()
    {
        var result = await SomeAsync(42).OrElse(() => Option.Some(99));

        await Assert.That(result.GetValueOrThrow()).IsEqualTo(42);
    }

    [Test]
    public async Task OrElse_with_factory_returns_alternative_for_none()
    {
        var result = await NoneAsync<int>().OrElse(() => Option.Some(99));

        await Assert.That(result.GetValueOrThrow()).IsEqualTo(99);
    }

    #endregion

    #region Chaining

    [Test]
    public async Task Fluent_chain_across_multiple_async_operations()
    {
        var result = await SomeAsync(5)
            .Map(x => x * 2)
            .Filter(x => x > 5)
            .Map(x => $"Value: {x}")
            .GetValueOrDefault("none");

        await Assert.That(result).IsEqualTo("Value: 10");
    }

    [Test]
    public async Task Fluent_chain_short_circuits_on_none()
    {
        var result = await SomeAsync(2)
            .Map(x => x * 2)
            .Filter(x => x > 5)
            .Map(x => $"Value: {x}")
            .GetValueOrDefault("none");

        await Assert.That(result).IsEqualTo("none");
    }

    #endregion

    #region Join

    [Test]
    public async Task Join_two_some_returns_tuple()
    {
        var result = await SomeAsync(1).Join(SomeAsync("two"));

        await Assert.That(result.IsSome).IsTrue();
        var (v1, v2) = result.GetValueOrThrow();
        await Assert.That(v1).IsEqualTo(1);
        await Assert.That(v2).IsEqualTo("two");
    }

    [Test]
    public async Task Join_first_none_returns_none()
    {
        var result = await NoneAsync<int>().Join(SomeAsync("two"));

        await Assert.That(result.IsNone).IsTrue();
    }

    [Test]
    public async Task Join_second_none_returns_none()
    {
        var result = await SomeAsync(1).Join(NoneAsync<string>());

        await Assert.That(result.IsNone).IsTrue();
    }

    [Test]
    public async Task Join_runs_tasks_concurrently()
    {
        var task1Started = new TaskCompletionSource();
        var task2Started = new TaskCompletionSource();

        var first = Task.Run(async () =>
        {
            task1Started.SetResult();
            await task2Started.Task;
            return Option.Some(1);
        });

        var second = Task.Run(async () =>
        {
            task2Started.SetResult();
            await task1Started.Task;
            return Option.Some("two");
        });

        var result = await first.Join(second);

        await Assert.That(result.IsSome).IsTrue();
    }

    [Test]
    public async Task Join_three_some_returns_tuple()
    {
        var result = await SomeAsync(1).Join(SomeAsync("two"), SomeAsync(true));

        await Assert.That(result.IsSome).IsTrue();
        var (v1, v2, v3) = result.GetValueOrThrow();
        await Assert.That(v1).IsEqualTo(1);
        await Assert.That(v2).IsEqualTo("two");
        await Assert.That(v3).IsTrue();
    }

    [Test]
    public async Task Join_three_any_none_returns_none()
    {
        var result = await SomeAsync(1).Join(NoneAsync<string>(), SomeAsync(true));

        await Assert.That(result.IsNone).IsTrue();
    }

    [Test]
    public async Task Join_four_some_returns_tuple()
    {
        var result = await SomeAsync(1)
            .Join(SomeAsync("two"), SomeAsync(true), SomeAsync(4.0));

        await Assert.That(result.IsSome).IsTrue();
        var (v1, v2, v3, v4) = result.GetValueOrThrow();
        await Assert.That(v1).IsEqualTo(1);
        await Assert.That(v4).IsEqualTo(4.0);
    }

    [Test]
    public async Task Join_four_any_none_returns_none()
    {
        var result = await SomeAsync(1)
            .Join(SomeAsync("two"), NoneAsync<bool>(), SomeAsync(4.0));

        await Assert.That(result.IsNone).IsTrue();
    }

    [Test]
    public async Task Join_five_some_returns_tuple()
    {
        var result = await SomeAsync(1)
            .Join(SomeAsync("two"), SomeAsync(true), SomeAsync(4.0), SomeAsync('e'));

        await Assert.That(result.IsSome).IsTrue();
        var (v1, v2, v3, v4, v5) = result.GetValueOrThrow();
        await Assert.That(v1).IsEqualTo(1);
        await Assert.That(v5).IsEqualTo('e');
    }

    [Test]
    public async Task Join_five_any_none_returns_none()
    {
        var result = await NoneAsync<int>()
            .Join(SomeAsync("two"), SomeAsync(true), SomeAsync(4.0), SomeAsync('e'));

        await Assert.That(result.IsNone).IsTrue();
    }

    [Test]
    public async Task Join_six_some_returns_tuple()
    {
        var result = await SomeAsync(1)
            .Join(SomeAsync("two"), SomeAsync(true), SomeAsync(4.0), SomeAsync('e'), SomeAsync(6L));

        await Assert.That(result.IsSome).IsTrue();
        var (v1, v2, v3, v4, v5, v6) = result.GetValueOrThrow();
        await Assert.That(v1).IsEqualTo(1);
        await Assert.That(v6).IsEqualTo(6L);
    }

    [Test]
    public async Task Join_six_any_none_returns_none()
    {
        var result = await SomeAsync(1)
            .Join(SomeAsync("two"), SomeAsync(true), SomeAsync(4.0), SomeAsync('e'), NoneAsync<long>());

        await Assert.That(result.IsNone).IsTrue();
    }

    [Test]
    public async Task Join_seven_some_returns_tuple()
    {
        var result = await SomeAsync(1)
            .Join(SomeAsync("two"), SomeAsync(true), SomeAsync(4.0), SomeAsync('e'), SomeAsync(6L), SomeAsync(7.0f));

        await Assert.That(result.IsSome).IsTrue();
        var (v1, v2, v3, v4, v5, v6, v7) = result.GetValueOrThrow();
        await Assert.That(v1).IsEqualTo(1);
        await Assert.That(v7).IsEqualTo(7.0f);
    }

    [Test]
    public async Task Join_seven_any_none_returns_none()
    {
        var result = await SomeAsync(1)
            .Join(SomeAsync("two"), NoneAsync<bool>(), SomeAsync(4.0), SomeAsync('e'), SomeAsync(6L), SomeAsync(7.0f));

        await Assert.That(result.IsNone).IsTrue();
    }

    [Test]
    public async Task Join_eight_some_returns_tuple()
    {
        var result = await SomeAsync(1)
            .Join(SomeAsync("two"), SomeAsync(true), SomeAsync(4.0), SomeAsync('e'), SomeAsync(6L), SomeAsync(7.0f), SomeAsync((byte)8));

        await Assert.That(result.IsSome).IsTrue();
        var (v1, v2, v3, v4, v5, v6, v7, v8) = result.GetValueOrThrow();
        await Assert.That(v1).IsEqualTo(1);
        await Assert.That(v8).IsEqualTo((byte)8);
    }

    [Test]
    public async Task Join_eight_any_none_returns_none()
    {
        var result = await SomeAsync(1)
            .Join(SomeAsync("two"), SomeAsync(true), NoneAsync<double>(), SomeAsync('e'), SomeAsync(6L), SomeAsync(7.0f), SomeAsync((byte)8));

        await Assert.That(result.IsNone).IsTrue();
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
            return Option.Some(1);
        });

        var second = Task.Run(async () =>
        {
            task2Started.SetResult();
            await task1Started.Task;
            await task3Started.Task;
            return Option.Some("two");
        });

        var third = Task.Run(async () =>
        {
            task3Started.SetResult();
            await task1Started.Task;
            await task2Started.Task;
            return Option.Some(true);
        });

        var result = await first.Join(second, third);

        await Assert.That(result.IsSome).IsTrue();
    }

    #endregion
}

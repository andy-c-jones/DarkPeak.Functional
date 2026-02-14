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
}

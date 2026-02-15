using DarkPeak.Functional;
using DarkPeak.Functional.Extensions;

namespace DarkPeak.Functional.Tests;

/// <summary>
/// Tests for AsyncEnumerableOptionExtensions methods.
/// </summary>
public class AsyncEnumerableOptionExtensionsShould
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

    // ── Choose ──

    [Test]
    public async Task Choose_filters_out_none_and_unwraps_some()
    {
        // Arrange
        var source = ToAsyncEnumerable(
            Option.Some(1),
            Option.None<int>(),
            Option.Some(2),
            Option.None<int>(),
            Option.Some(3));

        // Act
        var result = await source.Choose().ToListAsync();

        // Assert
        await Assert.That(result.Count).IsEqualTo(3);
        await Assert.That(result[0]).IsEqualTo(1);
        await Assert.That(result[1]).IsEqualTo(2);
        await Assert.That(result[2]).IsEqualTo(3);
    }

    [Test]
    public async Task Choose_returns_empty_when_all_none()
    {
        // Arrange
        var source = ToAsyncEnumerable(
            Option.None<int>(),
            Option.None<int>(),
            Option.None<int>());

        // Act
        var result = await source.Choose().ToListAsync();

        // Assert
        await Assert.That(result.Count).IsEqualTo(0);
    }

    [Test]
    public async Task Choose_returns_all_values_when_all_some()
    {
        // Arrange
        var source = ToAsyncEnumerable(
            Option.Some(10),
            Option.Some(20),
            Option.Some(30));

        // Act
        var result = await source.Choose().ToListAsync();

        // Assert
        await Assert.That(result.Count).IsEqualTo(3);
        await Assert.That(result[0]).IsEqualTo(10);
        await Assert.That(result[1]).IsEqualTo(20);
        await Assert.That(result[2]).IsEqualTo(30);
    }

    [Test]
    public async Task Choose_returns_empty_for_empty_source()
    {
        // Arrange
        var source = EmptyAsyncEnumerable<Option<int>>();

        // Act
        var result = await source.Choose().ToListAsync();

        // Assert
        await Assert.That(result.Count).IsEqualTo(0);
    }

    // ── ChooseMap ──

    [Test]
    public async Task Choose_map_keeps_some_and_filters_none()
    {
        // Arrange
        var source = ToAsyncEnumerable(1, 2, 3, 4, 5);
        Option<int> Selector(int x) => x % 2 == 0 ? Option.Some(x * 10) : Option.None<int>();

        // Act
        var result = await source.ChooseMap(Selector).ToListAsync();

        // Assert
        await Assert.That(result.Count).IsEqualTo(2);
        await Assert.That(result[0]).IsEqualTo(20);
        await Assert.That(result[1]).IsEqualTo(40);
    }

    // ── ChooseMapAsync ──

    [Test]
    public async Task Choose_map_async_keeps_some_and_filters_none()
    {
        // Arrange
        var source = ToAsyncEnumerable(1, 2, 3, 4, 5);
        Task<Option<int>> Selector(int x) =>
            Task.FromResult(x % 2 == 0 ? Option.Some(x * 10) : Option.None<int>());

        // Act
        var result = await source.ChooseMapAsync(Selector).ToListAsync();

        // Assert
        await Assert.That(result.Count).IsEqualTo(2);
        await Assert.That(result[0]).IsEqualTo(20);
        await Assert.That(result[1]).IsEqualTo(40);
    }

    // ── MapOption ──

    [Test]
    public async Task Map_option_maps_some_values_and_preserves_none()
    {
        // Arrange
        var source = ToAsyncEnumerable(
            Option.Some(2),
            Option.None<int>(),
            Option.Some(5));

        // Act
        var result = await source.MapOption(x => x * 3).ToListAsync();

        // Assert
        await Assert.That(result.Count).IsEqualTo(3);

        await Assert.That(result[0].IsSome).IsTrue();
        await Assert.That(result[0] is Some<int> s0 ? s0.Value : -1).IsEqualTo(6);

        await Assert.That(result[1].IsNone).IsTrue();

        await Assert.That(result[2].IsSome).IsTrue();
        await Assert.That(result[2] is Some<int> s2 ? s2.Value : -1).IsEqualTo(15);
    }

    [Test]
    public async Task Map_option_returns_empty_for_empty_source()
    {
        // Arrange
        var source = EmptyAsyncEnumerable<Option<int>>();

        // Act
        var result = await source.MapOption(x => x * 2).ToListAsync();

        // Assert
        await Assert.That(result.Count).IsEqualTo(0);
    }

    // ── MapOptionAsync ──

    [Test]
    public async Task Map_option_async_maps_some_values_and_preserves_none()
    {
        // Arrange
        var source = ToAsyncEnumerable(
            Option.Some(3),
            Option.None<int>(),
            Option.Some(7));

        // Act
        var result = await source.MapOptionAsync(x => Task.FromResult(x + 1)).ToListAsync();

        // Assert
        await Assert.That(result.Count).IsEqualTo(3);

        await Assert.That(result[0].IsSome).IsTrue();
        await Assert.That(result[0] is Some<int> s0 ? s0.Value : -1).IsEqualTo(4);

        await Assert.That(result[1].IsNone).IsTrue();

        await Assert.That(result[2].IsSome).IsTrue();
        await Assert.That(result[2] is Some<int> s2 ? s2.Value : -1).IsEqualTo(8);
    }

    // ── BindOption ──

    [Test]
    public async Task Bind_option_binds_some_to_some_and_some_to_none_and_passes_none_through()
    {
        // Arrange
        var source = ToAsyncEnumerable(
            Option.Some(4),
            Option.Some(0),
            Option.None<int>());

        Option<string> Binder(int x) =>
            x > 0 ? Option.Some($"val:{x}") : Option.None<string>();

        // Act
        var result = await source.BindOption(Binder).ToListAsync();

        // Assert
        await Assert.That(result.Count).IsEqualTo(3);

        await Assert.That(result[0].IsSome).IsTrue();
        await Assert.That(result[0] is Some<string> s0 ? s0.Value : "").IsEqualTo("val:4");

        await Assert.That(result[1].IsNone).IsTrue();

        await Assert.That(result[2].IsNone).IsTrue();
    }

    // ── BindOptionAsync ──

    [Test]
    public async Task Bind_option_async_binds_with_async_binder()
    {
        // Arrange
        var source = ToAsyncEnumerable(
            Option.Some(4),
            Option.Some(0),
            Option.None<int>());

        Task<Option<string>> Binder(int x) =>
            Task.FromResult(x > 0 ? Option.Some($"val:{x}") : Option.None<string>());

        // Act
        var result = await source.BindOptionAsync(Binder).ToListAsync();

        // Assert
        await Assert.That(result.Count).IsEqualTo(3);

        await Assert.That(result[0].IsSome).IsTrue();
        await Assert.That(result[0] is Some<string> s0 ? s0.Value : "").IsEqualTo("val:4");

        await Assert.That(result[1].IsNone).IsTrue();

        await Assert.That(result[2].IsNone).IsTrue();
    }

    // ── SequenceAsync ──

    [Test]
    public async Task Sequence_async_returns_some_with_all_values_when_all_some()
    {
        // Arrange
        var source = ToAsyncEnumerable(
            Option.Some(1),
            Option.Some(2),
            Option.Some(3));

        // Act
        var result = await source.SequenceAsync();

        // Assert
        await Assert.That(result.IsSome).IsTrue();
        var list = (result is Some<IReadOnlyList<int>> some) ? some.Value : [];
        await Assert.That(list.Count).IsEqualTo(3);
        await Assert.That(list[0]).IsEqualTo(1);
        await Assert.That(list[1]).IsEqualTo(2);
        await Assert.That(list[2]).IsEqualTo(3);
    }

    [Test]
    public async Task Sequence_async_returns_none_when_any_none()
    {
        // Arrange
        var source = ToAsyncEnumerable(
            Option.Some(1),
            Option.None<int>(),
            Option.Some(3));

        // Act
        var result = await source.SequenceAsync();

        // Assert
        await Assert.That(result.IsNone).IsTrue();
    }

    [Test]
    public async Task Sequence_async_returns_some_with_empty_list_for_empty_source()
    {
        // Arrange
        var source = EmptyAsyncEnumerable<Option<int>>();

        // Act
        var result = await source.SequenceAsync();

        // Assert
        await Assert.That(result.IsSome).IsTrue();
        var list = (result is Some<IReadOnlyList<int>> some) ? some.Value : null;
        await Assert.That(list).IsNotNull();
        await Assert.That(list!.Count).IsEqualTo(0);
    }

    // ── FirstOrNoneAsync (no predicate) ──

    [Test]
    public async Task First_or_none_async_returns_some_first_for_non_empty()
    {
        // Arrange
        var source = ToAsyncEnumerable(10, 20, 30);

        // Act
        var result = await source.FirstOrNoneAsync();

        // Assert
        await Assert.That(result.IsSome).IsTrue();
        await Assert.That(result is Some<int> some ? some.Value : -1).IsEqualTo(10);
    }

    [Test]
    public async Task First_or_none_async_returns_none_for_empty()
    {
        // Arrange
        var source = EmptyAsyncEnumerable<int>();

        // Act
        var result = await source.FirstOrNoneAsync();

        // Assert
        await Assert.That(result.IsNone).IsTrue();
    }

    // ── FirstOrNoneAsync (with predicate) ──

    [Test]
    public async Task First_or_none_async_with_predicate_returns_some_for_match()
    {
        // Arrange
        var source = ToAsyncEnumerable(1, 2, 3, 4, 5);

        // Act
        var result = await source.FirstOrNoneAsync(x => x > 3);

        // Assert
        await Assert.That(result.IsSome).IsTrue();
        await Assert.That(result is Some<int> some ? some.Value : -1).IsEqualTo(4);
    }

    [Test]
    public async Task First_or_none_async_with_predicate_returns_none_for_no_match()
    {
        // Arrange
        var source = ToAsyncEnumerable(1, 2, 3);

        // Act
        var result = await source.FirstOrNoneAsync(x => x > 10);

        // Assert
        await Assert.That(result.IsNone).IsTrue();
    }

    // ── SingleOrNoneAsync (no predicate) ──

    [Test]
    public async Task Single_or_none_async_returns_some_for_exactly_one_element()
    {
        // Arrange
        var source = ToAsyncEnumerable(42);

        // Act
        var result = await source.SingleOrNoneAsync();

        // Assert
        await Assert.That(result.IsSome).IsTrue();
        await Assert.That(result is Some<int> some ? some.Value : -1).IsEqualTo(42);
    }

    [Test]
    public async Task Single_or_none_async_returns_none_for_empty()
    {
        // Arrange
        var source = EmptyAsyncEnumerable<int>();

        // Act
        var result = await source.SingleOrNoneAsync();

        // Assert
        await Assert.That(result.IsNone).IsTrue();
    }

    [Test]
    public async Task Single_or_none_async_returns_none_for_multiple_elements()
    {
        // Arrange
        var source = ToAsyncEnumerable(1, 2);

        // Act
        var result = await source.SingleOrNoneAsync();

        // Assert
        await Assert.That(result.IsNone).IsTrue();
    }

    // ── SingleOrNoneAsync (with predicate) ──

    [Test]
    public async Task Single_or_none_async_with_predicate_returns_some_for_exactly_one_match()
    {
        // Arrange
        var source = ToAsyncEnumerable(1, 2, 3, 4, 5);

        // Act
        var result = await source.SingleOrNoneAsync(x => x == 3);

        // Assert
        await Assert.That(result.IsSome).IsTrue();
        await Assert.That(result is Some<int> some ? some.Value : -1).IsEqualTo(3);
    }

    [Test]
    public async Task Single_or_none_async_with_predicate_returns_none_for_no_match()
    {
        // Arrange
        var source = ToAsyncEnumerable(1, 2, 3);

        // Act
        var result = await source.SingleOrNoneAsync(x => x > 10);

        // Assert
        await Assert.That(result.IsNone).IsTrue();
    }

    [Test]
    public async Task Single_or_none_async_with_predicate_returns_none_for_multiple_matches()
    {
        // Arrange
        var source = ToAsyncEnumerable(1, 2, 3, 4, 5);

        // Act
        var result = await source.SingleOrNoneAsync(x => x > 2);

        // Assert
        await Assert.That(result.IsNone).IsTrue();
    }

    // ── LastOrNoneAsync (no predicate) ──

    [Test]
    public async Task Last_or_none_async_returns_some_last_for_non_empty()
    {
        // Arrange
        var source = ToAsyncEnumerable(10, 20, 30);

        // Act
        var result = await source.LastOrNoneAsync();

        // Assert
        await Assert.That(result.IsSome).IsTrue();
        await Assert.That(result is Some<int> some ? some.Value : -1).IsEqualTo(30);
    }

    [Test]
    public async Task Last_or_none_async_returns_none_for_empty()
    {
        // Arrange
        var source = EmptyAsyncEnumerable<int>();

        // Act
        var result = await source.LastOrNoneAsync();

        // Assert
        await Assert.That(result.IsNone).IsTrue();
    }

    // ── LastOrNoneAsync (with predicate) ──

    [Test]
    public async Task Last_or_none_async_with_predicate_returns_some_last_matching()
    {
        // Arrange
        var source = ToAsyncEnumerable(1, 2, 3, 4, 5);

        // Act
        var result = await source.LastOrNoneAsync(x => x % 2 == 0);

        // Assert
        await Assert.That(result.IsSome).IsTrue();
        await Assert.That(result is Some<int> some ? some.Value : -1).IsEqualTo(4);
    }

    [Test]
    public async Task Last_or_none_async_with_predicate_returns_none_for_no_match()
    {
        // Arrange
        var source = ToAsyncEnumerable(1, 2, 3);

        // Act
        var result = await source.LastOrNoneAsync(x => x > 10);

        // Assert
        await Assert.That(result.IsNone).IsTrue();
    }
}

using DarkPeak.Functional;
using DarkPeak.Functional.Extensions;

namespace DarkPeak.Functional.Tests;

/// <summary>
/// Tests for AsyncEnumerableExtensions functional operations on IAsyncEnumerable{T}.
/// </summary>
public class AsyncEnumerableExtensionsShould
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

    // ── Map ──

    [Test]
    public async Task Map_transforms_each_element()
    {
        // Arrange
        var source = ToAsyncEnumerable(1, 2, 3);

        // Act
        var result = await source.Map(x => x * 2).ToListAsync();

        // Assert
        await Assert.That(result).Count().IsEqualTo(3);
        await Assert.That(result[0]).IsEqualTo(2);
        await Assert.That(result[1]).IsEqualTo(4);
        await Assert.That(result[2]).IsEqualTo(6);
    }

    [Test]
    public async Task Map_returns_empty_for_empty_source()
    {
        // Arrange
        var source = EmptyAsyncEnumerable<int>();

        // Act
        var result = await source.Map(x => x * 2).ToListAsync();

        // Assert
        await Assert.That(result).Count().IsEqualTo(0);
    }

    // ── MapAsync ──

    [Test]
    public async Task Map_async_transforms_each_element_with_async_mapper()
    {
        // Arrange
        var source = ToAsyncEnumerable(1, 2, 3);

        // Act
        var result = await source.MapAsync(async x =>
        {
            await Task.Yield();
            return x * 2;
        }).ToListAsync();

        // Assert
        await Assert.That(result).Count().IsEqualTo(3);
        await Assert.That(result[0]).IsEqualTo(2);
        await Assert.That(result[1]).IsEqualTo(4);
        await Assert.That(result[2]).IsEqualTo(6);
    }

    [Test]
    public async Task Map_async_returns_empty_for_empty_source()
    {
        // Arrange
        var source = EmptyAsyncEnumerable<int>();

        // Act
        var result = await source.MapAsync(async x =>
        {
            await Task.Yield();
            return x * 2;
        }).ToListAsync();

        // Assert
        await Assert.That(result).Count().IsEqualTo(0);
    }

    // ── Filter ──

    [Test]
    public async Task Filter_keeps_only_matching_elements()
    {
        // Arrange
        var source = ToAsyncEnumerable(1, 2, 3, 4, 5, 6);

        // Act
        var result = await source.Filter(x => x % 2 == 0).ToListAsync();

        // Assert
        await Assert.That(result).Count().IsEqualTo(3);
        await Assert.That(result[0]).IsEqualTo(2);
        await Assert.That(result[1]).IsEqualTo(4);
        await Assert.That(result[2]).IsEqualTo(6);
    }

    [Test]
    public async Task Filter_returns_empty_when_no_elements_match()
    {
        // Arrange
        var source = ToAsyncEnumerable(1, 3, 5);

        // Act
        var result = await source.Filter(x => x % 2 == 0).ToListAsync();

        // Assert
        await Assert.That(result).Count().IsEqualTo(0);
    }

    [Test]
    public async Task Filter_returns_empty_for_empty_source()
    {
        // Arrange
        var source = EmptyAsyncEnumerable<int>();

        // Act
        var result = await source.Filter(x => x % 2 == 0).ToListAsync();

        // Assert
        await Assert.That(result).Count().IsEqualTo(0);
    }

    // ── FilterAsync ──

    [Test]
    public async Task Filter_async_keeps_only_matching_elements()
    {
        // Arrange
        var source = ToAsyncEnumerable(1, 2, 3, 4, 5, 6);

        // Act
        var result = await source.FilterAsync(async x =>
        {
            await Task.Yield();
            return x % 2 == 0;
        }).ToListAsync();

        // Assert
        await Assert.That(result).Count().IsEqualTo(3);
        await Assert.That(result[0]).IsEqualTo(2);
        await Assert.That(result[1]).IsEqualTo(4);
        await Assert.That(result[2]).IsEqualTo(6);
    }

    [Test]
    public async Task Filter_async_returns_empty_for_empty_source()
    {
        // Arrange
        var source = EmptyAsyncEnumerable<int>();

        // Act
        var result = await source.FilterAsync(async x =>
        {
            await Task.Yield();
            return x % 2 == 0;
        }).ToListAsync();

        // Assert
        await Assert.That(result).Count().IsEqualTo(0);
    }

    // ── Bind ──

    [Test]
    public async Task Bind_flattens_nested_sequences()
    {
        // Arrange
        var source = ToAsyncEnumerable(1, 2, 3);

        // Act
        var result = await source.Bind(x => ToAsyncEnumerable(x, x * 10)).ToListAsync();

        // Assert
        await Assert.That(result).Count().IsEqualTo(6);
        await Assert.That(result[0]).IsEqualTo(1);
        await Assert.That(result[1]).IsEqualTo(10);
        await Assert.That(result[2]).IsEqualTo(2);
        await Assert.That(result[3]).IsEqualTo(20);
        await Assert.That(result[4]).IsEqualTo(3);
        await Assert.That(result[5]).IsEqualTo(30);
    }

    [Test]
    public async Task Bind_returns_empty_for_empty_source()
    {
        // Arrange
        var source = EmptyAsyncEnumerable<int>();

        // Act
        var result = await source.Bind(x => ToAsyncEnumerable(x, x * 10)).ToListAsync();

        // Assert
        await Assert.That(result).Count().IsEqualTo(0);
    }

    // ── BindAsync ──

    [Test]
    public async Task Bind_async_flattens_with_async_binder()
    {
        // Arrange
        var source = ToAsyncEnumerable(1, 2, 3);

        // Act
        var result = await source.BindAsync(async x =>
        {
            await Task.Yield();
            return ToAsyncEnumerable(x, x * 10);
        }).ToListAsync();

        // Assert
        await Assert.That(result).Count().IsEqualTo(6);
        await Assert.That(result[0]).IsEqualTo(1);
        await Assert.That(result[1]).IsEqualTo(10);
        await Assert.That(result[2]).IsEqualTo(2);
        await Assert.That(result[3]).IsEqualTo(20);
        await Assert.That(result[4]).IsEqualTo(3);
        await Assert.That(result[5]).IsEqualTo(30);
    }

    [Test]
    public async Task Bind_async_returns_empty_for_empty_source()
    {
        // Arrange
        var source = EmptyAsyncEnumerable<int>();

        // Act
        var result = await source.BindAsync(async x =>
        {
            await Task.Yield();
            return ToAsyncEnumerable(x, x * 10);
        }).ToListAsync();

        // Assert
        await Assert.That(result).Count().IsEqualTo(0);
    }

    // ── Tap ──

    [Test]
    public async Task Tap_executes_side_effect_and_preserves_sequence()
    {
        // Arrange
        var source = ToAsyncEnumerable(1, 2, 3);
        var sideEffects = new List<int>();

        // Act
        var result = await source.Tap(x => sideEffects.Add(x)).ToListAsync();

        // Assert
        await Assert.That(result).Count().IsEqualTo(3);
        await Assert.That(result[0]).IsEqualTo(1);
        await Assert.That(result[1]).IsEqualTo(2);
        await Assert.That(result[2]).IsEqualTo(3);
        await Assert.That(sideEffects).Count().IsEqualTo(3);
        await Assert.That(sideEffects[0]).IsEqualTo(1);
        await Assert.That(sideEffects[1]).IsEqualTo(2);
        await Assert.That(sideEffects[2]).IsEqualTo(3);
    }

    [Test]
    public async Task Tap_triggers_no_side_effects_for_empty_source()
    {
        // Arrange
        var source = EmptyAsyncEnumerable<int>();
        var sideEffects = new List<int>();

        // Act
        var result = await source.Tap(x => sideEffects.Add(x)).ToListAsync();

        // Assert
        await Assert.That(result).Count().IsEqualTo(0);
        await Assert.That(sideEffects).Count().IsEqualTo(0);
    }

    // ── TapAsync ──

    [Test]
    public async Task Tap_async_executes_async_side_effect_and_preserves_sequence()
    {
        // Arrange
        var source = ToAsyncEnumerable(1, 2, 3);
        var sideEffects = new List<int>();

        // Act
        var result = await source.TapAsync(async x =>
        {
            await Task.Yield();
            sideEffects.Add(x);
        }).ToListAsync();

        // Assert
        await Assert.That(result).Count().IsEqualTo(3);
        await Assert.That(result[0]).IsEqualTo(1);
        await Assert.That(result[1]).IsEqualTo(2);
        await Assert.That(result[2]).IsEqualTo(3);
        await Assert.That(sideEffects).Count().IsEqualTo(3);
        await Assert.That(sideEffects[0]).IsEqualTo(1);
        await Assert.That(sideEffects[1]).IsEqualTo(2);
        await Assert.That(sideEffects[2]).IsEqualTo(3);
    }

    // ── Scan ──

    [Test]
    public async Task Scan_produces_running_accumulation()
    {
        // Arrange
        var source = ToAsyncEnumerable(1, 2, 3);

        // Act
        var result = await source.Scan(0, (acc, x) => acc + x).ToListAsync();

        // Assert
        await Assert.That(result).Count().IsEqualTo(3);
        await Assert.That(result[0]).IsEqualTo(1);
        await Assert.That(result[1]).IsEqualTo(3);
        await Assert.That(result[2]).IsEqualTo(6);
    }

    [Test]
    public async Task Scan_returns_empty_for_empty_source()
    {
        // Arrange
        var source = EmptyAsyncEnumerable<int>();

        // Act
        var result = await source.Scan(0, (acc, x) => acc + x).ToListAsync();

        // Assert
        await Assert.That(result).Count().IsEqualTo(0);
    }

    // ── ScanAsync ──

    [Test]
    public async Task Scan_async_produces_running_accumulation_with_async_accumulator()
    {
        // Arrange
        var source = ToAsyncEnumerable(1, 2, 3);

        // Act
        var result = await source.ScanAsync(0, async (acc, x) =>
        {
            await Task.Yield();
            return acc + x;
        }).ToListAsync();

        // Assert
        await Assert.That(result).Count().IsEqualTo(3);
        await Assert.That(result[0]).IsEqualTo(1);
        await Assert.That(result[1]).IsEqualTo(3);
        await Assert.That(result[2]).IsEqualTo(6);
    }

    // ── Unfold ──

    [Test]
    public async Task Unfold_generates_sequence_from_seed()
    {
        // Arrange & Act
        var result = await AsyncEnumerableExtensions.Unfold(
            1,
            state => state <= 5
                ? Option.Some((state, state + 1))
                : Option.None<(int, int)>()
        ).ToListAsync();

        // Assert
        await Assert.That(result).Count().IsEqualTo(5);
        await Assert.That(result[0]).IsEqualTo(1);
        await Assert.That(result[1]).IsEqualTo(2);
        await Assert.That(result[2]).IsEqualTo(3);
        await Assert.That(result[3]).IsEqualTo(4);
        await Assert.That(result[4]).IsEqualTo(5);
    }

    [Test]
    public async Task Unfold_returns_empty_when_generator_immediately_returns_none()
    {
        // Arrange & Act
        var result = await AsyncEnumerableExtensions.Unfold(
            0,
            (int _) => Option.None<(int, int)>()
        ).ToListAsync();

        // Assert
        await Assert.That(result).Count().IsEqualTo(0);
    }

    // ── UnfoldAsync ──

    [Test]
    public async Task Unfold_async_generates_sequence_from_seed()
    {
        // Arrange & Act
        var result = await AsyncEnumerableExtensions.UnfoldAsync(
            1,
            async state =>
            {
                await Task.Yield();
                return state <= 5
                    ? Option.Some((state, state + 1))
                    : Option.None<(int, int)>();
            }
        ).ToListAsync();

        // Assert
        await Assert.That(result).Count().IsEqualTo(5);
        await Assert.That(result[0]).IsEqualTo(1);
        await Assert.That(result[1]).IsEqualTo(2);
        await Assert.That(result[2]).IsEqualTo(3);
        await Assert.That(result[3]).IsEqualTo(4);
        await Assert.That(result[4]).IsEqualTo(5);
    }

    // ── Buffer ──

    [Test]
    public async Task Buffer_splits_evenly_into_batches()
    {
        // Arrange
        var source = ToAsyncEnumerable(1, 2, 3, 4);

        // Act
        var result = await source.Buffer(2).ToListAsync();

        // Assert
        await Assert.That(result).Count().IsEqualTo(2);
        await Assert.That(result[0]).Count().IsEqualTo(2);
        await Assert.That(result[0][0]).IsEqualTo(1);
        await Assert.That(result[0][1]).IsEqualTo(2);
        await Assert.That(result[1][0]).IsEqualTo(3);
        await Assert.That(result[1][1]).IsEqualTo(4);
    }

    [Test]
    public async Task Buffer_handles_uneven_split_with_smaller_last_batch()
    {
        // Arrange
        var source = ToAsyncEnumerable(1, 2, 3, 4, 5);

        // Act
        var result = await source.Buffer(2).ToListAsync();

        // Assert
        await Assert.That(result).Count().IsEqualTo(3);
        await Assert.That(result[0][0]).IsEqualTo(1);
        await Assert.That(result[0][1]).IsEqualTo(2);
        await Assert.That(result[1][0]).IsEqualTo(3);
        await Assert.That(result[1][1]).IsEqualTo(4);
        await Assert.That(result[2]).Count().IsEqualTo(1);
        await Assert.That(result[2][0]).IsEqualTo(5);
    }

    [Test]
    public async Task Buffer_returns_empty_for_empty_source()
    {
        // Arrange
        var source = EmptyAsyncEnumerable<int>();

        // Act
        var result = await source.Buffer(2).ToListAsync();

        // Assert
        await Assert.That(result).Count().IsEqualTo(0);
    }

    [Test]
    public async Task Buffer_throws_when_size_is_less_than_one()
    {
        // Arrange
        var source = ToAsyncEnumerable(1, 2, 3);

        // Act & Assert
        await Assert.That(async () => await source.Buffer(0).ToListAsync())
            .Throws<ArgumentOutOfRangeException>();
    }

    // ── ForEachAsync (sync action) ──

    [Test]
    public async Task For_each_async_calls_sync_action_for_each_element()
    {
        // Arrange
        var source = ToAsyncEnumerable(1, 2, 3);
        var collected = new List<int>();

        // Act
        await source.ForEachAsync(x => collected.Add(x));

        // Assert
        await Assert.That(collected).Count().IsEqualTo(3);
        await Assert.That(collected[0]).IsEqualTo(1);
        await Assert.That(collected[1]).IsEqualTo(2);
        await Assert.That(collected[2]).IsEqualTo(3);
    }

    // ── ForEachAsync (async action) ──

    [Test]
    public async Task For_each_async_calls_async_action_for_each_element()
    {
        // Arrange
        var source = ToAsyncEnumerable(1, 2, 3);
        var collected = new List<int>();

        // Act
        await source.ForEachAsync(async x =>
        {
            await Task.Yield();
            collected.Add(x);
        });

        // Assert
        await Assert.That(collected).Count().IsEqualTo(3);
        await Assert.That(collected[0]).IsEqualTo(1);
        await Assert.That(collected[1]).IsEqualTo(2);
        await Assert.That(collected[2]).IsEqualTo(3);
    }
}

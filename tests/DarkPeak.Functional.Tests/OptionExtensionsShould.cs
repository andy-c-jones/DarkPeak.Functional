using DarkPeak.Functional;
using DarkPeak.Functional.Extensions;

namespace DarkPeak.Functional.Tests;

/// <summary>
/// Tests for OptionExtensions.
/// </summary>
public class OptionExtensionsShould
{
    // ToOption - reference type

    [Test]
    public async Task Convert_non_null_reference_to_some()
    {
        string? value = "hello";
        var option = value.ToOption();

        await Assert.That(option.IsSome).IsTrue();
        await Assert.That(option.Match(v => v, () => "")).IsEqualTo("hello");
    }

    [Test]
    public async Task Convert_null_reference_to_none()
    {
        string? value = null;
        var option = value.ToOption();

        await Assert.That(option.IsNone).IsTrue();
    }

    // ToOption - value type

    [Test]
    public async Task Convert_nullable_value_with_value_to_some()
    {
        int? value = 42;
        var option = value.ToOption();

        await Assert.That(option.IsSome).IsTrue();
        await Assert.That(option.Match(v => v, () => 0)).IsEqualTo(42);
    }

    [Test]
    public async Task Convert_null_nullable_value_to_none()
    {
        int? value = null;
        var option = value.ToOption();

        await Assert.That(option.IsNone).IsTrue();
    }

    // FirstOrNone

    [Test]
    public async Task Return_some_for_first_of_non_empty_sequence()
    {
        var option = new[] { 1, 2, 3 }.FirstOrNone();

        await Assert.That(option.IsSome).IsTrue();
        await Assert.That(option.Match(v => v, () => 0)).IsEqualTo(1);
    }

    [Test]
    public async Task Return_none_for_first_of_empty_sequence()
    {
        var option = Array.Empty<int>().FirstOrNone();

        await Assert.That(option.IsNone).IsTrue();
    }

    [Test]
    public async Task Return_some_for_first_matching_predicate()
    {
        var option = new[] { 1, 2, 3, 4 }.FirstOrNone(x => x > 2);

        await Assert.That(option.IsSome).IsTrue();
        await Assert.That(option.Match(v => v, () => 0)).IsEqualTo(3);
    }

    [Test]
    public async Task Return_none_for_first_with_no_match()
    {
        var option = new[] { 1, 2, 3 }.FirstOrNone(x => x > 10);

        await Assert.That(option.IsNone).IsTrue();
    }

    // SingleOrNone

    [Test]
    public async Task Return_some_for_single_element_sequence()
    {
        var option = new[] { 42 }.SingleOrNone();

        await Assert.That(option.IsSome).IsTrue();
        await Assert.That(option.Match(v => v, () => 0)).IsEqualTo(42);
    }

    [Test]
    public async Task Return_none_for_empty_sequence_single()
    {
        var option = Array.Empty<int>().SingleOrNone();

        await Assert.That(option.IsNone).IsTrue();
    }

    [Test]
    public async Task Return_none_for_multiple_element_sequence()
    {
        var option = new[] { 1, 2 }.SingleOrNone();

        await Assert.That(option.IsNone).IsTrue();
    }

    [Test]
    public async Task Return_some_for_single_matching_predicate()
    {
        var option = new[] { 1, 2, 3 }.SingleOrNone(x => x == 2);

        await Assert.That(option.IsSome).IsTrue();
        await Assert.That(option.Match(v => v, () => 0)).IsEqualTo(2);
    }

    [Test]
    public async Task Return_none_for_multiple_matching_predicate()
    {
        var option = new[] { 1, 2, 2, 3 }.SingleOrNone(x => x == 2);

        await Assert.That(option.IsNone).IsTrue();
    }

    // LastOrNone

    [Test]
    public async Task Return_some_for_last_of_non_empty_sequence()
    {
        var option = new[] { 1, 2, 3 }.LastOrNone();

        await Assert.That(option.IsSome).IsTrue();
        await Assert.That(option.Match(v => v, () => 0)).IsEqualTo(3);
    }

    [Test]
    public async Task Return_none_for_last_of_empty_sequence()
    {
        var option = Array.Empty<int>().LastOrNone();

        await Assert.That(option.IsNone).IsTrue();
    }

    [Test]
    public async Task Return_some_for_last_matching_predicate()
    {
        var option = new[] { 1, 2, 3, 4 }.LastOrNone(x => x < 3);

        await Assert.That(option.IsSome).IsTrue();
        await Assert.That(option.Match(v => v, () => 0)).IsEqualTo(2);
    }

    [Test]
    public async Task Return_none_for_last_with_no_match()
    {
        var option = new[] { 1, 2, 3 }.LastOrNone(x => x > 10);

        await Assert.That(option.IsNone).IsTrue();
    }

    // TryGetValueAsOption - IDictionary

    [Test]
    public async Task Return_some_for_existing_dictionary_key()
    {
        IDictionary<string, int> dict = new Dictionary<string, int> { ["a"] = 1, ["b"] = 2 };
        var option = dict.TryGetValueAsOption("a");

        await Assert.That(option.IsSome).IsTrue();
        await Assert.That(option.Match(v => v, () => 0)).IsEqualTo(1);
    }

    [Test]
    public async Task Return_none_for_missing_dictionary_key()
    {
        IDictionary<string, int> dict = new Dictionary<string, int> { ["a"] = 1 };
        var option = dict.TryGetValueAsOption("z");

        await Assert.That(option.IsNone).IsTrue();
    }

    // TryGetValueAsOption - IReadOnlyDictionary

    [Test]
    public async Task Return_some_for_existing_readonly_dictionary_key()
    {
        IReadOnlyDictionary<string, int> dict = new Dictionary<string, int> { ["a"] = 1 };
        var option = dict.TryGetValueAsOption("a");

        await Assert.That(option.IsSome).IsTrue();
        await Assert.That(option.Match(v => v, () => 0)).IsEqualTo(1);
    }

    [Test]
    public async Task Return_none_for_missing_readonly_dictionary_key()
    {
        IReadOnlyDictionary<string, int> dict = new Dictionary<string, int> { ["a"] = 1 };
        var option = dict.TryGetValueAsOption("z");

        await Assert.That(option.IsNone).IsTrue();
    }

    // Flatten

    [Test]
    public async Task Flatten_nested_some_some()
    {
        var nested = Option.Some(Option.Some(42));
        var flat = nested.Flatten();

        await Assert.That(flat.IsSome).IsTrue();
        await Assert.That(flat.Match(v => v, () => 0)).IsEqualTo(42);
    }

    [Test]
    public async Task Flatten_nested_some_none()
    {
        var nested = Option.Some(Option.None<int>());
        var flat = nested.Flatten();

        await Assert.That(flat.IsNone).IsTrue();
    }

    [Test]
    public async Task Flatten_nested_none()
    {
        var nested = Option.None<Option<int>>();
        var flat = nested.Flatten();

        await Assert.That(flat.IsNone).IsTrue();
    }

    // Choose

    [Test]
    public async Task Choose_filters_none_and_unwraps_some()
    {
        var options = new[]
        {
            Option.Some(1),
            Option.None<int>(),
            Option.Some(3),
            Option.None<int>(),
            Option.Some(5)
        };

        var result = options.Choose().ToList();

        await Assert.That(result).Count().IsEqualTo(3);
        await Assert.That(result[0]).IsEqualTo(1);
        await Assert.That(result[1]).IsEqualTo(3);
        await Assert.That(result[2]).IsEqualTo(5);
    }

    // ChooseMap

    [Test]
    public async Task ChooseMap_applies_selector_and_filters()
    {
        var items = new[] { 1, 2, 3, 4, 5 };

        var result = items.ChooseMap(x =>
            x % 2 == 0 ? Option.Some(x * 10) : Option.None<int>()
        ).ToList();

        await Assert.That(result).Count().IsEqualTo(2);
        await Assert.That(result[0]).IsEqualTo(20);
        await Assert.That(result[1]).IsEqualTo(40);
    }

    // ChooseMapAsync

    [Test]
    public async Task ChooseMapAsync_applies_async_selector_and_filters()
    {
        var items = new[] { 1, 2, 3, 4, 5 };

        var result = (await items.ChooseMapAsync(async x =>
        {
            await Task.Yield();
            return x % 2 == 0 ? Option.Some(x * 10) : Option.None<int>();
        })).ToList();

        await Assert.That(result).Count().IsEqualTo(2);
        await Assert.That(result[0]).IsEqualTo(20);
        await Assert.That(result[1]).IsEqualTo(40);
    }

    // Sequence

    [Test]
    public async Task Sequence_all_some_returns_some_with_all_values()
    {
        var options = new[]
        {
            Option.Some(1),
            Option.Some(2),
            Option.Some(3)
        };

        var result = options.Sequence();

        await Assert.That(result.IsSome).IsTrue();
        var values = result.Match(v => v.ToList(), () => []);
        await Assert.That(values).Count().IsEqualTo(3);
        await Assert.That(values[0]).IsEqualTo(1);
        await Assert.That(values[1]).IsEqualTo(2);
        await Assert.That(values[2]).IsEqualTo(3);
    }

    [Test]
    public async Task Sequence_with_none_returns_none()
    {
        var options = new[]
        {
            Option.Some(1),
            Option.None<int>(),
            Option.Some(3)
        };

        var result = options.Sequence();

        await Assert.That(result.IsNone).IsTrue();
    }

    [Test]
    public async Task Sequence_empty_returns_some_empty()
    {
        var options = Array.Empty<Option<int>>();

        var result = options.Sequence();

        await Assert.That(result.IsSome).IsTrue();
        var values = result.Match(v => v.ToList(), () => []);
        await Assert.That(values).Count().IsEqualTo(0);
    }

    // Traverse

    [Test]
    public async Task Traverse_all_some_returns_mapped_values()
    {
        var source = new[] { 1, 2, 3 };

        var result = source.Traverse(x => Option.Some($"v{x}"));

        await Assert.That(result.IsSome).IsTrue();
        var values = result.Match(v => v.ToList(), () => []);
        await Assert.That(values).Count().IsEqualTo(3);
        await Assert.That(values[0]).IsEqualTo("v1");
        await Assert.That(values[1]).IsEqualTo("v2");
        await Assert.That(values[2]).IsEqualTo("v3");
    }

    [Test]
    public async Task Traverse_with_none_returns_none()
    {
        var source = new[] { 1, 2, 3 };

        var result = source.Traverse(x =>
            x == 2 ? Option.None<string>() : Option.Some($"v{x}"));

        await Assert.That(result.IsNone).IsTrue();
    }

    // Join (2-arity)

    [Test]
    public async Task Join_two_some_returns_tuple()
    {
        var first = Option.Some(1);
        var second = Option.Some("two");

        var joined = first.Join(second);

        await Assert.That(joined.IsSome).IsTrue();
        var (v1, v2) = joined.Match(v => v, () => default);
        await Assert.That(v1).IsEqualTo(1);
        await Assert.That(v2).IsEqualTo("two");
    }

    [Test]
    public async Task Join_two_first_none_returns_none()
    {
        var first = Option.None<int>();
        var second = Option.Some("two");

        var joined = first.Join(second);

        await Assert.That(joined.IsNone).IsTrue();
    }

    [Test]
    public async Task Join_two_second_none_returns_none()
    {
        var first = Option.Some(1);
        var second = Option.None<string>();

        var joined = first.Join(second);

        await Assert.That(joined.IsNone).IsTrue();
    }

    // Join (3-arity)

    [Test]
    public async Task Join_three_some_returns_tuple()
    {
        var first = Option.Some(1);
        var second = Option.Some("two");
        var third = Option.Some(true);

        var joined = first.Join(second, third);

        await Assert.That(joined.IsSome).IsTrue();
        var (v1, v2, v3) = joined.Match(v => v, () => default);
        await Assert.That(v1).IsEqualTo(1);
        await Assert.That(v2).IsEqualTo("two");
        await Assert.That(v3).IsTrue();
    }

    [Test]
    public async Task Join_three_any_none_returns_none()
    {
        var first = Option.Some(1);
        var second = Option.None<string>();
        var third = Option.Some(true);

        var joined = first.Join(second, third);

        await Assert.That(joined.IsNone).IsTrue();
    }

    // Join (4-arity)

    [Test]
    public async Task Join_four_some_returns_tuple()
    {
        var result = Option.Some(1)
            .Join(
                Option.Some("two"),
                Option.Some(true),
                Option.Some(4.0));

        await Assert.That(result.IsSome).IsTrue();
        var (v1, v2, v3, v4) = result.Match(v => v, () => default);
        await Assert.That(v1).IsEqualTo(1);
        await Assert.That(v2).IsEqualTo("two");
        await Assert.That(v3).IsTrue();
        await Assert.That(v4).IsEqualTo(4.0);
    }

    [Test]
    public async Task Join_four_any_none_returns_none()
    {
        var result = Option.Some(1)
            .Join(
                Option.None<string>(),
                Option.Some(true),
                Option.Some(4.0));

        await Assert.That(result.IsNone).IsTrue();
    }

    // Join (5-arity)

    [Test]
    public async Task Join_five_some_returns_tuple()
    {
        var result = Option.Some(1)
            .Join(
                Option.Some("two"),
                Option.Some(true),
                Option.Some(4.0),
                Option.Some('e'));

        await Assert.That(result.IsSome).IsTrue();
        var (v1, v2, v3, v4, v5) = result.Match(v => v, () => default);
        await Assert.That(v1).IsEqualTo(1);
        await Assert.That(v5).IsEqualTo('e');
    }

    [Test]
    public async Task Join_five_any_none_returns_none()
    {
        var result = Option.Some(1)
            .Join(
                Option.Some("two"),
                Option.Some(true),
                Option.None<double>(),
                Option.Some('e'));

        await Assert.That(result.IsNone).IsTrue();
    }

    // Join (6-arity)

    [Test]
    public async Task Join_six_some_returns_tuple()
    {
        var result = Option.Some(1)
            .Join(
                Option.Some("two"),
                Option.Some(true),
                Option.Some(4.0),
                Option.Some('e'),
                Option.Some(6L));

        await Assert.That(result.IsSome).IsTrue();
        var (v1, v2, v3, v4, v5, v6) = result.Match(v => v, () => default);
        await Assert.That(v1).IsEqualTo(1);
        await Assert.That(v6).IsEqualTo(6L);
    }

    [Test]
    public async Task Join_six_any_none_returns_none()
    {
        var result = Option.Some(1)
            .Join(
                Option.Some("two"),
                Option.Some(true),
                Option.Some(4.0),
                Option.Some('e'),
                Option.None<long>());

        await Assert.That(result.IsNone).IsTrue();
    }

    // Join (7-arity)

    [Test]
    public async Task Join_seven_some_returns_tuple()
    {
        var result = Option.Some(1)
            .Join(
                Option.Some("two"),
                Option.Some(true),
                Option.Some(4.0),
                Option.Some('e'),
                Option.Some(6L),
                Option.Some(7.0f));

        await Assert.That(result.IsSome).IsTrue();
        var (v1, v2, v3, v4, v5, v6, v7) = result.Match(v => v, () => default);
        await Assert.That(v1).IsEqualTo(1);
        await Assert.That(v7).IsEqualTo(7.0f);
    }

    [Test]
    public async Task Join_seven_any_none_returns_none()
    {
        var result = Option.None<int>()
            .Join(
                Option.Some("two"),
                Option.Some(true),
                Option.Some(4.0),
                Option.Some('e'),
                Option.Some(6L),
                Option.Some(7.0f));

        await Assert.That(result.IsNone).IsTrue();
    }

    // Join (8-arity)

    [Test]
    public async Task Join_eight_some_returns_tuple()
    {
        var result = Option.Some(1)
            .Join(
                Option.Some("two"),
                Option.Some(true),
                Option.Some(4.0),
                Option.Some('e'),
                Option.Some(6L),
                Option.Some(7.0f),
                Option.Some((byte)8));

        await Assert.That(result.IsSome).IsTrue();
        var (v1, v2, v3, v4, v5, v6, v7, v8) = result.Match(v => v, () => default);
        await Assert.That(v1).IsEqualTo(1);
        await Assert.That(v8).IsEqualTo((byte)8);
    }

    [Test]
    public async Task Join_eight_any_none_returns_none()
    {
        var result = Option.Some(1)
            .Join(
                Option.Some("two"),
                Option.Some(true),
                Option.None<double>(),
                Option.Some('e'),
                Option.Some(6L),
                Option.Some(7.0f),
                Option.Some((byte)8));

        await Assert.That(result.IsNone).IsTrue();
    }

    // SequenceAsync (sequential)

    [Test]
    public async Task SequenceAsync_all_some_returns_values()
    {
        var tasks = new[]
        {
            Task.FromResult(Option.Some(1)),
            Task.FromResult(Option.Some(2)),
            Task.FromResult(Option.Some(3))
        };

        var result = await tasks.SequenceAsync();

        await Assert.That(result.IsSome).IsTrue();
        var values = result.Match(v => v.ToList(), () => []);
        await Assert.That(values).Count().IsEqualTo(3);
        await Assert.That(values[0]).IsEqualTo(1);
        await Assert.That(values[1]).IsEqualTo(2);
        await Assert.That(values[2]).IsEqualTo(3);
    }

    [Test]
    public async Task SequenceAsync_with_none_returns_none()
    {
        var tasks = new[]
        {
            Task.FromResult(Option.Some(1)),
            Task.FromResult(Option.None<int>()),
            Task.FromResult(Option.Some(3))
        };

        var result = await tasks.SequenceAsync();

        await Assert.That(result.IsNone).IsTrue();
    }

    // TraverseAsync (sequential)

    [Test]
    public async Task TraverseAsync_all_some_returns_mapped_values()
    {
        var source = new[] { 1, 2, 3 };

        var result = await source.TraverseAsync(async x =>
        {
            await Task.Yield();
            return Option.Some($"v{x}");
        });

        await Assert.That(result.IsSome).IsTrue();
        var values = result.Match(v => v.ToList(), () => []);
        await Assert.That(values).Count().IsEqualTo(3);
        await Assert.That(values[0]).IsEqualTo("v1");
    }

    [Test]
    public async Task TraverseAsync_with_none_returns_none()
    {
        var source = new[] { 1, 2, 3 };

        var result = await source.TraverseAsync(async x =>
        {
            await Task.Yield();
            return x == 2 ? Option.None<string>() : Option.Some($"v{x}");
        });

        await Assert.That(result.IsNone).IsTrue();
    }

    // SequenceParallel

    [Test]
    public async Task SequenceParallel_all_some_returns_values()
    {
        var tasks = new[]
        {
            Task.FromResult(Option.Some(1)),
            Task.FromResult(Option.Some(2)),
            Task.FromResult(Option.Some(3))
        };

        var result = await tasks.SequenceParallel();

        await Assert.That(result.IsSome).IsTrue();
        var values = result.Match(v => v.ToList(), () => []);
        await Assert.That(values).Count().IsEqualTo(3);
    }

    [Test]
    public async Task SequenceParallel_with_none_returns_none()
    {
        var tasks = new[]
        {
            Task.FromResult(Option.Some(1)),
            Task.FromResult(Option.None<int>()),
            Task.FromResult(Option.Some(3))
        };

        var result = await tasks.SequenceParallel();

        await Assert.That(result.IsNone).IsTrue();
    }

    // TraverseParallel

    [Test]
    public async Task TraverseParallel_all_some_returns_mapped_values()
    {
        var source = new[] { 1, 2, 3 };

        var result = await source.TraverseParallel(async x =>
        {
            await Task.Yield();
            return Option.Some($"v{x}");
        });

        await Assert.That(result.IsSome).IsTrue();
        var values = result.Match(v => v.ToList(), () => []);
        await Assert.That(values).Count().IsEqualTo(3);
    }

    [Test]
    public async Task TraverseParallel_with_none_returns_none()
    {
        var source = new[] { 1, 2, 3 };

        var result = await source.TraverseParallel(async x =>
        {
            await Task.Yield();
            return x == 2 ? Option.None<string>() : Option.Some($"v{x}");
        });

        await Assert.That(result.IsNone).IsTrue();
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
                return Option.Some(1);
            }),
            Task.Run(async () =>
            {
                task2Started.SetResult();
                await task1Started.Task;
                return Option.Some(2);
            })
        };

        var result = await tasks.SequenceParallel();

        await Assert.That(result.IsSome).IsTrue();
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
            return Option.Some(x);
        });

        await Assert.That(result.IsSome).IsTrue();
    }
}

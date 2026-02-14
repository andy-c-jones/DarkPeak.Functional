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
}

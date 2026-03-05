using DarkPeak.Functional.Extensions;

namespace DarkPeak.Functional.Tests;

public class OneOfShould
{
    [Test]
    public async Task Create_from_implicit_conversion()
    {
        OneOf<string, int, bool> first = "hello";
        OneOf<string, int, bool> second = 42;
        OneOf<string, int, bool> third = true;

        await Assert.That(first.IsT1).IsTrue();
        await Assert.That(second.IsT2).IsTrue();
        await Assert.That(third.IsT3).IsTrue();
    }

    [Test]
    public async Task Match_returns_selected_case_result()
    {
        OneOf<string, int, bool> value = 42;

        var result = value.Match(
            t1 => t1.Length,
            t2 => t2,
            t3 => t3 ? 1 : 0);

        await Assert.That(result).IsEqualTo(42);
    }

    [Test]
    public async Task MatchAsync_returns_selected_case_result()
    {
        OneOf<string, int, bool> value = true;

        var result = await value.MatchAsync(
            t1 => Task.FromResult(t1.Length),
            t2 => Task.FromResult(t2),
            t3 => Task.FromResult(t3 ? 1 : 0));

        await Assert.That(result).IsEqualTo(1);
    }

    [Test]
    public async Task MapThird_transforms_third_case()
    {
        OneOf<string, int, bool> value = true;

        var mapped = value.MapThird(flag => flag ? "yes" : "no");

        await Assert.That(mapped.IsT3).IsTrue();
        await Assert.That(mapped.AsT3).IsEqualTo("yes");
    }

    [Test]
    public async Task ReduceThird_narrows_union()
    {
        OneOf<string, int, bool> value = true;

        var reduced = value.ReduceThird(flag => flag ? "ok" : 0);

        await Assert.That(reduced.IsT1).IsTrue();
        await Assert.That(reduced.AsT1).IsEqualTo("ok");
    }

    [Test]
    public async Task AsT_throws_when_wrong_case()
    {
        OneOf<string, int, bool> value = 42;

        await Assert.That(() => value.AsT1).Throws<InvalidOperationException>();
    }

    [Test]
    public async Task Select_maps_last_case()
    {
        OneOf<string, int, bool> value = true;

        var mapped = from v in value
                     select v ? "done" : "skip";

        await Assert.That(mapped.IsT3).IsTrue();
        await Assert.That(mapped.AsT3).IsEqualTo("done");
    }

    [Test]
    public async Task SelectMany_binds_last_case()
    {
        OneOf<string, int, bool> first = true;
        OneOf<string, int, bool> second = false;

        var query = from a in first
                    from b in second
                    select a && b;

        await Assert.That(query.IsT3).IsTrue();
        await Assert.That(query.AsT3).IsFalse();
    }

    [Test]
    public async Task SelectMany_preserves_earlier_case()
    {
        OneOf<string, int, bool> value = "error";

        var query = from v in value
                    select !v;

        await Assert.That(query.IsT1).IsTrue();
        await Assert.That(query.AsT1).IsEqualTo("error");
    }

    [Test]
    public async Task Converts_to_and_from_either()
    {
        Either<string, int> either = 42;

        var oneOf = either.ToOneOf();
        var converted = oneOf.ToEither();

        await Assert.That(oneOf.IsT2).IsTrue();
        await Assert.That(converted.IsRight).IsTrue();
        await Assert.That(converted.Match(_ => 0, v => v)).IsEqualTo(42);
    }

    [Test]
    public async Task Supports_factory_helpers()
    {
        var value = OneOf.Third<string, int, bool>(true);

        await Assert.That(value.IsT3).IsTrue();
        await Assert.That(value.AsT3).IsTrue();
    }

    [Test]
    public async Task Supports_higher_arity_match()
    {
        OneOf<int, string, bool, double> value = 1.5d;

        var result = value.Match(
            t1 => t1.ToString(),
            t2 => t2,
            t3 => t3.ToString(),
            t4 => t4.ToString("F1"));

        await Assert.That(result).IsEqualTo("1.5");
    }
}

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
    public async Task Arity2_map_methods_execute()
    {
        OneOf<string, int> value = "a";
        var mappedFirst = value.MapFirst(v => v + "!");
        await Assert.That(mappedFirst.IsT1).IsTrue();
        var mappedSecond = value.MapSecond(v => v);
        await Assert.That(mappedSecond.IsT1).IsTrue();
    }

    [Test]
    public async Task Arity2_linq_methods_execute()
    {
        OneOf<string, int> value = 2;
        var selected = from v in value
                       select v + 1;
        await Assert.That(selected.IsT2).IsTrue();
    }

    [Test]
    public async Task Arity2_wrong_accessor_throws()
    {
        OneOf<string, int> value = 2;
        await Assert.That(() => value.AsT1).Throws<InvalidOperationException>();
    }

    [Test]
    public async Task Arity3_map_methods_execute()
    {
        OneOf<string, int, bool> value = "a";
        var mappedFirst = value.MapFirst(v => v + "!");
        await Assert.That(mappedFirst.IsT1).IsTrue();
        var mappedSecond = value.MapSecond(v => v);
        await Assert.That(mappedSecond.IsT1).IsTrue();
        var mappedThird = value.MapThird(v => v);
        await Assert.That(mappedThird.IsT1).IsTrue();
    }

    [Test]
    public async Task Arity3_reduce_methods_execute()
    {
        OneOf<string, int, bool> value = "a";
        var reducedFirst = value.ReduceFirst(_ => 2);
        await Assert.That(reducedFirst.IsT1).IsTrue();
        var reducedSecond = value.ReduceSecond(_ => "a");
        await Assert.That(reducedSecond.IsT1).IsTrue();
        var reducedThird = value.ReduceThird(_ => "a");
        await Assert.That(reducedThird.IsT1).IsTrue();
    }

    [Test]
    public async Task Arity3_linq_methods_execute()
    {
        OneOf<string, int, bool> value = true;
        var selected = from v in value
                       select !v;
        await Assert.That(selected.IsT3).IsTrue();
    }

    [Test]
    public async Task Arity3_wrong_accessor_throws()
    {
        OneOf<string, int, bool> value = 2;
        await Assert.That(() => value.AsT1).Throws<InvalidOperationException>();
    }

    [Test]
    public async Task Arity4_map_methods_execute()
    {
        OneOf<string, int, bool, double> value = "a";
        var mappedFirst = value.MapFirst(v => v + "!");
        await Assert.That(mappedFirst.IsT1).IsTrue();
        var mappedSecond = value.MapSecond(v => v);
        await Assert.That(mappedSecond.IsT1).IsTrue();
        var mappedThird = value.MapThird(v => v);
        await Assert.That(mappedThird.IsT1).IsTrue();
        var mappedFourth = value.MapFourth(v => v);
        await Assert.That(mappedFourth.IsT1).IsTrue();
    }

    [Test]
    public async Task Arity4_reduce_methods_execute()
    {
        OneOf<string, int, bool, double> value = "a";
        var reducedFirst = value.ReduceFirst(_ => 2);
        await Assert.That(reducedFirst.IsT1).IsTrue();
        var reducedSecond = value.ReduceSecond(_ => "a");
        await Assert.That(reducedSecond.IsT1).IsTrue();
        var reducedThird = value.ReduceThird(_ => "a");
        await Assert.That(reducedThird.IsT1).IsTrue();
        var reducedFourth = value.ReduceFourth(_ => "a");
        await Assert.That(reducedFourth.IsT1).IsTrue();
    }

    [Test]
    public async Task Arity4_linq_methods_execute()
    {
        OneOf<string, int, bool, double> value = 4.0d;
        var selected = from v in value
                       select v;
        await Assert.That(selected.IsT4).IsTrue();
    }

    [Test]
    public async Task Arity4_wrong_accessor_throws()
    {
        OneOf<string, int, bool, double> value = 2;
        await Assert.That(() => value.AsT1).Throws<InvalidOperationException>();
    }

    [Test]
    public async Task Arity5_map_methods_execute()
    {
        OneOf<string, int, bool, double, long> value = "a";
        var mappedFirst = value.MapFirst(v => v + "!");
        await Assert.That(mappedFirst.IsT1).IsTrue();
        var mappedSecond = value.MapSecond(v => v);
        await Assert.That(mappedSecond.IsT1).IsTrue();
        var mappedThird = value.MapThird(v => v);
        await Assert.That(mappedThird.IsT1).IsTrue();
        var mappedFourth = value.MapFourth(v => v);
        await Assert.That(mappedFourth.IsT1).IsTrue();
        var mappedFifth = value.MapFifth(v => v);
        await Assert.That(mappedFifth.IsT1).IsTrue();
    }

    [Test]
    public async Task Arity5_reduce_methods_execute()
    {
        OneOf<string, int, bool, double, long> value = "a";
        var reducedFirst = value.ReduceFirst(_ => 2);
        await Assert.That(reducedFirst.IsT1).IsTrue();
        var reducedSecond = value.ReduceSecond(_ => "a");
        await Assert.That(reducedSecond.IsT1).IsTrue();
        var reducedThird = value.ReduceThird(_ => "a");
        await Assert.That(reducedThird.IsT1).IsTrue();
        var reducedFourth = value.ReduceFourth(_ => "a");
        await Assert.That(reducedFourth.IsT1).IsTrue();
        var reducedFifth = value.ReduceFifth(_ => "a");
        await Assert.That(reducedFifth.IsT1).IsTrue();
    }

    [Test]
    public async Task Arity5_linq_methods_execute()
    {
        OneOf<string, int, bool, double, long> value = 5L;
        var selected = from v in value
                       select v;
        await Assert.That(selected.IsT5).IsTrue();
    }

    [Test]
    public async Task Arity5_wrong_accessor_throws()
    {
        OneOf<string, int, bool, double, long> value = 2;
        await Assert.That(() => value.AsT1).Throws<InvalidOperationException>();
    }

    [Test]
    public async Task Arity6_map_methods_execute()
    {
        OneOf<string, int, bool, double, long, decimal> value = "a";
        var mappedFirst = value.MapFirst(v => v + "!");
        await Assert.That(mappedFirst.IsT1).IsTrue();
        var mappedSecond = value.MapSecond(v => v);
        await Assert.That(mappedSecond.IsT1).IsTrue();
        var mappedThird = value.MapThird(v => v);
        await Assert.That(mappedThird.IsT1).IsTrue();
        var mappedFourth = value.MapFourth(v => v);
        await Assert.That(mappedFourth.IsT1).IsTrue();
        var mappedFifth = value.MapFifth(v => v);
        await Assert.That(mappedFifth.IsT1).IsTrue();
        var mappedSixth = value.MapSixth(v => v);
        await Assert.That(mappedSixth.IsT1).IsTrue();
    }

    [Test]
    public async Task Arity6_reduce_methods_execute()
    {
        OneOf<string, int, bool, double, long, decimal> value = "a";
        var reducedFirst = value.ReduceFirst(_ => 2);
        await Assert.That(reducedFirst.IsT1).IsTrue();
        var reducedSecond = value.ReduceSecond(_ => "a");
        await Assert.That(reducedSecond.IsT1).IsTrue();
        var reducedThird = value.ReduceThird(_ => "a");
        await Assert.That(reducedThird.IsT1).IsTrue();
        var reducedFourth = value.ReduceFourth(_ => "a");
        await Assert.That(reducedFourth.IsT1).IsTrue();
        var reducedFifth = value.ReduceFifth(_ => "a");
        await Assert.That(reducedFifth.IsT1).IsTrue();
        var reducedSixth = value.ReduceSixth(_ => "a");
        await Assert.That(reducedSixth.IsT1).IsTrue();
    }

    [Test]
    public async Task Arity6_linq_methods_execute()
    {
        OneOf<string, int, bool, double, long, decimal> value = 6m;
        var selected = from v in value
                       select v;
        await Assert.That(selected.IsT6).IsTrue();
    }

    [Test]
    public async Task Arity6_wrong_accessor_throws()
    {
        OneOf<string, int, bool, double, long, decimal> value = 2;
        await Assert.That(() => value.AsT1).Throws<InvalidOperationException>();
    }

    [Test]
    public async Task Arity7_map_methods_execute()
    {
        OneOf<string, int, bool, double, long, decimal, char> value = "a";
        var mappedFirst = value.MapFirst(v => v + "!");
        await Assert.That(mappedFirst.IsT1).IsTrue();
        var mappedSecond = value.MapSecond(v => v);
        await Assert.That(mappedSecond.IsT1).IsTrue();
        var mappedThird = value.MapThird(v => v);
        await Assert.That(mappedThird.IsT1).IsTrue();
        var mappedFourth = value.MapFourth(v => v);
        await Assert.That(mappedFourth.IsT1).IsTrue();
        var mappedFifth = value.MapFifth(v => v);
        await Assert.That(mappedFifth.IsT1).IsTrue();
        var mappedSixth = value.MapSixth(v => v);
        await Assert.That(mappedSixth.IsT1).IsTrue();
        var mappedSeventh = value.MapSeventh(v => v);
        await Assert.That(mappedSeventh.IsT1).IsTrue();
    }

    [Test]
    public async Task Arity7_reduce_methods_execute()
    {
        OneOf<string, int, bool, double, long, decimal, char> value = "a";
        var reducedFirst = value.ReduceFirst(_ => 2);
        await Assert.That(reducedFirst.IsT1).IsTrue();
        var reducedSecond = value.ReduceSecond(_ => "a");
        await Assert.That(reducedSecond.IsT1).IsTrue();
        var reducedThird = value.ReduceThird(_ => "a");
        await Assert.That(reducedThird.IsT1).IsTrue();
        var reducedFourth = value.ReduceFourth(_ => "a");
        await Assert.That(reducedFourth.IsT1).IsTrue();
        var reducedFifth = value.ReduceFifth(_ => "a");
        await Assert.That(reducedFifth.IsT1).IsTrue();
        var reducedSixth = value.ReduceSixth(_ => "a");
        await Assert.That(reducedSixth.IsT1).IsTrue();
        var reducedSeventh = value.ReduceSeventh(_ => "a");
        await Assert.That(reducedSeventh.IsT1).IsTrue();
    }

    [Test]
    public async Task Arity7_linq_methods_execute()
    {
        OneOf<string, int, bool, double, long, decimal, char> value = 'x';
        var selected = from v in value
                       select v;
        await Assert.That(selected.IsT7).IsTrue();
    }

    [Test]
    public async Task Arity7_wrong_accessor_throws()
    {
        OneOf<string, int, bool, double, long, decimal, char> value = 2;
        await Assert.That(() => value.AsT1).Throws<InvalidOperationException>();
    }

    [Test]
    public async Task Arity8_map_methods_execute()
    {
        OneOf<string, int, bool, double, long, decimal, char, byte> value = "a";
        var mappedFirst = value.MapFirst(v => v + "!");
        await Assert.That(mappedFirst.IsT1).IsTrue();
        var mappedSecond = value.MapSecond(v => v);
        await Assert.That(mappedSecond.IsT1).IsTrue();
        var mappedThird = value.MapThird(v => v);
        await Assert.That(mappedThird.IsT1).IsTrue();
        var mappedFourth = value.MapFourth(v => v);
        await Assert.That(mappedFourth.IsT1).IsTrue();
        var mappedFifth = value.MapFifth(v => v);
        await Assert.That(mappedFifth.IsT1).IsTrue();
        var mappedSixth = value.MapSixth(v => v);
        await Assert.That(mappedSixth.IsT1).IsTrue();
        var mappedSeventh = value.MapSeventh(v => v);
        await Assert.That(mappedSeventh.IsT1).IsTrue();
        var mappedEighth = value.MapEighth(v => v);
        await Assert.That(mappedEighth.IsT1).IsTrue();
    }

    [Test]
    public async Task Arity8_reduce_methods_execute()
    {
        OneOf<string, int, bool, double, long, decimal, char, byte> value = "a";
        var reducedFirst = value.ReduceFirst(_ => 2);
        await Assert.That(reducedFirst.IsT1).IsTrue();
        var reducedSecond = value.ReduceSecond(_ => "a");
        await Assert.That(reducedSecond.IsT1).IsTrue();
        var reducedThird = value.ReduceThird(_ => "a");
        await Assert.That(reducedThird.IsT1).IsTrue();
        var reducedFourth = value.ReduceFourth(_ => "a");
        await Assert.That(reducedFourth.IsT1).IsTrue();
        var reducedFifth = value.ReduceFifth(_ => "a");
        await Assert.That(reducedFifth.IsT1).IsTrue();
        var reducedSixth = value.ReduceSixth(_ => "a");
        await Assert.That(reducedSixth.IsT1).IsTrue();
        var reducedSeventh = value.ReduceSeventh(_ => "a");
        await Assert.That(reducedSeventh.IsT1).IsTrue();
        var reducedEighth = value.ReduceEighth(_ => "a");
        await Assert.That(reducedEighth.IsT1).IsTrue();
    }

    [Test]
    public async Task Arity8_linq_methods_execute()
    {
        OneOf<string, int, bool, double, long, decimal, char, byte> value = (byte)8;
        var selected = from v in value
                       select v;
        await Assert.That(selected.IsT8).IsTrue();
    }

    [Test]
    public async Task Arity8_wrong_accessor_throws()
    {
        OneOf<string, int, bool, double, long, decimal, char, byte> value = 2;
        await Assert.That(() => value.AsT1).Throws<InvalidOperationException>();
    }

}
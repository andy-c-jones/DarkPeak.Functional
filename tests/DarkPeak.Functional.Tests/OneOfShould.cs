using DarkPeak.Functional.Extensions;

namespace DarkPeak.Functional.Tests;

public class OneOfShould
{
    private static T CreateInvalidState<T>(object value)
    {
        var ctor = typeof(T)
            .GetConstructors(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .Single(c => c.GetParameters().Length == 2 && c.GetParameters()[0].ParameterType == typeof(byte));
        return (T)ctor.Invoke(new object[] { (byte)255, value });
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
    public async Task Arity2_factory_methods_create_all_cases()
    {
        var v1 = OneOf.First<string, int>("a");
        await Assert.That(v1.IsT1).IsTrue();
        var v2 = OneOf.Second<string, int>(2);
        await Assert.That(v2.IsT2).IsTrue();
    }

    [Test]
    public async Task Arity2_match_and_accessors_cover_all_cases()
    {
        OneOf<string, int> c1 = "a";
        await Assert.That(c1.IsT1).IsTrue();
        await Assert.That(c1.AsT1).IsEqualTo("a");
        await Assert.That(c1.Match(t1 => 1, t2 => 2)).IsEqualTo(1);
        await Assert.That(await c1.MatchAsync(t1 => Task.FromResult(1), t2 => Task.FromResult(2))).IsEqualTo(1);
        OneOf<string, int> c2 = 2;
        await Assert.That(c2.IsT2).IsTrue();
        await Assert.That(c2.AsT2).IsEqualTo(2);
        await Assert.That(c2.Match(t1 => 1, t2 => 2)).IsEqualTo(2);
        await Assert.That(await c2.MatchAsync(t1 => Task.FromResult(1), t2 => Task.FromResult(2))).IsEqualTo(2);
    }

    [Test]
    public async Task Arity2_map_methods_execute_active_case()
    {
        OneOf<string, int> map1 = "a";
        var mapped1 = map1.MapFirst(v => v + "!");
        await Assert.That(mapped1.IsT1).IsTrue();
        await Assert.That(mapped1.AsT1).IsEqualTo("a!");
        OneOf<string, int> map2 = 2;
        var mapped2 = map2.MapSecond(v => v + 1);
        await Assert.That(mapped2.IsT2).IsTrue();
        await Assert.That(mapped2.AsT2).IsEqualTo(3);
    }

    [Test]
    public async Task Arity2_linq_select_and_bind_cover_paths()
    {
        OneOf<string, int> success = 2;
        var selected = from x in success
                       select (x + 1);
        await Assert.That(selected.IsT2).IsTrue();
        await Assert.That(selected.AsT2).IsEqualTo(3);

        var bound = from x in success
                    from y in (OneOf<string, int>)(2)
                    select x;
        await Assert.That(bound.IsT2).IsTrue();

        OneOf<string, int> fail = "a";
        var shortCircuit = from x in fail
                           from y in (OneOf<string, int>)(2)
                           select x;
        await Assert.That(shortCircuit.IsT1).IsTrue();
    }

    [Test]
    public async Task Arity2_wrong_accessor_throws()
    {
        OneOf<string, int> value = 2;
        await Assert.That(() => value.AsT1).Throws<InvalidOperationException>();
    }

    [Test]
    public async Task Arity2_map_does_not_execute_inactive_mapper()
    {
        OneOf<string, int> value = "left";
        var mapSecondCalled = false;

        var mapped = value.MapSecond(v =>
        {
            mapSecondCalled = true;
            return v + 1;
        });

        await Assert.That(mapped.IsT1).IsTrue();
        await Assert.That(mapped.AsT1).IsEqualTo("left");
        await Assert.That(mapSecondCalled).IsFalse();
    }

    [Test]
    public async Task Arity3_factory_methods_create_all_cases()
    {
        var v1 = OneOf.First<string, int, bool>("a");
        await Assert.That(v1.IsT1).IsTrue();
        var v2 = OneOf.Second<string, int, bool>(2);
        await Assert.That(v2.IsT2).IsTrue();
        var v3 = OneOf.Third<string, int, bool>(true);
        await Assert.That(v3.IsT3).IsTrue();
    }

    [Test]
    public async Task Arity3_match_and_accessors_cover_all_cases()
    {
        OneOf<string, int, bool> c1 = "a";
        await Assert.That(c1.IsT1).IsTrue();
        await Assert.That(c1.AsT1).IsEqualTo("a");
        await Assert.That(c1.Match(t1 => 1, t2 => 2, t3 => 3)).IsEqualTo(1);
        await Assert.That(await c1.MatchAsync(t1 => Task.FromResult(1), t2 => Task.FromResult(2), t3 => Task.FromResult(3))).IsEqualTo(1);
        OneOf<string, int, bool> c2 = 2;
        await Assert.That(c2.IsT2).IsTrue();
        await Assert.That(c2.AsT2).IsEqualTo(2);
        await Assert.That(c2.Match(t1 => 1, t2 => 2, t3 => 3)).IsEqualTo(2);
        await Assert.That(await c2.MatchAsync(t1 => Task.FromResult(1), t2 => Task.FromResult(2), t3 => Task.FromResult(3))).IsEqualTo(2);
        OneOf<string, int, bool> c3 = true;
        await Assert.That(c3.IsT3).IsTrue();
        await Assert.That(c3.AsT3).IsTrue();
        await Assert.That(c3.Match(t1 => 1, t2 => 2, t3 => 3)).IsEqualTo(3);
        await Assert.That(await c3.MatchAsync(t1 => Task.FromResult(1), t2 => Task.FromResult(2), t3 => Task.FromResult(3))).IsEqualTo(3);
    }

    [Test]
    public async Task Arity3_map_methods_execute_active_case()
    {
        OneOf<string, int, bool> map1 = "a";
        var mapped1 = map1.MapFirst(v => v + "!");
        await Assert.That(mapped1.IsT1).IsTrue();
        await Assert.That(mapped1.AsT1).IsEqualTo("a!");
        OneOf<string, int, bool> map2 = 2;
        var mapped2 = map2.MapSecond(v => v + 1);
        await Assert.That(mapped2.IsT2).IsTrue();
        await Assert.That(mapped2.AsT2).IsEqualTo(3);
        OneOf<string, int, bool> map3 = true;
        var mapped3 = map3.MapThird(v => !v);
        await Assert.That(mapped3.IsT3).IsTrue();
        await Assert.That(mapped3.AsT3).IsFalse();
    }

    [Test]
    public async Task Arity3_reduce_methods_execute_active_case()
    {
        OneOf<string, int, bool> r1 = "a";
        var reduced1 = r1.ReduceFirst(_ => 2);
        await Assert.That(reduced1.IsT1).IsTrue();
        OneOf<string, int, bool> r2 = 2;
        var reduced2 = r2.ReduceSecond(_ => "a");
        await Assert.That(reduced2.IsT1).IsTrue();
        OneOf<string, int, bool> r3 = true;
        var reduced3 = r3.ReduceThird(_ => "a");
        await Assert.That(reduced3.IsT1).IsTrue();
    }

    [Test]
    public async Task Arity3_linq_select_and_bind_cover_paths()
    {
        OneOf<string, int, bool> success = true;
        var selected = from x in success
                       select (!x);
        await Assert.That(selected.IsT3).IsTrue();
        await Assert.That(selected.AsT3).IsFalse();

        var bound = from x in success
                    from y in (OneOf<string, int, bool>)(true)
                    select x;
        await Assert.That(bound.IsT3).IsTrue();

        OneOf<string, int, bool> fail = "a";
        var shortCircuit = from x in fail
                           from y in (OneOf<string, int, bool>)(true)
                           select x;
        await Assert.That(shortCircuit.IsT1).IsTrue();
    }

    [Test]
    public async Task Arity3_wrong_accessor_throws()
    {
        OneOf<string, int, bool> value = 2;
        await Assert.That(() => value.AsT1).Throws<InvalidOperationException>();
    }

    [Test]
    public async Task Arity3_reduce_does_not_execute_inactive_reducer()
    {
        OneOf<string, int, bool> value = "left";
        var reduceThirdCalled = false;

        var reduced = value.ReduceThird(v =>
        {
            reduceThirdCalled = true;
            return v ? "true" : "false";
        });

        await Assert.That(reduced.IsT1).IsTrue();
        await Assert.That(reduced.AsT1).IsEqualTo("left");
        await Assert.That(reduceThirdCalled).IsFalse();
    }

    [Test]
    public async Task Arity4_factory_methods_create_all_cases()
    {
        var v1 = OneOf.First<string, int, bool, double>("a");
        await Assert.That(v1.IsT1).IsTrue();
        var v2 = OneOf.Second<string, int, bool, double>(2);
        await Assert.That(v2.IsT2).IsTrue();
        var v3 = OneOf.Third<string, int, bool, double>(true);
        await Assert.That(v3.IsT3).IsTrue();
        var v4 = OneOf.Fourth<string, int, bool, double>(4.0d);
        await Assert.That(v4.IsT4).IsTrue();
    }

    [Test]
    public async Task Arity4_match_and_accessors_cover_all_cases()
    {
        OneOf<string, int, bool, double> c1 = "a";
        await Assert.That(c1.IsT1).IsTrue();
        await Assert.That(c1.AsT1).IsEqualTo("a");
        await Assert.That(c1.Match(t1 => 1, t2 => 2, t3 => 3, t4 => 4)).IsEqualTo(1);
        await Assert.That(await c1.MatchAsync(t1 => Task.FromResult(1), t2 => Task.FromResult(2), t3 => Task.FromResult(3), t4 => Task.FromResult(4))).IsEqualTo(1);
        OneOf<string, int, bool, double> c2 = 2;
        await Assert.That(c2.IsT2).IsTrue();
        await Assert.That(c2.AsT2).IsEqualTo(2);
        await Assert.That(c2.Match(t1 => 1, t2 => 2, t3 => 3, t4 => 4)).IsEqualTo(2);
        await Assert.That(await c2.MatchAsync(t1 => Task.FromResult(1), t2 => Task.FromResult(2), t3 => Task.FromResult(3), t4 => Task.FromResult(4))).IsEqualTo(2);
        OneOf<string, int, bool, double> c3 = true;
        await Assert.That(c3.IsT3).IsTrue();
        await Assert.That(c3.AsT3).IsTrue();
        await Assert.That(c3.Match(t1 => 1, t2 => 2, t3 => 3, t4 => 4)).IsEqualTo(3);
        await Assert.That(await c3.MatchAsync(t1 => Task.FromResult(1), t2 => Task.FromResult(2), t3 => Task.FromResult(3), t4 => Task.FromResult(4))).IsEqualTo(3);
        OneOf<string, int, bool, double> c4 = 4.0d;
        await Assert.That(c4.IsT4).IsTrue();
        await Assert.That(c4.AsT4).IsEqualTo(4.0d);
        await Assert.That(c4.Match(t1 => 1, t2 => 2, t3 => 3, t4 => 4)).IsEqualTo(4);
        await Assert.That(await c4.MatchAsync(t1 => Task.FromResult(1), t2 => Task.FromResult(2), t3 => Task.FromResult(3), t4 => Task.FromResult(4))).IsEqualTo(4);
    }

    [Test]
    public async Task Arity4_map_methods_execute_active_case()
    {
        OneOf<string, int, bool, double> map1 = "a";
        var mapped1 = map1.MapFirst(v => v + "!");
        await Assert.That(mapped1.IsT1).IsTrue();
        await Assert.That(mapped1.AsT1).IsEqualTo("a!");
        OneOf<string, int, bool, double> map2 = 2;
        var mapped2 = map2.MapSecond(v => v + 1);
        await Assert.That(mapped2.IsT2).IsTrue();
        await Assert.That(mapped2.AsT2).IsEqualTo(3);
        OneOf<string, int, bool, double> map3 = true;
        var mapped3 = map3.MapThird(v => !v);
        await Assert.That(mapped3.IsT3).IsTrue();
        await Assert.That(mapped3.AsT3).IsFalse();
        OneOf<string, int, bool, double> map4 = 4.0d;
        var mapped4 = map4.MapFourth(v => v + 1d);
        await Assert.That(mapped4.IsT4).IsTrue();
        await Assert.That(mapped4.AsT4).IsEqualTo(5.0d);
    }

    [Test]
    public async Task Arity4_reduce_methods_execute_active_case()
    {
        OneOf<string, int, bool, double> r1 = "a";
        var reduced1 = r1.ReduceFirst(_ => 2);
        await Assert.That(reduced1.IsT1).IsTrue();
        OneOf<string, int, bool, double> r2 = 2;
        var reduced2 = r2.ReduceSecond(_ => "a");
        await Assert.That(reduced2.IsT1).IsTrue();
        OneOf<string, int, bool, double> r3 = true;
        var reduced3 = r3.ReduceThird(_ => "a");
        await Assert.That(reduced3.IsT1).IsTrue();
        OneOf<string, int, bool, double> r4 = 4.0d;
        var reduced4 = r4.ReduceFourth(_ => "a");
        await Assert.That(reduced4.IsT1).IsTrue();
    }

    [Test]
    public async Task Arity4_linq_select_and_bind_cover_paths()
    {
        OneOf<string, int, bool, double> success = 4.0d;
        var selected = from x in success
                       select (x + 1d);
        await Assert.That(selected.IsT4).IsTrue();
        await Assert.That(selected.AsT4).IsEqualTo(5.0d);

        var bound = from x in success
                    from y in (OneOf<string, int, bool, double>)(4.0d)
                    select x;
        await Assert.That(bound.IsT4).IsTrue();

        OneOf<string, int, bool, double> fail = "a";
        var shortCircuit = from x in fail
                           from y in (OneOf<string, int, bool, double>)(4.0d)
                           select x;
        await Assert.That(shortCircuit.IsT1).IsTrue();
    }

    [Test]
    public async Task Arity4_wrong_accessor_throws()
    {
        OneOf<string, int, bool, double> value = 2;
        await Assert.That(() => value.AsT1).Throws<InvalidOperationException>();
    }

    [Test]
    public async Task Arity5_factory_methods_create_all_cases()
    {
        var v1 = OneOf.First<string, int, bool, double, long>("a");
        await Assert.That(v1.IsT1).IsTrue();
        var v2 = OneOf.Second<string, int, bool, double, long>(2);
        await Assert.That(v2.IsT2).IsTrue();
        var v3 = OneOf.Third<string, int, bool, double, long>(true);
        await Assert.That(v3.IsT3).IsTrue();
        var v4 = OneOf.Fourth<string, int, bool, double, long>(4.0d);
        await Assert.That(v4.IsT4).IsTrue();
        var v5 = OneOf.Fifth<string, int, bool, double, long>(5L);
        await Assert.That(v5.IsT5).IsTrue();
    }

    [Test]
    public async Task Arity5_match_and_accessors_cover_all_cases()
    {
        OneOf<string, int, bool, double, long> c1 = "a";
        await Assert.That(c1.IsT1).IsTrue();
        await Assert.That(c1.AsT1).IsEqualTo("a");
        await Assert.That(c1.Match(t1 => 1, t2 => 2, t3 => 3, t4 => 4, t5 => 5)).IsEqualTo(1);
        await Assert.That(await c1.MatchAsync(t1 => Task.FromResult(1), t2 => Task.FromResult(2), t3 => Task.FromResult(3), t4 => Task.FromResult(4), t5 => Task.FromResult(5))).IsEqualTo(1);
        OneOf<string, int, bool, double, long> c2 = 2;
        await Assert.That(c2.IsT2).IsTrue();
        await Assert.That(c2.AsT2).IsEqualTo(2);
        await Assert.That(c2.Match(t1 => 1, t2 => 2, t3 => 3, t4 => 4, t5 => 5)).IsEqualTo(2);
        await Assert.That(await c2.MatchAsync(t1 => Task.FromResult(1), t2 => Task.FromResult(2), t3 => Task.FromResult(3), t4 => Task.FromResult(4), t5 => Task.FromResult(5))).IsEqualTo(2);
        OneOf<string, int, bool, double, long> c3 = true;
        await Assert.That(c3.IsT3).IsTrue();
        await Assert.That(c3.AsT3).IsTrue();
        await Assert.That(c3.Match(t1 => 1, t2 => 2, t3 => 3, t4 => 4, t5 => 5)).IsEqualTo(3);
        await Assert.That(await c3.MatchAsync(t1 => Task.FromResult(1), t2 => Task.FromResult(2), t3 => Task.FromResult(3), t4 => Task.FromResult(4), t5 => Task.FromResult(5))).IsEqualTo(3);
        OneOf<string, int, bool, double, long> c4 = 4.0d;
        await Assert.That(c4.IsT4).IsTrue();
        await Assert.That(c4.AsT4).IsEqualTo(4.0d);
        await Assert.That(c4.Match(t1 => 1, t2 => 2, t3 => 3, t4 => 4, t5 => 5)).IsEqualTo(4);
        await Assert.That(await c4.MatchAsync(t1 => Task.FromResult(1), t2 => Task.FromResult(2), t3 => Task.FromResult(3), t4 => Task.FromResult(4), t5 => Task.FromResult(5))).IsEqualTo(4);
        OneOf<string, int, bool, double, long> c5 = 5L;
        await Assert.That(c5.IsT5).IsTrue();
        await Assert.That(c5.AsT5).IsEqualTo(5L);
        await Assert.That(c5.Match(t1 => 1, t2 => 2, t3 => 3, t4 => 4, t5 => 5)).IsEqualTo(5);
        await Assert.That(await c5.MatchAsync(t1 => Task.FromResult(1), t2 => Task.FromResult(2), t3 => Task.FromResult(3), t4 => Task.FromResult(4), t5 => Task.FromResult(5))).IsEqualTo(5);
    }

    [Test]
    public async Task Arity5_map_methods_execute_active_case()
    {
        OneOf<string, int, bool, double, long> map1 = "a";
        var mapped1 = map1.MapFirst(v => v + "!");
        await Assert.That(mapped1.IsT1).IsTrue();
        await Assert.That(mapped1.AsT1).IsEqualTo("a!");
        OneOf<string, int, bool, double, long> map2 = 2;
        var mapped2 = map2.MapSecond(v => v + 1);
        await Assert.That(mapped2.IsT2).IsTrue();
        await Assert.That(mapped2.AsT2).IsEqualTo(3);
        OneOf<string, int, bool, double, long> map3 = true;
        var mapped3 = map3.MapThird(v => !v);
        await Assert.That(mapped3.IsT3).IsTrue();
        await Assert.That(mapped3.AsT3).IsFalse();
        OneOf<string, int, bool, double, long> map4 = 4.0d;
        var mapped4 = map4.MapFourth(v => v + 1d);
        await Assert.That(mapped4.IsT4).IsTrue();
        await Assert.That(mapped4.AsT4).IsEqualTo(5.0d);
        OneOf<string, int, bool, double, long> map5 = 5L;
        var mapped5 = map5.MapFifth(v => v + 1L);
        await Assert.That(mapped5.IsT5).IsTrue();
        await Assert.That(mapped5.AsT5).IsEqualTo(6L);
    }

    [Test]
    public async Task Arity5_reduce_methods_execute_active_case()
    {
        OneOf<string, int, bool, double, long> r1 = "a";
        var reduced1 = r1.ReduceFirst(_ => 2);
        await Assert.That(reduced1.IsT1).IsTrue();
        OneOf<string, int, bool, double, long> r2 = 2;
        var reduced2 = r2.ReduceSecond(_ => "a");
        await Assert.That(reduced2.IsT1).IsTrue();
        OneOf<string, int, bool, double, long> r3 = true;
        var reduced3 = r3.ReduceThird(_ => "a");
        await Assert.That(reduced3.IsT1).IsTrue();
        OneOf<string, int, bool, double, long> r4 = 4.0d;
        var reduced4 = r4.ReduceFourth(_ => "a");
        await Assert.That(reduced4.IsT1).IsTrue();
        OneOf<string, int, bool, double, long> r5 = 5L;
        var reduced5 = r5.ReduceFifth(_ => "a");
        await Assert.That(reduced5.IsT1).IsTrue();
    }

    [Test]
    public async Task Arity5_linq_select_and_bind_cover_paths()
    {
        OneOf<string, int, bool, double, long> success = 5L;
        var selected = from x in success
                       select (x + 1L);
        await Assert.That(selected.IsT5).IsTrue();
        await Assert.That(selected.AsT5).IsEqualTo(6L);

        var bound = from x in success
                    from y in (OneOf<string, int, bool, double, long>)(5L)
                    select x;
        await Assert.That(bound.IsT5).IsTrue();

        OneOf<string, int, bool, double, long> fail = "a";
        var shortCircuit = from x in fail
                           from y in (OneOf<string, int, bool, double, long>)(5L)
                           select x;
        await Assert.That(shortCircuit.IsT1).IsTrue();
    }

    [Test]
    public async Task Arity5_wrong_accessor_throws()
    {
        OneOf<string, int, bool, double, long> value = 2;
        await Assert.That(() => value.AsT1).Throws<InvalidOperationException>();
    }

    [Test]
    public async Task Arity6_factory_methods_create_all_cases()
    {
        var v1 = OneOf.First<string, int, bool, double, long, decimal>("a");
        await Assert.That(v1.IsT1).IsTrue();
        var v2 = OneOf.Second<string, int, bool, double, long, decimal>(2);
        await Assert.That(v2.IsT2).IsTrue();
        var v3 = OneOf.Third<string, int, bool, double, long, decimal>(true);
        await Assert.That(v3.IsT3).IsTrue();
        var v4 = OneOf.Fourth<string, int, bool, double, long, decimal>(4.0d);
        await Assert.That(v4.IsT4).IsTrue();
        var v5 = OneOf.Fifth<string, int, bool, double, long, decimal>(5L);
        await Assert.That(v5.IsT5).IsTrue();
        var v6 = OneOf.Sixth<string, int, bool, double, long, decimal>(6m);
        await Assert.That(v6.IsT6).IsTrue();
    }

    [Test]
    public async Task Arity6_match_and_accessors_cover_all_cases()
    {
        OneOf<string, int, bool, double, long, decimal> c1 = "a";
        await Assert.That(c1.IsT1).IsTrue();
        await Assert.That(c1.AsT1).IsEqualTo("a");
        await Assert.That(c1.Match(t1 => 1, t2 => 2, t3 => 3, t4 => 4, t5 => 5, t6 => 6)).IsEqualTo(1);
        await Assert.That(await c1.MatchAsync(t1 => Task.FromResult(1), t2 => Task.FromResult(2), t3 => Task.FromResult(3), t4 => Task.FromResult(4), t5 => Task.FromResult(5), t6 => Task.FromResult(6))).IsEqualTo(1);
        OneOf<string, int, bool, double, long, decimal> c2 = 2;
        await Assert.That(c2.IsT2).IsTrue();
        await Assert.That(c2.AsT2).IsEqualTo(2);
        await Assert.That(c2.Match(t1 => 1, t2 => 2, t3 => 3, t4 => 4, t5 => 5, t6 => 6)).IsEqualTo(2);
        await Assert.That(await c2.MatchAsync(t1 => Task.FromResult(1), t2 => Task.FromResult(2), t3 => Task.FromResult(3), t4 => Task.FromResult(4), t5 => Task.FromResult(5), t6 => Task.FromResult(6))).IsEqualTo(2);
        OneOf<string, int, bool, double, long, decimal> c3 = true;
        await Assert.That(c3.IsT3).IsTrue();
        await Assert.That(c3.AsT3).IsTrue();
        await Assert.That(c3.Match(t1 => 1, t2 => 2, t3 => 3, t4 => 4, t5 => 5, t6 => 6)).IsEqualTo(3);
        await Assert.That(await c3.MatchAsync(t1 => Task.FromResult(1), t2 => Task.FromResult(2), t3 => Task.FromResult(3), t4 => Task.FromResult(4), t5 => Task.FromResult(5), t6 => Task.FromResult(6))).IsEqualTo(3);
        OneOf<string, int, bool, double, long, decimal> c4 = 4.0d;
        await Assert.That(c4.IsT4).IsTrue();
        await Assert.That(c4.AsT4).IsEqualTo(4.0d);
        await Assert.That(c4.Match(t1 => 1, t2 => 2, t3 => 3, t4 => 4, t5 => 5, t6 => 6)).IsEqualTo(4);
        await Assert.That(await c4.MatchAsync(t1 => Task.FromResult(1), t2 => Task.FromResult(2), t3 => Task.FromResult(3), t4 => Task.FromResult(4), t5 => Task.FromResult(5), t6 => Task.FromResult(6))).IsEqualTo(4);
        OneOf<string, int, bool, double, long, decimal> c5 = 5L;
        await Assert.That(c5.IsT5).IsTrue();
        await Assert.That(c5.AsT5).IsEqualTo(5L);
        await Assert.That(c5.Match(t1 => 1, t2 => 2, t3 => 3, t4 => 4, t5 => 5, t6 => 6)).IsEqualTo(5);
        await Assert.That(await c5.MatchAsync(t1 => Task.FromResult(1), t2 => Task.FromResult(2), t3 => Task.FromResult(3), t4 => Task.FromResult(4), t5 => Task.FromResult(5), t6 => Task.FromResult(6))).IsEqualTo(5);
        OneOf<string, int, bool, double, long, decimal> c6 = 6m;
        await Assert.That(c6.IsT6).IsTrue();
        await Assert.That(c6.AsT6).IsEqualTo(6m);
        await Assert.That(c6.Match(t1 => 1, t2 => 2, t3 => 3, t4 => 4, t5 => 5, t6 => 6)).IsEqualTo(6);
        await Assert.That(await c6.MatchAsync(t1 => Task.FromResult(1), t2 => Task.FromResult(2), t3 => Task.FromResult(3), t4 => Task.FromResult(4), t5 => Task.FromResult(5), t6 => Task.FromResult(6))).IsEqualTo(6);
    }

    [Test]
    public async Task Arity6_map_methods_execute_active_case()
    {
        OneOf<string, int, bool, double, long, decimal> map1 = "a";
        var mapped1 = map1.MapFirst(v => v + "!");
        await Assert.That(mapped1.IsT1).IsTrue();
        await Assert.That(mapped1.AsT1).IsEqualTo("a!");
        OneOf<string, int, bool, double, long, decimal> map2 = 2;
        var mapped2 = map2.MapSecond(v => v + 1);
        await Assert.That(mapped2.IsT2).IsTrue();
        await Assert.That(mapped2.AsT2).IsEqualTo(3);
        OneOf<string, int, bool, double, long, decimal> map3 = true;
        var mapped3 = map3.MapThird(v => !v);
        await Assert.That(mapped3.IsT3).IsTrue();
        await Assert.That(mapped3.AsT3).IsFalse();
        OneOf<string, int, bool, double, long, decimal> map4 = 4.0d;
        var mapped4 = map4.MapFourth(v => v + 1d);
        await Assert.That(mapped4.IsT4).IsTrue();
        await Assert.That(mapped4.AsT4).IsEqualTo(5.0d);
        OneOf<string, int, bool, double, long, decimal> map5 = 5L;
        var mapped5 = map5.MapFifth(v => v + 1L);
        await Assert.That(mapped5.IsT5).IsTrue();
        await Assert.That(mapped5.AsT5).IsEqualTo(6L);
        OneOf<string, int, bool, double, long, decimal> map6 = 6m;
        var mapped6 = map6.MapSixth(v => v + 1m);
        await Assert.That(mapped6.IsT6).IsTrue();
        await Assert.That(mapped6.AsT6).IsEqualTo(7m);
    }

    [Test]
    public async Task Arity6_reduce_methods_execute_active_case()
    {
        OneOf<string, int, bool, double, long, decimal> r1 = "a";
        var reduced1 = r1.ReduceFirst(_ => 2);
        await Assert.That(reduced1.IsT1).IsTrue();
        OneOf<string, int, bool, double, long, decimal> r2 = 2;
        var reduced2 = r2.ReduceSecond(_ => "a");
        await Assert.That(reduced2.IsT1).IsTrue();
        OneOf<string, int, bool, double, long, decimal> r3 = true;
        var reduced3 = r3.ReduceThird(_ => "a");
        await Assert.That(reduced3.IsT1).IsTrue();
        OneOf<string, int, bool, double, long, decimal> r4 = 4.0d;
        var reduced4 = r4.ReduceFourth(_ => "a");
        await Assert.That(reduced4.IsT1).IsTrue();
        OneOf<string, int, bool, double, long, decimal> r5 = 5L;
        var reduced5 = r5.ReduceFifth(_ => "a");
        await Assert.That(reduced5.IsT1).IsTrue();
        OneOf<string, int, bool, double, long, decimal> r6 = 6m;
        var reduced6 = r6.ReduceSixth(_ => "a");
        await Assert.That(reduced6.IsT1).IsTrue();
    }

    [Test]
    public async Task Arity6_linq_select_and_bind_cover_paths()
    {
        OneOf<string, int, bool, double, long, decimal> success = 6m;
        var selected = from x in success
                       select (x + 1m);
        await Assert.That(selected.IsT6).IsTrue();
        await Assert.That(selected.AsT6).IsEqualTo(7m);

        var bound = from x in success
                    from y in (OneOf<string, int, bool, double, long, decimal>)(6m)
                    select x;
        await Assert.That(bound.IsT6).IsTrue();

        OneOf<string, int, bool, double, long, decimal> fail = "a";
        var shortCircuit = from x in fail
                           from y in (OneOf<string, int, bool, double, long, decimal>)(6m)
                           select x;
        await Assert.That(shortCircuit.IsT1).IsTrue();
    }

    [Test]
    public async Task Arity6_wrong_accessor_throws()
    {
        OneOf<string, int, bool, double, long, decimal> value = 2;
        await Assert.That(() => value.AsT1).Throws<InvalidOperationException>();
    }

    [Test]
    public async Task Arity7_factory_methods_create_all_cases()
    {
        var v1 = OneOf.First<string, int, bool, double, long, decimal, char>("a");
        await Assert.That(v1.IsT1).IsTrue();
        var v2 = OneOf.Second<string, int, bool, double, long, decimal, char>(2);
        await Assert.That(v2.IsT2).IsTrue();
        var v3 = OneOf.Third<string, int, bool, double, long, decimal, char>(true);
        await Assert.That(v3.IsT3).IsTrue();
        var v4 = OneOf.Fourth<string, int, bool, double, long, decimal, char>(4.0d);
        await Assert.That(v4.IsT4).IsTrue();
        var v5 = OneOf.Fifth<string, int, bool, double, long, decimal, char>(5L);
        await Assert.That(v5.IsT5).IsTrue();
        var v6 = OneOf.Sixth<string, int, bool, double, long, decimal, char>(6m);
        await Assert.That(v6.IsT6).IsTrue();
        var v7 = OneOf.Seventh<string, int, bool, double, long, decimal, char>('x');
        await Assert.That(v7.IsT7).IsTrue();
    }

    [Test]
    public async Task Arity7_match_and_accessors_cover_all_cases()
    {
        OneOf<string, int, bool, double, long, decimal, char> c1 = "a";
        await Assert.That(c1.IsT1).IsTrue();
        await Assert.That(c1.AsT1).IsEqualTo("a");
        await Assert.That(c1.Match(t1 => 1, t2 => 2, t3 => 3, t4 => 4, t5 => 5, t6 => 6, t7 => 7)).IsEqualTo(1);
        await Assert.That(await c1.MatchAsync(t1 => Task.FromResult(1), t2 => Task.FromResult(2), t3 => Task.FromResult(3), t4 => Task.FromResult(4), t5 => Task.FromResult(5), t6 => Task.FromResult(6), t7 => Task.FromResult(7))).IsEqualTo(1);
        OneOf<string, int, bool, double, long, decimal, char> c2 = 2;
        await Assert.That(c2.IsT2).IsTrue();
        await Assert.That(c2.AsT2).IsEqualTo(2);
        await Assert.That(c2.Match(t1 => 1, t2 => 2, t3 => 3, t4 => 4, t5 => 5, t6 => 6, t7 => 7)).IsEqualTo(2);
        await Assert.That(await c2.MatchAsync(t1 => Task.FromResult(1), t2 => Task.FromResult(2), t3 => Task.FromResult(3), t4 => Task.FromResult(4), t5 => Task.FromResult(5), t6 => Task.FromResult(6), t7 => Task.FromResult(7))).IsEqualTo(2);
        OneOf<string, int, bool, double, long, decimal, char> c3 = true;
        await Assert.That(c3.IsT3).IsTrue();
        await Assert.That(c3.AsT3).IsTrue();
        await Assert.That(c3.Match(t1 => 1, t2 => 2, t3 => 3, t4 => 4, t5 => 5, t6 => 6, t7 => 7)).IsEqualTo(3);
        await Assert.That(await c3.MatchAsync(t1 => Task.FromResult(1), t2 => Task.FromResult(2), t3 => Task.FromResult(3), t4 => Task.FromResult(4), t5 => Task.FromResult(5), t6 => Task.FromResult(6), t7 => Task.FromResult(7))).IsEqualTo(3);
        OneOf<string, int, bool, double, long, decimal, char> c4 = 4.0d;
        await Assert.That(c4.IsT4).IsTrue();
        await Assert.That(c4.AsT4).IsEqualTo(4.0d);
        await Assert.That(c4.Match(t1 => 1, t2 => 2, t3 => 3, t4 => 4, t5 => 5, t6 => 6, t7 => 7)).IsEqualTo(4);
        await Assert.That(await c4.MatchAsync(t1 => Task.FromResult(1), t2 => Task.FromResult(2), t3 => Task.FromResult(3), t4 => Task.FromResult(4), t5 => Task.FromResult(5), t6 => Task.FromResult(6), t7 => Task.FromResult(7))).IsEqualTo(4);
        OneOf<string, int, bool, double, long, decimal, char> c5 = 5L;
        await Assert.That(c5.IsT5).IsTrue();
        await Assert.That(c5.AsT5).IsEqualTo(5L);
        await Assert.That(c5.Match(t1 => 1, t2 => 2, t3 => 3, t4 => 4, t5 => 5, t6 => 6, t7 => 7)).IsEqualTo(5);
        await Assert.That(await c5.MatchAsync(t1 => Task.FromResult(1), t2 => Task.FromResult(2), t3 => Task.FromResult(3), t4 => Task.FromResult(4), t5 => Task.FromResult(5), t6 => Task.FromResult(6), t7 => Task.FromResult(7))).IsEqualTo(5);
        OneOf<string, int, bool, double, long, decimal, char> c6 = 6m;
        await Assert.That(c6.IsT6).IsTrue();
        await Assert.That(c6.AsT6).IsEqualTo(6m);
        await Assert.That(c6.Match(t1 => 1, t2 => 2, t3 => 3, t4 => 4, t5 => 5, t6 => 6, t7 => 7)).IsEqualTo(6);
        await Assert.That(await c6.MatchAsync(t1 => Task.FromResult(1), t2 => Task.FromResult(2), t3 => Task.FromResult(3), t4 => Task.FromResult(4), t5 => Task.FromResult(5), t6 => Task.FromResult(6), t7 => Task.FromResult(7))).IsEqualTo(6);
        OneOf<string, int, bool, double, long, decimal, char> c7 = 'x';
        await Assert.That(c7.IsT7).IsTrue();
        await Assert.That(c7.AsT7).IsEqualTo('x');
        await Assert.That(c7.Match(t1 => 1, t2 => 2, t3 => 3, t4 => 4, t5 => 5, t6 => 6, t7 => 7)).IsEqualTo(7);
        await Assert.That(await c7.MatchAsync(t1 => Task.FromResult(1), t2 => Task.FromResult(2), t3 => Task.FromResult(3), t4 => Task.FromResult(4), t5 => Task.FromResult(5), t6 => Task.FromResult(6), t7 => Task.FromResult(7))).IsEqualTo(7);
    }

    [Test]
    public async Task Arity7_map_methods_execute_active_case()
    {
        OneOf<string, int, bool, double, long, decimal, char> map1 = "a";
        var mapped1 = map1.MapFirst(v => v + "!");
        await Assert.That(mapped1.IsT1).IsTrue();
        await Assert.That(mapped1.AsT1).IsEqualTo("a!");
        OneOf<string, int, bool, double, long, decimal, char> map2 = 2;
        var mapped2 = map2.MapSecond(v => v + 1);
        await Assert.That(mapped2.IsT2).IsTrue();
        await Assert.That(mapped2.AsT2).IsEqualTo(3);
        OneOf<string, int, bool, double, long, decimal, char> map3 = true;
        var mapped3 = map3.MapThird(v => !v);
        await Assert.That(mapped3.IsT3).IsTrue();
        await Assert.That(mapped3.AsT3).IsFalse();
        OneOf<string, int, bool, double, long, decimal, char> map4 = 4.0d;
        var mapped4 = map4.MapFourth(v => v + 1d);
        await Assert.That(mapped4.IsT4).IsTrue();
        await Assert.That(mapped4.AsT4).IsEqualTo(5.0d);
        OneOf<string, int, bool, double, long, decimal, char> map5 = 5L;
        var mapped5 = map5.MapFifth(v => v + 1L);
        await Assert.That(mapped5.IsT5).IsTrue();
        await Assert.That(mapped5.AsT5).IsEqualTo(6L);
        OneOf<string, int, bool, double, long, decimal, char> map6 = 6m;
        var mapped6 = map6.MapSixth(v => v + 1m);
        await Assert.That(mapped6.IsT6).IsTrue();
        await Assert.That(mapped6.AsT6).IsEqualTo(7m);
        OneOf<string, int, bool, double, long, decimal, char> map7 = 'x';
        var mapped7 = map7.MapSeventh(v => (char)(v + 1));
        await Assert.That(mapped7.IsT7).IsTrue();
        await Assert.That(mapped7.AsT7).IsEqualTo('y');
    }

    [Test]
    public async Task Arity7_reduce_methods_execute_active_case()
    {
        OneOf<string, int, bool, double, long, decimal, char> r1 = "a";
        var reduced1 = r1.ReduceFirst(_ => 2);
        await Assert.That(reduced1.IsT1).IsTrue();
        OneOf<string, int, bool, double, long, decimal, char> r2 = 2;
        var reduced2 = r2.ReduceSecond(_ => "a");
        await Assert.That(reduced2.IsT1).IsTrue();
        OneOf<string, int, bool, double, long, decimal, char> r3 = true;
        var reduced3 = r3.ReduceThird(_ => "a");
        await Assert.That(reduced3.IsT1).IsTrue();
        OneOf<string, int, bool, double, long, decimal, char> r4 = 4.0d;
        var reduced4 = r4.ReduceFourth(_ => "a");
        await Assert.That(reduced4.IsT1).IsTrue();
        OneOf<string, int, bool, double, long, decimal, char> r5 = 5L;
        var reduced5 = r5.ReduceFifth(_ => "a");
        await Assert.That(reduced5.IsT1).IsTrue();
        OneOf<string, int, bool, double, long, decimal, char> r6 = 6m;
        var reduced6 = r6.ReduceSixth(_ => "a");
        await Assert.That(reduced6.IsT1).IsTrue();
        OneOf<string, int, bool, double, long, decimal, char> r7 = 'x';
        var reduced7 = r7.ReduceSeventh(_ => "a");
        await Assert.That(reduced7.IsT1).IsTrue();
    }

    [Test]
    public async Task Arity7_linq_select_and_bind_cover_paths()
    {
        OneOf<string, int, bool, double, long, decimal, char> success = 'x';
        var selected = from x in success
                       select ((char)(x + 1));
        await Assert.That(selected.IsT7).IsTrue();
        await Assert.That(selected.AsT7).IsEqualTo('y');

        var bound = from x in success
                    from y in (OneOf<string, int, bool, double, long, decimal, char>)('x')
                    select x;
        await Assert.That(bound.IsT7).IsTrue();

        OneOf<string, int, bool, double, long, decimal, char> fail = "a";
        var shortCircuit = from x in fail
                           from y in (OneOf<string, int, bool, double, long, decimal, char>)('x')
                           select x;
        await Assert.That(shortCircuit.IsT1).IsTrue();
    }

    [Test]
    public async Task Arity7_wrong_accessor_throws()
    {
        OneOf<string, int, bool, double, long, decimal, char> value = 2;
        await Assert.That(() => value.AsT1).Throws<InvalidOperationException>();
    }

    [Test]
    public async Task Arity8_factory_methods_create_all_cases()
    {
        var v1 = OneOf.First<string, int, bool, double, long, decimal, char, byte>("a");
        await Assert.That(v1.IsT1).IsTrue();
        var v2 = OneOf.Second<string, int, bool, double, long, decimal, char, byte>(2);
        await Assert.That(v2.IsT2).IsTrue();
        var v3 = OneOf.Third<string, int, bool, double, long, decimal, char, byte>(true);
        await Assert.That(v3.IsT3).IsTrue();
        var v4 = OneOf.Fourth<string, int, bool, double, long, decimal, char, byte>(4.0d);
        await Assert.That(v4.IsT4).IsTrue();
        var v5 = OneOf.Fifth<string, int, bool, double, long, decimal, char, byte>(5L);
        await Assert.That(v5.IsT5).IsTrue();
        var v6 = OneOf.Sixth<string, int, bool, double, long, decimal, char, byte>(6m);
        await Assert.That(v6.IsT6).IsTrue();
        var v7 = OneOf.Seventh<string, int, bool, double, long, decimal, char, byte>('x');
        await Assert.That(v7.IsT7).IsTrue();
        var v8 = OneOf.Eighth<string, int, bool, double, long, decimal, char, byte>((byte)8);
        await Assert.That(v8.IsT8).IsTrue();
    }

    [Test]
    public async Task Arity8_match_and_accessors_cover_all_cases()
    {
        OneOf<string, int, bool, double, long, decimal, char, byte> c1 = "a";
        await Assert.That(c1.IsT1).IsTrue();
        await Assert.That(c1.AsT1).IsEqualTo("a");
        await Assert.That(c1.Match(t1 => 1, t2 => 2, t3 => 3, t4 => 4, t5 => 5, t6 => 6, t7 => 7, t8 => 8)).IsEqualTo(1);
        await Assert.That(await c1.MatchAsync(t1 => Task.FromResult(1), t2 => Task.FromResult(2), t3 => Task.FromResult(3), t4 => Task.FromResult(4), t5 => Task.FromResult(5), t6 => Task.FromResult(6), t7 => Task.FromResult(7), t8 => Task.FromResult(8))).IsEqualTo(1);
        OneOf<string, int, bool, double, long, decimal, char, byte> c2 = 2;
        await Assert.That(c2.IsT2).IsTrue();
        await Assert.That(c2.AsT2).IsEqualTo(2);
        await Assert.That(c2.Match(t1 => 1, t2 => 2, t3 => 3, t4 => 4, t5 => 5, t6 => 6, t7 => 7, t8 => 8)).IsEqualTo(2);
        await Assert.That(await c2.MatchAsync(t1 => Task.FromResult(1), t2 => Task.FromResult(2), t3 => Task.FromResult(3), t4 => Task.FromResult(4), t5 => Task.FromResult(5), t6 => Task.FromResult(6), t7 => Task.FromResult(7), t8 => Task.FromResult(8))).IsEqualTo(2);
        OneOf<string, int, bool, double, long, decimal, char, byte> c3 = true;
        await Assert.That(c3.IsT3).IsTrue();
        await Assert.That(c3.AsT3).IsTrue();
        await Assert.That(c3.Match(t1 => 1, t2 => 2, t3 => 3, t4 => 4, t5 => 5, t6 => 6, t7 => 7, t8 => 8)).IsEqualTo(3);
        await Assert.That(await c3.MatchAsync(t1 => Task.FromResult(1), t2 => Task.FromResult(2), t3 => Task.FromResult(3), t4 => Task.FromResult(4), t5 => Task.FromResult(5), t6 => Task.FromResult(6), t7 => Task.FromResult(7), t8 => Task.FromResult(8))).IsEqualTo(3);
        OneOf<string, int, bool, double, long, decimal, char, byte> c4 = 4.0d;
        await Assert.That(c4.IsT4).IsTrue();
        await Assert.That(c4.AsT4).IsEqualTo(4.0d);
        await Assert.That(c4.Match(t1 => 1, t2 => 2, t3 => 3, t4 => 4, t5 => 5, t6 => 6, t7 => 7, t8 => 8)).IsEqualTo(4);
        await Assert.That(await c4.MatchAsync(t1 => Task.FromResult(1), t2 => Task.FromResult(2), t3 => Task.FromResult(3), t4 => Task.FromResult(4), t5 => Task.FromResult(5), t6 => Task.FromResult(6), t7 => Task.FromResult(7), t8 => Task.FromResult(8))).IsEqualTo(4);
        OneOf<string, int, bool, double, long, decimal, char, byte> c5 = 5L;
        await Assert.That(c5.IsT5).IsTrue();
        await Assert.That(c5.AsT5).IsEqualTo(5L);
        await Assert.That(c5.Match(t1 => 1, t2 => 2, t3 => 3, t4 => 4, t5 => 5, t6 => 6, t7 => 7, t8 => 8)).IsEqualTo(5);
        await Assert.That(await c5.MatchAsync(t1 => Task.FromResult(1), t2 => Task.FromResult(2), t3 => Task.FromResult(3), t4 => Task.FromResult(4), t5 => Task.FromResult(5), t6 => Task.FromResult(6), t7 => Task.FromResult(7), t8 => Task.FromResult(8))).IsEqualTo(5);
        OneOf<string, int, bool, double, long, decimal, char, byte> c6 = 6m;
        await Assert.That(c6.IsT6).IsTrue();
        await Assert.That(c6.AsT6).IsEqualTo(6m);
        await Assert.That(c6.Match(t1 => 1, t2 => 2, t3 => 3, t4 => 4, t5 => 5, t6 => 6, t7 => 7, t8 => 8)).IsEqualTo(6);
        await Assert.That(await c6.MatchAsync(t1 => Task.FromResult(1), t2 => Task.FromResult(2), t3 => Task.FromResult(3), t4 => Task.FromResult(4), t5 => Task.FromResult(5), t6 => Task.FromResult(6), t7 => Task.FromResult(7), t8 => Task.FromResult(8))).IsEqualTo(6);
        OneOf<string, int, bool, double, long, decimal, char, byte> c7 = 'x';
        await Assert.That(c7.IsT7).IsTrue();
        await Assert.That(c7.AsT7).IsEqualTo('x');
        await Assert.That(c7.Match(t1 => 1, t2 => 2, t3 => 3, t4 => 4, t5 => 5, t6 => 6, t7 => 7, t8 => 8)).IsEqualTo(7);
        await Assert.That(await c7.MatchAsync(t1 => Task.FromResult(1), t2 => Task.FromResult(2), t3 => Task.FromResult(3), t4 => Task.FromResult(4), t5 => Task.FromResult(5), t6 => Task.FromResult(6), t7 => Task.FromResult(7), t8 => Task.FromResult(8))).IsEqualTo(7);
        OneOf<string, int, bool, double, long, decimal, char, byte> c8 = (byte)8;
        await Assert.That(c8.IsT8).IsTrue();
        await Assert.That(c8.AsT8).IsEqualTo((byte)8);
        await Assert.That(c8.Match(t1 => 1, t2 => 2, t3 => 3, t4 => 4, t5 => 5, t6 => 6, t7 => 7, t8 => 8)).IsEqualTo(8);
        await Assert.That(await c8.MatchAsync(t1 => Task.FromResult(1), t2 => Task.FromResult(2), t3 => Task.FromResult(3), t4 => Task.FromResult(4), t5 => Task.FromResult(5), t6 => Task.FromResult(6), t7 => Task.FromResult(7), t8 => Task.FromResult(8))).IsEqualTo(8);
    }

    [Test]
    public async Task Arity8_map_methods_execute_active_case()
    {
        OneOf<string, int, bool, double, long, decimal, char, byte> map1 = "a";
        var mapped1 = map1.MapFirst(v => v + "!");
        await Assert.That(mapped1.IsT1).IsTrue();
        await Assert.That(mapped1.AsT1).IsEqualTo("a!");
        OneOf<string, int, bool, double, long, decimal, char, byte> map2 = 2;
        var mapped2 = map2.MapSecond(v => v + 1);
        await Assert.That(mapped2.IsT2).IsTrue();
        await Assert.That(mapped2.AsT2).IsEqualTo(3);
        OneOf<string, int, bool, double, long, decimal, char, byte> map3 = true;
        var mapped3 = map3.MapThird(v => !v);
        await Assert.That(mapped3.IsT3).IsTrue();
        await Assert.That(mapped3.AsT3).IsFalse();
        OneOf<string, int, bool, double, long, decimal, char, byte> map4 = 4.0d;
        var mapped4 = map4.MapFourth(v => v + 1d);
        await Assert.That(mapped4.IsT4).IsTrue();
        await Assert.That(mapped4.AsT4).IsEqualTo(5.0d);
        OneOf<string, int, bool, double, long, decimal, char, byte> map5 = 5L;
        var mapped5 = map5.MapFifth(v => v + 1L);
        await Assert.That(mapped5.IsT5).IsTrue();
        await Assert.That(mapped5.AsT5).IsEqualTo(6L);
        OneOf<string, int, bool, double, long, decimal, char, byte> map6 = 6m;
        var mapped6 = map6.MapSixth(v => v + 1m);
        await Assert.That(mapped6.IsT6).IsTrue();
        await Assert.That(mapped6.AsT6).IsEqualTo(7m);
        OneOf<string, int, bool, double, long, decimal, char, byte> map7 = 'x';
        var mapped7 = map7.MapSeventh(v => (char)(v + 1));
        await Assert.That(mapped7.IsT7).IsTrue();
        await Assert.That(mapped7.AsT7).IsEqualTo('y');
        OneOf<string, int, bool, double, long, decimal, char, byte> map8 = (byte)8;
        var mapped8 = map8.MapEighth(v => (byte)(v + 1));
        await Assert.That(mapped8.IsT8).IsTrue();
        await Assert.That(mapped8.AsT8).IsEqualTo((byte)9);
    }

    [Test]
    public async Task Arity8_reduce_methods_execute_active_case()
    {
        OneOf<string, int, bool, double, long, decimal, char, byte> r1 = "a";
        var reduced1 = r1.ReduceFirst(_ => 2);
        await Assert.That(reduced1.IsT1).IsTrue();
        OneOf<string, int, bool, double, long, decimal, char, byte> r2 = 2;
        var reduced2 = r2.ReduceSecond(_ => "a");
        await Assert.That(reduced2.IsT1).IsTrue();
        OneOf<string, int, bool, double, long, decimal, char, byte> r3 = true;
        var reduced3 = r3.ReduceThird(_ => "a");
        await Assert.That(reduced3.IsT1).IsTrue();
        OneOf<string, int, bool, double, long, decimal, char, byte> r4 = 4.0d;
        var reduced4 = r4.ReduceFourth(_ => "a");
        await Assert.That(reduced4.IsT1).IsTrue();
        OneOf<string, int, bool, double, long, decimal, char, byte> r5 = 5L;
        var reduced5 = r5.ReduceFifth(_ => "a");
        await Assert.That(reduced5.IsT1).IsTrue();
        OneOf<string, int, bool, double, long, decimal, char, byte> r6 = 6m;
        var reduced6 = r6.ReduceSixth(_ => "a");
        await Assert.That(reduced6.IsT1).IsTrue();
        OneOf<string, int, bool, double, long, decimal, char, byte> r7 = 'x';
        var reduced7 = r7.ReduceSeventh(_ => "a");
        await Assert.That(reduced7.IsT1).IsTrue();
        OneOf<string, int, bool, double, long, decimal, char, byte> r8 = (byte)8;
        var reduced8 = r8.ReduceEighth(_ => "a");
        await Assert.That(reduced8.IsT1).IsTrue();
    }

    [Test]
    public async Task Arity8_linq_select_and_bind_cover_paths()
    {
        OneOf<string, int, bool, double, long, decimal, char, byte> success = (byte)8;
        var selected = from x in success
                       select ((byte)(x + 1));
        await Assert.That(selected.IsT8).IsTrue();
        await Assert.That(selected.AsT8).IsEqualTo((byte)9);

        var bound = from x in success
                    from y in (OneOf<string, int, bool, double, long, decimal, char, byte>)((byte)8)
                    select x;
        await Assert.That(bound.IsT8).IsTrue();

        OneOf<string, int, bool, double, long, decimal, char, byte> fail = "a";
        var shortCircuit = from x in fail
                           from y in (OneOf<string, int, bool, double, long, decimal, char, byte>)((byte)8)
                           select x;
        await Assert.That(shortCircuit.IsT1).IsTrue();
    }

    [Test]
    public async Task Arity8_wrong_accessor_throws()
    {
        OneOf<string, int, bool, double, long, decimal, char, byte> value = 2;
        await Assert.That(() => value.AsT1).Throws<InvalidOperationException>();
    }

    [Test]
    public async Task Invalid_state_throws_for_all_match_methods()
    {
        var oneOf2 = CreateInvalidState<OneOf<string, int>>("x");
        await Assert.That(() => oneOf2.Match(_ => 1, _ => 2)).Throws<InvalidOperationException>();
        await Assert.That(async () => await oneOf2.MatchAsync(_ => Task.FromResult(1), _ => Task.FromResult(2))).Throws<InvalidOperationException>();

        var oneOf3 = CreateInvalidState<OneOf<string, int, bool>>("x");
        await Assert.That(() => oneOf3.Match(_ => 1, _ => 2, _ => 3)).Throws<InvalidOperationException>();
        await Assert.That(async () => await oneOf3.MatchAsync(_ => Task.FromResult(1), _ => Task.FromResult(2), _ => Task.FromResult(3))).Throws<InvalidOperationException>();

        var oneOf4 = CreateInvalidState<OneOf<string, int, bool, double>>("x");
        await Assert.That(() => oneOf4.Match(_ => 1, _ => 2, _ => 3, _ => 4)).Throws<InvalidOperationException>();
        await Assert.That(async () => await oneOf4.MatchAsync(_ => Task.FromResult(1), _ => Task.FromResult(2), _ => Task.FromResult(3), _ => Task.FromResult(4))).Throws<InvalidOperationException>();

        var oneOf5 = CreateInvalidState<OneOf<string, int, bool, double, long>>("x");
        await Assert.That(() => oneOf5.Match(_ => 1, _ => 2, _ => 3, _ => 4, _ => 5)).Throws<InvalidOperationException>();
        await Assert.That(async () => await oneOf5.MatchAsync(_ => Task.FromResult(1), _ => Task.FromResult(2), _ => Task.FromResult(3), _ => Task.FromResult(4), _ => Task.FromResult(5))).Throws<InvalidOperationException>();

        var oneOf6 = CreateInvalidState<OneOf<string, int, bool, double, long, decimal>>("x");
        await Assert.That(() => oneOf6.Match(_ => 1, _ => 2, _ => 3, _ => 4, _ => 5, _ => 6)).Throws<InvalidOperationException>();
        await Assert.That(async () => await oneOf6.MatchAsync(_ => Task.FromResult(1), _ => Task.FromResult(2), _ => Task.FromResult(3), _ => Task.FromResult(4), _ => Task.FromResult(5), _ => Task.FromResult(6))).Throws<InvalidOperationException>();

        var oneOf7 = CreateInvalidState<OneOf<string, int, bool, double, long, decimal, char>>("x");
        await Assert.That(() => oneOf7.Match(_ => 1, _ => 2, _ => 3, _ => 4, _ => 5, _ => 6, _ => 7)).Throws<InvalidOperationException>();
        await Assert.That(async () => await oneOf7.MatchAsync(_ => Task.FromResult(1), _ => Task.FromResult(2), _ => Task.FromResult(3), _ => Task.FromResult(4), _ => Task.FromResult(5), _ => Task.FromResult(6), _ => Task.FromResult(7))).Throws<InvalidOperationException>();

        var oneOf8 = CreateInvalidState<OneOf<string, int, bool, double, long, decimal, char, byte>>("x");
        await Assert.That(() => oneOf8.Match(_ => 1, _ => 2, _ => 3, _ => 4, _ => 5, _ => 6, _ => 7, _ => 8)).Throws<InvalidOperationException>();
        await Assert.That(async () => await oneOf8.MatchAsync(_ => Task.FromResult(1), _ => Task.FromResult(2), _ => Task.FromResult(3), _ => Task.FromResult(4), _ => Task.FromResult(5), _ => Task.FromResult(6), _ => Task.FromResult(7), _ => Task.FromResult(8))).Throws<InvalidOperationException>();
    }

    [Test]
    public async Task Accessors_throw_descriptive_errors_for_wrong_case()
    {
        OneOf<string, int> twoCase = 42;
        var exception = await Assert.That(() => twoCase.AsT1).Throws<InvalidOperationException>();
        await Assert.That(exception).IsNotNull();
        await Assert.That(exception!.Message).IsEqualTo("Value is not T1.");

        OneOf<string, int, bool> threeCase = "x";
        var exception2 = await Assert.That(() => threeCase.AsT3).Throws<InvalidOperationException>();
        await Assert.That(exception2).IsNotNull();
        await Assert.That(exception2!.Message).IsEqualTo("Value is not T3.");
    }

}

using DarkPeak.Functional;
using DarkPeak.Functional.Extensions;

namespace DarkPeak.Functional.Tests;

public class EitherExtensionsShould
{
    #region GetLeftOrDefault

    [Test]
    public async Task GetLeftOrDefault_returns_left_value_when_left()
    {
        var either = Either.Left<string, int>("hello");

        var result = either.GetLeftOrDefault("default");

        await Assert.That(result).IsEqualTo("hello");
    }

    [Test]
    public async Task GetLeftOrDefault_returns_default_when_right()
    {
        var either = Either.Right<string, int>(42);

        var result = either.GetLeftOrDefault("default");

        await Assert.That(result).IsEqualTo("default");
    }

    [Test]
    public async Task GetLeftOrDefault_with_factory_returns_left_value_when_left()
    {
        var either = Either.Left<string, int>("hello");

        var result = either.GetLeftOrDefault(() => "default");

        await Assert.That(result).IsEqualTo("hello");
    }

    [Test]
    public async Task GetLeftOrDefault_with_factory_returns_default_when_right()
    {
        var either = Either.Right<string, int>(42);

        var result = either.GetLeftOrDefault(() => "default");

        await Assert.That(result).IsEqualTo("default");
    }

    [Test]
    public async Task GetLeftOrDefault_with_factory_does_not_invoke_factory_when_left()
    {
        var either = Either.Left<string, int>("hello");
        var factoryCalled = false;

        either.GetLeftOrDefault(() =>
        {
            factoryCalled = true;
            return "default";
        });

        await Assert.That(factoryCalled).IsFalse();
    }

    #endregion

    #region GetRightOrDefault

    [Test]
    public async Task GetRightOrDefault_returns_right_value_when_right()
    {
        var either = Either.Right<string, int>(42);

        var result = either.GetRightOrDefault(0);

        await Assert.That(result).IsEqualTo(42);
    }

    [Test]
    public async Task GetRightOrDefault_returns_default_when_left()
    {
        var either = Either.Left<string, int>("hello");

        var result = either.GetRightOrDefault(0);

        await Assert.That(result).IsEqualTo(0);
    }

    [Test]
    public async Task GetRightOrDefault_with_factory_returns_right_value_when_right()
    {
        var either = Either.Right<string, int>(42);

        var result = either.GetRightOrDefault(() => 0);

        await Assert.That(result).IsEqualTo(42);
    }

    [Test]
    public async Task GetRightOrDefault_with_factory_returns_default_when_left()
    {
        var either = Either.Left<string, int>("hello");

        var result = either.GetRightOrDefault(() => 0);

        await Assert.That(result).IsEqualTo(0);
    }

    [Test]
    public async Task GetRightOrDefault_with_factory_does_not_invoke_factory_when_right()
    {
        var either = Either.Right<string, int>(42);
        var factoryCalled = false;

        either.GetRightOrDefault(() =>
        {
            factoryCalled = true;
            return 0;
        });

        await Assert.That(factoryCalled).IsFalse();
    }

    #endregion

    #region Flatten

    [Test]
    public async Task Flatten_unwraps_nested_right()
    {
        var nested = Either.Right<string, Either<string, int>>(Either.Right<string, int>(42));

        var result = nested.Flatten();

        await Assert.That(result.IsRight).IsTrue();
        var value = result.Match(_ => 0, v => v);
        await Assert.That(value).IsEqualTo(42);
    }

    [Test]
    public async Task Flatten_unwraps_inner_left()
    {
        var nested = Either.Right<string, Either<string, int>>(Either.Left<string, int>("inner error"));

        var result = nested.Flatten();

        await Assert.That(result.IsLeft).IsTrue();
        var value = result.Match(v => v, _ => "");
        await Assert.That(value).IsEqualTo("inner error");
    }

    [Test]
    public async Task Flatten_preserves_outer_left()
    {
        var nested = Either.Left<string, Either<string, int>>("outer error");

        var result = nested.Flatten();

        await Assert.That(result.IsLeft).IsTrue();
        var value = result.Match(v => v, _ => "");
        await Assert.That(value).IsEqualTo("outer error");
    }

    #endregion

    #region FlattenLeft

    [Test]
    public async Task FlattenLeft_unwraps_nested_left()
    {
        var nested = Either.Left<Either<string, int>, int>(Either.Left<string, int>("hello"));

        var result = nested.FlattenLeft();

        await Assert.That(result.IsLeft).IsTrue();
        var value = result.Match(v => v, _ => "");
        await Assert.That(value).IsEqualTo("hello");
    }

    [Test]
    public async Task FlattenLeft_unwraps_inner_right()
    {
        var nested = Either.Left<Either<string, int>, int>(Either.Right<string, int>(42));

        var result = nested.FlattenLeft();

        await Assert.That(result.IsRight).IsTrue();
        var value = result.Match(_ => 0, v => v);
        await Assert.That(value).IsEqualTo(42);
    }

    [Test]
    public async Task FlattenLeft_preserves_outer_right()
    {
        var nested = Either.Right<Either<string, int>, int>(99);

        var result = nested.FlattenLeft();

        await Assert.That(result.IsRight).IsTrue();
        var value = result.Match(_ => 0, v => v);
        await Assert.That(value).IsEqualTo(99);
    }

    #endregion

    #region Merge

    [Test]
    public async Task Merge_returns_left_value_when_left()
    {
        var either = Either.Left<string, string>("left");

        var result = either.Merge();

        await Assert.That(result).IsEqualTo("left");
    }

    [Test]
    public async Task Merge_returns_right_value_when_right()
    {
        var either = Either.Right<string, string>("right");

        var result = either.Merge();

        await Assert.That(result).IsEqualTo("right");
    }

    [Test]
    public async Task Merge_works_with_value_types()
    {
        var left = Either.Left<int, int>(1);
        var right = Either.Right<int, int>(2);

        await Assert.That(left.Merge()).IsEqualTo(1);
        await Assert.That(right.Merge()).IsEqualTo(2);
    }

    #endregion

    #region Partition

    [Test]
    public async Task Partition_separates_lefts_and_rights()
    {
        var eithers = new Either<string, int>[]
        {
            Either.Left<string, int>("a"),
            Either.Right<string, int>(1),
            Either.Left<string, int>("b"),
            Either.Right<string, int>(2),
            Either.Right<string, int>(3),
        };

        var (lefts, rights) = eithers.Partition();

        var leftList = lefts.ToList();
        var rightList = rights.ToList();

        await Assert.That(leftList.Count).IsEqualTo(2);
        await Assert.That(leftList[0]).IsEqualTo("a");
        await Assert.That(leftList[1]).IsEqualTo("b");

        await Assert.That(rightList.Count).IsEqualTo(3);
        await Assert.That(rightList[0]).IsEqualTo(1);
        await Assert.That(rightList[1]).IsEqualTo(2);
        await Assert.That(rightList[2]).IsEqualTo(3);
    }

    [Test]
    public async Task Partition_handles_all_lefts()
    {
        var eithers = new Either<string, int>[]
        {
            Either.Left<string, int>("a"),
            Either.Left<string, int>("b"),
        };

        var (lefts, rights) = eithers.Partition();

        await Assert.That(lefts.ToList().Count).IsEqualTo(2);
        await Assert.That(rights.ToList().Count).IsEqualTo(0);
    }

    [Test]
    public async Task Partition_handles_all_rights()
    {
        var eithers = new Either<string, int>[]
        {
            Either.Right<string, int>(1),
            Either.Right<string, int>(2),
        };

        var (lefts, rights) = eithers.Partition();

        await Assert.That(lefts.ToList().Count).IsEqualTo(0);
        await Assert.That(rights.ToList().Count).IsEqualTo(2);
    }

    [Test]
    public async Task Partition_handles_empty_sequence()
    {
        var eithers = Array.Empty<Either<string, int>>();

        var (lefts, rights) = eithers.Partition();

        await Assert.That(lefts.ToList().Count).IsEqualTo(0);
        await Assert.That(rights.ToList().Count).IsEqualTo(0);
    }

    #endregion
}

namespace DarkPeak.Functional.Extensions;

/// <summary>
/// Extension methods for working with Either types.
/// </summary>
public static class EitherExtensions
{
    /// <summary>
    /// Returns the left value if present, otherwise returns the provided default value.
    /// </summary>
    public static TLeft GetLeftOrDefault<TLeft, TRight>(this Either<TLeft, TRight> either, TLeft defaultValue) =>
        either.Match(left: value => value, right: _ => defaultValue);

    /// <summary>
    /// Returns the left value if present, otherwise calls the provided factory function.
    /// </summary>
    public static TLeft GetLeftOrDefault<TLeft, TRight>(this Either<TLeft, TRight> either, Func<TLeft> defaultFactory) =>
        either.Match(left: value => value, right: _ => defaultFactory());

    /// <summary>
    /// Returns the right value if present, otherwise returns the provided default value.
    /// </summary>
    public static TRight GetRightOrDefault<TLeft, TRight>(this Either<TLeft, TRight> either, TRight defaultValue) =>
        either.Match(left: _ => defaultValue, right: value => value);

    /// <summary>
    /// Returns the right value if present, otherwise calls the provided factory function.
    /// </summary>
    public static TRight GetRightOrDefault<TLeft, TRight>(this Either<TLeft, TRight> either, Func<TRight> defaultFactory) =>
        either.Match(left: _ => defaultFactory(), right: value => value);

    /// <summary>
    /// Flattens a nested Either where the right side is itself an Either with the same left type.
    /// </summary>
    public static Either<TLeft, TRight> Flatten<TLeft, TRight>(this Either<TLeft, Either<TLeft, TRight>> either) =>
        either.Match<Either<TLeft, TRight>>(
            left: value => new Left<TLeft, TRight>(value),
            right: inner => inner
        );

    /// <summary>
    /// Flattens a nested Either where the left side is itself an Either with the same right type.
    /// </summary>
    public static Either<TLeft, TRight> FlattenLeft<TLeft, TRight>(this Either<Either<TLeft, TRight>, TRight> either) =>
        either.Match<Either<TLeft, TRight>>(
            left: inner => inner,
            right: value => new Right<TLeft, TRight>(value)
        );

    /// <summary>
    /// Merges an Either where both sides are the same type into a single value.
    /// </summary>
    public static T Merge<T>(this Either<T, T> either) =>
        either.Match(left: value => value, right: value => value);

    /// <summary>
    /// Partitions a sequence of Eithers into left and right collections.
    /// </summary>
    public static (IEnumerable<TLeft> Lefts, IEnumerable<TRight> Rights) Partition<TLeft, TRight>(
        this IEnumerable<Either<TLeft, TRight>> source)
    {
        var lefts = new List<TLeft>();
        var rights = new List<TRight>();

        foreach (var either in source)
        {
            either
                .IfLeft(value => lefts.Add(value))
                .IfRight(value => rights.Add(value));
        }

        return (lefts, rights);
    }
}

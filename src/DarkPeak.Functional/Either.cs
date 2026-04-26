namespace DarkPeak.Functional;

/// <summary>
/// Represents a value that can be one of two types (left or right).
/// Unlike <see cref="Result{T, TError}"/>, both cases represent valid states (not success/failure).
/// Commonly used for branching logic where both paths are equally valid.
/// </summary>
/// <typeparam name="TLeft">The type of the left value.</typeparam>
/// <typeparam name="TRight">The type of the right value.</typeparam>
public union Either<TLeft, TRight>(Left<TLeft, TRight>, Right<TLeft, TRight>);

/// <summary>
/// Represents an either containing a left value.
/// </summary>
/// <typeparam name="TLeft">The type of the left value.</typeparam>
/// <typeparam name="TRight">The type parameter for the right.</typeparam>
public record Left<TLeft, TRight>(TLeft Value);

/// <summary>
/// Represents an either containing a right value.
/// </summary>
/// <typeparam name="TLeft">The type parameter for the left.</typeparam>
/// <typeparam name="TRight">The type of the right value.</typeparam>
public record Right<TLeft, TRight>(TRight Value);

/// <summary>
/// Provides static factory methods and extension methods for <see cref="Either{TLeft, TRight}"/>.
/// </summary>
public static class Either
{
    /// <summary>Creates an either with a left value.</summary>
    public static Either<TLeft, TRight> Left<TLeft, TRight>(TLeft value) => new Left<TLeft, TRight>(value);

    /// <summary>Creates an either with a right value.</summary>
    public static Either<TLeft, TRight> Right<TLeft, TRight>(TRight value) => new Right<TLeft, TRight>(value);

    // ── State checks ──

    /// <summary>Gets a value indicating whether this either contains a left value.</summary>
    public static bool IsLeft<TLeft, TRight>(this Either<TLeft, TRight> either) => either is Left<TLeft, TRight>;

    /// <summary>Gets a value indicating whether this either contains a right value.</summary>
    public static bool IsRight<TLeft, TRight>(this Either<TLeft, TRight> either) => either is Right<TLeft, TRight>;

    // ── Match ──

    /// <summary>
    /// Matches the either to one of two functions based on which value it contains.
    /// </summary>
    /// <typeparam name="TLeft">The left value type.</typeparam>
    /// <typeparam name="TRight">The right value type.</typeparam>
    /// <typeparam name="TResult">The result type.</typeparam>
    /// <param name="either">The either to match.</param>
    /// <param name="left">Function to call if the either contains a left value.</param>
    /// <param name="right">Function to call if the either contains a right value.</param>
    /// <returns>The result of the matching function.</returns>
    public static TResult Match<TLeft, TRight, TResult>(
        this Either<TLeft, TRight> either,
        Func<TLeft, TResult> left,
        Func<TRight, TResult> right) =>
        either switch
        {
            Left<TLeft, TRight> { Value: var l }  => left(l),
            Right<TLeft, TRight> { Value: var r } => right(r)
        };

    /// <summary>
    /// Asynchronously matches the either to one of two functions based on which value it contains.
    /// </summary>
    /// <typeparam name="TLeft">The left value type.</typeparam>
    /// <typeparam name="TRight">The right value type.</typeparam>
    /// <typeparam name="TResult">The result type.</typeparam>
    /// <param name="either">The either to match.</param>
    /// <param name="left">Async function to call if the either contains a left value.</param>
    /// <param name="right">Async function to call if the either contains a right value.</param>
    /// <returns>A task containing the result of the matching function.</returns>
    public static Task<TResult> MatchAsync<TLeft, TRight, TResult>(
        this Either<TLeft, TRight> either,
        Func<TLeft, Task<TResult>> left,
        Func<TRight, Task<TResult>> right) =>
        either switch
        {
            Left<TLeft, TRight> { Value: var l }  => left(l),
            Right<TLeft, TRight> { Value: var r } => right(r)
        };

    // ── MapLeft / MapRight ──

    /// <summary>
    /// Transforms the left value using the provided function.
    /// </summary>
    /// <typeparam name="TLeft">The source left value type.</typeparam>
    /// <typeparam name="TRight">The right value type.</typeparam>
    /// <typeparam name="TLeftResult">The result left value type.</typeparam>
    /// <param name="either">The either to map.</param>
    /// <param name="mapper">Function to transform the left value.</param>
    /// <returns>An either with the transformed left value, or the original right value.</returns>
    public static Either<TLeftResult, TRight> MapLeft<TLeft, TRight, TLeftResult>(
        this Either<TLeft, TRight> either,
        Func<TLeft, TLeftResult> mapper) =>
        either switch
        {
            Left<TLeft, TRight> { Value: var l }  => new Left<TLeftResult, TRight>(mapper(l)),
            Right<TLeft, TRight> { Value: var r } => new Right<TLeftResult, TRight>(r)
        };

    /// <summary>
    /// Transforms the right value using the provided function.
    /// </summary>
    /// <typeparam name="TLeft">The left value type.</typeparam>
    /// <typeparam name="TRight">The source right value type.</typeparam>
    /// <typeparam name="TRightResult">The result right value type.</typeparam>
    /// <param name="either">The either to map.</param>
    /// <param name="mapper">Function to transform the right value.</param>
    /// <returns>An either with the transformed right value, or the original left value.</returns>
    public static Either<TLeft, TRightResult> MapRight<TLeft, TRight, TRightResult>(
        this Either<TLeft, TRight> either,
        Func<TRight, TRightResult> mapper) =>
        either switch
        {
            Left<TLeft, TRight> { Value: var l }  => new Left<TLeft, TRightResult>(l),
            Right<TLeft, TRight> { Value: var r } => new Right<TLeft, TRightResult>(mapper(r))
        };

    /// <summary>
    /// Asynchronously transforms the left value using the provided function.
    /// </summary>
    /// <typeparam name="TLeft">The source left value type.</typeparam>
    /// <typeparam name="TRight">The right value type.</typeparam>
    /// <typeparam name="TLeftResult">The result left value type.</typeparam>
    /// <param name="either">The either to map.</param>
    /// <param name="mapper">Async function to transform the left value.</param>
    /// <returns>A task containing an either with the transformed left value, or the original right value.</returns>
    public static async Task<Either<TLeftResult, TRight>> MapLeftAsync<TLeft, TRight, TLeftResult>(
        this Either<TLeft, TRight> either,
        Func<TLeft, Task<TLeftResult>> mapper) =>
        either switch
        {
            Left<TLeft, TRight> { Value: var l }  => new Left<TLeftResult, TRight>(await mapper(l)),
            Right<TLeft, TRight> { Value: var r } => new Right<TLeftResult, TRight>(r)
        };

    /// <summary>
    /// Asynchronously transforms the right value using the provided function.
    /// </summary>
    /// <typeparam name="TLeft">The left value type.</typeparam>
    /// <typeparam name="TRight">The source right value type.</typeparam>
    /// <typeparam name="TRightResult">The result right value type.</typeparam>
    /// <param name="either">The either to map.</param>
    /// <param name="mapper">Async function to transform the right value.</param>
    /// <returns>A task containing an either with the transformed right value, or the original left value.</returns>
    public static async Task<Either<TLeft, TRightResult>> MapRightAsync<TLeft, TRight, TRightResult>(
        this Either<TLeft, TRight> either,
        Func<TRight, Task<TRightResult>> mapper) =>
        either switch
        {
            Left<TLeft, TRight> { Value: var l }  => new Left<TLeft, TRightResult>(l),
            Right<TLeft, TRight> { Value: var r } => new Right<TLeft, TRightResult>(await mapper(r))
        };

    // ── Map (both sides) ──

    /// <summary>
    /// Transforms both left and right values using the provided functions.
    /// </summary>
    /// <typeparam name="TLeft">The source left value type.</typeparam>
    /// <typeparam name="TRight">The source right value type.</typeparam>
    /// <typeparam name="TLeftResult">The result left value type.</typeparam>
    /// <typeparam name="TRightResult">The result right value type.</typeparam>
    /// <param name="either">The either to map.</param>
    /// <param name="leftMapper">Function to transform the left value.</param>
    /// <param name="rightMapper">Function to transform the right value.</param>
    /// <returns>An either with either the transformed left or right value.</returns>
    public static Either<TLeftResult, TRightResult> Map<TLeft, TRight, TLeftResult, TRightResult>(
        this Either<TLeft, TRight> either,
        Func<TLeft, TLeftResult> leftMapper,
        Func<TRight, TRightResult> rightMapper) =>
        either switch
        {
            Left<TLeft, TRight> { Value: var l }  => new Left<TLeftResult, TRightResult>(leftMapper(l)),
            Right<TLeft, TRight> { Value: var r } => new Right<TLeftResult, TRightResult>(rightMapper(r))
        };

    // ── Bind ──

    /// <summary>
    /// Binds the either to a function that returns another either if it contains a right value.
    /// </summary>
    /// <typeparam name="TLeft">The left value type.</typeparam>
    /// <typeparam name="TRight">The source right value type.</typeparam>
    /// <typeparam name="TRightResult">The result right value type.</typeparam>
    /// <param name="either">The either to bind.</param>
    /// <param name="binder">Function that returns an either.</param>
    /// <returns>The result of the binder function, or the original left value.</returns>
    public static Either<TLeft, TRightResult> Bind<TLeft, TRight, TRightResult>(
        this Either<TLeft, TRight> either,
        Func<TRight, Either<TLeft, TRightResult>> binder) =>
        either switch
        {
            Left<TLeft, TRight> { Value: var l }  => new Left<TLeft, TRightResult>(l),
            Right<TLeft, TRight> { Value: var r } => binder(r)
        };

    /// <summary>
    /// Asynchronously binds the either to a function that returns another either if it contains a right value.
    /// </summary>
    /// <typeparam name="TLeft">The left value type.</typeparam>
    /// <typeparam name="TRight">The source right value type.</typeparam>
    /// <typeparam name="TRightResult">The result right value type.</typeparam>
    /// <param name="either">The either to bind.</param>
    /// <param name="binder">Async function that returns an either.</param>
    /// <returns>A task containing the result of the binder function, or the original left value.</returns>
    public static Task<Either<TLeft, TRightResult>> BindAsync<TLeft, TRight, TRightResult>(
        this Either<TLeft, TRight> either,
        Func<TRight, Task<Either<TLeft, TRightResult>>> binder) =>
        either switch
        {
            Left<TLeft, TRight> { Value: var l }  => Task.FromResult<Either<TLeft, TRightResult>>(new Left<TLeft, TRightResult>(l)),
            Right<TLeft, TRight> { Value: var r } => binder(r)
        };

    // ── Tap ──

    /// <summary>
    /// Executes an action if the either contains a left value.
    /// </summary>
    /// <typeparam name="TLeft">The left value type.</typeparam>
    /// <typeparam name="TRight">The right value type.</typeparam>
    /// <param name="either">The either.</param>
    /// <param name="action">Action to execute with the left value.</param>
    /// <returns>This either for chaining.</returns>
    public static Either<TLeft, TRight> IfLeft<TLeft, TRight>(this Either<TLeft, TRight> either, Action<TLeft> action)
    {
        if (either is Left<TLeft, TRight> { Value: var l }) action(l);
        return either;
    }

    /// <summary>
    /// Executes an action if the either contains a right value.
    /// </summary>
    /// <typeparam name="TLeft">The left value type.</typeparam>
    /// <typeparam name="TRight">The right value type.</typeparam>
    /// <param name="either">The either.</param>
    /// <param name="action">Action to execute with the right value.</param>
    /// <returns>This either for chaining.</returns>
    public static Either<TLeft, TRight> IfRight<TLeft, TRight>(this Either<TLeft, TRight> either, Action<TRight> action)
    {
        if (either is Right<TLeft, TRight> { Value: var r }) action(r);
        return either;
    }

    // ── Swap ──

    /// <summary>
    /// Swaps left and right values.
    /// </summary>
    /// <typeparam name="TLeft">The left value type.</typeparam>
    /// <typeparam name="TRight">The right value type.</typeparam>
    /// <param name="either">The either to swap.</param>
    /// <returns>An either with left and right swapped.</returns>
    public static Either<TRight, TLeft> Swap<TLeft, TRight>(this Either<TLeft, TRight> either) =>
        either switch
        {
            Left<TLeft, TRight> { Value: var l }  => new Right<TRight, TLeft>(l),
            Right<TLeft, TRight> { Value: var r } => new Left<TRight, TLeft>(r)
        };

    // ── LINQ support (on right values, by convention) ──

    /// <summary>Maps the either using a selector (for LINQ support on right values).</summary>
    public static Either<TLeft, TRightResult> Select<TLeft, TRight, TRightResult>(
        this Either<TLeft, TRight> either,
        Func<TRight, TRightResult> selector) =>
        either.MapRight(selector);

    /// <summary>Binds the either using a selector (for LINQ query syntax support on right values).</summary>
    public static Either<TLeft, TRightResult> SelectMany<TLeft, TRight, TRightResult>(
        this Either<TLeft, TRight> either,
        Func<TRight, Either<TLeft, TRightResult>> selector) =>
        either.Bind(selector);

    /// <summary>Binds and projects the either (for LINQ query syntax support on right values).</summary>
    public static Either<TLeft, TRightResult> SelectMany<TLeft, TRight, TRightIntermediate, TRightResult>(
        this Either<TLeft, TRight> either,
        Func<TRight, Either<TLeft, TRightIntermediate>> selector,
        Func<TRight, TRightIntermediate, TRightResult> projector) =>
        either.Bind(right => selector(right).MapRight(intermediate => projector(right, intermediate)));
}

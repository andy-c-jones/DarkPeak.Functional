namespace DarkPeak.Functional;

/// <summary>
/// Represents the result of an operation that can either succeed with a value or fail with an error.
/// Provides a type-safe alternative to throwing exceptions for known error conditions.
/// </summary>
/// <typeparam name="T">The type of the success value.</typeparam>
/// <typeparam name="TError">The type of the error, must inherit from <see cref="Error"/>.</typeparam>
public union Result<T, TError>(Success<T, TError>, Failure<T, TError>) where TError : Error;

/// <summary>
/// Represents a successful result with a value.
/// </summary>
/// <typeparam name="T">The type of the value.</typeparam>
/// <typeparam name="TError">The error type.</typeparam>
public record Success<T, TError>(T Value) where TError : Error;

/// <summary>
/// Represents a failed result with an error.
/// </summary>
/// <typeparam name="T">The type parameter.</typeparam>
/// <typeparam name="TError">The error type.</typeparam>
public record Failure<T, TError>(TError Error) where TError : Error;

/// <summary>
/// Provides static factory methods and extension methods for <see cref="Result{T, TError}"/>.
/// </summary>
public static class Result
{
    /// <summary>Creates a successful result with a value.</summary>
    public static Result<T, TError> Success<T, TError>(T value) where TError : Error =>
        new Success<T, TError>(value);

    /// <summary>Creates a failed result with an error.</summary>
    public static Result<T, TError> Failure<T, TError>(TError error) where TError : Error =>
        new Failure<T, TError>(error);

    // ── State checks ──

    /// <summary>Gets a value indicating whether the result is a success.</summary>
    public static bool IsSuccess<T, TError>(this Result<T, TError> result) where TError : Error =>
        result is Success<T, TError>;

    /// <summary>Gets a value indicating whether the result is a failure.</summary>
    public static bool IsFailure<T, TError>(this Result<T, TError> result) where TError : Error =>
        result is Failure<T, TError>;

    // ── Match ──

    /// <summary>
    /// Matches the result to one of two functions based on success or failure.
    /// </summary>
    /// <typeparam name="T">The success value type.</typeparam>
    /// <typeparam name="TError">The error type.</typeparam>
    /// <typeparam name="TResult">The result type.</typeparam>
    /// <param name="result">The result to match.</param>
    /// <param name="success">Function to call if the result is successful.</param>
    /// <param name="failure">Function to call if the result is a failure.</param>
    /// <returns>The result of the matching function.</returns>
    public static TResult Match<T, TError, TResult>(
        this Result<T, TError> result,
        Func<T, TResult> success,
        Func<TError, TResult> failure)
        where TError : Error =>
        result switch
        {
            Success<T, TError> { Value: var v } => success(v),
            Failure<T, TError> { Error: var e } => failure(e)
        };

    /// <summary>
    /// Asynchronously matches the result to one of two functions based on success or failure.
    /// </summary>
    /// <typeparam name="T">The success value type.</typeparam>
    /// <typeparam name="TError">The error type.</typeparam>
    /// <typeparam name="TResult">The result type.</typeparam>
    /// <param name="result">The result to match.</param>
    /// <param name="success">Async function to call if the result is successful.</param>
    /// <param name="failure">Async function to call if the result is a failure.</param>
    /// <returns>A task containing the result of the matching function.</returns>
    public static Task<TResult> MatchAsync<T, TError, TResult>(
        this Result<T, TError> result,
        Func<T, Task<TResult>> success,
        Func<TError, Task<TResult>> failure)
        where TError : Error =>
        result switch
        {
            Success<T, TError> { Value: var v } => success(v),
            Failure<T, TError> { Error: var e } => failure(e)
        };

    // ── Map ──

    /// <summary>
    /// Transforms the success value using the provided function.
    /// </summary>
    /// <typeparam name="T">The source success type.</typeparam>
    /// <typeparam name="TError">The error type.</typeparam>
    /// <typeparam name="TResult">The result success type.</typeparam>
    /// <param name="result">The result to map.</param>
    /// <param name="mapper">Function to transform the success value.</param>
    /// <returns>A result with the transformed value, or the original failure.</returns>
    public static Result<TResult, TError> Map<T, TError, TResult>(
        this Result<T, TError> result,
        Func<T, TResult> mapper)
        where TError : Error =>
        result switch
        {
            Success<T, TError> { Value: var v } => new Success<TResult, TError>(mapper(v)),
            Failure<T, TError> { Error: var e } => new Failure<TResult, TError>(e)
        };

    /// <summary>
    /// Asynchronously transforms the success value using the provided function.
    /// </summary>
    /// <typeparam name="T">The source success type.</typeparam>
    /// <typeparam name="TError">The error type.</typeparam>
    /// <typeparam name="TResult">The result success type.</typeparam>
    /// <param name="result">The result to map.</param>
    /// <param name="mapper">Async function to transform the success value.</param>
    /// <returns>A task containing a result with the transformed value, or the original failure.</returns>
    public static async Task<Result<TResult, TError>> MapAsync<T, TError, TResult>(
        this Result<T, TError> result,
        Func<T, Task<TResult>> mapper)
        where TError : Error =>
        result switch
        {
            Success<T, TError> { Value: var v } => new Success<TResult, TError>(await mapper(v)),
            Failure<T, TError> { Error: var e } => new Failure<TResult, TError>(e)
        };

    // ── MapError ──

    /// <summary>
    /// Transforms the error value using the provided function.
    /// </summary>
    /// <typeparam name="T">The success type.</typeparam>
    /// <typeparam name="TError">The source error type.</typeparam>
    /// <typeparam name="TErrorResult">The result error type.</typeparam>
    /// <param name="result">The result to map.</param>
    /// <param name="mapper">Function to transform the error value.</param>
    /// <returns>A result with the transformed error, or the original success value.</returns>
    public static Result<T, TErrorResult> MapError<T, TError, TErrorResult>(
        this Result<T, TError> result,
        Func<TError, TErrorResult> mapper)
        where TError : Error where TErrorResult : Error =>
        result switch
        {
            Success<T, TError> { Value: var v } => new Success<T, TErrorResult>(v),
            Failure<T, TError> { Error: var e } => new Failure<T, TErrorResult>(mapper(e))
        };

    // ── Bind ──

    /// <summary>
    /// Binds the result to a function that returns another result (flatMap).
    /// </summary>
    /// <typeparam name="T">The source success type.</typeparam>
    /// <typeparam name="TError">The error type.</typeparam>
    /// <typeparam name="TResult">The result success type.</typeparam>
    /// <param name="result">The result to bind.</param>
    /// <param name="binder">Function that returns a result.</param>
    /// <returns>The result of the binder function, or the original failure.</returns>
    public static Result<TResult, TError> Bind<T, TError, TResult>(
        this Result<T, TError> result,
        Func<T, Result<TResult, TError>> binder)
        where TError : Error =>
        result switch
        {
            Success<T, TError> { Value: var v } => binder(v),
            Failure<T, TError> { Error: var e } => new Failure<TResult, TError>(e)
        };

    /// <summary>
    /// Asynchronously binds the result to a function that returns another result.
    /// </summary>
    /// <typeparam name="T">The source success type.</typeparam>
    /// <typeparam name="TError">The error type.</typeparam>
    /// <typeparam name="TResult">The result success type.</typeparam>
    /// <param name="result">The result to bind.</param>
    /// <param name="binder">Async function that returns a result.</param>
    /// <returns>A task containing the result of the binder function, or the original failure.</returns>
    public static Task<Result<TResult, TError>> BindAsync<T, TError, TResult>(
        this Result<T, TError> result,
        Func<T, Task<Result<TResult, TError>>> binder)
        where TError : Error =>
        result switch
        {
            Success<T, TError> { Value: var v } => binder(v),
            Failure<T, TError> { Error: var e } => Task.FromResult<Result<TResult, TError>>(new Failure<TResult, TError>(e))
        };

    // ── Tap ──

    /// <summary>
    /// Executes an action if the result is successful (side effect).
    /// </summary>
    /// <typeparam name="T">The success type.</typeparam>
    /// <typeparam name="TError">The error type.</typeparam>
    /// <param name="result">The result.</param>
    /// <param name="action">Action to execute with the success value.</param>
    /// <returns>This result for chaining.</returns>
    public static Result<T, TError> Tap<T, TError>(this Result<T, TError> result, Action<T> action)
        where TError : Error
    {
        if (result is Success<T, TError> { Value: var v }) action(v);
        return result;
    }

    /// <summary>
    /// Executes an action if the result is a failure (side effect).
    /// </summary>
    /// <typeparam name="T">The success type.</typeparam>
    /// <typeparam name="TError">The error type.</typeparam>
    /// <param name="result">The result.</param>
    /// <param name="action">Action to execute with the error.</param>
    /// <returns>This result for chaining.</returns>
    public static Result<T, TError> TapError<T, TError>(this Result<T, TError> result, Action<TError> action)
        where TError : Error
    {
        if (result is Failure<T, TError> { Error: var e }) action(e);
        return result;
    }

    // ── Get value ──

    /// <summary>
    /// Returns the success value if present, otherwise returns the provided default value.
    /// </summary>
    /// <typeparam name="T">The success type.</typeparam>
    /// <typeparam name="TError">The error type.</typeparam>
    /// <param name="result">The result.</param>
    /// <param name="defaultValue">The default value to return if the result is a failure.</param>
    /// <returns>The success value or the default value.</returns>
    public static T GetValueOrDefault<T, TError>(this Result<T, TError> result, T defaultValue)
        where TError : Error =>
        result switch
        {
            Success<T, TError> { Value: var v } => v,
            Failure<T, TError>                  => defaultValue
        };

    /// <summary>
    /// Returns the success value if present, otherwise calls the provided factory function.
    /// </summary>
    /// <typeparam name="T">The success type.</typeparam>
    /// <typeparam name="TError">The error type.</typeparam>
    /// <param name="result">The result.</param>
    /// <param name="defaultFactory">Function to produce a default value.</param>
    /// <returns>The success value or the result of the factory function.</returns>
    public static T GetValueOrDefault<T, TError>(this Result<T, TError> result, Func<T> defaultFactory)
        where TError : Error =>
        result switch
        {
            Success<T, TError> { Value: var v } => v,
            Failure<T, TError>                  => defaultFactory()
        };

    /// <summary>
    /// Returns the success value if present, otherwise throws an exception.
    /// Use this as an escape hatch — prefer <see cref="Match{T, TError, TResult}"/> or <see cref="GetValueOrDefault{T, TError}(Result{T, TError}, T)"/>.
    /// </summary>
    /// <typeparam name="T">The success type.</typeparam>
    /// <typeparam name="TError">The error type.</typeparam>
    /// <param name="result">The result.</param>
    /// <returns>The success value.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the result is a failure.</exception>
    public static T GetValueOrThrow<T, TError>(this Result<T, TError> result)
        where TError : Error =>
        result switch
        {
            Success<T, TError> { Value: var v } => v,
            Failure<T, TError> { Error: var e } => throw new InvalidOperationException(
                $"Cannot get value from a failed result. Error: {e.Message}")
        };

    // ── OrElse ──

    /// <summary>
    /// Returns this result if successful, otherwise returns the alternative result.
    /// </summary>
    /// <typeparam name="T">The success type.</typeparam>
    /// <typeparam name="TError">The error type.</typeparam>
    /// <param name="result">The result.</param>
    /// <param name="alternative">The alternative result to return if this is a failure.</param>
    /// <returns>This result or the alternative.</returns>
    public static Result<T, TError> OrElse<T, TError>(this Result<T, TError> result, Result<T, TError> alternative)
        where TError : Error =>
        result switch
        {
            Success<T, TError> => result,
            Failure<T, TError> => alternative
        };

    /// <summary>
    /// Returns this result if successful, otherwise calls the provided factory function.
    /// </summary>
    /// <typeparam name="T">The success type.</typeparam>
    /// <typeparam name="TError">The error type.</typeparam>
    /// <param name="result">The result.</param>
    /// <param name="alternativeFactory">Function to produce an alternative result.</param>
    /// <returns>This result or the result of the factory function.</returns>
    public static Result<T, TError> OrElse<T, TError>(this Result<T, TError> result, Func<Result<T, TError>> alternativeFactory)
        where TError : Error =>
        result switch
        {
            Success<T, TError> => result,
            Failure<T, TError> => alternativeFactory()
        };

    // ── LINQ support ──

    /// <summary>Maps the result using a selector (for LINQ support).</summary>
    public static Result<TResult, TError> Select<T, TError, TResult>(
        this Result<T, TError> result, Func<T, TResult> selector)
        where TError : Error =>
        result.Map(selector);

    /// <summary>Binds the result using a selector (for LINQ query syntax support).</summary>
    public static Result<TResult, TError> SelectMany<T, TError, TResult>(
        this Result<T, TError> result, Func<T, Result<TResult, TError>> selector)
        where TError : Error =>
        result.Bind(selector);

    /// <summary>Binds and projects the result (for LINQ query syntax support).</summary>
    public static Result<TResult, TError> SelectMany<T, TError, TIntermediate, TResult>(
        this Result<T, TError> result,
        Func<T, Result<TIntermediate, TError>> selector,
        Func<T, TIntermediate, TResult> projector)
        where TError : Error =>
        result.Bind(value => selector(value).Map(intermediate => projector(value, intermediate)));
}

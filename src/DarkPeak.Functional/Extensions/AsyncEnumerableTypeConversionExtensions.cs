namespace DarkPeak.Functional.Extensions;

/// <summary>
/// Extension methods for converting between monadic types within async sequences.
/// Provides stream-level type conversions between Option, Result, and Validation.
/// </summary>
public static class AsyncEnumerableTypeConversionExtensions
{
    // ── IAsyncEnumerable<Option<T>> → IAsyncEnumerable<Result<T, TError>> ──

    /// <summary>
    /// Converts each Option in an async sequence to a Result, using the provided error for None values.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <typeparam name="TError">The error type.</typeparam>
    /// <param name="source">The async sequence of options.</param>
    /// <param name="error">The error to use when an option is None.</param>
    /// <returns>An async sequence of results.</returns>
    public static IAsyncEnumerable<Result<T, TError>> ToResultStream<T, TError>(
        this IAsyncEnumerable<Option<T>> source, TError error)
        where TError : Error =>
        source.Select(option => option.ToResult(error));

    /// <summary>
    /// Converts each Option in an async sequence to a Result, using a factory to create the error for None values.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <typeparam name="TError">The error type.</typeparam>
    /// <param name="source">The async sequence of options.</param>
    /// <param name="errorFactory">The factory to create an error when an option is None.</param>
    /// <returns>An async sequence of results.</returns>
    public static IAsyncEnumerable<Result<T, TError>> ToResultStream<T, TError>(
        this IAsyncEnumerable<Option<T>> source, Func<TError> errorFactory)
        where TError : Error =>
        source.Select(option => option.ToResult(errorFactory));

    // ── IAsyncEnumerable<Result<T, TError>> → IAsyncEnumerable<Option<T>> ──

    /// <summary>
    /// Converts each Result in an async sequence to an Option, discarding error information.
    /// Success values become Some, failures become None.
    /// </summary>
    /// <typeparam name="T">The success type.</typeparam>
    /// <typeparam name="TError">The error type.</typeparam>
    /// <param name="source">The async sequence of results.</param>
    /// <returns>An async sequence of options.</returns>
    public static IAsyncEnumerable<Option<T>> ToOptionStream<T, TError>(
        this IAsyncEnumerable<Result<T, TError>> source)
        where TError : Error =>
        source.Select(result => result.AsOption());

    // ── IAsyncEnumerable<Result<T, TError>> → IAsyncEnumerable<Either<TError, T>> ──

    /// <summary>
    /// Converts each Result in an async sequence to an Either.
    /// Success values become Right, failures become Left.
    /// </summary>
    /// <typeparam name="T">The success type.</typeparam>
    /// <typeparam name="TError">The error type.</typeparam>
    /// <param name="source">The async sequence of results.</param>
    /// <returns>An async sequence of either values.</returns>
    public static IAsyncEnumerable<Either<TError, T>> ToEitherStream<T, TError>(
        this IAsyncEnumerable<Result<T, TError>> source)
        where TError : Error =>
        source.Select(result => result.ToEither());

    // ── IAsyncEnumerable<Option<T>> → IAsyncEnumerable<Either<TLeft, T>> ──

    /// <summary>
    /// Converts each Option in an async sequence to an Either.
    /// Some values become Right, None values become Left with the provided value.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <typeparam name="TLeft">The left type for None values.</typeparam>
    /// <param name="source">The async sequence of options.</param>
    /// <param name="leftValue">The left value to use for None options.</param>
    /// <returns>An async sequence of either values.</returns>
    public static IAsyncEnumerable<Either<TLeft, T>> ToEitherStream<TLeft, T>(
        this IAsyncEnumerable<Option<T>> source, TLeft leftValue) =>
        source.Select(option => option.ToEither(leftValue));

    // ── IAsyncEnumerable<Validation<T, TError>> → IAsyncEnumerable<Result<T, TError>> ──

    /// <summary>
    /// Converts each Validation in an async sequence to a Result.
    /// Valid values become Success, Invalid values become Failure (using the first error).
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <typeparam name="TError">The error type.</typeparam>
    /// <param name="source">The async sequence of validations.</param>
    /// <returns>An async sequence of results.</returns>
    public static IAsyncEnumerable<Result<T, TError>> ToResultStream<T, TError>(
        this IAsyncEnumerable<Validation<T, TError>> source)
        where TError : Error =>
        source.Select(validation => validation.Match<Result<T, TError>>(
            valid: value => new Success<T, TError>(value),
            invalid: errors => new Failure<T, TError>(errors[0])));

    // ── IAsyncEnumerable<Result<T, TError>> → IAsyncEnumerable<Validation<T, TError>> ──

    /// <summary>
    /// Converts each Result in an async sequence to a Validation.
    /// Success values become Valid, Failure values become Invalid (with a single error).
    /// </summary>
    /// <typeparam name="T">The success type.</typeparam>
    /// <typeparam name="TError">The error type.</typeparam>
    /// <param name="source">The async sequence of results.</param>
    /// <returns>An async sequence of validations.</returns>
    public static IAsyncEnumerable<Validation<T, TError>> ToValidationStream<T, TError>(
        this IAsyncEnumerable<Result<T, TError>> source)
        where TError : Error =>
        source.Select(result => result.Match<Validation<T, TError>>(
            success: value => new Valid<T, TError>(value),
            failure: error => new Invalid<T, TError>([error])));
}

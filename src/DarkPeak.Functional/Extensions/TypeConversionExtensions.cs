namespace DarkPeak.Functional.Extensions;

/// <summary>
/// Extension methods for converting between Option, Result, and Either types.
/// </summary>
public static class TypeConversionExtensions
{
    // ── Option → Result ──

    /// <summary>
    /// Converts an Option to a Result, using the provided error if the option is None.
    /// </summary>
    public static Result<T, TError> ToResult<T, TError>(this Option<T> option, TError error)
        where TError : Error =>
        option.Match<Result<T, TError>>(
            some: value => new Success<T, TError>(value),
            none: () => new Failure<T, TError>(error)
        );

    /// <summary>
    /// Converts an Option to a Result, using a factory to create the error if the option is None.
    /// </summary>
    public static Result<T, TError> ToResult<T, TError>(this Option<T> option, Func<TError> errorFactory)
        where TError : Error =>
        option.Match<Result<T, TError>>(
            some: value => new Success<T, TError>(value),
            none: () => new Failure<T, TError>(errorFactory())
        );

    // ── Option → Either ──

    /// <summary>
    /// Converts an Option to an Either, using the value as Right if Some, or the provided left value if None.
    /// </summary>
    public static Either<TLeft, T> ToEither<TLeft, T>(this Option<T> option, TLeft leftValue) =>
        option.Match<Either<TLeft, T>>(
            some: value => new Right<TLeft, T>(value),
            none: () => new Left<TLeft, T>(leftValue)
        );

    /// <summary>
    /// Converts an Option to an Either, using the value as Right if Some, or a factory for the left value if None.
    /// </summary>
    public static Either<TLeft, T> ToEither<TLeft, T>(this Option<T> option, Func<TLeft> leftFactory) =>
        option.Match<Either<TLeft, T>>(
            some: value => new Right<TLeft, T>(value),
            none: () => new Left<TLeft, T>(leftFactory())
        );

    // ── Result → Either ──

    /// <summary>
    /// Converts a Result to an Either, with the error as Left and the value as Right.
    /// </summary>
    public static Either<TError, T> ToEither<T, TError>(this Result<T, TError> result)
        where TError : Error =>
        result.Match<Either<TError, T>>(
            success: value => new Right<TError, T>(value),
            failure: error => new Left<TError, T>(error)
        );

    // ── Either → Option ──

    /// <summary>
    /// Converts the right value of an Either to an Option, discarding the left value.
    /// </summary>
    public static Option<TRight> RightToOption<TLeft, TRight>(this Either<TLeft, TRight> either) =>
        either.Match<Option<TRight>>(
            left: _ => new None<TRight>(),
            right: value => new Some<TRight>(value)
        );

    /// <summary>
    /// Converts the left value of an Either to an Option, discarding the right value.
    /// </summary>
    public static Option<TLeft> LeftToOption<TLeft, TRight>(this Either<TLeft, TRight> either) =>
        either.Match<Option<TLeft>>(
            left: value => new Some<TLeft>(value),
            right: _ => new None<TLeft>()
        );

    // ── Either → Result ──

    /// <summary>
    /// Converts an Either to a Result, treating Left as failure and Right as success.
    /// The left type must be an Error subtype.
    /// </summary>
    public static Result<TRight, TLeft> ToResult<TLeft, TRight>(this Either<TLeft, TRight> either)
        where TLeft : Error =>
        either.Match<Result<TRight, TLeft>>(
            left: error => new Failure<TRight, TLeft>(error),
            right: value => new Success<TRight, TLeft>(value)
        );
}

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
        option.Match(
            some: value => (Result<T, TError>)new Success<T, TError>(value),
            none: () => (Result<T, TError>)new Failure<T, TError>(error)
        );

    /// <summary>
    /// Converts an Option to a Result, using a factory to create the error if the option is None.
    /// </summary>
    public static Result<T, TError> ToResult<T, TError>(this Option<T> option, Func<TError> errorFactory)
        where TError : Error =>
        option.Match(
            some: value => (Result<T, TError>)new Success<T, TError>(value),
            none: () => (Result<T, TError>)new Failure<T, TError>(errorFactory())
        );

    // ── Option → Either ──

    /// <summary>
    /// Converts an Option to an Either, using the value as Right if Some, or the provided left value if None.
    /// </summary>
    public static Either<TLeft, T> ToEither<TLeft, T>(this Option<T> option, TLeft leftValue) =>
        option.Match(
            some: value => (Either<TLeft, T>)new Right<TLeft, T>(value),
            none: () => (Either<TLeft, T>)new Left<TLeft, T>(leftValue)
        );

    /// <summary>
    /// Converts an Option to an Either, using the value as Right if Some, or a factory for the left value if None.
    /// </summary>
    public static Either<TLeft, T> ToEither<TLeft, T>(this Option<T> option, Func<TLeft> leftFactory) =>
        option.Match(
            some: value => (Either<TLeft, T>)new Right<TLeft, T>(value),
            none: () => (Either<TLeft, T>)new Left<TLeft, T>(leftFactory())
        );

    // ── Result → Option ──

    /// <summary>
    /// Converts the result to an option, discarding any error information.
    /// </summary>
    /// <returns>Some with the value if successful, None if failure.</returns>
    public static Option<T> AsOption<T, TError>(this Result<T, TError> result)
        where TError : Error =>
        result.Match(
            success: value => (Option<T>)new Some<T>(value),
            failure: _ => (Option<T>)new None<T>()
        );

    // ── Result → Either ──

    /// <summary>
    /// Converts a Result to an Either, with the error as Left and the value as Right.
    /// </summary>
    public static Either<TError, T> ToEither<T, TError>(this Result<T, TError> result)
        where TError : Error =>
        result.Match(
            success: value => (Either<TError, T>)new Right<TError, T>(value),
            failure: error => (Either<TError, T>)new Left<TError, T>(error)
        );

    // ── Either → Option ──

    /// <summary>
    /// Converts the right value of an Either to an Option, discarding the left value.
    /// </summary>
    public static Option<TRight> RightToOption<TLeft, TRight>(this Either<TLeft, TRight> either) =>
        either.Match(
            left: _ => (Option<TRight>)new None<TRight>(),
            right: value => (Option<TRight>)new Some<TRight>(value)
        );

    /// <summary>
    /// Converts the left value of an Either to an Option, discarding the right value.
    /// </summary>
    public static Option<TLeft> LeftToOption<TLeft, TRight>(this Either<TLeft, TRight> either) =>
        either.Match(
            left: value => (Option<TLeft>)new Some<TLeft>(value),
            right: _ => (Option<TLeft>)new None<TLeft>()
        );

    // ── Either → Result ──

    /// <summary>
    /// Converts an Either to a Result, treating Left as failure and Right as success.
    /// The left type must be an Error subtype.
    /// </summary>
    public static Result<TRight, TLeft> ToResult<TLeft, TRight>(this Either<TLeft, TRight> either)
        where TLeft : Error =>
        either.Match(
            left: error => (Result<TRight, TLeft>)new Failure<TRight, TLeft>(error),
            right: value => (Result<TRight, TLeft>)new Success<TRight, TLeft>(value)
        );
}

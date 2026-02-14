namespace DarkPeak.Functional.Extensions;

/// <summary>
/// Extension methods for working with Validation types.
/// </summary>
public static class ValidationExtensions
{
    /// <summary>
    /// Applies a wrapped function to a wrapped value, accumulating errors from both sides.
    /// This is the applicative functor operation that distinguishes Validation from Result.
    /// </summary>
    public static Validation<TResult, TError> Apply<T, TResult, TError>(
        this Validation<Func<T, TResult>, TError> validationFunc,
        Validation<T, TError> validationValue)
        where TError : Error =>
        (validationFunc, validationValue) switch
        {
            (Valid<Func<T, TResult>, TError> f, Valid<T, TError> v) =>
                new Valid<TResult, TError>(f.Value(v.Value)),

            (Invalid<Func<T, TResult>, TError> f, Invalid<T, TError> v) =>
                new Invalid<TResult, TError>(f.Errors.Concat(v.Errors).ToList().AsReadOnly()),

            (Invalid<Func<T, TResult>, TError> f, _) =>
                new Invalid<TResult, TError>(f.Errors),

            (_, Invalid<T, TError> v) =>
                new Invalid<TResult, TError>(v.Errors),

            _ => throw new InvalidOperationException("Unexpected validation state")
        };

    /// <summary>
    /// Combines two validations, accumulating errors from both if either fails.
    /// If both succeed, applies the combiner function to produce the result.
    /// </summary>
    public static Validation<TResult, TError> Combine<T1, T2, TResult, TError>(
        this Validation<T1, TError> first,
        Validation<T2, TError> second,
        Func<T1, T2, TResult> combiner)
        where TError : Error =>
        first.Map<Func<T2, TResult>>(a => b => combiner(a, b)).Apply(second);

    /// <summary>
    /// Combines three validations, accumulating errors from all.
    /// </summary>
    public static Validation<TResult, TError> Combine<T1, T2, T3, TResult, TError>(
        this Validation<T1, TError> first,
        Validation<T2, TError> second,
        Validation<T3, TError> third,
        Func<T1, T2, T3, TResult> combiner)
        where TError : Error =>
        first
            .Map<Func<T2, Func<T3, TResult>>>(a => b => c => combiner(a, b, c))
            .Apply(second)
            .Apply(third);

    /// <summary>
    /// Combines a sequence of validations, accumulating all errors.
    /// Returns Valid with all values if all succeed, or Invalid with all collected errors.
    /// </summary>
    public static Validation<IEnumerable<T>, TError> Combine<T, TError>(
        this IEnumerable<Validation<T, TError>> validations)
        where TError : Error
    {
        var values = new List<T>();
        var errors = new List<TError>();

        foreach (var validation in validations)
        {
            validation.Match<object?>(
                valid: v => { values.Add(v); return null; },
                invalid: errs => { errors.AddRange(errs); return null; }
            );
        }

        return errors.Count > 0
            ? new Invalid<IEnumerable<T>, TError>(errors.AsReadOnly())
            : new Valid<IEnumerable<T>, TError>(values);
    }

    /// <summary>
    /// Converts a Validation to a Result, taking the first error if invalid.
    /// </summary>
    public static Result<T, TError> ToResult<T, TError>(this Validation<T, TError> validation)
        where TError : Error =>
        validation.Match<Result<T, TError>>(
            valid: value => new Success<T, TError>(value),
            invalid: errors => new Failure<T, TError>(errors[0])
        );

    /// <summary>
    /// Converts a Result to a Validation.
    /// </summary>
    public static Validation<T, TError> ToValidation<T, TError>(this Result<T, TError> result)
        where TError : Error =>
        result.Match<Validation<T, TError>>(
            success: value => new Valid<T, TError>(value),
            failure: error => new Invalid<T, TError>([error])
        );
}

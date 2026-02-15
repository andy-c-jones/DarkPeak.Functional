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
    /// Combines two validations with a projection function, accumulating errors from both if either fails.
    /// If both succeed, applies the combiner function to produce the result.
    /// </summary>
    public static Validation<TResult, TError> ZipWith<T1, T2, TResult, TError>(
        this Validation<T1, TError> first,
        Validation<T2, TError> second,
        Func<T1, T2, TResult> combiner)
        where TError : Error =>
        first.Map<Func<T2, TResult>>(a => b => combiner(a, b)).Apply(second);

    /// <summary>
    /// Combines three validations with a projection function, accumulating errors from all.
    /// </summary>
    public static Validation<TResult, TError> ZipWith<T1, T2, T3, TResult, TError>(
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
    /// Converts a sequence of validations into a single validation containing all values, accumulating all errors.
    /// Returns Valid with all values if all succeed, or Invalid with all collected errors.
    /// </summary>
    public static Validation<IEnumerable<T>, TError> Sequence<T, TError>(
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

    // --- Traverse ---

    /// <summary>
    /// Applies a Validation-returning function to each element, then sequences the results.
    /// Returns Valid with all mapped values if all succeed, or Invalid with all accumulated errors.
    /// </summary>
    /// <typeparam name="T">The source element type.</typeparam>
    /// <typeparam name="TResult">The mapped valid type.</typeparam>
    /// <typeparam name="TError">The error type.</typeparam>
    /// <param name="source">The source sequence.</param>
    /// <param name="func">A function that returns a Validation for each element.</param>
    /// <returns>Valid with all mapped values, or Invalid with all accumulated errors.</returns>
    public static Validation<IEnumerable<TResult>, TError> Traverse<T, TResult, TError>(
        this IEnumerable<T> source, Func<T, Validation<TResult, TError>> func)
        where TError : Error
    {
        var values = new List<TResult>();
        var errors = new List<TError>();

        foreach (var item in source)
        {
            var validation = func(item);
            validation.Match<object?>(
                valid: v => { values.Add(v); return null; },
                invalid: errs => { errors.AddRange(errs); return null; }
            );
        }

        return errors.Count > 0
            ? new Invalid<IEnumerable<TResult>, TError>(errors.AsReadOnly())
            : new Valid<IEnumerable<TResult>, TError>(values);
    }

    // --- Join ---

    /// <summary>
    /// Combines two independent Validations into a tuple, accumulating all errors from both.
    /// </summary>
    /// <typeparam name="T1">The first valid type.</typeparam>
    /// <typeparam name="T2">The second valid type.</typeparam>
    /// <typeparam name="TError">The error type.</typeparam>
    /// <param name="first">The first validation.</param>
    /// <param name="second">The second validation.</param>
    /// <returns>Valid with a tuple of both values, or Invalid with all accumulated errors.</returns>
    public static Validation<(T1, T2), TError> Join<T1, T2, TError>(
        this Validation<T1, TError> first, Validation<T2, TError> second)
        where TError : Error =>
        first.ZipWith(second, (v1, v2) => (v1, v2));

    /// <summary>
    /// Combines three independent Validations into a tuple, accumulating all errors from all.
    /// </summary>
    /// <typeparam name="T1">The first valid type.</typeparam>
    /// <typeparam name="T2">The second valid type.</typeparam>
    /// <typeparam name="T3">The third valid type.</typeparam>
    /// <typeparam name="TError">The error type.</typeparam>
    /// <param name="first">The first validation.</param>
    /// <param name="second">The second validation.</param>
    /// <param name="third">The third validation.</param>
    /// <returns>Valid with a tuple of all values, or Invalid with all accumulated errors.</returns>
    public static Validation<(T1, T2, T3), TError> Join<T1, T2, T3, TError>(
        this Validation<T1, TError> first, Validation<T2, TError> second, Validation<T3, TError> third)
        where TError : Error =>
        first.ZipWith(second, third, (v1, v2, v3) => (v1, v2, v3));
}

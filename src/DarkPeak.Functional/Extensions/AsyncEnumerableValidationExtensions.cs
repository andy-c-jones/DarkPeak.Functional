using System.Runtime.CompilerServices;

namespace DarkPeak.Functional.Extensions;

/// <summary>
/// Extension methods for working with <see cref="IAsyncEnumerable{T}"/> of <see cref="Validation{T, TError}"/>.
/// Provides element-wise Map, Tap operations on streams of validations,
/// plus terminal operations like SequenceAsync and PartitionAsync.
/// </summary>
public static class AsyncEnumerableValidationExtensions
{
    // ── MapValid ──

    /// <summary>
    /// Transforms the value inside each Valid in an async sequence of validations.
    /// Invalid values are passed through unchanged.
    /// </summary>
    /// <typeparam name="T">The source value type.</typeparam>
    /// <typeparam name="TError">The error type.</typeparam>
    /// <typeparam name="TResult">The result value type.</typeparam>
    /// <param name="source">The async sequence of validations.</param>
    /// <param name="mapper">The function to apply to each valid value.</param>
    /// <returns>An async sequence of validations with mapped values.</returns>
    public static IAsyncEnumerable<Validation<TResult, TError>> MapValid<T, TError, TResult>(
        this IAsyncEnumerable<Validation<T, TError>> source, Func<T, TResult> mapper)
        where TError : Error =>
        source.Select(validation => validation.Map(mapper));

    /// <summary>
    /// Transforms the value inside each Valid in an async sequence of validations using an async function.
    /// Invalid values are passed through unchanged.
    /// </summary>
    /// <typeparam name="T">The source value type.</typeparam>
    /// <typeparam name="TError">The error type.</typeparam>
    /// <typeparam name="TResult">The result value type.</typeparam>
    /// <param name="source">The async sequence of validations.</param>
    /// <param name="mapper">The async function to apply to each valid value.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An async sequence of validations with mapped values.</returns>
    public static async IAsyncEnumerable<Validation<TResult, TError>> MapValidAsync<T, TError, TResult>(
        this IAsyncEnumerable<Validation<T, TError>> source,
        Func<T, Task<TResult>> mapper,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
        where TError : Error
    {
        await foreach (var validation in source.WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            yield return await validation.MapAsync(mapper).ConfigureAwait(false);
        }
    }

    // ── TapValid / TapInvalid ──

    /// <summary>
    /// Executes a side-effect action for each valid value in an async sequence of validations.
    /// The sequence is not modified.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <typeparam name="TError">The error type.</typeparam>
    /// <param name="source">The async sequence of validations.</param>
    /// <param name="action">The action to execute for each valid value.</param>
    /// <returns>The original async sequence with side-effects applied to valid values.</returns>
    public static IAsyncEnumerable<Validation<T, TError>> TapValid<T, TError>(
        this IAsyncEnumerable<Validation<T, TError>> source, Action<T> action)
        where TError : Error =>
        source.Select(validation => validation.Tap(action));

    /// <summary>
    /// Executes a side-effect action for each invalid value in an async sequence of validations.
    /// The sequence is not modified.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <typeparam name="TError">The error type.</typeparam>
    /// <param name="source">The async sequence of validations.</param>
    /// <param name="action">The action to execute for each list of errors.</param>
    /// <returns>The original async sequence with side-effects applied to invalid values.</returns>
    public static IAsyncEnumerable<Validation<T, TError>> TapInvalid<T, TError>(
        this IAsyncEnumerable<Validation<T, TError>> source, Action<IReadOnlyList<TError>> action)
        where TError : Error =>
        source.Select(validation => validation.TapInvalid(action));

    // ── ChooseValid ──

    /// <summary>
    /// Filters out Invalid values and unwraps the Valid values from an async sequence of validations.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <typeparam name="TError">The error type.</typeparam>
    /// <param name="source">The async sequence of validations.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An async sequence containing only the unwrapped valid values.</returns>
    public static async IAsyncEnumerable<T> ChooseValid<T, TError>(
        this IAsyncEnumerable<Validation<T, TError>> source,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
        where TError : Error
    {
        await foreach (var validation in source.WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            if (validation is Valid<T, TError> valid)
            {
                yield return valid.Value;
            }
        }
    }

    // ── SequenceAsync ──

    /// <summary>
    /// Collects all validations from an async sequence into a single validation.
    /// Returns Valid with all values if ALL are valid, or Invalid with ALL accumulated errors.
    /// Unlike Result's SequenceAsync, this does NOT short-circuit — it collects every error.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <typeparam name="TError">The error type.</typeparam>
    /// <param name="source">The async sequence of validations.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Valid with all values, or Invalid with all accumulated errors.</returns>
    public static async Task<Validation<IReadOnlyList<T>, TError>> SequenceAsync<T, TError>(
        this IAsyncEnumerable<Validation<T, TError>> source,
        CancellationToken cancellationToken = default)
        where TError : Error
    {
        var values = new List<T>();
        var errors = new List<TError>();

        await foreach (var validation in source.WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            if (validation is Valid<T, TError> valid)
            {
                values.Add(valid.Value);
            }
            else if (validation is Invalid<T, TError> invalid)
            {
                errors.AddRange(invalid.Errors);
            }
        }

        if (errors.Count > 0)
        {
            return new Invalid<IReadOnlyList<T>, TError>(errors);
        }

        return new Valid<IReadOnlyList<T>, TError>(values);
    }

    // ── PartitionAsync ──

    /// <summary>
    /// Consumes an async sequence of validations and partitions them into valid values and errors.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <typeparam name="TError">The error type.</typeparam>
    /// <param name="source">The async sequence of validations.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A tuple of (valid values, errors).</returns>
    public static async Task<(IReadOnlyList<T> Valid, IReadOnlyList<TError> Errors)> PartitionAsync<T, TError>(
        this IAsyncEnumerable<Validation<T, TError>> source,
        CancellationToken cancellationToken = default)
        where TError : Error
    {
        var valid = new List<T>();
        var errors = new List<TError>();

        await foreach (var validation in source.WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            if (validation is Valid<T, TError> v)
            {
                valid.Add(v.Value);
            }
            else if (validation is Invalid<T, TError> invalid)
            {
                errors.AddRange(invalid.Errors);
            }
        }

        return (valid, errors);
    }
}

using System.Runtime.CompilerServices;

namespace DarkPeak.Functional.Extensions;

/// <summary>
/// Extension methods for working with <see cref="IAsyncEnumerable{T}"/> of <see cref="Result{T, TError}"/>.
/// Provides element-wise Map, Bind, Tap operations on streams of results,
/// plus terminal operations like SequenceAsync and PartitionAsync.
/// </summary>
public static class AsyncEnumerableResultExtensions
{
    // ── MapResult ──

    /// <summary>
    /// Transforms the success value inside each Result in an async sequence.
    /// Failure values are passed through unchanged.
    /// </summary>
    /// <typeparam name="T">The source success type.</typeparam>
    /// <typeparam name="TError">The error type.</typeparam>
    /// <typeparam name="TResult">The result success type.</typeparam>
    /// <param name="source">The async sequence of results.</param>
    /// <param name="mapper">The function to apply to each success value.</param>
    /// <returns>An async sequence of results with mapped success values.</returns>
    public static IAsyncEnumerable<Result<TResult, TError>> MapResult<T, TError, TResult>(
        this IAsyncEnumerable<Result<T, TError>> source, Func<T, TResult> mapper)
        where TError : Error =>
        source.Select(result => result.Map(mapper));

    /// <summary>
    /// Transforms the success value inside each Result in an async sequence using an async function.
    /// Failure values are passed through unchanged.
    /// </summary>
    /// <typeparam name="T">The source success type.</typeparam>
    /// <typeparam name="TError">The error type.</typeparam>
    /// <typeparam name="TResult">The result success type.</typeparam>
    /// <param name="source">The async sequence of results.</param>
    /// <param name="mapper">The async function to apply to each success value.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An async sequence of results with mapped success values.</returns>
    public static async IAsyncEnumerable<Result<TResult, TError>> MapResultAsync<T, TError, TResult>(
        this IAsyncEnumerable<Result<T, TError>> source,
        Func<T, Task<TResult>> mapper,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
        where TError : Error
    {
        await foreach (var result in source.WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            yield return await result.MapAsync(mapper).ConfigureAwait(false);
        }
    }

    // ── BindResult ──

    /// <summary>
    /// Applies a function that returns a Result to each success value in an async sequence of results.
    /// Failure values are passed through unchanged.
    /// </summary>
    /// <typeparam name="T">The source success type.</typeparam>
    /// <typeparam name="TError">The error type.</typeparam>
    /// <typeparam name="TResult">The result success type.</typeparam>
    /// <param name="source">The async sequence of results.</param>
    /// <param name="binder">The function to apply to each success value.</param>
    /// <returns>An async sequence of results with bound values.</returns>
    public static IAsyncEnumerable<Result<TResult, TError>> BindResult<T, TError, TResult>(
        this IAsyncEnumerable<Result<T, TError>> source, Func<T, Result<TResult, TError>> binder)
        where TError : Error =>
        source.Select(result => result.Bind(binder));

    /// <summary>
    /// Applies an async function that returns a Result to each success value in an async sequence of results.
    /// Failure values are passed through unchanged.
    /// </summary>
    /// <typeparam name="T">The source success type.</typeparam>
    /// <typeparam name="TError">The error type.</typeparam>
    /// <typeparam name="TResult">The result success type.</typeparam>
    /// <param name="source">The async sequence of results.</param>
    /// <param name="binder">The async function to apply to each success value.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An async sequence of results with bound values.</returns>
    public static async IAsyncEnumerable<Result<TResult, TError>> BindResultAsync<T, TError, TResult>(
        this IAsyncEnumerable<Result<T, TError>> source,
        Func<T, Task<Result<TResult, TError>>> binder,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
        where TError : Error
    {
        await foreach (var result in source.WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            yield return await result.BindAsync(binder).ConfigureAwait(false);
        }
    }

    // ── TapResult / TapResultError ──

    /// <summary>
    /// Executes a side-effect action for each success value in an async sequence of results.
    /// The sequence is not modified.
    /// </summary>
    /// <typeparam name="T">The success type.</typeparam>
    /// <typeparam name="TError">The error type.</typeparam>
    /// <param name="source">The async sequence of results.</param>
    /// <param name="action">The action to execute for each success value.</param>
    /// <returns>The original async sequence with side-effects applied to successes.</returns>
    public static IAsyncEnumerable<Result<T, TError>> TapResult<T, TError>(
        this IAsyncEnumerable<Result<T, TError>> source, Action<T> action)
        where TError : Error =>
        source.Select(result => result.Tap(action));

    /// <summary>
    /// Executes a side-effect action for each error in an async sequence of results.
    /// The sequence is not modified.
    /// </summary>
    /// <typeparam name="T">The success type.</typeparam>
    /// <typeparam name="TError">The error type.</typeparam>
    /// <param name="source">The async sequence of results.</param>
    /// <param name="action">The action to execute for each error.</param>
    /// <returns>The original async sequence with side-effects applied to failures.</returns>
    public static IAsyncEnumerable<Result<T, TError>> TapResultError<T, TError>(
        this IAsyncEnumerable<Result<T, TError>> source, Action<TError> action)
        where TError : Error =>
        source.Select(result => result.TapError(action));

    // ── ChooseResults ──

    /// <summary>
    /// Filters out failures from an async sequence of results and unwraps the success values.
    /// </summary>
    /// <typeparam name="T">The success type.</typeparam>
    /// <typeparam name="TError">The error type.</typeparam>
    /// <param name="source">The async sequence of results.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An async sequence containing only the unwrapped success values.</returns>
    public static async IAsyncEnumerable<T> ChooseResults<T, TError>(
        this IAsyncEnumerable<Result<T, TError>> source,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
        where TError : Error
    {
        await foreach (var result in source.WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            if (result is Success<T, TError> success)
            {
                yield return success.Value;
            }
        }
    }

    // ── SequenceAsync ──

    /// <summary>
    /// Collects all results from an async sequence into a single result.
    /// Returns Success with all values if ALL succeed, or the first Failure encountered (short-circuits).
    /// </summary>
    /// <typeparam name="T">The success type.</typeparam>
    /// <typeparam name="TError">The error type.</typeparam>
    /// <param name="source">The async sequence of results.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Success with all values, or the first Failure.</returns>
    public static async Task<Result<IReadOnlyList<T>, TError>> SequenceAsync<T, TError>(
        this IAsyncEnumerable<Result<T, TError>> source,
        CancellationToken cancellationToken = default)
        where TError : Error
    {
        var values = new List<T>();

        await foreach (var result in source.WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            if (result is Success<T, TError> success)
            {
                values.Add(success.Value);
            }
            else if (result is Failure<T, TError> failure)
            {
                return Result.Failure<IReadOnlyList<T>, TError>(failure.Error);
            }
        }

        return Result.Success<IReadOnlyList<T>, TError>(values);
    }

    // ── PartitionAsync ──

    /// <summary>
    /// Consumes an async sequence of results and partitions them into successes and failures.
    /// </summary>
    /// <typeparam name="T">The success type.</typeparam>
    /// <typeparam name="TError">The error type.</typeparam>
    /// <param name="source">The async sequence of results.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A tuple of (successes, failures).</returns>
    public static async Task<(IReadOnlyList<T> Successes, IReadOnlyList<TError> Failures)> PartitionAsync<T, TError>(
        this IAsyncEnumerable<Result<T, TError>> source,
        CancellationToken cancellationToken = default)
        where TError : Error
    {
        var successes = new List<T>();
        var failures = new List<TError>();

        await foreach (var result in source.WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            if (result is Success<T, TError> success)
            {
                successes.Add(success.Value);
            }
            else if (result is Failure<T, TError> failure)
            {
                failures.Add(failure.Error);
            }
        }

        return (successes, failures);
    }
}

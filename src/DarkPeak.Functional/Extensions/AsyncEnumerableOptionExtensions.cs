using System.Runtime.CompilerServices;

namespace DarkPeak.Functional.Extensions;

/// <summary>
/// Extension methods for working with <see cref="IAsyncEnumerable{T}"/> of <see cref="Option{T}"/>.
/// Provides Choose, ChooseMap, MapOption, BindOption, and terminal query operations
/// that return <see cref="Option{T}"/> instead of throwing on empty sequences.
/// </summary>
public static class AsyncEnumerableOptionExtensions
{
    // ── Choose ──

    /// <summary>
    /// Filters out None values and unwraps the Some values from an async sequence of options.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="source">The async sequence of options.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An async sequence containing only the unwrapped Some values.</returns>
    public static async IAsyncEnumerable<T> Choose<T>(
        this IAsyncEnumerable<Option<T>> source,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await foreach (var option in source.WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            if (option is Some<T> some)
            {
                yield return some.Value;
            }
        }
    }

    /// <summary>
    /// Applies a function that returns an Option to each element and filters out None values.
    /// Equivalent to Map followed by Choose, but more efficient.
    /// </summary>
    /// <typeparam name="TSource">The source element type.</typeparam>
    /// <typeparam name="TResult">The result value type.</typeparam>
    /// <param name="source">The async sequence.</param>
    /// <param name="selector">Function that returns an Option for each element.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An async sequence containing only the unwrapped Some values.</returns>
    public static async IAsyncEnumerable<TResult> ChooseMap<TSource, TResult>(
        this IAsyncEnumerable<TSource> source,
        Func<TSource, Option<TResult>> selector,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await foreach (var item in source.WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            var option = selector(item);
            if (option is Some<TResult> some)
            {
                yield return some.Value;
            }
        }
    }

    /// <summary>
    /// Applies an async function that returns an Option to each element and filters out None values.
    /// </summary>
    /// <typeparam name="TSource">The source element type.</typeparam>
    /// <typeparam name="TResult">The result value type.</typeparam>
    /// <param name="source">The async sequence.</param>
    /// <param name="selector">Async function that returns an Option for each element.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An async sequence containing only the unwrapped Some values.</returns>
    public static async IAsyncEnumerable<TResult> ChooseMapAsync<TSource, TResult>(
        this IAsyncEnumerable<TSource> source,
        Func<TSource, Task<Option<TResult>>> selector,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await foreach (var item in source.WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            var option = await selector(item).ConfigureAwait(false);
            if (option is Some<TResult> some)
            {
                yield return some.Value;
            }
        }
    }

    // ── MapOption ──

    /// <summary>
    /// Transforms the value inside each Some in an async sequence of options.
    /// None values are passed through unchanged.
    /// </summary>
    /// <typeparam name="T">The source value type.</typeparam>
    /// <typeparam name="TResult">The result value type.</typeparam>
    /// <param name="source">The async sequence of options.</param>
    /// <param name="mapper">The function to apply to each Some value.</param>
    /// <returns>An async sequence of options with mapped values.</returns>
    public static IAsyncEnumerable<Option<TResult>> MapOption<T, TResult>(
        this IAsyncEnumerable<Option<T>> source, Func<T, TResult> mapper) =>
        source.Select(option => option.Map(mapper));

    /// <summary>
    /// Transforms the value inside each Some in an async sequence of options using an async function.
    /// None values are passed through unchanged.
    /// </summary>
    /// <typeparam name="T">The source value type.</typeparam>
    /// <typeparam name="TResult">The result value type.</typeparam>
    /// <param name="source">The async sequence of options.</param>
    /// <param name="mapper">The async function to apply to each Some value.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An async sequence of options with mapped values.</returns>
    public static async IAsyncEnumerable<Option<TResult>> MapOptionAsync<T, TResult>(
        this IAsyncEnumerable<Option<T>> source,
        Func<T, Task<TResult>> mapper,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await foreach (var option in source.WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            yield return await option.MapAsync(mapper).ConfigureAwait(false);
        }
    }

    // ── BindOption ──

    /// <summary>
    /// Applies a function that returns an Option to each Some value in an async sequence of options.
    /// None values are passed through unchanged.
    /// </summary>
    /// <typeparam name="T">The source value type.</typeparam>
    /// <typeparam name="TResult">The result value type.</typeparam>
    /// <param name="source">The async sequence of options.</param>
    /// <param name="binder">The function to apply to each Some value.</param>
    /// <returns>An async sequence of options with bound values.</returns>
    public static IAsyncEnumerable<Option<TResult>> BindOption<T, TResult>(
        this IAsyncEnumerable<Option<T>> source, Func<T, Option<TResult>> binder) =>
        source.Select(option => option.Bind(binder));

    /// <summary>
    /// Applies an async function that returns an Option to each Some value in an async sequence of options.
    /// None values are passed through unchanged.
    /// </summary>
    /// <typeparam name="T">The source value type.</typeparam>
    /// <typeparam name="TResult">The result value type.</typeparam>
    /// <param name="source">The async sequence of options.</param>
    /// <param name="binder">The async function to apply to each Some value.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An async sequence of options with bound values.</returns>
    public static async IAsyncEnumerable<Option<TResult>> BindOptionAsync<T, TResult>(
        this IAsyncEnumerable<Option<T>> source,
        Func<T, Task<Option<TResult>>> binder,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await foreach (var option in source.WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            yield return await option.BindAsync(binder).ConfigureAwait(false);
        }
    }

    // ── Sequence ──

    /// <summary>
    /// Converts an async sequence of Options into an Option of a list.
    /// Returns Some with all values if ALL are Some, or None if ANY is None (short-circuits).
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="source">The async sequence of options.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Some with all values, or None if any option is None.</returns>
    public static async Task<Option<IReadOnlyList<T>>> SequenceAsync<T>(
        this IAsyncEnumerable<Option<T>> source,
        CancellationToken cancellationToken = default)
    {
        var values = new List<T>();

        await foreach (var option in source.WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            if (option is Some<T> some)
            {
                values.Add(some.Value);
            }
            else
            {
                return Option.None<IReadOnlyList<T>>();
            }
        }

        return Option.Some<IReadOnlyList<T>>(values);
    }

    // ── FirstOrNoneAsync ──

    /// <summary>
    /// Returns the first element of an async sequence as an option, or None if the sequence is empty.
    /// </summary>
    /// <typeparam name="T">The element type.</typeparam>
    /// <param name="source">The async sequence.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Some with the first element, or None if the sequence is empty.</returns>
    public static async Task<Option<T>> FirstOrNoneAsync<T>(
        this IAsyncEnumerable<T> source,
        CancellationToken cancellationToken = default)
    {
        await foreach (var item in source.WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            return Option.Some(item);
        }

        return Option.None<T>();
    }

    /// <summary>
    /// Returns the first element of an async sequence that satisfies a condition as an option,
    /// or None if no such element is found.
    /// </summary>
    /// <typeparam name="T">The element type.</typeparam>
    /// <param name="source">The async sequence.</param>
    /// <param name="predicate">The condition to test.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Some with the first matching element, or None if no element matches.</returns>
    public static async Task<Option<T>> FirstOrNoneAsync<T>(
        this IAsyncEnumerable<T> source,
        Func<T, bool> predicate,
        CancellationToken cancellationToken = default)
    {
        await foreach (var item in source.WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            if (predicate(item))
            {
                return Option.Some(item);
            }
        }

        return Option.None<T>();
    }

    // ── SingleOrNoneAsync ──

    /// <summary>
    /// Returns the single element of an async sequence as an option,
    /// or None if the sequence is empty or contains more than one element.
    /// </summary>
    /// <typeparam name="T">The element type.</typeparam>
    /// <param name="source">The async sequence.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Some with the single element, or None if the sequence is empty or contains multiple elements.</returns>
    public static async Task<Option<T>> SingleOrNoneAsync<T>(
        this IAsyncEnumerable<T> source,
        CancellationToken cancellationToken = default)
    {
        var enumerator = source.GetAsyncEnumerator(cancellationToken);
        await using (enumerator.ConfigureAwait(false))
        {
            if (!await enumerator.MoveNextAsync().ConfigureAwait(false))
            {
                return Option.None<T>();
            }

            var first = enumerator.Current;

            if (await enumerator.MoveNextAsync().ConfigureAwait(false))
            {
                return Option.None<T>();
            }

            return Option.Some(first);
        }
    }

    /// <summary>
    /// Returns the single element of an async sequence that satisfies a condition as an option,
    /// or None if no such element exists or multiple elements match.
    /// </summary>
    /// <typeparam name="T">The element type.</typeparam>
    /// <param name="source">The async sequence.</param>
    /// <param name="predicate">The condition to test.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Some with the single matching element, or None if no element matches or multiple elements match.</returns>
    public static Task<Option<T>> SingleOrNoneAsync<T>(
        this IAsyncEnumerable<T> source,
        Func<T, bool> predicate,
        CancellationToken cancellationToken = default) =>
        source.Where(predicate).SingleOrNoneAsync(cancellationToken);

    // ── LastOrNoneAsync ──

    /// <summary>
    /// Returns the last element of an async sequence as an option, or None if the sequence is empty.
    /// </summary>
    /// <typeparam name="T">The element type.</typeparam>
    /// <param name="source">The async sequence.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Some with the last element, or None if the sequence is empty.</returns>
    public static async Task<Option<T>> LastOrNoneAsync<T>(
        this IAsyncEnumerable<T> source,
        CancellationToken cancellationToken = default)
    {
        var hasValue = false;
        T last = default!;

        await foreach (var item in source.WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            hasValue = true;
            last = item;
        }

        return hasValue ? Option.Some(last) : Option.None<T>();
    }

    /// <summary>
    /// Returns the last element of an async sequence that satisfies a condition as an option,
    /// or None if no such element is found.
    /// </summary>
    /// <typeparam name="T">The element type.</typeparam>
    /// <param name="source">The async sequence.</param>
    /// <param name="predicate">The condition to test.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Some with the last matching element, or None if no element matches.</returns>
    public static Task<Option<T>> LastOrNoneAsync<T>(
        this IAsyncEnumerable<T> source,
        Func<T, bool> predicate,
        CancellationToken cancellationToken = default) =>
        source.Where(predicate).LastOrNoneAsync(cancellationToken);
}

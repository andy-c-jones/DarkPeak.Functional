using System.Runtime.CompilerServices;

namespace DarkPeak.Functional.Extensions;

/// <summary>
/// Functional extension methods for <see cref="IAsyncEnumerable{T}"/>.
/// Provides Map, Filter, Bind, Tap, Scan, Unfold, Buffer, and ForEachAsync
/// as functional aliases and additional operations beyond what the BCL provides.
/// </summary>
public static class AsyncEnumerableExtensions
{
    // ── Map ──

    /// <summary>
    /// Transforms each element of an async sequence using the specified mapping function.
    /// </summary>
    /// <typeparam name="T">The source element type.</typeparam>
    /// <typeparam name="TResult">The result element type.</typeparam>
    /// <param name="source">The async sequence to transform.</param>
    /// <param name="mapper">The function to apply to each element.</param>
    /// <returns>An async sequence of transformed elements.</returns>
    public static IAsyncEnumerable<TResult> Map<T, TResult>(
        this IAsyncEnumerable<T> source, Func<T, TResult> mapper) =>
        source.Select(mapper);

    /// <summary>
    /// Transforms each element of an async sequence using the specified async mapping function.
    /// </summary>
    /// <typeparam name="T">The source element type.</typeparam>
    /// <typeparam name="TResult">The result element type.</typeparam>
    /// <param name="source">The async sequence to transform.</param>
    /// <param name="mapper">The async function to apply to each element.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An async sequence of transformed elements.</returns>
    public static async IAsyncEnumerable<TResult> MapAsync<T, TResult>(
        this IAsyncEnumerable<T> source,
        Func<T, Task<TResult>> mapper,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await foreach (var item in source.WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            yield return await mapper(item).ConfigureAwait(false);
        }
    }

    // ── Filter ──

    /// <summary>
    /// Filters elements of an async sequence based on a predicate.
    /// </summary>
    /// <typeparam name="T">The element type.</typeparam>
    /// <param name="source">The async sequence to filter.</param>
    /// <param name="predicate">The predicate to test each element.</param>
    /// <returns>An async sequence containing only elements that satisfy the predicate.</returns>
    public static IAsyncEnumerable<T> Filter<T>(
        this IAsyncEnumerable<T> source, Func<T, bool> predicate) =>
        source.Where(predicate);

    /// <summary>
    /// Filters elements of an async sequence based on an async predicate.
    /// </summary>
    /// <typeparam name="T">The element type.</typeparam>
    /// <param name="source">The async sequence to filter.</param>
    /// <param name="predicate">The async predicate to test each element.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An async sequence containing only elements that satisfy the predicate.</returns>
    public static async IAsyncEnumerable<T> FilterAsync<T>(
        this IAsyncEnumerable<T> source,
        Func<T, Task<bool>> predicate,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await foreach (var item in source.WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            if (await predicate(item).ConfigureAwait(false))
            {
                yield return item;
            }
        }
    }

    // ── Bind (FlatMap) ──

    /// <summary>
    /// Projects each element to an async sequence and flattens the results.
    /// </summary>
    /// <typeparam name="T">The source element type.</typeparam>
    /// <typeparam name="TResult">The result element type.</typeparam>
    /// <param name="source">The async sequence to project.</param>
    /// <param name="binder">The function that returns an async sequence for each element.</param>
    /// <returns>A flattened async sequence.</returns>
    public static IAsyncEnumerable<TResult> Bind<T, TResult>(
        this IAsyncEnumerable<T> source, Func<T, IAsyncEnumerable<TResult>> binder) =>
        source.SelectMany(binder);

    /// <summary>
    /// Projects each element to an async sequence using an async selector and flattens the results.
    /// </summary>
    /// <typeparam name="T">The source element type.</typeparam>
    /// <typeparam name="TResult">The result element type.</typeparam>
    /// <param name="source">The async sequence to project.</param>
    /// <param name="binder">The async function that returns an async sequence for each element.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A flattened async sequence.</returns>
    public static async IAsyncEnumerable<TResult> BindAsync<T, TResult>(
        this IAsyncEnumerable<T> source,
        Func<T, Task<IAsyncEnumerable<TResult>>> binder,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await foreach (var item in source.WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            var inner = await binder(item).ConfigureAwait(false);
            await foreach (var innerItem in inner.WithCancellation(cancellationToken).ConfigureAwait(false))
            {
                yield return innerItem;
            }
        }
    }

    // ── Tap (side-effect) ──

    /// <summary>
    /// Executes a side-effect action for each element without modifying the sequence.
    /// </summary>
    /// <typeparam name="T">The element type.</typeparam>
    /// <param name="source">The async sequence.</param>
    /// <param name="action">The action to execute for each element.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The original async sequence with side-effects applied.</returns>
    public static async IAsyncEnumerable<T> Tap<T>(
        this IAsyncEnumerable<T> source,
        Action<T> action,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await foreach (var item in source.WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            action(item);
            yield return item;
        }
    }

    /// <summary>
    /// Executes an async side-effect action for each element without modifying the sequence.
    /// </summary>
    /// <typeparam name="T">The element type.</typeparam>
    /// <param name="source">The async sequence.</param>
    /// <param name="action">The async action to execute for each element.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The original async sequence with side-effects applied.</returns>
    public static async IAsyncEnumerable<T> TapAsync<T>(
        this IAsyncEnumerable<T> source,
        Func<T, Task> action,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await foreach (var item in source.WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            await action(item).ConfigureAwait(false);
            yield return item;
        }
    }

    // ── Scan ──

    /// <summary>
    /// Applies an accumulator function over an async sequence and yields each intermediate result.
    /// </summary>
    /// <typeparam name="T">The source element type.</typeparam>
    /// <typeparam name="TAccumulate">The accumulator type.</typeparam>
    /// <param name="source">The async sequence.</param>
    /// <param name="seed">The initial accumulator value.</param>
    /// <param name="accumulator">The accumulator function.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An async sequence of intermediate accumulator values.</returns>
    public static async IAsyncEnumerable<TAccumulate> Scan<T, TAccumulate>(
        this IAsyncEnumerable<T> source,
        TAccumulate seed,
        Func<TAccumulate, T, TAccumulate> accumulator,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var state = seed;
        await foreach (var item in source.WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            state = accumulator(state, item);
            yield return state;
        }
    }

    /// <summary>
    /// Applies an async accumulator function over an async sequence and yields each intermediate result.
    /// </summary>
    /// <typeparam name="T">The source element type.</typeparam>
    /// <typeparam name="TAccumulate">The accumulator type.</typeparam>
    /// <param name="source">The async sequence.</param>
    /// <param name="seed">The initial accumulator value.</param>
    /// <param name="accumulator">The async accumulator function.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An async sequence of intermediate accumulator values.</returns>
    public static async IAsyncEnumerable<TAccumulate> ScanAsync<T, TAccumulate>(
        this IAsyncEnumerable<T> source,
        TAccumulate seed,
        Func<TAccumulate, T, Task<TAccumulate>> accumulator,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var state = seed;
        await foreach (var item in source.WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            state = await accumulator(state, item).ConfigureAwait(false);
            yield return state;
        }
    }

    // ── Unfold ──

    /// <summary>
    /// Generates an async sequence from a seed value using a generator function.
    /// The generator returns <see cref="Option{T}"/> — <c>Some((element, nextSeed))</c> to continue
    /// or <c>None</c> to stop.
    /// </summary>
    /// <typeparam name="T">The element type.</typeparam>
    /// <typeparam name="TState">The seed/state type.</typeparam>
    /// <param name="seed">The initial state.</param>
    /// <param name="generator">A function that produces the next element and state, or None to stop.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An async sequence of generated elements.</returns>
    public static async IAsyncEnumerable<T> Unfold<T, TState>(
        TState seed,
        Func<TState, Option<(T Value, TState NextState)>> generator,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var state = seed;
        while (!cancellationToken.IsCancellationRequested)
        {
            var result = generator(state);
            if (result is Some<(T Value, TState NextState)> some)
            {
                yield return some.Value.Value;
                state = some.Value.NextState;
            }
            else
            {
                yield break;
            }
        }
    }

    /// <summary>
    /// Generates an async sequence from a seed value using an async generator function.
    /// The generator returns <see cref="Option{T}"/> — <c>Some((element, nextSeed))</c> to continue
    /// or <c>None</c> to stop.
    /// </summary>
    /// <typeparam name="T">The element type.</typeparam>
    /// <typeparam name="TState">The seed/state type.</typeparam>
    /// <param name="seed">The initial state.</param>
    /// <param name="generator">An async function that produces the next element and state, or None to stop.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An async sequence of generated elements.</returns>
    public static async IAsyncEnumerable<T> UnfoldAsync<T, TState>(
        TState seed,
        Func<TState, Task<Option<(T Value, TState NextState)>>> generator,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var state = seed;
        while (!cancellationToken.IsCancellationRequested)
        {
            var result = await generator(state).ConfigureAwait(false);
            if (result is Some<(T Value, TState NextState)> some)
            {
                yield return some.Value.Value;
                state = some.Value.NextState;
            }
            else
            {
                yield break;
            }
        }
    }

    // ── Buffer ──

    /// <summary>
    /// Batches elements of an async sequence into chunks of the specified size.
    /// The last batch may contain fewer elements.
    /// </summary>
    /// <typeparam name="T">The element type.</typeparam>
    /// <param name="source">The async sequence to batch.</param>
    /// <param name="size">The maximum number of elements per batch.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An async sequence of batches.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when size is less than 1.</exception>
    public static async IAsyncEnumerable<IReadOnlyList<T>> Buffer<T>(
        this IAsyncEnumerable<T> source,
        int size,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(size, 1);

        var batch = new List<T>(size);
        await foreach (var item in source.WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            batch.Add(item);
            if (batch.Count == size)
            {
                yield return batch;
                batch = new List<T>(size);
            }
        }

        if (batch.Count > 0)
        {
            yield return batch;
        }
    }

    // ── ForEachAsync ──

    /// <summary>
    /// Consumes the async sequence, executing an action for each element.
    /// </summary>
    /// <typeparam name="T">The element type.</typeparam>
    /// <param name="source">The async sequence to consume.</param>
    /// <param name="action">The action to execute for each element.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    public static async Task ForEachAsync<T>(
        this IAsyncEnumerable<T> source,
        Action<T> action,
        CancellationToken cancellationToken = default)
    {
        await foreach (var item in source.WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            action(item);
        }
    }

    /// <summary>
    /// Consumes the async sequence, executing an async action for each element.
    /// </summary>
    /// <typeparam name="T">The element type.</typeparam>
    /// <param name="source">The async sequence to consume.</param>
    /// <param name="action">The async action to execute for each element.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    public static async Task ForEachAsync<T>(
        this IAsyncEnumerable<T> source,
        Func<T, Task> action,
        CancellationToken cancellationToken = default)
    {
        await foreach (var item in source.WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            await action(item).ConfigureAwait(false);
        }
    }
}

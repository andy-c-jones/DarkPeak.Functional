namespace DarkPeak.Functional.Extensions;

/// <summary>
/// Extension methods for working with Task&lt;Option&lt;T&gt;&gt; to enable fluent async chaining.
/// </summary>
public static class TaskOptionExtensions
{
    /// <summary>
    /// Asynchronously transforms the value inside a <see cref="Some{T}"/> using the specified mapping function.
    /// </summary>
    /// <typeparam name="T">The source value type.</typeparam>
    /// <typeparam name="TResult">The result value type.</typeparam>
    /// <param name="task">The task producing an option.</param>
    /// <param name="mapper">The function to apply to the contained value.</param>
    /// <returns>A new option containing the mapped value, or <see cref="None{T}"/> if the source is None.</returns>
    public static async Task<Option<TResult>> Map<T, TResult>(
        this Task<Option<T>> task, Func<T, TResult> mapper) =>
        (await task).Map(mapper);

    /// <summary>
    /// Asynchronously transforms the value inside a <see cref="Some{T}"/> using an async mapping function.
    /// </summary>
    /// <typeparam name="T">The source value type.</typeparam>
    /// <typeparam name="TResult">The result value type.</typeparam>
    /// <param name="task">The task producing an option.</param>
    /// <param name="mapper">The async function to apply to the contained value.</param>
    /// <returns>A new option containing the mapped value, or <see cref="None{T}"/> if the source is None.</returns>
    public static async Task<Option<TResult>> MapAsync<T, TResult>(
        this Task<Option<T>> task, Func<T, Task<TResult>> mapper) =>
        await (await task).MapAsync(mapper);

    /// <summary>
    /// Asynchronously applies a function that returns an option to the value inside a <see cref="Some{T}"/>.
    /// </summary>
    /// <typeparam name="T">The source value type.</typeparam>
    /// <typeparam name="TResult">The result value type.</typeparam>
    /// <param name="task">The task producing an option.</param>
    /// <param name="binder">The function to apply to the contained value.</param>
    /// <returns>The option returned by the binder, or <see cref="None{T}"/> if the source is None.</returns>
    public static async Task<Option<TResult>> Bind<T, TResult>(
        this Task<Option<T>> task, Func<T, Option<TResult>> binder) =>
        (await task).Bind(binder);

    /// <summary>
    /// Asynchronously applies an async function that returns an option to the value inside a <see cref="Some{T}"/>.
    /// </summary>
    /// <typeparam name="T">The source value type.</typeparam>
    /// <typeparam name="TResult">The result value type.</typeparam>
    /// <param name="task">The task producing an option.</param>
    /// <param name="binder">The async function to apply to the contained value.</param>
    /// <returns>The option returned by the binder, or <see cref="None{T}"/> if the source is None.</returns>
    public static async Task<Option<TResult>> BindAsync<T, TResult>(
        this Task<Option<T>> task, Func<T, Task<Option<TResult>>> binder) =>
        await (await task).BindAsync(binder);

    /// <summary>
    /// Asynchronously pattern matches on the option, returning the result of the appropriate function.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <typeparam name="TResult">The result type.</typeparam>
    /// <param name="task">The task producing an option.</param>
    /// <param name="some">The function to invoke if the option contains a value.</param>
    /// <param name="none">The function to invoke if the option is None.</param>
    /// <returns>The result of the matched function.</returns>
    public static async Task<TResult> Match<T, TResult>(
        this Task<Option<T>> task, Func<T, TResult> some, Func<TResult> none) =>
        (await task).Match(some, none);

    /// <summary>
    /// Asynchronously pattern matches on the option using async handler functions.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <typeparam name="TResult">The result type.</typeparam>
    /// <param name="task">The task producing an option.</param>
    /// <param name="some">The async function to invoke if the option contains a value.</param>
    /// <param name="none">The async function to invoke if the option is None.</param>
    /// <returns>The result of the matched async function.</returns>
    public static async Task<TResult> MatchAsync<T, TResult>(
        this Task<Option<T>> task, Func<T, Task<TResult>> some, Func<Task<TResult>> none) =>
        await (await task).MatchAsync(some, none);

    /// <summary>
    /// Asynchronously filters the option, returning None if the predicate is not satisfied.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="task">The task producing an option.</param>
    /// <param name="predicate">The predicate to test the contained value against.</param>
    /// <returns>The original option if Some and the predicate is satisfied; otherwise None.</returns>
    public static async Task<Option<T>> Filter<T>(
        this Task<Option<T>> task, Func<T, bool> predicate) =>
        (await task).Filter(predicate);

    /// <summary>
    /// Asynchronously extracts the value from the option, or returns the specified default.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="task">The task producing an option.</param>
    /// <param name="defaultValue">The value to return if the option is None.</param>
    /// <returns>The contained value, or <paramref name="defaultValue"/> if None.</returns>
    public static async Task<T> GetValueOrDefault<T>(
        this Task<Option<T>> task, T defaultValue) =>
        (await task).GetValueOrDefault(defaultValue);

    /// <summary>
    /// Asynchronously extracts the value from the option, or invokes a factory to produce a default.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="task">The task producing an option.</param>
    /// <param name="defaultFactory">The factory function invoked if the option is None.</param>
    /// <returns>The contained value, or the result of <paramref name="defaultFactory"/> if None.</returns>
    public static async Task<T> GetValueOrDefault<T>(
        this Task<Option<T>> task, Func<T> defaultFactory) =>
        (await task).GetValueOrDefault(defaultFactory);

    /// <summary>
    /// Asynchronously extracts the value from the option, or throws <see cref="InvalidOperationException"/> if None.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="task">The task producing an option.</param>
    /// <returns>The contained value.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the option is None.</exception>
    public static async Task<T> GetValueOrThrow<T>(
        this Task<Option<T>> task) =>
        (await task).GetValueOrThrow();

    /// <summary>
    /// Asynchronously executes a side-effect action on the value if the option is Some.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="task">The task producing an option.</param>
    /// <param name="action">The action to execute on the contained value.</param>
    /// <returns>The original option, unchanged.</returns>
    public static async Task<Option<T>> Tap<T>(
        this Task<Option<T>> task, Action<T> action) =>
        (await task).Tap(action);

    /// <summary>
    /// Asynchronously executes a side-effect action if the option is None.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="task">The task producing an option.</param>
    /// <param name="action">The action to execute when None.</param>
    /// <returns>The original option, unchanged.</returns>
    public static async Task<Option<T>> TapNone<T>(
        this Task<Option<T>> task, Action action) =>
        (await task).TapNone(action);

    /// <summary>
    /// Asynchronously returns the option if it is Some; otherwise returns the specified alternative.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="task">The task producing an option.</param>
    /// <param name="alternative">The alternative option to return if the source is None.</param>
    /// <returns>The original option if Some; otherwise <paramref name="alternative"/>.</returns>
    public static async Task<Option<T>> OrElse<T>(
        this Task<Option<T>> task, Option<T> alternative) =>
        (await task).OrElse(alternative);

    /// <summary>
    /// Asynchronously returns the option if it is Some; otherwise invokes a factory to produce an alternative.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="task">The task producing an option.</param>
    /// <param name="alternativeFactory">The factory function invoked if the source is None.</param>
    /// <returns>The original option if Some; otherwise the result of <paramref name="alternativeFactory"/>.</returns>
    public static async Task<Option<T>> OrElse<T>(
        this Task<Option<T>> task, Func<Option<T>> alternativeFactory) =>
        (await task).OrElse(alternativeFactory);

    /// <summary>
    /// Asynchronously combines two independent Option-producing tasks into a tuple.
    /// Both tasks are awaited concurrently. Returns Some only if both are Some, otherwise None.
    /// </summary>
    /// <typeparam name="T1">The first value type.</typeparam>
    /// <typeparam name="T2">The second value type.</typeparam>
    /// <param name="first">The first task producing an option.</param>
    /// <param name="second">The second task producing an option.</param>
    /// <returns>Some with a tuple of both values, or None.</returns>
    public static async Task<Option<(T1, T2)>> Join<T1, T2>(
        this Task<Option<T1>> first, Task<Option<T2>> second)
    {
        await Task.WhenAll(first, second);
        return first.Result.Join(second.Result);
    }
}

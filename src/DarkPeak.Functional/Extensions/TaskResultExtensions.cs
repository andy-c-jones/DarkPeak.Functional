namespace DarkPeak.Functional.Extensions;

/// <summary>
/// Extension methods for working with Task&lt;Result&lt;T, TError&gt;&gt; to enable fluent async chaining.
/// </summary>
public static class TaskResultExtensions
{
    /// <summary>
    /// Asynchronously transforms the success value using the specified mapping function.
    /// </summary>
    /// <typeparam name="T">The source success type.</typeparam>
    /// <typeparam name="TError">The error type.</typeparam>
    /// <typeparam name="TResult">The result success type.</typeparam>
    /// <param name="task">The task producing a result.</param>
    /// <param name="mapper">The function to apply to the success value.</param>
    /// <returns>A new result with the mapped value, or the original failure.</returns>
    public static async Task<Result<TResult, TError>> Map<T, TError, TResult>(
        this Task<Result<T, TError>> task, Func<T, TResult> mapper) where TError : Error =>
        (await task).Map(mapper);

    /// <summary>
    /// Asynchronously transforms the success value using an async mapping function.
    /// </summary>
    /// <typeparam name="T">The source success type.</typeparam>
    /// <typeparam name="TError">The error type.</typeparam>
    /// <typeparam name="TResult">The result success type.</typeparam>
    /// <param name="task">The task producing a result.</param>
    /// <param name="mapper">The async function to apply to the success value.</param>
    /// <returns>A new result with the mapped value, or the original failure.</returns>
    public static async Task<Result<TResult, TError>> MapAsync<T, TError, TResult>(
        this Task<Result<T, TError>> task, Func<T, Task<TResult>> mapper) where TError : Error =>
        await (await task).MapAsync(mapper);

    /// <summary>
    /// Asynchronously transforms the error value using the specified mapping function.
    /// </summary>
    /// <typeparam name="T">The success type.</typeparam>
    /// <typeparam name="TError">The source error type.</typeparam>
    /// <typeparam name="TErrorResult">The result error type.</typeparam>
    /// <param name="task">The task producing a result.</param>
    /// <param name="mapper">The function to apply to the error value.</param>
    /// <returns>A new result with the mapped error, or the original success.</returns>
    public static async Task<Result<T, TErrorResult>> MapError<T, TError, TErrorResult>(
        this Task<Result<T, TError>> task, Func<TError, TErrorResult> mapper)
        where TError : Error where TErrorResult : Error =>
        (await task).MapError(mapper);

    /// <summary>
    /// Asynchronously applies a function that returns a result to the success value.
    /// </summary>
    /// <typeparam name="T">The source success type.</typeparam>
    /// <typeparam name="TError">The error type.</typeparam>
    /// <typeparam name="TResult">The result success type.</typeparam>
    /// <param name="task">The task producing a result.</param>
    /// <param name="binder">The function to apply to the success value.</param>
    /// <returns>The result returned by the binder, or the original failure.</returns>
    public static async Task<Result<TResult, TError>> Bind<T, TError, TResult>(
        this Task<Result<T, TError>> task, Func<T, Result<TResult, TError>> binder) where TError : Error =>
        (await task).Bind(binder);

    /// <summary>
    /// Asynchronously applies an async function that returns a result to the success value.
    /// </summary>
    /// <typeparam name="T">The source success type.</typeparam>
    /// <typeparam name="TError">The error type.</typeparam>
    /// <typeparam name="TResult">The result success type.</typeparam>
    /// <param name="task">The task producing a result.</param>
    /// <param name="binder">The async function to apply to the success value.</param>
    /// <returns>The result returned by the binder, or the original failure.</returns>
    public static async Task<Result<TResult, TError>> BindAsync<T, TError, TResult>(
        this Task<Result<T, TError>> task, Func<T, Task<Result<TResult, TError>>> binder) where TError : Error =>
        await (await task).BindAsync(binder);

    /// <summary>
    /// Asynchronously pattern matches on the result, returning the output of the appropriate function.
    /// </summary>
    /// <typeparam name="T">The success type.</typeparam>
    /// <typeparam name="TError">The error type.</typeparam>
    /// <typeparam name="TResult">The result type.</typeparam>
    /// <param name="task">The task producing a result.</param>
    /// <param name="success">The function to invoke on success.</param>
    /// <param name="failure">The function to invoke on failure.</param>
    /// <returns>The output of the matched function.</returns>
    public static async Task<TResult> Match<T, TError, TResult>(
        this Task<Result<T, TError>> task, Func<T, TResult> success, Func<TError, TResult> failure) where TError : Error =>
        (await task).Match(success, failure);

    /// <summary>
    /// Asynchronously pattern matches on the result using async handler functions.
    /// </summary>
    /// <typeparam name="T">The success type.</typeparam>
    /// <typeparam name="TError">The error type.</typeparam>
    /// <typeparam name="TResult">The result type.</typeparam>
    /// <param name="task">The task producing a result.</param>
    /// <param name="success">The async function to invoke on success.</param>
    /// <param name="failure">The async function to invoke on failure.</param>
    /// <returns>The output of the matched async function.</returns>
    public static async Task<TResult> MatchAsync<T, TError, TResult>(
        this Task<Result<T, TError>> task, Func<T, Task<TResult>> success, Func<TError, Task<TResult>> failure) where TError : Error =>
        await (await task).MatchAsync(success, failure);

    /// <summary>
    /// Asynchronously executes a side-effect action on the success value.
    /// </summary>
    /// <typeparam name="T">The success type.</typeparam>
    /// <typeparam name="TError">The error type.</typeparam>
    /// <param name="task">The task producing a result.</param>
    /// <param name="action">The action to execute on the success value.</param>
    /// <returns>The original result, unchanged.</returns>
    public static async Task<Result<T, TError>> Tap<T, TError>(
        this Task<Result<T, TError>> task, Action<T> action) where TError : Error =>
        (await task).Tap(action);

    /// <summary>
    /// Asynchronously executes a side-effect action on the error value.
    /// </summary>
    /// <typeparam name="T">The success type.</typeparam>
    /// <typeparam name="TError">The error type.</typeparam>
    /// <param name="task">The task producing a result.</param>
    /// <param name="action">The action to execute on the error value.</param>
    /// <returns>The original result, unchanged.</returns>
    public static async Task<Result<T, TError>> TapError<T, TError>(
        this Task<Result<T, TError>> task, Action<TError> action) where TError : Error =>
        (await task).TapError(action);

    /// <summary>
    /// Asynchronously extracts the success value, or returns the specified default on failure.
    /// </summary>
    /// <typeparam name="T">The success type.</typeparam>
    /// <typeparam name="TError">The error type.</typeparam>
    /// <param name="task">The task producing a result.</param>
    /// <param name="defaultValue">The value to return on failure.</param>
    /// <returns>The success value, or <paramref name="defaultValue"/> on failure.</returns>
    public static async Task<T> GetValueOrDefault<T, TError>(
        this Task<Result<T, TError>> task, T defaultValue) where TError : Error =>
        (await task).GetValueOrDefault(defaultValue);

    /// <summary>
    /// Asynchronously extracts the success value, or invokes a factory to produce a default on failure.
    /// </summary>
    /// <typeparam name="T">The success type.</typeparam>
    /// <typeparam name="TError">The error type.</typeparam>
    /// <param name="task">The task producing a result.</param>
    /// <param name="defaultFactory">The factory function invoked on failure.</param>
    /// <returns>The success value, or the result of <paramref name="defaultFactory"/> on failure.</returns>
    public static async Task<T> GetValueOrDefault<T, TError>(
        this Task<Result<T, TError>> task, Func<T> defaultFactory) where TError : Error =>
        (await task).GetValueOrDefault(defaultFactory);

    /// <summary>
    /// Asynchronously extracts the success value, or throws <see cref="InvalidOperationException"/> on failure.
    /// </summary>
    /// <typeparam name="T">The success type.</typeparam>
    /// <typeparam name="TError">The error type.</typeparam>
    /// <param name="task">The task producing a result.</param>
    /// <returns>The success value.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the result is a failure.</exception>
    public static async Task<T> GetValueOrThrow<T, TError>(
        this Task<Result<T, TError>> task) where TError : Error =>
        (await task).GetValueOrThrow();

    /// <summary>
    /// Asynchronously returns the result if it is a success; otherwise returns the specified alternative.
    /// </summary>
    /// <typeparam name="T">The success type.</typeparam>
    /// <typeparam name="TError">The error type.</typeparam>
    /// <param name="task">The task producing a result.</param>
    /// <param name="alternative">The alternative result to return on failure.</param>
    /// <returns>The original result if success; otherwise <paramref name="alternative"/>.</returns>
    public static async Task<Result<T, TError>> OrElse<T, TError>(
        this Task<Result<T, TError>> task, Result<T, TError> alternative) where TError : Error =>
        (await task).OrElse(alternative);

    /// <summary>
    /// Asynchronously returns the result if it is a success; otherwise invokes a factory to produce an alternative.
    /// </summary>
    /// <typeparam name="T">The success type.</typeparam>
    /// <typeparam name="TError">The error type.</typeparam>
    /// <param name="task">The task producing a result.</param>
    /// <param name="alternativeFactory">The factory function invoked on failure.</param>
    /// <returns>The original result if success; otherwise the result of <paramref name="alternativeFactory"/>.</returns>
    public static async Task<Result<T, TError>> OrElse<T, TError>(
        this Task<Result<T, TError>> task, Func<Result<T, TError>> alternativeFactory) where TError : Error =>
        (await task).OrElse(alternativeFactory);

    /// <summary>
    /// Asynchronously combines two independent Result-producing tasks into a tuple.
    /// Both tasks are awaited concurrently. Fails fast on the first failure.
    /// </summary>
    /// <typeparam name="T1">The first success type.</typeparam>
    /// <typeparam name="T2">The second success type.</typeparam>
    /// <typeparam name="TError">The error type.</typeparam>
    /// <param name="first">The first task producing a result.</param>
    /// <param name="second">The second task producing a result.</param>
    /// <returns>Success with a tuple of both values, or the first Failure.</returns>
    public static async Task<Result<(T1, T2), TError>> Join<T1, T2, TError>(
        this Task<Result<T1, TError>> first, Task<Result<T2, TError>> second) where TError : Error
    {
        await Task.WhenAll(first, second);
        return first.Result.Join(second.Result);
    }
}

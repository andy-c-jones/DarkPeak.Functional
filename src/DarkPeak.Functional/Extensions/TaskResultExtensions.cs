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
    public static async Task<Result<(T1, T2), TError>> Join<T1, T2, TError>(
        this Task<Result<T1, TError>> first, Task<Result<T2, TError>> second) where TError : Error
    {
        await Task.WhenAll(first, second);
        return first.Result.Join(second.Result);
    }

    /// <summary>
    /// Asynchronously combines three independent Result-producing tasks into a tuple.
    /// All tasks are awaited concurrently. Fails fast on the first failure.
    /// </summary>
    public static async Task<Result<(T1, T2, T3), TError>> Join<T1, T2, T3, TError>(
        this Task<Result<T1, TError>> first, Task<Result<T2, TError>> second,
        Task<Result<T3, TError>> third) where TError : Error
    {
        await Task.WhenAll(first, second, third);
        return first.Result.Join(second.Result, third.Result);
    }

    /// <summary>
    /// Asynchronously combines four independent Result-producing tasks into a tuple.
    /// All tasks are awaited concurrently. Fails fast on the first failure.
    /// </summary>
    public static async Task<Result<(T1, T2, T3, T4), TError>> Join<T1, T2, T3, T4, TError>(
        this Task<Result<T1, TError>> first, Task<Result<T2, TError>> second,
        Task<Result<T3, TError>> third, Task<Result<T4, TError>> fourth) where TError : Error
    {
        await Task.WhenAll(first, second, third, fourth);
        return first.Result.Join(second.Result, third.Result, fourth.Result);
    }

    /// <summary>
    /// Asynchronously combines five independent Result-producing tasks into a tuple.
    /// All tasks are awaited concurrently. Fails fast on the first failure.
    /// </summary>
    public static async Task<Result<(T1, T2, T3, T4, T5), TError>> Join<T1, T2, T3, T4, T5, TError>(
        this Task<Result<T1, TError>> first, Task<Result<T2, TError>> second,
        Task<Result<T3, TError>> third, Task<Result<T4, TError>> fourth,
        Task<Result<T5, TError>> fifth) where TError : Error
    {
        await Task.WhenAll(first, second, third, fourth, fifth);
        return first.Result.Join(second.Result, third.Result, fourth.Result, fifth.Result);
    }

    /// <summary>
    /// Asynchronously combines six independent Result-producing tasks into a tuple.
    /// All tasks are awaited concurrently. Fails fast on the first failure.
    /// </summary>
    public static async Task<Result<(T1, T2, T3, T4, T5, T6), TError>> Join<T1, T2, T3, T4, T5, T6, TError>(
        this Task<Result<T1, TError>> first, Task<Result<T2, TError>> second,
        Task<Result<T3, TError>> third, Task<Result<T4, TError>> fourth,
        Task<Result<T5, TError>> fifth, Task<Result<T6, TError>> sixth) where TError : Error
    {
        await Task.WhenAll(first, second, third, fourth, fifth, sixth);
        return first.Result.Join(second.Result, third.Result, fourth.Result, fifth.Result, sixth.Result);
    }

    /// <summary>
    /// Asynchronously combines seven independent Result-producing tasks into a tuple.
    /// All tasks are awaited concurrently. Fails fast on the first failure.
    /// </summary>
    public static async Task<Result<(T1, T2, T3, T4, T5, T6, T7), TError>> Join<T1, T2, T3, T4, T5, T6, T7, TError>(
        this Task<Result<T1, TError>> first, Task<Result<T2, TError>> second,
        Task<Result<T3, TError>> third, Task<Result<T4, TError>> fourth,
        Task<Result<T5, TError>> fifth, Task<Result<T6, TError>> sixth,
        Task<Result<T7, TError>> seventh) where TError : Error
    {
        await Task.WhenAll(first, second, third, fourth, fifth, sixth, seventh);
        return first.Result.Join(second.Result, third.Result, fourth.Result, fifth.Result, sixth.Result, seventh.Result);
    }

    /// <summary>
    /// Asynchronously combines eight independent Result-producing tasks into a tuple.
    /// All tasks are awaited concurrently. Fails fast on the first failure.
    /// </summary>
    public static async Task<Result<(T1, T2, T3, T4, T5, T6, T7, T8), TError>> Join<T1, T2, T3, T4, T5, T6, T7, T8, TError>(
        this Task<Result<T1, TError>> first, Task<Result<T2, TError>> second,
        Task<Result<T3, TError>> third, Task<Result<T4, TError>> fourth,
        Task<Result<T5, TError>> fifth, Task<Result<T6, TError>> sixth,
        Task<Result<T7, TError>> seventh, Task<Result<T8, TError>> eighth) where TError : Error
    {
        await Task.WhenAll(first, second, third, fourth, fifth, sixth, seventh, eighth);
        return first.Result.Join(second.Result, third.Result, fourth.Result, fifth.Result, sixth.Result, seventh.Result, eighth.Result);
    }
}

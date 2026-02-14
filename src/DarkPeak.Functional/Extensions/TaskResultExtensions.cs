namespace DarkPeak.Functional.Extensions;

/// <summary>
/// Extension methods for working with Task&lt;Result&lt;T, TError&gt;&gt; to enable fluent async chaining.
/// </summary>
public static class TaskResultExtensions
{
    public static async Task<Result<TResult, TError>> Map<T, TError, TResult>(
        this Task<Result<T, TError>> task, Func<T, TResult> mapper) where TError : Error =>
        (await task).Map(mapper);

    public static async Task<Result<TResult, TError>> MapAsync<T, TError, TResult>(
        this Task<Result<T, TError>> task, Func<T, Task<TResult>> mapper) where TError : Error =>
        await (await task).MapAsync(mapper);

    public static async Task<Result<T, TErrorResult>> MapError<T, TError, TErrorResult>(
        this Task<Result<T, TError>> task, Func<TError, TErrorResult> mapper)
        where TError : Error where TErrorResult : Error =>
        (await task).MapError(mapper);

    public static async Task<Result<TResult, TError>> Bind<T, TError, TResult>(
        this Task<Result<T, TError>> task, Func<T, Result<TResult, TError>> binder) where TError : Error =>
        (await task).Bind(binder);

    public static async Task<Result<TResult, TError>> BindAsync<T, TError, TResult>(
        this Task<Result<T, TError>> task, Func<T, Task<Result<TResult, TError>>> binder) where TError : Error =>
        await (await task).BindAsync(binder);

    public static async Task<TResult> Match<T, TError, TResult>(
        this Task<Result<T, TError>> task, Func<T, TResult> success, Func<TError, TResult> failure) where TError : Error =>
        (await task).Match(success, failure);

    public static async Task<TResult> MatchAsync<T, TError, TResult>(
        this Task<Result<T, TError>> task, Func<T, Task<TResult>> success, Func<TError, Task<TResult>> failure) where TError : Error =>
        await (await task).MatchAsync(success, failure);

    public static async Task<Result<T, TError>> Tap<T, TError>(
        this Task<Result<T, TError>> task, Action<T> action) where TError : Error =>
        (await task).Tap(action);

    public static async Task<Result<T, TError>> TapError<T, TError>(
        this Task<Result<T, TError>> task, Action<TError> action) where TError : Error =>
        (await task).TapError(action);

    public static async Task<T> GetValueOrDefault<T, TError>(
        this Task<Result<T, TError>> task, T defaultValue) where TError : Error =>
        (await task).GetValueOrDefault(defaultValue);

    public static async Task<T> GetValueOrDefault<T, TError>(
        this Task<Result<T, TError>> task, Func<T> defaultFactory) where TError : Error =>
        (await task).GetValueOrDefault(defaultFactory);

    public static async Task<T> GetValueOrThrow<T, TError>(
        this Task<Result<T, TError>> task) where TError : Error =>
        (await task).GetValueOrThrow();

    public static async Task<Result<T, TError>> OrElse<T, TError>(
        this Task<Result<T, TError>> task, Result<T, TError> alternative) where TError : Error =>
        (await task).OrElse(alternative);

    public static async Task<Result<T, TError>> OrElse<T, TError>(
        this Task<Result<T, TError>> task, Func<Result<T, TError>> alternativeFactory) where TError : Error =>
        (await task).OrElse(alternativeFactory);
}

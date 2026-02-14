namespace DarkPeak.Functional.Extensions;

/// <summary>
/// Extension methods for working with Task&lt;Option&lt;T&gt;&gt; to enable fluent async chaining.
/// </summary>
public static class TaskOptionExtensions
{
    public static async Task<Option<TResult>> Map<T, TResult>(
        this Task<Option<T>> task, Func<T, TResult> mapper) =>
        (await task).Map(mapper);

    public static async Task<Option<TResult>> MapAsync<T, TResult>(
        this Task<Option<T>> task, Func<T, Task<TResult>> mapper) =>
        await (await task).MapAsync(mapper);

    public static async Task<Option<TResult>> Bind<T, TResult>(
        this Task<Option<T>> task, Func<T, Option<TResult>> binder) =>
        (await task).Bind(binder);

    public static async Task<Option<TResult>> BindAsync<T, TResult>(
        this Task<Option<T>> task, Func<T, Task<Option<TResult>>> binder) =>
        await (await task).BindAsync(binder);

    public static async Task<TResult> Match<T, TResult>(
        this Task<Option<T>> task, Func<T, TResult> some, Func<TResult> none) =>
        (await task).Match(some, none);

    public static async Task<TResult> MatchAsync<T, TResult>(
        this Task<Option<T>> task, Func<T, Task<TResult>> some, Func<Task<TResult>> none) =>
        await (await task).MatchAsync(some, none);

    public static async Task<Option<T>> Filter<T>(
        this Task<Option<T>> task, Func<T, bool> predicate) =>
        (await task).Filter(predicate);

    public static async Task<T> GetValueOrDefault<T>(
        this Task<Option<T>> task, T defaultValue) =>
        (await task).GetValueOrDefault(defaultValue);

    public static async Task<T> GetValueOrDefault<T>(
        this Task<Option<T>> task, Func<T> defaultFactory) =>
        (await task).GetValueOrDefault(defaultFactory);

    public static async Task<T> GetValueOrThrow<T>(
        this Task<Option<T>> task) =>
        (await task).GetValueOrThrow();

    public static async Task<Option<T>> Tap<T>(
        this Task<Option<T>> task, Action<T> action) =>
        (await task).Tap(action);

    public static async Task<Option<T>> TapNone<T>(
        this Task<Option<T>> task, Action action) =>
        (await task).TapNone(action);

    public static async Task<Option<T>> OrElse<T>(
        this Task<Option<T>> task, Option<T> alternative) =>
        (await task).OrElse(alternative);

    public static async Task<Option<T>> OrElse<T>(
        this Task<Option<T>> task, Func<Option<T>> alternativeFactory) =>
        (await task).OrElse(alternativeFactory);
}

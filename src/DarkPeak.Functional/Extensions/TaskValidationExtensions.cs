namespace DarkPeak.Functional.Extensions;

/// <summary>
/// Extension methods for working with Task&lt;Validation&lt;T, TError&gt;&gt; to enable fluent async chaining.
/// </summary>
public static class TaskValidationExtensions
{
    // --- Chaining ---

    /// <summary>
    /// Asynchronously transforms the valid value using the specified mapping function.
    /// </summary>
    public static async Task<Validation<TResult, TError>> Map<T, TResult, TError>(
        this Task<Validation<T, TError>> task, Func<T, TResult> mapper) where TError : Error =>
        (await task).Map(mapper);

    /// <summary>
    /// Asynchronously transforms the valid value using an async mapping function.
    /// </summary>
    public static async Task<Validation<TResult, TError>> MapAsync<T, TResult, TError>(
        this Task<Validation<T, TError>> task, Func<T, Task<TResult>> mapper) where TError : Error =>
        await (await task).MapAsync(mapper);

    /// <summary>
    /// Asynchronously applies a function that returns a validation to the valid value (short-circuits on failure).
    /// </summary>
    public static async Task<Validation<TResult, TError>> Bind<T, TResult, TError>(
        this Task<Validation<T, TError>> task, Func<T, Validation<TResult, TError>> binder) where TError : Error =>
        (await task).Bind(binder);

    /// <summary>
    /// Asynchronously applies an async function that returns a validation to the valid value (short-circuits on failure).
    /// </summary>
    public static async Task<Validation<TResult, TError>> BindAsync<T, TResult, TError>(
        this Task<Validation<T, TError>> task, Func<T, Task<Validation<TResult, TError>>> binder) where TError : Error =>
        await (await task).BindAsync(binder);

    /// <summary>
    /// Asynchronously pattern matches on the validation, returning the output of the appropriate function.
    /// </summary>
    public static async Task<TResult> Match<T, TError, TResult>(
        this Task<Validation<T, TError>> task, Func<T, TResult> valid,
        Func<IReadOnlyList<TError>, TResult> invalid) where TError : Error =>
        (await task).Match(valid, invalid);

    /// <summary>
    /// Asynchronously pattern matches on the validation using async handler functions.
    /// </summary>
    public static async Task<TResult> MatchAsync<T, TError, TResult>(
        this Task<Validation<T, TError>> task, Func<T, Task<TResult>> valid,
        Func<IReadOnlyList<TError>, Task<TResult>> invalid) where TError : Error =>
        await (await task).MatchAsync(valid, invalid);

    /// <summary>
    /// Asynchronously executes a side-effect action on the valid value.
    /// </summary>
    public static async Task<Validation<T, TError>> Tap<T, TError>(
        this Task<Validation<T, TError>> task, Action<T> action) where TError : Error =>
        (await task).Tap(action);

    /// <summary>
    /// Asynchronously executes a side-effect action on the errors if invalid.
    /// </summary>
    public static async Task<Validation<T, TError>> TapInvalid<T, TError>(
        this Task<Validation<T, TError>> task, Action<IReadOnlyList<TError>> action) where TError : Error =>
        (await task).TapInvalid(action);

    /// <summary>
    /// Asynchronously extracts the valid value, or returns the specified default on failure.
    /// </summary>
    public static async Task<T> GetValueOrDefault<T, TError>(
        this Task<Validation<T, TError>> task, T defaultValue) where TError : Error =>
        (await task).GetValueOrDefault(defaultValue);

    /// <summary>
    /// Asynchronously extracts the valid value, or invokes a factory to produce a default on failure.
    /// </summary>
    public static async Task<T> GetValueOrDefault<T, TError>(
        this Task<Validation<T, TError>> task, Func<T> defaultFactory) where TError : Error =>
        (await task).GetValueOrDefault(defaultFactory);

    /// <summary>
    /// Asynchronously extracts the valid value, or throws <see cref="InvalidOperationException"/> on failure.
    /// </summary>
    public static async Task<T> GetValueOrThrow<T, TError>(
        this Task<Validation<T, TError>> task) where TError : Error =>
        (await task).GetValueOrThrow();

    /// <summary>
    /// Asynchronously converts a Validation to a Result, taking the first error if invalid.
    /// </summary>
    public static async Task<Result<T, TError>> ToResult<T, TError>(
        this Task<Validation<T, TError>> task) where TError : Error =>
        (await task).ToResult();

    // --- ZipWithAsync (concurrent via Task.WhenAll, error-accumulating) ---

    /// <summary>
    /// Combines two async validations with a projection function, running concurrently.
    /// Accumulates errors from both if either fails.
    /// </summary>
    public static async Task<Validation<TResult, TError>> ZipWithAsync<T1, T2, TResult, TError>(
        this Task<Validation<T1, TError>> first,
        Task<Validation<T2, TError>> second,
        Func<T1, T2, TResult> combiner) where TError : Error
    {
        await Task.WhenAll(first, second);
        return first.Result.ZipWith(second.Result, combiner);
    }

    /// <summary>
    /// Combines three async validations with a projection function, running concurrently.
    /// Accumulates errors from all.
    /// </summary>
    public static async Task<Validation<TResult, TError>> ZipWithAsync<T1, T2, T3, TResult, TError>(
        this Task<Validation<T1, TError>> first,
        Task<Validation<T2, TError>> second,
        Task<Validation<T3, TError>> third,
        Func<T1, T2, T3, TResult> combiner) where TError : Error
    {
        await Task.WhenAll(first, second, third);
        return first.Result.ZipWith(second.Result, third.Result, combiner);
    }

    /// <summary>
    /// Combines four async validations with a projection function, running concurrently.
    /// Accumulates errors from all.
    /// </summary>
    public static async Task<Validation<TResult, TError>> ZipWithAsync<T1, T2, T3, T4, TResult, TError>(
        this Task<Validation<T1, TError>> first,
        Task<Validation<T2, TError>> second,
        Task<Validation<T3, TError>> third,
        Task<Validation<T4, TError>> fourth,
        Func<T1, T2, T3, T4, TResult> combiner) where TError : Error
    {
        await Task.WhenAll(first, second, third, fourth);
        return first.Result.ZipWith(second.Result, third.Result, fourth.Result, combiner);
    }

    /// <summary>
    /// Combines five async validations with a projection function, running concurrently.
    /// Accumulates errors from all.
    /// </summary>
    public static async Task<Validation<TResult, TError>> ZipWithAsync<T1, T2, T3, T4, T5, TResult, TError>(
        this Task<Validation<T1, TError>> first,
        Task<Validation<T2, TError>> second,
        Task<Validation<T3, TError>> third,
        Task<Validation<T4, TError>> fourth,
        Task<Validation<T5, TError>> fifth,
        Func<T1, T2, T3, T4, T5, TResult> combiner) where TError : Error
    {
        await Task.WhenAll(first, second, third, fourth, fifth);
        return first.Result.ZipWith(second.Result, third.Result, fourth.Result, fifth.Result, combiner);
    }

    /// <summary>
    /// Combines six async validations with a projection function, running concurrently.
    /// Accumulates errors from all.
    /// </summary>
    public static async Task<Validation<TResult, TError>> ZipWithAsync<T1, T2, T3, T4, T5, T6, TResult, TError>(
        this Task<Validation<T1, TError>> first,
        Task<Validation<T2, TError>> second,
        Task<Validation<T3, TError>> third,
        Task<Validation<T4, TError>> fourth,
        Task<Validation<T5, TError>> fifth,
        Task<Validation<T6, TError>> sixth,
        Func<T1, T2, T3, T4, T5, T6, TResult> combiner) where TError : Error
    {
        await Task.WhenAll(first, second, third, fourth, fifth, sixth);
        return first.Result.ZipWith(second.Result, third.Result, fourth.Result, fifth.Result, sixth.Result, combiner);
    }

    /// <summary>
    /// Combines seven async validations with a projection function, running concurrently.
    /// Accumulates errors from all.
    /// </summary>
    public static async Task<Validation<TResult, TError>> ZipWithAsync<T1, T2, T3, T4, T5, T6, T7, TResult, TError>(
        this Task<Validation<T1, TError>> first,
        Task<Validation<T2, TError>> second,
        Task<Validation<T3, TError>> third,
        Task<Validation<T4, TError>> fourth,
        Task<Validation<T5, TError>> fifth,
        Task<Validation<T6, TError>> sixth,
        Task<Validation<T7, TError>> seventh,
        Func<T1, T2, T3, T4, T5, T6, T7, TResult> combiner) where TError : Error
    {
        await Task.WhenAll(first, second, third, fourth, fifth, sixth, seventh);
        return first.Result.ZipWith(second.Result, third.Result, fourth.Result, fifth.Result, sixth.Result, seventh.Result, combiner);
    }

    /// <summary>
    /// Combines eight async validations with a projection function, running concurrently.
    /// Accumulates errors from all.
    /// </summary>
    public static async Task<Validation<TResult, TError>> ZipWithAsync<T1, T2, T3, T4, T5, T6, T7, T8, TResult, TError>(
        this Task<Validation<T1, TError>> first,
        Task<Validation<T2, TError>> second,
        Task<Validation<T3, TError>> third,
        Task<Validation<T4, TError>> fourth,
        Task<Validation<T5, TError>> fifth,
        Task<Validation<T6, TError>> sixth,
        Task<Validation<T7, TError>> seventh,
        Task<Validation<T8, TError>> eighth,
        Func<T1, T2, T3, T4, T5, T6, T7, T8, TResult> combiner) where TError : Error
    {
        await Task.WhenAll(first, second, third, fourth, fifth, sixth, seventh, eighth);
        return first.Result.ZipWith(second.Result, third.Result, fourth.Result, fifth.Result, sixth.Result, seventh.Result, eighth.Result, combiner);
    }

    // --- JoinAsync (concurrent via Task.WhenAll, error-accumulating) ---

    /// <summary>
    /// Combines two async validations into a tuple, running concurrently. Accumulates errors from both.
    /// </summary>
    public static async Task<Validation<(T1, T2), TError>> Join<T1, T2, TError>(
        this Task<Validation<T1, TError>> first, Task<Validation<T2, TError>> second)
        where TError : Error
    {
        await Task.WhenAll(first, second);
        return first.Result.Join(second.Result);
    }

    /// <summary>
    /// Combines three async validations into a tuple, running concurrently. Accumulates errors from all.
    /// </summary>
    public static async Task<Validation<(T1, T2, T3), TError>> Join<T1, T2, T3, TError>(
        this Task<Validation<T1, TError>> first, Task<Validation<T2, TError>> second,
        Task<Validation<T3, TError>> third)
        where TError : Error
    {
        await Task.WhenAll(first, second, third);
        return first.Result.Join(second.Result, third.Result);
    }

    /// <summary>
    /// Combines four async validations into a tuple, running concurrently. Accumulates errors from all.
    /// </summary>
    public static async Task<Validation<(T1, T2, T3, T4), TError>> Join<T1, T2, T3, T4, TError>(
        this Task<Validation<T1, TError>> first, Task<Validation<T2, TError>> second,
        Task<Validation<T3, TError>> third, Task<Validation<T4, TError>> fourth)
        where TError : Error
    {
        await Task.WhenAll(first, second, third, fourth);
        return first.Result.Join(second.Result, third.Result, fourth.Result);
    }

    /// <summary>
    /// Combines five async validations into a tuple, running concurrently. Accumulates errors from all.
    /// </summary>
    public static async Task<Validation<(T1, T2, T3, T4, T5), TError>> Join<T1, T2, T3, T4, T5, TError>(
        this Task<Validation<T1, TError>> first, Task<Validation<T2, TError>> second,
        Task<Validation<T3, TError>> third, Task<Validation<T4, TError>> fourth,
        Task<Validation<T5, TError>> fifth)
        where TError : Error
    {
        await Task.WhenAll(first, second, third, fourth, fifth);
        return first.Result.Join(second.Result, third.Result, fourth.Result, fifth.Result);
    }

    /// <summary>
    /// Combines six async validations into a tuple, running concurrently. Accumulates errors from all.
    /// </summary>
    public static async Task<Validation<(T1, T2, T3, T4, T5, T6), TError>> Join<T1, T2, T3, T4, T5, T6, TError>(
        this Task<Validation<T1, TError>> first, Task<Validation<T2, TError>> second,
        Task<Validation<T3, TError>> third, Task<Validation<T4, TError>> fourth,
        Task<Validation<T5, TError>> fifth, Task<Validation<T6, TError>> sixth)
        where TError : Error
    {
        await Task.WhenAll(first, second, third, fourth, fifth, sixth);
        return first.Result.Join(second.Result, third.Result, fourth.Result, fifth.Result, sixth.Result);
    }

    /// <summary>
    /// Combines seven async validations into a tuple, running concurrently. Accumulates errors from all.
    /// </summary>
    public static async Task<Validation<(T1, T2, T3, T4, T5, T6, T7), TError>> Join<T1, T2, T3, T4, T5, T6, T7, TError>(
        this Task<Validation<T1, TError>> first, Task<Validation<T2, TError>> second,
        Task<Validation<T3, TError>> third, Task<Validation<T4, TError>> fourth,
        Task<Validation<T5, TError>> fifth, Task<Validation<T6, TError>> sixth,
        Task<Validation<T7, TError>> seventh)
        where TError : Error
    {
        await Task.WhenAll(first, second, third, fourth, fifth, sixth, seventh);
        return first.Result.Join(second.Result, third.Result, fourth.Result, fifth.Result, sixth.Result, seventh.Result);
    }

    /// <summary>
    /// Combines eight async validations into a tuple, running concurrently. Accumulates errors from all.
    /// </summary>
    public static async Task<Validation<(T1, T2, T3, T4, T5, T6, T7, T8), TError>> Join<T1, T2, T3, T4, T5, T6, T7, T8, TError>(
        this Task<Validation<T1, TError>> first, Task<Validation<T2, TError>> second,
        Task<Validation<T3, TError>> third, Task<Validation<T4, TError>> fourth,
        Task<Validation<T5, TError>> fifth, Task<Validation<T6, TError>> sixth,
        Task<Validation<T7, TError>> seventh, Task<Validation<T8, TError>> eighth)
        where TError : Error
    {
        await Task.WhenAll(first, second, third, fourth, fifth, sixth, seventh, eighth);
        return first.Result.Join(second.Result, third.Result, fourth.Result, fifth.Result, sixth.Result, seventh.Result, eighth.Result);
    }

    // --- Async Sequential ---

    /// <summary>
    /// Awaits a sequence of tasks producing Validations one by one, accumulating all errors.
    /// Returns Valid with all values if all succeed, or Invalid with all accumulated errors.
    /// </summary>
    public static async Task<Validation<IEnumerable<T>, TError>> SequenceAsync<T, TError>(
        this IEnumerable<Task<Validation<T, TError>>> tasks)
        where TError : Error
    {
        var values = new List<T>();
        var errors = new List<TError>();

        foreach (var task in tasks)
        {
            var validation = await task;
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
    /// Applies an async Validation-returning function to each element sequentially, accumulating all errors.
    /// Returns Valid with all mapped values if all succeed, or Invalid with all accumulated errors.
    /// </summary>
    public static async Task<Validation<IEnumerable<TResult>, TError>> TraverseAsync<T, TResult, TError>(
        this IEnumerable<T> source, Func<T, Task<Validation<TResult, TError>>> func)
        where TError : Error
    {
        var values = new List<TResult>();
        var errors = new List<TError>();

        foreach (var item in source)
        {
            var validation = await func(item);
            validation.Match<object?>(
                valid: v => { values.Add(v); return null; },
                invalid: errs => { errors.AddRange(errs); return null; }
            );
        }

        return errors.Count > 0
            ? new Invalid<IEnumerable<TResult>, TError>(errors.AsReadOnly())
            : new Valid<IEnumerable<TResult>, TError>(values);
    }

    // --- Async Parallel ---

    /// <summary>
    /// Awaits all tasks concurrently, then sequences the validation results, accumulating all errors.
    /// </summary>
    public static async Task<Validation<IEnumerable<T>, TError>> SequenceParallel<T, TError>(
        this IEnumerable<Task<Validation<T, TError>>> tasks)
        where TError : Error
    {
        var results = await Task.WhenAll(tasks);
        return results.Sequence();
    }

    /// <summary>
    /// Applies an async Validation-returning function to all elements concurrently, then sequences the results.
    /// Accumulates all errors.
    /// </summary>
    public static async Task<Validation<IEnumerable<TResult>, TError>> TraverseParallel<T, TResult, TError>(
        this IEnumerable<T> source, Func<T, Task<Validation<TResult, TError>>> func)
        where TError : Error
    {
        var results = await Task.WhenAll(source.Select(func));
        return results.Sequence();
    }
}

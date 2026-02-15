namespace DarkPeak.Functional.Extensions;

/// <summary>
/// Extension methods for working with Result types.
/// </summary>
public static class ResultExtensions
{
    /// <summary>
    /// Executes a function and wraps the result or any exception in a Result.
    /// </summary>
    /// <typeparam name="T">The return type.</typeparam>
    /// <param name="func">The function to execute.</param>
    /// <returns>Success with the value, or Failure with an InternalError if an exception occurs.</returns>
    public static Result<T, Error> ToResult<T>(Func<T> func)
    {
        try
        {
            return Result.Success<T, Error>(func());
        }
        catch (Exception ex)
        {
            return Result.Failure<T, Error>(new InternalError
            {
                Message = ex.Message,
                ExceptionType = ex.GetType().Name,
                Metadata = new Dictionary<string, object>
                {
                    ["StackTrace"] = ex.StackTrace ?? string.Empty
                }
            });
        }
    }

    /// <summary>
    /// Executes an async function and wraps the result or any exception in a Result.
    /// </summary>
    /// <typeparam name="T">The return type.</typeparam>
    /// <param name="func">The async function to execute.</param>
    /// <returns>A task containing Success with the value, or Failure with an InternalError if an exception occurs.</returns>
    public static async Task<Result<T, Error>> ToResultAsync<T>(Func<Task<T>> func)
    {
        try
        {
            return Result.Success<T, Error>(await func());
        }
        catch (Exception ex)
        {
            return Result.Failure<T, Error>(new InternalError
            {
                Message = ex.Message,
                ExceptionType = ex.GetType().Name,
                Metadata = new Dictionary<string, object>
                {
                    ["StackTrace"] = ex.StackTrace ?? string.Empty
                }
            });
        }
    }

    /// <summary>
    /// Converts a sequence of results into a single result containing a list of values.
    /// If any result is a failure, returns the first failure (fail-fast).
    /// </summary>
    /// <typeparam name="T">The type of the values.</typeparam>
    /// <typeparam name="TError">The error type.</typeparam>
    /// <param name="results">The results to sequence.</param>
    /// <returns>Success with all values, or the first Failure encountered.</returns>
    public static Result<IEnumerable<T>, TError> Sequence<T, TError>(this IEnumerable<Result<T, TError>> results)
        where TError : Error
    {
        var values = new List<T>();
        
        foreach (var result in results)
        {
            if (result.IsFailure)
            {
                return result.Match<Result<IEnumerable<T>, TError>>(
                    success: _ => throw new InvalidOperationException("Unexpected success"),
                    failure: error => Result.Failure<IEnumerable<T>, TError>(error)
                );
            }
            
            values.Add(result.Match(
                success: value => value,
                failure: _ => throw new InvalidOperationException("Unexpected failure")
            ));
        }
        
        return Result.Success<IEnumerable<T>, TError>(values);
    }

    /// <summary>
    /// Collects all errors from a sequence of results.
    /// Returns Success with all values if all succeed, or Failure with all collected errors.
    /// </summary>
    /// <typeparam name="T">The type of the values.</typeparam>
    /// <param name="results">The results to process.</param>
    /// <returns>Success with all values, or Failure with aggregated ValidationError.</returns>
    public static Result<IEnumerable<T>, ValidationError> CollectErrors<T>(
        this IEnumerable<Result<T, ValidationError>> results)
    {
        var values = new List<T>();
        var errors = new List<ValidationError>();
        
        foreach (var result in results)
        {
            if (result.IsSuccess)
            {
                values.Add(result.Match(success: v => v, failure: _ => default!));
            }
            else
            {
                errors.Add(result.Match(success: _ => default!, failure: e => e));
            }
        }
        
        if (errors.Count > 0)
        {
            return Result.Failure<IEnumerable<T>, ValidationError>(new ValidationError
            {
                Message = $"{errors.Count} validation error(s) occurred",
                Errors = errors
                    .SelectMany(e => e.Errors ?? new Dictionary<string, string[]>())
                    .GroupBy(kvp => kvp.Key)
                    .ToDictionary(
                        g => g.Key,
                        g => g.SelectMany(kvp => kvp.Value).ToArray()
                    )
            });
        }
        
        return Result.Success<IEnumerable<T>, ValidationError>(values);
    }

    /// <summary>
    /// Filters out failures from a sequence of results and unwraps the success values.
    /// </summary>
    /// <typeparam name="T">The type of the values.</typeparam>
    /// <typeparam name="TError">The error type.</typeparam>
    /// <param name="source">The sequence of results.</param>
    /// <returns>A sequence containing only the unwrapped success values.</returns>
    public static IEnumerable<T> Choose<T, TError>(this IEnumerable<Result<T, TError>> source)
        where TError : Error
    {
        foreach (var result in source)
        {
            if (result.IsSuccess)
            {
                yield return result.Match(
                    success: value => value,
                    failure: _ => throw new InvalidOperationException("Unexpected failure")
                );
            }
        }
    }

    /// <summary>
    /// Partitions a sequence of results into successes and failures.
    /// </summary>
    /// <typeparam name="T">The type of the values.</typeparam>
    /// <typeparam name="TError">The error type.</typeparam>
    /// <param name="source">The sequence of results.</param>
    /// <returns>A tuple of (successes, failures).</returns>
    public static (IEnumerable<T> Successes, IEnumerable<TError> Failures) Partition<T, TError>(
        this IEnumerable<Result<T, TError>> source)
        where TError : Error
    {
        var successes = new List<T>();
        var failures = new List<TError>();
        
        foreach (var result in source)
        {
            if (result.IsSuccess)
            {
                successes.Add(result.Match(success: v => v, failure: _ => default!));
            }
            else
            {
                failures.Add(result.Match(success: _ => default!, failure: e => e));
            }
        }
        
        return (successes, failures);
    }

    // --- Traverse ---

    /// <summary>
    /// Applies a Result-returning function to each element, then sequences the results.
    /// Returns Success with all mapped values if all succeed, or the first Failure encountered (fail-fast).
    /// </summary>
    /// <typeparam name="T">The source element type.</typeparam>
    /// <typeparam name="TResult">The mapped success type.</typeparam>
    /// <typeparam name="TError">The error type.</typeparam>
    /// <param name="source">The source sequence.</param>
    /// <param name="func">A function that returns a Result for each element.</param>
    /// <returns>Success with all mapped values, or the first Failure.</returns>
    public static Result<IEnumerable<TResult>, TError> Traverse<T, TResult, TError>(
        this IEnumerable<T> source, Func<T, Result<TResult, TError>> func)
        where TError : Error
    {
        var values = new List<TResult>();

        foreach (var item in source)
        {
            var result = func(item);
            if (result.IsFailure)
            {
                return result.Match<Result<IEnumerable<TResult>, TError>>(
                    success: _ => throw new InvalidOperationException("Unexpected success"),
                    failure: error => Result.Failure<IEnumerable<TResult>, TError>(error));
            }

            values.Add(result.Match(
                success: v => v,
                failure: _ => throw new InvalidOperationException("Unexpected failure")));
        }

        return Result.Success<IEnumerable<TResult>, TError>(values);
    }

    // --- Join ---

    /// <summary>
    /// Combines two independent Results into a tuple. Fails fast on the first failure.
    /// </summary>
    /// <typeparam name="T1">The first success type.</typeparam>
    /// <typeparam name="T2">The second success type.</typeparam>
    /// <typeparam name="TError">The error type.</typeparam>
    /// <param name="first">The first result.</param>
    /// <param name="second">The second result.</param>
    /// <returns>Success with a tuple of both values, or the first Failure.</returns>
    public static Result<(T1, T2), TError> Join<T1, T2, TError>(
        this Result<T1, TError> first, Result<T2, TError> second)
        where TError : Error =>
        first.Bind(v1 => second.Map(v2 => (v1, v2)));

    /// <summary>
    /// Combines three independent Results into a tuple. Fails fast on the first failure.
    /// </summary>
    public static Result<(T1, T2, T3), TError> Join<T1, T2, T3, TError>(
        this Result<T1, TError> first, Result<T2, TError> second, Result<T3, TError> third)
        where TError : Error =>
        first.Bind(v1 => second.Bind(v2 => third.Map(v3 => (v1, v2, v3))));

    /// <summary>
    /// Combines four independent Results into a tuple. Fails fast on the first failure.
    /// </summary>
    public static Result<(T1, T2, T3, T4), TError> Join<T1, T2, T3, T4, TError>(
        this Result<T1, TError> first, Result<T2, TError> second,
        Result<T3, TError> third, Result<T4, TError> fourth)
        where TError : Error =>
        first.Bind(v1 => second.Bind(v2 => third.Bind(v3 => fourth.Map(v4 => (v1, v2, v3, v4)))));

    /// <summary>
    /// Combines five independent Results into a tuple. Fails fast on the first failure.
    /// </summary>
    public static Result<(T1, T2, T3, T4, T5), TError> Join<T1, T2, T3, T4, T5, TError>(
        this Result<T1, TError> first, Result<T2, TError> second,
        Result<T3, TError> third, Result<T4, TError> fourth,
        Result<T5, TError> fifth)
        where TError : Error =>
        first.Bind(v1 => second.Bind(v2 => third.Bind(v3 => fourth.Bind(v4 => fifth.Map(v5 => (v1, v2, v3, v4, v5))))));

    /// <summary>
    /// Combines six independent Results into a tuple. Fails fast on the first failure.
    /// </summary>
    public static Result<(T1, T2, T3, T4, T5, T6), TError> Join<T1, T2, T3, T4, T5, T6, TError>(
        this Result<T1, TError> first, Result<T2, TError> second,
        Result<T3, TError> third, Result<T4, TError> fourth,
        Result<T5, TError> fifth, Result<T6, TError> sixth)
        where TError : Error =>
        first.Bind(v1 => second.Bind(v2 => third.Bind(v3 => fourth.Bind(v4 => fifth.Bind(v5 => sixth.Map(v6 => (v1, v2, v3, v4, v5, v6)))))));

    /// <summary>
    /// Combines seven independent Results into a tuple. Fails fast on the first failure.
    /// </summary>
    public static Result<(T1, T2, T3, T4, T5, T6, T7), TError> Join<T1, T2, T3, T4, T5, T6, T7, TError>(
        this Result<T1, TError> first, Result<T2, TError> second,
        Result<T3, TError> third, Result<T4, TError> fourth,
        Result<T5, TError> fifth, Result<T6, TError> sixth,
        Result<T7, TError> seventh)
        where TError : Error =>
        first.Bind(v1 => second.Bind(v2 => third.Bind(v3 => fourth.Bind(v4 => fifth.Bind(v5 => sixth.Bind(v6 => seventh.Map(v7 => (v1, v2, v3, v4, v5, v6, v7))))))));

    /// <summary>
    /// Combines eight independent Results into a tuple. Fails fast on the first failure.
    /// </summary>
    public static Result<(T1, T2, T3, T4, T5, T6, T7, T8), TError> Join<T1, T2, T3, T4, T5, T6, T7, T8, TError>(
        this Result<T1, TError> first, Result<T2, TError> second,
        Result<T3, TError> third, Result<T4, TError> fourth,
        Result<T5, TError> fifth, Result<T6, TError> sixth,
        Result<T7, TError> seventh, Result<T8, TError> eighth)
        where TError : Error =>
        first.Bind(v1 => second.Bind(v2 => third.Bind(v3 => fourth.Bind(v4 => fifth.Bind(v5 => sixth.Bind(v6 => seventh.Bind(v7 => eighth.Map(v8 => (v1, v2, v3, v4, v5, v6, v7, v8)))))))));

    // --- Async Sequential ---

    /// <summary>
    /// Awaits a sequence of tasks producing Results one by one, collecting all success values.
    /// Returns the first Failure encountered (fail-fast, sequential).
    /// </summary>
    /// <typeparam name="T">The success type.</typeparam>
    /// <typeparam name="TError">The error type.</typeparam>
    /// <param name="tasks">The tasks producing results.</param>
    /// <returns>Success with all values, or the first Failure.</returns>
    public static async Task<Result<IEnumerable<T>, TError>> SequenceAsync<T, TError>(
        this IEnumerable<Task<Result<T, TError>>> tasks)
        where TError : Error
    {
        var values = new List<T>();

        foreach (var task in tasks)
        {
            var result = await task;
            if (result.IsFailure)
            {
                return result.Match<Result<IEnumerable<T>, TError>>(
                    success: _ => throw new InvalidOperationException("Unexpected success"),
                    failure: error => Result.Failure<IEnumerable<T>, TError>(error));
            }

            values.Add(result.Match(
                success: v => v,
                failure: _ => throw new InvalidOperationException("Unexpected failure")));
        }

        return Result.Success<IEnumerable<T>, TError>(values);
    }

    /// <summary>
    /// Applies an async Result-returning function to each element sequentially, then sequences the results.
    /// Returns the first Failure encountered (fail-fast, sequential).
    /// </summary>
    /// <typeparam name="T">The source element type.</typeparam>
    /// <typeparam name="TResult">The mapped success type.</typeparam>
    /// <typeparam name="TError">The error type.</typeparam>
    /// <param name="source">The source sequence.</param>
    /// <param name="func">An async function that returns a Result for each element.</param>
    /// <returns>Success with all mapped values, or the first Failure.</returns>
    public static async Task<Result<IEnumerable<TResult>, TError>> TraverseAsync<T, TResult, TError>(
        this IEnumerable<T> source, Func<T, Task<Result<TResult, TError>>> func)
        where TError : Error
    {
        var values = new List<TResult>();

        foreach (var item in source)
        {
            var result = await func(item);
            if (result.IsFailure)
            {
                return result.Match<Result<IEnumerable<TResult>, TError>>(
                    success: _ => throw new InvalidOperationException("Unexpected success"),
                    failure: error => Result.Failure<IEnumerable<TResult>, TError>(error));
            }

            values.Add(result.Match(
                success: v => v,
                failure: _ => throw new InvalidOperationException("Unexpected failure")));
        }

        return Result.Success<IEnumerable<TResult>, TError>(values);
    }

    /// <summary>
    /// Awaits a sequence of tasks producing Results one by one, partitioning into successes and failures.
    /// </summary>
    /// <typeparam name="T">The success type.</typeparam>
    /// <typeparam name="TError">The error type.</typeparam>
    /// <param name="tasks">The tasks producing results.</param>
    /// <returns>A tuple of (successes, failures).</returns>
    public static async Task<(IEnumerable<T> Successes, IEnumerable<TError> Failures)> PartitionAsync<T, TError>(
        this IEnumerable<Task<Result<T, TError>>> tasks)
        where TError : Error
    {
        var successes = new List<T>();
        var failures = new List<TError>();

        foreach (var task in tasks)
        {
            var result = await task;
            if (result.IsSuccess)
            {
                successes.Add(result.Match(success: v => v, failure: _ => default!));
            }
            else
            {
                failures.Add(result.Match(success: _ => default!, failure: e => e));
            }
        }

        return (successes, failures);
    }

    /// <summary>
    /// Awaits a sequence of tasks producing Results one by one, filtering out failures and unwrapping successes.
    /// </summary>
    /// <typeparam name="T">The success type.</typeparam>
    /// <typeparam name="TError">The error type.</typeparam>
    /// <param name="tasks">The tasks producing results.</param>
    /// <returns>A sequence containing only the unwrapped success values.</returns>
    public static async Task<IEnumerable<T>> ChooseAsync<T, TError>(
        this IEnumerable<Task<Result<T, TError>>> tasks)
        where TError : Error
    {
        var values = new List<T>();

        foreach (var task in tasks)
        {
            var result = await task;
            if (result.IsSuccess)
            {
                values.Add(result.Match(
                    success: v => v,
                    failure: _ => throw new InvalidOperationException("Unexpected failure")));
            }
        }

        return values;
    }

    // --- Async Parallel ---

    /// <summary>
    /// Awaits all tasks concurrently, then sequences the results.
    /// Returns the first Failure encountered (fail-fast after all tasks complete).
    /// </summary>
    /// <typeparam name="T">The success type.</typeparam>
    /// <typeparam name="TError">The error type.</typeparam>
    /// <param name="tasks">The tasks producing results.</param>
    /// <returns>Success with all values, or the first Failure.</returns>
    public static async Task<Result<IEnumerable<T>, TError>> SequenceParallel<T, TError>(
        this IEnumerable<Task<Result<T, TError>>> tasks)
        where TError : Error
    {
        var results = await Task.WhenAll(tasks);
        return results.Sequence();
    }

    /// <summary>
    /// Applies an async Result-returning function to all elements concurrently, then sequences the results.
    /// Returns the first Failure encountered (fail-fast after all tasks complete).
    /// </summary>
    /// <typeparam name="T">The source element type.</typeparam>
    /// <typeparam name="TResult">The mapped success type.</typeparam>
    /// <typeparam name="TError">The error type.</typeparam>
    /// <param name="source">The source sequence.</param>
    /// <param name="func">An async function that returns a Result for each element.</param>
    /// <returns>Success with all mapped values, or the first Failure.</returns>
    public static async Task<Result<IEnumerable<TResult>, TError>> TraverseParallel<T, TResult, TError>(
        this IEnumerable<T> source, Func<T, Task<Result<TResult, TError>>> func)
        where TError : Error
    {
        var results = await Task.WhenAll(source.Select(func));
        return results.Sequence();
    }

    /// <summary>
    /// Awaits all tasks concurrently, then partitions the results into successes and failures.
    /// </summary>
    /// <typeparam name="T">The success type.</typeparam>
    /// <typeparam name="TError">The error type.</typeparam>
    /// <param name="tasks">The tasks producing results.</param>
    /// <returns>A tuple of (successes, failures).</returns>
    public static async Task<(IEnumerable<T> Successes, IEnumerable<TError> Failures)> PartitionParallel<T, TError>(
        this IEnumerable<Task<Result<T, TError>>> tasks)
        where TError : Error
    {
        var results = await Task.WhenAll(tasks);
        return results.Partition();
    }

    /// <summary>
    /// Awaits all tasks concurrently, then filters out failures and unwraps successes.
    /// </summary>
    /// <typeparam name="T">The success type.</typeparam>
    /// <typeparam name="TError">The error type.</typeparam>
    /// <param name="tasks">The tasks producing results.</param>
    /// <returns>A sequence containing only the unwrapped success values.</returns>
    public static async Task<IEnumerable<T>> ChooseParallel<T, TError>(
        this IEnumerable<Task<Result<T, TError>>> tasks)
        where TError : Error
    {
        var results = await Task.WhenAll(tasks);
        return results.Choose();
    }
}

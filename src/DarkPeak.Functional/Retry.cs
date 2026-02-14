namespace DarkPeak.Functional;

/// <summary>
/// Immutable retry policy built via fluent API.
/// </summary>
public sealed record RetryPolicy
{
    internal int MaxAttempts { get; init; } = 3;
    internal Func<int, TimeSpan> BackoffStrategy { get; init; } = _ => TimeSpan.Zero;
    internal Func<Error, bool>? RetryPredicate { get; init; }
    internal Action<int, Error>? OnRetryCallback { get; init; }

    /// <summary>
    /// Sets the backoff strategy for delays between retry attempts.
    /// The function receives the attempt number (1-based) and returns the delay.
    /// </summary>
    public RetryPolicy WithBackoff(Func<int, TimeSpan> backoffStrategy) =>
        this with { BackoffStrategy = backoffStrategy };

    /// <summary>
    /// Sets a predicate to determine which errors are retryable.
    /// If not set, all errors are retried.
    /// </summary>
    public RetryPolicy WithRetryWhen(Func<Error, bool> predicate) =>
        this with { RetryPredicate = predicate };

    /// <summary>
    /// Sets a callback invoked before each retry attempt, for logging or observability.
    /// Receives the attempt number (1-based) and the error that triggered the retry.
    /// </summary>
    public RetryPolicy OnRetry(Action<int, Error> callback) =>
        this with { OnRetryCallback = callback };

    /// <summary>
    /// Executes a function with the configured retry policy.
    /// Returns the first successful result, or the last failure if all attempts are exhausted.
    /// </summary>
    public Result<T, TError> Execute<T, TError>(Func<Result<T, TError>> func) where TError : Error
    {
        Result<T, TError>? lastResult = null;

        for (var attempt = 1; attempt <= MaxAttempts; attempt++)
        {
            lastResult = func();

            if (lastResult.IsSuccess)
                return lastResult;

            var shouldRetry = attempt < MaxAttempts;
            if (!shouldRetry)
                break;

            var error = lastResult.Match<TError>(
                success: _ => throw new InvalidOperationException("Unexpected success"),
                failure: e => e);

            if (RetryPredicate is not null && !RetryPredicate(error))
                break;

            OnRetryCallback?.Invoke(attempt, error);

            var delay = BackoffStrategy(attempt);
            if (delay > TimeSpan.Zero)
                Thread.Sleep(delay);
        }

        return lastResult!;
    }

    /// <summary>
    /// Executes an async function with the configured retry policy.
    /// Returns the first successful result, or the last failure if all attempts are exhausted.
    /// </summary>
    public async Task<Result<T, TError>> ExecuteAsync<T, TError>(Func<Task<Result<T, TError>>> func)
        where TError : Error
    {
        Result<T, TError>? lastResult = null;

        for (var attempt = 1; attempt <= MaxAttempts; attempt++)
        {
            lastResult = await func();

            if (lastResult.IsSuccess)
                return lastResult;

            var shouldRetry = attempt < MaxAttempts;
            if (!shouldRetry)
                break;

            var error = lastResult.Match<TError>(
                success: _ => throw new InvalidOperationException("Unexpected success"),
                failure: e => e);

            if (RetryPredicate is not null && !RetryPredicate(error))
                break;

            OnRetryCallback?.Invoke(attempt, error);

            var delay = BackoffStrategy(attempt);
            if (delay > TimeSpan.Zero)
                await Task.Delay(delay);
        }

        return lastResult!;
    }
}

/// <summary>
/// Entry point for building retry policies.
/// </summary>
public static class Retry
{
    /// <summary>
    /// Creates a retry policy with the specified maximum number of attempts.
    /// </summary>
    /// <param name="maxAttempts">The maximum number of attempts (must be at least 1).</param>
    public static RetryPolicy WithMaxAttempts(int maxAttempts)
    {
        if (maxAttempts < 1)
            throw new ArgumentOutOfRangeException(nameof(maxAttempts), "Must be at least 1.");

        return new RetryPolicy { MaxAttempts = maxAttempts };
    }
}

/// <summary>
/// Provides static factory methods for common backoff strategies.
/// </summary>
public static class Backoff
{
    /// <summary>
    /// No delay between retries.
    /// </summary>
    public static Func<int, TimeSpan> None => _ => TimeSpan.Zero;

    /// <summary>
    /// Constant delay between retries.
    /// </summary>
    public static Func<int, TimeSpan> Constant(TimeSpan delay) =>
        _ => delay;

    /// <summary>
    /// Linearly increasing delay: initial + (attempt - 1) * increment.
    /// </summary>
    public static Func<int, TimeSpan> Linear(TimeSpan initial, TimeSpan increment) =>
        attempt => initial + TimeSpan.FromTicks(increment.Ticks * (attempt - 1));

    /// <summary>
    /// Exponentially increasing delay: initial * multiplier^(attempt - 1).
    /// </summary>
    public static Func<int, TimeSpan> Exponential(TimeSpan initial, double multiplier = 2.0) =>
        attempt => TimeSpan.FromTicks((long)(initial.Ticks * Math.Pow(multiplier, attempt - 1)));

    /// <summary>
    /// Exponentially increasing delay with a maximum cap.
    /// </summary>
    public static Func<int, TimeSpan> Exponential(TimeSpan initial, double multiplier, TimeSpan maxDelay) =>
        attempt =>
        {
            var delay = TimeSpan.FromTicks((long)(initial.Ticks * Math.Pow(multiplier, attempt - 1)));
            return delay > maxDelay ? maxDelay : delay;
        };
}

namespace DarkPeak.Functional;

/// <summary>
/// Immutable timeout policy built via fluent API.
/// </summary>
/// <remarks>
/// <para>
/// The timeout policy wraps async operations and returns a <see cref="Result{T,TError}"/> 
/// containing a <see cref="TimeoutError"/> if the operation exceeds the configured timeout duration.
/// Instead of throwing exceptions, the timeout is returned as a typed error.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// var timeout = TimeoutPolicy.Create()
///     .WithTimeout(TimeSpan.FromSeconds(5))
///     .WithTimeoutError(elapsed => new TimeoutError 
///     { 
///         Message = $"Operation timed out after {elapsed.TotalSeconds}s",
///         Elapsed = elapsed
///     });
///
/// var result = await timeout.ExecuteAsync(
///     async ct => await httpClient.GetResultAsync&lt;Data&gt;("/api/data", ct));
/// </code>
/// </example>
public sealed record TimeoutPolicy
{
    internal TimeSpan Timeout { get; init; } = TimeSpan.FromSeconds(30);
    internal Func<TimeSpan, Error>? TimeoutErrorFactory { get; init; }

    /// <summary>
    /// Sets the timeout duration for the operation.
    /// </summary>
    /// <param name="timeout">The maximum duration the operation is allowed to run.</param>
    public TimeoutPolicy WithTimeout(TimeSpan timeout)
    {
        if (timeout <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(timeout), "Must be greater than zero.");

        return this with { Timeout = timeout };
    }

    /// <summary>
    /// Sets a custom error factory to create the timeout error.
    /// If not set, a default <see cref="TimeoutError"/> is created.
    /// </summary>
    /// <param name="errorFactory">A function that receives the elapsed time and returns an error.</param>
    public TimeoutPolicy WithTimeoutError(Func<TimeSpan, Error> errorFactory) =>
        this with { TimeoutErrorFactory = errorFactory };

    /// <summary>
    /// Executes an async function with the configured timeout policy.
    /// Returns the function's result if it completes within the timeout,
    /// or a <see cref="TimeoutError"/> if the timeout is exceeded.
    /// </summary>
    /// <param name="func">The async function to execute. Receives a <see cref="CancellationToken"/> 
    /// that will be cancelled when the timeout is reached.</param>
    /// <param name="cancellationToken">Optional cancellation token to cancel the operation externally.</param>
    public async Task<Result<T, TError>> ExecuteAsync<T, TError>(
        Func<CancellationToken, Task<Result<T, TError>>> func,
        CancellationToken cancellationToken = default)
        where TError : Error
    {
        using var timeoutCts = new CancellationTokenSource();
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        try
        {
            timeoutCts.CancelAfter(Timeout);
            var result = await func(linkedCts.Token);
            return result;
        }
        catch (OperationCanceledException) when (timeoutCts.Token.IsCancellationRequested && !cancellationToken.IsCancellationRequested)
        {
            // Timeout occurred (not external cancellation)
            stopwatch.Stop();
            var error = TimeoutErrorFactory?.Invoke(stopwatch.Elapsed) ??
                        new TimeoutError
                        {
                            Message = $"Operation timed out after {stopwatch.Elapsed.TotalSeconds:F2} seconds.",
                            Timeout = Timeout,
                            Elapsed = stopwatch.Elapsed
                        };

            return Result.Failure<T, TError>((TError)error);
        }
    }

    /// <summary>
    /// Executes an async function that returns a plain value (not wrapped in Result) with the configured timeout policy.
    /// Returns a Success result if it completes within the timeout,
    /// or a Failure with <see cref="TimeoutError"/> if the timeout is exceeded.
    /// </summary>
    /// <param name="func">The async function to execute. Receives a <see cref="CancellationToken"/> 
    /// that will be cancelled when the timeout is reached.</param>
    /// <param name="cancellationToken">Optional cancellation token to cancel the operation externally.</param>
    public async Task<Result<T, TError>> ExecuteAsync<T, TError>(
        Func<CancellationToken, Task<T>> func,
        CancellationToken cancellationToken = default)
        where TError : Error
    {
        using var timeoutCts = new CancellationTokenSource();
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        try
        {
            timeoutCts.CancelAfter(Timeout);
            var value = await func(linkedCts.Token);
            return Result.Success<T, TError>(value);
        }
        catch (OperationCanceledException) when (timeoutCts.Token.IsCancellationRequested && !cancellationToken.IsCancellationRequested)
        {
            // Timeout occurred (not external cancellation)
            stopwatch.Stop();
            var error = TimeoutErrorFactory?.Invoke(stopwatch.Elapsed) ??
                        new TimeoutError
                        {
                            Message = $"Operation timed out after {stopwatch.Elapsed.TotalSeconds:F2} seconds.",
                            Timeout = Timeout,
                            Elapsed = stopwatch.Elapsed
                        };

            return Result.Failure<T, TError>((TError)error);
        }
    }
}

/// <summary>
/// Entry point for building timeout policies.
/// </summary>
/// <example>
/// <code>
/// var timeout = TimeoutPolicy.Create()
///     .WithTimeout(TimeSpan.FromSeconds(10));
/// </code>
/// </example>
public static class Timeout
{
    /// <summary>
    /// Creates a new timeout policy with default settings (30 second timeout).
    /// </summary>
    public static TimeoutPolicy Create() => new();
}

namespace DarkPeak.Functional;

/// <summary>
/// Immutable bulkhead policy built via fluent API.
/// </summary>
/// <remarks>
/// <para>
/// The bulkhead policy limits the number of concurrent operations to prevent resource exhaustion.
/// When the maximum concurrency is reached, new requests are queued up to the configured queue size.
/// If the queue is also full, requests are immediately rejected with <see cref="BulkheadRejectedError"/>.
/// </para>
/// <para>
/// The policy configuration is immutable. Internal state (current concurrency, queue)
/// is managed by a shared <see cref="BulkheadStateTracker"/> allocated when the policy is created.
/// Multiple calls to <see cref="ExecuteAsync{T,TError}(Func{CancellationToken, Task{Result{T, TError}}}, CancellationToken)"/> 
/// share this state, making the policy instance safe to reuse across threads.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// var bulkhead = Bulkhead.WithMaxConcurrency(10)
///     .WithMaxQueueSize(20)
///     .OnRejected(() => logger.LogWarning("Request rejected - bulkhead full"));
///
/// var result = await bulkhead.ExecuteAsync(
///     async ct => await httpClient.GetResultAsync&lt;Data&gt;("/api/data", ct));
/// </code>
/// </example>
public sealed record BulkheadPolicy
{
    internal int MaxConcurrency { get; init; } = 10;
    internal int MaxQueueSize { get; init; } = 0;
    internal Action? OnRejectedCallback { get; init; }
    internal BulkheadStateTracker StateTracker { get; init; } = new();

    /// <summary>
    /// Sets the maximum queue size for requests waiting for an available slot.
    /// When the queue is full, new requests are immediately rejected.
    /// </summary>
    /// <param name="maxQueueSize">The maximum number of queued requests (must be at least 0).</param>
    public BulkheadPolicy WithMaxQueueSize(int maxQueueSize)
    {
        if (maxQueueSize < 0)
            throw new ArgumentOutOfRangeException(nameof(maxQueueSize), "Must be at least 0.");

        return this with { MaxQueueSize = maxQueueSize };
    }

    /// <summary>
    /// Sets a callback invoked when a request is rejected due to the bulkhead being full,
    /// for logging or observability.
    /// </summary>
    /// <param name="callback">A function to invoke when rejection occurs.</param>
    public BulkheadPolicy OnRejected(Action callback) =>
        this with { OnRejectedCallback = callback };

    /// <summary>
    /// Executes an async function through the bulkhead.
    /// Returns the function's result if a slot is available (or becomes available within the queue),
    /// or a <see cref="BulkheadRejectedError"/> if the bulkhead is full.
    /// </summary>
    /// <param name="func">The async function to execute.</param>
    /// <param name="cancellationToken">Optional cancellation token to cancel the operation externally.</param>
    public async Task<Result<T, TError>> ExecuteAsync<T, TError>(
        Func<CancellationToken, Task<Result<T, TError>>> func,
        CancellationToken cancellationToken = default)
        where TError : Error
    {
        // Try to acquire a slot
        if (!await StateTracker.TryAcquireAsync(MaxConcurrency, MaxQueueSize, cancellationToken))
        {
            OnRejectedCallback?.Invoke();
            return Result.Failure<T, TError>((TError)(Error)new BulkheadRejectedError
            {
                Message = "Request rejected. Maximum concurrency and queue size reached.",
                MaxConcurrency = MaxConcurrency,
                MaxQueueSize = MaxQueueSize
            });
        }

        try
        {
            return await func(cancellationToken);
        }
        finally
        {
            StateTracker.Release();
        }
    }

    /// <summary>
    /// Executes an async function that returns a plain value (not wrapped in Result) through the bulkhead.
    /// Returns a Success result if a slot is available,
    /// or a Failure with <see cref="BulkheadRejectedError"/> if the bulkhead is full.
    /// </summary>
    /// <param name="func">The async function to execute.</param>
    /// <param name="cancellationToken">Optional cancellation token to cancel the operation externally.</param>
    public async Task<Result<T, TError>> ExecuteAsync<T, TError>(
        Func<CancellationToken, Task<T>> func,
        CancellationToken cancellationToken = default)
        where TError : Error
    {
        // Try to acquire a slot
        if (!await StateTracker.TryAcquireAsync(MaxConcurrency, MaxQueueSize, cancellationToken))
        {
            OnRejectedCallback?.Invoke();
            return Result.Failure<T, TError>((TError)(Error)new BulkheadRejectedError
            {
                Message = "Request rejected. Maximum concurrency and queue size reached.",
                MaxConcurrency = MaxConcurrency,
                MaxQueueSize = MaxQueueSize
            });
        }

        try
        {
            var value = await func(cancellationToken);
            return Result.Success<T, TError>(value);
        }
        finally
        {
            StateTracker.Release();
        }
    }
}

/// <summary>
/// Mutable state tracker for a bulkhead, shared across all executions of a policy instance.
/// Thread safety is ensured via lock.
/// </summary>
internal sealed class BulkheadStateTracker
{
    private readonly Lock _lock = new();
    private int _currentConcurrency;
    private readonly Queue<TaskCompletionSource<bool>> _queue = new();

    internal async Task<bool> TryAcquireAsync(int maxConcurrency, int maxQueueSize, CancellationToken cancellationToken)
    {
        TaskCompletionSource<bool>? tcs = null;

        lock (_lock)
        {
            if (_currentConcurrency < maxConcurrency)
            {
                // Slot available
                _currentConcurrency++;
                return true;
            }

            // Check if we can queue
            if (_queue.Count >= maxQueueSize)
            {
                // Queue is full
                return false;
            }

            // Queue the request
            tcs = new TaskCompletionSource<bool>();
            _queue.Enqueue(tcs);
        }

        // Register cancellation
        using var registration = cancellationToken.Register(() => tcs.TrySetCanceled(cancellationToken));

        try
        {
            return await tcs.Task;
        }
        catch (OperationCanceledException)
        {
            // Remove from queue if still there
            lock (_lock)
            {
                if (_queue.Contains(tcs))
                {
                    var tempQueue = new Queue<TaskCompletionSource<bool>>(_queue.Count);
                    while (_queue.Count > 0)
                    {
                        var item = _queue.Dequeue();
                        if (item != tcs)
                            tempQueue.Enqueue(item);
                    }
                    while (tempQueue.Count > 0)
                        _queue.Enqueue(tempQueue.Dequeue());
                }
            }
            return false;
        }
    }

    internal void Release()
    {
        lock (_lock)
        {
            _currentConcurrency--;

            // Try to dequeue and release a waiting task
            while (_queue.Count > 0)
            {
                var tcs = _queue.Dequeue();
                if (tcs.TrySetResult(true))
                {
                    _currentConcurrency++;
                    return;
                }
                // Task was cancelled, try next one
            }
        }
    }
}

/// <summary>
/// Entry point for building bulkhead policies.
/// </summary>
/// <example>
/// <code>
/// var bulkhead = Bulkhead.WithMaxConcurrency(5)
///     .WithMaxQueueSize(10);
/// </code>
/// </example>
public static class Bulkhead
{
    /// <summary>
    /// Creates a bulkhead policy with the specified maximum number of concurrent operations.
    /// </summary>
    /// <param name="maxConcurrency">The maximum number of concurrent operations (must be at least 1).</param>
    public static BulkheadPolicy WithMaxConcurrency(int maxConcurrency)
    {
        if (maxConcurrency < 1)
            throw new ArgumentOutOfRangeException(nameof(maxConcurrency), "Must be at least 1.");

        return new BulkheadPolicy { MaxConcurrency = maxConcurrency };
    }
}

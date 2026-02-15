namespace DarkPeak.Functional;

/// <summary>
/// Represents the state of a circuit breaker.
/// </summary>
public enum CircuitBreakerState
{
    /// <summary>
    /// Requests flow through normally. Failures are counted toward the threshold.
    /// </summary>
    Closed,

    /// <summary>
    /// Requests are immediately rejected. The circuit waits for the reset timeout before
    /// transitioning to <see cref="HalfOpen"/>.
    /// </summary>
    Open,

    /// <summary>
    /// A single probe request is allowed through. Success transitions to <see cref="Closed"/>,
    /// failure transitions back to <see cref="Open"/>.
    /// </summary>
    HalfOpen
}

/// <summary>
/// Error returned when a circuit breaker is in the <see cref="CircuitBreakerState.Open"/> state
/// and rejects a request without executing it.
/// </summary>
public sealed record CircuitBreakerOpenError : Error
{
    /// <summary>
    /// Gets or sets the time remaining until the circuit breaker transitions to half-open.
    /// </summary>
    public TimeSpan? RetryAfter { get; init; }
}

/// <summary>
/// Immutable circuit breaker policy built via fluent API.
/// </summary>
/// <remarks>
/// <para>
/// The circuit breaker prevents cascading failures by short-circuiting requests to a failing
/// dependency. It tracks consecutive failures and transitions between three states:
/// </para>
/// <list type="bullet">
///   <item><term>Closed</term><description>Normal operation. Failures increment a counter. When the counter reaches the failure threshold, the circuit opens.</description></item>
///   <item><term>Open</term><description>All requests are immediately rejected with <see cref="CircuitBreakerOpenError"/>. After the reset timeout, the circuit transitions to half-open.</description></item>
///   <item><term>HalfOpen</term><description>One probe request is allowed through. On success the circuit closes; on failure it reopens.</description></item>
/// </list>
/// <para>
/// The policy configuration is immutable. Internal state (failure count, current state, timestamps)
/// is managed by a shared <see cref="CircuitBreakerStateTracker"/> allocated when the policy is created.
/// Multiple calls to <see cref="Execute{T,TError}"/> and <see cref="ExecuteAsync{T,TError}"/> share
/// this state, making the policy instance safe to reuse across threads.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// var breaker = CircuitBreaker
///     .WithFailureThreshold(5)
///     .WithResetTimeout(TimeSpan.FromSeconds(30))
///     .WithBreakWhen(error => error is ExternalServiceError)
///     .OnStateChange((from, to) =>
///         logger.LogWarning("Circuit breaker: {From} -> {To}", from, to));
///
/// var result = await breaker.ExecuteAsync(
///     () => httpClient.GetResultAsync&lt;Data&gt;("/api/data"));
/// </code>
/// </example>
public sealed record CircuitBreakerPolicy
{
    internal int FailureThreshold { get; init; } = 5;
    internal TimeSpan ResetTimeout { get; init; } = TimeSpan.FromSeconds(30);
    internal Func<Error, bool>? BreakPredicate { get; init; }
    internal Action<CircuitBreakerState, CircuitBreakerState>? OnStateChangeCallback { get; init; }
    internal CircuitBreakerStateTracker StateTracker { get; init; } = new();

    /// <summary>
    /// Sets the duration the circuit stays open before transitioning to half-open.
    /// </summary>
    /// <param name="timeout">The reset timeout duration.</param>
    public CircuitBreakerPolicy WithResetTimeout(TimeSpan timeout)
    {
        if (timeout <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(timeout), "Must be greater than zero.");

        return this with { ResetTimeout = timeout };
    }

    /// <summary>
    /// Sets a predicate to determine which errors count toward the failure threshold.
    /// If not set, all errors count as failures.
    /// </summary>
    /// <param name="predicate">A function that returns true for errors that should trip the breaker.</param>
    public CircuitBreakerPolicy WithBreakWhen(Func<Error, bool> predicate) =>
        this with { BreakPredicate = predicate };

    /// <summary>
    /// Sets a callback invoked when the circuit breaker changes state, for logging or observability.
    /// </summary>
    /// <param name="callback">A function receiving the previous and new state.</param>
    public CircuitBreakerPolicy OnStateChange(Action<CircuitBreakerState, CircuitBreakerState> callback) =>
        this with { OnStateChangeCallback = callback };

    /// <summary>
    /// Executes a function through the circuit breaker.
    /// Returns the function's result if the circuit is closed or half-open,
    /// or a <see cref="CircuitBreakerOpenError"/> if the circuit is open.
    /// </summary>
    public Result<T, TError> Execute<T, TError>(Func<Result<T, TError>> func) where TError : Error
    {
        var (state, retryAfter) = GetEffectiveState();

        if (state == CircuitBreakerState.Open)
        {
            return Result.Failure<T, TError>((TError)(Error)new CircuitBreakerOpenError
            {
                Message = "Circuit breaker is open. Requests are being rejected.",
                RetryAfter = retryAfter
            });
        }

        var result = func();

        if (result.IsSuccess)
        {
            OnSuccess();
        }
        else
        {
            var error = result.Match<TError>(
                success: _ => throw new InvalidOperationException("Unexpected success"),
                failure: e => e);

            OnFailure(error);
        }

        return result;
    }

    /// <summary>
    /// Executes an async function through the circuit breaker.
    /// Returns the function's result if the circuit is closed or half-open,
    /// or a <see cref="CircuitBreakerOpenError"/> if the circuit is open.
    /// </summary>
    public async Task<Result<T, TError>> ExecuteAsync<T, TError>(Func<Task<Result<T, TError>>> func)
        where TError : Error
    {
        var (state, retryAfter) = GetEffectiveState();

        if (state == CircuitBreakerState.Open)
        {
            return Result.Failure<T, TError>((TError)(Error)new CircuitBreakerOpenError
            {
                Message = "Circuit breaker is open. Requests are being rejected.",
                RetryAfter = retryAfter
            });
        }

        var result = await func();

        if (result.IsSuccess)
        {
            OnSuccess();
        }
        else
        {
            var error = result.Match<TError>(
                success: _ => throw new InvalidOperationException("Unexpected success"),
                failure: e => e);

            OnFailure(error);
        }

        return result;
    }

    private (CircuitBreakerState State, TimeSpan? RetryAfter) GetEffectiveState()
    {
        lock (StateTracker.Lock)
        {
            if (StateTracker.State == CircuitBreakerState.Open)
            {
                var elapsed = DateTimeOffset.UtcNow - StateTracker.LastFailureTime;
                if (elapsed >= ResetTimeout)
                {
                    TransitionTo(CircuitBreakerState.HalfOpen);
                    return (CircuitBreakerState.HalfOpen, null);
                }

                var retryAfter = ResetTimeout - elapsed;
                return (CircuitBreakerState.Open, retryAfter > TimeSpan.Zero ? retryAfter : null);
            }

            return (StateTracker.State, null);
        }
    }

    private void OnSuccess()
    {
        lock (StateTracker.Lock)
        {
            StateTracker.ConsecutiveFailures = 0;
            if (StateTracker.State != CircuitBreakerState.Closed)
            {
                TransitionTo(CircuitBreakerState.Closed);
            }
        }
    }

    private void OnFailure(Error error)
    {
        if (BreakPredicate is not null && !BreakPredicate(error))
            return;

        lock (StateTracker.Lock)
        {
            StateTracker.ConsecutiveFailures++;
            StateTracker.LastFailureTime = DateTimeOffset.UtcNow;

            if (StateTracker.State == CircuitBreakerState.HalfOpen)
            {
                TransitionTo(CircuitBreakerState.Open);
            }
            else if (StateTracker.State == CircuitBreakerState.Closed &&
                     StateTracker.ConsecutiveFailures >= FailureThreshold)
            {
                TransitionTo(CircuitBreakerState.Open);
            }
        }
    }

    private void TransitionTo(CircuitBreakerState newState)
    {
        var previousState = StateTracker.State;
        StateTracker.State = newState;

        if (newState == CircuitBreakerState.Closed)
            StateTracker.ConsecutiveFailures = 0;

        OnStateChangeCallback?.Invoke(previousState, newState);
    }
}

/// <summary>
/// Mutable state tracker for a circuit breaker, shared across all executions of a policy instance.
/// Thread safety is ensured via the <see cref="Lock"/> object.
/// </summary>
internal sealed class CircuitBreakerStateTracker
{
    internal readonly Lock Lock = new();
    internal CircuitBreakerState State = CircuitBreakerState.Closed;
    internal int ConsecutiveFailures;
    internal DateTimeOffset? LastFailureTime;
}

/// <summary>
/// Entry point for building circuit breaker policies.
/// </summary>
/// <example>
/// <code>
/// var breaker = CircuitBreaker.WithFailureThreshold(3)
///     .WithResetTimeout(TimeSpan.FromSeconds(60));
/// </code>
/// </example>
public static class CircuitBreaker
{
    /// <summary>
    /// Creates a circuit breaker policy with the specified failure threshold.
    /// The circuit opens after this many consecutive failures.
    /// </summary>
    /// <param name="threshold">The number of consecutive failures before the circuit opens (must be at least 1).</param>
    public static CircuitBreakerPolicy WithFailureThreshold(int threshold)
    {
        if (threshold < 1)
            throw new ArgumentOutOfRangeException(nameof(threshold), "Must be at least 1.");

        return new CircuitBreakerPolicy { FailureThreshold = threshold };
    }
}

namespace DarkPeak.Functional;

/// <summary>
/// Composite resilience policy that chains multiple resilience strategies
/// (Timeout, Retry, Circuit Breaker, Bulkhead) in a unified, composable way.
/// </summary>
/// <remarks>
/// <para>
/// The resilience policy applies strategies in the following order:
/// <list type="number">
///   <item><description>Overall Timeout (wraps everything)</description></item>
///   <item><description>Retry (with optional per-attempt timeout)</description></item>
///   <item><description>Circuit Breaker (protects downstream)</description></item>
///   <item><description>Bulkhead (limits concurrency)</description></item>
/// </list>
/// </para>
/// <para>
/// This ordering ensures that:
/// - The overall timeout prevents infinite retries
/// - Retries can have their own per-attempt timeout
/// - Circuit breaker prevents cascading failures even during retries
/// - Bulkhead protects resources at the innermost level
/// </para>
/// </remarks>
/// <example>
/// <code>
/// var policy = ResiliencePolicy.Create&lt;Error&gt;()
///     .WithTimeout(TimeSpan.FromSeconds(30))
///     .WithRetry(r => r
///         .WithMaxAttempts(3)
///         .WithBackoff(Backoff.Exponential(TimeSpan.FromMilliseconds(200)))
///         .WithTimeout(TimeSpan.FromSeconds(5)))
///     .WithCircuitBreaker(cb => cb
///         .WithFailureThreshold(5)
///         .WithResetTimeout(TimeSpan.FromMinutes(1)))
///     .WithBulkhead(b => b
///         .WithMaxConcurrency(10)
///         .WithMaxQueueSize(20))
///     .Build();
///
/// var result = await policy.ExecuteAsync(
///     async ct => await httpClient.GetResultAsync&lt;Data&gt;("/api/data", ct));
/// </code>
/// </example>
/// <typeparam name="TError">The error type constraint.</typeparam>
public sealed class ResiliencePolicy<TError> where TError : Error
{
    private readonly TimeoutPolicy? _overallTimeout;
    private readonly RetryPolicy? _retry;
    private readonly TimeoutPolicy? _perAttemptTimeout;
    private readonly CircuitBreakerPolicy? _circuitBreaker;
    private readonly BulkheadPolicy? _bulkhead;

    internal ResiliencePolicy(
        TimeoutPolicy? overallTimeout,
        RetryPolicy? retry,
        TimeoutPolicy? perAttemptTimeout,
        CircuitBreakerPolicy? circuitBreaker,
        BulkheadPolicy? bulkhead)
    {
        _overallTimeout = overallTimeout;
        _retry = retry;
        _perAttemptTimeout = perAttemptTimeout;
        _circuitBreaker = circuitBreaker;
        _bulkhead = bulkhead;
    }

    /// <summary>
    /// Executes an async function through all configured resilience policies.
    /// </summary>
    /// <param name="func">The async function to execute. Receives a <see cref="CancellationToken"/>.</param>
    /// <param name="cancellationToken">Optional cancellation token to cancel the operation externally.</param>
    public async Task<Result<T, TError>> ExecuteAsync<T>(
        Func<CancellationToken, Task<Result<T, TError>>> func,
        CancellationToken cancellationToken = default)
    {
        // Build the execution pipeline from innermost to outermost
        Func<CancellationToken, Task<Result<T, TError>>> pipeline = func;

        // 4. Bulkhead (innermost - limits concurrency)
        if (_bulkhead is not null)
        {
            var capturedBulkhead = _bulkhead;
            var capturedPipeline = pipeline;
            pipeline = ct => capturedBulkhead.ExecuteAsync(capturedPipeline, ct);
        }

        // 3. Circuit Breaker (protects downstream)
        if (_circuitBreaker is not null)
        {
            var capturedBreaker = _circuitBreaker;
            var capturedPipeline = pipeline;
            pipeline = ct => capturedBreaker.ExecuteAsync(capturedPipeline, ct);
        }

        // 2. Per-attempt timeout (wraps each retry attempt)
        if (_perAttemptTimeout is not null)
        {
            var capturedTimeout = _perAttemptTimeout;
            var capturedPipeline = pipeline;
            pipeline = ct => capturedTimeout.ExecuteAsync(capturedPipeline, ct);
        }

        // 1b. Retry (wraps per-attempt timeout, circuit breaker, bulkhead)
        if (_retry is not null)
        {
            var capturedRetry = _retry;
            var capturedPipeline = pipeline;
            pipeline = ct => capturedRetry.ExecuteAsync(capturedPipeline, ct);
        }

        // 1a. Overall timeout (outermost - wraps everything)
        if (_overallTimeout is not null)
        {
            return await _overallTimeout.ExecuteAsync(pipeline, cancellationToken);
        }

        return await pipeline(cancellationToken);
    }

    /// <summary>
    /// Executes an async function that returns a plain value (not wrapped in Result) through all configured resilience policies.
    /// </summary>
    /// <param name="func">The async function to execute. Receives a <see cref="CancellationToken"/>.</param>
    /// <param name="cancellationToken">Optional cancellation token to cancel the operation externally.</param>
    public async Task<Result<T, TError>> ExecuteAsync<T>(
        Func<CancellationToken, Task<T>> func,
        CancellationToken cancellationToken = default)
    {
        return await ExecuteAsync<T>(
            async ct =>
            {
                var value = await func(ct);
                return Result.Success<T, TError>(value);
            },
            cancellationToken);
    }
}

/// <summary>
/// Builder for constructing composite resilience policies.
/// </summary>
/// <typeparam name="TError">The error type constraint.</typeparam>
public sealed class ResiliencePolicyBuilder<TError> where TError : Error
{
    private TimeoutPolicy? _overallTimeout;
    private RetryPolicy? _retry;
    private TimeoutPolicy? _perAttemptTimeout;
    private CircuitBreakerPolicy? _circuitBreaker;
    private BulkheadPolicy? _bulkhead;

    /// <summary>
    /// Configures an overall timeout that wraps the entire operation.
    /// This timeout applies across all retry attempts.
    /// </summary>
    /// <param name="timeout">The maximum duration for the entire operation.</param>
    public ResiliencePolicyBuilder<TError> WithTimeout(TimeSpan timeout)
    {
        _overallTimeout = Timeout.Create().WithTimeout(timeout);
        return this;
    }

    /// <summary>
    /// Configures an overall timeout with a custom error factory.
    /// </summary>
    /// <param name="timeout">The maximum duration for the entire operation.</param>
    /// <param name="errorFactory">A function to create the timeout error.</param>
    public ResiliencePolicyBuilder<TError> WithTimeout(TimeSpan timeout, Func<TimeSpan, Error> errorFactory)
    {
        _overallTimeout = Timeout.Create().WithTimeout(timeout).WithTimeoutError(errorFactory);
        return this;
    }

    /// <summary>
    /// Configures a retry policy with an optional per-attempt timeout.
    /// The retry policy wraps the circuit breaker and bulkhead.
    /// </summary>
    /// <param name="configure">A function to configure the retry policy.</param>
    public ResiliencePolicyBuilder<TError> WithRetry(Func<RetryPolicyBuilder, RetryPolicyBuilder> configure)
    {
        var builder = new RetryPolicyBuilder();
        builder = configure(builder);
        _retry = builder.Build();
        _perAttemptTimeout = builder.PerAttemptTimeout;
        return this;
    }

    /// <summary>
    /// Configures a circuit breaker policy.
    /// The circuit breaker sits between retry and bulkhead.
    /// </summary>
    /// <param name="configure">A function to configure the circuit breaker starting with a default configuration.</param>
    public ResiliencePolicyBuilder<TError> WithCircuitBreaker(Func<CircuitBreakerPolicyBuilder, CircuitBreakerPolicyBuilder> configure)
    {
        var builder = new CircuitBreakerPolicyBuilder();
        builder = configure(builder);
        _circuitBreaker = builder.Build();
        return this;
    }

    /// <summary>
    /// Configures a bulkhead policy to limit concurrency.
    /// The bulkhead is the innermost policy, closest to the actual operation.
    /// </summary>
    /// <param name="configure">A function to configure the bulkhead starting with a default configuration.</param>
    public ResiliencePolicyBuilder<TError> WithBulkhead(Func<BulkheadPolicyBuilder, BulkheadPolicyBuilder> configure)
    {
        var builder = new BulkheadPolicyBuilder();
        builder = configure(builder);
        _bulkhead = builder.Build();
        return this;
    }

    /// <summary>
    /// Builds the composite resilience policy.
    /// </summary>
    public ResiliencePolicy<TError> Build()
    {
        return new ResiliencePolicy<TError>(
            _overallTimeout,
            _retry,
            _perAttemptTimeout,
            _circuitBreaker,
            _bulkhead);
    }
}

/// <summary>
/// Helper builder for configuring retry policies within a resilience policy.
/// Extends the standard retry configuration with per-attempt timeout support.
/// </summary>
public sealed class RetryPolicyBuilder
{
    private RetryPolicy _policy = new() { MaxAttempts = 3 };
    internal TimeoutPolicy? PerAttemptTimeout { get; private set; }

    /// <summary>
    /// Sets the maximum number of retry attempts.
    /// </summary>
    public RetryPolicyBuilder WithMaxAttempts(int maxAttempts)
    {
        if (maxAttempts < 1)
            throw new ArgumentOutOfRangeException(nameof(maxAttempts), "Must be at least 1.");

        _policy = _policy with { MaxAttempts = maxAttempts };
        return this;
    }

    /// <summary>
    /// Sets the backoff strategy for delays between retry attempts.
    /// </summary>
    public RetryPolicyBuilder WithBackoff(Func<int, TimeSpan> backoffStrategy)
    {
        _policy = _policy.WithBackoff(backoffStrategy);
        return this;
    }

    /// <summary>
    /// Sets a predicate to determine which errors are retryable.
    /// </summary>
    public RetryPolicyBuilder WithRetryWhen(Func<Error, bool> predicate)
    {
        _policy = _policy.WithRetryWhen(predicate);
        return this;
    }

    /// <summary>
    /// Sets a callback invoked before each retry attempt.
    /// </summary>
    public RetryPolicyBuilder OnRetry(Action<int, Error> callback)
    {
        _policy = _policy.OnRetry(callback);
        return this;
    }

    /// <summary>
    /// Sets a per-attempt timeout that applies to each individual retry attempt.
    /// This is different from the overall timeout which applies to all attempts combined.
    /// </summary>
    public RetryPolicyBuilder WithTimeout(TimeSpan timeout)
    {
        PerAttemptTimeout = Timeout.Create().WithTimeout(timeout);
        return this;
    }

    /// <summary>
    /// Sets a per-attempt timeout with a custom error factory.
    /// </summary>
    public RetryPolicyBuilder WithTimeout(TimeSpan timeout, Func<TimeSpan, Error> errorFactory)
    {
        PerAttemptTimeout = Timeout.Create().WithTimeout(timeout).WithTimeoutError(errorFactory);
        return this;
    }

    internal RetryPolicy Build() => _policy;
}

/// <summary>
/// Helper builder for configuring circuit breaker policies within a resilience policy.
/// </summary>
public sealed class CircuitBreakerPolicyBuilder
{
    private CircuitBreakerPolicy _policy = new() { FailureThreshold = 5 };

    /// <summary>
    /// Sets the number of consecutive failures before the circuit opens.
    /// </summary>
    public CircuitBreakerPolicyBuilder WithFailureThreshold(int threshold)
    {
        if (threshold < 1)
            throw new ArgumentOutOfRangeException(nameof(threshold), "Must be at least 1.");

        _policy = _policy with { FailureThreshold = threshold };
        return this;
    }

    /// <summary>
    /// Sets the duration the circuit stays open before transitioning to half-open.
    /// </summary>
    public CircuitBreakerPolicyBuilder WithResetTimeout(TimeSpan timeout)
    {
        _policy = _policy.WithResetTimeout(timeout);
        return this;
    }

    /// <summary>
    /// Sets a predicate to determine which errors count toward the failure threshold.
    /// </summary>
    public CircuitBreakerPolicyBuilder WithBreakWhen(Func<Error, bool> predicate)
    {
        _policy = _policy.WithBreakWhen(predicate);
        return this;
    }

    /// <summary>
    /// Sets a callback invoked when the circuit breaker changes state.
    /// </summary>
    public CircuitBreakerPolicyBuilder OnStateChange(Action<CircuitBreakerState, CircuitBreakerState> callback)
    {
        _policy = _policy.OnStateChange(callback);
        return this;
    }

    internal CircuitBreakerPolicy Build() => _policy;
}

/// <summary>
/// Helper builder for configuring bulkhead policies within a resilience policy.
/// </summary>
public sealed class BulkheadPolicyBuilder
{
    private BulkheadPolicy _policy = new() { MaxConcurrency = 10 };

    /// <summary>
    /// Sets the maximum number of concurrent operations.
    /// </summary>
    public BulkheadPolicyBuilder WithMaxConcurrency(int maxConcurrency)
    {
        if (maxConcurrency < 1)
            throw new ArgumentOutOfRangeException(nameof(maxConcurrency), "Must be at least 1.");

        _policy = _policy with { MaxConcurrency = maxConcurrency };
        return this;
    }

    /// <summary>
    /// Sets the maximum queue size for waiting requests.
    /// </summary>
    public BulkheadPolicyBuilder WithMaxQueueSize(int maxQueueSize)
    {
        _policy = _policy.WithMaxQueueSize(maxQueueSize);
        return this;
    }

    /// <summary>
    /// Sets a callback invoked when a request is rejected.
    /// </summary>
    public BulkheadPolicyBuilder OnRejected(Action callback)
    {
        _policy = _policy.OnRejected(callback);
        return this;
    }

    internal BulkheadPolicy Build() => _policy;
}

/// <summary>
/// Entry point for building composite resilience policies.
/// </summary>
public static class ResiliencePolicy
{
    /// <summary>
    /// Creates a new resilience policy builder.
    /// </summary>
    /// <typeparam name="TError">The error type that all policies will use.</typeparam>
    public static ResiliencePolicyBuilder<TError> Create<TError>() where TError : Error
    {
        return new ResiliencePolicyBuilder<TError>();
    }
}

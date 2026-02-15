using DarkPeak.Functional;

namespace DarkPeak.Functional.Tests;

public class ResiliencePolicyShould
{
    #region Basic Composition

    [Test]
    public async Task ExecuteAsync_with_all_policies_applies_them_correctly()
    {
        var policy = ResiliencePolicy.Create<Error>()
            .WithTimeout(TimeSpan.FromSeconds(10))
            .WithRetry(r => r.WithMaxAttempts(3))
            .WithCircuitBreaker(cb => cb.WithFailureThreshold(5))
            .WithBulkhead(b => b.WithMaxConcurrency(10))
            .Build();

        var result = await policy.ExecuteAsync(
            async ct =>
            {
                await Task.Delay(10, ct);
                return Result.Success<int, Error>(42);
            });

        await Assert.That(result.IsSuccess).IsTrue();
        await Assert.That(result.GetValueOrThrow()).IsEqualTo(42);
    }

    [Test]
    public async Task ExecuteAsync_with_plain_value_succeeds()
    {
        var policy = ResiliencePolicy.Create<Error>()
            .WithTimeout(TimeSpan.FromSeconds(1))
            .Build();

        var result = await policy.ExecuteAsync(
            async ct =>
            {
                await Task.Delay(10, ct);
                return 42;
            });

        await Assert.That(result.IsSuccess).IsTrue();
        await Assert.That(result.GetValueOrThrow()).IsEqualTo(42);
    }

    #endregion

    #region Timeout Integration

    [Test]
    public async Task ExecuteAsync_applies_overall_timeout()
    {
        var policy = ResiliencePolicy.Create<Error>()
            .WithTimeout(TimeSpan.FromMilliseconds(100))
            .Build();

        var result = await policy.ExecuteAsync(
            async ct =>
            {
                await Task.Delay(500, ct);
                return Result.Success<int, Error>(42);
            });

        await Assert.That(result.IsFailure).IsTrue();
        var error = result.Match(_ => null!, e => e);
        await Assert.That(error).IsAssignableTo<TimeoutError>();
    }

    [Test]
    public async Task ExecuteAsync_applies_per_attempt_timeout()
    {
        var attempts = 0;
        var policy = ResiliencePolicy.Create<Error>()
            .WithRetry(r => r
                .WithMaxAttempts(3)
                .WithTimeout(TimeSpan.FromMilliseconds(100)))
            .Build();

        var result = await policy.ExecuteAsync(
            async ct =>
            {
                attempts++;
                await Task.Delay(500, ct);
                return Result.Success<int, Error>(42);
            });

        await Assert.That(result.IsFailure).IsTrue();
        await Assert.That(attempts).IsEqualTo(3); // Should retry 3 times
        
        var error = result.Match(_ => null!, e => e);
        await Assert.That(error).IsAssignableTo<TimeoutError>();
    }

    [Test]
    public async Task ExecuteAsync_overall_timeout_prevents_infinite_retries()
    {
        var attempts = 0;
        var policy = ResiliencePolicy.Create<Error>()
            .WithTimeout(TimeSpan.FromMilliseconds(150))
            .WithRetry(r => r
                .WithMaxAttempts(100)
                .WithBackoff(Backoff.Constant(TimeSpan.FromMilliseconds(50))))
            .Build();

        var result = await policy.ExecuteAsync(
            async ct =>
            {
                attempts++;
                await Task.CompletedTask;
                return Result.Failure<int, Error>(new ExternalServiceError { Message = "fail" });
            });

        await Assert.That(result.IsFailure).IsTrue();
        await Assert.That(attempts).IsLessThan(100); // Should be stopped by overall timeout
        
        var error = result.Match(_ => null!, e => e);
        await Assert.That(error).IsAssignableTo<TimeoutError>();
    }

    #endregion

    #region Retry Integration

    [Test]
    public async Task ExecuteAsync_retries_on_failure()
    {
        var attempts = 0;
        var policy = ResiliencePolicy.Create<Error>()
            .WithRetry(r => r
                .WithMaxAttempts(3)
                .WithBackoff(Backoff.Constant(TimeSpan.FromMilliseconds(10))))
            .Build();

        var result = await policy.ExecuteAsync(
            async ct =>
            {
                attempts++;
                await Task.CompletedTask;
                return attempts < 3
                    ? Result.Failure<int, Error>(new ExternalServiceError { Message = "fail" })
                    : Result.Success<int, Error>(42);
            });

        await Assert.That(result.IsSuccess).IsTrue();
        await Assert.That(attempts).IsEqualTo(3);
    }

    [Test]
    public async Task ExecuteAsync_retry_respects_predicate()
    {
        var attempts = 0;
        var policy = ResiliencePolicy.Create<Error>()
            .WithRetry(r => r
                .WithMaxAttempts(5)
                .WithRetryWhen(error => error is ExternalServiceError))
            .Build();

        var result = await policy.ExecuteAsync(
            async ct =>
            {
                attempts++;
                await Task.CompletedTask;
                return Result.Failure<int, Error>(new ValidationError { Message = "not retryable" });
            });

        await Assert.That(result.IsFailure).IsTrue();
        await Assert.That(attempts).IsEqualTo(1); // Should not retry ValidationError
    }

    #endregion

    #region Circuit Breaker Integration

    [Test]
    public async Task ExecuteAsync_circuit_breaker_opens_after_threshold()
    {
        var policy = ResiliencePolicy.Create<Error>()
            .WithCircuitBreaker(cb => cb
                .WithFailureThreshold(3)
                .WithResetTimeout(TimeSpan.FromSeconds(60)))
            .Build();

        // Cause 3 failures to open circuit
        for (var i = 0; i < 3; i++)
        {
            await policy.ExecuteAsync(
                async ct =>
                {
                    await Task.CompletedTask;
                    return Result.Failure<int, Error>(new ExternalServiceError { Message = "fail" });
                });
        }

        // Next call should be rejected by circuit breaker
        var result = await policy.ExecuteAsync(
            async ct =>
            {
                await Task.CompletedTask;
                return Result.Success<int, Error>(42);
            });

        await Assert.That(result.IsFailure).IsTrue();
        var error = result.Match(_ => null!, e => e);
        await Assert.That(error).IsAssignableTo<CircuitBreakerOpenError>();
    }

    [Test]
    public async Task ExecuteAsync_circuit_breaker_protects_inside_retry()
    {
        var attempts = 0;
        var policy = ResiliencePolicy.Create<Error>()
            .WithRetry(r => r
                .WithMaxAttempts(10)
                .WithBackoff(Backoff.Constant(TimeSpan.FromMilliseconds(10))))
            .WithCircuitBreaker(cb => cb
                .WithFailureThreshold(3)
                .WithResetTimeout(TimeSpan.FromSeconds(60)))
            .Build();

        var result = await policy.ExecuteAsync(
            async ct =>
            {
                attempts++;
                await Task.CompletedTask;
                return Result.Failure<int, Error>(new ExternalServiceError { Message = "fail" });
            });

        await Assert.That(result.IsFailure).IsTrue();
        // Circuit should open after 3 attempts, stopping further retries
        await Assert.That(attempts).IsLessThanOrEqualTo(4); // 3 to open + 1 rejection
    }

    #endregion

    #region Bulkhead Integration

    [Test]
    public async Task ExecuteAsync_bulkhead_limits_concurrency()
    {
        var policy = ResiliencePolicy.Create<Error>()
            .WithBulkhead(b => b
                .WithMaxConcurrency(2)
                .WithMaxQueueSize(0))
            .Build();

        var executing = 0;
        var maxConcurrent = 0;

        var task1 = policy.ExecuteAsync(async ct =>
        {
            var current = Interlocked.Increment(ref executing);
            maxConcurrent = Math.Max(maxConcurrent, current);
            await Task.Delay(100, ct);
            Interlocked.Decrement(ref executing);
            return Result.Success<int, Error>(1);
        });

        var task2 = policy.ExecuteAsync(async ct =>
        {
            var current = Interlocked.Increment(ref executing);
            maxConcurrent = Math.Max(maxConcurrent, current);
            await Task.Delay(100, ct);
            Interlocked.Decrement(ref executing);
            return Result.Success<int, Error>(2);
        });

        await Task.Delay(20);

        var result3 = await policy.ExecuteAsync(
            async ct =>
            {
                await Task.CompletedTask;
                return Result.Success<int, Error>(3);
            });

        await Assert.That(maxConcurrent).IsLessThanOrEqualTo(2);
        await Assert.That(result3.IsFailure).IsTrue();
        
        var error = result3.Match(_ => null!, e => e);
        await Assert.That(error).IsAssignableTo<BulkheadRejectedError>();

        await Task.WhenAll(task1, task2);
    }

    #endregion

    #region Full Stack Scenario

    [Test]
    public async Task ExecuteAsync_full_resilience_stack_with_transient_failure()
    {
        var attempts = 0;
        var policy = ResiliencePolicy.Create<Error>()
            .WithTimeout(TimeSpan.FromSeconds(5))
            .WithRetry(r => r
                .WithMaxAttempts(3)
                .WithBackoff(Backoff.Exponential(TimeSpan.FromMilliseconds(50)))
                .WithTimeout(TimeSpan.FromMilliseconds(500)))
            .WithCircuitBreaker(cb => cb
                .WithFailureThreshold(5)
                .WithResetTimeout(TimeSpan.FromMinutes(1)))
            .WithBulkhead(b => b
                .WithMaxConcurrency(10))
            .Build();

        var result = await policy.ExecuteAsync(
            async ct =>
            {
                attempts++;
                await Task.Delay(10, ct);
                return attempts < 2
                    ? Result.Failure<string, Error>(new ExternalServiceError { Message = "transient" })
                    : Result.Success<string, Error>("success");
            });

        await Assert.That(result.IsSuccess).IsTrue();
        await Assert.That(result.GetValueOrThrow()).IsEqualTo("success");
        await Assert.That(attempts).IsEqualTo(2);
    }

    [Test]
    public async Task ExecuteAsync_full_resilience_stack_with_permanent_failure()
    {
        var attempts = 0;
        var policy = ResiliencePolicy.Create<Error>()
            .WithTimeout(TimeSpan.FromSeconds(5))
            .WithRetry(r => r
                .WithMaxAttempts(3)
                .WithBackoff(Backoff.Constant(TimeSpan.FromMilliseconds(10))))
            .WithCircuitBreaker(cb => cb
                .WithFailureThreshold(5))
            .Build();

        var result = await policy.ExecuteAsync(
            async ct =>
            {
                attempts++;
                await Task.CompletedTask;
                return Result.Failure<string, Error>(new ExternalServiceError { Message = "permanent" });
            });

        await Assert.That(result.IsFailure).IsTrue();
        await Assert.That(attempts).IsEqualTo(3); // All retries exhausted
        
        var error = result.Match(_ => null!, e => e);
        await Assert.That(error).IsAssignableTo<ExternalServiceError>();
    }

    #endregion

    #region CancellationToken Threading

    [Test]
    public async Task ExecuteAsync_threads_cancellation_token_through_all_policies()
    {
        var policy = ResiliencePolicy.Create<Error>()
            .WithRetry(r => r.WithMaxAttempts(3))
            .WithCircuitBreaker(cb => cb.WithFailureThreshold(5))
            .WithBulkhead(b => b.WithMaxConcurrency(10))
            .Build();

        var cts = new CancellationTokenSource();
        cts.CancelAfter(50);

        var tokenReceived = false;

        await Assert.That(async () =>
        {
            await policy.ExecuteAsync(
                async ct =>
                {
                    tokenReceived = ct.CanBeCanceled;
                    await Task.Delay(5000, ct);
                    return Result.Success<int, Error>(42);
                },
                cts.Token);
        }).Throws<OperationCanceledException>();

        await Assert.That(tokenReceived).IsTrue();
    }

    #endregion

    #region Builder Patterns

    [Test]
    public async Task Builder_can_create_policy_with_only_timeout()
    {
        var policy = ResiliencePolicy.Create<Error>()
            .WithTimeout(TimeSpan.FromMilliseconds(100))
            .Build();

        var result = await policy.ExecuteAsync(
            async ct =>
            {
                await Task.Delay(10, ct);
                return Result.Success<int, Error>(42);
            });

        await Assert.That(result.IsSuccess).IsTrue();
    }

    [Test]
    public async Task Builder_can_create_policy_with_only_retry()
    {
        var policy = ResiliencePolicy.Create<Error>()
            .WithRetry(r => r.WithMaxAttempts(3))
            .Build();

        var attempts = 0;
        var result = await policy.ExecuteAsync(
            async ct =>
            {
                attempts++;
                await Task.CompletedTask;
                return attempts < 2
                    ? Result.Failure<int, Error>(new ExternalServiceError { Message = "fail" })
                    : Result.Success<int, Error>(42);
            });

        await Assert.That(result.IsSuccess).IsTrue();
        await Assert.That(attempts).IsEqualTo(2);
    }

    [Test]
    public async Task Builder_can_create_policy_with_custom_timeout_error()
    {
        var policy = ResiliencePolicy.Create<Error>()
            .WithTimeout(
                TimeSpan.FromMilliseconds(50),
                elapsed => new ExternalServiceError
                {
                    Message = "Custom timeout",
                    ServiceName = "Test"
                })
            .Build();

        var result = await policy.ExecuteAsync(
            async ct =>
            {
                await Task.Delay(500, ct);
                return Result.Success<int, Error>(42);
            });

        await Assert.That(result.IsFailure).IsTrue();
        var error = result.Match(_ => null!, e => e);
        await Assert.That(error).IsAssignableTo<ExternalServiceError>();
        await Assert.That(((ExternalServiceError)error).ServiceName).IsEqualTo("Test");
    }

    #endregion

    #region Real-World Payment Gateway Scenario

    [Test]
    public async Task ExecuteAsync_payment_gateway_with_full_resilience()
    {
        var attempts = 0;
        var circuitOpenEvents = 0;

        var policy = ResiliencePolicy.Create<Error>()
            .WithTimeout(TimeSpan.FromSeconds(30))
            .WithRetry(r => r
                .WithMaxAttempts(3)
                .WithBackoff(Backoff.Exponential(TimeSpan.FromMilliseconds(500)))
                .WithTimeout(TimeSpan.FromSeconds(5))
                .WithRetryWhen(error => error is ExternalServiceError or TimeoutError))
            .WithCircuitBreaker(cb => cb
                .WithFailureThreshold(10)
                .WithResetTimeout(TimeSpan.FromMinutes(2))
                .WithBreakWhen(error => error is ExternalServiceError)
                .OnStateChange((from, to) =>
                {
                    if (to == CircuitBreakerState.Open)
                        circuitOpenEvents++;
                }))
            .Build();

        var result = await policy.ExecuteAsync(
            async ct =>
            {
                attempts++;
                await Task.Delay(10, ct);
                return attempts < 2
                    ? Result.Failure<string, Error>(new ExternalServiceError
                    {
                        Message = "Payment provider unavailable",
                        ServiceName = "PaymentGateway"
                    })
                    : Result.Success<string, Error>("payment-id-12345");
            });

        await Assert.That(result.IsSuccess).IsTrue();
        await Assert.That(result.GetValueOrThrow()).IsEqualTo("payment-id-12345");
        await Assert.That(attempts).IsEqualTo(2);
        await Assert.That(circuitOpenEvents).IsEqualTo(0); // Circuit should not open
    }

    #endregion
}

using DarkPeak.Functional;

namespace DarkPeak.Functional.Tests;

public class CircuitBreakerShould
{
    #region Execute (sync)

    [Test]
    public async Task Execute_returns_success_when_circuit_is_closed()
    {
        var breaker = CircuitBreaker.WithFailureThreshold(3);

        var result = breaker.Execute(() => Result.Success<int, Error>(42));

        await Assert.That(result.IsSuccess).IsTrue();
        await Assert.That(result.GetValueOrThrow()).IsEqualTo(42);
    }

    [Test]
    public async Task Execute_returns_failure_and_counts_toward_threshold()
    {
        var breaker = CircuitBreaker.WithFailureThreshold(3);

        var result = breaker.Execute(
            () => Result.Failure<int, Error>(new ExternalServiceError { Message = "fail" }));

        await Assert.That(result.IsFailure).IsTrue();
        var error = result.Match(_ => null!, e => e);
        await Assert.That(error).IsAssignableTo<ExternalServiceError>();
    }

    [Test]
    public async Task Execute_opens_circuit_after_failure_threshold()
    {
        var breaker = CircuitBreaker.WithFailureThreshold(3)
            .WithResetTimeout(TimeSpan.FromSeconds(60));

        for (var i = 0; i < 3; i++)
        {
            breaker.Execute(
                () => Result.Failure<int, Error>(new ExternalServiceError { Message = $"fail {i}" }));
        }

        var result = breaker.Execute(() => Result.Success<int, Error>(42));

        await Assert.That(result.IsFailure).IsTrue();
        var error = result.Match(_ => null!, e => e);
        await Assert.That(error).IsAssignableTo<CircuitBreakerOpenError>();
    }

    [Test]
    public async Task Execute_resets_failure_count_on_success()
    {
        var breaker = CircuitBreaker.WithFailureThreshold(3);

        breaker.Execute(() => Result.Failure<int, Error>(new ExternalServiceError { Message = "fail 1" }));
        breaker.Execute(() => Result.Failure<int, Error>(new ExternalServiceError { Message = "fail 2" }));
        breaker.Execute(() => Result.Success<int, Error>(42));

        // Should not be open because success reset the counter
        var result = breaker.Execute(() => Result.Success<int, Error>(99));
        await Assert.That(result.IsSuccess).IsTrue();
        await Assert.That(result.GetValueOrThrow()).IsEqualTo(99);
    }

    [Test]
    public async Task Execute_respects_break_predicate()
    {
        var breaker = CircuitBreaker.WithFailureThreshold(2)
            .WithResetTimeout(TimeSpan.FromSeconds(60))
            .WithBreakWhen(error => error is ExternalServiceError);

        // These failures don't match the predicate — should not trip
        for (var i = 0; i < 5; i++)
        {
            breaker.Execute(
                () => Result.Failure<int, Error>(new NotFoundError { Message = "not found" }));
        }

        var result = breaker.Execute(() => Result.Success<int, Error>(42));
        await Assert.That(result.IsSuccess).IsTrue();
    }

    [Test]
    public async Task Execute_trips_only_on_matching_predicate_errors()
    {
        var breaker = CircuitBreaker.WithFailureThreshold(2)
            .WithResetTimeout(TimeSpan.FromSeconds(60))
            .WithBreakWhen(error => error is ExternalServiceError);

        breaker.Execute(() => Result.Failure<int, Error>(new ExternalServiceError { Message = "fail 1" }));
        breaker.Execute(() => Result.Failure<int, Error>(new ExternalServiceError { Message = "fail 2" }));

        var result = breaker.Execute(() => Result.Success<int, Error>(42));
        await Assert.That(result.IsFailure).IsTrue();
        var error = result.Match(_ => null!, e => e);
        await Assert.That(error).IsAssignableTo<CircuitBreakerOpenError>();
    }

    #endregion

    #region ExecuteAsync

    [Test]
    public async Task ExecuteAsync_returns_success_when_circuit_is_closed()
    {
        var breaker = CircuitBreaker.WithFailureThreshold(3);

        var result = await breaker.ExecuteAsync(
            () => Task.FromResult(Result.Success<int, Error>(42)));

        await Assert.That(result.IsSuccess).IsTrue();
        await Assert.That(result.GetValueOrThrow()).IsEqualTo(42);
    }

    [Test]
    public async Task ExecuteAsync_opens_circuit_after_failure_threshold()
    {
        var breaker = CircuitBreaker.WithFailureThreshold(2)
            .WithResetTimeout(TimeSpan.FromSeconds(60));

        for (var i = 0; i < 2; i++)
        {
            await breaker.ExecuteAsync(
                () => Task.FromResult(Result.Failure<int, Error>(
                    new ExternalServiceError { Message = $"fail {i}" })));
        }

        var result = await breaker.ExecuteAsync(
            () => Task.FromResult(Result.Success<int, Error>(42)));

        await Assert.That(result.IsFailure).IsTrue();
        var error = result.Match(_ => null!, e => e);
        await Assert.That(error).IsAssignableTo<CircuitBreakerOpenError>();
    }

    [Test]
    public async Task ExecuteAsync_resets_failure_count_on_success()
    {
        var breaker = CircuitBreaker.WithFailureThreshold(3);

        await breaker.ExecuteAsync(
            () => Task.FromResult(Result.Failure<int, Error>(
                new ExternalServiceError { Message = "fail" })));
        await breaker.ExecuteAsync(
            () => Task.FromResult(Result.Success<int, Error>(42)));

        // Counter should be reset — two more failures should not trip
        await breaker.ExecuteAsync(
            () => Task.FromResult(Result.Failure<int, Error>(
                new ExternalServiceError { Message = "fail" })));
        await breaker.ExecuteAsync(
            () => Task.FromResult(Result.Failure<int, Error>(
                new ExternalServiceError { Message = "fail" })));

        var result = await breaker.ExecuteAsync(
            () => Task.FromResult(Result.Success<int, Error>(99)));
        await Assert.That(result.IsSuccess).IsTrue();
    }

    #endregion

    #region State Transitions

    [Test]
    public async Task Circuit_transitions_to_half_open_after_reset_timeout()
    {
        var breaker = CircuitBreaker.WithFailureThreshold(1)
            .WithResetTimeout(TimeSpan.FromMilliseconds(50));

        breaker.Execute(() => Result.Failure<int, Error>(
            new ExternalServiceError { Message = "fail" }));

        // Circuit should be open
        var openResult = breaker.Execute(() => Result.Success<int, Error>(42));
        await Assert.That(openResult.IsFailure).IsTrue();

        // Wait for reset timeout
        await Task.Delay(100);

        // Circuit should be half-open — allow one request through
        var halfOpenResult = breaker.Execute(() => Result.Success<int, Error>(42));
        await Assert.That(halfOpenResult.IsSuccess).IsTrue();
        await Assert.That(halfOpenResult.GetValueOrThrow()).IsEqualTo(42);
    }

    [Test]
    public async Task Circuit_closes_on_success_in_half_open()
    {
        var breaker = CircuitBreaker.WithFailureThreshold(1)
            .WithResetTimeout(TimeSpan.FromMilliseconds(50));

        breaker.Execute(() => Result.Failure<int, Error>(
            new ExternalServiceError { Message = "fail" }));
        await Task.Delay(100);

        // Half-open: success should close
        breaker.Execute(() => Result.Success<int, Error>(42));

        // Should now be closed — multiple successes should work
        var result1 = breaker.Execute(() => Result.Success<int, Error>(1));
        var result2 = breaker.Execute(() => Result.Success<int, Error>(2));
        await Assert.That(result1.IsSuccess).IsTrue();
        await Assert.That(result2.IsSuccess).IsTrue();
    }

    [Test]
    public async Task Circuit_reopens_on_failure_in_half_open()
    {
        var breaker = CircuitBreaker.WithFailureThreshold(1)
            .WithResetTimeout(TimeSpan.FromMilliseconds(50));

        breaker.Execute(() => Result.Failure<int, Error>(
            new ExternalServiceError { Message = "fail" }));
        await Task.Delay(100);

        // Half-open: failure should reopen
        breaker.Execute(() => Result.Failure<int, Error>(
            new ExternalServiceError { Message = "fail again" }));

        // Should be open again
        var result = breaker.Execute(() => Result.Success<int, Error>(42));
        await Assert.That(result.IsFailure).IsTrue();
        var error = result.Match(_ => null!, e => e);
        await Assert.That(error).IsAssignableTo<CircuitBreakerOpenError>();
    }

    #endregion

    #region OnStateChange Callback

    [Test]
    public async Task OnStateChange_is_invoked_on_transition_to_open()
    {
        var transitions = new List<(CircuitBreakerState From, CircuitBreakerState To)>();

        var breaker = CircuitBreaker.WithFailureThreshold(2)
            .WithResetTimeout(TimeSpan.FromSeconds(60))
            .OnStateChange((from, to) => transitions.Add((from, to)));

        breaker.Execute(() => Result.Failure<int, Error>(
            new ExternalServiceError { Message = "fail 1" }));
        breaker.Execute(() => Result.Failure<int, Error>(
            new ExternalServiceError { Message = "fail 2" }));

        await Assert.That(transitions).Count().IsEqualTo(1);
        await Assert.That(transitions[0].From).IsEqualTo(CircuitBreakerState.Closed);
        await Assert.That(transitions[0].To).IsEqualTo(CircuitBreakerState.Open);
    }

    [Test]
    public async Task OnStateChange_is_invoked_through_full_cycle()
    {
        var transitions = new List<(CircuitBreakerState From, CircuitBreakerState To)>();

        var breaker = CircuitBreaker.WithFailureThreshold(1)
            .WithResetTimeout(TimeSpan.FromMilliseconds(50))
            .OnStateChange((from, to) => transitions.Add((from, to)));

        // Closed -> Open
        breaker.Execute(() => Result.Failure<int, Error>(
            new ExternalServiceError { Message = "fail" }));
        await Task.Delay(100);

        // Open -> HalfOpen -> Closed (on success in half-open)
        breaker.Execute(() => Result.Success<int, Error>(42));

        await Assert.That(transitions).Count().IsEqualTo(3);
        await Assert.That(transitions[0]).IsEqualTo((CircuitBreakerState.Closed, CircuitBreakerState.Open));
        await Assert.That(transitions[1]).IsEqualTo((CircuitBreakerState.Open, CircuitBreakerState.HalfOpen));
        await Assert.That(transitions[2]).IsEqualTo((CircuitBreakerState.HalfOpen, CircuitBreakerState.Closed));
    }

    #endregion

    #region CircuitBreakerOpenError

    [Test]
    public async Task CircuitBreakerOpenError_contains_retry_after()
    {
        var breaker = CircuitBreaker.WithFailureThreshold(1)
            .WithResetTimeout(TimeSpan.FromSeconds(30));

        breaker.Execute(() => Result.Failure<int, Error>(
            new ExternalServiceError { Message = "fail" }));

        var result = breaker.Execute(() => Result.Success<int, Error>(42));
        var error = result.Match(_ => null!, e => e);
        await Assert.That(error).IsAssignableTo<CircuitBreakerOpenError>();

        var cbError = (CircuitBreakerOpenError)error;
        await Assert.That(cbError.RetryAfter).IsNotNull();
        await Assert.That(cbError.RetryAfter!.Value).IsGreaterThan(TimeSpan.Zero);
    }

    #endregion

    #region Validation

    [Test]
    public async Task WithFailureThreshold_throws_on_zero()
    {
        await Assert.That(() => CircuitBreaker.WithFailureThreshold(0))
            .Throws<ArgumentOutOfRangeException>();
    }

    [Test]
    public async Task WithFailureThreshold_throws_on_negative()
    {
        await Assert.That(() => CircuitBreaker.WithFailureThreshold(-1))
            .Throws<ArgumentOutOfRangeException>();
    }

    [Test]
    public async Task WithResetTimeout_throws_on_zero()
    {
        await Assert.That(() => CircuitBreaker.WithFailureThreshold(3)
            .WithResetTimeout(TimeSpan.Zero))
            .Throws<ArgumentOutOfRangeException>();
    }

    [Test]
    public async Task WithResetTimeout_throws_on_negative()
    {
        await Assert.That(() => CircuitBreaker.WithFailureThreshold(3)
            .WithResetTimeout(TimeSpan.FromSeconds(-1)))
            .Throws<ArgumentOutOfRangeException>();
    }

    #endregion
}

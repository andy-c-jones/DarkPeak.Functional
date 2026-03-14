using DarkPeak.Functional;
using DarkPeak.Functional.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace DarkPeak.Functional.HealthChecks.Tests;

public class CircuitBreakerHealthCheckShould
{
    #region State Mapping

    [Test]
    public async Task Report_healthy_when_circuit_is_closed()
    {
        var breaker = CircuitBreaker.WithFailureThreshold(3);
        var healthCheck = new CircuitBreakerHealthCheck(breaker);

        var result = await healthCheck.CheckHealthAsync(CreateContext());

        await Assert.That(result.Status).IsEqualTo(HealthStatus.Healthy);
        await Assert.That(result.Description).IsEqualTo("Circuit breaker is Closed");
    }

    [Test]
    public async Task Report_degraded_when_circuit_is_half_open()
    {
        var breaker = CircuitBreaker.WithFailureThreshold(1)
            .WithResetTimeout(TimeSpan.FromMilliseconds(500));

        breaker.Execute(() => Result.Failure<int, Error>(
            new ExternalServiceError { Message = "fail" }));

        await Task.Delay(1000);

        // After reset timeout, GetSnapshot reports effective state as HalfOpen
        var healthCheck = new CircuitBreakerHealthCheck(breaker);
        var result = await healthCheck.CheckHealthAsync(CreateContext());

        await Assert.That(result.Status).IsEqualTo(HealthStatus.Degraded);
        await Assert.That(result.Description).IsEqualTo("Circuit breaker is HalfOpen");
    }

    [Test]
    public async Task Report_unhealthy_when_circuit_is_open()
    {
        var breaker = CircuitBreaker.WithFailureThreshold(2)
            .WithResetTimeout(TimeSpan.FromSeconds(60));

        breaker.Execute(() => Result.Failure<int, Error>(
            new ExternalServiceError { Message = "fail 1" }));
        breaker.Execute(() => Result.Failure<int, Error>(
            new ExternalServiceError { Message = "fail 2" }));

        var healthCheck = new CircuitBreakerHealthCheck(breaker);
        var result = await healthCheck.CheckHealthAsync(CreateContext());

        await Assert.That(result.Status).IsEqualTo(HealthStatus.Unhealthy);
        await Assert.That(result.Description).Contains("Circuit breaker is Open");
        await Assert.That(result.Description).Contains("2 consecutive failures");
    }

    #endregion

    #region Data Dictionary

    [Test]
    public async Task Include_state_and_failure_count_in_data_when_closed()
    {
        var breaker = CircuitBreaker.WithFailureThreshold(3);
        var healthCheck = new CircuitBreakerHealthCheck(breaker);

        var result = await healthCheck.CheckHealthAsync(CreateContext());

        await Assert.That(result.Data["state"]).IsEqualTo("Closed");
        await Assert.That(result.Data["failureCount"]).IsEqualTo(0);
    }

    [Test]
    public async Task Include_reset_time_in_data_when_open()
    {
        var breaker = CircuitBreaker.WithFailureThreshold(1)
            .WithResetTimeout(TimeSpan.FromSeconds(60));

        breaker.Execute(() => Result.Failure<int, Error>(
            new ExternalServiceError { Message = "fail" }));

        var healthCheck = new CircuitBreakerHealthCheck(breaker);
        var result = await healthCheck.CheckHealthAsync(CreateContext());

        await Assert.That(result.Data.ContainsKey("resetTime")).IsTrue();
        await Assert.That(result.Data["state"]).IsEqualTo("Open");
        await Assert.That(result.Data["failureCount"]).IsEqualTo(1);
    }

    [Test]
    public async Task Include_failure_count_after_multiple_failures()
    {
        var breaker = CircuitBreaker.WithFailureThreshold(5)
            .WithResetTimeout(TimeSpan.FromSeconds(60));

        for (var i = 0; i < 5; i++)
        {
            breaker.Execute(() => Result.Failure<int, Error>(
                new ExternalServiceError { Message = $"fail {i}" }));
        }

        var healthCheck = new CircuitBreakerHealthCheck(breaker);
        var result = await healthCheck.CheckHealthAsync(CreateContext());

        await Assert.That(result.Data["failureCount"]).IsEqualTo(5);
    }

    #endregion

    #region Description Format

    [Test]
    public async Task Format_open_description_with_failure_count_and_reset_time()
    {
        var breaker = CircuitBreaker.WithFailureThreshold(3)
            .WithResetTimeout(TimeSpan.FromMinutes(5));

        for (var i = 0; i < 3; i++)
        {
            breaker.Execute(() => Result.Failure<int, Error>(
                new ExternalServiceError { Message = $"fail {i}" }));
        }

        var healthCheck = new CircuitBreakerHealthCheck(breaker);
        var result = await healthCheck.CheckHealthAsync(CreateContext());

        await Assert.That(result.Description).Contains("3 consecutive failures");
        await Assert.That(result.Description).Contains("Resets at");
    }

    #endregion

    #region Constructor Validation

    [Test]
    public async Task Throw_when_circuit_breaker_is_null()
    {
        await Assert.That(() => new CircuitBreakerHealthCheck(null!))
            .Throws<ArgumentNullException>();
    }

    #endregion

    private static HealthCheckContext CreateContext() =>
        new()
        {
            Registration = new HealthCheckRegistration(
                "test",
                new CircuitBreakerHealthCheck(CircuitBreaker.WithFailureThreshold(1)),
                failureStatus: null,
                tags: null)
        };
}

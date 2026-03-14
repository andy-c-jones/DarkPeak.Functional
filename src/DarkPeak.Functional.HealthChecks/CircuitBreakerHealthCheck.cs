using Microsoft.Extensions.Diagnostics.HealthChecks;
using DarkPeak.Functional;

namespace DarkPeak.Functional.HealthChecks;

/// <summary>
/// An <see cref="IHealthCheck"/> that reports the current state of a <see cref="CircuitBreakerPolicy"/>.
/// </summary>
/// <remarks>
/// <list type="bullet">
///   <item><term>Closed</term><description><see cref="HealthStatus.Healthy"/> — normal operation.</description></item>
///   <item><term>HalfOpen</term><description><see cref="HealthStatus.Degraded"/> — probe request allowed.</description></item>
///   <item><term>Open</term><description><see cref="HealthStatus.Unhealthy"/> — requests rejected.</description></item>
/// </list>
/// </remarks>
public sealed class CircuitBreakerHealthCheck : IHealthCheck
{
    private readonly CircuitBreakerPolicy _circuitBreaker;

    /// <summary>
    /// Creates a health check that monitors the specified circuit breaker.
    /// </summary>
    /// <param name="circuitBreaker">The circuit breaker policy to monitor.</param>
    public CircuitBreakerHealthCheck(CircuitBreakerPolicy circuitBreaker)
    {
        ArgumentNullException.ThrowIfNull(circuitBreaker);
        _circuitBreaker = circuitBreaker;
    }

    /// <inheritdoc />
    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var snapshot = _circuitBreaker.GetSnapshot();

        var data = new Dictionary<string, object>
        {
            ["state"] = snapshot.State.ToString(),
            ["failureCount"] = snapshot.ConsecutiveFailures
        };

        var result = snapshot.State switch
        {
            CircuitBreakerState.Closed => HealthCheckResult.Healthy(
                "Circuit breaker is Closed",
                data),

            CircuitBreakerState.HalfOpen => HealthCheckResult.Degraded(
                "Circuit breaker is HalfOpen",
                data: data),

            CircuitBreakerState.Open => BuildOpenResult(snapshot, data),

            _ => HealthCheckResult.Unhealthy(
                $"Circuit breaker is in unknown state: {snapshot.State}",
                data: data)
        };

        return Task.FromResult(result);
    }

    private static HealthCheckResult BuildOpenResult(
        CircuitBreakerSnapshot snapshot,
        Dictionary<string, object> data)
    {
        var resetTime = snapshot.LastFailureTime.HasValue
            ? snapshot.LastFailureTime.Value + snapshot.ResetTimeout
            : (DateTimeOffset?)null;

        if (resetTime.HasValue)
            data["resetTime"] = resetTime.Value.ToString("o");

        var description = resetTime.HasValue
            ? $"Circuit breaker is Open. {snapshot.ConsecutiveFailures} consecutive failures. Resets at {resetTime.Value:u}"
            : $"Circuit breaker is Open. {snapshot.ConsecutiveFailures} consecutive failures.";

        return HealthCheckResult.Unhealthy(description, data: data);
    }
}

using DarkPeak.Functional;
using DarkPeak.Functional.HealthChecks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace DarkPeak.Functional.HealthChecks.Tests;

public class HealthChecksBuilderExtensionsShould
{
    #region Circuit Breaker Registration

    [Test]
    public async Task Register_circuit_breaker_health_check()
    {
        var services = new ServiceCollection();
        var breaker = CircuitBreaker.WithFailureThreshold(3);

        services.AddHealthChecks()
            .AddCircuitBreakerHealthCheck("test-breaker", breaker);

        await using var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<
            Microsoft.Extensions.Options.IOptions<HealthCheckServiceOptions>>().Value;

        var registration = options.Registrations.Single(r => r.Name == "test-breaker");
        await Assert.That(registration.Name).IsEqualTo("test-breaker");

        var healthCheck = registration.Factory(provider);
        await Assert.That(healthCheck).IsAssignableTo<CircuitBreakerHealthCheck>();
    }

    [Test]
    public async Task Register_circuit_breaker_health_check_with_tags()
    {
        var services = new ServiceCollection();
        var breaker = CircuitBreaker.WithFailureThreshold(3);

        services.AddHealthChecks()
            .AddCircuitBreakerHealthCheck("test-breaker", breaker, tags: ["ready", "live"]);

        await using var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<
            Microsoft.Extensions.Options.IOptions<HealthCheckServiceOptions>>().Value;

        var registration = options.Registrations.Single(r => r.Name == "test-breaker");
        await Assert.That(registration.Tags).Contains("ready");
        await Assert.That(registration.Tags).Contains("live");
    }

    #endregion

    #region Cache Provider Registration

    [Test]
    public async Task Register_cache_provider_health_check_with_typed_provider()
    {
        var services = new ServiceCollection();
        var cacheProvider = new FakeCacheProvider();

        services.AddHealthChecks()
            .AddCacheProviderHealthCheck("test-cache", cacheProvider, probeKey: "health-probe");

        await using var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<
            Microsoft.Extensions.Options.IOptions<HealthCheckServiceOptions>>().Value;

        var registration = options.Registrations.Single(r => r.Name == "test-cache");
        var healthCheck = registration.Factory(provider);
        await Assert.That(healthCheck).IsAssignableTo<CacheProviderHealthCheck<string, string>>();
    }

    [Test]
    public async Task Register_cache_provider_health_check_with_tags()
    {
        var services = new ServiceCollection();
        var cacheProvider = new FakeCacheProvider();

        services.AddHealthChecks()
            .AddCacheProviderHealthCheck("test-cache", cacheProvider, probeKey: "health-probe", tags: ["ready"]);

        await using var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<
            Microsoft.Extensions.Options.IOptions<HealthCheckServiceOptions>>().Value;

        var registration = options.Registrations.Single(r => r.Name == "test-cache");
        await Assert.That(registration.Tags).Contains("ready");
    }

    [Test]
    public async Task Execute_typed_provider_probe_on_health_check()
    {
        var services = new ServiceCollection();
        var cacheProvider = new FakeCacheProvider();

        services.AddHealthChecks()
            .AddCacheProviderHealthCheck("test-cache", cacheProvider, probeKey: "health-probe");

        await using var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<
            Microsoft.Extensions.Options.IOptions<HealthCheckServiceOptions>>().Value;

        var registration = options.Registrations.Single(r => r.Name == "test-cache");
        var healthCheck = registration.Factory(provider);

        var result = await healthCheck.CheckHealthAsync(new HealthCheckContext
        {
            Registration = registration
        });

        await Assert.That(result.Status).IsEqualTo(HealthStatus.Healthy);
        await Assert.That(cacheProvider.GetAsyncCallCount).IsGreaterThanOrEqualTo(1);
    }

    [Test]
    public async Task Report_unhealthy_when_cache_provider_throws()
    {
        var cacheProvider = new FailingCacheProvider();

        var healthCheck = new CacheProviderHealthCheck<string, string>(cacheProvider, "probe");
        var result = await healthCheck.CheckHealthAsync(new HealthCheckContext
        {
            Registration = new HealthCheckRegistration(
                "test", healthCheck, failureStatus: null, tags: null)
        });

        await Assert.That(result.Status).IsEqualTo(HealthStatus.Unhealthy);
        await Assert.That(result.Description).IsEqualTo("Cache provider is unreachable");
        await Assert.That(result.Exception).IsAssignableTo<InvalidOperationException>();
    }

    #endregion

    #region Chaining

    [Test]
    public async Task Support_fluent_chaining_of_multiple_health_checks()
    {
        var services = new ServiceCollection();
        var breaker1 = CircuitBreaker.WithFailureThreshold(3);
        var breaker2 = CircuitBreaker.WithFailureThreshold(5);
        var cacheProvider = new FakeCacheProvider();

        services.AddHealthChecks()
            .AddCircuitBreakerHealthCheck("breaker-1", breaker1)
            .AddCircuitBreakerHealthCheck("breaker-2", breaker2)
            .AddCacheProviderHealthCheck("cache-1", cacheProvider, probeKey: "probe");

        await using var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<
            Microsoft.Extensions.Options.IOptions<HealthCheckServiceOptions>>().Value;

        await Assert.That(options.Registrations.Count).IsEqualTo(3);
    }

    #endregion

    #region Argument Validation

    [Test]
    public async Task Throw_when_circuit_breaker_name_is_null()
    {
        var services = new ServiceCollection();
        var builder = services.AddHealthChecks();
        var breaker = CircuitBreaker.WithFailureThreshold(3);

        await Assert.That(() => builder.AddCircuitBreakerHealthCheck(null!, breaker))
            .Throws<ArgumentNullException>();
    }

    [Test]
    public async Task Throw_when_circuit_breaker_policy_is_null()
    {
        var services = new ServiceCollection();
        var builder = services.AddHealthChecks();

        await Assert.That(() => builder.AddCircuitBreakerHealthCheck("test", null!))
            .Throws<ArgumentNullException>();
    }

    [Test]
    public async Task Throw_when_cache_provider_is_null()
    {
        var services = new ServiceCollection();
        var builder = services.AddHealthChecks();

        await Assert.That(() => builder.AddCacheProviderHealthCheck<string, string>("test", null!, "key"))
            .Throws<ArgumentNullException>();
    }

    #endregion

    private sealed class FakeCacheProvider : ICacheProvider<string, string>
    {
        public int GetAsyncCallCount;

        public Option<string> Get(string key) => Option.None<string>();

        public Task<Option<string>> GetAsync(string key)
        {
            Interlocked.Increment(ref GetAsyncCallCount);
            return Task.FromResult(Option.None<string>());
        }

        public void Set(string key, string value, TimeSpan? expiration) { }

        public Task SetAsync(string key, string value, TimeSpan? expiration) =>
            Task.CompletedTask;

        public void Remove(string key) { }

        public Task RemoveAsync(string key) => Task.CompletedTask;
    }

    private sealed class FailingCacheProvider : ICacheProvider<string, string>
    {
        public Option<string> Get(string key) => throw new InvalidOperationException("Connection refused");

        public Task<Option<string>> GetAsync(string key) =>
            throw new InvalidOperationException("Connection refused");

        public void Set(string key, string value, TimeSpan? expiration) =>
            throw new InvalidOperationException("Connection refused");

        public Task SetAsync(string key, string value, TimeSpan? expiration) =>
            throw new InvalidOperationException("Connection refused");

        public void Remove(string key) => throw new InvalidOperationException("Connection refused");

        public Task RemoveAsync(string key) => throw new InvalidOperationException("Connection refused");
    }
}

using DarkPeak.Functional;

namespace DarkPeak.Functional.Tests;

public class TimeoutPolicyShould
{
    #region Basic Timeout Behavior

    [Test]
    public async Task ExecuteAsync_returns_success_when_operation_completes_within_timeout()
    {
        var timeout = Timeout.Create()
            .WithTimeout(TimeSpan.FromSeconds(1));

        var result = await timeout.ExecuteAsync<int, Error>(
            async ct =>
            {
                await Task.Delay(10, ct);
                return Result.Success<int, Error>(42);
            });

        await Assert.That(result.IsSuccess).IsTrue();
        await Assert.That(result.GetValueOrThrow()).IsEqualTo(42);
    }

    [Test]
    public async Task ExecuteAsync_returns_timeout_error_when_operation_exceeds_timeout()
    {
        var timeout = Timeout.Create()
            .WithTimeout(TimeSpan.FromMilliseconds(50));

        var result = await timeout.ExecuteAsync<int, Error>(
            async ct =>
            {
                await Task.Delay(500, ct);
                return Result.Success<int, Error>(42);
            });

        await Assert.That(result.IsFailure).IsTrue();
        var error = result.Match(_ => null!, e => e);
        await Assert.That(error).IsAssignableTo<TimeoutError>();
        
        var timeoutError = (TimeoutError)error;
        await Assert.That(timeoutError.Timeout).IsEqualTo(TimeSpan.FromMilliseconds(50));
        await Assert.That(timeoutError.Elapsed).IsNotNull();
    }

    [Test]
    public async Task ExecuteAsync_returns_timeout_error_with_custom_message()
    {
        var timeout = Timeout.Create()
            .WithTimeout(TimeSpan.FromMilliseconds(50))
            .WithTimeoutError(elapsed => new TimeoutError
            {
                Message = $"Custom timeout message: {elapsed.TotalMilliseconds}ms",
                Elapsed = elapsed
            });

        var result = await timeout.ExecuteAsync<int, Error>(
            async ct =>
            {
                await Task.Delay(500, ct);
                return Result.Success<int, Error>(42);
            });

        await Assert.That(result.IsFailure).IsTrue();
        var error = result.Match(_ => null!, e => e);
        await Assert.That(error.Message).Contains("Custom timeout message");
    }

    [Test]
    public async Task ExecuteAsync_cancels_operation_on_timeout()
    {
        var timeout = Timeout.Create()
            .WithTimeout(TimeSpan.FromMilliseconds(50));
        
        var wasCancelled = false;

        var result = await timeout.ExecuteAsync<int, Error>(
            async ct =>
            {
                try
                {
                    await Task.Delay(500, ct);
                }
                catch (OperationCanceledException)
                {
                    wasCancelled = true;
                    throw;
                }
                return Result.Success<int, Error>(42);
            });

        await Assert.That(result.IsFailure).IsTrue();
        await Assert.That(wasCancelled).IsTrue();
    }

    #endregion

    #region Plain Value Overload

    [Test]
    public async Task ExecuteAsync_with_plain_value_returns_success_when_completes_within_timeout()
    {
        var timeout = Timeout.Create()
            .WithTimeout(TimeSpan.FromSeconds(1));

        var result = await timeout.ExecuteAsync<int, Error>(
            async ct =>
            {
                await Task.Delay(10, ct);
                return 42;
            });

        await Assert.That(result.IsSuccess).IsTrue();
        await Assert.That(result.GetValueOrThrow()).IsEqualTo(42);
    }

    [Test]
    public async Task ExecuteAsync_with_plain_value_returns_timeout_error_when_exceeds_timeout()
    {
        var timeout = Timeout.Create()
            .WithTimeout(TimeSpan.FromMilliseconds(50));

        var result = await timeout.ExecuteAsync<int, Error>(
            async ct =>
            {
                await Task.Delay(500, ct);
                return 42;
            });

        await Assert.That(result.IsFailure).IsTrue();
        var error = result.Match(_ => null!, e => e);
        await Assert.That(error).IsAssignableTo<TimeoutError>();
    }

    #endregion

    #region External Cancellation

    [Test]
    public async Task ExecuteAsync_respects_external_cancellation_token()
    {
        var timeout = Timeout.Create()
            .WithTimeout(TimeSpan.FromSeconds(10));

        var cts = new CancellationTokenSource();
        cts.CancelAfter(50);

        await Assert.That(async () =>
        {
            await timeout.ExecuteAsync<int, Error>(
                async ct =>
                {
                    await Task.Delay(5000, ct);
                    return Result.Success<int, Error>(42);
                },
                cts.Token);
        }).Throws<OperationCanceledException>();
    }

    [Test]
    public async Task ExecuteAsync_distinguishes_timeout_from_external_cancellation()
    {
        var timeout = Timeout.Create()
            .WithTimeout(TimeSpan.FromMilliseconds(50));

        var cts = new CancellationTokenSource();

        var result = await timeout.ExecuteAsync<int, Error>(
            async ct =>
            {
                await Task.Delay(500, ct);
                return Result.Success<int, Error>(42);
            },
            cts.Token);

        // Should return TimeoutError, not throw OperationCanceledException
        await Assert.That(result.IsFailure).IsTrue();
        var error = result.Match(_ => null!, e => e);
        await Assert.That(error).IsAssignableTo<TimeoutError>();
    }

    #endregion

    #region Validation

    [Test]
    public async Task WithTimeout_throws_on_zero()
    {
        await Assert.That(() => Timeout.Create().WithTimeout(TimeSpan.Zero))
            .Throws<ArgumentOutOfRangeException>();
    }

    [Test]
    public async Task WithTimeout_throws_on_negative()
    {
        await Assert.That(() => Timeout.Create().WithTimeout(TimeSpan.FromSeconds(-1)))
            .Throws<ArgumentOutOfRangeException>();
    }

    #endregion

    #region Custom Error Factory

    [Test]
    public async Task WithTimeoutError_uses_custom_error_type()
    {
        var timeout = Timeout.Create()
            .WithTimeout(TimeSpan.FromMilliseconds(50))
            .WithTimeoutError(elapsed => new ExternalServiceError
            {
                Message = "Service timed out",
                ServiceName = "TestService"
            });

        var result = await timeout.ExecuteAsync<int, Error>(
            async ct =>
            {
                await Task.Delay(500, ct);
                return Result.Success<int, Error>(42);
            });

        await Assert.That(result.IsFailure).IsTrue();
        var error = result.Match(_ => null!, e => e);
        await Assert.That(error).IsAssignableTo<ExternalServiceError>();
        await Assert.That(((ExternalServiceError)error).ServiceName).IsEqualTo("TestService");
    }

    #endregion

    #region Real-World Scenario

    [Test]
    public async Task Timeout_with_slow_operation_scenario()
    {
        var timeout = Timeout.Create()
            .WithTimeout(TimeSpan.FromMilliseconds(100));

        var attempts = 0;

        var result = await timeout.ExecuteAsync<string, Error>(
            async ct =>
            {
                attempts++;
                // Simulate a slow external API call
                await Task.Delay(500, ct);
                return Result.Success<string, Error>("data");
            });

        await Assert.That(result.IsFailure).IsTrue();
        await Assert.That(attempts).IsEqualTo(1);
        
        var error = result.Match(_ => null!, e => e);
        await Assert.That(error).IsAssignableTo<TimeoutError>();
    }

    #endregion
}

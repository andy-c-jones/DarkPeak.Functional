using DarkPeak.Functional;

namespace DarkPeak.Functional.Tests;

public class RetryShould
{
    #region Execute (sync)

    [Test]
    public async Task Execute_returns_success_on_first_attempt()
    {
        var attempts = 0;

        var result = Retry.WithMaxAttempts(3)
            .Execute(() =>
            {
                attempts++;
                return Result.Success<int, Error>(42);
            });

        await Assert.That(result.IsSuccess).IsTrue();
        await Assert.That(result.GetValueOrThrow()).IsEqualTo(42);
        await Assert.That(attempts).IsEqualTo(1);
    }

    [Test]
    public async Task Execute_retries_on_failure_then_succeeds()
    {
        var attempts = 0;

        var result = Retry.WithMaxAttempts(3)
            .Execute(() =>
            {
                attempts++;
                return attempts < 3
                    ? Result.Failure<int, Error>(new ExternalServiceError { Message = $"Attempt {attempts}" })
                    : Result.Success<int, Error>(42);
            });

        await Assert.That(result.IsSuccess).IsTrue();
        await Assert.That(result.GetValueOrThrow()).IsEqualTo(42);
        await Assert.That(attempts).IsEqualTo(3);
    }

    [Test]
    public async Task Execute_returns_last_failure_when_all_attempts_exhausted()
    {
        var attempts = 0;

        var result = Retry.WithMaxAttempts(3)
            .Execute(() =>
            {
                attempts++;
                return Result.Failure<int, Error>(
                    new ExternalServiceError { Message = $"Attempt {attempts}" });
            });

        await Assert.That(result.IsFailure).IsTrue();
        var error = result.Match(_ => (Error)null!, e => e);
        await Assert.That(error.Message).IsEqualTo("Attempt 3");
        await Assert.That(attempts).IsEqualTo(3);
    }

    [Test]
    public async Task Execute_with_single_attempt_does_not_retry()
    {
        var attempts = 0;

        var result = Retry.WithMaxAttempts(1)
            .Execute(() =>
            {
                attempts++;
                return Result.Failure<int, Error>(
                    new ExternalServiceError { Message = "fail" });
            });

        await Assert.That(result.IsFailure).IsTrue();
        await Assert.That(attempts).IsEqualTo(1);
    }

    #endregion

    #region ExecuteAsync

    [Test]
    public async Task ExecuteAsync_returns_success_on_first_attempt()
    {
        var attempts = 0;

        var result = await Retry.WithMaxAttempts(3)
            .ExecuteAsync(async () =>
            {
                attempts++;
                await Task.Delay(1);
                return Result.Success<int, Error>(42);
            });

        await Assert.That(result.IsSuccess).IsTrue();
        await Assert.That(attempts).IsEqualTo(1);
    }

    [Test]
    public async Task ExecuteAsync_retries_on_failure_then_succeeds()
    {
        var attempts = 0;

        var result = await Retry.WithMaxAttempts(3)
            .ExecuteAsync(async () =>
            {
                attempts++;
                await Task.Delay(1);
                return attempts < 2
                    ? Result.Failure<int, Error>(new ExternalServiceError { Message = "fail" })
                    : Result.Success<int, Error>(42);
            });

        await Assert.That(result.IsSuccess).IsTrue();
        await Assert.That(attempts).IsEqualTo(2);
    }

    [Test]
    public async Task ExecuteAsync_returns_last_failure_when_exhausted()
    {
        var attempts = 0;

        var result = await Retry.WithMaxAttempts(2)
            .ExecuteAsync(async () =>
            {
                attempts++;
                await Task.Delay(1);
                return Result.Failure<int, Error>(
                    new ExternalServiceError { Message = $"Attempt {attempts}" });
            });

        await Assert.That(result.IsFailure).IsTrue();
        var error = result.Match(_ => (Error)null!, e => e);
        await Assert.That(error.Message).IsEqualTo("Attempt 2");
    }

    #endregion

    #region WithRetryWhen

    [Test]
    public async Task WithRetryWhen_retries_only_matching_errors()
    {
        var attempts = 0;

        var result = Retry.WithMaxAttempts(3)
            .WithRetryWhen(e => e is ExternalServiceError)
            .Execute(() =>
            {
                attempts++;
                return Result.Failure<int, Error>(
                    new ExternalServiceError { Message = "transient" });
            });

        await Assert.That(attempts).IsEqualTo(3);
    }

    [Test]
    public async Task WithRetryWhen_stops_on_non_matching_error()
    {
        var attempts = 0;

        var result = Retry.WithMaxAttempts(3)
            .WithRetryWhen(e => e is ExternalServiceError)
            .Execute(() =>
            {
                attempts++;
                return Result.Failure<int, Error>(
                    new ValidationError { Message = "not retryable" });
            });

        await Assert.That(result.IsFailure).IsTrue();
        await Assert.That(attempts).IsEqualTo(1);
    }

    [Test]
    public async Task WithRetryWhen_retries_matching_then_stops_on_non_matching()
    {
        var attempts = 0;

        var result = Retry.WithMaxAttempts(5)
            .WithRetryWhen(e => e is ExternalServiceError)
            .Execute(() =>
            {
                attempts++;
                return attempts < 3
                    ? Result.Failure<int, Error>(new ExternalServiceError { Message = "transient" })
                    : Result.Failure<int, Error>(new ValidationError { Message = "permanent" });
            });

        await Assert.That(result.IsFailure).IsTrue();
        await Assert.That(attempts).IsEqualTo(3);
        var error = result.Match(_ => (Error)null!, e => e);
        await Assert.That(error.Message).IsEqualTo("permanent");
    }

    #endregion

    #region OnRetry callback

    [Test]
    public async Task OnRetry_is_called_before_each_retry()
    {
        var retryAttempts = new List<int>();

        Retry.WithMaxAttempts(3)
            .OnRetry((attempt, _) => retryAttempts.Add(attempt))
            .Execute(() => Result.Failure<int, Error>(
                new ExternalServiceError { Message = "fail" }));

        await Assert.That(retryAttempts.Count).IsEqualTo(2);
        await Assert.That(retryAttempts[0]).IsEqualTo(1);
        await Assert.That(retryAttempts[1]).IsEqualTo(2);
    }

    [Test]
    public async Task OnRetry_receives_the_error()
    {
        var errors = new List<string>();

        Retry.WithMaxAttempts(2)
            .OnRetry((_, error) => errors.Add(error.Message))
            .Execute(() => Result.Failure<int, Error>(
                new ExternalServiceError { Message = "service down" }));

        await Assert.That(errors.Count).IsEqualTo(1);
        await Assert.That(errors[0]).IsEqualTo("service down");
    }

    [Test]
    public async Task OnRetry_not_called_on_first_success()
    {
        var called = false;

        Retry.WithMaxAttempts(3)
            .OnRetry((_, _) => called = true)
            .Execute(() => Result.Success<int, Error>(42));

        await Assert.That(called).IsFalse();
    }

    #endregion

    #region Backoff strategies

    [Test]
    public async Task Backoff_None_returns_zero()
    {
        var delay = Backoff.None(1);

        await Assert.That(delay).IsEqualTo(TimeSpan.Zero);
    }

    [Test]
    public async Task Backoff_Constant_returns_same_delay()
    {
        var strategy = Backoff.Constant(TimeSpan.FromMilliseconds(100));

        await Assert.That(strategy(1)).IsEqualTo(TimeSpan.FromMilliseconds(100));
        await Assert.That(strategy(2)).IsEqualTo(TimeSpan.FromMilliseconds(100));
        await Assert.That(strategy(5)).IsEqualTo(TimeSpan.FromMilliseconds(100));
    }

    [Test]
    public async Task Backoff_Linear_increases_linearly()
    {
        var strategy = Backoff.Linear(
            TimeSpan.FromMilliseconds(100),
            TimeSpan.FromMilliseconds(50));

        await Assert.That(strategy(1)).IsEqualTo(TimeSpan.FromMilliseconds(100));
        await Assert.That(strategy(2)).IsEqualTo(TimeSpan.FromMilliseconds(150));
        await Assert.That(strategy(3)).IsEqualTo(TimeSpan.FromMilliseconds(200));
    }

    [Test]
    public async Task Backoff_Exponential_increases_exponentially()
    {
        var strategy = Backoff.Exponential(TimeSpan.FromMilliseconds(100));

        await Assert.That(strategy(1)).IsEqualTo(TimeSpan.FromMilliseconds(100));
        await Assert.That(strategy(2)).IsEqualTo(TimeSpan.FromMilliseconds(200));
        await Assert.That(strategy(3)).IsEqualTo(TimeSpan.FromMilliseconds(400));
    }

    [Test]
    public async Task Backoff_Exponential_with_custom_multiplier()
    {
        var strategy = Backoff.Exponential(TimeSpan.FromMilliseconds(100), 3.0);

        await Assert.That(strategy(1)).IsEqualTo(TimeSpan.FromMilliseconds(100));
        await Assert.That(strategy(2)).IsEqualTo(TimeSpan.FromMilliseconds(300));
        await Assert.That(strategy(3)).IsEqualTo(TimeSpan.FromMilliseconds(900));
    }

    [Test]
    public async Task Backoff_Exponential_with_max_delay_caps()
    {
        var strategy = Backoff.Exponential(
            TimeSpan.FromMilliseconds(100), 2.0, TimeSpan.FromMilliseconds(300));

        await Assert.That(strategy(1)).IsEqualTo(TimeSpan.FromMilliseconds(100));
        await Assert.That(strategy(2)).IsEqualTo(TimeSpan.FromMilliseconds(200));
        await Assert.That(strategy(3)).IsEqualTo(TimeSpan.FromMilliseconds(300));
        await Assert.That(strategy(4)).IsEqualTo(TimeSpan.FromMilliseconds(300));
    }

    #endregion

    #region Backoff with actual delay

    [Test]
    public async Task ExecuteAsync_with_backoff_delays_between_retries()
    {
        var attempts = 0;
        var sw = System.Diagnostics.Stopwatch.StartNew();

        var result = await Retry.WithMaxAttempts(3)
            .WithBackoff(Backoff.Constant(TimeSpan.FromMilliseconds(50)))
            .ExecuteAsync(async () =>
            {
                attempts++;
                await Task.CompletedTask;
                return Result.Failure<int, Error>(
                    new ExternalServiceError { Message = "fail" });
            });

        sw.Stop();

        await Assert.That(result.IsFailure).IsTrue();
        await Assert.That(attempts).IsEqualTo(3);
        // 2 retries * 50ms = ~100ms minimum
        await Assert.That(sw.ElapsedMilliseconds).IsGreaterThanOrEqualTo(80);
    }

    #endregion

    #region Validation

    [Test]
    public void WithMaxAttempts_throws_for_zero()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Retry.WithMaxAttempts(0));
    }

    [Test]
    public void WithMaxAttempts_throws_for_negative()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Retry.WithMaxAttempts(-1));
    }

    #endregion

    #region Full scenario

    [Test]
    public async Task Full_scenario_retry_with_backoff_and_predicate()
    {
        var attempts = 0;
        var retryLog = new List<(int Attempt, string Error)>();

        var result = await Retry.WithMaxAttempts(5)
            .WithBackoff(Backoff.Constant(TimeSpan.FromMilliseconds(10)))
            .WithRetryWhen(e => e is ExternalServiceError)
            .OnRetry((attempt, error) => retryLog.Add((attempt, error.Message)))
            .ExecuteAsync(async () =>
            {
                attempts++;
                await Task.Delay(1);
                return attempts < 4
                    ? Result.Failure<string, Error>(
                        new ExternalServiceError { Message = $"Service unavailable (attempt {attempts})" })
                    : Result.Success<string, Error>("Connected!");
            });

        await Assert.That(result.IsSuccess).IsTrue();
        await Assert.That(result.GetValueOrThrow()).IsEqualTo("Connected!");
        await Assert.That(attempts).IsEqualTo(4);
        await Assert.That(retryLog.Count).IsEqualTo(3);
    }

    #endregion
}

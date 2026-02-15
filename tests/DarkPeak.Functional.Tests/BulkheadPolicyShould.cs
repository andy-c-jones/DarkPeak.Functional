using DarkPeak.Functional;

namespace DarkPeak.Functional.Tests;

public class BulkheadPolicyShould
{
    #region Basic Bulkhead Behavior

    [Test]
    public async Task ExecuteAsync_allows_operations_within_concurrency_limit()
    {
        var bulkhead = Bulkhead.WithMaxConcurrency(3);

        var result = await bulkhead.ExecuteAsync<int, Error>(
            async ct =>
            {
                await Task.Delay(10, ct);
                return Result.Success<int, Error>(42);
            });

        await Assert.That(result.IsSuccess).IsTrue();
        await Assert.That(result.GetValueOrThrow()).IsEqualTo(42);
    }

    [Test]
    public async Task ExecuteAsync_allows_max_concurrent_operations()
    {
        var bulkhead = Bulkhead.WithMaxConcurrency(2)
            .WithMaxQueueSize(3); // Allow 3 to queue
        var executing = 0;
        var maxConcurrent = 0;

        var tasks = new List<Task<Result<int, Error>>>();
        
        for (var i = 0; i < 5; i++)
        {
            var task = bulkhead.ExecuteAsync<int, Error>(async ct =>
            {
                var current = Interlocked.Increment(ref executing);
                maxConcurrent = Math.Max(maxConcurrent, current);
                await Task.Delay(50, ct);
                Interlocked.Decrement(ref executing);
                return Result.Success<int, Error>(i);
            });
            
            tasks.Add(task);
        }

        var results = await Task.WhenAll(tasks);

        await Assert.That(maxConcurrent).IsLessThanOrEqualTo(2);
        await Assert.That(results.All(r => r.IsSuccess)).IsTrue();
    }

    [Test]
    public async Task ExecuteAsync_rejects_when_max_concurrency_and_queue_full()
    {
        var bulkhead = Bulkhead.WithMaxConcurrency(2)
            .WithMaxQueueSize(0); // No queue

        var executing = 0;
        var started = new SemaphoreSlim(0, 2);
        var tasks = new List<Task<Result<int, Error>>>();

        // Fill the bulkhead
        for (var i = 0; i < 2; i++)
        {
            tasks.Add(bulkhead.ExecuteAsync<int, Error>(async ct =>
            {
                Interlocked.Increment(ref executing);
                started.Release(); // Signal that this task has started
                await Task.Delay(500, ct);
                Interlocked.Decrement(ref executing);
                return Result.Success<int, Error>(i);
            }));
        }

        // Wait for both tasks to actually start executing
        await started.WaitAsync();
        await started.WaitAsync();

        // This should be rejected
        var result = await bulkhead.ExecuteAsync<int, Error>(
            async ct =>
            {
                await Task.CompletedTask;
                return Result.Success<int, Error>(999);
            });

        await Assert.That(result.IsFailure).IsTrue();
        var error = result.Match(_ => null!, e => e);
        await Assert.That(error).IsAssignableTo<BulkheadRejectedError>();

        await Task.WhenAll(tasks);
    }

    [Test]
    public async Task ExecuteAsync_queues_requests_when_concurrency_limit_reached()
    {
        var bulkhead = Bulkhead.WithMaxConcurrency(2)
            .WithMaxQueueSize(3);

        var tasks = new List<Task<Result<int, Error>>>();

        // Create 5 tasks (2 concurrent + 3 queued)
        for (var i = 0; i < 5; i++)
        {
            var index = i;
            tasks.Add(bulkhead.ExecuteAsync<int, Error>(async ct =>
            {
                await Task.Delay(50, ct);
                return Result.Success<int, Error>(index);
            }));
        }

        var results = await Task.WhenAll(tasks);

        await Assert.That(results.All(r => r.IsSuccess)).IsTrue();
        await Assert.That(results.Length).IsEqualTo(5);
    }

    #endregion

    #region Plain Value Overload

    [Test]
    public async Task ExecuteAsync_with_plain_value_allows_operations_within_limit()
    {
        var bulkhead = Bulkhead.WithMaxConcurrency(3);

        var result = await bulkhead.ExecuteAsync<int, Error>(
            async ct =>
            {
                await Task.Delay(10, ct);
                return 42;
            });

        await Assert.That(result.IsSuccess).IsTrue();
        await Assert.That(result.GetValueOrThrow()).IsEqualTo(42);
    }

    [Test]
    public async Task ExecuteAsync_with_plain_value_rejects_when_full()
    {
        var bulkhead = Bulkhead.WithMaxConcurrency(1)
            .WithMaxQueueSize(0);

        var started = new SemaphoreSlim(0, 1);
        
        var task1 = bulkhead.ExecuteAsync<int, Error>(async ct =>
        {
            started.Release(); // Signal that this task has started
            await Task.Delay(500, ct);
            return 1;
        });

        // Wait for task1 to actually start executing
        await started.WaitAsync();

        var result2 = await bulkhead.ExecuteAsync<int, Error>(
            async ct =>
            {
                await Task.CompletedTask;
                return 2;
            });

        await Assert.That(result2.IsFailure).IsTrue();
        var error = result2.Match(_ => null!, e => e);
        await Assert.That(error).IsAssignableTo<BulkheadRejectedError>();

        await task1;
    }

    #endregion

    #region OnRejected Callback

    [Test]
    public async Task OnRejected_is_called_when_request_rejected()
    {
        var rejectionCount = 0;
        var bulkhead = Bulkhead.WithMaxConcurrency(1)
            .WithMaxQueueSize(0)
            .OnRejected(() => rejectionCount++);

        var started = new SemaphoreSlim(0, 1);
        
        var task1 = bulkhead.ExecuteAsync<int, Error>(async ct =>
        {
            started.Release(); // Signal that this task has started
            await Task.Delay(100, ct);
            return Result.Success<int, Error>(1);
        });

        // Wait for task1 to actually start executing
        await started.WaitAsync();

        var result2 = await bulkhead.ExecuteAsync<int, Error>(
            async ct =>
            {
                await Task.CompletedTask;
                return Result.Success<int, Error>(2);
            });

        await Assert.That(rejectionCount).IsEqualTo(1);
        await Assert.That(result2.IsFailure).IsTrue();

        await task1;
    }

    #endregion

    #region Validation

    [Test]
    public async Task WithMaxConcurrency_throws_on_zero()
    {
        await Assert.That(() => Bulkhead.WithMaxConcurrency(0))
            .Throws<ArgumentOutOfRangeException>();
    }

    [Test]
    public async Task WithMaxConcurrency_throws_on_negative()
    {
        await Assert.That(() => Bulkhead.WithMaxConcurrency(-1))
            .Throws<ArgumentOutOfRangeException>();
    }

    [Test]
    public async Task WithMaxQueueSize_throws_on_negative()
    {
        await Assert.That(() => Bulkhead.WithMaxConcurrency(5)
            .WithMaxQueueSize(-1))
            .Throws<ArgumentOutOfRangeException>();
    }

    #endregion

    #region BulkheadRejectedError

    [Test]
    public async Task BulkheadRejectedError_contains_max_concurrency_and_queue_size()
    {
        var bulkhead = Bulkhead.WithMaxConcurrency(2)
            .WithMaxQueueSize(1);

        var started = new SemaphoreSlim(0, 2);

        // Fill bulkhead and queue
        var task1 = bulkhead.ExecuteAsync<int, Error>(async ct =>
        {
            started.Release(); // Signal that this task has started
            await Task.Delay(500, ct);
            return Result.Success<int, Error>(1);
        });

        var task2 = bulkhead.ExecuteAsync<int, Error>(async ct =>
        {
            started.Release(); // Signal that this task has started
            await Task.Delay(500, ct);
            return Result.Success<int, Error>(2);
        });

        // Wait for both executing tasks to start
        await started.WaitAsync();
        await started.WaitAsync();

        // Now start task3 which should be queued (won't execute until task1 or task2 completes)
        var task3 = bulkhead.ExecuteAsync<int, Error>(async ct =>
        {
            await Task.Delay(500, ct);
            return Result.Success<int, Error>(3);
        });

        // Give task3 a moment to be queued
        await Task.Yield();

        var result4 = await bulkhead.ExecuteAsync<int, Error>(
            async ct =>
            {
                await Task.CompletedTask;
                return Result.Success<int, Error>(4);
            });

        await Assert.That(result4.IsFailure).IsTrue();
        var error = result4.Match(_ => null!, e => e);
        await Assert.That(error).IsAssignableTo<BulkheadRejectedError>();

        var bulkheadError = (BulkheadRejectedError)error;
        await Assert.That(bulkheadError.MaxConcurrency).IsEqualTo(2);
        await Assert.That(bulkheadError.MaxQueueSize).IsEqualTo(1);

        await Task.WhenAll(task1, task2, task3);
    }

    #endregion

    #region Cancellation

    [Test]
    public async Task ExecuteAsync_respects_cancellation_token()
    {
        var bulkhead = Bulkhead.WithMaxConcurrency(10);
        var cts = new CancellationTokenSource();
        cts.CancelAfter(50);

        await Assert.That(async () =>
        {
            await bulkhead.ExecuteAsync<int, Error>(
                async ct =>
                {
                    await Task.Delay(5000, ct);
                    return Result.Success<int, Error>(42);
                },
                cts.Token);
        }).Throws<OperationCanceledException>();
    }

    #endregion

    #region Real-World Scenario

    [Test]
    public async Task Bulkhead_protects_downstream_service_from_overload()
    {
        var bulkhead = Bulkhead.WithMaxConcurrency(3)
            .WithMaxQueueSize(2);

        var rejectionCount = 0;
        var successCount = 0;

        bulkhead = bulkhead.OnRejected(() => Interlocked.Increment(ref rejectionCount));

        var tasks = new List<Task<Result<int, Error>>>();

        // Simulate 10 requests hitting the bulkhead
        for (var i = 0; i < 10; i++)
        {
            var index = i;
            tasks.Add(Task.Run(async () =>
            {
                var result = await bulkhead.ExecuteAsync<int, Error>(async ct =>
                {
                    await Task.Delay(50, ct);
                    Interlocked.Increment(ref successCount);
                    return Result.Success<int, Error>(index);
                });
                return result;
            }));
        }

        var results = await Task.WhenAll(tasks);

        var succeeded = results.Count(r => r.IsSuccess);
        var rejected = results.Count(r => r.IsFailure);

        await Assert.That(succeeded + rejected).IsEqualTo(10);
        await Assert.That(succeeded).IsEqualTo(successCount);
        await Assert.That(rejected).IsEqualTo(rejectionCount);
        await Assert.That(rejected).IsGreaterThan(0); // Some should be rejected
    }

    #endregion
}

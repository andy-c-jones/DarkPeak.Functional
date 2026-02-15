using DarkPeak.Functional;

namespace DarkPeak.Functional.Tests;

public class MemoizeResultShould
{
    #region FuncAsync (no options)

    [Test]
    public async Task FuncAsync_caches_successful_result()
    {
        var callCount = 0;

        var cached = MemoizeResult.FuncAsync<string, int, Error>(async key =>
        {
            callCount++;
            return Result.Success<int, Error>(42);
        });

        var result1 = await cached("key");
        var result2 = await cached("key");

        await Assert.That(result1.IsSuccess).IsTrue();
        await Assert.That(result2.IsSuccess).IsTrue();
        await Assert.That(result1.GetValueOrThrow()).IsEqualTo(42);
        await Assert.That(callCount).IsEqualTo(1);
    }

    [Test]
    public async Task FuncAsync_does_not_cache_failed_result()
    {
        var callCount = 0;

        var cached = MemoizeResult.FuncAsync<string, int, Error>(async key =>
        {
            callCount++;
            return callCount == 1
                ? Result.Failure<int, Error>(new ExternalServiceError { Message = "fail" })
                : Result.Success<int, Error>(42);
        });

        var result1 = await cached("key");
        var result2 = await cached("key");

        await Assert.That(result1.IsFailure).IsTrue();
        await Assert.That(result2.IsSuccess).IsTrue();
        await Assert.That(result2.GetValueOrThrow()).IsEqualTo(42);
        await Assert.That(callCount).IsEqualTo(2);
    }

    [Test]
    public async Task FuncAsync_caches_per_key()
    {
        var callCount = 0;

        var cached = MemoizeResult.FuncAsync<string, int, Error>(async key =>
        {
            callCount++;
            return Result.Success<int, Error>(key.Length);
        });

        var result1 = await cached("abc");
        var result2 = await cached("abcdef");
        var result3 = await cached("abc");

        await Assert.That(result1.GetValueOrThrow()).IsEqualTo(3);
        await Assert.That(result2.GetValueOrThrow()).IsEqualTo(6);
        await Assert.That(callCount).IsEqualTo(2);
    }

    #endregion

    #region FuncAsync (with options)

    [Test]
    public async Task FuncAsync_with_ttl_caches_successful_result()
    {
        var callCount = 0;

        var cached = MemoizeResult.FuncAsync<string, int, Error>(
            async key =>
            {
                callCount++;
                return Result.Success<int, Error>(42);
            },
            opts => opts.WithExpiration(TimeSpan.FromMinutes(5)));

        var result1 = await cached("key");
        var result2 = await cached("key");

        await Assert.That(result1.IsSuccess).IsTrue();
        await Assert.That(result2.IsSuccess).IsTrue();
        await Assert.That(callCount).IsEqualTo(1);
    }

    [Test]
    public async Task FuncAsync_with_ttl_does_not_cache_failure()
    {
        var callCount = 0;

        var cached = MemoizeResult.FuncAsync<string, int, Error>(
            async key =>
            {
                callCount++;
                return callCount == 1
                    ? Result.Failure<int, Error>(new ExternalServiceError { Message = "fail" })
                    : Result.Success<int, Error>(99);
            },
            opts => opts.WithExpiration(TimeSpan.FromMinutes(5)));

        var result1 = await cached("key");
        var result2 = await cached("key");

        await Assert.That(result1.IsFailure).IsTrue();
        await Assert.That(result2.IsSuccess).IsTrue();
        await Assert.That(result2.GetValueOrThrow()).IsEqualTo(99);
        await Assert.That(callCount).IsEqualTo(2);
    }

    [Test]
    public async Task FuncAsync_with_ttl_expires_cached_success()
    {
        var callCount = 0;

        var cached = MemoizeResult.FuncAsync<string, int, Error>(
            async key =>
            {
                callCount++;
                return Result.Success<int, Error>(callCount * 10);
            },
            opts => opts.WithExpiration(TimeSpan.FromMilliseconds(50)));

        var result1 = await cached("key");
        await Assert.That(result1.GetValueOrThrow()).IsEqualTo(10);

        await Task.Delay(100);

        var result2 = await cached("key");
        await Assert.That(result2.GetValueOrThrow()).IsEqualTo(20);
        await Assert.That(callCount).IsEqualTo(2);
    }

    [Test]
    public async Task FuncAsync_with_max_size_evicts_lru()
    {
        var callCount = 0;

        var cached = MemoizeResult.FuncAsync<int, string, Error>(
            async key =>
            {
                callCount++;
                return Result.Success<string, Error>($"value-{key}");
            },
            opts => opts.WithMaxSize(2));

        await cached(1);
        await cached(2);
        await cached(3); // evicts key 1

        callCount = 0;

        await cached(2); // cache hit
        await cached(3); // cache hit
        await cached(1); // cache miss, recomputed

        await Assert.That(callCount).IsEqualTo(1);
    }

    #endregion

    #region Func (sync, no options)

    [Test]
    public async Task Func_caches_successful_result()
    {
        var callCount = 0;

        var cached = MemoizeResult.Func<string, int, Error>(key =>
        {
            callCount++;
            return Result.Success<int, Error>(42);
        });

        var result1 = cached("key");
        var result2 = cached("key");

        await Assert.That(result1.IsSuccess).IsTrue();
        await Assert.That(result2.IsSuccess).IsTrue();
        await Assert.That(callCount).IsEqualTo(1);
    }

    [Test]
    public async Task Func_does_not_cache_failed_result()
    {
        var callCount = 0;

        var cached = MemoizeResult.Func<string, int, Error>(key =>
        {
            callCount++;
            return callCount == 1
                ? Result.Failure<int, Error>(new NotFoundError { Message = "not found" })
                : Result.Success<int, Error>(42);
        });

        var result1 = cached("key");
        var result2 = cached("key");

        await Assert.That(result1.IsFailure).IsTrue();
        await Assert.That(result2.IsSuccess).IsTrue();
        await Assert.That(callCount).IsEqualTo(2);
    }

    #endregion

    #region Func (sync, with options)

    [Test]
    public async Task Func_with_ttl_caches_successful_result()
    {
        var callCount = 0;

        var cached = MemoizeResult.Func<string, int, Error>(
            key =>
            {
                callCount++;
                return Result.Success<int, Error>(42);
            },
            opts => opts.WithExpiration(TimeSpan.FromMinutes(5)));

        var result1 = cached("key");
        var result2 = cached("key");

        await Assert.That(result1.IsSuccess).IsTrue();
        await Assert.That(result2.IsSuccess).IsTrue();
        await Assert.That(callCount).IsEqualTo(1);
    }

    [Test]
    public async Task Func_with_ttl_does_not_cache_failure()
    {
        var callCount = 0;

        var cached = MemoizeResult.Func<string, int, Error>(
            key =>
            {
                callCount++;
                return callCount == 1
                    ? Result.Failure<int, Error>(new ExternalServiceError { Message = "fail" })
                    : Result.Success<int, Error>(99);
            },
            opts => opts.WithExpiration(TimeSpan.FromMinutes(5)));

        var result1 = cached("key");
        var result2 = cached("key");

        await Assert.That(result1.IsFailure).IsTrue();
        await Assert.That(result2.IsSuccess).IsTrue();
        await Assert.That(callCount).IsEqualTo(2);
    }

    #endregion
}

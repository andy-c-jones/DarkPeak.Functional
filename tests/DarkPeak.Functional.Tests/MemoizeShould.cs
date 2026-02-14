namespace DarkPeak.Functional.Tests;

public class MemoizeShould
{
    // --- Zero-arg (lazy singleton) ---

    [Test]
    public async Task Memoize_ZeroArg_CallsFunctionOnce()
    {
        var callCount = 0;
        var memoized = Memoize.Func(() =>
        {
            callCount++;
            return 42;
        });

        var result1 = memoized();
        var result2 = memoized();

        await Assert.That(result1).IsEqualTo(42);
        await Assert.That(result2).IsEqualTo(42);
        await Assert.That(callCount).IsEqualTo(1);
    }

    [Test]
    public async Task Memoize_ZeroArg_IsThreadSafe()
    {
        var callCount = 0;
        var memoized = Memoize.Func(() =>
        {
            Interlocked.Increment(ref callCount);
            return "hello";
        });

        var tasks = Enumerable.Range(0, 100).Select(_ => Task.Run(() => memoized())).ToArray();
        var results = await Task.WhenAll(tasks);

        await Assert.That(results.All(r => r == "hello")).IsTrue();
        await Assert.That(callCount).IsEqualTo(1);
    }

    // --- Single-arg (unbounded) ---

    [Test]
    public async Task Memoize_SingleArg_CachesPerKey()
    {
        var callCount = 0;
        var memoized = Memoize.Func<int, int>(x =>
        {
            callCount++;
            return x * 2;
        });

        var a = memoized(3);
        var b = memoized(3);
        var c = memoized(5);

        await Assert.That(a).IsEqualTo(6);
        await Assert.That(b).IsEqualTo(6);
        await Assert.That(c).IsEqualTo(10);
        await Assert.That(callCount).IsEqualTo(2); // 3 and 5
    }

    [Test]
    public async Task Memoize_SingleArg_DifferentKeysIndependent()
    {
        var memoized = Memoize.Func<string, int>(s => s.Length);

        await Assert.That(memoized("hi")).IsEqualTo(2);
        await Assert.That(memoized("hello")).IsEqualTo(5);
        await Assert.That(memoized("hi")).IsEqualTo(2);
    }

    // --- Two-arg (unbounded) ---

    [Test]
    public async Task Memoize_TwoArg_CachesPerKeyPair()
    {
        var callCount = 0;
        var memoized = Memoize.Func<int, int, int>((a, b) =>
        {
            callCount++;
            return a + b;
        });

        var r1 = memoized(1, 2);
        var r2 = memoized(1, 2);
        var r3 = memoized(2, 1);

        await Assert.That(r1).IsEqualTo(3);
        await Assert.That(r2).IsEqualTo(3);
        await Assert.That(r3).IsEqualTo(3);
        await Assert.That(callCount).IsEqualTo(2); // (1,2) and (2,1)
    }

    // --- TTL expiration ---

    [Test]
    public async Task Memoize_WithExpiration_RecomputesAfterTtl()
    {
        var callCount = 0;
        var memoized = Memoize.Func<string, int>(
            _ =>
            {
                callCount++;
                return callCount;
            },
            opts => opts.WithExpiration(TimeSpan.FromMilliseconds(50)));

        var first = memoized("key");
        await Assert.That(first).IsEqualTo(1);

        var cached = memoized("key");
        await Assert.That(cached).IsEqualTo(1);
        await Assert.That(callCount).IsEqualTo(1);

        await Task.Delay(80);

        var recomputed = memoized("key");
        await Assert.That(recomputed).IsEqualTo(2);
        await Assert.That(callCount).IsEqualTo(2);
    }

    [Test]
    public async Task Memoize_WithExpiration_NonExpiredEntryReturnsCached()
    {
        var callCount = 0;
        var memoized = Memoize.Func<int, string>(
            x =>
            {
                callCount++;
                return $"val-{x}";
            },
            opts => opts.WithExpiration(TimeSpan.FromSeconds(10)));

        memoized(1);
        memoized(1);
        memoized(1);

        await Assert.That(callCount).IsEqualTo(1);
    }

    // --- Max size / LRU eviction ---

    [Test]
    public async Task Memoize_WithMaxSize_EvictsLeastRecentlyUsed()
    {
        var callCount = 0;
        var memoized = Memoize.Func<int, int>(
            x =>
            {
                callCount++;
                return x * 10;
            },
            opts => opts.WithMaxSize(2));

        memoized(1); // cache: [1]
        memoized(2); // cache: [1, 2]
        memoized(3); // cache should evict 1, now [2, 3]

        await Assert.That(callCount).IsEqualTo(3);

        // 2 and 3 should be cached
        memoized(2);
        memoized(3);
        await Assert.That(callCount).IsEqualTo(3);

        // 1 was evicted, should recompute
        memoized(1);
        await Assert.That(callCount).IsEqualTo(4);
    }

    [Test]
    public async Task Memoize_WithMaxSize_AccessRefreshesLru()
    {
        var callCount = 0;
        var memoized = Memoize.Func<int, int>(
            x =>
            {
                callCount++;
                return x;
            },
            opts => opts.WithMaxSize(2));

        memoized(1); // cache: [1]
        memoized(2); // cache: [1, 2]
        memoized(1); // access 1 again, refreshes LRU order: [2, 1]
        memoized(3); // should evict 2 (least recently used): [1, 3]

        await Assert.That(callCount).IsEqualTo(3); // 1, 2, 3

        // 1 should still be cached (was refreshed)
        memoized(1);
        await Assert.That(callCount).IsEqualTo(3);

        // 2 was evicted
        memoized(2);
        await Assert.That(callCount).IsEqualTo(4);
    }

    [Test]
    public void Memoize_WithMaxSize_ThrowsForInvalidSize()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new MemoizeOptions().WithMaxSize(0));
    }

    // --- Async memoization ---

    [Test]
    public async Task MemoizeAsync_CachesPerKey()
    {
        var callCount = 0;
        var memoized = Memoize.FuncAsync<string, int>(async key =>
        {
            Interlocked.Increment(ref callCount);
            await Task.Delay(10);
            return key.Length;
        });

        var r1 = await memoized("hello");
        var r2 = await memoized("hello");
        var r3 = await memoized("hi");

        await Assert.That(r1).IsEqualTo(5);
        await Assert.That(r2).IsEqualTo(5);
        await Assert.That(r3).IsEqualTo(2);
        await Assert.That(callCount).IsEqualTo(2);
    }

    [Test]
    public async Task MemoizeAsync_ThunderingHerdProtection()
    {
        var callCount = 0;
        var memoized = Memoize.FuncAsync<string, int>(async key =>
        {
            Interlocked.Increment(ref callCount);
            await Task.Delay(50);
            return key.Length;
        });

        // Fire 50 concurrent calls for the same key
        var tasks = Enumerable.Range(0, 50)
            .Select(_ => Task.Run(() => memoized("test")))
            .ToArray();

        var results = await Task.WhenAll(tasks);

        await Assert.That(results.All(r => r == 4)).IsTrue();
        await Assert.That(callCount).IsEqualTo(1);
    }

    [Test]
    public async Task MemoizeAsync_WithExpiration_RecomputesAfterTtl()
    {
        var callCount = 0;
        var memoized = Memoize.FuncAsync<string, int>(
            async _ =>
            {
                callCount++;
                await Task.Yield();
                return callCount;
            },
            opts => opts.WithExpiration(TimeSpan.FromMilliseconds(50)));

        var first = await memoized("key");
        await Assert.That(first).IsEqualTo(1);

        await Task.Delay(80);

        var second = await memoized("key");
        await Assert.That(second).IsEqualTo(2);
    }

    [Test]
    public async Task MemoizeAsync_WithMaxSize_EvictsOldEntries()
    {
        var callCount = 0;
        var memoized = Memoize.FuncAsync<int, int>(
            async x =>
            {
                callCount++;
                await Task.Yield();
                return x * 10;
            },
            opts => opts.WithMaxSize(2));

        await memoized(1);
        await memoized(2);
        await memoized(3); // evicts 1

        await Assert.That(callCount).IsEqualTo(3);

        await memoized(2); // cached
        await memoized(3); // cached
        await Assert.That(callCount).IsEqualTo(3);

        await memoized(1); // recomputed
        await Assert.That(callCount).IsEqualTo(4);
    }

    // --- Two-arg with options ---

    [Test]
    public async Task Memoize_TwoArg_WithExpiration_Works()
    {
        var callCount = 0;
        var memoized = Memoize.Func<int, int, int>(
            (a, b) =>
            {
                callCount++;
                return a + b;
            },
            opts => opts.WithExpiration(TimeSpan.FromMilliseconds(50)));

        memoized(1, 2);
        memoized(1, 2);
        await Assert.That(callCount).IsEqualTo(1);

        await Task.Delay(80);

        memoized(1, 2);
        await Assert.That(callCount).IsEqualTo(2);
    }

    // --- Edge cases ---

    [Test]
    public async Task Memoize_SingleArg_HandlesNullReturnValues()
    {
        var callCount = 0;
        var memoized = Memoize.Func<string, string?>(
            _ =>
            {
                callCount++;
                return null;
            });

        var r1 = memoized("key");
        var r2 = memoized("key");

        await Assert.That(r1).IsNull();
        await Assert.That(r2).IsNull();
        await Assert.That(callCount).IsEqualTo(1);
    }

    [Test]
    public async Task Memoize_SingleArg_WithOptions_HandlesNullReturnValues()
    {
        var callCount = 0;
        var memoized = Memoize.Func<string, string?>(
            _ =>
            {
                callCount++;
                return null;
            },
            opts => opts.WithMaxSize(10));

        var r1 = memoized("key");
        var r2 = memoized("key");

        await Assert.That(r1).IsNull();
        await Assert.That(r2).IsNull();
        await Assert.That(callCount).IsEqualTo(1);
    }

    [Test]
    public async Task Memoize_SingleArg_ThreadSafe()
    {
        var callCount = 0;
        var memoized = Memoize.Func<int, int>(x =>
        {
            Interlocked.Increment(ref callCount);
            return x * 2;
        });

        var tasks = Enumerable.Range(0, 100)
            .Select(i => Task.Run(() => memoized(i % 10)))
            .ToArray();

        var results = await Task.WhenAll(tasks);

        // Each of the 10 distinct keys should be computed at most once
        // (ConcurrentDictionary.GetOrAdd may occasionally double-compute under contention)
        await Assert.That(callCount).IsGreaterThanOrEqualTo(10).And.IsLessThanOrEqualTo(20);
        await Assert.That(results.Where((_, i) => i % 10 == 0).All(r => r == 0)).IsTrue();
    }

    // --- Composability with functional types ---

    [Test]
    public async Task Memoize_WorksWithOptionReturningFunctions()
    {
        var callCount = 0;
        var memoized = Memoize.Func<int, Option<string>>(x =>
        {
            callCount++;
            return x > 0 ? Option.Some(x.ToString()) : Option.None<string>();
        });

        var some = memoized(5);
        var none = memoized(-1);
        var someCached = memoized(5);

        await Assert.That(some).IsTypeOf<Some<string>>();
        await Assert.That(none).IsTypeOf<None<string>>();
        await Assert.That(callCount).IsEqualTo(2);
    }

    [Test]
    public async Task Memoize_WorksWithResultReturningFunctions()
    {
        var callCount = 0;
        var memoized = Memoize.Func<string, Result<int, ValidationError>>(s =>
        {
            callCount++;
            return int.TryParse(s, out var n)
                ? Result.Success<int, ValidationError>(n)
                : Result.Failure<int, ValidationError>(new ValidationError { Message = "Not a number" });
        });

        var ok = memoized("42");
        var err = memoized("abc");
        var okCached = memoized("42");

        await Assert.That(ok.IsSuccess).IsTrue();
        await Assert.That(err.IsSuccess).IsFalse();
        await Assert.That(callCount).IsEqualTo(2);
    }
}

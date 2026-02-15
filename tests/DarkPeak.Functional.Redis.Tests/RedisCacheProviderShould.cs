using DarkPeak.Functional.Redis;
using StackExchange.Redis;
using Testcontainers.Redis;

namespace DarkPeak.Functional.Redis.Tests;

public class RedisCacheProviderShould : IAsyncDisposable
{
    private readonly RedisContainer _container;
    private ConnectionMultiplexer? _connection;
    private IDatabase? _database;

    public RedisCacheProviderShould()
    {
        _container = new RedisBuilder("redis:7-alpine")
            .Build();
    }

    private async Task<IDatabase> GetDatabaseAsync()
    {
        if (_database is not null) return _database;

        await _container.StartAsync();
        _connection = await ConnectionMultiplexer.ConnectAsync(_container.GetConnectionString());
        _database = _connection.GetDatabase();
        return _database;
    }

    public async ValueTask DisposeAsync()
    {
        if (_connection is not null) await _connection.DisposeAsync();
        await _container.DisposeAsync();
    }

    // --- Get ---

    [Test]
    public async Task Return_none_when_key_does_not_exist()
    {
        var db = await GetDatabaseAsync();
        var provider = new RedisCacheProvider<string, string>(db);

        var result = await provider.GetAsync("nonexistent");

        await Assert.That(result).IsTypeOf<None<string>>();
    }

    [Test]
    public async Task Return_some_when_key_exists()
    {
        var db = await GetDatabaseAsync();
        var provider = new RedisCacheProvider<string, string>(db);

        await provider.SetAsync("exists", "hello", null);
        var result = await provider.GetAsync("exists");

        await Assert.That(result).IsTypeOf<Some<string>>();
        var value = ((Some<string>)result).Value;
        await Assert.That(value).IsEqualTo("hello");
    }

    [Test]
    public async Task Return_some_for_sync_get_when_key_exists()
    {
        var db = await GetDatabaseAsync();
        var provider = new RedisCacheProvider<string, string>(db);

        provider.Set("sync-key", "sync-value", null);
        var result = provider.Get("sync-key");

        await Assert.That(result).IsTypeOf<Some<string>>();
        var value = ((Some<string>)result).Value;
        await Assert.That(value).IsEqualTo("sync-value");
    }

    [Test]
    public async Task Return_none_for_sync_get_when_key_does_not_exist()
    {
        var db = await GetDatabaseAsync();
        var provider = new RedisCacheProvider<string, string>(db);

        var result = provider.Get("no-such-key");

        await Assert.That(result).IsTypeOf<None<string>>();
    }

    // --- Set with TTL ---

    [Test]
    public async Task Set_value_without_expiration()
    {
        var db = await GetDatabaseAsync();
        var provider = new RedisCacheProvider<string, string>(db);

        await provider.SetAsync("no-ttl", "persistent", null);

        var ttl = await db.KeyTimeToLiveAsync("no-ttl");
        await Assert.That(ttl).IsNull();

        var result = await provider.GetAsync("no-ttl");
        await Assert.That(result).IsTypeOf<Some<string>>();
    }

    [Test]
    public async Task Set_value_with_expiration()
    {
        var db = await GetDatabaseAsync();
        var provider = new RedisCacheProvider<string, string>(db);

        await provider.SetAsync("with-ttl", "temporary", TimeSpan.FromMinutes(5));

        var ttl = await db.KeyTimeToLiveAsync("with-ttl");
        await Assert.That(ttl).IsNotNull();
        await Assert.That(ttl!.Value.TotalSeconds).IsGreaterThan(0);
        await Assert.That(ttl!.Value.TotalMinutes).IsLessThanOrEqualTo(5);
    }

    [Test]
    public async Task Sync_set_value_with_expiration()
    {
        var db = await GetDatabaseAsync();
        var provider = new RedisCacheProvider<string, string>(db);

        provider.Set("sync-ttl", "temporary", TimeSpan.FromMinutes(5));

        var ttl = await db.KeyTimeToLiveAsync("sync-ttl");
        await Assert.That(ttl).IsNotNull();
        await Assert.That(ttl!.Value.TotalSeconds).IsGreaterThan(0);
    }

    // --- Remove ---

    [Test]
    public async Task Remove_existing_key()
    {
        var db = await GetDatabaseAsync();
        var provider = new RedisCacheProvider<string, string>(db);

        await provider.SetAsync("to-remove", "value", null);
        await provider.RemoveAsync("to-remove");
        var result = await provider.GetAsync("to-remove");

        await Assert.That(result).IsTypeOf<None<string>>();
    }

    [Test]
    public async Task Sync_remove_existing_key()
    {
        var db = await GetDatabaseAsync();
        var provider = new RedisCacheProvider<string, string>(db);

        provider.Set("sync-remove", "value", null);
        provider.Remove("sync-remove");
        var result = provider.Get("sync-remove");

        await Assert.That(result).IsTypeOf<None<string>>();
    }

    // --- Key prefix ---

    [Test]
    public async Task Apply_key_prefix_to_all_operations()
    {
        var db = await GetDatabaseAsync();
        var provider = new RedisCacheProvider<string, string>(db, keyPrefix: "myapp:cache:");

        await provider.SetAsync("user:123", "Alice", null);

        // Verify the actual Redis key has the prefix
        var rawValue = await db.StringGetAsync("myapp:cache:user:123");
        await Assert.That(rawValue.IsNullOrEmpty).IsFalse();

        // Verify the unprefixed key does not exist
        var noPrefix = await db.StringGetAsync("user:123");
        await Assert.That(noPrefix.IsNullOrEmpty).IsTrue();

        // Provider can still read back with its own prefix
        var result = await provider.GetAsync("user:123");
        await Assert.That(result).IsTypeOf<Some<string>>();
    }

    [Test]
    public async Task Remove_uses_key_prefix()
    {
        var db = await GetDatabaseAsync();
        var provider = new RedisCacheProvider<string, string>(db, keyPrefix: "pfx:");

        await provider.SetAsync("k", "v", null);
        await provider.RemoveAsync("k");

        var raw = await db.StringGetAsync("pfx:k");
        await Assert.That(raw.IsNullOrEmpty).IsTrue();
    }

    // --- Complex value serialization ---

    [Test]
    public async Task Serialize_and_deserialize_complex_objects()
    {
        var db = await GetDatabaseAsync();
        var provider = new RedisCacheProvider<string, TestRecord>(db);

        var original = new TestRecord(42, "Alice", true);
        await provider.SetAsync("complex", original, null);
        var result = await provider.GetAsync("complex");

        await Assert.That(result).IsTypeOf<Some<TestRecord>>();
        var retrieved = ((Some<TestRecord>)result).Value;
        await Assert.That(retrieved.Id).IsEqualTo(42);
        await Assert.That(retrieved.Name).IsEqualTo("Alice");
        await Assert.That(retrieved.Active).IsTrue();
    }

    // --- Integer keys ---

    [Test]
    public async Task Support_integer_keys()
    {
        var db = await GetDatabaseAsync();
        var provider = new RedisCacheProvider<int, string>(db);

        await provider.SetAsync(42, "answer", null);
        var result = await provider.GetAsync(42);

        await Assert.That(result).IsTypeOf<Some<string>>();
        var value = ((Some<string>)result).Value;
        await Assert.That(value).IsEqualTo("answer");
    }

    // --- Integration with Memoize ---

    [Test]
    public async Task Work_as_memoize_cache_provider()
    {
        var db = await GetDatabaseAsync();
        var provider = new RedisCacheProvider<string, int>(db, keyPrefix: "memo:");

        var callCount = 0;
        var cached = Memoize.FuncAsync<string, int>(
            async key =>
            {
                Interlocked.Increment(ref callCount);
                await Task.Delay(1);
                return key.Length;
            },
            opts => opts.WithCacheProvider(provider));

        var first = await cached("hello");
        var second = await cached("hello");

        await Assert.That(first).IsEqualTo(5);
        await Assert.That(second).IsEqualTo(5);
        await Assert.That(callCount).IsEqualTo(1);
    }

    // --- Integration with MemoizeResult ---

    [Test]
    public async Task Work_as_memoize_result_cache_provider_caching_only_successes()
    {
        var db = await GetDatabaseAsync();
        var provider = new RedisCacheProvider<string, string>(db, keyPrefix: "memres:");

        var callCount = 0;
        var shouldFail = true;

        var cached = MemoizeResult.FuncAsync<string, string, Error>(
            async key =>
            {
                Interlocked.Increment(ref callCount);
                await Task.Delay(1);
                if (shouldFail)
                    return Result.Failure<string, Error>(new InternalError { Message = "fail" });
                return Result.Success<string, Error>($"ok:{key}");
            },
            opts => opts.WithCacheProvider(provider));

        // First call: fails, should NOT cache
        var result1 = await cached("test");
        await Assert.That(result1.IsSuccess).IsFalse();
        await Assert.That(callCount).IsEqualTo(1);

        // Second call: still fails, function is called again (not cached)
        var result2 = await cached("test");
        await Assert.That(result2.IsSuccess).IsFalse();
        await Assert.That(callCount).IsEqualTo(2);

        // Now succeed
        shouldFail = false;
        var result3 = await cached("test");
        await Assert.That(result3.IsSuccess).IsTrue();
        await Assert.That(callCount).IsEqualTo(3);

        // Fourth call: should be cached
        var result4 = await cached("test");
        await Assert.That(result4.IsSuccess).IsTrue();
        await Assert.That(callCount).IsEqualTo(3); // not called again
    }

    // --- Overwrite existing key ---

    [Test]
    public async Task Overwrite_existing_value_on_set()
    {
        var db = await GetDatabaseAsync();
        var provider = new RedisCacheProvider<string, string>(db);

        await provider.SetAsync("overwrite", "first", null);
        await provider.SetAsync("overwrite", "second", null);
        var result = await provider.GetAsync("overwrite");

        await Assert.That(result).IsTypeOf<Some<string>>();
        var value = ((Some<string>)result).Value;
        await Assert.That(value).IsEqualTo("second");
    }

    private record TestRecord(int Id, string Name, bool Active);
}

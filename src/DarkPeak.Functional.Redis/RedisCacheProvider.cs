using System.Text.Json;
using StackExchange.Redis;

namespace DarkPeak.Functional.Redis;

/// <summary>
/// A Redis-backed implementation of <see cref="ICacheProvider{TKey, TValue}"/> using
/// StackExchange.Redis. Serializes values as JSON via <see cref="System.Text.Json"/>.
/// </summary>
/// <remarks>
/// <para>
/// This provider can be used with <see cref="Memoize"/> and <see cref="MemoizeResult"/>
/// to add distributed caching to memoized functions. When combined with in-memory options
/// (<c>WithMaxSize</c>, <c>WithExpiration</c>), it acts as an L2 cache behind the in-memory L1.
/// When used alone, all caching is delegated to Redis.
/// </para>
/// <para>
/// Keys are converted to Redis keys using <c>ToString()</c> with an optional prefix.
/// Values are serialized/deserialized using <see cref="System.Text.Json.JsonSerializer"/>.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// var redis = ConnectionMultiplexer.Connect("localhost:6379");
/// var provider = new RedisCacheProvider&lt;string, UserProfile&gt;(redis.GetDatabase());
///
/// var cachedLookup = Memoize.FuncAsync&lt;string, UserProfile&gt;(
///     LoadUserProfileAsync,
///     opts => opts
///         .WithExpiration(TimeSpan.FromMinutes(10))
///         .WithCacheProvider(provider));
/// </code>
/// </example>
/// <typeparam name="TKey">The cache key type. Must override <c>ToString()</c> to produce a meaningful key.</typeparam>
/// <typeparam name="TValue">The cached value type. Must be serializable by <see cref="System.Text.Json"/>.</typeparam>
public sealed class RedisCacheProvider<TKey, TValue> : ICacheProvider<TKey, TValue>
    where TKey : notnull
{
    private readonly IDatabase _database;
    private readonly string _keyPrefix;
    private readonly JsonSerializerOptions _jsonOptions;

    /// <summary>
    /// Creates a new Redis cache provider.
    /// </summary>
    /// <param name="database">The StackExchange.Redis <see cref="IDatabase"/> to use for cache operations.</param>
    /// <param name="keyPrefix">
    /// An optional prefix prepended to all Redis keys (e.g. <c>"myapp:cache:"</c>).
    /// Useful for namespacing keys to avoid collisions with other applications sharing the same Redis instance.
    /// </param>
    /// <param name="jsonOptions">
    /// Optional <see cref="JsonSerializerOptions"/> for value serialization.
    /// Defaults to <see cref="JsonSerializerOptions.Default"/> if not specified.
    /// </param>
    public RedisCacheProvider(
        IDatabase database,
        string keyPrefix = "",
        JsonSerializerOptions? jsonOptions = null)
    {
        _database = database ?? throw new ArgumentNullException(nameof(database));
        _keyPrefix = keyPrefix;
        _jsonOptions = jsonOptions ?? JsonSerializerOptions.Default;
    }

    /// <inheritdoc />
    public Option<TValue> Get(TKey key)
    {
        var redisValue = _database.StringGet(ToRedisKey(key));
        return DeserializeValue(redisValue);
    }

    /// <inheritdoc />
    public async Task<Option<TValue>> GetAsync(TKey key)
    {
        var redisValue = await _database.StringGetAsync(ToRedisKey(key));
        return DeserializeValue(redisValue);
    }

    /// <inheritdoc />
    public void Set(TKey key, TValue value, TimeSpan? expiration)
    {
        var serialized = JsonSerializer.Serialize(value, _jsonOptions);
        _database.StringSet(ToRedisKey(key), serialized, expiration, When.Always);
    }

    /// <inheritdoc />
    public async Task SetAsync(TKey key, TValue value, TimeSpan? expiration)
    {
        var serialized = JsonSerializer.Serialize(value, _jsonOptions);
        await _database.StringSetAsync(ToRedisKey(key), serialized, expiration, When.Always);
    }

    /// <inheritdoc />
    public void Remove(TKey key)
    {
        _database.KeyDelete(ToRedisKey(key));
    }

    /// <inheritdoc />
    public async Task RemoveAsync(TKey key)
    {
        await _database.KeyDeleteAsync(ToRedisKey(key));
    }

    private RedisKey ToRedisKey(TKey key) => $"{_keyPrefix}{key}";

    private Option<TValue> DeserializeValue(RedisValue redisValue)
    {
        if (redisValue.IsNullOrEmpty)
            return Option.None<TValue>();

        var value = JsonSerializer.Deserialize<TValue>((string)redisValue!, _jsonOptions);
        return value is not null
            ? Option.Some(value)
            : Option.None<TValue>();
    }
}

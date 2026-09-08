using System.Text.Json;
using Contracts;
using Events.Application;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace Events.Infrastructure;

public sealed class RedisCacheService : ICacheService
{
    private readonly IDatabase _redisDb;
    private readonly RedisConfig _config;
    private readonly ILogger<RedisCacheService> _logger;

    public RedisCacheService(
        IOptions<RedisConfig> options,
        IConnectionMultiplexer redis,
        ILogger<RedisCacheService> logger)
    {
        _redisDb = redis.GetDatabase();
        _config = options.Value;
        _logger = logger;
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
        where T : class
    {
        try
        {
            var value = await _redisDb.StringGetAsync(key);
            if (!value.HasValue)
                return null;

            return JsonSerializer.Deserialize<T>(value.ToString());
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Redis get key {key} failed", key);
            return null;
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan ttl, CancellationToken cancellationToken = default)
        where T : class
    {
        try
        {
            var json = JsonSerializer.Serialize(value);
            await _redisDb.StringSetAsync(key, json, ttl);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Redis set key {key} failed", key);
        }
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            await _redisDb.KeyDeleteAsync(key);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Redis delete key {key} failed", key);
        }
    }
}

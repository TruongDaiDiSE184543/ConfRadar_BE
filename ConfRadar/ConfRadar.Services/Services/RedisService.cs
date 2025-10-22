using Microsoft.Extensions.Options;
using StackExchange.Redis;
using static ConfRadar.Services.Common.AppSettingConfig;

namespace ConfRadar.Services.Services
{
    public interface IRedisService
    {
        Task SetStringAsync(string key, string value, TimeSpan? expiry = null);
        Task<string?> GetStringAsync(string key);
        Task<bool> DeleteKeyAsync(string key);
        Task<bool> KeyExistsAsync(string key);
        Task<long> DecrementAsync(string key, long value = 1);
        Task<long> IncrementAsync(string key, long value = 1);
    }
    public class RedisService : IRedisService
    {
        private readonly ConnectionMultiplexer _redis;
        private readonly IDatabase _db;
        private readonly IOptions<RedisSettings> _redisSettings;
        public RedisService(IOptions<RedisSettings> redisSettings)
        {
            _redisSettings = redisSettings;
            var options = new ConfigurationOptions()
            {
                EndPoints = { $"{_redisSettings.Value.Host}:{_redisSettings.Value.Port}" },
                Password = _redisSettings.Value.Password,
                AbortOnConnectFail = false,
                ConnectRetry = 3,
            };
            _redis = ConnectionMultiplexer.Connect(options);
            _db = _redis.GetDatabase();
        }
        public async Task SetStringAsync(string key, string value, TimeSpan? expiry = null)
        {
            await _db.StringSetAsync(key, value, expiry);
        }
        public async Task<string?> GetStringAsync(string key)
        {
            return await _db.StringGetAsync(key);
        }
        public async Task<bool> DeleteKeyAsync(string key)
        {
            return await _db.KeyDeleteAsync(key);
        }
        public async Task<bool> KeyExistsAsync(string key)
        {
            return await _db.KeyExistsAsync(key);
        }
        public async Task<long> IncrementAsync(string key, long value = 1)
        {
            return await _db.StringIncrementAsync(key, value);
        }

        public async Task<long> DecrementAsync(string key, long value = 1)
        {
            return await _db.StringDecrementAsync(key, value);
        }
    }

}

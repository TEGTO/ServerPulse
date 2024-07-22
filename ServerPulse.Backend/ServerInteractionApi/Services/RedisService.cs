using StackExchange.Redis;

namespace ServerInteractionApi.Services
{
    public class RedisService : IRedisService
    {
        private readonly IConnectionMultiplexer connectionMultiplexer;

        public RedisService(IConnectionMultiplexer connectionMultiplexer)
        {
            this.connectionMultiplexer = connectionMultiplexer;
        }

        public async Task SetValueAsync(string key, string value, double expiryInMinutes)
        {
            var db = GetDatabase();
            await db.StringSetAsync(key, value, TimeSpan.FromMinutes(expiryInMinutes));
        }
        public async Task<string> GetValueAsync(string key)
        {
            var db = GetDatabase();
            return await db.StringGetAsync(key);
        }
        public async Task<bool> RemoveKeyAsync(string key)
        {
            var db = GetDatabase();
            return await db.KeyDeleteAsync(key);
        }
        private IDatabase GetDatabase(int db = -1)
        {
            return connectionMultiplexer.GetDatabase(db);
        }
    }
}
namespace AnalyzerApi.Infrastructure.Configuration
{
    public class CacheSettings
    {
        public const string SETTINGS_SECTION = "Cache";
        public const string REDIS_SERVER_CONNECTION_STRING = "RedisServer";

        public float ExpiryInMinutes { get; set; }
    }
}

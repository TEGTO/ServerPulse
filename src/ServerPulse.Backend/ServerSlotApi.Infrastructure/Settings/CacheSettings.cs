namespace ServerSlotApi.Infrastructure.Configuration
{
    public class CacheSettings
    {
        public const string SETTINGS_SECTION = "Cache";
        public const string REDIS_SERVER_CONNECTION_STRING = "RedisServer";

        public float ServerSlotCheckExpiryInSeconds { get; set; }
        public float GetServerSlotByEmailExpiryInSeconds { get; set; }
    }
}

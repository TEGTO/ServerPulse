namespace AnalyzerApi.Infrastructure.Configuration
{
    public class CacheSettings
    {
        public const string SETTINGS_SECTION = "Cache";
        public const string REDIS_SERVER_CONNECTION_STRING = "RedisServer";

        public float GetLoadEventsInDataRangeExpiryInMinutes { get; set; }
        public float GetDailyLoadAmountStatisticsExpiryInMinutes { get; set; }
        public float GetLoadAmountStatisticsInRangeExpiryInMinutes { get; set; }
        public float GetSlotStatisticsExpiryInMinutes { get; set; }
    }
}

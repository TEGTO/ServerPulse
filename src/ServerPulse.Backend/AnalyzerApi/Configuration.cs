namespace AnalyzerApi
{
    public class Configuration
    {
        public static string KAFKA_BOOTSTRAP_SERVERS { get; } = "Kafka:BootstrapServers";
        public static string KAFKA_CLIENT_ID { get; } = "Kafka:ClientId";
        public static string KAFKA_GROUP_ID { get; } = "Kafka:GroupId";
        public static string KAFKA_TIMEOUT_IN_MILLISECONDS { get; } = "Kafka:AnalyzerReceiveTimeout";
        public static string KAFKA_ALIVE_TOPIC { get; } = "Kafka:AliveTopic";
        public static string KAFKA_LOAD_TOPIC { get; } = "Kafka:LoadTopic";
        public static string KAFKA_CONFIGURATION_TOPIC { get; } = "Kafka:ConfigurationTopic";
        public static string KAFKA_TOPIC_DATA_SAVE_IN_DAYS { get; } = "Kafka:TopicDataSaveInDays";
        public static string STATISTICS_COLLECT_INTERVAL_IN_MILLISECONDS { get; } = "StatisticsCollectIntervalInMilliseconds";
        public static string CACHE_SERVER_LOAD_STATISTICS_PER_DAY_EXPIRY_IN_MINUTES { get; } = "Cache:ServerLoadStatisticsPerDayExpiryInMinutes";
        public static string CACHE_STATISTICS_KEY { get; } = "Cache:StatisticsKey";
        public static string MIN_STATISTICS_TIMESPAN_IN_SECONDS { get; } = "MinimumStatisticsTimeSpanInSeconds";
    }
}
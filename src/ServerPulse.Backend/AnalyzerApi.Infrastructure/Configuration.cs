namespace AnalyzerApi.Infrastructure
{
    public static class Configuration
    {
        public static string KAFKA_BOOTSTRAP_SERVERS { get; } = "Kafka:BootstrapServers";
        public static string KAFKA_CLIENT_ID { get; } = "Kafka:ClientId";
        public static string KAFKA_GROUP_ID { get; } = "Kafka:GroupId";
        public static string KAFKA_TIMEOUT_IN_MILLISECONDS { get; } = "Kafka:AnalyzerReceiveTimeout";
        public static string KAFKA_ALIVE_TOPIC { get; } = "Kafka:AliveTopic";
        public static string KAFKA_LOAD_TOPIC { get; } = "Kafka:LoadTopic";
        public static string KAFKA_LOAD_TOPIC_PROCESS { get; } = "Kafka:ProcessLoadEventTopic";
        public static string KAFKA_CONFIGURATION_TOPIC { get; } = "Kafka:ConfigurationTopic";
        public static string KAFKA_SERVER_STATISTICS_TOPIC { get; } = "Kafka:ServerStatisticsTopic";
        public static string KAFKA_CUSTOM_TOPIC { get; } = "Kafka:CustomTopic";
        public static string KAFKA_LOAD_METHOD_STATISTICS_TOPIC { get; } = "Kafka:LoadMethodStatisticsTopic";
        public static string KAFKA_TOPIC_DATA_SAVE_IN_DAYS { get; } = "Kafka:TopicDataSaveInDays";
        public static string STATISTICS_COLLECT_INTERVAL_IN_MILLISECONDS { get; } = "StatisticsCollectIntervalInMilliseconds";
        public static string CACHE_EXPIRY_IN_MINUTES { get; } = "Cache:ExpiryInMinutes";
        public static string MIN_STATISTICS_TIMESPAN_IN_SECONDS { get; } = "MinimumStatisticsTimeSpanInSeconds";
        public static string MAX_EVENT_AMOUNT_TO_GET_IN_SLOT_DATA { get; } = "MaxEventAmountToGetInSlotData";
        public static string MAX_EVENT_AMOUNT_PER_REQUEST { get; } = "MaxEventAmountToReadPerRequest";
        public static string LOAD_EVENT_PROCESSING_RESILLIENCE { get; } = "LoadEventProcessing:Resillience";
        public static string LOAD_EVENT_PROCESSING_BATCH_SIZE { get; } = "LoadEventProcessing:BatchSize";
        public static string LOAD_EVENT_PROCESSING_BATCH_INTERVAL_IN_MILLISECONDS { get; } = "LoadEventProcessing:BatchIntervalInMilliseconds";
        public static string REDIS_SERVER_CONNECTION_STRING { get; } = "RedisServer";
    }
}
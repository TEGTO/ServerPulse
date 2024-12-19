namespace ServerMonitorApi
{
    public static class Configuration
    {
        public static string KAFKA_BOOTSTRAP_SERVERS { get; } = "Kafka:BootstrapServers";
        public static string KAFKA_CLIENT_ID { get; } = "Kafka:ClientId";
        public static string KAFKA_ALIVE_TOPIC { get; } = "Kafka:AliveTopic";
        public static string KAFKA_CONFIGURATION_TOPIC { get; } = "Kafka:ConfigurationTopic";
        public static string KAFKA_LOAD_TOPIC { get; } = "Kafka:LoadTopic";
        public static string KAFKA_LOAD_TOPIC_PROCESS { get; } = "Kafka:ProcessLoadEventTopic";
        public static string KAFKA_CUSTOM_TOPIC { get; } = "Kafka:CustomTopic";
        public static string API_GATEWAY { get; } = "ApiGateway";
        public static string SERVER_SLOT_ALIVE_CHECKER { get; } = "ServerSlotApi:Check";
    }
}
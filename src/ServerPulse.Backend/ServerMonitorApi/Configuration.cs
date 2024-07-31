namespace ServerMonitorApi
{
    public class Configuration
    {
        public static string CONSUL_HOST { get; } = "Consul:Host";
        public static string CONSUL_SERVICE_NAME { get; } = "Consul:ServiceName";
        public static string CONSUL_SERVICE_PORT { get; } = "Consul:ServicePort";
        public static string KAFKA_BOOTSTRAP_SERVERS { get; } = "Kafka:BootstrapServers";
        public static string KAFKA_CLIENT_ID { get; } = "Kafka:ClientId";
        public static string KAFKA_PARTITIONS_AMOUNT { get; } = "Kafka:PartitionsAmount";
        public static string KAFKA_ALIVE_TOPIC { get; } = "Kafka:AliveTopic";
        public static string KAFKA_CONFIGURATION_TOPIC { get; } = "Kafka:ConfigurationTopic";
        public static string KAFKA_LOAD_TOPIC { get; } = "Kafka:LoadTopic";
        public static string REDIS_CONNECTION_STRING { get; } = "RedisServer";
        public static string REDIS_SERVER_SLOT_EXPIRY_IN_MINUTES { get; } = "Redis:ServerSlotExpiryInMinutes";
        public static string API_GATEWAY { get; } = "ApiGateway";
        public static string SERVER_SLOT_ALIVE_CHECKER { get; } = "ServerSlotApi:Check";
    }
}
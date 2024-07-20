namespace ServerInteractionApi
{
    public class Configuration
    {
        public static string KAFKA_BOOTSTRAP_SERVERS { get; } = "Kafka:BootstrapServers";
        public static string KAFKA_CLIENT_ID { get; } = "Kafka:ClientId";
        public static string KAFKA_PARTITIONS_AMOUNT { get; } = "Kafka:PartitionsAmount";
        public static string SERVER_SLOT_API { get; } = "ServerSlotApi";
        public static string REDIS_CONNECTION_STRING { get; } = "ServerInteractionRedis";
    }
}

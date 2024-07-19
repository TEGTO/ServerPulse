namespace ServerInteractionApi
{
    public class Configuration
    {
        public static string KAFKA_BOOTSTRAP_SERVERS { get; } = "Kafka:BootstrapServers";
        public static string KAFKA_CLIENT_ID { get; } = "Kafka:ClientId";
        public static string KAFKA_PARTITIONS_AMOUNT { get; } = "Kafka:PartitionsAmount";
    }
}

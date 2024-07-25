namespace AnalyzerApi
{
    public class Configuration
    {
        public static string KAFKA_BOOTSTRAP_SERVERS { get; } = "Kafka:BootstrapServers";
        public static string KAFKA_CLIENT_ID { get; } = "Kafka:ClientId";
        public static string KAFKA_GROUP_ID { get; } = "Kafka:GroupId";
        public static string KAFKA_ALIVE_TOPIC { get; } = "Kafka:AliveTopic";
        public static string KAFKA_TIMEOUT_IN_MILLISECONDS { get; } = "Kafka:AnalyzerReceiveTimeout";
    }
}

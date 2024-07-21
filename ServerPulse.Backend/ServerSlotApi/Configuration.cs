namespace ServerSlotApi
{
    public class Configuration
    {
        public static string EF_CREATE_DATABASE { get; } = "EFCreateDatabase";
        public static string SERVER_SLOT_DATABASE_CONNECTION_STRING { get; } = "ServerSlotDataConnection";
        public static string JWT_SETTINGS_KEY { get; } = "AuthSettings:Key";
        public static string JWT_SETTINGS_AUDIENCE { get; } = "AuthSettings:Audience";
        public static string JWT_SETTINGS_ISSUER { get; } = "AuthSettings:Issuer";
        public static string JWT_SETTINGS_EXPIRY_IN_MINUTES { get; } = "AuthSettings:ExpiryInMinutes";
        public static string SERVER_SLOTS_PER_USER { get; } = "SlotsPerUser";
        public static string KAFKA_KEY_DELETE_TOPIC { get; } = "Kafka:DeleteTopic";
        public static string KAFKA_TIMEOUT_IN_MILLISECONDS { get; } = "Kafka:TimeoutInMilliseconds";
        public static string KAFKA_PARTITIONS_AMOUNT { get; } = "Kafka:PartitionsAmount";
        public static string KAFKA_BOOTSTRAP_SERVERS { get; } = "Kafka:BootstrapServers";
        public static string KAFKA_CLIENT_ID { get; } = "Kafka:ClientId";
    }
}

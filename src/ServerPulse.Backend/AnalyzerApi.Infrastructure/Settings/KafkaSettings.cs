namespace AnalyzerApi.Infrastructure.Settings
{
    public class KafkaSettings
    {
        public const string SETTINGS_SECTION = "Kafka";

        public string BootstrapServers { get; set; } = string.Empty;
        public string ClientId { get; set; } = string.Empty;
        public string GroupId { get; set; } = string.Empty;
    }
}

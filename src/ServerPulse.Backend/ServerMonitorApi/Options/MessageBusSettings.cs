namespace ServerMonitorApi.Options
{
    public class MessageBusSettings
    {
        public const string SETTINGS_SECTION = "Kafka";

        public string BootstrapServers { get; set; } = string.Empty;
        public string ClientId { get; set; } = string.Empty;
        public string AliveTopic { get; set; } = string.Empty;
        public string ConfigurationTopic { get; set; } = string.Empty;
        public string LoadTopic { get; set; } = string.Empty;
        public string LoadTopicProcess { get; set; } = string.Empty;
        public string CustomTopic { get; set; } = string.Empty;
    }
}

namespace AnalyzerApi.Infrastructure.Configuration
{
    public class LoadProcessingSettings
    {
        public const string SETTINGS_SECTION = "LoadEventProcessing";

        public string Resilience { get; set; } = "LoadEventProcessing:Resilience";
        public int BatchSize { get; set; }
        public int BatchIntervalInMilliseconds { get; set; }
    }
}

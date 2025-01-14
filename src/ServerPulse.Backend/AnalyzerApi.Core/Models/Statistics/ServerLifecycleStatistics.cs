namespace AnalyzerApi.Core.Models.Statistics
{
    public class ServerLifecycleStatistics : BaseStatistics
    {
        public bool IsAlive { get; set; }
        public bool DataExists { get; set; }
        public DateTime? ServerLastStartDateTimeUTC { get; set; }
        public TimeSpan? ServerUptime { get; set; }
        public TimeSpan? LastServerUptime { get; set; }
        public DateTime? LastPulseDateTimeUTC { get; set; }
    }
}
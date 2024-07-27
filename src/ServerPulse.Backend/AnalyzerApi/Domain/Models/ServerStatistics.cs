namespace AnalyzerApi.Domain.Models
{
    public class ServerStatistics
    {
        public bool IsAlive { get; set; }
        public DateTime? ServerLastStartDateTime { get; set; }
        public TimeSpan? ServerUptime { get; set; }
        public TimeSpan? LastServerUptime { get; set; }
        public DateTime? LastPulseDateTime { get; set; }
        public DateTime? LastLoadDateTime { get; set; }
        public double? LoadEventNumber { get; set; }
    }
}
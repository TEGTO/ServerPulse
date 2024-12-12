using AnalyzerApi.Infrastructure.Models.Wrappers;

namespace AnalyzerApi.Infrastructure.Models.Statistics
{
    public class ServerLoadStatistics : BaseStatistics
    {
        public int AmountOfEvents { get; set; }
        public LoadEventWrapper? LastEvent { get; set; }
        public LoadMethodStatistics? LoadMethodStatistics { get; set; }
    }
}
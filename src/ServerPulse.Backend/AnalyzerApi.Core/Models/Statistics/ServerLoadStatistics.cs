using AnalyzerApi.Core.Models.Wrappers;

namespace AnalyzerApi.Core.Models.Statistics
{
    public class ServerLoadStatistics : BaseStatistics
    {
        public int AmountOfEvents { get; set; }
        public LoadEventWrapper? LastEvent { get; set; }
        public LoadMethodStatistics? LoadMethodStatistics { get; set; }
    }
}
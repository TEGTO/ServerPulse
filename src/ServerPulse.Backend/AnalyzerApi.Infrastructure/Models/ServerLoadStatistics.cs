using AnalyzerApi.Infrastructure.Wrappers;

namespace AnalyzerApi.Infrastructure.Models
{
    public class ServerLoadStatistics : BaseStatistics
    {
        public int AmountOfEvents { get; set; }
        public LoadEventWrapper? LastEvent { get; set; }
        public LoadMethodStatistics? LoadMethodStatistics { get; set; }
    }
}
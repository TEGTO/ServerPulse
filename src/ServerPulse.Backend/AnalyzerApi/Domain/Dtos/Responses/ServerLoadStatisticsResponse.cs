using AnalyzerApi.Domain.Dtos.Wrappers;

namespace AnalyzerApi.Domain.Dtos.Responses
{
    public class ServerLoadStatisticsResponse
    {
        public int AmountOfEvents { get; set; }
        public LoadEventWrapper? LastEvent { get; set; }
        public LoadMethodStatisticsWrapper? LoadMethodStatistics { get; set; }
        public DateTime CollectedDateUTC { get; set; }
        public bool IsInitial { get; set; }
    }
}
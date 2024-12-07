using AnalyzerApi.Infrastructure.Wrappers;

namespace AnalyzerApi.Infrastructure.Dtos.Responses.Statistics
{
    public class ServerLoadStatisticsResponse : BaseStatisticsResponse
    {
        public int AmountOfEvents { get; set; }
        public LoadEventWrapper? LastEvent { get; set; }
        public LoadMethodStatisticsResponse? LoadMethodStatistics { get; set; }
    }
}
using AnalyzerApi.Infrastructure.Dtos.Responses.Events;

namespace AnalyzerApi.Infrastructure.Dtos.Responses.Statistics
{
    public class ServerLoadStatisticsResponse : BaseStatisticsResponse
    {
        public int AmountOfEvents { get; set; }
        public LoadEventResponse? LastEvent { get; set; }
        public LoadMethodStatisticsResponse? LoadMethodStatistics { get; set; }
    }
}
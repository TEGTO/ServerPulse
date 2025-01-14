using AnalyzerApi.Core.Dtos.Responses.Events;

namespace AnalyzerApi.Core.Dtos.Responses.Statistics
{
    public class ServerLoadStatisticsResponse : BaseStatisticsResponse
    {
        public int AmountOfEvents { get; set; }
        public LoadEventResponse? LastEvent { get; set; }
        public LoadMethodStatisticsResponse? LoadMethodStatistics { get; set; }
    }
}
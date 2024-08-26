using AnalyzerApi.Domain.Dtos.Wrappers;

namespace AnalyzerApi.Domain.Dtos.Responses
{
    public class ServerLoadStatisticsResponse : BaseStatisticsResponse
    {
        public int AmountOfEvents { get; set; }
        public LoadEventWrapper? LastEvent { get; set; }
        public LoadMethodStatisticsResponse? LoadMethodStatistics { get; set; }
    }
}
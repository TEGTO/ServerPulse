using AnalyzerApi.Infrastructure.Dtos.Responses.Events;

namespace AnalyzerApi.Infrastructure.Dtos.Responses.Statistics
{
    public class ServerCustomStatisticsResponse : BaseStatisticsResponse
    {
        public CustomEventResponse? LastEvent { get; set; }
    }
}
using AnalyzerApi.Core.Dtos.Responses.Events;

namespace AnalyzerApi.Core.Dtos.Responses.Statistics
{
    public class ServerCustomStatisticsResponse : BaseStatisticsResponse
    {
        public CustomEventResponse? LastEvent { get; set; }
    }
}
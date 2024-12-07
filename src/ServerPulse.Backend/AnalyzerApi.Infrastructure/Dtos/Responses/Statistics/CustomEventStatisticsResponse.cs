using AnalyzerApi.Infrastructure.Wrappers;

namespace AnalyzerApi.Infrastructure.Dtos.Responses.Statistics
{
    public class CustomEventStatisticsResponse : BaseStatisticsResponse
    {
        public CustomEventWrapper? LastEvent { get; set; }
    }
}
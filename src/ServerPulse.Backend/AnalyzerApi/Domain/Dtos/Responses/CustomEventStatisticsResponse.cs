using AnalyzerApi.Domain.Dtos.Wrappers;

namespace AnalyzerApi.Domain.Dtos.Responses
{
    public class CustomEventStatisticsResponse : BaseStatisticsResponse
    {
        public CustomEventWrapper? LastEvent { get; set; }
    }
}
using AnalyzerApi.Domain.Dtos.Wrappers;

namespace AnalyzerApi.Domain.Dtos.Responses
{
    public class CustomEventStatisticsResponse
    {
        public CustomEventWrapper? LastEvent { get; set; }
    }
}
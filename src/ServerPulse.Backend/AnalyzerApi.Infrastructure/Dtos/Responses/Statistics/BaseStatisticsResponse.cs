namespace AnalyzerApi.Infrastructure.Dtos.Responses.Statistics
{
    public class BaseStatisticsResponse
    {
        public bool IsInitial { get; set; }
        public DateTime CollectedDateUTC { get; set; }
    }
}
namespace AnalyzerApi.Infrastructure.Dtos.Responses.Statistics
{
    public abstract class BaseStatisticsResponse
    {
        public bool IsInitial { get; set; }
        public DateTime CollectedDateUTC { get; set; }
    }
}
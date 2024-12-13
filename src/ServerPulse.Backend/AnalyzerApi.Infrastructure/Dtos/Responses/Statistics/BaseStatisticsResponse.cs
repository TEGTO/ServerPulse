namespace AnalyzerApi.Infrastructure.Dtos.Responses.Statistics
{
    public abstract class BaseStatisticsResponse
    {
        public DateTime CollectedDateUTC { get; set; }
    }
}
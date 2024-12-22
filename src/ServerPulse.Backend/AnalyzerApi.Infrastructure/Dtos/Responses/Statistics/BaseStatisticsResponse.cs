namespace AnalyzerApi.Infrastructure.Dtos.Responses.Statistics
{
    public abstract class BaseStatisticsResponse
    {
        public string? Id { get; init; }
        public DateTime CollectedDateUTC { get; set; }
    }
}
namespace AnalyzerApi.Core.Dtos.Responses.Statistics
{
    public abstract class BaseStatisticsResponse
    {
        public string? Id { get; init; }
        public DateTime CollectedDateUTC { get; set; }
    }
}
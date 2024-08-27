namespace AnalyzerApi.Domain.Dtos.Responses
{
    public class BaseStatisticsResponse
    {
        public bool IsInitial { get; set; }
        public DateTime CollectedDateUTC { get; set; }
    }
}
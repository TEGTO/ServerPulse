namespace AnalyzerApi.Core.Dtos.Endpoints.Analyze.GetLoadAmountStatisticsInRange
{
    public class GetLoadAmountStatisticsInRangeRequest
    {
        public string Key { get; set; } = string.Empty;
        public DateTime From { get; set; }
        public DateTime To { get; set; }
        public TimeSpan TimeSpan { get; set; }
    }
}

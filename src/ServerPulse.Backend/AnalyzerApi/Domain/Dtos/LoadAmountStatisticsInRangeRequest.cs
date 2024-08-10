namespace AnalyzerApi.Domain.Dtos
{
    public class LoadAmountStatisticsInRangeRequest
    {
        public string Key { get; set; }
        public DateTime From { get; set; }
        public DateTime To { get; set; }
        public TimeSpan TimeSpan { get; set; }
    }
}

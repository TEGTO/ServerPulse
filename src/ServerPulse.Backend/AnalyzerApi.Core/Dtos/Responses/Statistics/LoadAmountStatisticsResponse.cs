namespace AnalyzerApi.Core.Dtos.Responses.Statistics
{
    public class LoadAmountStatisticsResponse : BaseStatisticsResponse
    {
        public int AmountOfEvents { get; set; }
        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }
    }
}

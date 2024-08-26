namespace AnalyzerApi.Domain.Dtos.Responses
{
    public class LoadAmountStatisticsResponse : BaseStatisticsResponse
    {
        public int AmountOfEvents { get; set; }
        public DateTime Date { get; set; }
    }
}

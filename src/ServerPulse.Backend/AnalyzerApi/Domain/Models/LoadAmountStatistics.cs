namespace AnalyzerApi.Domain.Models
{
    public class LoadAmountStatistics : BaseStatistics
    {
        public int AmountOfEvents { get; set; }
        public DateTime Date { get; set; }
    }
}

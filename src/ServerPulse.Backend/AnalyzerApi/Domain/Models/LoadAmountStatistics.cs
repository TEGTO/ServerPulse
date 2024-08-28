namespace AnalyzerApi.Domain.Models
{
    public class LoadAmountStatistics : BaseStatistics
    {
        public int AmountOfEvents { get; set; }
        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }
    }
}

namespace AnalyzerApi.Infrastructure.Models.Statistics
{
    public abstract class BaseStatistics
    {
        public bool IsInitial { get; set; }
        public DateTime CollectedDateUTC { get; init; }

        public BaseStatistics()
        {
            CollectedDateUTC = DateTime.UtcNow;
        }
    }
}
namespace AnalyzerApi.Infrastructure.Models.Statistics
{
    public abstract class BaseStatistics
    {
        public DateTime CollectedDateUTC { get; init; }

        protected BaseStatistics()
        {
            CollectedDateUTC = DateTime.UtcNow;
        }
    }
}
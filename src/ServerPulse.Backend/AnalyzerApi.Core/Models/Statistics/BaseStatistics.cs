namespace AnalyzerApi.Core.Models.Statistics
{
    public abstract class BaseStatistics
    {
        public string Id { get; init; }
        public DateTime CollectedDateUTC { get; init; }

        protected BaseStatistics()
        {
            CollectedDateUTC = DateTime.UtcNow;
            Id = Guid.NewGuid().ToString();
        }
    }
}
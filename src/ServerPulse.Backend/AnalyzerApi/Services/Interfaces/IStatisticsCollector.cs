namespace AnalyzerApi.Services.Interfaces
{
    public interface IStatisticsCollector
    {
        public void StartConsumingStatistics(string key);
        public void StopConsumingStatistics(string key);
    }
}
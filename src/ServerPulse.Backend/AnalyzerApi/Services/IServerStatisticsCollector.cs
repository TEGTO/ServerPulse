namespace AnalyzerApi.Services
{
    public interface IServerStatisticsCollector
    {
        public void StartConsumingStatistics(string key);
        public void StopConsumingStatistics(string key);
    }
}
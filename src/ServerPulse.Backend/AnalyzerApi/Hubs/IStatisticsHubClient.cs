namespace AnalyzerApi.Hubs
{
    public interface IStatisticsHubClient
    {
        public Task ReceiveStatistics(string key, string serializedStatistics);
    }
}

using AnalyzerApi.Infrastructure.Dtos.Responses.Statistics;

namespace AnalyzerApi.Hubs
{
    public interface IStatisticsHubClient
    {
        public Task ReceiveStatistics(string key, ServerCustomStatisticsResponse response);
        public Task ReceiveStatistics(string key, ServerLifecycleStatisticsResponse response);
        public Task ReceiveStatistics(string key, ServerLoadStatisticsResponse response);
    }
}
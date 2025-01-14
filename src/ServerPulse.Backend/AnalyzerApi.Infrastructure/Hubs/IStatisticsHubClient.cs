using AnalyzerApi.Core.Dtos.Responses.Statistics;

namespace AnalyzerApi.Infrastructure.Hubs
{
    public interface IStatisticsHubClient<in T> where T : BaseStatisticsResponse
    {
        public Task ReceiveStatistics(string key, T response);
    }
}
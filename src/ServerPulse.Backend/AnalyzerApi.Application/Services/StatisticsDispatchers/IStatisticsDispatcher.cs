using AnalyzerApi.Core.Models.Statistics;

namespace AnalyzerApi.Application.Services.StatisticsDispatchers
{
    public interface IStatisticsDispatcher<T> where T : BaseStatistics
    {
        public Task StartStatisticsDispatchingAsync(string key);
        public Task StopStatisticsDispatchingAsync(string key);
        public Task DispatchInitialStatisticsAsync(string key, CancellationToken cancellationToken = default);
    }
}
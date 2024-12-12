using AnalyzerApi.Infrastructure.Models.Statistics;

namespace AnalyzerApi.Services.StatisticsDispatchers
{
    public interface IStatisticsDispatcher<T> where T : BaseStatistics
    {
        public void StartStatisticsDispatching(string key);
        public void StopStatisticsDispatching(string key);
    }
}
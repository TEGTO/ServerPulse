using AnalyzerApi.Domain.Models;

namespace AnalyzerApi.Services.Interfaces
{
    public interface IStatisticsConsumer<T> where T : BaseStatistics
    {
        public void StartConsumingStatistics(string key);
        public void StopConsumingStatistics(string key);
    }
}
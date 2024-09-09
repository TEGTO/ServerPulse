using AnalyzerApi.Domain.Models;

namespace AnalyzerApi.Services.Interfaces
{
    public interface IStatisticsCollector<T> where T : BaseStatistics
    {
        public Task<T> ReceiveLastStatisticsAsync(string key, CancellationToken cancellationToken);
    }
}

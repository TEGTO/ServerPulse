using AnalyzerApi.Infrastructure.Models.Statistics;

namespace AnalyzerApi.Services.Receivers.Statistics
{
    public interface IStatisticsReceiver<T> where T : BaseStatistics
    {
        public Task<T?> GetLastStatisticsAsync(string key, CancellationToken cancellationToken);
    }
}

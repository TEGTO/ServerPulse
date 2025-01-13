using AnalyzerApi.Core.Models.Statistics;

namespace AnalyzerApi.Application.Services.Receivers.Statistics
{
    public interface IStatisticsReceiver<T> where T : BaseStatistics
    {
        public Task<T?> GetLastStatisticsAsync(string key, CancellationToken cancellationToken);
    }
}

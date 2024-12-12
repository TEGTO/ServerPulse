using AnalyzerApi.Infrastructure.Models;
using AnalyzerApi.Infrastructure.Models.Statistics;

namespace AnalyzerApi.Services.Receivers.Statistics
{
    public interface ILoadAmountStatisticsReceiver : IStatisticsReceiver<LoadAmountStatistics>
    {
        public Task<IEnumerable<LoadAmountStatistics>> GetStatisticsInRangeAsync(InRangeQuery options, TimeSpan timeSpan, CancellationToken cancellationToken);
        public Task<IEnumerable<LoadAmountStatistics>> GetWholeStatisticsInTimeSpanAsync(string key, TimeSpan timeSpan, CancellationToken cancellationToken);
    }
}
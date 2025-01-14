using AnalyzerApi.Core.Models;
using AnalyzerApi.Core.Models.Statistics;

namespace AnalyzerApi.Application.Services.Receivers.Statistics
{
    public interface ILoadAmountStatisticsReceiver : IStatisticsReceiver<LoadAmountStatistics>
    {
        public Task<IEnumerable<LoadAmountStatistics>> GetStatisticsInRangeAsync(GetInRangeOptions options, TimeSpan timeSpan, CancellationToken cancellationToken);
        public Task<IEnumerable<LoadAmountStatistics>> GetWholeStatisticsInTimeSpanAsync(string key, TimeSpan timeSpan, CancellationToken cancellationToken);
    }
}
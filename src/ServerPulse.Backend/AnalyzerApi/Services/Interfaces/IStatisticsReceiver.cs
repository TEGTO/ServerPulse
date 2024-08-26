using AnalyzerApi.Domain.Models;

namespace AnalyzerApi.Services.Interfaces
{
    public record InRangeQueryOptions(string Key, DateTime From, DateTime To);
    public record ReadCertainMessageNumberOptions(string Key, int NumberOfMessages, DateTime StartDate, bool ReadNew);

    public interface IStatisticsReceiver<TStatistics> where TStatistics : BaseStatistics
    {
        public Task<TStatistics?> ReceiveLastStatisticsByKeyAsync(string key, CancellationToken cancellationToken);
        public Task<IEnumerable<TStatistics>> GetWholeStatisticsInTimeSpanAsync(string key, TimeSpan timeSpan, CancellationToken cancellationToken);
        public Task<IEnumerable<TStatistics>> GetStatisticsInRangeAsync(InRangeQueryOptions options, TimeSpan timeSpan, CancellationToken cancellationToken);
    }
}
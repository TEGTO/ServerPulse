using AnalyzerApi.Domain.Models;

namespace AnalyzerApi.Services.Interfaces
{
    public record InRangeQueryOptions(string Key, DateTime From, DateTime To);
    public record ReadCertainMessageNumberOptions(string Key, int NumberOfMessages, DateTime StartDate, bool ReadNew);
    public record class StatisticsReceiverTopicData<TStatistics>(string topicOriginName) where TStatistics : BaseStatistics;
    public interface IStatisticsReceiver<T> where T : BaseStatistics
    {
        public Task<T> ReceiveLastStatisticsAsync(string key, CancellationToken cancellationToken);
        public Task<IEnumerable<T>> GetWholeStatisticsInTimeSpanAsync(string key, TimeSpan timeSpan, CancellationToken cancellationToken);
        public Task<IEnumerable<T>> GetStatisticsInRangeAsync(InRangeQueryOptions options, TimeSpan timeSpan, CancellationToken cancellationToken);
    }
}

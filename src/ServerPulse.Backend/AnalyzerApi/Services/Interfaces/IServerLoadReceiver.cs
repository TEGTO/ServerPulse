using AnalyzerApi.Domain.Models;
using ServerPulse.EventCommunication.Events;
using System.Runtime.CompilerServices;

namespace AnalyzerApi.Services.Interfaces
{
    public interface IServerLoadReceiver
    {
        public IAsyncEnumerable<LoadEvent> ConsumeLoadEventAsync(string key, [EnumeratorCancellation] CancellationToken cancellationToken);
        public Task<IEnumerable<LoadEvent>> ReceiveEventsInRangeAsync(InRangeQueryOptions options, CancellationToken cancellationToken);
        public Task<LoadEvent?> ReceiveLastLoadEventByKeyAsync(string key, CancellationToken cancellationToken);
        public Task<int> ReceiveLoadEventAmountByKeyAsync(string key, CancellationToken cancellationToken);
        public Task<IEnumerable<LoadAmountStatistics>> GetAmountStatisticsInDaysAsync(string key, CancellationToken cancellationToken);
        public Task<IEnumerable<LoadAmountStatistics>> GetAmountStatisticsLastDayAsync(string key, CancellationToken cancellationToken);
        public Task<IEnumerable<LoadAmountStatistics>> GetAmountStatisticsInRangeAsync(InRangeQueryOptions options, TimeSpan timeSpan, CancellationToken cancellationToken);
    }
}
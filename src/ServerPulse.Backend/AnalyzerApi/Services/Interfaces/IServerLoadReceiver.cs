using AnalyzerApi.Domain.Dtos.Wrappers;
using AnalyzerApi.Domain.Models;
using System.Runtime.CompilerServices;

namespace AnalyzerApi.Services.Interfaces
{
    public interface IServerLoadReceiver
    {
        public IAsyncEnumerable<LoadEventWrapper> ConsumeLoadEventAsync(string key, [EnumeratorCancellation] CancellationToken cancellationToken);
        public Task<IEnumerable<LoadEventWrapper>> ReceiveEventsInRangeAsync(InRangeQueryOptions options, CancellationToken cancellationToken);
        public Task<LoadEventWrapper?> ReceiveLastLoadEventByKeyAsync(string key, CancellationToken cancellationToken);
        public Task<int> ReceiveLoadEventAmountByKeyAsync(string key, CancellationToken cancellationToken);
        public Task<IEnumerable<LoadEventWrapper>> GetCertainAmountOfEvents(ReadCertainMessageNumberOptions options, CancellationToken cancellationToken);
        public Task<IEnumerable<LoadAmountStatistics>> GetAmountStatisticsInDaysAsync(string key, CancellationToken cancellationToken);
        public Task<IEnumerable<LoadAmountStatistics>> GetAmountStatisticsLastDayAsync(string key, CancellationToken cancellationToken);
        public Task<IEnumerable<LoadAmountStatistics>> GetAmountStatisticsInRangeAsync(InRangeQueryOptions options, TimeSpan timeSpan, CancellationToken cancellationToken);
    }
}
using AnalyzerApi.Infrastructure.Wrappers;

namespace AnalyzerApi.Services.Interfaces
{
    public interface IEventReceiver<TEvent> where TEvent : BaseEventWrapper
    {
        public IAsyncEnumerable<TEvent> ConsumeEventAsync(string key, CancellationToken cancellationToken);
        public Task<IEnumerable<TEvent>> GetCertainAmountOfEventsAsync(ReadCertainMessageNumberOptions options, CancellationToken cancellationToken);
        public Task<IEnumerable<TEvent>> ReceiveEventsInRangeAsync(InRangeQueryOptions options, CancellationToken cancellationToken);
        public Task<TEvent?> ReceiveLastEventByKeyAsync(string key, CancellationToken cancellationToken);
        public Task<int> ReceiveEventAmountByKeyAsync(string key, CancellationToken cancellationToken);
    }
}
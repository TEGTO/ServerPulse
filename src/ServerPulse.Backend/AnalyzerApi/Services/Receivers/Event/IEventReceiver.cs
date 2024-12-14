using AnalyzerApi.Infrastructure.Models;
using AnalyzerApi.Infrastructure.Models.Wrappers;

namespace AnalyzerApi.Services.Receivers.Event
{
    public interface IEventReceiver<TEvent> where TEvent : BaseEventWrapper
    {
        public IAsyncEnumerable<TEvent> GetEventStreamAsync(string key, CancellationToken cancellationToken);
        public Task<int> GetEventAmountByKeyAsync(string key, CancellationToken cancellationToken);
        public Task<IEnumerable<TEvent>> GetCertainAmountOfEventsAsync(GetCertainMessageNumberOptions options, CancellationToken cancellationToken);
        public Task<IEnumerable<TEvent>> GetEventsInRangeAsync(GetInRangeOptions options, CancellationToken cancellationToken);
        public Task<TEvent?> GetLastEventByKeyAsync(string key, CancellationToken cancellationToken);
    }
}
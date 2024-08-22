using AnalyzerApi.Domain.Dtos.Wrappers;

namespace AnalyzerApi.Services.Interfaces
{
    public interface ICustomReceiver
    {
        public IAsyncEnumerable<CustomEventWrapper> ConsumeCustomEventAsync(string key, CancellationToken cancellationToken);
        public Task<IEnumerable<CustomEventWrapper>> ReceiveEventsInRangeAsync(InRangeQueryOptions options, CancellationToken cancellationToken);
        public Task<CustomEventWrapper?> ReceiveLastCustomEventByKeyAsync(string key, CancellationToken cancellationToken);
        public Task<IEnumerable<CustomEventWrapper>> GetCertainAmountOfEvents(ReadCertainMessageNumberOptions options, CancellationToken cancellationToken);
    }
}
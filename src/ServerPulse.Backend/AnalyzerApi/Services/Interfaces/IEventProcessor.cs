using ServerPulse.EventCommunication.Events;

namespace AnalyzerApi.Services.Interfaces
{
    public interface IEventProcessor
    {
        public Task ProcessEventsAsync<T>(T[] events, CancellationToken cancellationToken) where T : BaseEvent;
    }
}
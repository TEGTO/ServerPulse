using ServerPulse.EventCommunication.Events;

namespace ServerMonitorApi.Services
{
    public interface IEventProcessing
    {
        Task SendEventsForProcessingsAsync<T>(T[] events, CancellationToken cancellationToken) where T : BaseEvent;
    }
}
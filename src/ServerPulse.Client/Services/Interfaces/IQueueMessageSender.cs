using ServerPulse.EventCommunication.Events;

namespace ServerPulse.Client.Services.Interfaces
{
    public interface IQueueMessageSender<T> where T : BaseEvent
    {
        public void SendEvent(T ev);
    }
}
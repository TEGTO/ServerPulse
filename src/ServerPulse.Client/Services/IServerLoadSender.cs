using ServerPulse.EventCommunication.Events;

namespace ServerPulse.Client.Services
{
    public interface IServerLoadSender
    {
        public void SendEvent(LoadEvent loadEvent);
    }
}
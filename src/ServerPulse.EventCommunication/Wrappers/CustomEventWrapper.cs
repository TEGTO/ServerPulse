using ServerPulse.EventCommunication.Events;

namespace ServerPulse.EventCommunication
{
    public record class CustomEventWrapper
    {
        public CustomEvent CustomEvent { get; init; }
        public string CustomEventSerialized { get; init; }

        public CustomEventWrapper(CustomEvent customEvent, string customEventSerialized)
        {
            CustomEvent = customEvent;
            this.CustomEventSerialized = customEventSerialized;
        }
    }
}
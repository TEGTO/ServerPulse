using ServerPulse.EventCommunication.Events;

namespace ServerPulse.EventCommunication
{
    public record class CustomEventShell
    {
        public CustomEvent CustomEvent { get; init; }
        public string CustomEventSerialized { get; init; }

        public CustomEventShell(CustomEvent customEvent)
        {
            CustomEvent = customEvent;
            CustomEventSerialized = customEvent.ToString();
        }
    }
}
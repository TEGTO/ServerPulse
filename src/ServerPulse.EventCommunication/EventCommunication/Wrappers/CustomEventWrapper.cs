using EventCommunication.Events;

namespace EventCommunication.Wrappers
{
    public record class CustomEventWrapper
    {
        public CustomEvent CustomEvent { get; init; }
        public string CustomEventSerialized { get; init; }

        public CustomEventWrapper(CustomEvent customEvent, string customEventSerialized)
        {
            CustomEvent = customEvent;
            CustomEventSerialized = customEventSerialized;
        }
    }
}
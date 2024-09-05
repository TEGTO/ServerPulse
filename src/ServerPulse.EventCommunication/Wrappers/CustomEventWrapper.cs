using ServerPulse.EventCommunication.Events;
using System.Text.Json;

namespace ServerPulse.EventCommunication
{
    public record class CustomEventWrapper
    {
        public CustomEvent CustomEvent { get; init; }
        public string CustomEventSerialized { get; init; }

        public CustomEventWrapper(CustomEvent customEvent)
        {
            CustomEvent = customEvent;
            CustomEventSerialized = JsonSerializer.Serialize(customEvent);
        }
    }
}
namespace EventCommunication
{
    public record class CustomEventContainer
    {
        public CustomEvent CustomEvent { get; init; }
        public string CustomEventSerialized { get; init; }

        public CustomEventContainer(CustomEvent customEvent, string customEventSerialized)
        {
            CustomEvent = customEvent;
            CustomEventSerialized = customEventSerialized;
        }
    }
}
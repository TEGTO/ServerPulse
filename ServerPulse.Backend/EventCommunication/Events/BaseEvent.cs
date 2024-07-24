namespace EventCommunication.Events
{
    public record BaseEvent()
    {
        public DateTime CreationDate { get; } = DateTime.UtcNow;
    }
}

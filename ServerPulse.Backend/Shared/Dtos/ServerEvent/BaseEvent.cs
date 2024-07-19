namespace MessageBus.Dtos
{
    public record BaseEvent(string SeverSlotId)
    {
        public DateTime CreationDate { get; } = DateTime.UtcNow;
    }
}

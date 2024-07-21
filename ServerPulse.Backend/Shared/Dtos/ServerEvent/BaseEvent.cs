namespace MessageBus.Dtos
{
    public record BaseEvent()
    {
        public DateTime CreationDate { get; } = DateTime.UtcNow;
    }
}

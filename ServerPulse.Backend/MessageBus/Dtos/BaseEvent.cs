namespace Kafka.Dtos
{
    public class BaseEvent
    {
        public DateTime CreationDate { get; set; } = DateTime.UtcNow;
    }
}

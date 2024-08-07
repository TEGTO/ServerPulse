using System.Text.Json;

namespace ServerPulse.EventCommunication.Events
{
    public record BaseEvent
    {
        public string Id { get; init; }
        public DateTime CreationDateUTC { get; init; }
        public string Key { get; init; }

        public BaseEvent(string Key)
        {
            Id = Guid.NewGuid().ToString();
            CreationDateUTC = DateTime.UtcNow;
            this.Key = Key;
        }

        public override string ToString()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}
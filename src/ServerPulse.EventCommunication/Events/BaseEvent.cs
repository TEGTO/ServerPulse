using System.Text.Json;

namespace ServerPulse.EventCommunication.Events
{
    public record BaseEvent
    {
        public DateTime CreationDate { get; init; }
        public string Key { get; init; }

        public BaseEvent(string Key)
        {
            CreationDate = DateTime.UtcNow;
            this.Key = Key;
        }

        public override string ToString()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}
using System.Text.Json;

namespace ServerPulse.EventCommunication.Events
{
    public record BaseEvent
    {
        public string Id { get; init; }
        public string Key { get; init; }

        public BaseEvent(string Key)
        {
            Id = Guid.NewGuid().ToString();
            this.Key = Key;
        }

        public override string ToString()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}
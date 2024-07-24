using System.Text.Json;

namespace ServerPulse.Client.Events
{
    public record BaseEvent()
    {
        public DateTime CreationDate { get; } = DateTime.UtcNow;

        public override string ToString()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}

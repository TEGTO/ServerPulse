using System.Text.Json;

namespace ServerPulse.EventCommunication.Events
{
    public record class CustomEvent(string Key, string Name, string Description) : BaseEvent(Key)
    {
        public override string ToString()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}
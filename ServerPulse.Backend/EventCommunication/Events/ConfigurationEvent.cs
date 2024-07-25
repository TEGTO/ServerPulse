using System.Text.Json;

namespace EventCommunication.Events
{
    public sealed record class ConfigurationEvent(string Key, TimeSpan AliveEventSendInterval) : BaseEvent(Key)
    {
        public override string ToString()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}
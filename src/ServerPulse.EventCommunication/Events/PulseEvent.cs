using System.Text.Json;

namespace ServerPulse.EventCommunication.Events
{
    public sealed record PulseEvent(string Key, bool IsAlive) : BaseEvent(Key)
    {
        public override string ToString()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}
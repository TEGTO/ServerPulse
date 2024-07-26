using System.Text.Json;

namespace ServerPulse.EventCommunication.Events
{
    public sealed record AliveEvent(string Key, bool IsAlive) : BaseEvent(Key)
    {
        public override string ToString()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}
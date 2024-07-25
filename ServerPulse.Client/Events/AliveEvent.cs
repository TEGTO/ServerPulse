using System.Text.Json;

namespace ServerPulse.Client.Events
{
    internal sealed record AliveEvent(string Key, bool IsAlive) : BaseEvent(Key)
    {
        public override string ToString()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}
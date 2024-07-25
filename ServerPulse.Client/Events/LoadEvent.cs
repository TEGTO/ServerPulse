using System.Text.Json;

namespace ServerPulse.Client.Events
{
    public sealed record class LoadEvent(string Key) : BaseEvent(Key)
    {
        public override string ToString()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}
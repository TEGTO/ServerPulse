namespace EventCommunication.Events
{
    public sealed record PulseEvent(string Key, bool IsAlive) : BaseEvent(Key);
}
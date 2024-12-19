namespace EventCommunication
{
    public sealed record PulseEvent(string Key, bool IsAlive) : BaseEvent(Key);
}
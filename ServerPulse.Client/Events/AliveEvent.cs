namespace ServerPulse.Client.Events
{
    internal sealed record AliveEvent(string Key, bool IsAlive) : BaseEvent(Key);
}
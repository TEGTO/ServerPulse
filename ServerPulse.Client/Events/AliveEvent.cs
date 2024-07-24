namespace ServerPulse.Client.Events
{
    public sealed record AliveEvent(string Key, bool IsAlive) : BaseEvent();
}
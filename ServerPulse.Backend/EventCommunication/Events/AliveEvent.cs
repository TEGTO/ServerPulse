namespace EventCommunication.Events
{
    public sealed record AliveEvent(string Key, bool IsAlive) : BaseEvent();
}
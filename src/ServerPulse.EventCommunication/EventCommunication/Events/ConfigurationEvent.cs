namespace EventCommunication.Events
{
    public sealed record class ConfigurationEvent(string Key, TimeSpan ServerKeepAliveInterval) : BaseEvent(Key);
}
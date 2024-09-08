namespace ServerPulse.EventCommunication.Events
{
    public sealed record class LoadEvent(
        string Key,
        string Endpoint,
        string Method,
        int StatusCode,
        TimeSpan Duration,
        DateTime TimestampUTC) : BaseEvent(Key);
}
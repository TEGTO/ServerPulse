namespace EventCommunication.Events
{
    public record class CustomEvent(string Key, string Name, string Description) : BaseEvent(Key);
}
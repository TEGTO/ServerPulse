namespace ServerPulse.EventCommunication.Events
{
    public record class CustomEvent(string Key, string Name, string Description) : BaseEvent(Key)
    {
        public sealed override string ToString()
        {
            return base.ToString();
        }
    }
}
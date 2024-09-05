namespace ServerPulse.EventCommunication.Events
{
    public sealed record class ConfigurationEvent(string Key, TimeSpan ServerKeepAliveInterval) : BaseEvent(Key)
    {
        public override string ToString()
        {
            return base.ToString();
        }
    }
}
using System.Text.Json.Serialization;

namespace EventCommunication
{
    [JsonDerivedType(typeof(LoadEvent))]
    [JsonDerivedType(typeof(PulseEvent))]
    [JsonDerivedType(typeof(ConfigurationEvent))]
    [JsonDerivedType(typeof(CustomEvent))]
    public abstract record BaseEvent
    {
        public string Id { get; init; }
        public string Key { get; init; }

        protected BaseEvent(string Key)
        {
            Id = Guid.NewGuid().ToString();
            this.Key = Key;
        }
    }
}
﻿using System.Text.Json.Serialization;

namespace ServerPulse.EventCommunication.Events
{
    [JsonDerivedType(typeof(LoadEvent))]
    [JsonDerivedType(typeof(PulseEvent))]
    [JsonDerivedType(typeof(ConfigurationEvent))]
    [JsonDerivedType(typeof(CustomEvent))]
    public abstract record BaseEvent
    {
        public string Id { get; init; }
        public string Key { get; init; }

        public BaseEvent(string Key)
        {
            Id = Guid.NewGuid().ToString();
            this.Key = Key;
        }
    }
}
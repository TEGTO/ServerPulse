﻿using System.Text.Json;

namespace ServerPulse.Client.Events
{
    internal sealed record class ConfigurationEvent(string Key, TimeSpan ServerKeepAliveInterval) : BaseEvent(Key)
    {
        public override string ToString()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}
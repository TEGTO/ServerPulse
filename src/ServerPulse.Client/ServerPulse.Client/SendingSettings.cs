namespace ServerPulse.Client
{
    public class SendingSettings
    {
        /// <summary>
        /// The Server Slot key. Required to link metrics to Server Pulse.
        /// </summary>
        public required string Key { get; set; }

        /// <summary>
        /// The Server Pulse API URL or Server Monitor API URL for local setups.
        /// </summary>
        public required string EventServer { get; set; } = default!;

        /// <summary>
        /// The frequency (in seconds) for sending pulse events. 
        /// Backend default is one message every 3 seconds.
        /// </summary>
        public double ServerKeepAliveInterval { get; set; } = 10d;

        /// <summary>
        /// Maximum number of events per message (excluding pulse events). 
        /// Backend default is 99 messages.
        /// </summary>
        public int MaxEventSendingAmount { get; set; } = 10;

        /// <summary>
        /// Frequency (in seconds) for sending event data messages (excluding pulse events). 
        /// Backend default is one message every 3 seconds.
        /// </summary>
        public double SendingInterval { get; set; } = 15d;
    }

    public class SendingSettings<T> where T : class
    {
        public required string Key { get; set; }
        public required string SendingEndpoint { get; set; } = default!;
        public int MaxMessageSendingAmount { get; set; } = 10;
        public double SendingInterval { get; set; } = 15d;

        public static SendingSettings<T> CreateCustomSettings(SendingSettings settings, string endpoint, double eventSendingInterval)
        {
            return new SendingSettings<T>
            {
                Key = settings.Key,
                SendingEndpoint = settings.EventServer + endpoint,
                MaxMessageSendingAmount = settings.MaxEventSendingAmount,
                SendingInterval = eventSendingInterval
            };
        }
    }
}
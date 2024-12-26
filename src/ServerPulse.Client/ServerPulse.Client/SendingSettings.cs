namespace ServerPulse.Client
{
    public class SendingSettings
    {
        /// <summary>
        /// The server slot key.
        /// </summary>
        public required string Key { get; set; }
        /// <summary>
        /// Event monitoring server uri. 
        /// </summary>
        public required string EventServer { get; set; } = default!;
        /// <summary>
        /// The frequency, in seconds, at which a message containing pulse event data will be sent.
        /// </summary>
        public double ServerKeepAliveInterval { get; set; } = 10d;
        /// <summary>
        /// The maximum number of events that will be attached to a message (exclude pulse events).
        /// </summary>
        public int MaxEventSendingAmount { get; set; } = 10;
        /// <summary>
        /// How often a message with event data will be sent in seconds (exclude pulse events).
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
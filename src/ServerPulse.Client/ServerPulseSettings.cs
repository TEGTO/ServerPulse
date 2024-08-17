namespace ServerPulse.Client
{
    public class ServerPulseSettings
    {
        public required string Key { get; set; }
        /// <summary>
        /// Event endpoints.
        /// </summary>
        public required string EventController { get; set; } = default!;
        /// <summary>
        /// How often a message with alive event data will be sent in seconds.
        /// </summary>
        public double ServerKeepAliveInterval { get; set; } = 10d;
        /// <summary>
        /// The maximum number of events that will be attached to a message (exclude alive events).
        /// </summary>
        public int MaxEventSendingAmount { get; set; } = 10;
        /// <summary>
        /// How often a message with event data will be sent in seconds (exclude alive events).
        /// </summary>
        public double EventSendingInterval { get; set; } = 15d;
    }
}
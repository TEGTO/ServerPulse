namespace ServerPulse.Client
{
    public class Configuration
    {
        public required string SlotKey { get; set; }
        /// <summary>
        /// Event endpoints.
        /// </summary>
        public required string EventController { get; set; } = default!;
        /// <summary>
        /// How often a message with alive event data will be sent in seconds.
        /// </summary>
        public double AliveEventSendInterval { get; set; } = 5d;
        /// <summary>
        /// The maximum number of events that will be attached to a message.
        /// </summary>
        public int MaxEventSendAmount { get; set; } = 10;
        /// <summary>
        /// How often a message with event data will be sent in seconds.
        /// </summary>
        public double EventSendInterval { get; set; } = 10d;
    }
}
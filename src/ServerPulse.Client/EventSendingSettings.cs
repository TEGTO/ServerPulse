using ServerPulse.EventCommunication.Events;

namespace ServerPulse.Client
{
    public class EventSendingSettings
    {
        /// <summary>
        /// Key of the server slot to connect.
        /// </summary>
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
        /// The maximum number of events that will be attached to a message (exclude pulse events).
        /// </summary>
        public int MaxEventSendingAmount { get; set; } = 10;
        /// <summary>
        /// How often a message with event data will be sent in seconds (exclude pulse events).
        /// </summary>
        public double EventSendingInterval { get; set; } = 15d;
    }
    public class EventSendingSettings<T> where T : BaseEvent
    {
        public required string Key { get; set; }
        public required string EventController { get; set; } = default!;
        public int MaxEventSendingAmount { get; set; } = 10;
        public double EventSendingInterval { get; set; } = 15d;

        public static EventSendingSettings<T> CreateCustomSettings(EventSendingSettings settings, string controller, double eventSendingInterval)
        {
            return new EventSendingSettings<T>
            {
                Key = settings.Key,
                EventController = settings.EventController + controller,
                MaxEventSendingAmount = settings.MaxEventSendingAmount,
                EventSendingInterval = eventSendingInterval
            };
        }
    }
}
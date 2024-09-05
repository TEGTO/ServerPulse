using Microsoft.Extensions.Logging;
using ServerPulse.EventCommunication;
using ServerPulse.EventCommunication.Events;
using System.Text.Json;

namespace ServerPulse.Client.Services
{
    internal sealed class CustomEventSender : QueueMessageSender<CustomEvent>
    {
        public CustomEventSender(IMessageSender messageSender, EventSendingSettings<CustomEvent> settings, ILogger<QueueMessageSender<CustomEvent>> logger) : base(messageSender, settings, logger)
        {
        }

        protected override string GetEventsJson()
        {
            var events = new List<CustomEventWrapper>();
            for (int i = 0; i < settings.MaxEventSendingAmount && eventQueue.TryDequeue(out var ev); i++)
            {
                events.Add(new CustomEventWrapper(ev));
            }
            return JsonSerializer.Serialize(events);
        }
    }
}

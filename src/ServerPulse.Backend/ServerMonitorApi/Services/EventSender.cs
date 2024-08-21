using MessageBus.Interfaces;
using ServerPulse.EventCommunication.Events;

namespace ServerMonitorApi.Services
{
    public class EventSender : IEventSender
    {
        private readonly IMessageProducer producer;
        private readonly string pulseTopic;
        private readonly string configurationTopic;
        private readonly string loadTopic;

        public EventSender(IMessageProducer producer, IConfiguration configuration)
        {
            this.producer = producer;
            pulseTopic = configuration[Configuration.KAFKA_ALIVE_TOPIC]!;
            configurationTopic = configuration[Configuration.KAFKA_CONFIGURATION_TOPIC]!;
            loadTopic = configuration[Configuration.KAFKA_LOAD_TOPIC]!;
        }

        public async Task SendEventsAsync<T>(T[] events, CancellationToken cancellationToken) where T : BaseEvent
        {
            await Parallel.ForEachAsync(events, cancellationToken, async (ev, ct) =>
            {
                string topic = GetTopic(ev);
                if (!string.IsNullOrEmpty(topic))
                {
                    await SendEvent(ev, topic, ct);
                }
            });
        }
        private string GetTopic(BaseEvent ev)
        {
            return ev switch
            {
                PulseEvent _ => GetPulseTopic(ev.Key),
                ConfigurationEvent _ => GetConfigurationTopic(ev.Key),
                LoadEvent _ => GetLoadTopic(ev.Key),
                _ => string.Empty
            };
        }
        private async Task SendEvent(BaseEvent ev, string topic, CancellationToken cancellationToken)
        {
            var message = ev.ToString();
            await producer.ProduceAsync(topic, message, cancellationToken);
        }
        private string GetPulseTopic(string key)
        {
            return pulseTopic + key;
        }
        private string GetConfigurationTopic(string key)
        {
            return configurationTopic + key;
        }
        private string GetLoadTopic(string key)
        {
            return loadTopic + key;
        }
    }
}
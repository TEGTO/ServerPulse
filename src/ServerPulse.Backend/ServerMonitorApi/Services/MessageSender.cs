using MessageBus.Interfaces;
using ServerPulse.EventCommunication.Events;

namespace ServerMonitorApi.Services
{
    public class MessageSender : IMessageSender
    {
        private readonly IMessageProducer producer;
        private readonly string aliveTopic;
        private readonly string configurationTopic;
        private readonly string loadTopic;

        public MessageSender(IMessageProducer producer, IConfiguration configuration)
        {
            this.producer = producer;
            aliveTopic = configuration[Configuration.KAFKA_ALIVE_TOPIC]!;
            configurationTopic = configuration[Configuration.KAFKA_CONFIGURATION_TOPIC]!;
            loadTopic = configuration[Configuration.KAFKA_LOAD_TOPIC]!;
        }

        public async Task SendPulseEventAsync(PulseEvent ev, CancellationToken cancellationToken)
        {
            string topic = GetAliveTopic(ev.Key);
            await SendEvent(ev, topic, cancellationToken);
        }
        public async Task SendConfigurationEventAsync(ConfigurationEvent ev, CancellationToken cancellationToken)
        {
            string topic = GetConfigurationTopic(ev.Key);
            await SendEvent(ev, topic, cancellationToken);
        }
        public async Task SendLoadEventsAsync(LoadEvent[] events, CancellationToken cancellationToken)
        {
            await Parallel.ForEachAsync(events, async (loadEvent, ct) =>
            {
                string topic = GetLoadTopic(loadEvent.Key);
                await SendEvent(loadEvent, topic, ct);
            });
        }
        private async Task SendEvent(BaseEvent ev, string topic, CancellationToken cancellationToken)
        {
            var message = ev.ToString();
            await producer.ProduceAsync(topic, message, cancellationToken);
        }
        private string GetAliveTopic(string key)
        {
            return aliveTopic + key;
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
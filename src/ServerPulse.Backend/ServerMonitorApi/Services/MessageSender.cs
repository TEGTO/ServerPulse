using MessageBus;
using ServerPulse.EventCommunication.Events;

namespace ServerMonitorApi.Services
{
    public class MessageSender : IMessageSender
    {
        private readonly IMessageProducer producer;
        private readonly string aliveTopic;
        private readonly string configurationTopic;
        private readonly string loadTopic;
        private readonly int partitionsAmount;

        public MessageSender(IMessageProducer producer, IConfiguration configuration)
        {
            this.producer = producer;
            partitionsAmount = int.Parse(configuration[Configuration.KAFKA_PARTITIONS_AMOUNT]!);
            aliveTopic = configuration[Configuration.KAFKA_ALIVE_TOPIC]!;
            configurationTopic = configuration[Configuration.KAFKA_CONFIGURATION_TOPIC]!;
            loadTopic = configuration[Configuration.KAFKA_LOAD_TOPIC]!;
        }

        public async Task SendPulseEventAsync(PulseEvent ev, CancellationToken cancellationToken)
        {
            string topic = aliveTopic.Replace("{id}", ev.Key);
            await SendEvent(ev, topic, cancellationToken);
        }
        public async Task SendConfigurationEventAsync(ConfigurationEvent ev, CancellationToken cancellationToken)
        {
            string topic = configurationTopic.Replace("{id}", ev.Key);
            await SendEvent(ev, topic, cancellationToken);
        }
        public async Task SendLoadEventsAsync(LoadEvent[] events, CancellationToken cancellationToken)
        {
            await Parallel.ForEachAsync(events, async (loadEvent, ct) =>
            {
                string topic = loadTopic.Replace("{id}", loadEvent.Key);
                await SendEvent(loadEvent, topic, ct);
            });
        }
        private async Task SendEvent(BaseEvent ev, string topic, CancellationToken cancellationToken)
        {
            var message = ev.ToString();
            await producer.ProduceAsync(topic, message, partitionsAmount, cancellationToken);
        }
    }
}
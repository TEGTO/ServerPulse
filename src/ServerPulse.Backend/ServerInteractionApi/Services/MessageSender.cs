using MessageBus;
using ServerPulse.EventCommunication.Events;

namespace ServerInteractionApi.Services
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

        public async Task SendAliveEventAsync(AliveEvent aliveEvent, CancellationToken cancellationToken)
        {
            string topic = aliveTopic.Replace("{id}", aliveEvent.Key);
            await SendEvent(aliveEvent, topic, cancellationToken);
        }
        public async Task SendConfigurationEventAsync(ConfigurationEvent configurationEvent, CancellationToken cancellationToken)
        {
            string topic = configurationTopic.Replace("{id}", configurationEvent.Key);
            await SendEvent(configurationEvent, topic, cancellationToken);
        }
        public async Task SendLoadEventsAsync(LoadEvent[] loadEvents, CancellationToken cancellationToken)
        {
            await Parallel.ForEachAsync(loadEvents, async (loadEvent, ct) =>
            {
                string topic = loadTopic.Replace("{id}", loadEvent.Key);
                await SendEvent(loadEvent, topic, ct);
            });
        }
        private async Task SendEvent(BaseEvent @event, string topic, CancellationToken cancellationToken)
        {
            var message = @event.ToString();
            await producer.ProduceAsync(topic, message, partitionsAmount, cancellationToken);
        }
    }
}
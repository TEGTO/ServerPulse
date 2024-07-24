using EventCommunication.Events;
using MessageBus;
using System.Text.Json;

namespace ServerInteractionApi.Services
{
    public class MessageSender : IMessageSender
    {
        private readonly IMessageProducer producer;
        private readonly string aliveTopic;
        private readonly int partitionsAmount;

        public MessageSender(IMessageProducer producer, IConfiguration configuration)
        {
            this.producer = producer;
            aliveTopic = configuration[Configuration.KAFKA_ALIVE_TOPIC]!;
            partitionsAmount = int.Parse(configuration[Configuration.KAFKA_PARTITIONS_AMOUNT]!);
        }

        public async Task SendAliveEventAsync(string slotId, CancellationToken cancellationToken)
        {
            string topic = aliveTopic.Replace("{id}", slotId);
            AliveEvent aliveEvent = new AliveEvent(slotId, true);
            var message = JsonSerializer.Serialize(aliveEvent);
            await producer.ProduceAsync(topic, message, partitionsAmount, cancellationToken);
        }
    }
}
using MessageBus;
using Shared.Dtos.ServerEvent;
using System.Text.Json;

namespace ServerInteractionApi.Services
{
    public class MessageSender : IMessageSender
    {
        private readonly IMessageProducer producer;
        private readonly IConfiguration configuration;
        private readonly string aliveTopic;
        private readonly int partitionsAmount;

        public MessageSender(IMessageProducer producer, IConfiguration configuration)
        {
            this.producer = producer;
            this.configuration = configuration;
            aliveTopic = configuration[Configuration.KAFKA_ALIVE_TOPIC]!;
            partitionsAmount = int.Parse(configuration[Configuration.KAFKA_PARTITIONS_AMOUNT]!);
        }

        public async Task SendAliveEventAsync(string slotId)
        {
            string topic = $"{aliveTopic}-{slotId}";
            AliveEvent aliveEvent = new AliveEvent(slotId, true);
            var message = JsonSerializer.Serialize(aliveEvent);
            await producer.ProduceAsync(topic, message, partitionsAmount);
        }
    }
}
using MessageBus;
using Shared.Dtos.ServerEvent;

namespace ServerInteractionApi.Services
{
    public class MessageSender : IMessageSender
    {
        private const string ALIVE_TOPIC = "AliveTopic";

        private readonly IMessageProducer producer;
        private readonly IConfiguration configuration;
        private readonly int partitionsAmount;

        public MessageSender(IMessageProducer producer, IConfiguration configuration)
        {
            this.producer = producer;
            this.configuration = configuration;
            partitionsAmount = int.Parse(configuration[Configuration.KAFKA_PARTITIONS_AMOUNT]);
        }

        public async Task SendAliveEventAsync(string slotId)
        {
            string topic = $"{ALIVE_TOPIC}-{slotId}";
            AliveEvent aliveEvent = new AliveEvent(slotId, true);
            await producer.ProduceAsync(topic, aliveEvent, partitionsAmount);
        }
    }
}
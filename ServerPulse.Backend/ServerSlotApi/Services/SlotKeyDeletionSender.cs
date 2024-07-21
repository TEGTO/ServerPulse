using MessageBus;
using Shared.Dtos.ServerEvent;
using System.Text.Json;

namespace ServerSlotApi.Services
{
    public class SlotKeyDeletionSender : ISlotKeyDeletionSender
    {
        private readonly IMessageProducer producer;
        private readonly string deletionTopic;
        private readonly int timeoutInMilliseconds;
        private readonly int partitionsAmount;

        public SlotKeyDeletionSender(IMessageProducer producer, IConfiguration configuration)
        {
            deletionTopic = configuration[Configuration.KAFKA_KEY_DELETE_TOPIC]!;
            timeoutInMilliseconds = int.Parse(configuration[Configuration.KAFKA_TIMEOUT_IN_MILLISECONDS]!);
            partitionsAmount = int.Parse(configuration[Configuration.KAFKA_PARTITIONS_AMOUNT]!);
            this.producer = producer;
        }

        public async Task SendDeleteSlotKeyEventAsync(string slotKey, CancellationToken cancellationToken)
        {
            string topic = deletionTopic;
            var deletionEvent = new SlotKeyDeletionEvent(slotKey);
            var message = JsonSerializer.Serialize(deletionEvent);
            await producer.ProduceAsync(topic, message, partitionsAmount, cancellationToken);
        }
    }
}

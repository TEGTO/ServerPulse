using Confluent.Kafka;

namespace MessageBus.Kafka
{
    public interface IKafkaConsumerFactory
    {
        public IConsumer<string, string> CreateConsumer();
    }
}
using Confluent.Kafka;

namespace MessageBus.Interfaces
{
    public interface IKafkaConsumerFactory
    {
        public IConsumer<string, string> CreateConsumer();
    }
}
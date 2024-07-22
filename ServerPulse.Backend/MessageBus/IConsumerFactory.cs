using Confluent.Kafka;

namespace MessageBus
{
    public interface IConsumerFactory
    {
        public IConsumer<string, string> CreateConsumer();
    }
}
using Confluent.Kafka;

namespace MessageBus.Interfaces
{
    public interface IKafkaProducerFactory
    {
        public IProducer<string, string> CreateProducer();
    }
}

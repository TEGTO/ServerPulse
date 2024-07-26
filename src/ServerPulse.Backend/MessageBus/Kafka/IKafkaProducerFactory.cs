using Confluent.Kafka;

namespace MessageBus.Kafka
{
    public interface IKafkaProducerFactory
    {
        public IProducer<string, string> CreateProducer();
    }
}

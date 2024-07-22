using Confluent.Kafka;

namespace MessageBus
{
    public interface IProducerFactory
    {
        public IProducer<string, string> CreateProducer();
    }
}

using Confluent.Kafka;
using MessageBus.Interfaces;

namespace MessageBus.Kafka
{
    public class KafkaProducerFactory : IKafkaProducerFactory
    {
        private readonly ProducerBuilder<string, string> producerBuilder;

        public KafkaProducerFactory(ProducerConfig config)
        {
            producerBuilder = new ProducerBuilder<string, string>(config);
        }

        public IProducer<string, string> CreateProducer()
        {
            return producerBuilder.Build();
        }
    }
}

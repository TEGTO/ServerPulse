using Confluent.Kafka;

namespace MessageBus
{
    public class ProducerFactory : IProducerFactory
    {
        private readonly ProducerBuilder<string, string> producerBuilder;

        public ProducerFactory(ProducerConfig config)
        {
            producerBuilder = new ProducerBuilder<string, string>(config);
        }

        public IProducer<string, string> CreateProducer()
        {
            return producerBuilder.Build();
        }
    }
}

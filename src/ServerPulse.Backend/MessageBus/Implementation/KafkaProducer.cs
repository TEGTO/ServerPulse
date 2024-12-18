using Confluent.Kafka;
using MessageBus.Interfaces;

namespace MessageBus.Kafka
{
    public class KafkaProducer : IMessageProducer
    {
        private readonly IKafkaProducerFactory producerFactory;

        public KafkaProducer(IKafkaProducerFactory producerFactory)
        {
            this.producerFactory = producerFactory;
        }

        public async Task ProduceAsync(string topic, string value, CancellationToken cancellationToken)
        {
            using (var producer = producerFactory.CreateProducer())
            {
                var message = new Message<string, string>
                {
                    Value = value
                };

                await producer.ProduceAsync(topic, message, cancellationToken).ConfigureAwait(false);
            }
        }
    }
}
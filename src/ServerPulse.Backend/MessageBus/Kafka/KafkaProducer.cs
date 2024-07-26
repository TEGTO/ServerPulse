using Confluent.Kafka;

namespace MessageBus.Kafka
{
    public class KafkaProducer : IMessageProducer
    {
        private readonly IKafkaProducerFactory producerFactory;

        public KafkaProducer(IKafkaProducerFactory producerFactory)
        {
            this.producerFactory = producerFactory;
        }

        public async Task ProduceAsync(string topic, string message, int partitionAmount, CancellationToken cancellationToken)
        {
            using (var producer = producerFactory.CreateProducer())
            {
                var kafkaMessage = new Message<string, string>
                {
                    Value = message
                };

                await producer.ProduceAsync(topic, kafkaMessage, cancellationToken);
            }
        }
    }
}
using Confluent.Kafka;

namespace Kafka
{
    public class KafkaProducer : IMessageProducer
    {
        private readonly ProducerConfig producerConfig;
        private readonly ProducerBuilder<string, string> producerBuilder;

        public KafkaProducer(ProducerConfig producerConfig)
        {
            this.producerConfig = producerConfig;
            producerBuilder = new ProducerBuilder<string, string>(producerConfig);
        }

        public async Task ProduceAsync(string topic, string message, int partitionAmount)
        {
            using (var producer = CreateProducer())
            {
                var kafkaMessage = new Message<string, string>
                {
                    Value = message
                };

                var topicPart = new TopicPartition(topic, new Partition(partitionAmount));

                await producer.ProduceAsync(topicPart, kafkaMessage);
            }
        }
        private IProducer<string, string> CreateProducer()
        {
            return producerBuilder.Build();
        }
    }
}
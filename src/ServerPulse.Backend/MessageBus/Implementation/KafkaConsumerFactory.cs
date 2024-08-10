using Confluent.Kafka;
using MessageBus.Interfaces;

namespace MessageBus.Kafka
{
    public class KafkaConsumerFactory : IKafkaConsumerFactory
    {
        private readonly ConsumerBuilder<string, string> consumerBuilder;

        public KafkaConsumerFactory(ConsumerConfig config)
        {
            consumerBuilder = new ConsumerBuilder<string, string>(config);
        }

        public IConsumer<string, string> CreateConsumer()
        {
            return consumerBuilder.Build();
        }
    }
}
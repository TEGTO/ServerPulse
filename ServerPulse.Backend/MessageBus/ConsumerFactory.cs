using Confluent.Kafka;

namespace MessageBus
{
    public class ConsumerFactory : IConsumerFactory
    {
        private readonly ConsumerBuilder<string, string> consumerBuilder;

        public ConsumerFactory(ConsumerConfig config)
        {
            consumerBuilder = new ConsumerBuilder<string, string>(config);
        }

        public IConsumer<string, string> CreateConsumer()
        {
            return consumerBuilder.Build();
        }
    }
}
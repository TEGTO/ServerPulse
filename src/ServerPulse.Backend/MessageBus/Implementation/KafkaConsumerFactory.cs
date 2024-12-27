using Confluent.Kafka;
using MessageBus.Implementation;
using MessageBus.Interfaces;
using Polly;
using Polly.Registry;

namespace MessageBus.Kafka
{
    public class KafkaConsumerFactory : IKafkaConsumerFactory
    {
        private readonly ConsumerBuilder<string, string> consumerBuilder;
        private readonly ResiliencePipeline resiliencePipeline;

        public KafkaConsumerFactory(ConsumerConfig config, ResiliencePipelineProvider<string> resiliencePipelineProvider)
        {
            consumerBuilder = new ConsumerBuilder<string, string>(config);
            resiliencePipeline = resiliencePipelineProvider.GetPipeline(MessageBusConfigurationKeys.MESSAGE_BUS_RESILIENCE_PIPELINE);
        }

        public IConsumer<string, string> CreateConsumer()
        {
            var consumer = consumerBuilder.Build();
            return new ResilienceConsumer(consumer, resiliencePipeline);
        }
    }
}
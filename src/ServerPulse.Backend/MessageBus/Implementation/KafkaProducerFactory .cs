using Confluent.Kafka;
using MessageBus.Implementation;
using MessageBus.Interfaces;
using Polly;
using Polly.Registry;

namespace MessageBus.Kafka
{
    public class KafkaProducerFactory : IKafkaProducerFactory
    {
        private readonly ProducerBuilder<string, string> producerBuilder;
        private readonly ResiliencePipeline resiliencePipeline;

        public KafkaProducerFactory(ProducerConfig config, ResiliencePipelineProvider<string> resiliencePipelineProvider)
        {
            producerBuilder = new ProducerBuilder<string, string>(config);
            resiliencePipeline = resiliencePipelineProvider.GetPipeline(MessageBusConfigurationKeys.MESSAGE_BUS_RESILIENCE_PIPELINE);
        }

        public IProducer<string, string> CreateProducer()
        {
            var producer = producerBuilder.Build();
            return new ResilienceProducer(producer, resiliencePipeline);
        }
    }
}

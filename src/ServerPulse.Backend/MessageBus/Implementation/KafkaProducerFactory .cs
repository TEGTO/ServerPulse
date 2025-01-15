using Castle.DynamicProxy;
using Confluent.Kafka;
using MessageBus.Interfaces;
using Polly;
using Polly.Registry;
using Proxies.Interceptors;

namespace MessageBus.Kafka
{
    public class KafkaProducerFactory : IKafkaProducerFactory
    {
        private readonly ProducerBuilder<string, string> producerBuilder;
        private readonly ResiliencePipeline resiliencePipeline;
        private readonly IProxyGenerator proxyGenerator;

        public KafkaProducerFactory(ProducerConfig config, ResiliencePipelineProvider<string> resiliencePipelineProvider, IProxyGenerator proxyGenerator)
        {
            producerBuilder = new ProducerBuilder<string, string>(config);
            resiliencePipeline = resiliencePipelineProvider.GetPipeline(MessageBusConfigurationKeys.MESSAGE_BUS_RESILIENCE_PIPELINE);
            this.proxyGenerator = proxyGenerator;
        }

        public IProducer<string, string> CreateProducer()
        {
            var producer = producerBuilder.Build();

            return proxyGenerator.CreateInterfaceProxyWithTarget(
                producer,
                new ResilienceInterceptor(resiliencePipeline, interceptAll: true)
            );
        }
    }
}

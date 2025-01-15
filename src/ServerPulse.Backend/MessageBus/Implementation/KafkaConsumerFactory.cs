using Castle.DynamicProxy;
using Confluent.Kafka;
using MessageBus.Interfaces;
using Polly;
using Polly.Registry;
using Proxies.Interceptors;

namespace MessageBus.Kafka
{
    public class KafkaConsumerFactory : IKafkaConsumerFactory
    {
        private readonly ConsumerBuilder<string, string> consumerBuilder;
        private readonly ResiliencePipeline resiliencePipeline;
        private readonly IProxyGenerator proxyGenerator;

        public KafkaConsumerFactory(ConsumerConfig config, ResiliencePipelineProvider<string> resiliencePipelineProvider, IProxyGenerator proxyGenerator)
        {
            consumerBuilder = new ConsumerBuilder<string, string>(config);
            resiliencePipeline = resiliencePipelineProvider.GetPipeline(MessageBusConfigurationKeys.MESSAGE_BUS_RESILIENCE_PIPELINE);
            this.proxyGenerator = proxyGenerator;
        }

        public IConsumer<string, string> CreateConsumer()
        {
            var consumer = consumerBuilder.Build();

            return proxyGenerator.CreateInterfaceProxyWithTarget(
                consumer,
                new ResilienceInterceptor(resiliencePipeline, interceptAll: true)
            );
        }
    }
}
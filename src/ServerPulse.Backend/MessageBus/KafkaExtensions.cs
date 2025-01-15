using Castle.DynamicProxy;
using Confluent.Kafka;
using MessageBus.Interfaces;
using MessageBus.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Polly;
using Resilience;

namespace MessageBus
{
    public static class KafkaExtensions
    {
        public static IServiceCollection AddKafkaConsumer(this IServiceCollection services, ConsumerConfig consumerConfig, AdminClientConfig adminConfig)
        {
            services.AddSingleton(consumerConfig);
            services.AddSingleton(new AdminClientBuilder(adminConfig).Build());
            services.TryAddSingleton<IProxyGenerator, ProxyGenerator>();
            services.AddSingleton<IKafkaConsumerFactory, KafkaConsumerFactory>();
            services.AddSingleton<IMessageConsumer, KafkaConsumer>();
            services.AddMessageBusResilience();
            return services;
        }

        public static IServiceCollection AddKafkaProducer(this IServiceCollection services, ProducerConfig producerConfig, AdminClientConfig adminConfig)
        {
            services.AddSingleton(producerConfig);
            services.AddSingleton(new AdminClientBuilder(adminConfig).Build());
            services.TryAddSingleton<IProxyGenerator, ProxyGenerator>();
            services.AddSingleton<IKafkaProducerFactory, KafkaProducerFactory>();
            services.AddSingleton<IMessageProducer, KafkaProducer>();
            services.AddSingleton<ITopicManager, KafkaTopicManager>();
            services.AddMessageBusResilience();
            return services;
        }

        private static IServiceCollection AddMessageBusResilience(this IServiceCollection services)
        {
            var serviceProvider = services.BuildServiceProvider();
            var configuration = serviceProvider.GetRequiredService<IConfiguration>();

            var pipelineConfiguration = configuration
                .GetSection(MessageBusConfigurationKeys.MESSAGE_BUS_RESILIENCE_PIPELINE)?
                .Get<ResiliencePipelineSettings>() ?? new ResiliencePipelineSettings();

            services.AddResiliencePipeline(MessageBusConfigurationKeys.MESSAGE_BUS_RESILIENCE_PIPELINE, (builder, context) =>
            {
                ResilienceHelpers.ConfigureResiliencePipeline(builder, context, pipelineConfiguration, false);
            });

            return services;
        }
    }
}
using Confluent.Kafka;
using MessageBus.Kafka;
using Microsoft.Extensions.DependencyInjection;
using TestKafka.Consumer.Services;

namespace MessageBus
{
    public static class KafkaExtenstions
    {

        public static IServiceCollection AddKafkaConsumer(this IServiceCollection services, ConsumerConfig consumerConfig, AdminClientConfig adminConfig)
        {
            services.AddSingleton(consumerConfig);
            services.AddSingleton(new AdminClientBuilder(adminConfig).Build());
            services.AddSingleton<IKafkaConsumerFactory, KafkaConsumerFactory>();
            services.AddSingleton<IMessageConsumer, KafkaConsumer>();
            return services;
        }
        public static IServiceCollection AddKafkaProducer(this IServiceCollection services, ProducerConfig producerConfig, AdminClientConfig adminConfig)
        {
            services.AddSingleton(producerConfig);
            services.AddSingleton(new AdminClientBuilder(adminConfig).Build());
            services.AddSingleton<IKafkaProducerFactory, KafkaProducerFactory>();
            services.AddSingleton<IMessageProducer, KafkaProducer>();
            services.AddSingleton<ITopicManager, KafkaTopicManager>();
            return services;
        }
    }
}
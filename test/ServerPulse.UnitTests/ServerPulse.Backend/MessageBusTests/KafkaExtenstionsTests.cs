using Confluent.Kafka;
using MessageBus.Interfaces;
using MessageBus.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Polly.Registry;

namespace MessageBus.Tests
{
    [TestFixture]
    internal class KafkaExtensionsTests
    {
        [Test]
        public void AddKafkaConsumer_ShouldRegisterKafkaConsumerServices()
        {
            // Arrange
            var services = new ServiceCollection();
            var consumerConfig = new Mock<ConsumerConfig>().Object;
            var adminConfig = new Mock<AdminClientConfig>().Object;

            var mockConfiguration = new Mock<IConfiguration>();
            var mockResiliencePipelineProvider = new Mock<ResiliencePipelineProvider<string>>();

            services.AddSingleton(mockConfiguration.Object);
            services.AddSingleton(mockResiliencePipelineProvider.Object);

            // Act
            services.AddKafkaConsumer(consumerConfig, adminConfig);
            var serviceProvider = services.BuildServiceProvider();

            // Assert
            var registeredConsumerConfig = serviceProvider.GetService<ConsumerConfig>();
            Assert.IsNotNull(registeredConsumerConfig);
            Assert.IsInstanceOf<ConsumerConfig>(registeredConsumerConfig);

            var adminClient = serviceProvider.GetService<IAdminClient>();
            Assert.IsNotNull(adminClient);
            Assert.IsInstanceOf<IAdminClient>(adminClient);

            var kafkaConsumerFactory = serviceProvider.GetService<IKafkaConsumerFactory>();
            Assert.IsNotNull(kafkaConsumerFactory);
            Assert.IsInstanceOf<KafkaConsumerFactory>(kafkaConsumerFactory);

            var messageConsumer = serviceProvider.GetService<IMessageConsumer>();
            Assert.IsNotNull(messageConsumer);
            Assert.IsInstanceOf<KafkaConsumer>(messageConsumer);
        }

        [Test]
        public void AddKafkaProducer_ShouldRegisterKafkaProducerServices()
        {
            // Arrange
            var services = new ServiceCollection();
            var producerConfig = new Mock<ProducerConfig>().Object;
            var adminConfig = new Mock<AdminClientConfig>().Object;

            var mockConfiguration = new Mock<IConfiguration>();
            var mockResiliencePipelineProvider = new Mock<ResiliencePipelineProvider<string>>();

            services.AddSingleton(mockConfiguration.Object);
            services.AddSingleton(mockResiliencePipelineProvider.Object);

            // Act
            services.AddKafkaProducer(producerConfig, adminConfig);
            var serviceProvider = services.BuildServiceProvider();

            // Assert
            var registeredProducerConfig = serviceProvider.GetService<ProducerConfig>();
            Assert.IsNotNull(registeredProducerConfig);
            Assert.IsInstanceOf<ProducerConfig>(registeredProducerConfig);

            var adminClient = serviceProvider.GetService<IAdminClient>();
            Assert.IsNotNull(adminClient);
            Assert.IsInstanceOf<IAdminClient>(adminClient);

            var kafkaProducerFactory = serviceProvider.GetService<IKafkaProducerFactory>();
            Assert.IsNotNull(kafkaProducerFactory);
            Assert.IsInstanceOf<KafkaProducerFactory>(kafkaProducerFactory);

            var messageProducer = serviceProvider.GetService<IMessageProducer>();
            Assert.IsNotNull(messageProducer);
            Assert.IsInstanceOf<KafkaProducer>(messageProducer);

            var topicManager = serviceProvider.GetService<ITopicManager>();
            Assert.IsNotNull(topicManager);
            Assert.IsInstanceOf<KafkaTopicManager>(topicManager);
        }
    }
}
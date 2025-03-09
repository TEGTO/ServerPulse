using Castle.DynamicProxy;
using Confluent.Kafka;
using MessageBus;
using MessageBus.Kafka;
using Moq;
using Polly;
using Polly.Registry;

namespace MessageBusTests.Implementation
{
    [TestFixture]
    internal class KafkaConsumerFactoryTests
    {
        private Mock<ResiliencePipelineProvider<string>> mockResiliencePipelineProvider;
        private Mock<IProxyGenerator> mockProxyGenerator;
        private Mock<IConsumer<string, string>> mockConsumer;
        private ConsumerConfig config;
        private KafkaConsumerFactory factory;

        [SetUp]
        public void SetUp()
        {
            config = new ConsumerConfig
            {
                BootstrapServers = "localhost:9092",
                GroupId = "test-group"
            };

            mockResiliencePipelineProvider = new Mock<ResiliencePipelineProvider<string>>();
            mockProxyGenerator = new Mock<IProxyGenerator>();
            mockConsumer = new Mock<IConsumer<string, string>>();

            mockResiliencePipelineProvider
                .Setup(rp => rp.GetPipeline(ConfigurationKeys.MESSAGE_BUS_RESILIENCE_PIPELINE))
                .Returns(ResiliencePipeline.Empty);
            mockProxyGenerator
                .Setup(rp => rp.CreateInterfaceProxyWithTarget(
                    It.IsAny<IConsumer<string, string>>(),
                    It.IsAny<IInterceptor[]>()))
                .Returns(mockConsumer.Object);

            factory = new KafkaConsumerFactory(config, mockResiliencePipelineProvider.Object, mockProxyGenerator.Object);
        }

        [Test]
        public void CreateConsumer_ReturnsConsumerWithResilience()
        {
            // Act
            var consumer = factory.CreateConsumer();

            // Assert
            Assert.IsNotNull(consumer);
            Assert.That(consumer, Is.EqualTo(mockConsumer.Object));

            mockResiliencePipelineProvider.Verify(rp => rp.GetPipeline(ConfigurationKeys.MESSAGE_BUS_RESILIENCE_PIPELINE), Times.Once);
            mockProxyGenerator.Verify(rp => rp.CreateInterfaceProxyWithTarget(
               It.IsAny<IConsumer<string, string>>(),
               It.IsAny<IInterceptor[]>()),
               Times.Once
            );
        }
    }
}
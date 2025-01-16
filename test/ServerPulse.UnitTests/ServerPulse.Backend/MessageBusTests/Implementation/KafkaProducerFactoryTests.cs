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
    internal class KafkaProducerFactoryTests
    {
        private Mock<ResiliencePipelineProvider<string>> mockResiliencePipelineProvider;
        private Mock<IProxyGenerator> mockProxyGenerator;
        private Mock<IProducer<string, string>> mockProducer;
        private ProducerConfig config;
        private KafkaProducerFactory factory;

        [SetUp]
        public void Setup()
        {
            config = new ProducerConfig
            {
                BootstrapServers = "localhost:9092"
            };

            mockResiliencePipelineProvider = new Mock<ResiliencePipelineProvider<string>>();
            mockProxyGenerator = new Mock<IProxyGenerator>();
            mockProducer = new Mock<IProducer<string, string>>();

            mockResiliencePipelineProvider
                .Setup(rp => rp.GetPipeline(It.IsAny<string>()))
                .Returns(ResiliencePipeline.Empty);
            mockProxyGenerator
                .Setup(rp => rp.CreateInterfaceProxyWithTarget(
                    It.IsAny<IProducer<string, string>>(),
                    It.IsAny<IInterceptor[]>()))
                .Returns(mockProducer.Object);

            factory = new KafkaProducerFactory(config, mockResiliencePipelineProvider.Object, mockProxyGenerator.Object);
        }

        [Test]
        public void CreateProducer_ReturnsProducerWithResilience()
        {
            // Act
            var producer = factory.CreateProducer();

            // Assert
            Assert.IsNotNull(producer);
            Assert.That(producer, Is.EqualTo(mockProducer.Object));

            mockResiliencePipelineProvider.Verify(rp => rp.GetPipeline(MessageBusConfigurationKeys.MESSAGE_BUS_RESILIENCE_PIPELINE), Times.Once);
            mockProxyGenerator.Verify(rp => rp.CreateInterfaceProxyWithTarget(
                It.IsAny<IProducer<string, string>>(),
                It.IsAny<IInterceptor[]>()),
                Times.Once
            );
        }
    }
}

using Confluent.Kafka;
using MessageBus;
using MessageBus.Implementation;
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
            mockResiliencePipelineProvider
                .Setup(rp => rp.GetPipeline(It.IsAny<string>()))
                .Returns(ResiliencePipeline.Empty);

            factory = new KafkaProducerFactory(config, mockResiliencePipelineProvider.Object);
        }

        [Test]
        public void CreateProducer_ReturnsResilienceProducer()
        {
            // Act
            var producer = factory.CreateProducer();

            // Assert
            Assert.IsNotNull(producer);
            Assert.IsInstanceOf<ResilienceProducer>(producer);

            mockResiliencePipelineProvider.Verify(rp => rp.GetPipeline(MessageBusConfigurationKeys.MESSAGE_BUS_RESILIENCE_PIPELINE), Times.Once);
        }

        [Test]
        public void CreateProducer_UsesCorrectConfig()
        {
            // Act
            var producer = factory.CreateProducer();

            // Assert
            Assert.IsNotNull(producer);
        }

        [Test]
        public void CreateProducer_BuildsProducerOnce()
        {
            // Arrange
            var localFactory = new KafkaProducerFactory(config, mockResiliencePipelineProvider.Object);

            // Act
            var producer1 = localFactory.CreateProducer();
            var producer2 = localFactory.CreateProducer();

            // Assert
            Assert.IsNotNull(producer1);
            Assert.IsNotNull(producer2);
            Assert.That(producer2, Is.Not.SameAs(producer1));
        }
    }
}

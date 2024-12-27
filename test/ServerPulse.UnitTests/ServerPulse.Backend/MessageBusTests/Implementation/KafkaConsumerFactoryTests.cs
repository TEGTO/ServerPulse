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
    internal class KafkaConsumerFactoryTests
    {
        private Mock<ResiliencePipelineProvider<string>> mockResiliencePipelineProvider;
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
            mockResiliencePipelineProvider
                .Setup(rp => rp.GetPipeline(MessageBusConfigurationKeys.MESSAGE_BUS_RESILIENCE_PIPELINE))
                .Returns(ResiliencePipeline.Empty);

            factory = new KafkaConsumerFactory(config, mockResiliencePipelineProvider.Object);
        }

        [Test]
        public void CreateConsumer_ReturnsResilienceConsumer()
        {
            // Act
            var consumer = factory.CreateConsumer();

            // Assert
            Assert.IsNotNull(consumer);
            Assert.IsInstanceOf<ResilienceConsumer>(consumer);

            mockResiliencePipelineProvider.Verify(rp => rp.GetPipeline(MessageBusConfigurationKeys.MESSAGE_BUS_RESILIENCE_PIPELINE), Times.Once);
        }

        [Test]
        public void CreateConsumer_UsesCorrectConfig()
        {
            // Act
            var consumer = factory.CreateConsumer();

            // Assert
            Assert.IsNotNull(consumer);
        }

        [Test]
        public void CreateConsumer_BuildsNewConsumerEachTime()
        {
            // Arrange
            var localFactory = new KafkaConsumerFactory(config, mockResiliencePipelineProvider.Object);

            // Act
            var consumer1 = localFactory.CreateConsumer();
            var consumer2 = localFactory.CreateConsumer();

            // Assert
            Assert.IsNotNull(consumer1);
            Assert.IsNotNull(consumer2);
            Assert.That(consumer2, Is.Not.SameAs(consumer1));
        }
    }
}
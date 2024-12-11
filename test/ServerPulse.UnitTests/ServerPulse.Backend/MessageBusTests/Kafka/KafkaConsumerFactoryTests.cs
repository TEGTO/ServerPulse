using Confluent.Kafka;
using MessageBus.Kafka;
using Moq;

namespace MessageBusTests.Kafka
{
    [TestFixture]
    internal class KafkaConsumerFactoryTests
    {
        private Mock<ConsumerBuilder<string, string>> mockConsumerBuilder;
        private ConsumerConfig config;
        private KafkaConsumerFactory factory;

        [SetUp]
        public void Setup()
        {
            config = new ConsumerConfig
            {
                BootstrapServers = "localhost:9092",
                GroupId = "test-group",
                AutoOffsetReset = AutoOffsetReset.Earliest
            };
            mockConsumerBuilder = new Mock<ConsumerBuilder<string, string>>(config);
            var mockConsumer = new Mock<IConsumer<string, string>>();
            mockConsumerBuilder
                .Setup(cb => cb.Build())
                .Returns(mockConsumer.Object);
            factory = new KafkaConsumerFactory(config);
        }

        [Test]
        public void CreateConsumer_ReturnsConsumer()
        {
            // Act
            var consumer = factory.CreateConsumer();
            // Assert
            Assert.IsNotNull(consumer);
            Assert.IsInstanceOf<IConsumer<string, string>>(consumer);
        }
    }
}
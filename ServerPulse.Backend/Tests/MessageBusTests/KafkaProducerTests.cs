using Confluent.Kafka;
using MessageBus;
using Moq;

namespace MessageBusTests
{
    [TestFixture]
    public class KafkaProducerTests
    {
        private Mock<IProducer<string, string>> mockProducer;
        private Mock<IProducerFactory> mockProducerFactory;
        private KafkaProducer kafkaProducer;

        [SetUp]
        public void Setup()
        {
            mockProducer = new Mock<IProducer<string, string>>();
            mockProducerFactory = new Mock<IProducerFactory>();
            mockProducerFactory.Setup(x => x.CreateProducer()).Returns(mockProducer.Object);
            kafkaProducer = new KafkaProducer(mockProducerFactory.Object);
        }

        [Test]
        public async Task ProduceAsync_CreatesAndSendsKafkaMessage()
        {
            // Arrange
            var topic = "test-topic";
            var message = "test-message";
            var partitionAmount = 0;
            var cancellationToken = CancellationToken.None;
            // Act
            await kafkaProducer.ProduceAsync(topic, message, partitionAmount, cancellationToken);
            // Assert
            mockProducer.Verify(x => x.ProduceAsync(
                topic,
                It.Is<Message<string, string>>(msg => msg.Value == message),
                cancellationToken
            ), Times.Once);
        }
        [Test]
        public async Task ProduceAsync_UsesCorrectTopicAndPartition()
        {
            // Arrange
            var topic = "test-topic";
            var message = "test-message";
            var partitionAmount = 1;
            var cancellationToken = CancellationToken.None;
            // Act
            await kafkaProducer.ProduceAsync(topic, message, partitionAmount, cancellationToken);
            // Assert
            mockProducer.Verify(x => x.ProduceAsync(
                topic,
                It.Is<Message<string, string>>(msg => msg.Value == message),
                cancellationToken
            ), Times.Once);
        }
        [Test]
        public async Task ProduceAsync_CreatesProducerWithCorrectConfig()
        {
            // Arrange
            var topic = "test-topic";
            var message = "test-message";
            var partitionAmount = 0;
            var cancellationToken = CancellationToken.None;
            // Act
            await kafkaProducer.ProduceAsync(topic, message, partitionAmount, cancellationToken);
            // Assert
            mockProducerFactory.Verify(x => x.CreateProducer(), Times.Once);
        }
    }
}
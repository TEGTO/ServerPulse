using Confluent.Kafka;
using MessageBus.Interfaces;
using MessageBus.Kafka;
using Moq;

namespace MessageBusTests.Implementation
{
    [TestFixture]
    internal class KafkaProducerTests
    {
        private Mock<IProducer<string, string>> mockProducer;
        private Mock<IKafkaProducerFactory> mockProducerFactory;
        private KafkaProducer producer;

        [SetUp]
        public void Setup()
        {
            mockProducer = new Mock<IProducer<string, string>>();
            mockProducerFactory = new Mock<IKafkaProducerFactory>();

            mockProducerFactory.Setup(x => x.CreateProducer()).Returns(mockProducer.Object);

            producer = new KafkaProducer(mockProducerFactory.Object);
        }

        private static IEnumerable<TestCaseData> ProduceAsyncTestCases()
        {
            yield return new TestCaseData("test-topic-1", "test-message-1", CancellationToken.None)
                .SetDescription("Validates message is produced to the correct topic with valid message 1.");
            yield return new TestCaseData("test-topic-2", "test-message-2", new CancellationTokenSource().Token)
                .SetDescription("Validates message is produced to the correct topic with valid message 2 and cancellation token.");
        }

        [Test]
        [TestCaseSource(nameof(ProduceAsyncTestCases))]
        public async Task ProduceAsync_CreatesAndSendsKafkaMessage(string topic, string message, CancellationToken cancellationToken)
        {
            // Act
            await producer.ProduceAsync(topic, message, cancellationToken);

            // Assert
            mockProducer.Verify(x => x.ProduceAsync(
                topic,
                It.Is<Message<string, string>>(msg => msg.Value == message),
                cancellationToken
            ), Times.Once);
        }

        [Test]
        public async Task ProduceAsync_CreatesProducerOnce()
        {
            // Arrange
            var topic = "test-topic";
            var message = "test-message";
            var cancellationToken = CancellationToken.None;

            // Act
            await producer.ProduceAsync(topic, message, cancellationToken);

            // Assert
            mockProducerFactory.Verify(x => x.CreateProducer(), Times.Once);
        }

        [TestCase("test-topic-1", "test-message-1")]
        [TestCase("test-topic-2", "test-message-2")]
        public async Task ProduceAsync_UsesCorrectTopicAndValue(string topic, string message)
        {
            // Arrange
            var cancellationToken = CancellationToken.None;

            // Act
            await producer.ProduceAsync(topic, message, cancellationToken);

            // Assert
            mockProducer.Verify(x => x.ProduceAsync(
                topic,
                It.Is<Message<string, string>>(msg => msg.Value == message),
                cancellationToken
            ), Times.Once);
        }
    }
}
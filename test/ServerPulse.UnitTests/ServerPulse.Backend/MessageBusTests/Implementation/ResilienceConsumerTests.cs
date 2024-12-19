using Confluent.Kafka;
using Moq;
using Polly;

namespace MessageBus.Implementation.Tests
{
    [TestFixture]
    internal class ResilienceConsumerTests
    {
        private Mock<IConsumer<string, string>> mockConsumer;
        private ResilienceConsumer resilienceConsumer;

        [SetUp]
        public void SetUp()
        {
            mockConsumer = new Mock<IConsumer<string, string>>();

            resilienceConsumer = new ResilienceConsumer(mockConsumer.Object, ResiliencePipeline.Empty);
        }

        [TearDown]
        public void TearDown()
        {
            resilienceConsumer.Dispose();
        }

        [Test]
        public void Subscribe_CallsUnderlyingConsumer()
        {
            // Arrange
            var topics = new List<string> { "topic1", "topic2" };

            // Act
            resilienceConsumer.Subscribe(topics);

            // Assert
            mockConsumer.Verify(c => c.Subscribe(topics), Times.Once);
        }

        [Test]
        public void Unsubscribe_CallsUnderlyingConsumer()
        {
            // Act
            resilienceConsumer.Unsubscribe();

            // Assert
            mockConsumer.Verify(c => c.Unsubscribe(), Times.Once);
        }

        [Test]
        public void Consume_WithTimeout_CallsUnderlyingConsumer()
        {
            // Arrange
            var timeout = TimeSpan.FromSeconds(1);

            // Act
            resilienceConsumer.Consume(timeout);

            // Assert
            mockConsumer.Verify(c => c.Consume(timeout), Times.Once);
        }

        [Test]
        public void Consume_WithCancellationToken_CallsUnderlyingConsumer()
        {
            // Arrange
            var cancellationToken = new CancellationToken();

            // Act
            resilienceConsumer.Consume(cancellationToken);

            // Assert
            mockConsumer.Verify(c => c.Consume(cancellationToken), Times.Once);
        }

        [Test]
        public void GetWatermarkOffsets_CallsUnderlyingConsumer()
        {
            // Arrange
            var topicPartition = new TopicPartition("topic", 0);

            // Act
            resilienceConsumer.GetWatermarkOffsets(topicPartition);

            // Assert
            mockConsumer.Verify(c => c.GetWatermarkOffsets(topicPartition), Times.Once);
        }

        [Test]
        public void Dispose_CallsUnderlyingConsumer()
        {
            // Act
            resilienceConsumer.Dispose();

            // Assert
            mockConsumer.Verify(c => c.Dispose(), Times.Once);
        }
    }
}
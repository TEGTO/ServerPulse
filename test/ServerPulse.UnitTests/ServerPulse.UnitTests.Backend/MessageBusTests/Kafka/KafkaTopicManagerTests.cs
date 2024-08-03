using Confluent.Kafka;
using MessageBus.Kafka;
using Moq;

namespace MessageBusTests.Kafka
{
    [TestFixture]
    internal class KafkaTopicManagerTests
    {
        private Mock<IAdminClient> mockAdminClient;
        private KafkaTopicManager kafkaTopicManager;

        [SetUp]
        public void Setup()
        {
            mockAdminClient = new Mock<IAdminClient>();
            kafkaTopicManager = new KafkaTopicManager(mockAdminClient.Object);
        }

        [Test]
        public async Task DeleteTopicsAsync_CallsDeleteTopicsAsync()
        {
            // Arrange
            var topics = new List<string> { "topic1", "topic2" };
            // Act
            await kafkaTopicManager.DeleteTopicsAsync(topics);
            // Assert
            mockAdminClient.Verify(x => x.DeleteTopicsAsync(topics, null), Times.Once);
        }
        [Test]
        public async Task DeleteTopicsAsync_HandlesEmptyList()
        {
            // Arrange
            var topics = new List<string>();
            // Act
            await kafkaTopicManager.DeleteTopicsAsync(topics);
            // Assert
            mockAdminClient.Verify(x => x.DeleteTopicsAsync(topics, null), Times.Once);
        }
    }
}

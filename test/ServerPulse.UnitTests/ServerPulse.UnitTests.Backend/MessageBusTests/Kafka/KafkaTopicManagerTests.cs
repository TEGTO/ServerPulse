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
        public async Task DeleteTopicsAsync_CallsDeleteTopicsAsync_ForExistingTopics()
        {
            // Arrange
            var existingTopics = new List<string> { "topic1", "topic2", "topic3" };
            var topicsToDelete = new List<string> { "topic1", "topic3" };
            mockAdminClient.Setup(x => x.GetMetadata(It.IsAny<TimeSpan>()))
                .Returns(new Metadata(
                    new List<BrokerMetadata>(),
                    existingTopics.Select(t => new TopicMetadata(t, new List<PartitionMetadata>(), null)).ToList(),
                    0,
                    "cluster-id"
                ));
            // Act
            await kafkaTopicManager.DeleteTopicsAsync(topicsToDelete);
            // Assert
            mockAdminClient.Verify(x => x.DeleteTopicsAsync(It.Is<IEnumerable<string>>(topics => topics.SequenceEqual(new[] { "topic1", "topic3" })), null), Times.Once);
        }
        [Test]
        public async Task DeleteTopicsAsync_DoesNotCallDeleteTopicsAsync_ForNonExistentTopics()
        {
            // Arrange
            var existingTopics = new List<string> { "topic1", "topic2" };
            var topicsToDelete = new List<string> { "topic3", "topic4" };
            mockAdminClient.Setup(x => x.GetMetadata(It.IsAny<TimeSpan>()))
                .Returns(new Metadata(
                    new List<BrokerMetadata>(),
                    existingTopics.Select(t => new TopicMetadata(t, new List<PartitionMetadata>(), null)).ToList(),
                    0,
                    "cluster-id"
                ));
            // Act
            await kafkaTopicManager.DeleteTopicsAsync(topicsToDelete);
            // Assert
            mockAdminClient.Verify(x => x.DeleteTopicsAsync(It.IsAny<IEnumerable<string>>(), null), Times.Never);
        }
        [Test]
        public async Task DeleteTopicsAsync_HandlesMixedExistingAndNonExistentTopics()
        {
            // Arrange
            var existingTopics = new List<string> { "topic1", "topic2" };
            var topicsToDelete = new List<string> { "topic1", "topic3" };
            mockAdminClient.Setup(x => x.GetMetadata(It.IsAny<TimeSpan>()))
                .Returns(new Metadata(
                    new List<BrokerMetadata>(),
                    existingTopics.Select(t => new TopicMetadata(t, new List<PartitionMetadata>(), null)).ToList(),
                    0,
                    "cluster-id"
                ));
            // Act
            await kafkaTopicManager.DeleteTopicsAsync(topicsToDelete);
            // Assert
            mockAdminClient.Verify(x => x.DeleteTopicsAsync(It.Is<IEnumerable<string>>(topics => topics.SequenceEqual(new[] { "topic1" })), null), Times.Once);
        }
        [Test]
        public async Task DeleteTopicsAsync_DoesNotCallDeleteTopicsAsync_WhenTopicListIsEmpty()
        {
            // Arrange
            var existingTopics = new List<string> { "topic1", "topic2" };
            var topicsToDelete = new List<string>();
            mockAdminClient.Setup(x => x.GetMetadata(It.IsAny<TimeSpan>()))
               .Returns(new Metadata(
                   new List<BrokerMetadata>(),
                   existingTopics.Select(t => new TopicMetadata(t, new List<PartitionMetadata>(), null)).ToList(),
                   0,
                   "cluster-id"
               ));
            // Act
            await kafkaTopicManager.DeleteTopicsAsync(topicsToDelete);
            // Assert
            mockAdminClient.Verify(x => x.DeleteTopicsAsync(It.IsAny<IEnumerable<string>>(), null), Times.Never);
        }
        [Test]
        public async Task DeleteTopicsAsync_CallsGetExistingTopicsAsync()
        {
            // Arrange
            var topicsToDelete = new List<string> { "topic1" };
            mockAdminClient.Setup(x => x.GetMetadata(It.IsAny<TimeSpan>()))
                .Returns(new Metadata(
                    new List<BrokerMetadata>(),
                    new List<TopicMetadata> { new TopicMetadata("topic1", new List<PartitionMetadata>(), null) },
                    0,
                    "cluster-id"
                ));
            // Act
            await kafkaTopicManager.DeleteTopicsAsync(topicsToDelete);

            // Assert
            mockAdminClient.Verify(x => x.GetMetadata(It.IsAny<TimeSpan>()), Times.Once);
        }
    }
}
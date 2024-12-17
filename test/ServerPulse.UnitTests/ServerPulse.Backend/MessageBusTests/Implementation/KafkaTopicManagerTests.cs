using Confluent.Kafka;
using Confluent.Kafka.Admin;
using MessageBus.Kafka;
using Moq;

namespace MessageBusTests.Implementation
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

        private static IEnumerable<TestCaseData> CreateTopicsTestCases()
        {
            yield return new TestCaseData(
                new List<string> { "new-topic1", "new-topic2" },
                new List<string> { "existing-topic1" },
                new List<TopicSpecification>
                {
                    new TopicSpecification { Name = "new-topic1", NumPartitions = 3, ReplicationFactor = 1 },
                    new TopicSpecification { Name = "new-topic2", NumPartitions = 3, ReplicationFactor = 1 }
                }
            ).SetDescription("Creates topics that do not already exist.");

            yield return new TestCaseData(
                new List<string> { "existing-topic1", "existing-topic2" },
                new List<string> { "existing-topic1", "existing-topic2" },
                new List<TopicSpecification>()
            ).SetDescription("Does not attempt to create topics that already exist.");

            yield return new TestCaseData(
                new List<string>(),
                new List<string> { "existing-topic1" },
                new List<TopicSpecification>()
            ).SetDescription("Handles empty topic list gracefully.");
        }

        [Test]
        [TestCaseSource(nameof(CreateTopicsTestCases))]
        public async Task CreateTopicsAsync_ValidatesTopicCreation(
            List<string> topicList,
            List<string> existingTopics,
            List<TopicSpecification> expectedTopicsToCreate)
        {
            // Arrange
            mockAdminClient.Setup(x => x.GetMetadata(It.IsAny<TimeSpan>()))
                .Returns(new Metadata(new List<BrokerMetadata>(), existingTopics.Select(t => new TopicMetadata(t, new List<PartitionMetadata>(), null))
                .ToList(), 0, "cluster-id"));

            // Act
            await kafkaTopicManager.CreateTopicsAsync(topicList, 3, 1);

            // Assert
            if (expectedTopicsToCreate.Any())
            {
                mockAdminClient.Verify(x => x.CreateTopicsAsync(
                    It.IsAny<IEnumerable<TopicSpecification>>(),
                    It.IsAny<CreateTopicsOptions>()
                ), Times.Once);
            }
            else
            {
                mockAdminClient.Verify(x => x.CreateTopicsAsync(It.IsAny<IEnumerable<TopicSpecification>>(), It.IsAny<CreateTopicsOptions>()), Times.Never);
            }
        }

        [Test]
        public async Task CreateTopicsAsync_DoesNotCallCreateTopicsAsync_WhenAllTopicsExist()
        {
            // Arrange
            var topicList = new List<string> { "existing-topic1", "existing-topic2" };
            var existingTopics = new List<string> { "existing-topic1", "existing-topic2" };

            mockAdminClient.Setup(x => x.GetMetadata(It.IsAny<TimeSpan>()))
                .Returns(new Metadata(new List<BrokerMetadata>(), existingTopics.Select(t => new TopicMetadata(t, new List<PartitionMetadata>(), null))
                .ToList(), 0, "cluster-id"));

            // Act
            await kafkaTopicManager.CreateTopicsAsync(topicList, 3, 1);

            // Assert
            mockAdminClient.Verify(x => x.CreateTopicsAsync(It.IsAny<IEnumerable<TopicSpecification>>(), It.IsAny<CreateTopicsOptions>()), Times.Never);
        }

        private static IEnumerable<TestCaseData> DeleteTopicsTestCases()
        {
            yield return new TestCaseData(
                new List<string> { "delete-topic1", "delete-topic2" },
                new List<string> { "delete-topic1", "existing-topic" },
                new List<string> { "delete-topic1" }
            ).SetDescription("Deletes topics that exist and ignores non-existing topics.");

            yield return new TestCaseData(
                new List<string> { "non-existing-topic1", "non-existing-topic2" },
                new List<string> { "existing-topic" },
                new List<string>()
            ).SetDescription("Does not attempt to delete non-existing topics.");

            yield return new TestCaseData(
                new List<string>(),
                new List<string> { "existing-topic" },
                new List<string>()
            ).SetDescription("Handles empty delete list gracefully.");
        }

        [Test]
        [TestCaseSource(nameof(DeleteTopicsTestCases))]
        public async Task DeleteTopicsAsync_ValidatesTopicDeletion(
            List<string> topicsToDelete,
            List<string> existingTopics,
            List<string> expectedTopicsToDelete)
        {
            // Arrange
            mockAdminClient.Setup(x => x.GetMetadata(It.IsAny<TimeSpan>()))
                .Returns(new Metadata(new List<BrokerMetadata>(), existingTopics.Select(t => new TopicMetadata(t, new List<PartitionMetadata>(), null))
                .ToList(), 0, "cluster-id"));

            // Act
            await kafkaTopicManager.DeleteTopicsAsync(topicsToDelete);

            // Assert
            if (expectedTopicsToDelete.Any())
            {
                mockAdminClient.Verify(
                    x => x.DeleteTopicsAsync(It.Is<IEnumerable<string>>(topics => topics.SequenceEqual(expectedTopicsToDelete)), null),
                    Times.Once);
            }
            else
            {
                mockAdminClient.Verify(x => x.DeleteTopicsAsync(It.IsAny<IEnumerable<string>>(), null), Times.Never);
            }
        }
    }
}
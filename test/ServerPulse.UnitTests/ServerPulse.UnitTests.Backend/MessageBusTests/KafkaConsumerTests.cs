using Confluent.Kafka;
using MessageBus.Interfaces;
using MessageBus.Kafka;
using Moq;

namespace MessageBusTests
{
    [TestFixture]
    internal class KafkaConsumerTests
    {
        private Mock<IAdminClient> mockAdminClient;
        private Mock<IConsumer<string, string>> mockConsumer;
        private Mock<IKafkaConsumerFactory> mockConsumerFactory;
        private KafkaConsumer kafkaConsumer;

        [SetUp]
        public void Setup()
        {
            mockAdminClient = new Mock<IAdminClient>();
            mockConsumer = new Mock<IConsumer<string, string>>();
            mockConsumerFactory = new Mock<IKafkaConsumerFactory>();
            mockConsumerFactory.Setup(x => x.CreateConsumer()).Returns(mockConsumer.Object);
            kafkaConsumer = new KafkaConsumer(mockAdminClient.Object, mockConsumerFactory.Object);
        }
        [Test]
        public async Task ConsumeAsync_ConsumesMessages()
        {
            // Arrange
            var topic = "test-topic";
            var timeoutInMilliseconds = 1000;
            var consumeFrom = Offset.Beginning;
            var cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = cancellationTokenSource.Token;
            var messages = new List<string> { "message1", "message2" };
            var messageIndex = 0;
            mockAdminClient.Setup(x => x.GetMetadata(topic, It.IsAny<TimeSpan>())).Returns(new Metadata(
                new List<BrokerMetadata>(),
                new List<TopicMetadata> { new TopicMetadata(topic, new List<PartitionMetadata>(), null) },
                0,
                "cluster-id"
            ));
            mockConsumer.Setup(x => x.Consume(It.IsAny<TimeSpan>()))
                .Returns(() =>
                {
                    if (messageIndex < messages.Count)
                    {
                        return new ConsumeResult<string, string>
                        {
                            Message = new Message<string, string> { Value = messages[messageIndex++] }
                        };
                    }
                    return null;
                });
            var partitions = new List<TopicPartitionOffset> { new TopicPartitionOffset(topic, 0, consumeFrom) };
            mockConsumer.Setup(x => x.Assign(It.Is<List<TopicPartitionOffset>>(l => l.SequenceEqual(partitions))));
            // Act
            var resultMessages = new List<string>();
            await foreach (var message in kafkaConsumer.ConsumeAsync(topic, timeoutInMilliseconds, consumeFrom, cancellationToken))
            {
                resultMessages.Add(message);
                if (resultMessages.Count >= messages.Count)
                {
                    cancellationTokenSource.Cancel();
                }
            }
            // Assert
            Assert.That(resultMessages, Is.EqualTo(messages));
        }
        private static async Task<List<string>> ConsumeMessagesAsync(IAsyncEnumerable<string> asyncEnumerable, CancellationToken cancellationToken)
        {
            var result = new List<string>();
            await foreach (var item in asyncEnumerable.WithCancellation(cancellationToken))
            {
                result.Add(item);
            }
            return result;
        }
        [Test]
        public async Task ReadLastTopicMessageAsync_ReturnsLastMessage()
        {
            // Arrange
            var topic = "test-topic";
            var timeoutInMilliseconds = 2000;
            var cancellationToken = CancellationToken.None;
            var partitions = new List<PartitionMetadata>
            {
                new PartitionMetadata(0, 0, new int[0], new int[0], null)
            };
            var topicMetadata = new TopicMetadata(topic, partitions, null);

            mockAdminClient.Setup(x => x.GetMetadata(topic, It.IsAny<TimeSpan>())).Returns(new Metadata(
                new List<BrokerMetadata>(),
                new List<TopicMetadata> { topicMetadata },
                0,
                "cluster-id"
            ));
            mockConsumer.Setup(x => x.Subscribe(topic));
            mockConsumer.Setup(x => x.QueryWatermarkOffsets(It.IsAny<TopicPartition>(), It.IsAny<TimeSpan>()))
                .Returns(new WatermarkOffsets(0, 1));
            mockConsumer.Setup(x => x.Consume(It.IsAny<TimeSpan>()))
                .Returns(new ConsumeResult<string, string>
                {
                    Message = new Message<string, string> { Value = "last-message" }
                });

            // Act
            var result = await kafkaConsumer.ReadLastTopicMessageAsync(topic, timeoutInMilliseconds, cancellationToken);

            // Assert
            Assert.That(result, Is.EqualTo("last-message"));
        }
        [Test]
        public async Task ReadMessagesInDateRangeAsync_ReturnsMessagesInRange()
        {
            // Arrange
            var topic = "test-topic";
            var startDate = DateTime.UtcNow.AddDays(-1);
            var endDate = DateTime.UtcNow;
            var timeoutInMilliseconds = 2000;
            var cancellationToken = CancellationToken.None;
            var partitions = new List<PartitionMetadata>
            {
                new PartitionMetadata(0, 0, new int [0], new int [0], null)
            };
            var topicMetadata = new TopicMetadata(topic, partitions, null);
            var messages = new List<ConsumeResult<string, string>>
            {
                new ConsumeResult<string, string>
                {
                    Message = new Message<string, string> { Value = "message1", Timestamp = new Timestamp(startDate.AddMinutes(1)) },
                    TopicPartitionOffset = new TopicPartitionOffset(topic, 0, 1)
                },
                new ConsumeResult<string, string>
                {
                    Message = new Message<string, string> { Value = "message2", Timestamp = new Timestamp(endDate.AddMinutes(-1)) },
                    TopicPartitionOffset = new TopicPartitionOffset(topic, 0, 2)
                }
            };
            var messageIndex = 0;
            mockAdminClient.Setup(x => x.GetMetadata(topic, It.IsAny<TimeSpan>())).Returns(new Metadata(
                new List<BrokerMetadata>(),
                new List<TopicMetadata> { topicMetadata },
                0,
                "cluster-id"
            ));
            mockConsumer.Setup(x => x.Subscribe(topic));
            mockConsumer.Setup(x => x.OffsetsForTimes(It.IsAny<IEnumerable<TopicPartitionTimestamp>>(), It.IsAny<TimeSpan>()))
                .Returns(new List<TopicPartitionOffset> { new TopicPartitionOffset(topic, 0, 1), new TopicPartitionOffset(topic, 0, 2) });
            mockConsumer.Setup(x => x.Consume(It.IsAny<int>()))
                .Returns(() =>
                {
                    if (messageIndex < messages.Count)
                    {
                        return messages[messageIndex++];
                    }
                    return null;
                });
            // Act
            var result = await kafkaConsumer.ReadMessagesInDateRangeAsync(topic, startDate, endDate, timeoutInMilliseconds, cancellationToken);
            // Assert
            Assert.That(result.Count, Is.EqualTo(2));
            Assert.Contains("message1", result);
            Assert.Contains("message2", result);
        }
    }
}
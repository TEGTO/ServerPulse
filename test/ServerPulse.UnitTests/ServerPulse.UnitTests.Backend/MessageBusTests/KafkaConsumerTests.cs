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
            mockAdminClient.Setup(x => x.GetMetadata(topic, It.IsAny<TimeSpan>()))
                .Returns(new Metadata(
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
            var resultMessages = new List<ConsumeResponse>();
            await foreach (var message in kafkaConsumer.ConsumeAsync(topic, timeoutInMilliseconds, consumeFrom, cancellationToken))
            {
                resultMessages.Add(message);
                if (resultMessages.Count >= messages.Count)
                {
                    cancellationTokenSource.Cancel();
                }
            }
            // Assert
            Assert.That(resultMessages[0].Message, Is.EqualTo(messages[0]));
            Assert.That(resultMessages[1].Message, Is.EqualTo(messages[1]));
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
            Assert.That(result.Message, Is.EqualTo("last-message"));
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
            var options = new MessageInRangeQueryOptions(topic, timeoutInMilliseconds, startDate, endDate);
            var result = await kafkaConsumer.ReadMessagesInDateRangeAsync(options, cancellationToken);
            // Assert
            Assert.That(result.Count, Is.EqualTo(2));
            Assert.That(result.Any(x => x.Message == "message1"));
            Assert.That(result.Any(x => x.Message == "message2"));
        }
        [Test]
        public async Task GetAmountTopicMessagesAsync_ReturnsTotalMessageCount()
        {
            // Arrange
            var topicName = "test-topic";
            var timeoutInMilliseconds = 1000;
            var cancellationToken = CancellationToken.None;
            var partitions = new List<PartitionMetadata>
            {
                new PartitionMetadata(0, 0, new int[0], new int[0], null),
                new PartitionMetadata(1, 0, new int[0], new int[0], null)
            };
            mockAdminClient.Setup(x => x.GetMetadata(topicName, It.IsAny<TimeSpan>()))
                .Returns(new Metadata(
                    new List<BrokerMetadata>(),
                    new List<TopicMetadata> { new TopicMetadata(topicName, partitions, null) },
                    0,
                    "cluster-id"
                ));
            mockConsumer.Setup(x => x.QueryWatermarkOffsets(It.IsAny<TopicPartition>(), It.IsAny<TimeSpan>()))
                .Returns(new WatermarkOffsets(0, 10));
            // Act
            var result = await kafkaConsumer.GetAmountTopicMessagesAsync(topicName, timeoutInMilliseconds, cancellationToken);
            // Assert
            Assert.That(result, Is.EqualTo(20));
        }
        [Test]
        public async Task GetAmountTopicMessagesAsync_EmptyPartition_ReturnsZero()
        {
            // Arrange
            var topicName = "test-topic";
            var timeoutInMilliseconds = 1000;
            var cancellationToken = CancellationToken.None;
            var partitions = new List<PartitionMetadata>
            {
                new PartitionMetadata(0, 0, new int[0], new int[0], null)
            };
            mockAdminClient.Setup(x => x.GetMetadata(topicName, It.IsAny<TimeSpan>()))
                .Returns(new Metadata(
                    new List<BrokerMetadata>(),
                    new List<TopicMetadata> { new TopicMetadata(topicName, partitions, null) },
                    0,
                    "cluster-id"
                ));
            mockConsumer.Setup(x => x.QueryWatermarkOffsets(It.IsAny<TopicPartition>(), It.IsAny<TimeSpan>()))
                .Returns(new WatermarkOffsets(0, 0));
            // Act
            var result = await kafkaConsumer.GetAmountTopicMessagesAsync(topicName, timeoutInMilliseconds, cancellationToken);
            // Assert
            Assert.That(result, Is.EqualTo(0));
        }
        [Test]
        public async Task GetMessageAmountPerTimespanAsync_ReturnsCorrectCounts()
        {
            // Arrange
            var topicName = "test-topic";
            var timeoutInMilliseconds = 1000;
            var timeSpan = TimeSpan.FromHours(1);
            var cancellationToken = CancellationToken.None;
            var partitions = new List<PartitionMetadata>
            {
                new PartitionMetadata(0, 0, new int[0], new int[0], null),
                new PartitionMetadata(1, 0, new int[0], new int[0], null)
            };
            var options = new MessageInRangeQueryOptions(topicName, timeoutInMilliseconds, DateTime.UtcNow.AddDays(-1), DateTime.UtcNow);
            mockAdminClient.Setup(x => x.GetMetadata(topicName, It.IsAny<TimeSpan>()))
                .Returns(new Metadata(
                    new List<BrokerMetadata>(),
                    new List<TopicMetadata> { new TopicMetadata(topicName, partitions, null) },
                    0,
                    "cluster-id"
                ));
            mockConsumer.Setup(x => x.QueryWatermarkOffsets(It.IsAny<TopicPartition>(), It.IsAny<TimeSpan>()))
                .Returns(new WatermarkOffsets(0, 10));
            mockConsumer.Setup(x => x.OffsetsForTimes(It.IsAny<IEnumerable<TopicPartitionTimestamp>>(), It.IsAny<TimeSpan>()))
                .Returns((IEnumerable<TopicPartitionTimestamp> timestamps, TimeSpan ts) =>
                {
                    var timestampList = timestamps.ToList();
                    return timestampList.Select(t => new TopicPartitionOffset(t.TopicPartition, t.Timestamp.UnixTimestampMs / (1000 * 60 * 60))).ToList();
                });
            // Act
            var result = await kafkaConsumer.GetMessageAmountPerTimespanAsync(options, timeSpan, cancellationToken);
            // Assert
            Assert.That(result.Values.Count(), Is.EqualTo(24));
            Assert.That(result.Values.Sum(), Is.EqualTo(48));
        }
        [Test]
        public async Task GetMessageAmountPerTimespanAsync_EmptyPartitions_ReturnsEmptyResult()
        {
            // Arrange
            var topicName = "test-topic";
            var timeoutInMilliseconds = 1000;
            var timeSpan = TimeSpan.FromHours(1);
            var cancellationToken = CancellationToken.None;
            var partitions = new List<PartitionMetadata>
            {
                new PartitionMetadata(0, 0, new int[0], new int[0], null)
            };
            var options = new MessageInRangeQueryOptions(topicName, timeoutInMilliseconds, DateTime.UtcNow.AddDays(-1), DateTime.UtcNow);
            mockAdminClient.Setup(x => x.GetMetadata(topicName, It.IsAny<TimeSpan>()))
                .Returns(new Metadata(
                    new List<BrokerMetadata>(),
                    new List<TopicMetadata> { new TopicMetadata(topicName, partitions, null) },
                    0,
                    "cluster-id"
                ));
            mockConsumer.Setup(x => x.QueryWatermarkOffsets(It.IsAny<TopicPartition>(), It.IsAny<TimeSpan>()))
                .Returns(new WatermarkOffsets(0, 0));
            mockConsumer.Setup(x => x.OffsetsForTimes(It.IsAny<IEnumerable<TopicPartitionTimestamp>>(), It.IsAny<TimeSpan>()))
                .Returns(new List<TopicPartitionOffset>());
            // Act
            var result = await kafkaConsumer.GetMessageAmountPerTimespanAsync(options, timeSpan, cancellationToken);
            // Assert
            Assert.That(result.Values.Sum(), Is.EqualTo(0));
        }
        [Test]
        public async Task ReadSomeMessagesAsync_ReturnsRequestedNumberOfMessages()
        {
            // Arrange
            var options = new ReadSomeMessagesOptions("test-topic", 1000, 5, DateTime.UtcNow);
            var cancellationToken = CancellationToken.None;
            var partitions = new List<int> { 0, 1 };
            var topicMetadata = new Metadata(
                new List<BrokerMetadata>(),
                new List<TopicMetadata> { new TopicMetadata(options.TopicName, partitions.Select(p => new PartitionMetadata(p, 0, new int[0], new int[0], null)).ToList(), null) },
                0,
                "cluster-id"
            );
            mockAdminClient.Setup(x => x.GetMetadata(options.TopicName, It.IsAny<TimeSpan>())).Returns(topicMetadata);
            mockConsumer.Setup(x => x.OffsetsForTimes(It.IsAny<IEnumerable<TopicPartitionTimestamp>>(), It.IsAny<TimeSpan>()))
                .Returns(partitions.Select(p => new TopicPartitionOffset(new TopicPartition(options.TopicName, p), 0)).ToList());
            mockConsumer.Setup(x => x.QueryWatermarkOffsets(It.IsAny<TopicPartition>(), It.IsAny<TimeSpan>()))
                .Returns(new WatermarkOffsets(0, 10));
            var messageList = new List<ConsumeResult<string, string>>
            {
                new ConsumeResult<string, string> { Message = new Message<string, string> { Value = "message1", Timestamp = new Timestamp(DateTime.MinValue.AddDays(4)) }, TopicPartitionOffset = new TopicPartitionOffset(options.TopicName, 0, 1) },
                new ConsumeResult<string, string> { Message = new Message<string, string> { Value = "message2", Timestamp = new Timestamp(DateTime.MinValue.AddDays(3))}, TopicPartitionOffset = new TopicPartitionOffset(options.TopicName, 0, 2) },
                new ConsumeResult<string, string> { Message = new Message<string, string> { Value = "message3", Timestamp = new Timestamp(DateTime.MinValue.AddDays(2)) }, TopicPartitionOffset = new TopicPartitionOffset(options.TopicName, 0, 3) },
                new ConsumeResult<string, string> { Message = new Message<string, string> { Value = "message4", Timestamp = new Timestamp(DateTime.MinValue.AddDays(1)) }, TopicPartitionOffset = new TopicPartitionOffset(options.TopicName, 0, 4) },
                new ConsumeResult<string, string> { Message = new Message<string, string> { Value = "message5", Timestamp = new Timestamp(DateTime.MinValue) }, TopicPartitionOffset = new TopicPartitionOffset(options.TopicName, 0, 5) }
            };
            var messageIndex = 0;
            mockConsumer.Setup(x => x.Consume(It.IsAny<int>())).Returns(() =>
            {
                if (messageIndex < messageList.Count)
                {
                    return messageList[messageIndex++];
                }
                return null;
            });
            // Act
            var result = await kafkaConsumer.ReadSomeMessagesAsync(options, cancellationToken);
            // Assert
            Assert.That(result.Count, Is.EqualTo(5));
            Assert.That(result[0].Message, Is.EqualTo("message1"));
            Assert.That(result[1].Message, Is.EqualTo("message2"));
            Assert.That(result[2].Message, Is.EqualTo("message3"));
            Assert.That(result[3].Message, Is.EqualTo("message4"));
            Assert.That(result[4].Message, Is.EqualTo("message5"));
        }
        [Test]
        public async Task ReadSomeMessagesAsync_ReturnsRequestedNumberOfMessagesOrderedFromOldest()
        {
            // Arrange
            var options = new ReadSomeMessagesOptions("test-topic", 1000, 5, DateTime.MinValue, true);
            var cancellationToken = CancellationToken.None;
            var partitions = new List<int> { 0, 1 };
            var topicMetadata = new Metadata(
                new List<BrokerMetadata>(),
                new List<TopicMetadata> { new TopicMetadata(options.TopicName, partitions.Select(p => new PartitionMetadata(p, 0, new int[0], new int[0], null)).ToList(), null) },
                0,
                "cluster-id"
            );
            mockAdminClient.Setup(x => x.GetMetadata(options.TopicName, It.IsAny<TimeSpan>())).Returns(topicMetadata);
            mockConsumer.Setup(x => x.OffsetsForTimes(It.IsAny<IEnumerable<TopicPartitionTimestamp>>(), It.IsAny<TimeSpan>()))
                .Returns(partitions.Select(p => new TopicPartitionOffset(new TopicPartition(options.TopicName, p), 0)).ToList());
            mockConsumer.Setup(x => x.QueryWatermarkOffsets(It.IsAny<TopicPartition>(), It.IsAny<TimeSpan>()))
                .Returns(new WatermarkOffsets(0, 10));
            var messageList = new List<ConsumeResult<string, string>>
            {
                new ConsumeResult<string, string> { Message = new Message<string, string> { Value = "message1", Timestamp = new Timestamp(DateTime.MinValue.AddDays(5)) }, TopicPartitionOffset = new TopicPartitionOffset(options.TopicName, 0, 1) },
                new ConsumeResult<string, string> { Message = new Message<string, string> { Value = "message2", Timestamp = new Timestamp(DateTime.MinValue.AddDays(4))}, TopicPartitionOffset = new TopicPartitionOffset(options.TopicName, 0, 2) },
                new ConsumeResult<string, string> { Message = new Message<string, string> { Value = "message3", Timestamp = new Timestamp(DateTime.MinValue.AddDays(3)) }, TopicPartitionOffset = new TopicPartitionOffset(options.TopicName, 0, 3) },
                new ConsumeResult<string, string> { Message = new Message<string, string> { Value = "message4", Timestamp = new Timestamp(DateTime.MinValue.AddDays(2)) }, TopicPartitionOffset = new TopicPartitionOffset(options.TopicName, 0, 4) },
                new ConsumeResult<string, string> { Message = new Message<string, string> { Value = "message5", Timestamp = new Timestamp(DateTime.MinValue.AddDays(1)) }, TopicPartitionOffset = new TopicPartitionOffset(options.TopicName, 0, 5) }
            };
            var messageIndex = 0;
            mockConsumer.Setup(x => x.Consume(It.IsAny<int>())).Returns(() =>
            {
                if (messageIndex < messageList.Count)
                {
                    return messageList[messageIndex++];
                }
                return null;
            });
            // Act
            var result = await kafkaConsumer.ReadSomeMessagesAsync(options, cancellationToken);
            // Assert
            Assert.That(result.Count, Is.EqualTo(5));
            Assert.That(result[4].Message, Is.EqualTo("message1"));
            Assert.That(result[3].Message, Is.EqualTo("message2"));
            Assert.That(result[2].Message, Is.EqualTo("message3"));
            Assert.That(result[1].Message, Is.EqualTo("message4"));
            Assert.That(result[0].Message, Is.EqualTo("message5"));
        }
        [Test]
        public async Task ReadSomeMessagesAsync_ReturnsFewerMessagesThanRequested()
        {
            // Arrange
            var options = new ReadSomeMessagesOptions("test-topic", 1000, 5, DateTime.UtcNow);
            var cancellationToken = CancellationToken.None;
            var partitions = new List<int> { 0 };
            var topicMetadata = new Metadata(
                new List<BrokerMetadata>(),
                new List<TopicMetadata> { new TopicMetadata(options.TopicName, partitions.Select(p => new PartitionMetadata(p, 0, new int[0], new int[0], null)).ToList(), null) },
                0,
                "cluster-id"
            );
            mockAdminClient.Setup(x => x.GetMetadata(options.TopicName, It.IsAny<TimeSpan>())).Returns(topicMetadata);
            mockConsumer.Setup(x => x.OffsetsForTimes(It.IsAny<IEnumerable<TopicPartitionTimestamp>>(), It.IsAny<TimeSpan>()))
                .Returns(partitions.Select(p => new TopicPartitionOffset(new TopicPartition(options.TopicName, p), 0)).ToList());
            mockConsumer.Setup(x => x.QueryWatermarkOffsets(It.IsAny<TopicPartition>(), It.IsAny<TimeSpan>()))
                .Returns(new WatermarkOffsets(0, 3));
            var messageList = new List<ConsumeResult<string, string>>
            {
                new ConsumeResult<string, string> { Message = new Message<string, string> { Value = "message1", Timestamp = new Timestamp(DateTime.MinValue.AddDays(4)) }, TopicPartitionOffset = new TopicPartitionOffset(options.TopicName, 0, 1) },
                new ConsumeResult<string, string> { Message = new Message<string, string> { Value = "message2", Timestamp = new Timestamp(DateTime.MinValue.AddDays(3))}, TopicPartitionOffset = new TopicPartitionOffset(options.TopicName, 0, 2) },
                new ConsumeResult<string, string> { Message = new Message<string, string> { Value = "message3", Timestamp = new Timestamp(DateTime.MinValue.AddDays(2)) }, TopicPartitionOffset = new TopicPartitionOffset(options.TopicName, 0, 3) },
            };
            var messageIndex = 0;
            mockConsumer.Setup(x => x.Consume(It.IsAny<int>())).Returns(() =>
            {
                if (messageIndex < messageList.Count)
                {
                    return messageList[messageIndex++];
                }
                return null;
            });
            // Act
            var result = await kafkaConsumer.ReadSomeMessagesAsync(options, cancellationToken);
            // Assert
            Assert.That(result.Count, Is.EqualTo(3));
            Assert.That(result[0].Message, Is.EqualTo("message1"));
            Assert.That(result[1].Message, Is.EqualTo("message2"));
            Assert.That(result[2].Message, Is.EqualTo("message3"));
        }
        [Test]
        public async Task ReadSomeMessagesAsync_HandlesCancellationGracefully()
        {
            // Arrange
            var options = new ReadSomeMessagesOptions("test-topic", 1000, 5, DateTime.UtcNow);
            var cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = cancellationTokenSource.Token;
            var partitions = new List<int> { 0 };
            var topicMetadata = new Metadata(
                new List<BrokerMetadata>(),
                new List<TopicMetadata> { new TopicMetadata(options.TopicName, partitions.Select(p => new PartitionMetadata(p, 0, new int[0], new int[0], null)).ToList(), null) },
                0,
                "cluster-id"
            );
            mockAdminClient.Setup(x => x.GetMetadata(options.TopicName, It.IsAny<TimeSpan>())).Returns(topicMetadata);
            mockConsumer.Setup(x => x.OffsetsForTimes(It.IsAny<IEnumerable<TopicPartitionTimestamp>>(), It.IsAny<TimeSpan>()))
                .Returns(partitions.Select(p => new TopicPartitionOffset(new TopicPartition(options.TopicName, p), 0)).ToList());
            mockConsumer.Setup(x => x.QueryWatermarkOffsets(It.IsAny<TopicPartition>(), It.IsAny<TimeSpan>()))
                .Returns(new WatermarkOffsets(0, 10));
            var messageList = new List<ConsumeResult<string, string>>
            {
                new ConsumeResult<string, string> { Message = new Message<string, string> { Value = "message1", Timestamp = new Timestamp(DateTime.UtcNow) }, TopicPartitionOffset = new TopicPartitionOffset(options.TopicName, 0, 1) }
            };
            var messageIndex = 0;
            mockConsumer.Setup(x => x.Consume(It.IsAny<int>())).Returns(() =>
            {
                if (messageIndex < messageList.Count)
                {
                    cancellationTokenSource.Cancel();
                    return messageList[messageIndex++];
                }
                return null;
            });
            // Act + Assert
            Assert.ThrowsAsync<OperationCanceledException>(async () => await kafkaConsumer.ReadSomeMessagesAsync(options, cancellationToken));
        }
        [Test]
        public async Task ReadSomeMessagesAsync_ReturnsEmptyListIfNoMessages()
        {
            // Arrange
            var options = new ReadSomeMessagesOptions("test-topic", 1000, 5, DateTime.UtcNow);
            var cancellationToken = CancellationToken.None;
            var partitions = new List<int> { 0 };
            var topicMetadata = new Metadata(
                new List<BrokerMetadata>(),
                new List<TopicMetadata> { new TopicMetadata(options.TopicName, partitions.Select(p => new PartitionMetadata(p, 0, new int[0], new int[0], null)).ToList(), null) },
                0,
                "cluster-id"
            );
            mockAdminClient.Setup(x => x.GetMetadata(options.TopicName, It.IsAny<TimeSpan>())).Returns(topicMetadata);
            mockConsumer.Setup(x => x.OffsetsForTimes(It.IsAny<IEnumerable<TopicPartitionTimestamp>>(), It.IsAny<TimeSpan>()))
                .Returns(partitions.Select(p => new TopicPartitionOffset(new TopicPartition(options.TopicName, p), 0)).ToList());
            mockConsumer.Setup(x => x.QueryWatermarkOffsets(It.IsAny<TopicPartition>(), It.IsAny<TimeSpan>()))
                .Returns(new WatermarkOffsets(0, 0));
            mockConsumer.Setup(x => x.Consume(It.IsAny<int>())).Returns(() => null);
            // Act
            var result = await kafkaConsumer.ReadSomeMessagesAsync(options, cancellationToken);
            // Assert
            Assert.That(result.Count, Is.EqualTo(0));
        }
    }
}
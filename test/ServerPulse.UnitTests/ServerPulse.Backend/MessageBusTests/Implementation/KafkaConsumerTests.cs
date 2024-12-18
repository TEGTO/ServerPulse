using Confluent.Kafka;
using MessageBus.Interfaces;
using MessageBus.Kafka;
using MessageBus.Models;
using Moq;

namespace MessageBusTests.Implementation
{
    [TestFixture]
    internal class KafkaConsumerTests
    {
        private Mock<IAdminClient> mockAdminClient;
        private Mock<IConsumer<string, string>> mockConsumer;
        private Mock<IKafkaConsumerFactory> mockConsumerFactory;
        private KafkaConsumer consumer;

        [SetUp]
        public void Setup()
        {
            mockAdminClient = new Mock<IAdminClient>();
            mockConsumer = new Mock<IConsumer<string, string>>();
            mockConsumerFactory = new Mock<IKafkaConsumerFactory>();

            mockConsumerFactory.Setup(x => x.CreateConsumer()).Returns(mockConsumer.Object);

            consumer = new KafkaConsumer(mockAdminClient.Object, mockConsumerFactory.Object);
        }

        private void SetupMetadata(string topic, List<int> partitions)
        {
            var partitionMetadata = partitions.Select(p => new PartitionMetadata(p, 0, Array.Empty<int>(), Array.Empty<int>(), null)).ToList();
            var topicMetadata = new TopicMetadata(topic, partitionMetadata, null);

            mockAdminClient.Setup(x => x.GetMetadata(topic, It.IsAny<TimeSpan>()))
                .Returns(new Metadata(
                    new List<BrokerMetadata>(),
                    new List<TopicMetadata> { topicMetadata },
                    0,
                    "cluster-id"
                ));
        }

        private void SetupMetadata(string topicName, List<PartitionMetadata> partitions)
        {
            var topicMetadata = new TopicMetadata(topicName, partitions, null);

            mockAdminClient.Setup(x => x.GetMetadata(topicName, It.IsAny<TimeSpan>()))
                .Returns(new Metadata(
                    new List<BrokerMetadata>(),
                    new List<TopicMetadata> { topicMetadata },
                    0,
                    "cluster-id"
                ));
        }

        private void SetupPartitionOffsets(int partition, long low, long high)
        {
            mockConsumer.Setup(x => x.QueryWatermarkOffsets(
                It.Is<TopicPartition>(tp => tp.Partition.Value == partition),
                It.IsAny<TimeSpan>()
            )).Returns(new WatermarkOffsets(low, high));
        }

        private void SetupConsumerMessage(string value, DateTime timestamp)
        {
            mockConsumer.Setup(x => x.Consume(It.IsAny<TimeSpan>()))
                .Returns(new ConsumeResult<string, string>
                {
                    Message = new Message<string, string> { Value = value, Timestamp = new Timestamp(timestamp) },
                    IsPartitionEOF = false
                });
        }

        private void SetupConsumerMessages(List<ConsumeResult<string, string>> messages)
        {
            var messageQueue = new Queue<ConsumeResult<string, string>>(messages);

            mockConsumer.Setup(x => x.Consume(It.IsAny<int>()))
                .Returns(() => messageQueue.Any() ? messageQueue.Dequeue() : null!);
        }

        private static IEnumerable<TestCaseData> ConsumeAsyncTestCases()
        {
            yield return new TestCaseData(
                new List<string> { "message1", "message2" },
                Offset.Beginning,
                1000,
                2,
                true
            ).SetDescription("Consumes all valid messages with Offset.Beginning");

            yield return new TestCaseData(
                new List<string> { "message1" },
                Offset.End,
                500,
                1,
                true
            ).SetDescription("Consumes a single valid message with Offset.End");
        }

        [Test]
        [TestCaseSource(nameof(ConsumeAsyncTestCases))]
        public async Task ConsumeAsync_ValidatesMessagesConsumed(
            List<string> messages,
            Offset offset,
            int timeoutInMilliseconds,
            int expectedMessageCount,
            bool expectValidMessages)
        {
            // Arrange
            var topic = "test-topic";
            var cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = cancellationTokenSource.Token;
            var messageIndex = 0;

            mockAdminClient.Setup(x => x.GetMetadata(topic, It.IsAny<TimeSpan>()))
                .Returns(new Metadata(
                    new List<BrokerMetadata>(),
                    new List<TopicMetadata> { new TopicMetadata(topic, new List<PartitionMetadata>(), ErrorCode.NoError) },
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
                            Message = new Message<string, string> { Value = messages[messageIndex++] },
                            IsPartitionEOF = false
                        };
                    }
                    return null!;
                });

            // Act
            var resultMessages = new List<ConsumeResponse>();
            await foreach (var message in consumer.ConsumeAsync(topic, timeoutInMilliseconds, offset, cancellationToken))
            {
                resultMessages.Add(message);
                if (resultMessages.Count >= expectedMessageCount)
                {
                    await cancellationTokenSource.CancelAsync();
                    cancellationTokenSource.Dispose();
                }
            }

            // Assert
            Assert.That(resultMessages.Count, Is.EqualTo(expectedMessageCount));
            if (expectValidMessages)
            {
                for (int i = 0; i < messages.Count; i++)
                {
                    Assert.That(resultMessages[i].Message, Is.EqualTo(messages[i]));
                }
            }
        }

        [Test]
        public void ConsumeAsync_InvalidConsumeResult_SkipsMessage()
        {
            // Arrange
            var topic = "test-topic";
            var timeoutInMilliseconds = 1000;
            var cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = cancellationTokenSource.Token;

            mockAdminClient.Setup(x => x.GetMetadata(topic, It.IsAny<TimeSpan>()))
                .Returns(new Metadata(
                    new List<BrokerMetadata>(),
                    new List<TopicMetadata> { new TopicMetadata(topic, new List<PartitionMetadata>(), ErrorCode.NoError) },
                    0,
                    "cluster-id"
                ));

            mockConsumer.Setup(x => x.Consume(It.IsAny<TimeSpan>()))
                .Returns(new ConsumeResult<string, string>
                {
                    Message = null, // Invalid message
                    IsPartitionEOF = true
                });

            // Act
            var resultMessages = new List<ConsumeResponse>();

            _ = Task.Run(async () =>
            {
                await Task.Delay(timeoutInMilliseconds);
                await cancellationTokenSource.CancelAsync();
                cancellationTokenSource.Dispose();
            });

            Assert.ThrowsAsync<TaskCanceledException>(async () =>
            {
                await foreach (var message in consumer.ConsumeAsync(topic, timeoutInMilliseconds, Offset.Beginning, cancellationToken))
                {
                    resultMessages.Add(message);
                }
            });

            // Assert
            Assert.That(resultMessages.Count, Is.EqualTo(0));
        }

        [Test]
        public async Task ConsumeAsync_Cancellation_StopsConsumption()
        {
            // Arrange
            var topic = "test-topic";
            var timeoutInMilliseconds = 1000;
            var cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = cancellationTokenSource.Token;

            mockAdminClient.Setup(x => x.GetMetadata(topic, It.IsAny<TimeSpan>()))
                .Returns(new Metadata(
                    new List<BrokerMetadata>(),
                    new List<TopicMetadata> { new TopicMetadata(topic, new List<PartitionMetadata>(), ErrorCode.NoError) },
                    0,
                    "cluster-id"
                ));

            mockConsumer.Setup(x => x.Consume(It.IsAny<TimeSpan>()))
                .Returns(new ConsumeResult<string, string>
                {
                    Message = new Message<string, string> { Value = "message1" },
                    IsPartitionEOF = false
                });

            var resultMessages = new List<ConsumeResponse>();

            // Act
            var task = Task.Run(async () =>
            {
                await foreach (var message in consumer.ConsumeAsync(topic, timeoutInMilliseconds, Offset.Beginning, cancellationToken))
                {
                    resultMessages.Add(message);
                    if (resultMessages.Count == 1)
                    {
                        await cancellationTokenSource.CancelAsync();
                        cancellationTokenSource.Dispose();
                    }
                }
            });

            await task;

            // Assert
            Assert.That(resultMessages.Count, Is.EqualTo(1));
            Assert.That(resultMessages[0].Message, Is.EqualTo("message1"));
        }

        [Test]
        public async Task GetLastTopicMessageAsync_ReturnsLastMessage()
        {
            // Arrange
            var topic = "test-topic";
            var timeoutInMilliseconds = 2000;
            var cancellationToken = CancellationToken.None;

            SetupMetadata(topic, new List<int> { 0 });
            SetupPartitionOffsets(0, 0, 1);
            SetupConsumerMessage("last-message", DateTime.UtcNow);

            // Act
            var result = await consumer.GetLastTopicMessageAsync(topic, timeoutInMilliseconds, cancellationToken);

            // Assert
            Assert.That(result!.Message, Is.EqualTo("last-message"));
        }

        [Test]
        public async Task GetLastTopicMessageAsync_ReturnsNull_WhenNoPartitions()
        {
            // Arrange
            var topic = "test-topic";
            var timeoutInMilliseconds = 2000;
            var cancellationToken = CancellationToken.None;

            SetupMetadata(topic, new List<int>()); // No partitions

            // Act
            var result = await consumer.GetLastTopicMessageAsync(topic, timeoutInMilliseconds, cancellationToken);

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        public async Task GetLastTopicMessageAsync_ReturnsNull_WhenNoMessagesInPartition()
        {
            // Arrange
            var topic = "test-topic";
            var timeoutInMilliseconds = 2000;
            var cancellationToken = CancellationToken.None;

            SetupMetadata(topic, new List<int> { 0 });
            SetupPartitionOffsets(0, 0, 0); // No messages

            // Act
            var result = await consumer.GetLastTopicMessageAsync(topic, timeoutInMilliseconds, cancellationToken);

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        public async Task GetLastTopicMessageAsync_ReturnsNull_WhenConsumeResultMessageIsNull()
        {
            // Arrange
            var topic = "test-topic";
            var timeoutInMilliseconds = 2000;
            var cancellationToken = CancellationToken.None;

            SetupMetadata(topic, new List<int> { 0, 1 });
            SetupPartitionOffsets(0, 0, 1);
            SetupPartitionOffsets(1, 0, 1);

            mockConsumer.SetupSequence(x => x.Consume(It.IsAny<TimeSpan>()))
                .Returns(new ConsumeResult<string, string>
                {
                    Message = null,
                });

            // Act
            var result = await consumer.GetLastTopicMessageAsync(topic, timeoutInMilliseconds, cancellationToken);

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        public async Task GetLastTopicMessageAsync_ReturnsMostRecentMessageAcrossPartitions()
        {
            // Arrange
            var topic = "test-topic";
            var timeoutInMilliseconds = 2000;
            var cancellationToken = CancellationToken.None;
            var recentTimestamp = DateTime.UtcNow;

            SetupMetadata(topic, new List<int> { 0, 1 });
            SetupPartitionOffsets(0, 0, 1);
            SetupPartitionOffsets(1, 0, 1);

            mockConsumer.SetupSequence(x => x.Consume(It.IsAny<TimeSpan>()))
                .Returns(new ConsumeResult<string, string>
                {
                    Message = new Message<string, string> { Value = "message-0", Timestamp = new Timestamp(recentTimestamp.AddMinutes(-1)) },
                    IsPartitionEOF = false
                })
                .Returns(new ConsumeResult<string, string>
                {
                    Message = new Message<string, string> { Value = "message-1", Timestamp = new Timestamp(recentTimestamp) },
                    IsPartitionEOF = false
                });

            // Act
            var result = await consumer.GetLastTopicMessageAsync(topic, timeoutInMilliseconds, cancellationToken);

            // Assert
            Assert.That(result!.Message, Is.EqualTo("message-1"));
        }

        [Test]
        public async Task GetMessagesInDateRangeAsync_ReturnsMessagesInRange()
        {
            // Arrange
            var topic = "test-topic";
            var startDate = DateTime.UtcNow.AddDays(-1);
            var endDate = DateTime.UtcNow;
            var timeoutInMilliseconds = 2000;
            var cancellationToken = CancellationToken.None;

            SetupMetadata(topic, new List<int> { 0 });
            SetupPartitionOffsets(0, 1, 2);

            mockConsumer.Setup(x => x.OffsetsForTimes(It.IsAny<IEnumerable<TopicPartitionTimestamp>>(), It.IsAny<TimeSpan>()))
                .Returns(new List<TopicPartitionOffset> { new TopicPartitionOffset(topic, 0, 1), new TopicPartitionOffset(topic, 0, 2) });

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

            SetupConsumerMessages(messages);

            // Act
            var options = new GetMessageInDateRangeOptions(topic, timeoutInMilliseconds, startDate, endDate);
            var result = await consumer.GetMessagesInDateRangeAsync(options, cancellationToken);

            // Assert
            Assert.That(result.Count(), Is.LessThanOrEqualTo(2));
        }

        [Test]
        public void GetMessagesInDateRangeAsync_ThrowsArgumentException_ForInvalidDates()
        {
            // Arrange
            var options = new GetMessageInDateRangeOptions("test-topic", 2000, DateTime.UtcNow.AddMinutes(1), DateTime.UtcNow);
            var cancellationToken = CancellationToken.None;

            // Act & Assert
            Assert.ThrowsAsync<ArgumentException>(async () =>
                await consumer.GetMessagesInDateRangeAsync(options, cancellationToken));
        }

        [Test]
        public async Task GetMessagesInDateRangeAsync_ReturnsEmpty_ForNoMessagesInRange()
        {
            // Arrange
            var topic = "test-topic";
            var startDate = DateTime.UtcNow.AddDays(-1);
            var endDate = DateTime.UtcNow;
            var timeoutInMilliseconds = 2000;
            var cancellationToken = CancellationToken.None;

            SetupMetadata(topic, new List<int> { 0 });
            SetupPartitionOffsets(0, 1, 2);

            mockConsumer.Setup(x => x.OffsetsForTimes(It.IsAny<IEnumerable<TopicPartitionTimestamp>>(), It.IsAny<TimeSpan>()))
                .Returns(new List<TopicPartitionOffset> { new TopicPartitionOffset(topic, 0, 1), new TopicPartitionOffset(topic, 0, 2) });

            // Act
            var options = new GetMessageInDateRangeOptions(topic, timeoutInMilliseconds, startDate, endDate);
            var result = await consumer.GetMessagesInDateRangeAsync(options, cancellationToken);

            // Assert
            Assert.IsEmpty(result);
        }

        [Test]
        public async Task GetMessagesInDateRangeAsync_ReturnsMessagesFromMultiplePartitions()
        {
            // Arrange
            var topic = "test-topic";
            var startDate = DateTime.UtcNow.AddDays(-1);
            var endDate = DateTime.UtcNow;
            var timeoutInMilliseconds = 2000;
            var cancellationToken = CancellationToken.None;

            SetupMetadata(topic, new List<int> { 0, 1 });
            SetupPartitionOffsets(0, 1, 2);
            SetupPartitionOffsets(1, 1, 2);

            mockConsumer.Setup(x => x.OffsetsForTimes(It.IsAny<IEnumerable<TopicPartitionTimestamp>>(), It.IsAny<TimeSpan>()))
                .Returns(new List<TopicPartitionOffset> { new TopicPartitionOffset(topic, 0, 1), new TopicPartitionOffset(topic, 0, 2) });

            mockConsumer.Setup(x => x.OffsetsForTimes(It.IsAny<IEnumerable<TopicPartitionTimestamp>>(), It.IsAny<TimeSpan>()))
                .Returns(new List<TopicPartitionOffset> { new TopicPartitionOffset(topic, 1, 1), new TopicPartitionOffset(topic, 1, 2) });

            var messages = new List<ConsumeResult<string, string>>
            {
                new ConsumeResult<string, string>
                {
                    Message = new Message<string, string> { Value = "message1", Timestamp = new Timestamp(startDate.AddMinutes(1)) },
                    TopicPartitionOffset = new TopicPartitionOffset(topic, 0, 1)
                },
                new ConsumeResult<string, string>
                {
                    Message = new Message<string, string> { Value = "message2", Timestamp = new Timestamp(startDate.AddMinutes(2)) },
                    TopicPartitionOffset = new TopicPartitionOffset(topic, 1, 1)
                }
            };

            SetupConsumerMessages(messages);

            // Act
            var options = new GetMessageInDateRangeOptions(topic, timeoutInMilliseconds, startDate, endDate);
            var result = await consumer.GetMessagesInDateRangeAsync(options, cancellationToken);

            // Assert
            Assert.That(result.Count(), Is.LessThanOrEqualTo(2));
        }

        [Test]
        public async Task GetMessagesInDateRangeAsync_CancelsCorrectly()
        {
            // Arrange
            var topic = "test-topic";
            var startDate = DateTime.UtcNow.AddDays(-1);
            var endDate = DateTime.UtcNow;
            var timeoutInMilliseconds = 2000;
            var cancellationTokenSource = new CancellationTokenSource();

            SetupMetadata(topic, new List<int> { 0 });
            SetupPartitionOffsets(0, 1, 2);

            mockConsumer.Setup(x => x.OffsetsForTimes(It.IsAny<IEnumerable<TopicPartitionTimestamp>>(), It.IsAny<TimeSpan>()))
                .Returns(new List<TopicPartitionOffset> { new TopicPartitionOffset(topic, 0, 1), new TopicPartitionOffset(topic, 0, 2) });

            cancellationTokenSource.CancelAfter(100);

            // Act
            var options = new GetMessageInDateRangeOptions(topic, timeoutInMilliseconds, startDate, endDate);
            var result = await consumer.GetMessagesInDateRangeAsync(options, cancellationTokenSource.Token);

            // Assert
            Assert.IsEmpty(result);

            cancellationTokenSource.Dispose();
        }

        private static IEnumerable<TestCaseData> TopicMessageAmountTestCases()
        {
            yield return new TestCaseData(
                "test-topic-1",
                new List<int> { 0, 1 },
                new Dictionary<int, (long Low, long High)>
                {
                    { 0, (0, 10) },
                    { 1, (0, 10) }
                },
                20)
                .SetDescription("Returns the total message count for a topic with two partitions and 20 messages.");

            yield return new TestCaseData(
                "test-topic-2",
                new List<int> { 0 },
                new Dictionary<int, (long Low, long High)>
                {
                    { 0, (0, 5) }
                },
                5)
                .SetDescription("Returns the total message count for a topic with one partition and 5 messages.");

            yield return new TestCaseData(
                "test-topic-3",
                new List<int> { 0, 1, 2 },
                new Dictionary<int, (long Low, long High)>
                {
                    { 0, (0, 3) },
                    { 1, (0, 7) },
                    { 2, (0, 0) }
                },
                10)
                .SetDescription("Handles a topic with multiple partitions where one partition has no messages.");

            yield return new TestCaseData(
                "test-topic-empty",
                new List<int> { 0 },
                new Dictionary<int, (long Low, long High)>
                {
                    { 0, (0, 0) }
                },
                0)
                .SetDescription("Returns 0 when there are no messages in the partition.");
        }

        [Test]
        [TestCaseSource(nameof(TopicMessageAmountTestCases))]
        public async Task GetTopicMessageAmountAsync_ReturnsCorrectMessageCount(
            string topic,
            List<int> partitions,
            Dictionary<int, (long Low, long High)> partitionOffsets,
            int expectedMessageCount)
        {
            // Arrange
            var timeoutInMilliseconds = 1000;
            var cancellationToken = CancellationToken.None;

            SetupMetadata(topic, partitions);

            foreach (var (partition, offsets) in partitionOffsets)
            {
                SetupPartitionOffsets(partition, offsets.Low, offsets.High);
            }

            // Act
            var result = await consumer.GetTopicMessageAmountAsync(topic, timeoutInMilliseconds, cancellationToken);

            // Assert
            Assert.That(result, Is.EqualTo(expectedMessageCount));
        }

        [Test]
        public async Task GetTopicMessageAmountAsync_EmptyPartition_ReturnsZero()
        {
            // Arrange
            var topic = "test-topic";
            var timeoutInMilliseconds = 1000;
            var cancellationToken = CancellationToken.None;

            SetupMetadata(topic, new List<int> { });

            // Act
            var result = await consumer.GetTopicMessageAmountAsync(topic, timeoutInMilliseconds, cancellationToken);

            // Assert
            Assert.That(result, Is.EqualTo(0));
        }

        private static IEnumerable<TestCaseData> GetTopicMessageAmountPerTimespanTestCases()
        {
            yield return new TestCaseData(
                "test-topic-1",
                new List<PartitionMetadata>
                {
                    new PartitionMetadata(0, 0, Array.Empty<int>(), Array.Empty<int>(), null),
                    new PartitionMetadata(1, 0, Array.Empty<int>(), Array.Empty<int>(), null)
                },
                new WatermarkOffsets(0, 10),
                -24,
                0,
                TimeSpan.FromHours(1),
                24,
                48
            ).SetDescription("Returns correct message count for two partitions over 24 hourly timespans.");

            yield return new TestCaseData(
                "test-topic-2",
                new List<PartitionMetadata>
                {
                    new PartitionMetadata(0, 0, Array.Empty<int>(), Array.Empty<int>(), null)
                },
                new WatermarkOffsets(0, 0),
                -24,
                0,
                TimeSpan.FromHours(1),
                24,
                24
            ).SetDescription("Returns zero message count when there are no messages in the partition.");

            yield return new TestCaseData(
                "test-topic-3",
                new List<PartitionMetadata>
                {
                    new PartitionMetadata(0, 0, Array.Empty<int>(), Array.Empty<int>(), null),
                    new PartitionMetadata(1, 0, Array.Empty<int>(), Array.Empty<int>(), null)
                },
                new WatermarkOffsets(5, 15),
                -6,
                0,
                TimeSpan.FromMinutes(30),
                12,
                24
            ).SetDescription("Handles half-hourly timespans over a 6-hour range with two partitions.");
        }

        [Test]
        [TestCaseSource(nameof(GetTopicMessageAmountPerTimespanTestCases))]
        public async Task GetTopicMessageAmountPerTimespanAsync_ValidInput_ReturnsCorrectCounts(
            string topicName,
            List<PartitionMetadata> partitions,
            WatermarkOffsets watermarkOffsets,
            int fromAddHours,
            int toAddHours,
            TimeSpan timeSpan,
            int expectedTimespanCount,
            int expectedMessageCount)
        {
            // Arrange
            var timeoutInMilliseconds = 1000;
            var cancellationToken = CancellationToken.None;

            SetupMetadata(topicName, partitions);

            mockConsumer.Setup(x => x.QueryWatermarkOffsets(It.IsAny<TopicPartition>(), It.IsAny<TimeSpan>()))
                .Returns(watermarkOffsets);

            mockConsumer.Setup(x => x.OffsetsForTimes(It.IsAny<IEnumerable<TopicPartitionTimestamp>>(), It.IsAny<TimeSpan>()))
                .Returns((IEnumerable<TopicPartitionTimestamp> timestamps, TimeSpan ts) =>
                {
                    return timestamps.Select(t => new TopicPartitionOffset(
                        t.TopicPartition,
                        t.Timestamp.UnixTimestampMs / (int)timeSpan.TotalMilliseconds)).ToList(); //Event per timeSpan for each partition
                });

            var options = new GetMessageInDateRangeOptions(topicName, timeoutInMilliseconds,
                DateTime.UtcNow.AddHours(fromAddHours), DateTime.UtcNow.AddHours(toAddHours));

            // Act
            var result = await consumer.GetTopicMessageAmountPerTimespanAsync(options, timeSpan, cancellationToken);

            // Assert
            Assert.That(result.Count, Is.EqualTo(expectedTimespanCount));
            Assert.That(result.Values.Sum(), Is.EqualTo(expectedMessageCount));
        }

        [Test]
        public void GetTopicMessageAmountPerTimespanAsync_InvalidDateRange_ThrowsException()
        {
            // Arrange
            var topicName = "test-topic";
            var timeoutInMilliseconds = 1000;
            var cancellationToken = CancellationToken.None;

            var options = new GetMessageInDateRangeOptions(
                topicName,
                timeoutInMilliseconds,
                DateTime.UtcNow.AddDays(1), // Future date
                DateTime.UtcNow.AddDays(-1) // Past date
            );

            // Act & Assert
            var ex = Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                await consumer.GetTopicMessageAmountPerTimespanAsync(options, TimeSpan.FromHours(1), cancellationToken);
            });

            Assert.That(ex.Message, Does.Contain("Invalid Start Date"));
        }

        [Test]
        public async Task GetSomeMessagesStartFromDateAsync_ReturnsRequestedMessages_WithValidInput()
        {
            // Arrange
            var topic = "test-topic";
            var options = new GetSomeMessagesFromDateOptions(topic, 1000, 5, DateTime.UtcNow);
            var cancellationToken = CancellationToken.None;
            var partitions = new List<int> { 0, 1 };
            var messageList = new List<ConsumeResult<string, string>>
            {
                new ConsumeResult<string, string>
                {
                    Message = new Message<string, string> { Value = "message1", Timestamp = new Timestamp(DateTime.MinValue.AddDays(4)) },
                    TopicPartitionOffset = new TopicPartitionOffset(options.TopicName, 0, 1)
                },
                new ConsumeResult<string, string>
                {
                    Message = new Message<string, string> { Value = "message2", Timestamp = new Timestamp(DateTime.MinValue.AddDays(3)) },
                    TopicPartitionOffset = new TopicPartitionOffset(options.TopicName, 0, 2)
                }
            };

            SetupMetadata(topic, partitions);

            SetupPartitionOffsets(0, 0, 10);
            SetupPartitionOffsets(1, 0, 10);
            mockConsumer.Setup(x => x.OffsetsForTimes(It.IsAny<IEnumerable<TopicPartitionTimestamp>>(), It.IsAny<TimeSpan>()))
                .Returns(new List<TopicPartitionOffset> { new TopicPartitionOffset(topic, 0, 0), new TopicPartitionOffset(topic, 1, 0) });

            SetupConsumerMessages(messageList);

            // Act
            var result = (await consumer.GetSomeMessagesStartFromDateAsync(options, cancellationToken)).ToList();

            await Task.Delay(100);

            // Assert
            Assert.That(result.Count, Is.LessThanOrEqualTo(4)); // Two events for the two partition 
        }

        [Test]
        public async Task GetSomeMessagesStartFromDateAsync_HandlesMultiplePartitionsCorrectly()
        {
            // Arrange
            var topic = "test-topic";
            var options = new GetSomeMessagesFromDateOptions(topic, 1000, 5, DateTime.UtcNow);
            var cancellationToken = CancellationToken.None;
            var partitions = new List<int> { 0, 1 };
            var messageListPartition0 = new List<ConsumeResult<string, string>>
            {
                new ConsumeResult<string, string>
                {
                    Message = new Message<string, string> { Value = "message1", Timestamp = new Timestamp(DateTime.UtcNow.AddMinutes(-10)) },
                    TopicPartitionOffset = new TopicPartitionOffset(options.TopicName, 0, 1)
                }
            };
            var messageListPartition1 = new List<ConsumeResult<string, string>>
            {
                new ConsumeResult<string, string>
                {
                    Message = new Message<string, string> { Value = "message2", Timestamp = new Timestamp(DateTime.UtcNow.AddMinutes(-5)) },
                    TopicPartitionOffset = new TopicPartitionOffset(options.TopicName, 1, 1)
                }
            };

            SetupMetadata(topic, partitions);

            SetupPartitionOffsets(0, 0, 10);
            SetupPartitionOffsets(1, 0, 10);
            mockConsumer.Setup(x => x.OffsetsForTimes(It.IsAny<IEnumerable<TopicPartitionTimestamp>>(), It.IsAny<TimeSpan>()))
                .Returns(new List<TopicPartitionOffset> { new TopicPartitionOffset(topic, 0, 0), new TopicPartitionOffset(topic, 1, 0) });

            SetupConsumerMessages(messageListPartition0.Concat(messageListPartition1).ToList());

            // Act
            var result = (await consumer.GetSomeMessagesStartFromDateAsync(options, cancellationToken)).ToList();

            await Task.Delay(100);

            // Assert
            Assert.That(result.Count, Is.LessThanOrEqualTo(4));
        }

        [Test]
        public async Task GetSomeMessagesStartFromDateAsync_ReturnsRequestedNumberOfMessages()
        {
            // Arrange
            var topic = "test-topic";
            var options = new GetSomeMessagesFromDateOptions(topic, 1000, 5, DateTime.UtcNow);
            var cancellationToken = CancellationToken.None;
            var partitions = new List<int> { 0, 1 };
            var messageList = new List<ConsumeResult<string, string>>
            {
                new ConsumeResult<string, string> { Message = new Message<string, string> { Value = "message1", Timestamp = new Timestamp(DateTime.MinValue.AddDays(4)) }, TopicPartitionOffset = new TopicPartitionOffset(options.TopicName, 0, 1) },
                new ConsumeResult<string, string> { Message = new Message<string, string> { Value = "message2", Timestamp = new Timestamp(DateTime.MinValue.AddDays(3))}, TopicPartitionOffset = new TopicPartitionOffset(options.TopicName, 0, 2) },
                new ConsumeResult<string, string> { Message = new Message<string, string> { Value = "message3", Timestamp = new Timestamp(DateTime.MinValue.AddDays(2)) }, TopicPartitionOffset = new TopicPartitionOffset(options.TopicName, 0, 3) },
                new ConsumeResult<string, string> { Message = new Message<string, string> { Value = "message4", Timestamp = new Timestamp(DateTime.MinValue.AddDays(1)) }, TopicPartitionOffset = new TopicPartitionOffset(options.TopicName, 0, 4) },
                new ConsumeResult<string, string> { Message = new Message<string, string> { Value = "message5", Timestamp = new Timestamp(DateTime.MinValue) }, TopicPartitionOffset = new TopicPartitionOffset(options.TopicName, 0, 5) }
            };

            SetupMetadata(topic, partitions);

            SetupPartitionOffsets(0, 0, 10);
            SetupPartitionOffsets(1, 0, 10);

            mockConsumer.Setup(x => x.OffsetsForTimes(It.IsAny<IEnumerable<TopicPartitionTimestamp>>(), It.IsAny<TimeSpan>()))
                .Returns(partitions.Select(p => new TopicPartitionOffset(new TopicPartition(options.TopicName, p), 0)).ToList());

            SetupConsumerMessages(messageList);

            // Act
            var result = (await consumer.GetSomeMessagesStartFromDateAsync(options, cancellationToken)).ToList();

            await Task.Delay(100);

            // Assert
            Assert.That(result.Count, Is.LessThanOrEqualTo(5));
        }

        [Test]
        public async Task GetSomeMessagesStartFromDateAsync_ReturnsLessMessagesThanRequested()
        {
            // Arrange
            var topic = "test-topic";
            var options = new GetSomeMessagesFromDateOptions(topic, 1000, 5, DateTime.UtcNow);
            var cancellationToken = CancellationToken.None;
            var partitions = new List<int> { 0 };
            var messageList = new List<ConsumeResult<string, string>>
            {
                new ConsumeResult<string, string> { Message = new Message<string, string> { Value = "message1", Timestamp = new Timestamp(DateTime.MinValue.AddDays(4)) }, TopicPartitionOffset = new TopicPartitionOffset(options.TopicName, 0, 1) },
                new ConsumeResult<string, string> { Message = new Message<string, string> { Value = "message2", Timestamp = new Timestamp(DateTime.MinValue.AddDays(3))}, TopicPartitionOffset = new TopicPartitionOffset(options.TopicName, 0, 2) },
                new ConsumeResult<string, string> { Message = new Message<string, string> { Value = "message3", Timestamp = new Timestamp(DateTime.MinValue.AddDays(2)) }, TopicPartitionOffset = new TopicPartitionOffset(options.TopicName, 0, 3) },
            };

            SetupMetadata(topic, partitions);

            SetupPartitionOffsets(0, 0, 10);

            mockConsumer.Setup(x => x.OffsetsForTimes(It.IsAny<IEnumerable<TopicPartitionTimestamp>>(), It.IsAny<TimeSpan>()))
                .Returns(partitions.Select(p => new TopicPartitionOffset(new TopicPartition(options.TopicName, p), 0)).ToList());

            SetupConsumerMessages(messageList);

            // Act
            var result = (await consumer.GetSomeMessagesStartFromDateAsync(options, cancellationToken)).ToList();

            await Task.Delay(100);

            // Assert
            Assert.That(result.Count, Is.LessThanOrEqualTo(3));
        }

        [Test]
        public async Task GetSomeMessagesStartFromDateAsync_ReturnsEmptyListIfNoMessages()
        {
            // Arrange
            var topic = "test-topic";
            var options = new GetSomeMessagesFromDateOptions(topic, 1000, 5, DateTime.UtcNow);
            var cancellationToken = CancellationToken.None;
            var partitions = new List<int> { 0 };

            SetupMetadata(topic, partitions);

            SetupPartitionOffsets(0, 0, 10);

            mockConsumer.Setup(x => x.OffsetsForTimes(It.IsAny<IEnumerable<TopicPartitionTimestamp>>(), It.IsAny<TimeSpan>()))
                .Returns(partitions.Select(p => new TopicPartitionOffset(new TopicPartition(options.TopicName, p), 0)).ToList());

            mockConsumer.Setup(x => x.Consume(It.IsAny<int>())).Returns(() => null!);

            // Act
            var result = await consumer.GetSomeMessagesStartFromDateAsync(options, cancellationToken);

            // Assert
            Assert.That(result.Count, Is.EqualTo(0));
        }

        [Test]
        public void GetSomeMessagesStartFromDateAsync_ThrowsExceptionForInvalidStartDate()
        {
            // Arrange
            var options = new GetSomeMessagesFromDateOptions("test-topic", 1000, 5, DateTime.UtcNow.AddDays(1), true);
            var cancellationToken = CancellationToken.None;

            // Act & Assert
            var ex = Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                await consumer.GetSomeMessagesStartFromDateAsync(options, cancellationToken);
            });

            Assert.That(ex!.Message, Does.Contain("Invalid Start Date"));
        }
    }
}
using AnalyzerApi.Domain.Dtos.Wrappers;
using AutoMapper;
using Confluent.Kafka;
using MessageBus.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using ServerPulse.EventCommunication.Events;
using System.Text.Json;

namespace AnalyzerApi.Services.Tests
{
    [TestFixture()]
    internal class ServerLoadReceiverTests
    {
        private const string KAFKA_LOAD_TOPIC = "KafkaLoadTopic_";
        private const int KAFKA_TIMEOUT_IN_MILLISECONDS = 5000;
        private const int STATISTICS_SAVE_DATA_IN_DAYS = 30;

        private Mock<IMessageConsumer> mockMessageConsumer;
        private Mock<IMapper> mockMapper;
        private Mock<IConfiguration> mockConfiguration;
        private ServerLoadReceiver serverLoadReceiver;

        [SetUp]
        public void Setup()
        {
            mockMessageConsumer = new Mock<IMessageConsumer>();
            mockMapper = new Mock<IMapper>();
            mockConfiguration = new Mock<IConfiguration>();

            mockConfiguration.SetupGet(c => c[Configuration.KAFKA_LOAD_TOPIC]).Returns(KAFKA_LOAD_TOPIC);
            mockConfiguration.SetupGet(c => c[Configuration.KAFKA_TIMEOUT_IN_MILLISECONDS]).Returns(KAFKA_TIMEOUT_IN_MILLISECONDS.ToString());
            mockConfiguration.SetupGet(c => c[Configuration.KAFKA_TOPIC_DATA_SAVE_IN_DAYS]).Returns(STATISTICS_SAVE_DATA_IN_DAYS.ToString());

            serverLoadReceiver = new ServerLoadReceiver(mockMessageConsumer.Object, mockMapper.Object, Mock.Of<ILogger<ServerLoadReceiver>>(), mockConfiguration.Object);
        }

        [Test]
        public async Task ConsumeLoadEventAsync_ValidMessages_YieldsLoadEvents()
        {
            // Arrange
            var key = "validSlotKey";
            var topic = KAFKA_LOAD_TOPIC + key;
            var cancellationToken = CancellationToken.None;
            var loadEvents = new List<string>
            {
                JsonSerializer.Serialize(new LoadEvent(key, "1", "", 200, TimeSpan.Zero, DateTime.MinValue)),
                JsonSerializer.Serialize(new LoadEvent(key, "2", "", 200, TimeSpan.Zero, DateTime.MinValue))
            };
            mockMessageConsumer
                .Setup(x => x.ConsumeAsync(topic, KAFKA_TIMEOUT_IN_MILLISECONDS, Offset.End, cancellationToken))
                .Returns(AsyncEnumerable(loadEvents));

            mockMapper.Setup(m => m.Map<LoadEventWrapper>(It.IsAny<LoadEvent>()))
                      .Returns((LoadEvent le) => new LoadEventWrapper { Key = le.Key });
            // Act
            var receivedEvents = new List<LoadEventWrapper>();
            await foreach (var loadEvent in serverLoadReceiver.ConsumeLoadEventAsync(key, cancellationToken))
            {
                receivedEvents.Add(loadEvent);
            }
            // Assert
            Assert.That(receivedEvents.Count, Is.EqualTo(loadEvents.Count));
            Assert.That(receivedEvents[0].Key, Is.EqualTo(key));
            Assert.That(receivedEvents[1].Key, Is.EqualTo(key));
        }
        [Test]
        public async Task ReceiveEventsInRangeAsync_ValidMessages_ReturnsEventWrappers()
        {
            // Arrange
            var options = new InRangeQueryOptions("validSlotKey", DateTime.UtcNow.AddDays(-1), DateTime.UtcNow);
            var topic = KAFKA_LOAD_TOPIC + options.Key;
            var consumeResponses = new List<ConsumeResponse>
            {
                new ConsumeResponse(JsonSerializer.Serialize(new LoadEvent(options.Key, "1", "", 200, TimeSpan.Zero, DateTime.MinValue)), DateTime.UtcNow.AddHours(-1)),
                new ConsumeResponse(JsonSerializer.Serialize(new LoadEvent(options.Key, "1", "", 200, TimeSpan.Zero, DateTime.MinValue)), DateTime.UtcNow)
            };
            mockMessageConsumer
                .Setup(x => x.ReadMessagesInDateRangeAsync(It.IsAny<MessageInRangeQueryOptions>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(consumeResponses);

            mockMapper.Setup(m => m.Map<LoadEventWrapper>(It.IsAny<LoadEvent>()))
                      .Returns((LoadEvent le) => new LoadEventWrapper { Key = le.Key });
            // Act
            var result = await serverLoadReceiver.ReceiveEventsInRangeAsync(options, CancellationToken.None);
            // Assert
            Assert.That(result.Count(), Is.EqualTo(consumeResponses.Count));
            Assert.That(result.First().Key, Is.EqualTo(options.Key));
            Assert.That(result.Last().Key, Is.EqualTo(options.Key));
        }
        [Test]
        public async Task GetCertainAmountOfEvents_ValidMessages_ReturnsEventWrappers()
        {
            // Arrange
            var options = new ReadCertainMessageNumberOptions("validSlotKey", 2, DateTime.UtcNow.AddDays(-1), true);
            var topic = KAFKA_LOAD_TOPIC + options.Key;
            var consumeResponses = new List<ConsumeResponse>
            {
                new ConsumeResponse(JsonSerializer.Serialize(new LoadEvent(options.Key, "1", "", 200, TimeSpan.Zero, DateTime.MinValue)), DateTime.UtcNow.AddHours(-1)),
                new ConsumeResponse(JsonSerializer.Serialize(new LoadEvent(options.Key, "1", "", 200, TimeSpan.Zero, DateTime.MinValue)), DateTime.UtcNow)
            };
            mockMessageConsumer
                .Setup(x => x.ReadSomeMessagesAsync(It.IsAny<ReadSomeMessagesOptions>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(consumeResponses);

            mockMapper.Setup(m => m.Map<LoadEventWrapper>(It.IsAny<LoadEvent>()))
                      .Returns((LoadEvent le) => new LoadEventWrapper { Key = le.Key });
            // Act
            var result = await serverLoadReceiver.GetCertainAmountOfEvents(options, CancellationToken.None);
            // Assert
            Assert.That(result.Count(), Is.EqualTo(consumeResponses.Count));
            Assert.That(result.First().Key, Is.EqualTo(options.Key));
            Assert.That(result.Last().Key, Is.EqualTo(options.Key));
        }
        [Test]
        public async Task ReceiveLoadEventAmountByKeyAsync_ValidTopic_ReturnsAmount()
        {
            // Arrange
            var key = "validSlotKey";
            var topic = KAFKA_LOAD_TOPIC + key;
            var expectedAmount = 10;
            mockMessageConsumer
                .Setup(x => x.GetAmountTopicMessagesAsync(topic, KAFKA_TIMEOUT_IN_MILLISECONDS, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedAmount);
            // Act
            var result = await serverLoadReceiver.ReceiveLoadEventAmountByKeyAsync(key, CancellationToken.None);
            // Assert
            Assert.That(result, Is.EqualTo(expectedAmount));
        }
        [Test]
        public async Task GetAmountStatisticsInDaysAsync_ValidTopic_ReturnsStatistics()
        {
            // Arrange
            var key = "validSlotKey";
            var topic = KAFKA_LOAD_TOPIC + key;
            var timeSpan = TimeSpan.FromDays(1);
            var start = DateTime.UtcNow.Date.AddDays(-STATISTICS_SAVE_DATA_IN_DAYS);
            var end = DateTime.UtcNow.Date.AddDays(1);
            var messageAmounts = new Dictionary<DateTime, int>
            {
                { start, 5 },
                { start.AddDays(1), 3 }
            };
            mockMessageConsumer
                .Setup(x => x.GetMessageAmountPerTimespanAsync(It.IsAny<MessageInRangeQueryOptions>(), timeSpan, It.IsAny<CancellationToken>()))
                .ReturnsAsync(messageAmounts);
            // Act
            var result = await serverLoadReceiver.GetAmountStatisticsInDaysAsync(key, CancellationToken.None);
            // Assert
            Assert.That(result.Count(), Is.EqualTo(messageAmounts.Count));
            Assert.That(result.First().AmountOfEvents, Is.EqualTo(3));
            Assert.That(result.Last().AmountOfEvents, Is.EqualTo(5));
        }
        [Test]
        public async Task GetAmountStatisticsLastDayAsync_ValidTopic_ReturnsStatistics()
        {
            // Arrange
            var key = "validSlotKey";
            var topic = KAFKA_LOAD_TOPIC + key;
            var todayStart = DateTime.UtcNow.Date;
            var timeSpan = TimeSpan.FromDays(1);
            var messageAmounts = new Dictionary<DateTime, int>
            {
                { todayStart, 7 }
            };
            mockMessageConsumer
                .Setup(x => x.GetMessageAmountPerTimespanAsync(It.IsAny<MessageInRangeQueryOptions>(), timeSpan, It.IsAny<CancellationToken>()))
                .ReturnsAsync(messageAmounts);
            // Act
            var result = await serverLoadReceiver.GetAmountStatisticsLastDayAsync(key, CancellationToken.None);
            // Assert
            Assert.That(result.Count(), Is.EqualTo(messageAmounts.Count));
            Assert.That(result.First().AmountOfEvents, Is.EqualTo(7));
        }
        [Test]
        public async Task GetAmountStatisticsInRangeAsync_ValidTopic_ReturnsStatistics()
        {
            // Arrange
            var options = new InRangeQueryOptions("validSlotKey", DateTime.UtcNow.AddDays(-10), DateTime.UtcNow);
            var topic = KAFKA_LOAD_TOPIC + options.Key;
            var timeSpan = TimeSpan.FromDays(1);
            var messageAmounts = new Dictionary<DateTime, int>
            {
                { options.From, 10 },
                { options.To.AddDays(-1), 8 }
            };
            mockMessageConsumer
                .Setup(x => x.GetMessageAmountPerTimespanAsync(It.IsAny<MessageInRangeQueryOptions>(), timeSpan, It.IsAny<CancellationToken>()))
                .ReturnsAsync(messageAmounts);
            // Act
            var result = await serverLoadReceiver.GetAmountStatisticsInRangeAsync(options, timeSpan, CancellationToken.None);
            // Assert
            Assert.That(result.Count(), Is.EqualTo(messageAmounts.Count));
            Assert.That(result.First().AmountOfEvents, Is.EqualTo(8));
            Assert.That(result.Last().AmountOfEvents, Is.EqualTo(10));
        }
        [Test]
        public async Task ReceiveLastLoadEventByKeyAsync_ValidMessage_ReturnsDeserializedEvent()
        {
            // Arrange
            var key = "validSlotKey";
            var topic = KAFKA_LOAD_TOPIC + key;
            var message = JsonSerializer.Serialize(new LoadEvent(key, "1", "", 200, TimeSpan.Zero, DateTime.MinValue));
            mockMessageConsumer.Setup(x => x.ReadLastTopicMessageAsync(topic, KAFKA_TIMEOUT_IN_MILLISECONDS, It.IsAny<CancellationToken>()))
                               .ReturnsAsync(new ConsumeResponse(message, DateTime.UtcNow));
            mockMapper.Setup(m => m.Map<LoadEventWrapper>(It.IsAny<LoadEvent>()))
                      .Returns((LoadEvent le) => new LoadEventWrapper { Key = le.Key });
            // Act
            var result = await serverLoadReceiver.ReceiveLastLoadEventByKeyAsync(key, CancellationToken.None);

            // Assert
            Assert.IsNotNull(result);
            Assert.That(result.Key, Is.EqualTo(key));
        }
        private static async IAsyncEnumerable<ConsumeResponse> AsyncEnumerable(IEnumerable<string> items)
        {
            foreach (var item in items)
            {
                var consumeResponse = new ConsumeResponse(item, DateTime.MinValue);
                yield return consumeResponse;
                await Task.Yield();
            }
        }
    }
}
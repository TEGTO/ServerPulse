using AnalyzerApi.Domain.Dtos.Wrappers;
using AnalyzerApi.Services.Interfaces;
using AnalyzerApiTests.Services;
using Confluent.Kafka;
using MessageBus.Interfaces;
using Moq;
using ServerPulse.EventCommunication.Events;
using System.Text.Json;

namespace AnalyzerApi.Services
{
    [TestFixture]
    internal class CustomReceiverTests : BaseEventReceiverTests
    {
        private const string KAFKA_CUSTOM_TOPIC = "KafkaCustomTopic_";

        private CustomReceiver customReceiver;

        [SetUp]
        public override void Setup()
        {
            base.Setup();

            mockConfiguration.SetupGet(c => c[Configuration.KAFKA_CUSTOM_TOPIC]).Returns(KAFKA_CUSTOM_TOPIC);

            customReceiver = new CustomReceiver(mockMessageConsumer.Object, mockMapper.Object, mockConfiguration.Object);
        }

        [Test]
        public async Task ConsumeCustomEventAsync_ValidMessages_YieldsLoadEvents()
        {
            // Arrange
            var key = "validSlotKey";
            var topic = KAFKA_CUSTOM_TOPIC + key;
            var cancellationToken = CancellationToken.None;
            var events = new List<string>
            {
                JsonSerializer.Serialize(new CustomEvent(key, "name1", "desc1")),
                JsonSerializer.Serialize(new CustomEvent(key, "name2", "desc2"))
            };
            mockMessageConsumer
                .Setup(x => x.ConsumeAsync(topic, KAFKA_TIMEOUT_IN_MILLISECONDS, Offset.End, cancellationToken))
                .Returns(AsyncEnumerable(events));

            mockMapper.Setup(m => m.Map<CustomEventWrapper>(It.IsAny<CustomEvent>()))
                      .Returns((CustomEvent le) => new CustomEventWrapper { Key = le.Key });
            // Act
            var receivedEvents = new List<CustomEventWrapper>();
            await foreach (var loadEvent in customReceiver.ConsumeCustomEventAsync(key, cancellationToken))
            {
                receivedEvents.Add(loadEvent);
            }
            // Assert
            Assert.That(receivedEvents.Count, Is.EqualTo(events.Count));
            Assert.That(receivedEvents[0].Key, Is.EqualTo(key));
            Assert.That(receivedEvents[1].Key, Is.EqualTo(key));
        }

        [Test]
        public async Task ReceiveEventsInRangeAsync_ValidMessages_ReturnsEventWrappers()
        {
            // Arrange
            var options = new InRangeQueryOptions("validSlotKey", DateTime.UtcNow.AddDays(-1), DateTime.UtcNow);
            var topic = KAFKA_CUSTOM_TOPIC + options.Key;
            var consumeResponses = new List<ConsumeResponse>
            {
                new ConsumeResponse(JsonSerializer.Serialize(new CustomEvent(options.Key, "name1", "desc1")), DateTime.UtcNow.AddHours(-1)),
                new ConsumeResponse(JsonSerializer.Serialize(new CustomEvent(options.Key, "name1", "desc1")), DateTime.UtcNow)
            };
            mockMessageConsumer
                .Setup(x => x.ReadMessagesInDateRangeAsync(It.IsAny<MessageInRangeQueryOptions>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(consumeResponses);

            mockMapper.Setup(m => m.Map<CustomEventWrapper>(It.IsAny<CustomEvent>()))
                      .Returns((CustomEvent le) => new CustomEventWrapper { Key = le.Key });
            // Act
            var result = await customReceiver.ReceiveEventsInRangeAsync(options, CancellationToken.None);
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
            var topic = KAFKA_CUSTOM_TOPIC + options.Key;
            var consumeResponses = new List<ConsumeResponse>
            {
                new ConsumeResponse(JsonSerializer.Serialize(new CustomEvent(options.Key, "name1", "desc1")), DateTime.UtcNow.AddHours(-1)),
                new ConsumeResponse(JsonSerializer.Serialize(new CustomEvent(options.Key, "name1", "desc1")), DateTime.UtcNow)
            };
            mockMessageConsumer
                .Setup(x => x.ReadSomeMessagesAsync(It.IsAny<ReadSomeMessagesOptions>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(consumeResponses);

            mockMapper.Setup(m => m.Map<CustomEventWrapper>(It.IsAny<CustomEvent>()))
                      .Returns((CustomEvent le) => new CustomEventWrapper { Key = le.Key });
            // Act
            var result = await customReceiver.GetCertainAmountOfEvents(options, CancellationToken.None);
            // Assert
            Assert.That(result.Count(), Is.EqualTo(consumeResponses.Count));
            Assert.That(result.First().Key, Is.EqualTo(options.Key));
            Assert.That(result.Last().Key, Is.EqualTo(options.Key));
        }
        [Test]
        public async Task ReceiveLastCustomEventByKeyAsync_ValidMessage_ReturnsDeserializedEvent()
        {
            // Arrange
            var key = "validSlotKey";
            var topic = KAFKA_CUSTOM_TOPIC + key;
            var message = JsonSerializer.Serialize(new CustomEvent(key, "name1", "desc1"));
            mockMessageConsumer.Setup(x => x.ReadLastTopicMessageAsync(topic, KAFKA_TIMEOUT_IN_MILLISECONDS, It.IsAny<CancellationToken>()))
                               .ReturnsAsync(new ConsumeResponse(message, DateTime.UtcNow));
            mockMapper.Setup(m => m.Map<CustomEventWrapper>(It.IsAny<CustomEvent>()))
                      .Returns((CustomEvent le) => new CustomEventWrapper { Key = le.Key });
            // Act
            var result = await customReceiver.ReceiveLastCustomEventByKeyAsync(key, CancellationToken.None);

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
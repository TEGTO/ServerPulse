using AnalyzerApi;
using AnalyzerApi.Domain.Dtos.Wrappers;
using AnalyzerApi.Services.Interfaces;
using AnalyzerApi.Services.Receivers.Event;
using AutoMapper;
using Confluent.Kafka;
using MessageBus.Interfaces;
using Microsoft.Extensions.Configuration;
using Moq;
using ServerPulse.EventCommunication.Events;
using System.Text.Json;

namespace AnalyzerApiTests.Services.Receivers.Event
{
    [TestFixture]
    internal class EventReceiverTests
    {
        private const string KafkaTimeoutInMilliseconds = "5000";
        private const string TopicName = "test-topic";

        private Mock<IMessageConsumer> mockMessageConsumer;
        private Mock<IMapper> mockMapper;
        private Mock<IConfiguration> mockConfiguration;
        private EventReceiverTopicData<MockEventWrapper> topicData;
        private EventReceiver<MockEvent, MockEventWrapper> eventReceiver;

        [SetUp]
        public void Setup()
        {
            mockMessageConsumer = new Mock<IMessageConsumer>();
            mockMapper = new Mock<IMapper>();
            mockConfiguration = new Mock<IConfiguration>();
            mockConfiguration.SetupGet(c => c[Configuration.KAFKA_TIMEOUT_IN_MILLISECONDS])
                             .Returns(KafkaTimeoutInMilliseconds);

            topicData = new EventReceiverTopicData<MockEventWrapper>(TopicName);

            eventReceiver = new EventReceiver<MockEvent, MockEventWrapper>(
                mockMessageConsumer.Object,
                mockMapper.Object,
                mockConfiguration.Object,
                topicData);
        }

        [Test]
        public async Task ConsumeEventAsync_MessagesExist_AddsReturnEvents()
        {
            // Arrange
            var cancellationToken = CancellationToken.None;
            var consumeResponses = new List<ConsumeResponse>
            {
                new ConsumeResponse(JsonSerializer.Serialize(new MockEvent("")), DateTime.UtcNow),
                new ConsumeResponse(JsonSerializer.Serialize(new MockEvent("")), DateTime.UtcNow)
            };
            mockMessageConsumer.Setup(m => m.ConsumeAsync(It.IsAny<string>(), It.IsAny<int>(), Offset.End, cancellationToken))
                               .Returns(AsyncEnumerable(consumeResponses));
            mockMapper.Setup(m => m.Map<MockEventWrapper>(It.IsAny<MockEvent>()))
                      .Returns(new MockEventWrapper());
            mockMessageConsumer.Setup(m => m.ReadLastTopicMessageAsync(It.IsAny<string>(), It.IsAny<int>(), cancellationToken))
                               .ReturnsAsync(consumeResponses.Last());
            // Act
            var results = new List<MockEventWrapper>();
            await foreach (var ev in eventReceiver.ConsumeEventAsync("key", cancellationToken))
            {
                results.Add(ev);
            }
            // Assert
            Assert.That(results.Count, Is.EqualTo(consumeResponses.Count));
            mockMessageConsumer.Verify(m => m.ConsumeAsync(It.IsAny<string>(), It.IsAny<int>(), Offset.End, cancellationToken), Times.Once);
        }
        [Test]
        public async Task GetCertainAmountOfEventsAsync_ReturnsEvents()
        {
            // Arrange
            var cancellationToken = CancellationToken.None;
            var options = new ReadCertainMessageNumberOptions("key", 5, DateTime.UtcNow, true);
            var consumeResponses = new List<ConsumeResponse>
            {
                new ConsumeResponse(JsonSerializer.Serialize(new MockEvent("")), DateTime.UtcNow),
                new ConsumeResponse(JsonSerializer.Serialize(new MockEvent("")), DateTime.UtcNow)
            };

            mockMessageConsumer.Setup(m => m.ReadSomeMessagesAsync(It.IsAny<ReadSomeMessagesOptions>(), cancellationToken))
                               .ReturnsAsync(consumeResponses);
            mockMapper.Setup(m => m.Map<MockEventWrapper>(It.IsAny<MockEvent>()))
                      .Returns(new MockEventWrapper());
            // Act
            var results = await eventReceiver.GetCertainAmountOfEventsAsync(options, cancellationToken);
            // Assert
            Assert.That(results.Count(), Is.EqualTo(consumeResponses.Count));
            mockMessageConsumer.Verify(m => m.ReadSomeMessagesAsync(It.IsAny<ReadSomeMessagesOptions>(), cancellationToken), Times.Once);
        }
        [Test]
        public async Task ReceiveEventsInRangeAsync_ReturnEventsd()
        {
            // Arrange
            var cancellationToken = CancellationToken.None;
            var options = new InRangeQueryOptions("key", DateTime.UtcNow.AddHours(-1), DateTime.UtcNow);
            var consumeResponses = new List<ConsumeResponse>
            {
                new ConsumeResponse(JsonSerializer.Serialize(new MockEvent("")), DateTime.UtcNow),
                new ConsumeResponse(JsonSerializer.Serialize(new MockEvent("")), DateTime.UtcNow)
            };

            mockMessageConsumer.Setup(m => m.ReadMessagesInDateRangeAsync(It.IsAny<MessageInRangeQueryOptions>(), cancellationToken))
                               .ReturnsAsync(consumeResponses);
            mockMapper.Setup(m => m.Map<MockEventWrapper>(It.IsAny<MockEvent>()))
                      .Returns(new MockEventWrapper());
            // Act
            var results = await eventReceiver.ReceiveEventsInRangeAsync(options, cancellationToken);
            // Assert
            Assert.That(results.Count(), Is.EqualTo(consumeResponses.Count));
            mockMessageConsumer.Verify(m => m.ReadMessagesInDateRangeAsync(It.IsAny<MessageInRangeQueryOptions>(), cancellationToken), Times.Once);
        }
        [Test]
        public async Task ReceiveLastEventByKeyAsync_ReturnsLastEvent()
        {
            // Arrange
            var cancellationToken = CancellationToken.None;
            var consumeResponse = new ConsumeResponse(JsonSerializer.Serialize(new MockEvent("")), DateTime.UtcNow);
            mockMessageConsumer.Setup(m => m.ReadLastTopicMessageAsync(It.IsAny<string>(), It.IsAny<int>(), cancellationToken))
                               .ReturnsAsync(consumeResponse);
            mockMapper.Setup(m => m.Map<MockEventWrapper>(It.IsAny<MockEvent>()))
                      .Returns(new MockEventWrapper());
            // Act
            var result = await eventReceiver.ReceiveLastEventByKeyAsync("key", cancellationToken);
            // Assert
            Assert.IsNotNull(result);
            mockMessageConsumer.Verify(m => m.ReadLastTopicMessageAsync(It.IsAny<string>(), It.IsAny<int>(), cancellationToken), Times.Once);
        }
        [Test]
        public async Task ReceiveEventAmountByKeyAsync_ReturnsEventCount()
        {
            // Arrange
            var cancellationToken = CancellationToken.None;
            var expectedCount = 10;

            mockMessageConsumer.Setup(m => m.GetAmountTopicMessagesAsync(It.IsAny<string>(), It.IsAny<int>(), cancellationToken))
                               .ReturnsAsync(expectedCount);
            // Act
            var result = await eventReceiver.ReceiveEventAmountByKeyAsync("key", cancellationToken);
            // Assert
            Assert.That(result, Is.EqualTo(expectedCount));
            mockMessageConsumer.Verify(m => m.GetAmountTopicMessagesAsync(It.IsAny<string>(), It.IsAny<int>(), cancellationToken), Times.Once);
        }

        private static async IAsyncEnumerable<T> AsyncEnumerable<T>(IEnumerable<T> items)
        {
            foreach (var item in items)
            {
                yield return item;
                await Task.Yield();
            }
        }
    }

    public record class MockEvent(string Key) : BaseEvent(Key);
    public class MockEventWrapper : BaseEventWrapper { }
}
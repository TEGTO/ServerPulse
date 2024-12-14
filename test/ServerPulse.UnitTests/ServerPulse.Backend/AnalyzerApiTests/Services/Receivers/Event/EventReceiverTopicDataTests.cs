using AnalyzerApi.Infrastructure;
using AnalyzerApi.Infrastructure.Configurations;
using AnalyzerApi.Infrastructure.Models;
using AnalyzerApi.Infrastructure.Models.Wrappers;
using AnalyzerApi.Services.SerializeStrategies;
using Confluent.Kafka;
using EventCommunication;
using MessageBus.Interfaces;
using Microsoft.Extensions.Configuration;
using Moq;
using System.Text.Json;

namespace AnalyzerApi.Services.Receivers.Event.Tests
{
    [TestFixture]
    internal class EventReceiverTests
    {
        private const string KafkaTimeoutInMilliseconds = "5000";
        private const string TopicName = "test-topic";

        private Mock<IMessageConsumer> mockMessageConsumer;
        private Mock<IConfiguration> mockConfiguration;
        private Mock<IEventSerializeStrategy<MockEventWrapper>> mockSerializeStrategy;
        private EventReceiverTopicConfiguration<MockEventWrapper> topicData;
        private EventReceiver<MockEventWrapper> eventReceiver;

        [SetUp]
        public void Setup()
        {
            mockMessageConsumer = new Mock<IMessageConsumer>();
            mockConfiguration = new Mock<IConfiguration>();
            mockSerializeStrategy = new Mock<IEventSerializeStrategy<MockEventWrapper>>();

            mockConfiguration.SetupGet(c => c[Configuration.KAFKA_TIMEOUT_IN_MILLISECONDS])
                .Returns(KafkaTimeoutInMilliseconds);

            topicData = new EventReceiverTopicConfiguration<MockEventWrapper>(TopicName);

            eventReceiver = new EventReceiver<MockEventWrapper>(
                mockMessageConsumer.Object,
                mockConfiguration.Object,
                mockSerializeStrategy.Object,
                topicData
            );
        }

        [Test]
        public async Task GetEventStreamAsync_HandlesLargeNumberOfMessages()
        {
            // Arrange
            var cancellationToken = CancellationToken.None;
            var key = "test-key";
            var largeNumberOfMessages = 1000;

            var responses = Enumerable.Range(1, largeNumberOfMessages)
                .Select(i => new ConsumeResponse(JsonSerializer.Serialize(new MockEventWrapper() { Id = "someId", Key = key }), DateTime.UtcNow));

            mockMessageConsumer.Setup(m => m.ConsumeAsync(It.IsAny<string>(), It.IsAny<int>(), Offset.End, cancellationToken))
                .Returns(AsyncEnumerable(responses));
            mockSerializeStrategy.Setup(s => s.SerializeResponse(It.IsAny<ConsumeResponse>()))
                .Returns<ConsumeResponse>(response => response.Message != null ? new MockEventWrapper() { Id = "someId", Key = key } : null);

            // Act
            var results = new List<MockEventWrapper>();
            await foreach (var ev in eventReceiver.GetEventStreamAsync(key, cancellationToken))
            {
                results.Add(ev);
            }

            // Assert
            Assert.That(results.Count, Is.EqualTo(largeNumberOfMessages));
            Assert.That(results[0].Key, Is.EqualTo(key));

            mockMessageConsumer.Verify(m => m.ConsumeAsync(It.IsAny<string>(), It.IsAny<int>(), Offset.End, cancellationToken), Times.Once);
        }

        [Test]
        public async Task GetEventStreamAsync_SkipsNullSerializedEvents()
        {
            // Arrange
            var cancellationToken = CancellationToken.None;
            var key = "test-key";

            var consumeResponses = new List<ConsumeResponse>
            {
                new ConsumeResponse(JsonSerializer.Serialize(new MockEventWrapper() { Id = "someId", Key = key }), DateTime.UtcNow),
                new ConsumeResponse(null!, DateTime.UtcNow)
            };

            mockMessageConsumer.Setup(m => m.ConsumeAsync(It.IsAny<string>(), It.IsAny<int>(), Offset.End, cancellationToken))
               .Returns(AsyncEnumerable(consumeResponses));
            mockSerializeStrategy.Setup(s => s.SerializeResponse(It.IsAny<ConsumeResponse>()))
                .Returns<ConsumeResponse>(response => response.Message != null ? new MockEventWrapper() { Id = "someId", Key = key } : null);

            // Act
            var results = new List<MockEventWrapper>();
            await foreach (var ev in eventReceiver.GetEventStreamAsync(key, cancellationToken))
            {
                results.Add(ev);
            }

            // Assert
            Assert.That(results.Count, Is.EqualTo(1));
            Assert.That(results[0].Key, Is.EqualTo(key));

            mockMessageConsumer.Verify(m => m.ConsumeAsync(It.IsAny<string>(), It.IsAny<int>(), Offset.End, cancellationToken), Times.Once);
        }

        [Test]
        public async Task GetEventAmountByKeyAsync_ReturnsAmount()
        {
            // Arrange
            var cancellationToken = CancellationToken.None;
            var key = "test-key";

            mockMessageConsumer.Setup(m => m.GetAmountTopicMessagesAsync(It.IsAny<string>(), It.IsAny<int>(), cancellationToken))
                .ReturnsAsync(10);

            // Act
            var result = await eventReceiver.GetEventAmountByKeyAsync(key, cancellationToken);

            // Assert
            Assert.That(result, Is.EqualTo(10));

            mockMessageConsumer.Verify(m => m.GetAmountTopicMessagesAsync(It.IsAny<string>(), It.IsAny<int>(), cancellationToken), Times.Once);
        }

        [Test]
        public async Task GetCertainAmountOfEventsAsync_ReturnsEvents()
        {
            // Arrange
            var cancellationToken = CancellationToken.None;
            var key = "test-key";
            var options = new GetCertainMessageNumberOptions(key, 5, DateTime.UtcNow, true);
            var consumeResponses = new List<ConsumeResponse>
            {
                new ConsumeResponse(JsonSerializer.Serialize(new MockEvent("")), DateTime.UtcNow),
                new ConsumeResponse(JsonSerializer.Serialize(new MockEvent("")), DateTime.UtcNow)
            };

            mockMessageConsumer.Setup(m => m.ReadSomeMessagesAsync(It.IsAny<ReadSomeMessagesOptions>(), cancellationToken))
                .ReturnsAsync(consumeResponses);
            mockSerializeStrategy.Setup(s => s.SerializeResponse(It.IsAny<ConsumeResponse>()))
                .Returns<ConsumeResponse>(response => response.Message != null ? new MockEventWrapper() { Id = "someId", Key = key } : null);

            // Act
            var results = await eventReceiver.GetCertainAmountOfEventsAsync(options, cancellationToken);

            // Assert
            Assert.That(results.Count(), Is.EqualTo(consumeResponses.Count));
            Assert.That(results.First().Key, Is.EqualTo(key));

            mockMessageConsumer.Verify(m => m.ReadSomeMessagesAsync(It.IsAny<ReadSomeMessagesOptions>(), cancellationToken), Times.Once);
        }


        [Test]
        public async Task GetCertainAmountOfEventsAsync_SkipsNullSerializedEvents()
        {
            // Arrange
            var cancellationToken = CancellationToken.None;
            var key = "test-key";
            var options = new GetCertainMessageNumberOptions(key, 5, DateTime.UtcNow, true);
            var consumeResponses = new List<ConsumeResponse>
            {
                new ConsumeResponse(JsonSerializer.Serialize(new MockEvent("")), DateTime.UtcNow),
                new ConsumeResponse(null!, DateTime.UtcNow)
            };

            mockMessageConsumer.Setup(m => m.ReadSomeMessagesAsync(It.IsAny<ReadSomeMessagesOptions>(), cancellationToken))
                .ReturnsAsync(consumeResponses);
            mockSerializeStrategy.Setup(s => s.SerializeResponse(It.IsAny<ConsumeResponse>()))
                .Returns<ConsumeResponse>(response => response.Message != null ? new MockEventWrapper() { Id = "someId", Key = key } : null);

            // Act
            var results = await eventReceiver.GetCertainAmountOfEventsAsync(options, cancellationToken);

            // Assert
            Assert.That(results.Count(), Is.EqualTo(1));
            Assert.That(results.First().Key, Is.EqualTo(key));

            mockMessageConsumer.Verify(m => m.ReadSomeMessagesAsync(It.IsAny<ReadSomeMessagesOptions>(), cancellationToken), Times.Once);
        }

        [Test]
        public async Task GetEventsInRangeAsync_ReturnEvents()
        {
            // Arrange
            var cancellationToken = CancellationToken.None;
            var key = "test-key";
            var options = new GetInRangeOptions(key, DateTime.UtcNow.AddHours(-1), DateTime.UtcNow);
            var consumeResponses = new List<ConsumeResponse>
            {
                new ConsumeResponse(JsonSerializer.Serialize(new MockEvent("")), DateTime.UtcNow),
                new ConsumeResponse(JsonSerializer.Serialize(new MockEvent("")), DateTime.UtcNow)
            };

            mockMessageConsumer.Setup(m => m.ReadMessagesInDateRangeAsync(It.IsAny<MessageInRangeQueryOptions>(), cancellationToken))
                .ReturnsAsync(consumeResponses);
            mockSerializeStrategy.Setup(s => s.SerializeResponse(It.IsAny<ConsumeResponse>()))
                .Returns<ConsumeResponse>(response => response.Message != null ? new MockEventWrapper() { Id = "someId", Key = key } : null);

            // Act
            var results = await eventReceiver.GetEventsInRangeAsync(options, cancellationToken);

            // Assert
            Assert.That(results.Count(), Is.EqualTo(consumeResponses.Count));
            Assert.That(results.First().Key, Is.EqualTo(key));

            mockMessageConsumer.Verify(m => m.ReadMessagesInDateRangeAsync(It.IsAny<MessageInRangeQueryOptions>(), cancellationToken), Times.Once);
        }

        [Test]
        public async Task GetEventsInRangeAsync_SkipsNullSerializedEvents()
        {
            // Arrange
            var cancellationToken = CancellationToken.None;
            var key = "test-key";
            var options = new GetInRangeOptions(key, DateTime.UtcNow.AddHours(-1), DateTime.UtcNow);
            var consumeResponses = new List<ConsumeResponse>
            {
                new ConsumeResponse(JsonSerializer.Serialize(new MockEvent("")), DateTime.UtcNow),
                new ConsumeResponse(null!, DateTime.UtcNow)
            };

            mockMessageConsumer.Setup(m => m.ReadMessagesInDateRangeAsync(It.IsAny<MessageInRangeQueryOptions>(), cancellationToken))
                .ReturnsAsync(consumeResponses);
            mockSerializeStrategy.Setup(s => s.SerializeResponse(It.IsAny<ConsumeResponse>()))
                .Returns<ConsumeResponse>(response => response.Message != null ? new MockEventWrapper() { Id = "someId", Key = key } : null);

            // Act
            var results = await eventReceiver.GetEventsInRangeAsync(options, cancellationToken);

            // Assert
            Assert.That(results.Count(), Is.EqualTo(1));
            Assert.That(results.First().Key, Is.EqualTo(key));

            mockMessageConsumer.Verify(m => m.ReadMessagesInDateRangeAsync(It.IsAny<MessageInRangeQueryOptions>(), cancellationToken), Times.Once);
        }

        [Test]
        public async Task GetLastEventByKeyAsync_ReturnsLastEvent()
        {
            // Arrange
            var key = "test-key";
            var cancellationToken = CancellationToken.None;
            var consumeResponse = new ConsumeResponse(JsonSerializer.Serialize(new MockEvent(key)), DateTime.UtcNow);

            mockMessageConsumer.Setup(m => m.ReadLastTopicMessageAsync(It.IsAny<string>(), It.IsAny<int>(), cancellationToken))
                .ReturnsAsync(consumeResponse);
            mockSerializeStrategy.Setup(s => s.SerializeResponse(It.IsAny<ConsumeResponse>()))
                .Returns<ConsumeResponse>(response => response.Message != null ? new MockEventWrapper() { Id = "someId", Key = key } : null);

            // Act
            var result = await eventReceiver.GetLastEventByKeyAsync(key, cancellationToken);

            // Assert
            Assert.IsNotNull(result);
            Assert.That(result.Key, Is.EqualTo(key));

            mockMessageConsumer.Verify(m => m.ReadLastTopicMessageAsync(It.IsAny<string>(), It.IsAny<int>(), cancellationToken), Times.Once);
        }

        [Test]
        public async Task GetLastEventByKeyAsync_ReturnsNullIfConsumeResponseIsNull()
        {
            // Arrange
            var key = "test-key";
            var cancellationToken = CancellationToken.None;

            mockMessageConsumer.Setup(m => m.ReadLastTopicMessageAsync(It.IsAny<string>(), It.IsAny<int>(), cancellationToken))
                .ReturnsAsync((ConsumeResponse)null!);

            // Act
            var result = await eventReceiver.GetLastEventByKeyAsync(key, cancellationToken);

            // Assert
            Assert.IsNull(result);

            mockMessageConsumer.Verify(m => m.ReadLastTopicMessageAsync(It.IsAny<string>(), It.IsAny<int>(), cancellationToken), Times.Once);
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
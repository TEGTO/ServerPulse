using AnalyzerApi;
using AnalyzerApi.Services;
using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Moq;
using ServerPulse.EventCommunication.Events;
using System.Text.Json;
using TestKafka.Consumer.Services;

namespace AnalyzerApiTests.Services
{
    [TestFixture]
    internal class MessageReceiverTests
    {
        private const string KAFKA_PULSE_TOPIC = "KafkaPulseTopic_{id}";
        private const string KAFKA_CONFIGURATION_TOPIC = "KafkaConfigurationTopic_{id}";
        private const string KAFKA_LOAD_TOPIC = "KafkaLoadTopic_{id}";
        private const int KAFKA_TIMEOUT_IN_MILLISECONDS = 5000;

        private Mock<IMessageConsumer> mockMessageConsumer;
        private Mock<IConfiguration> mockConfiguration;
        private ServerStatusReceiver messageReceiver;

        [SetUp]
        public void Setup()
        {
            mockMessageConsumer = new Mock<IMessageConsumer>();
            mockConfiguration = new Mock<IConfiguration>();
            mockConfiguration.SetupGet(c => c[Configuration.KAFKA_ALIVE_TOPIC]).Returns(KAFKA_PULSE_TOPIC);
            mockConfiguration.SetupGet(c => c[Configuration.KAFKA_CONFIGURATION_TOPIC]).Returns(KAFKA_CONFIGURATION_TOPIC);
            mockConfiguration.SetupGet(c => c[Configuration.KAFKA_LOAD_TOPIC]).Returns(KAFKA_LOAD_TOPIC);
            mockConfiguration.SetupGet(c => c[Configuration.KAFKA_TIMEOUT_IN_MILLISECONDS]).Returns(KAFKA_TIMEOUT_IN_MILLISECONDS.ToString());
            messageReceiver = new ServerStatusReceiver(mockMessageConsumer.Object, mockConfiguration.Object);
        }

        [Test]
        public async Task ConsumePulseEventAsync_ValidMessages_YieldsPulseEvents()
        {
            // Arrange
            var key = "validSlotKey";
            var topic = KAFKA_PULSE_TOPIC.Replace("{id}", key);
            var cancellationToken = CancellationToken.None;
            var pulseEvents = new List<string>
            {
                JsonSerializer.Serialize(new PulseEvent(key, true)),
                JsonSerializer.Serialize(new PulseEvent(key, false))
            };
            mockMessageConsumer
                .Setup(x => x.ConsumeAsync(topic, KAFKA_TIMEOUT_IN_MILLISECONDS, Offset.End, cancellationToken))
                .Returns(AsyncEnumerable(pulseEvents));
            // Act
            var receivedEvents = new List<PulseEvent>();
            await foreach (var pulseEvent in messageReceiver.ConsumePulseEventAsync(key, cancellationToken))
            {
                receivedEvents.Add(pulseEvent);
            }
            // Assert
            Assert.That(receivedEvents.Count, Is.EqualTo(pulseEvents.Count));
            Assert.That(receivedEvents[0].IsAlive, Is.True);
            Assert.That(receivedEvents[1].IsAlive, Is.False);
        }
        [Test]
        public async Task ConsumeConfigurationEventAsync_ValidMessages_YieldsPulseEvents()
        {
            // Arrange
            var key = "validSlotKey";
            var topic = KAFKA_CONFIGURATION_TOPIC.Replace("{id}", key);
            var cancellationToken = CancellationToken.None;
            var confEvents = new List<string>
            {
                JsonSerializer.Serialize(new ConfigurationEvent(key, TimeSpan.Zero)),
                JsonSerializer.Serialize(new ConfigurationEvent(key, TimeSpan.FromHours(1)))
            };
            mockMessageConsumer
                .Setup(x => x.ConsumeAsync(topic, KAFKA_TIMEOUT_IN_MILLISECONDS, Offset.End, cancellationToken))
                .Returns(AsyncEnumerable(confEvents));
            // Act
            var receivedEvents = new List<ConfigurationEvent>();
            await foreach (var confEvent in messageReceiver.ConsumeConfigurationEventAsync(key, cancellationToken))
            {
                receivedEvents.Add(confEvent);
            }
            // Assert
            Assert.That(receivedEvents.Count, Is.EqualTo(confEvents.Count));
            Assert.That(receivedEvents[0].ServerKeepAliveInterval, Is.EqualTo(TimeSpan.Zero));
            Assert.That(receivedEvents[1].ServerKeepAliveInterval, Is.EqualTo(TimeSpan.FromHours(1)));
        }
        [Test]
        public async Task ReceiveLastPulseEventByKeyAsync_ValidMessage_ReturnsDeserializedPulseEvent()
        {
            // Arrange
            var key = "validSlotKey";
            var topic = KAFKA_PULSE_TOPIC.Replace("{id}", key);
            var message = JsonSerializer.Serialize(new PulseEvent(key, true));
            var cancellationToken = CancellationToken.None;
            mockMessageConsumer.Setup(x => x.ReadLastTopicMessageAsync(topic, KAFKA_TIMEOUT_IN_MILLISECONDS, cancellationToken)).ReturnsAsync(message);
            // Act
            var result = await messageReceiver.ReceiveLastPulseEventByKeyAsync(key, cancellationToken);
            // Assert
            Assert.IsInstanceOf<PulseEvent>(result);
            Assert.That(result.Key, Is.EqualTo(key));
            Assert.That(result.IsAlive, Is.EqualTo(true));
        }
        [Test]
        public async Task ReceiveLastConfigurationEventByKeyAsync_ValidMessage_ReturnsDeserializedConfigurationEvent()
        {
            // Arrange
            var key = "validSlotKey";
            var topic = KAFKA_CONFIGURATION_TOPIC.Replace("{id}", key);
            var message = JsonSerializer.Serialize(new ConfigurationEvent(key, TimeSpan.FromSeconds(60)));
            var cancellationToken = CancellationToken.None;
            mockMessageConsumer.Setup(x => x.ReadLastTopicMessageAsync(topic, KAFKA_TIMEOUT_IN_MILLISECONDS, cancellationToken)).ReturnsAsync(message);
            // Act
            var result = await messageReceiver.ReceiveLastConfigurationEventByKeyAsync(key, cancellationToken);
            // Assert
            Assert.IsInstanceOf<ConfigurationEvent>(result);
            Assert.That(result.Key, Is.EqualTo(key));
            Assert.That(result.ServerKeepAliveInterval.TotalSeconds, Is.EqualTo(60));
        }
        [Test]
        public async Task ReceiveLastLoadEventByKeyAsync_ValidMessage_ReturnsDeserializedLoadEvent()
        {
            // Arrange
            var key = "validSlotKey";
            var topic = KAFKA_LOAD_TOPIC.Replace("{id}", key);
            var message = JsonSerializer.Serialize(new LoadEvent(key, "", "", 200, TimeSpan.Zero, DateTime.MinValue));
            var cancellationToken = CancellationToken.None;
            mockMessageConsumer.Setup(x => x.ReadLastTopicMessageAsync(topic, KAFKA_TIMEOUT_IN_MILLISECONDS, cancellationToken)).ReturnsAsync(message);
            // Act
            var result = await messageReceiver.ReceiveLastLoadEventByKeyAsync(key, cancellationToken);
            // Assert
            Assert.IsInstanceOf<LoadEvent>(result);
            Assert.That(result.Key, Is.EqualTo(key));
            Assert.That(result.Timestamp, Is.EqualTo(DateTime.MinValue));
        }
        [Test]
        public async Task ReceiveLoadEventAmountByKeyAsync_ReturnsCorrectAmount()
        {
            // Arrange
            var key = "validSlotKey";
            var topic = KAFKA_LOAD_TOPIC.Replace("{id}", key);
            var expectedAmount = 5;
            var cancellationToken = CancellationToken.None;
            mockMessageConsumer.Setup(x => x.GetAmountTopicMessagesAsync(topic, KAFKA_TIMEOUT_IN_MILLISECONDS, cancellationToken)).ReturnsAsync(expectedAmount);
            // Act
            var amount = await messageReceiver.ReceiveLoadEventAmountByKeyAsync(key, cancellationToken);
            // Assert
            Assert.That(amount, Is.EqualTo(expectedAmount));
        }
        [Test]
        public async Task ReceiveLastPulseEventByKeyAsync_NoMessage_ReturnsNull()
        {
            // Arrange
            var key = "validSlotKey";
            var topic = KAFKA_PULSE_TOPIC.Replace("{id}", key);
            var cancellationToken = CancellationToken.None;
            mockMessageConsumer.Setup(x => x.ReadLastTopicMessageAsync(topic, KAFKA_TIMEOUT_IN_MILLISECONDS, cancellationToken)).ReturnsAsync((string?)null);
            // Act
            var result = await messageReceiver.ReceiveLastPulseEventByKeyAsync(key, cancellationToken);
            // Assert
            Assert.That(result, Is.Null);
        }
        [Test]
        public async Task ReceiveLastPulseEventByKeyAsync_EmptyMessage_ReturnsNull()
        {
            // Arrange
            var key = "validSlotKey";
            var topic = KAFKA_PULSE_TOPIC.Replace("{id}", key);
            var cancellationToken = CancellationToken.None;
            mockMessageConsumer.Setup(x => x.ReadLastTopicMessageAsync(topic, KAFKA_TIMEOUT_IN_MILLISECONDS, cancellationToken)).ReturnsAsync(string.Empty);
            // Act
            var result = await messageReceiver.ReceiveLastPulseEventByKeyAsync(key, cancellationToken);
            // Assert
            Assert.That(result, Is.Null);
        }
        [Test]
        public async Task ReceiveLastPulseEventByKeyAsync_ChecksCorrectTopicAndTimeout()
        {
            // Arrange
            var key = "validSlotKey";
            var topic = KAFKA_PULSE_TOPIC.Replace("{id}", key);
            var message = JsonSerializer.Serialize(new PulseEvent(key, true));
            var cancellationToken = CancellationToken.None;
            mockMessageConsumer.Setup(x => x.ReadLastTopicMessageAsync(topic, KAFKA_TIMEOUT_IN_MILLISECONDS, cancellationToken)).ReturnsAsync(message);
            // Act
            await messageReceiver.ReceiveLastPulseEventByKeyAsync(key, cancellationToken);
            // Assert
            mockMessageConsumer.Verify(x => x.ReadLastTopicMessageAsync(
                topic,
                KAFKA_TIMEOUT_IN_MILLISECONDS,
                cancellationToken
            ), Times.Once);
        }
        private static async IAsyncEnumerable<string> AsyncEnumerable(IEnumerable<string> items)
        {
            foreach (var item in items)
            {
                yield return item;
                await Task.Yield();
            }
        }
    }
}
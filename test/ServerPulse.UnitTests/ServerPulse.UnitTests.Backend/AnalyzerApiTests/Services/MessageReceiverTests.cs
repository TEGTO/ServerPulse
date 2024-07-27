using AnalyzerApi;
using AnalyzerApi.Services;
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
        private const int KAFKA_TIMEOUT_IN_MILLISECONDS = 5000;

        private Mock<IMessageConsumer> mockMessageConsumer;
        private Mock<IConfiguration> mockConfiguration;
        private MessageReceiver messageReceiver;

        [SetUp]
        public void Setup()
        {
            mockMessageConsumer = new Mock<IMessageConsumer>();
            mockConfiguration = new Mock<IConfiguration>();
            mockConfiguration.SetupGet(c => c[Configuration.KAFKA_ALIVE_TOPIC]).Returns(KAFKA_PULSE_TOPIC);
            mockConfiguration.SetupGet(c => c[Configuration.KAFKA_TIMEOUT_IN_MILLISECONDS]).Returns(KAFKA_TIMEOUT_IN_MILLISECONDS.ToString());
            messageReceiver = new MessageReceiver(mockMessageConsumer.Object, mockConfiguration.Object);
        }
        [Test]
        public async Task ReceiveLastPulseEventByKeyAsync_ValidMessage_ReturnsDeserializedAliveEvent()
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
        public async Task ReceiveLastPulseEventByKeyAsync_NoMessage_ReturnsDefaultAliveEvent()
        {
            // Arrange
            var key = "validSlotKey";
            var topic = KAFKA_PULSE_TOPIC.Replace("{id}", key);
            var cancellationToken = CancellationToken.None;
            mockMessageConsumer.Setup(x => x.ReadLastTopicMessageAsync(topic, KAFKA_TIMEOUT_IN_MILLISECONDS, cancellationToken)).ReturnsAsync((string?)null);
            // Act
            var result = await messageReceiver.ReceiveLastPulseEventByKeyAsync(key, cancellationToken);
            // Assert
            Assert.IsInstanceOf<PulseEvent>(result);
            Assert.That(result.Key, Is.EqualTo(key));
            Assert.That(result.IsAlive, Is.EqualTo(false));
        }
        [Test]
        public async Task ReceiveLastPulseEventByKeyAsync_EmptyMessage_ReturnsDefaultAliveEvent()
        {
            // Arrange
            var key = "validSlotKey";
            var topic = KAFKA_PULSE_TOPIC.Replace("{id}", key);
            var cancellationToken = CancellationToken.None;
            mockMessageConsumer.Setup(x => x.ReadLastTopicMessageAsync(topic, KAFKA_TIMEOUT_IN_MILLISECONDS, cancellationToken)).ReturnsAsync(string.Empty);
            // Act
            var result = await messageReceiver.ReceiveLastPulseEventByKeyAsync(key, cancellationToken);
            // Assert
            Assert.IsInstanceOf<PulseEvent>(result);
            Assert.That(result.Key, Is.EqualTo(key));
            Assert.That(result.IsAlive, Is.EqualTo(false));
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
    }
}
using MessageBus;
using Microsoft.Extensions.Configuration;
using Moq;
using ServerMonitorApi;
using ServerMonitorApi.Services;
using ServerPulse.EventCommunication.Events;

namespace ServerMonitorApiTests.Services
{
    [TestFixture]
    internal class MessageSenderTests
    {
        private const string KAFKA_ALIVE_TOPIC = "KafkaAliveTopic_{id}";
        private const string KAFKA_CONFIGURATION_TOPIC = "KafkaConfigurationTopic_{id}";
        private const string KAFKA_LOAD_TOPIC = "KafkaLoadTopic_{id}";
        private const int KAFKA_PARTITIONS_AMOUNT = 3;

        private Mock<IMessageProducer> mockProducer;
        private Mock<IConfiguration> mockConfiguration;
        private MessageSender messageSender;

        [SetUp]
        public void Setup()
        {
            mockProducer = new Mock<IMessageProducer>();
            mockConfiguration = new Mock<IConfiguration>();
            mockConfiguration.SetupGet(c => c[Configuration.KAFKA_ALIVE_TOPIC]).Returns(KAFKA_ALIVE_TOPIC);
            mockConfiguration.SetupGet(c => c[Configuration.KAFKA_CONFIGURATION_TOPIC]).Returns(KAFKA_CONFIGURATION_TOPIC);
            mockConfiguration.SetupGet(c => c[Configuration.KAFKA_LOAD_TOPIC]).Returns(KAFKA_LOAD_TOPIC);
            mockConfiguration.SetupGet(c => c[Configuration.KAFKA_PARTITIONS_AMOUNT]).Returns(KAFKA_PARTITIONS_AMOUNT.ToString());
            messageSender = new MessageSender(mockProducer.Object, mockConfiguration.Object);
        }

        [Test]
        public async Task SendPulseEventAsync_CreatesCorrectTopicAndMessage()
        {
            // Arrange
            var key = "slot123";
            var expectedTopic = KAFKA_ALIVE_TOPIC.Replace("{id}", key);
            var aliveEvent = new PulseEvent(key, true);
            var cancellationToken = CancellationToken.None;
            // Act
            await messageSender.SendPulseEventAsync(aliveEvent, cancellationToken);
            // Assert
            mockProducer.Verify(x => x.ProduceAsync(
                expectedTopic,
                It.IsAny<string>(),
                KAFKA_PARTITIONS_AMOUNT,
                cancellationToken
            ), Times.Once);
        }
        [Test]
        public async Task SendPulseEventAsync_SerializesPulseEventCorrectly()
        {
            // Arrange
            var key = "slot123";
            var aliveEvent = new PulseEvent(key, true);
            var expectedMessage = aliveEvent.ToString();
            var cancellationToken = CancellationToken.None;
            // Act
            await messageSender.SendPulseEventAsync(aliveEvent, cancellationToken);
            // Assert
            mockProducer.Verify(x => x.ProduceAsync(
                It.IsAny<string>(),
                expectedMessage,
                It.IsAny<int>(),
                cancellationToken
            ), Times.Once);
        }
        [Test]
        public async Task SendPulseEventAsync_UsesCorrectPartitionAmount()
        {
            // Arrange
            var key = "slot123";
            var aliveEvent = new PulseEvent(key, true);
            var cancellationToken = CancellationToken.None;
            // Act
            await messageSender.SendPulseEventAsync(aliveEvent, cancellationToken);
            // Assert
            mockProducer.Verify(x => x.ProduceAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                KAFKA_PARTITIONS_AMOUNT,
                cancellationToken
            ), Times.Once);
        }
        [Test]
        public async Task SendConfigurationEventAsync_CreatesCorrectTopicAndMessage()
        {
            // Arrange
            var key = "config123";
            var expectedTopic = KAFKA_CONFIGURATION_TOPIC.Replace("{id}", key);
            var configurationEvent = new ConfigurationEvent(key, TimeSpan.FromMinutes(5));
            var cancellationToken = CancellationToken.None;
            // Act
            await messageSender.SendConfigurationEventAsync(configurationEvent, cancellationToken);
            // Assert
            mockProducer.Verify(x => x.ProduceAsync(
                expectedTopic,
                It.IsAny<string>(),
                KAFKA_PARTITIONS_AMOUNT,
                cancellationToken
            ), Times.Once);
        }
        [Test]
        public async Task SendConfigurationEventAsync_SerializesConfigurationEventCorrectly()
        {
            // Arrange
            var key = "config123";
            var configurationEvent = new ConfigurationEvent(key, TimeSpan.FromMinutes(5));
            var expectedMessage = configurationEvent.ToString();
            var cancellationToken = CancellationToken.None;
            // Act
            await messageSender.SendConfigurationEventAsync(configurationEvent, cancellationToken);
            // Assert
            mockProducer.Verify(x => x.ProduceAsync(
                It.IsAny<string>(),
                expectedMessage,
                It.IsAny<int>(),
                cancellationToken
            ), Times.Once);
        }
        [Test]
        public async Task SendConfigurationEventAsync_UsesCorrectPartitionAmount()
        {
            // Arrange
            var key = "config123";
            var configurationEvent = new ConfigurationEvent(key, TimeSpan.FromMinutes(5));
            var cancellationToken = CancellationToken.None;
            // Act
            await messageSender.SendConfigurationEventAsync(configurationEvent, cancellationToken);
            // Assert
            mockProducer.Verify(x => x.ProduceAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                KAFKA_PARTITIONS_AMOUNT,
                cancellationToken
            ), Times.Once);
        }
        [Test]
        public async Task SendLoadEventsAsync_CreatesCorrectTopicsAndMessages()
        {
            // Arrange
            var loadEvents = new[]
            {
                new LoadEvent("load1", "endpoint1", "GET", 200, TimeSpan.FromSeconds(10), DateTime.Now),
                new LoadEvent("load2", "endpoint2", "POST", 404, TimeSpan.FromSeconds(20), DateTime.Now)
            };
            var cancellationToken = CancellationToken.None;
            // Act
            await messageSender.SendLoadEventsAsync(loadEvents, cancellationToken);
            // Assert
            foreach (var loadEvent in loadEvents)
            {
                var expectedTopic = KAFKA_LOAD_TOPIC.Replace("{id}", loadEvent.Key);
                var expectedMessage = loadEvent.ToString();
                mockProducer.Verify(x => x.ProduceAsync(
                    expectedTopic,
                    expectedMessage,
                    KAFKA_PARTITIONS_AMOUNT,
                    It.IsAny<CancellationToken>()
                ), Times.Once);
            }
        }
        [Test]
        public async Task SendLoadEventsAsync_HandlesParallelExecution()
        {
            // Arrange
            var loadEvents = new[]
            {
                new LoadEvent("load1", "endpoint1", "GET", 200, TimeSpan.FromSeconds(10), DateTime.Now),
                new LoadEvent("load2", "endpoint2", "POST", 404, TimeSpan.FromSeconds(20), DateTime.Now)
            };
            var cancellationToken = CancellationToken.None;
            // Act
            await messageSender.SendLoadEventsAsync(loadEvents, cancellationToken);
            // Assert
            mockProducer.Verify(x => x.ProduceAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()
            ), Times.Exactly(loadEvents.Length));
        }
    }
}
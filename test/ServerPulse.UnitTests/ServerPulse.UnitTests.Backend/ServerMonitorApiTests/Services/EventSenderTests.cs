using MessageBus.Interfaces;
using Microsoft.Extensions.Configuration;
using Moq;
using ServerMonitorApi;
using ServerMonitorApi.Services;
using ServerPulse.EventCommunication.Events;

namespace ServerMonitorApiTests.Services
{
    [TestFixture]
    internal class EventSenderTests
    {
        private const string KAFKA_ALIVE_TOPIC = "KafkaAliveTopic_";
        private const string KAFKA_CONFIGURATION_TOPIC = "KafkaConfigurationTopic_";
        private const string KAFKA_LOAD_TOPIC = "KafkaLoadTopic_";
        private const string KAFKA_CUSTOM_TOPIC = "KafkaCustomTopic_";

        private Mock<IMessageProducer> mockProducer;
        private Mock<IConfiguration> mockConfiguration;
        private EventSender eventSender;

        [SetUp]
        public void Setup()
        {
            mockProducer = new Mock<IMessageProducer>();
            mockConfiguration = new Mock<IConfiguration>();
            mockConfiguration.SetupGet(c => c[Configuration.KAFKA_ALIVE_TOPIC]).Returns(KAFKA_ALIVE_TOPIC);
            mockConfiguration.SetupGet(c => c[Configuration.KAFKA_CONFIGURATION_TOPIC]).Returns(KAFKA_CONFIGURATION_TOPIC);
            mockConfiguration.SetupGet(c => c[Configuration.KAFKA_LOAD_TOPIC]).Returns(KAFKA_LOAD_TOPIC);
            mockConfiguration.SetupGet(c => c[Configuration.KAFKA_CUSTOM_TOPIC]).Returns(KAFKA_CUSTOM_TOPIC);
            eventSender = new EventSender(mockProducer.Object, mockConfiguration.Object);
        }

        [Test]
        public async Task SendCustomEventsAsync_SendsAllCustomEventsToCorrectTopic()
        {
            // Arrange
            var key = "customKey";
            var events = new[] { "event1", "event2", "event3" };
            var expectedTopic = KAFKA_CUSTOM_TOPIC + key;
            var cancellationToken = CancellationToken.None;
            // Act
            await eventSender.SendCustomEventsAsync(key, events, cancellationToken);
            // Assert
            foreach (var ev in events)
            {
                mockProducer.Verify(x => x.ProduceAsync(
                    expectedTopic,
                    ev,
                    cancellationToken
                ), Times.Once);
            }
        }
        [Test]
        public async Task SendEventsAsync_SendsPulseEventsToCorrectTopic()
        {
            // Arrange
            var events = new[]
            {
                new PulseEvent("key1", true),
                new PulseEvent("key2", false)
            };
            var cancellationToken = CancellationToken.None;
            // Act
            await eventSender.SendEventsAsync(events, cancellationToken);
            // Assert
            foreach (var ev in events)
            {
                var expectedTopic = KAFKA_ALIVE_TOPIC + ev.Key;
                mockProducer.Verify(x => x.ProduceAsync(
                    expectedTopic,
                    It.Is<string>(x => x.Contains(ev.Id)),
                    It.IsAny<CancellationToken>()
                ), Times.Exactly(1));
            }
        }
        [Test]
        public async Task SendEventsAsync_SendsConfigurationEventsToCorrectTopic()
        {
            // Arrange
            var events = new[]
            {
                new ConfigurationEvent("key1", TimeSpan.FromMinutes(5)),
                new ConfigurationEvent("key2", TimeSpan.FromMinutes(10))
            };
            var cancellationToken = CancellationToken.None;
            // Act
            await eventSender.SendEventsAsync(events, cancellationToken);
            // Assert
            foreach (var ev in events)
            {
                var expectedTopic = KAFKA_CONFIGURATION_TOPIC + ev.Key;
                mockProducer.Verify(x => x.ProduceAsync(
                    expectedTopic,
                    It.Is<string>(x => x.Contains(ev.Id)),
                    It.IsAny<CancellationToken>()
                ), Times.Exactly(1));
            }
        }
        [Test]
        public async Task SendEventsAsync_SendsLoadEventsToCorrectTopic()
        {
            // Arrange
            var events = new[]
            {
                new LoadEvent("key1", "endpoint1", "GET", 200, TimeSpan.FromSeconds(10), DateTime.Now),
                new LoadEvent("key2", "endpoint2", "POST", 404, TimeSpan.FromSeconds(20), DateTime.Now)
            };
            var cancellationToken = CancellationToken.None;
            // Act
            await eventSender.SendEventsAsync(events, cancellationToken);
            // Assert
            foreach (var ev in events)
            {
                var expectedTopic = KAFKA_LOAD_TOPIC + ev.Key;
                mockProducer.Verify(x => x.ProduceAsync(
                    expectedTopic,
                    It.Is<string>(x => x.Contains(ev.Id)),
                    It.IsAny<CancellationToken>()
                ), Times.Exactly(1));
            }
        }
        [Test]
        public async Task SendEventsAsync_HandlesParallelExecution()
        {
            // Arrange
            var events = new BaseEvent[]
            {
                new LoadEvent("key1", "endpoint1", "GET", 200, TimeSpan.FromSeconds(10), DateTime.Now),
                new PulseEvent("key2", true),
                new ConfigurationEvent("key3", TimeSpan.FromMinutes(5))
            };
            var cancellationToken = CancellationToken.None;
            // Act
            await eventSender.SendEventsAsync(events, cancellationToken);
            // Assert
            mockProducer.Verify(x => x.ProduceAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()
            ), Times.Exactly(events.Length));
        }
    }
}
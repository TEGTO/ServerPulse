using AnalyzerApi;
using AnalyzerApi.Domain.Dtos.Wrappers;
using AnalyzerApi.Domain.Models;
using AnalyzerApi.Services;
using AutoMapper;
using Confluent.Kafka;
using MessageBus.Interfaces;
using Microsoft.Extensions.Configuration;
using Moq;
using ServerPulse.EventCommunication.Events;
using System.Text.Json;

namespace AnalyzerApiTests.Services
{
    [TestFixture]
    internal class ServerStatusReceiverTests
    {
        private const string KAFKA_ALIVE_TOPIC = "KafkaPulseTopic_";
        private const string KAFKA_CONFIGURATION_TOPIC = "KafkaConfigurationTopic_";
        private const string KAFKA_SERVER_STATISTICS_TOPIC = "KafkaServerStatisticsTopic_";
        private const int KAFKA_TIMEOUT_IN_MILLISECONDS = 5000;

        private Mock<IMessageConsumer> mockMessageConsumer;
        private Mock<IMapper> mockMapper;
        private Mock<IConfiguration> mockConfiguration;
        private ServerStatusReceiver serverStatusReceiver;

        [SetUp]
        public void Setup()
        {
            mockMessageConsumer = new Mock<IMessageConsumer>();
            mockMapper = new Mock<IMapper>();
            mockConfiguration = new Mock<IConfiguration>();

            mockConfiguration.SetupGet(c => c[Configuration.KAFKA_ALIVE_TOPIC]).Returns(KAFKA_ALIVE_TOPIC);
            mockConfiguration.SetupGet(c => c[Configuration.KAFKA_CONFIGURATION_TOPIC]).Returns(KAFKA_CONFIGURATION_TOPIC);
            mockConfiguration.SetupGet(c => c[Configuration.KAFKA_SERVER_STATISTICS_TOPIC]).Returns(KAFKA_SERVER_STATISTICS_TOPIC);
            mockConfiguration.SetupGet(c => c[Configuration.KAFKA_TIMEOUT_IN_MILLISECONDS]).Returns(KAFKA_TIMEOUT_IN_MILLISECONDS.ToString());

            serverStatusReceiver = new ServerStatusReceiver(mockMessageConsumer.Object, mockMapper.Object, mockConfiguration.Object);
        }

        [Test]
        public async Task ConsumePulseEventAsync_ValidMessages_YieldsPulseEvents()
        {
            // Arrange
            var key = "validSlotKey";
            var topic = KAFKA_ALIVE_TOPIC + key;
            var cancellationToken = CancellationToken.None;
            var pulseEvents = new List<string>
            {
                JsonSerializer.Serialize(new PulseEvent(key, true)),
                JsonSerializer.Serialize(new PulseEvent(key, false))
            };
            mockMessageConsumer
                .Setup(x => x.ConsumeAsync(topic, KAFKA_TIMEOUT_IN_MILLISECONDS, Offset.End, cancellationToken))
                .Returns(AsyncEnumerable(pulseEvents));
            mockMapper.Setup(m => m.Map<PulseEventWrapper>(It.IsAny<PulseEvent>()))
                      .Returns((PulseEvent pe) => new PulseEventWrapper { Key = pe.Key, IsAlive = pe.IsAlive });
            // Act
            var receivedEvents = new List<PulseEventWrapper>();
            await foreach (var pulseEvent in serverStatusReceiver.ConsumePulseEventAsync(key, cancellationToken))
            {
                receivedEvents.Add(pulseEvent);
            }
            // Assert
            Assert.That(receivedEvents.Count, Is.EqualTo(pulseEvents.Count));
            Assert.That(receivedEvents[0].IsAlive, Is.True);
            Assert.That(receivedEvents[1].IsAlive, Is.False);
        }
        [Test]
        public async Task ConsumeConfigurationEventAsync_ValidMessages_YieldsConfigurationEvents()
        {
            // Arrange
            var key = "validSlotKey";
            var topic = KAFKA_CONFIGURATION_TOPIC + key;
            var cancellationToken = CancellationToken.None;
            var configEvents = new List<string>
            {
                JsonSerializer.Serialize(new ConfigurationEvent(key, TimeSpan.Zero)),
                JsonSerializer.Serialize(new ConfigurationEvent(key, TimeSpan.FromHours(1)))
            };
            mockMessageConsumer
                .Setup(x => x.ConsumeAsync(topic, KAFKA_TIMEOUT_IN_MILLISECONDS, Offset.End, cancellationToken))
                .Returns(AsyncEnumerable(configEvents));
            mockMapper.Setup(m => m.Map<ConfigurationEventWrapper>(It.IsAny<ConfigurationEvent>()))
                      .Returns((ConfigurationEvent ce) => new ConfigurationEventWrapper { Key = ce.Key, ServerKeepAliveInterval = ce.ServerKeepAliveInterval });
            // Act
            var receivedEvents = new List<ConfigurationEventWrapper>();
            await foreach (var configEvent in serverStatusReceiver.ConsumeConfigurationEventAsync(key, cancellationToken))
            {
                receivedEvents.Add(configEvent);
            }
            // Assert
            Assert.That(receivedEvents.Count, Is.EqualTo(configEvents.Count));
            Assert.That(receivedEvents[0].ServerKeepAliveInterval, Is.EqualTo(TimeSpan.Zero));
            Assert.That(receivedEvents[1].ServerKeepAliveInterval, Is.EqualTo(TimeSpan.FromHours(1)));
        }

        [Test]
        public async Task ReceiveLastServerStatisticsByKeyAsync_ValidMessage_ReturnsDeserializedStatistics()
        {
            // Arrange
            var key = "validSlotKey";
            var topic = KAFKA_SERVER_STATISTICS_TOPIC + key;
            var message = JsonSerializer.Serialize(new ServerStatistics { IsAlive = true });
            var cancellationToken = CancellationToken.None;
            mockMessageConsumer.Setup(x => x.ReadLastTopicMessageAsync(topic, KAFKA_TIMEOUT_IN_MILLISECONDS, cancellationToken))
                               .ReturnsAsync(new ConsumeResponse(message, DateTime.MinValue));
            // Act
            var result = await serverStatusReceiver.ReceiveLastServerStatisticsByKeyAsync(key, cancellationToken);
            // Assert
            Assert.IsNotNull(result);
            Assert.That(result?.IsAlive, Is.True);
        }
        [Test]
        public async Task ReceiveLastPulseEventByKeyAsync_ValidMessage_ReturnsDeserializedPulseEvent()
        {
            // Arrange
            var key = "validSlotKey";
            var topic = KAFKA_ALIVE_TOPIC + key;
            var message = JsonSerializer.Serialize(new PulseEvent(key, true));
            var cancellationToken = CancellationToken.None;

            mockMessageConsumer.Setup(x => x.ReadLastTopicMessageAsync(topic, KAFKA_TIMEOUT_IN_MILLISECONDS, cancellationToken))
                               .ReturnsAsync(new ConsumeResponse(message, DateTime.MinValue));
            mockMapper.Setup(m => m.Map<PulseEventWrapper>(It.IsAny<PulseEvent>()))
                      .Returns((PulseEvent pe) => new PulseEventWrapper { Key = pe.Key, IsAlive = pe.IsAlive });
            // Act
            var result = await serverStatusReceiver.ReceiveLastPulseEventByKeyAsync(key, cancellationToken);
            // Assert
            Assert.IsNotNull(result);
            Assert.That(result?.Key, Is.EqualTo(key));
            Assert.That(result?.IsAlive, Is.True);
        }
        [Test]
        public async Task ReceiveLastConfigurationEventByKeyAsync_ValidMessage_ReturnsDeserializedConfigurationEvent()
        {
            // Arrange
            var key = "validSlotKey";
            var topic = KAFKA_CONFIGURATION_TOPIC + key;
            var message = JsonSerializer.Serialize(new ConfigurationEvent(key, TimeSpan.FromSeconds(60)));
            var cancellationToken = CancellationToken.None;
            mockMessageConsumer.Setup(x => x.ReadLastTopicMessageAsync(topic, KAFKA_TIMEOUT_IN_MILLISECONDS, cancellationToken))
                               .ReturnsAsync(new ConsumeResponse(message, DateTime.MinValue));
            mockMapper.Setup(m => m.Map<ConfigurationEventWrapper>(It.IsAny<ConfigurationEvent>()))
                      .Returns((ConfigurationEvent ce) => new ConfigurationEventWrapper { Key = ce.Key, ServerKeepAliveInterval = ce.ServerKeepAliveInterval });
            // Act
            var result = await serverStatusReceiver.ReceiveLastConfigurationEventByKeyAsync(key, cancellationToken);
            // Assert
            Assert.IsNotNull(result);
            Assert.That(result?.Key, Is.EqualTo(key));
            Assert.That(result?.ServerKeepAliveInterval.TotalSeconds, Is.EqualTo(60));
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
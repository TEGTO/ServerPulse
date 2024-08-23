using AnalyzerApi.Domain.Models;
using AnalyzerApi.Services.Interfaces;
using MessageBus.Interfaces;
using Microsoft.Extensions.Configuration;
using Moq;
using ServerPulse.EventCommunication.Events;

namespace AnalyzerApi.Services
{
    [TestFixture]
    internal class EventProcessorTests
    {
        private Mock<IMessageProducer> mockProducer;
        private Mock<IServerLoadReceiver> mockServerLoadReceiver;
        private Mock<IConfiguration> mockConfiguration;
        private EventProcessor eventProcessor;

        private const string KAFKA_LOAD_METHOD_STATISTICS_TOPIC = "KafkaLoadMethodStatisticsTopic_";

        [SetUp]
        public void Setup()
        {
            mockProducer = new Mock<IMessageProducer>();
            mockServerLoadReceiver = new Mock<IServerLoadReceiver>();
            mockConfiguration = new Mock<IConfiguration>();
            mockConfiguration.SetupGet(c => c[Configuration.KAFKA_LOAD_METHOD_STATISTICS_TOPIC]).Returns(KAFKA_LOAD_METHOD_STATISTICS_TOPIC);
            eventProcessor = new EventProcessor(mockProducer.Object, mockServerLoadReceiver.Object, mockConfiguration.Object);
        }

        #region ProcessEventsAsync Tests

        [Test]
        public void ProcessEventsAsync_NullEvents_ThrowsInvalidDataException()
        {
            // Arrange
            var cancellationToken = CancellationToken.None;
            // Act & Assert
            Assert.ThrowsAsync<InvalidDataException>(() => eventProcessor.ProcessEventsAsync<LoadEvent>(null, cancellationToken));
        }
        [Test]
        public void ProcessEventsAsync_EmptyEvents_ThrowsInvalidDataException()
        {
            // Arrange
            var cancellationToken = CancellationToken.None;
            var events = Array.Empty<LoadEvent>();
            // Act & Assert
            Assert.ThrowsAsync<InvalidDataException>(() => eventProcessor.ProcessEventsAsync(events, cancellationToken));
        }
        [Test]
        public void ProcessEventsAsync_DifferentKeys_ThrowsInvalidDataException()
        {
            // Arrange
            var cancellationToken = CancellationToken.None;
            var events = new[]
            {
                new LoadEvent("key1", "endpoint1", "GET", 200, TimeSpan.FromSeconds(10), DateTime.Now),
                new LoadEvent("key2", "endpoint2", "POST", 404, TimeSpan.FromSeconds(20), DateTime.Now)
            };
            // Act & Assert
            Assert.ThrowsAsync<InvalidDataException>(() => eventProcessor.ProcessEventsAsync(events, cancellationToken));
        }
        [Test]
        public async Task ProcessEventsAsync_LoadEvents_ProcessesCorrectly()
        {
            // Arrange
            var cancellationToken = CancellationToken.None;
            var key = "validKey";
            var events = new[]
            {
                new LoadEvent(key, "endpoint1", "GET", 200, TimeSpan.FromSeconds(10), DateTime.Now),
                new LoadEvent(key, "endpoint2", "POST", 404, TimeSpan.FromSeconds(20), DateTime.Now)
            };
            var statistics = new LoadMethodStatistics { GetAmount = 1, PostAmount = 2 };
            mockServerLoadReceiver.Setup(x => x.ReceiveLastLoadMethodStatisticsByKeyAsync(key, cancellationToken))
                                  .ReturnsAsync(statistics);
            // Act
            await eventProcessor.ProcessEventsAsync(events, cancellationToken);
            // Assert
            mockServerLoadReceiver.Verify(x => x.ReceiveLastLoadMethodStatisticsByKeyAsync(key, cancellationToken), Times.Once);
            mockProducer.Verify(x => x.ProduceAsync(KAFKA_LOAD_METHOD_STATISTICS_TOPIC + key, It.IsAny<string>(), cancellationToken), Times.Once);
            mockProducer.Verify(x => x.ProduceAsync(It.IsAny<string>(), It.Is<string>(x => x.Contains("{\"GetAmount\":2,\"PostAmount\":3")), cancellationToken), Times.Once);
        }

        #endregion

        #region ProcessLoadEventsAsync Tests

        [Test]
        public async Task ProcessLoadEventsAsync_StatisticsExists_UpdatesStatisticsCorrectly()
        {
            // Arrange
            var cancellationToken = CancellationToken.None;
            var key = "validKey";
            var events = new[]
            {
                new LoadEvent(key, "endpoint1", "GET", 200, TimeSpan.FromSeconds(10), DateTime.Now),
                new LoadEvent(key, "endpoint2", "POST", 404, TimeSpan.FromSeconds(20), DateTime.Now)
            };
            var statistics = new LoadMethodStatistics { GetAmount = 1, PostAmount = 1 };
            mockServerLoadReceiver.Setup(x => x.ReceiveLastLoadMethodStatisticsByKeyAsync(key, cancellationToken))
                                  .ReturnsAsync(statistics);
            // Act
            await eventProcessor.ProcessEventsAsync(events, cancellationToken);
            // Assert
            mockProducer.Verify(x => x.ProduceAsync(KAFKA_LOAD_METHOD_STATISTICS_TOPIC + key, It.Is<string>(x => x.Contains("{\"GetAmount\":2,\"PostAmount\":2")), cancellationToken), Times.Once);
        }
        [Test]
        public async Task ProcessLoadEventsAsync_StatisticsDoesNotExist_InitializesAndUpdatesStatistics()
        {
            // Arrange
            var cancellationToken = CancellationToken.None;
            var key = "validKey";
            var events = new[]
            {
                new LoadEvent(key, "endpoint1", "GET", 200, TimeSpan.FromSeconds(10), DateTime.Now),
                new LoadEvent(key, "endpoint2", "POST", 404, TimeSpan.FromSeconds(20), DateTime.Now)
            };

            mockServerLoadReceiver.Setup(x => x.ReceiveLastLoadMethodStatisticsByKeyAsync(key, cancellationToken))
                                  .ReturnsAsync((LoadMethodStatistics)null);
            // Act
            await eventProcessor.ProcessEventsAsync(events, cancellationToken);
            // Assert
            mockProducer.Verify(x => x.ProduceAsync(KAFKA_LOAD_METHOD_STATISTICS_TOPIC + key, It.Is<string>(x => x.Contains("{\"GetAmount\":1,\"PostAmount\":1")), cancellationToken), Times.Once);
        }

        #endregion
    }
}
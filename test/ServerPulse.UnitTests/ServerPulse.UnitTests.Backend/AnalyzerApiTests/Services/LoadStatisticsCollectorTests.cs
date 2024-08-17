using AnalyzerApi.Domain.Dtos.Wrappers;
using AnalyzerApi.Domain.Models;
using AnalyzerApi.Services;
using AnalyzerApi.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;

namespace AnalyzerApiTests.Services
{
    [TestFixture]
    internal class LoadStatisticsCollectorTests
    {
        private Mock<IServerLoadReceiver> mockLoadReceiver;
        private Mock<IStatisticsSender> mockStatisticsSender;
        private Mock<ILogger<LoadStatisticsCollector>> mockLogger;
        private LoadStatisticsCollector loadStatisticsCollector;

        [SetUp]
        public void Setup()
        {
            mockLoadReceiver = new Mock<IServerLoadReceiver>();
            mockStatisticsSender = new Mock<IStatisticsSender>();
            mockLogger = new Mock<ILogger<LoadStatisticsCollector>>();

            loadStatisticsCollector = new LoadStatisticsCollector(
                mockLoadReceiver.Object,
                mockStatisticsSender.Object,
                mockLogger.Object
            );
        }

        [Test]
        public async Task StartConsumingStatistics_ShouldAddListenerAndSendOnlyInitialStatistics()
        {
            // Arrange
            var key = "testKey";
            mockLoadReceiver.Setup(m => m.ReceiveLoadEventAmountByKeyAsync(key, It.IsAny<CancellationToken>()))
                            .ReturnsAsync(100);
            mockLoadReceiver.Setup(m => m.ReceiveLastLoadEventByKeyAsync(key, It.IsAny<CancellationToken>()))
                            .ReturnsAsync(new LoadEventWrapper());
            // Act
            loadStatisticsCollector.StartConsumingStatistics(key);
            await Task.Delay(500);
            loadStatisticsCollector.StopConsumingStatistics(key);
            // Assert
            mockStatisticsSender.Verify(m => m.SendServerLoadStatisticsAsync(It.IsAny<string>(), It.IsAny<ServerLoadStatistics>(), It.IsAny<CancellationToken>()), Times.Once);
        }
        [Test]
        public async Task StartConsumingStatistics_ShouldSendMoreThanOneStatistics()
        {
            // Arrange
            var key = "testKey";
            mockLoadReceiver.Setup(m => m.ReceiveLoadEventAmountByKeyAsync(key, It.IsAny<CancellationToken>()))
                            .ReturnsAsync(100);
            mockLoadReceiver.Setup(m => m.ReceiveLastLoadEventByKeyAsync(key, It.IsAny<CancellationToken>()))
                            .ReturnsAsync(new LoadEventWrapper());
            mockLoadReceiver.Setup(m => m.ConsumeLoadEventAsync(key, It.IsAny<CancellationToken>()))
                            .Returns(AsyncEnumerable(new List<LoadEventWrapper> { new LoadEventWrapper() }));
            // Act
            loadStatisticsCollector.StartConsumingStatistics(key);
            await Task.Delay(2000);
            loadStatisticsCollector.StopConsumingStatistics(key);
            // Assert
            mockStatisticsSender.Verify(m => m.SendServerLoadStatisticsAsync(It.IsAny<string>(), It.IsAny<ServerLoadStatistics>(), It.IsAny<CancellationToken>()), Times.AtLeast(2));
        }
        [Test]
        public async Task StopConsumingStatistics_ShouldStopSendingStatistics()
        {
            // Arrange
            var key = "testKey";
            mockLoadReceiver.Setup(m => m.ReceiveLoadEventAmountByKeyAsync(key, It.IsAny<CancellationToken>()))
                            .ReturnsAsync(100);
            mockLoadReceiver.Setup(m => m.ReceiveLastLoadEventByKeyAsync(key, It.IsAny<CancellationToken>()))
                            .ReturnsAsync(new LoadEventWrapper());
            // Act
            loadStatisticsCollector.StartConsumingStatistics(key);
            await Task.Delay(1500);
            loadStatisticsCollector.StopConsumingStatistics(key);
            await Task.Delay(1500);
            // Assert
            mockStatisticsSender.Verify(m => m.SendServerLoadStatisticsAsync(It.IsAny<string>(), It.IsAny<ServerLoadStatistics>(), It.IsAny<CancellationToken>()), Times.AtMost(2));
        }
        [Test]
        public async Task SubscribeToPulseEventsAsync_ShouldSendStatisticsOnNewEvent()
        {
            // Arrange
            var key = "testKey";
            var loadEvent = new LoadEventWrapper();
            mockLoadReceiver.Setup(m => m.ConsumeLoadEventAsync(key, It.IsAny<CancellationToken>()))
                            .Returns(AsyncEnumerable(new List<LoadEventWrapper> { loadEvent }));
            mockLoadReceiver.Setup(m => m.ReceiveLoadEventAmountByKeyAsync(key, It.IsAny<CancellationToken>()))
                            .ReturnsAsync(100);
            // Act
            loadStatisticsCollector.StartConsumingStatistics(key);
            await Task.Delay(2000);
            loadStatisticsCollector.StopConsumingStatistics(key);
            // Assert
            mockStatisticsSender.Verify(m => m.SendServerLoadStatisticsAsync(It.IsAny<string>(), It.Is<ServerLoadStatistics>(s => s.LastEvent == loadEvent), It.IsAny<CancellationToken>()), Times.AtLeastOnce);
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
}
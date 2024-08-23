using AnalyzerApi.Domain.Dtos.Wrappers;
using AnalyzerApi.Domain.Models;
using AnalyzerApi.Services;
using AnalyzerApi.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;

namespace AnalyzerApiTests.Services
{
    [TestFixture]
    internal class LoadStatisticsCollectorTests : BaseStatisticsCollectorTests
    {
        private Mock<IServerLoadReceiver> mockLoadReceiver;
        private Mock<ILogger<LoadStatisticsCollector>> mockLogger;
        private LoadStatisticsCollector collector;

        [SetUp]
        public override void Setup()
        {
            base.Setup();

            mockLoadReceiver = new Mock<IServerLoadReceiver>();
            mockLogger = new Mock<ILogger<LoadStatisticsCollector>>();

            collector = new LoadStatisticsCollector(
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
            collector.StartConsumingStatistics(key);
            await Task.Delay(500);
            collector.StopConsumingStatistics(key);
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
            collector.StartConsumingStatistics(key);
            await Task.Delay(2000);
            collector.StopConsumingStatistics(key);
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
            collector.StartConsumingStatistics(key);
            await Task.Delay(1500);
            collector.StopConsumingStatistics(key);
            await Task.Delay(1500);
            // Assert
            mockStatisticsSender.Verify(m => m.SendServerLoadStatisticsAsync(It.IsAny<string>(), It.IsAny<ServerLoadStatistics>(), It.IsAny<CancellationToken>()), Times.AtMost(2));
        }
        [Test]
        public async Task SubscribeToLoadEventsAsync_ShouldSendStatisticsOnNewEvent()
        {
            // Arrange
            var key = "testKey";
            var loadEvent = new LoadEventWrapper();
            mockLoadReceiver.Setup(m => m.ConsumeLoadEventAsync(key, It.IsAny<CancellationToken>()))
                            .Returns(AsyncEnumerable(new List<LoadEventWrapper> { loadEvent }));
            mockLoadReceiver.Setup(m => m.ReceiveLoadEventAmountByKeyAsync(key, It.IsAny<CancellationToken>()))
                            .ReturnsAsync(100);
            // Act
            collector.StartConsumingStatistics(key);
            await Task.Delay(2000);
            collector.StopConsumingStatistics(key);
            // Assert
            mockStatisticsSender.Verify(m => m.SendServerLoadStatisticsAsync(It.IsAny<string>(), It.Is<ServerLoadStatistics>(s => s.LastEvent == loadEvent), It.IsAny<CancellationToken>()), Times.AtLeastOnce);
        }
    }
}
using AnalyzerApi.Domain.Dtos.Wrappers;
using AnalyzerApi.Domain.Models;
using AnalyzerApi.Services.Consumers;
using AnalyzerApi.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;

namespace AnalyzerApiTests.Services.Collector
{
    [TestFixture]
    internal class LoadStatisticsCollectorTests : BaseStatisticsCollectorTests
    {
        private Mock<IEventReceiver<LoadEventWrapper>> mockEventReceiver;
        private Mock<IStatisticsReceiver<LoadMethodStatistics>> mockStatisticsReceiver;
        private Mock<ILogger<LoadStatisticsConsumer>> mockLogger;
        private LoadStatisticsConsumer collector;

        [SetUp]
        public override void Setup()
        {
            base.Setup();

            mockEventReceiver = new Mock<IEventReceiver<LoadEventWrapper>>();
            mockStatisticsReceiver = new Mock<IStatisticsReceiver<LoadMethodStatistics>>();
            mockLogger = new Mock<ILogger<LoadStatisticsConsumer>>();

            collector = new LoadStatisticsConsumer(
                mockEventReceiver.Object,
                mockStatisticsReceiver.Object,
                mockStatisticsSender.Object,
                mockLogger.Object
            );
        }

        [Test]
        public async Task StartConsumingStatistics_AddsListenerAndSendOnlyInitialStatistics()
        {
            // Arrange
            var key = "testKey";
            mockEventReceiver.Setup(m => m.ReceiveEventAmountByKeyAsync(key, It.IsAny<CancellationToken>()))
                             .ReturnsAsync(100);
            mockEventReceiver.Setup(m => m.ReceiveLastEventByKeyAsync(key, It.IsAny<CancellationToken>()))
                             .ReturnsAsync(new LoadEventWrapper());
            mockStatisticsReceiver.Setup(m => m.ReceiveLastStatisticsByKeyAsync(key, It.IsAny<CancellationToken>()))
                                  .ReturnsAsync(new LoadMethodStatistics());
            // Act
            collector.StartConsumingStatistics(key);
            await Task.Delay(500);
            collector.StopConsumingStatistics(key);
            // Assert
            mockStatisticsSender.Verify(m => m.SendStatisticsAsync(It.IsAny<string>(), It.IsAny<ServerLoadStatistics>(), It.IsAny<CancellationToken>()), Times.Once);
        }
        [Test]
        public async Task StartConsumingStatistics_SendsMoreThanOneStatistics()
        {
            // Arrange
            var key = "testKey";
            mockEventReceiver.Setup(m => m.ReceiveEventAmountByKeyAsync(key, It.IsAny<CancellationToken>()))
                             .ReturnsAsync(100);
            mockEventReceiver.Setup(m => m.ReceiveLastEventByKeyAsync(key, It.IsAny<CancellationToken>()))
                             .ReturnsAsync(new LoadEventWrapper());
            mockEventReceiver.Setup(m => m.ConsumeEventAsync(key, It.IsAny<CancellationToken>()))
                             .Returns(AsyncEnumerable(new List<LoadEventWrapper> { new LoadEventWrapper() }));
            mockStatisticsReceiver.Setup(m => m.ReceiveLastStatisticsByKeyAsync(key, It.IsAny<CancellationToken>()))
                                  .ReturnsAsync(new LoadMethodStatistics());
            // Act
            collector.StartConsumingStatistics(key);
            await Task.Delay(2000);
            collector.StopConsumingStatistics(key);
            // Assert
            mockStatisticsSender.Verify(m => m.SendStatisticsAsync(It.IsAny<string>(), It.IsAny<ServerLoadStatistics>(), It.IsAny<CancellationToken>()), Times.AtLeast(2));
        }
        [Test]
        public async Task StopConsumingStatistics_StopsSendingStatistics()
        {
            // Arrange
            var key = "testKey";
            mockEventReceiver.Setup(m => m.ReceiveEventAmountByKeyAsync(key, It.IsAny<CancellationToken>()))
                             .ReturnsAsync(100);
            mockEventReceiver.Setup(m => m.ReceiveLastEventByKeyAsync(key, It.IsAny<CancellationToken>()))
                             .ReturnsAsync(new LoadEventWrapper());
            mockStatisticsReceiver.Setup(m => m.ReceiveLastStatisticsByKeyAsync(key, It.IsAny<CancellationToken>()))
                                  .ReturnsAsync(new LoadMethodStatistics());
            // Act
            collector.StartConsumingStatistics(key);
            await Task.Delay(1500);
            collector.StopConsumingStatistics(key);
            await Task.Delay(1500);
            // Assert
            mockStatisticsSender.Verify(m => m.SendStatisticsAsync(It.IsAny<string>(), It.IsAny<ServerLoadStatistics>(), It.IsAny<CancellationToken>()), Times.AtMost(2));
        }
        [Test]
        public async Task SubscribeToLoadEventsAsync_SendsStatisticsOnNewEvent()
        {
            // Arrange
            var key = "testKey";
            var loadEvent = new LoadEventWrapper();
            mockEventReceiver.Setup(m => m.ConsumeEventAsync(key, It.IsAny<CancellationToken>()))
                             .Returns(AsyncEnumerable(new List<LoadEventWrapper> { loadEvent }));
            mockEventReceiver.Setup(m => m.ReceiveEventAmountByKeyAsync(key, It.IsAny<CancellationToken>()))
                             .ReturnsAsync(100);
            mockStatisticsReceiver.Setup(m => m.ReceiveLastStatisticsByKeyAsync(key, It.IsAny<CancellationToken>()))
                                  .ReturnsAsync(new LoadMethodStatistics());
            // Act
            collector.StartConsumingStatistics(key);
            await Task.Delay(2000);
            collector.StopConsumingStatistics(key);
            // Assert
            mockStatisticsSender.Verify(m => m.SendStatisticsAsync(It.IsAny<string>(), It.Is<ServerLoadStatistics>(s => s.LastEvent == loadEvent), It.IsAny<CancellationToken>()), Times.AtLeastOnce);
        }
    }
}
using AnalyzerApi.Domain.Dtos.Wrappers;
using AnalyzerApi.Domain.Models;
using AnalyzerApi.Services.Interfaces;
using Moq;

namespace AnalyzerApi.Services.Collectors.Tests
{
    [TestFixture]
    internal class LoadStatisticsCollectorTests
    {
        private Mock<IEventReceiver<LoadEventWrapper>> eventReceiverMock;
        private Mock<IStatisticsReceiver<LoadMethodStatistics>> methodStatsReceiverMock;
        private LoadStatisticsCollector loadStatisticsCollector;

        [SetUp]
        public void Setup()
        {
            eventReceiverMock = new Mock<IEventReceiver<LoadEventWrapper>>();
            methodStatsReceiverMock = new Mock<IStatisticsReceiver<LoadMethodStatistics>>();
            loadStatisticsCollector = new LoadStatisticsCollector(eventReceiverMock.Object, methodStatsReceiverMock.Object);
        }

        [Test]
        public async Task ReceiveLastStatisticsAsync_ValidKey_ReturnsStatistics()
        {
            // Arrange
            string key = "validKey";
            int expectedEventAmount = 5;
            var lastEvent = new LoadEventWrapper();
            var methodStatistics = new LoadMethodStatistics();
            eventReceiverMock
                .Setup(x => x.ReceiveEventAmountByKeyAsync(key, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedEventAmount);
            eventReceiverMock
                .Setup(x => x.ReceiveLastEventByKeyAsync(key, It.IsAny<CancellationToken>()))
                .ReturnsAsync(lastEvent);
            methodStatsReceiverMock
                .Setup(x => x.ReceiveLastStatisticsAsync(key, It.IsAny<CancellationToken>()))
                .ReturnsAsync(methodStatistics);
            // Act
            var result = await loadStatisticsCollector.ReceiveLastStatisticsAsync(key, CancellationToken.None);
            // Assert
            Assert.That(result.AmountOfEvents, Is.EqualTo(expectedEventAmount));
            Assert.That(result.LastEvent, Is.EqualTo(lastEvent));
            Assert.That(result.LoadMethodStatistics, Is.EqualTo(methodStatistics));
        }
        [Test]
        public async Task ReceiveLastStatisticsAsync_EmptyMethodStatistics_ReturnsDefaultStatistics()
        {
            // Arrange
            string key = "validKey";
            int expectedEventAmount = 5;
            var lastEvent = new LoadEventWrapper();
            eventReceiverMock
                .Setup(x => x.ReceiveEventAmountByKeyAsync(key, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedEventAmount);
            eventReceiverMock
                .Setup(x => x.ReceiveLastEventByKeyAsync(key, It.IsAny<CancellationToken>()))
                .ReturnsAsync(lastEvent);
            methodStatsReceiverMock
                .Setup(x => x.ReceiveLastStatisticsAsync(key, It.IsAny<CancellationToken>()))
                .ReturnsAsync((LoadMethodStatistics)null);
            // Act
            var result = await loadStatisticsCollector.ReceiveLastStatisticsAsync(key, CancellationToken.None);
            // Assert
            Assert.That(result.AmountOfEvents, Is.EqualTo(expectedEventAmount));
            Assert.That(result.LastEvent, Is.EqualTo(lastEvent));
            Assert.IsNotNull(result.LoadMethodStatistics);
            Assert.IsInstanceOf<LoadMethodStatistics>(result.LoadMethodStatistics);
        }
        [Test]
        public void ReceiveLastStatisticsAsync_CancellationRequested_ThrowsTaskCanceledException()
        {
            // Arrange
            string key = "validKey";
            var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.Cancel();
            eventReceiverMock
                .Setup(x => x.ReceiveEventAmountByKeyAsync(key, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new TaskCanceledException());
            // Act & Assert
            Assert.ThrowsAsync<TaskCanceledException>(
                async () => await loadStatisticsCollector.ReceiveLastStatisticsAsync(key, cancellationTokenSource.Token));
        }
    }
}
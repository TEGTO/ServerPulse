using AnalyzerApi.Domain.Dtos.Wrappers;
using AnalyzerApi.Domain.Models;
using AnalyzerApi.Services.Collectors;
using AnalyzerApi.Services.Interfaces;
using AnalyzerApiTests.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace AnalyzerApi.Services
{
    [TestFixture]
    internal class CustomStatisticsCollectorTests : BaseStatisticsCollectorTests
    {
        private Mock<ICustomReceiver> mockCustomReceiver;
        private Mock<ILogger<CustomStatisticsCollector>> mockLogger;
        private CustomStatisticsCollector collector;

        [SetUp]
        public override void Setup()
        {
            base.Setup();

            mockCustomReceiver = new Mock<ICustomReceiver>();
            mockLogger = new Mock<ILogger<CustomStatisticsCollector>>();

            collector = new CustomStatisticsCollector(
                mockCustomReceiver.Object,
                mockStatisticsSender.Object,
                mockLogger.Object
            );
        }

        [Test]
        public async Task StartConsumingStatistics_ShouldAddListenerAndSendOnlyInitialStatistics()
        {
            // Arrange
            var key = "testKey";
            mockCustomReceiver.Setup(m => m.ReceiveLastCustomEventByKeyAsync(key, It.IsAny<CancellationToken>()))
                            .ReturnsAsync(new CustomEventWrapper());
            // Act
            collector.StartConsumingStatistics(key);
            await Task.Delay(500);
            collector.StopConsumingStatistics(key);
            // Assert
            mockStatisticsSender.Verify(m => m.SendServerCustomStatisticsAsync(It.IsAny<string>(), It.IsAny<CustomEventStatistics>(), It.IsAny<CancellationToken>()), Times.Once);
        }
        [Test]
        public async Task StartConsumingStatistics_ShouldSendMoreThanOneStatistics()
        {
            // Arrange
            var key = "testKey";
            mockCustomReceiver.Setup(m => m.ReceiveLastCustomEventByKeyAsync(key, It.IsAny<CancellationToken>()))
                            .ReturnsAsync(new CustomEventWrapper());
            mockCustomReceiver.Setup(m => m.ConsumeCustomEventAsync(key, It.IsAny<CancellationToken>()))
                            .Returns(AsyncEnumerable(new List<CustomEventWrapper> { new CustomEventWrapper() }));
            // Act
            collector.StartConsumingStatistics(key);
            await Task.Delay(2000);
            collector.StopConsumingStatistics(key);
            // Assert
            mockStatisticsSender.Verify(m => m.SendServerCustomStatisticsAsync(It.IsAny<string>(), It.IsAny<CustomEventStatistics>(), It.IsAny<CancellationToken>()), Times.AtLeast(2));
        }
        [Test]
        public async Task StopConsumingStatistics_ShouldStopSendingStatistics()
        {
            // Arrange
            var key = "testKey";
            mockCustomReceiver.Setup(m => m.ReceiveLastCustomEventByKeyAsync(key, It.IsAny<CancellationToken>()))
                         .ReturnsAsync(new CustomEventWrapper());
            // Act
            collector.StartConsumingStatistics(key);
            await Task.Delay(1500);
            collector.StopConsumingStatistics(key);
            await Task.Delay(1500);
            // Assert
            mockStatisticsSender.Verify(m => m.SendServerCustomStatisticsAsync(It.IsAny<string>(), It.IsAny<CustomEventStatistics>(), It.IsAny<CancellationToken>()), Times.AtMost(2));
        }
        [Test]
        public async Task SubscribeToEventsAsync_ShouldSendStatisticsOnNewEvent()
        {
            // Arrange
            var key = "testKey";
            var eventWrapper = new CustomEventWrapper();
            mockCustomReceiver.Setup(m => m.ConsumeCustomEventAsync(key, It.IsAny<CancellationToken>()))
                            .Returns(AsyncEnumerable(new List<CustomEventWrapper> { eventWrapper }));
            // Act
            collector.StartConsumingStatistics(key);
            await Task.Delay(2000);
            collector.StopConsumingStatistics(key);
            // Assert
            mockStatisticsSender.Verify(m => m.SendServerCustomStatisticsAsync(It.IsAny<string>(), It.Is<CustomEventStatistics>(s => s.LastEvent == eventWrapper), It.IsAny<CancellationToken>()), Times.AtLeastOnce);
        }
    }
}
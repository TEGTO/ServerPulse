using AnalyzerApi.Domain.Dtos.Wrappers;
using AnalyzerApi.Domain.Models;
using AnalyzerApi.Services.Collectors;
using AnalyzerApi.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;

namespace AnalyzerApiTests.Services.Collector
{
    [TestFixture]
    internal class CustomStatisticsCollectorTests : BaseStatisticsCollectorTests
    {
        private Mock<IEventReceiver<CustomEventWrapper>> mockEventReceiver;
        private Mock<ILogger<CustomStatisticsCollector>> mockLogger;
        private CustomStatisticsCollector collector;

        [SetUp]
        public override void Setup()
        {
            base.Setup();

            mockEventReceiver = new Mock<IEventReceiver<CustomEventWrapper>>();
            mockLogger = new Mock<ILogger<CustomStatisticsCollector>>();

            collector = new CustomStatisticsCollector(
                mockEventReceiver.Object,
                mockStatisticsSender.Object,
                mockLogger.Object
            );
        }

        [Test]
        public async Task SendInitialStatisticsAsync_SendsInitialStatistics()
        {
            // Arrange
            var key = "testKey";
            var eventWrapper = new CustomEventWrapper();
            mockEventReceiver.Setup(m => m.ReceiveLastEventByKeyAsync(key, It.IsAny<CancellationToken>()))
                             .ReturnsAsync(eventWrapper);
            // Act
            collector.StartConsumingStatistics(key);
            await Task.Delay(500);
            // Assert
            mockStatisticsSender.Verify(m => m.SendStatisticsAsync(key,
                It.Is<CustomEventStatistics>(s => s.LastEvent == eventWrapper), It.IsAny<CancellationToken>()), Times.Once);
        }
        [Test]
        public async Task GetEventSubscriptionTasks_SubscribesToCustomEventsAndSendStatistics()
        {
            // Arrange
            var key = "testKey";
            var eventWrapper = new CustomEventWrapper();
            mockEventReceiver.Setup(m => m.ConsumeEventAsync(key, It.IsAny<CancellationToken>()))
                             .Returns(AsyncEnumerable(new List<CustomEventWrapper> { eventWrapper }));
            // Act
            collector.StartConsumingStatistics(key);
            await Task.Delay(2000);
            // Assert
            mockStatisticsSender.Verify(m => m.SendStatisticsAsync(key,
                It.Is<CustomEventStatistics>(s => s.LastEvent == eventWrapper), It.IsAny<CancellationToken>()), Times.AtLeastOnce);
        }
        [Test]
        public async Task StopConsumingStatistics_StopsSendingStatistics()
        {
            // Arrange
            var key = "testKey";
            var eventWrapper = new CustomEventWrapper();
            mockEventReceiver.Setup(m => m.ReceiveLastEventByKeyAsync(key, It.IsAny<CancellationToken>()))
                             .ReturnsAsync(eventWrapper);
            mockEventReceiver.Setup(m => m.ConsumeEventAsync(key, It.IsAny<CancellationToken>()))
                             .Returns(AsyncEnumerable(new List<CustomEventWrapper> { eventWrapper }));
            // Act
            collector.StartConsumingStatistics(key);
            await Task.Delay(1500);
            collector.StopConsumingStatistics(key);
            await Task.Delay(1500);
            // Assert
            mockStatisticsSender.Verify(m => m.SendStatisticsAsync(key, It.IsAny<CustomEventStatistics>(), It.IsAny<CancellationToken>()), Times.AtMost(2));
        }
        [Test]
        public async Task SubscribeToCustomEventsAsync_SendsStatisticsOnNewEvent()
        {
            // Arrange
            var key = "testKey";
            var eventWrapper = new CustomEventWrapper();
            mockEventReceiver.Setup(m => m.ConsumeEventAsync(key, It.IsAny<CancellationToken>()))
                             .Returns(AsyncEnumerable(new List<CustomEventWrapper> { eventWrapper }));
            // Act
            collector.StartConsumingStatistics(key);
            await Task.Delay(2000);
            // Assert
            mockStatisticsSender.Verify(m => m.SendStatisticsAsync(key,
                It.Is<CustomEventStatistics>(s => s.LastEvent == eventWrapper), It.IsAny<CancellationToken>()), Times.AtLeastOnce);
        }
    }
}
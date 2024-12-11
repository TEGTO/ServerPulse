using AnalyzerApi.Domain.Dtos.Wrappers;
using AnalyzerApi.Services.Interfaces;
using Moq;

namespace AnalyzerApi.Services.Collectors.Tests
{
    [TestFixture]
    public class CustomStatisticsCollectorTests
    {
        private Mock<IEventReceiver<CustomEventWrapper>> eventReceiverMock;
        private CustomStatisticsCollector customStatisticsCollector;

        [SetUp]
        public void Setup()
        {
            eventReceiverMock = new Mock<IEventReceiver<CustomEventWrapper>>();
            customStatisticsCollector = new CustomStatisticsCollector(eventReceiverMock.Object);
        }

        [Test]
        public async Task ReceiveLastStatisticsAsync_ValidKey_ReturnsCustomStatistics()
        {
            // Arrange
            string key = "validKey";
            var lastEvent = new CustomEventWrapper();
            eventReceiverMock
                .Setup(x => x.ReceiveLastEventByKeyAsync(key, It.IsAny<CancellationToken>()))
                .ReturnsAsync(lastEvent);
            // Act
            var result = await customStatisticsCollector.ReceiveLastStatisticsAsync(key, CancellationToken.None);
            // Assert
            Assert.IsNotNull(result);
            Assert.That(result.LastEvent, Is.EqualTo(lastEvent));
        }
        [Test]
        public async Task ReceiveLastStatisticsAsync_NullLastEvent_ReturnsStatisticsWithNullEvent()
        {
            // Arrange
            string key = "validKey";
            eventReceiverMock
                .Setup(x => x.ReceiveLastEventByKeyAsync(key, It.IsAny<CancellationToken>()))
                .ReturnsAsync((CustomEventWrapper)null);
            // Act
            var result = await customStatisticsCollector.ReceiveLastStatisticsAsync(key, CancellationToken.None);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsNull(result.LastEvent);
        }
        [Test]
        public void ReceiveLastStatisticsAsync_CancellationRequested_ThrowsTaskCanceledException()
        {
            // Arrange
            string key = "validKey";
            var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.Cancel();
            eventReceiverMock
                .Setup(x => x.ReceiveLastEventByKeyAsync(key, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new TaskCanceledException());
            // Act & Assert
            Assert.ThrowsAsync<TaskCanceledException>(
                async () => await customStatisticsCollector.ReceiveLastStatisticsAsync(key, cancellationTokenSource.Token));
        }
    }
}
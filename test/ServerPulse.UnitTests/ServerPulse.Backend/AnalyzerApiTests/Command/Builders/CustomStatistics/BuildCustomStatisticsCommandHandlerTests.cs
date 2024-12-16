using AnalyzerApi.Infrastructure.Models.Statistics;
using AnalyzerApi.Infrastructure.Models.Wrappers;
using AnalyzerApi.Services.Receivers.Event;
using Moq;

namespace AnalyzerApi.Command.Builders.CustomStatistics.Tests
{
    [TestFixture]
    internal class BuildCustomStatisticsCommandHandlerTests
    {
        private Mock<IEventReceiver<CustomEventWrapper>> mockEventReceiver;
        private BuildCustomStatisticsCommandHandler handler;

        [SetUp]
        public void SetUp()
        {
            mockEventReceiver = new Mock<IEventReceiver<CustomEventWrapper>>();

            handler = new BuildCustomStatisticsCommandHandler(mockEventReceiver.Object);
        }

        [Test]
        public async Task Handle_ReturnsServerCustomStatistics_WhenLastEventExists()
        {
            // Arrange
            var key = "testKey";
            var lastEvent = new CustomEventWrapper
            {
                Id = "eventId",
                Key = key,
                Name = "TestEvent",
                Description = "Test Description",
                SerializedMessage = "{}",
                CreationDateUTC = DateTime.UtcNow.AddMinutes(-5)
            };

            mockEventReceiver.Setup(r => r.GetLastEventByKeyAsync(key, It.IsAny<CancellationToken>()))
                .ReturnsAsync(lastEvent);

            var command = new BuildStatisticsCommand<ServerCustomStatistics>(key);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.IsNotNull(result);
            Assert.That(result.LastEvent, Is.EqualTo(lastEvent));
            Assert.That(result.CollectedDateUTC, Is.EqualTo(DateTime.UtcNow).Within(TimeSpan.FromSeconds(1)));
            mockEventReceiver.Verify(r => r.GetLastEventByKeyAsync(key, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task Handle_ReturnsStatisticsWithNullLastEvent_WhenNoEventExists()
        {
            // Arrange
            var key = "testKey";

            mockEventReceiver.Setup(r => r.GetLastEventByKeyAsync(key, It.IsAny<CancellationToken>()))
                .ReturnsAsync((CustomEventWrapper?)null);

            var command = new BuildStatisticsCommand<ServerCustomStatistics>(key);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNull(result.LastEvent);
            Assert.That(result.CollectedDateUTC, Is.EqualTo(DateTime.UtcNow).Within(TimeSpan.FromSeconds(1)));
            mockEventReceiver.Verify(r => r.GetLastEventByKeyAsync(key, It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
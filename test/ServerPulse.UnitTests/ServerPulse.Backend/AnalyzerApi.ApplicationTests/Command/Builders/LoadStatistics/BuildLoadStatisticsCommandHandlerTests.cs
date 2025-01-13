using AnalyzerApi.Application.Services.Receivers.Event;
using AnalyzerApi.Application.Services.Receivers.Statistics;
using AnalyzerApi.Core.Models.Statistics;
using AnalyzerApi.Core.Models.Wrappers;
using Moq;

namespace AnalyzerApi.Application.Command.Builders.LoadStatistics.Tests
{
    [TestFixture]
    internal class BuildLoadStatisticsCommandHandlerTests
    {
        private Mock<IEventReceiver<LoadEventWrapper>> mockEventReceiver;
        private Mock<IStatisticsReceiver<LoadMethodStatistics>> mockMethodStatsReceiver;
        private BuildLoadStatisticsCommandHandler handler;

        [SetUp]
        public void SetUp()
        {
            mockEventReceiver = new Mock<IEventReceiver<LoadEventWrapper>>();
            mockMethodStatsReceiver = new Mock<IStatisticsReceiver<LoadMethodStatistics>>();
            handler = new BuildLoadStatisticsCommandHandler(mockEventReceiver.Object, mockMethodStatsReceiver.Object);
        }

        [Test]
        public async Task Handle_AllDataExists_ReturnsExpectedStatistics()
        {
            // Arrange
            var key = "testKey";
            var eventAmount = 10;
            var loadEvent = new LoadEventWrapper
            {
                Id = Guid.NewGuid().ToString(),
                Key = key,
                Endpoint = "/test-endpoint",
                Method = "GET",
                StatusCode = 200,
                Duration = TimeSpan.FromMilliseconds(150),
                CreationDateUTC = DateTime.UtcNow
            };

            var methodStats = new LoadMethodStatistics
            {
                GetAmount = 5,
                PostAmount = 3,
                DeleteAmount = 2
            };

            mockEventReceiver.Setup(m => m.GetEventAmountByKeyAsync(key, It.IsAny<CancellationToken>()))
                .ReturnsAsync(eventAmount);

            mockEventReceiver.Setup(m => m.GetLastEventByKeyAsync(key, It.IsAny<CancellationToken>()))
                .ReturnsAsync(loadEvent);

            mockMethodStatsReceiver.Setup(m => m.GetLastStatisticsAsync(key, It.IsAny<CancellationToken>()))
                .ReturnsAsync(methodStats);

            // Act
            var result = await handler.Handle(new BuildStatisticsCommand<ServerLoadStatistics>(key), CancellationToken.None);

            // Assert
            Assert.That(result.AmountOfEvents, Is.EqualTo(eventAmount));
            Assert.That(result.LastEvent, Is.EqualTo(loadEvent));
            Assert.That(result.LoadMethodStatistics, Is.EqualTo(methodStats));
        }

        [Test]
        public async Task Handle_NoEventsExist_ReturnsZeroEventsAndNoLastEvent()
        {
            // Arrange
            var key = "testKey";

            mockEventReceiver.Setup(m => m.GetEventAmountByKeyAsync(key, It.IsAny<CancellationToken>()))
                .ReturnsAsync(0);

            mockEventReceiver.Setup(m => m.GetLastEventByKeyAsync(key, It.IsAny<CancellationToken>()))
                .ReturnsAsync((LoadEventWrapper?)null);

            mockMethodStatsReceiver.Setup(m => m.GetLastStatisticsAsync(key, It.IsAny<CancellationToken>()))
                .ReturnsAsync((LoadMethodStatistics?)null);

            // Act
            var result = await handler.Handle(new BuildStatisticsCommand<ServerLoadStatistics>(key), CancellationToken.None);

            // Assert
            Assert.That(result.AmountOfEvents, Is.EqualTo(0));
            Assert.That(result.LastEvent, Is.Null);
            Assert.That(result.LoadMethodStatistics, Is.Not.Null);
        }

        [Test]
        public async Task Handle_NullStatisticsWithEvents_ReturnsExpectedValues()
        {
            // Arrange
            var key = "testKey";
            var eventAmount = 5;

            var loadEvent = new LoadEventWrapper
            {
                Id = Guid.NewGuid().ToString(),
                Key = key,
                Endpoint = "/some-endpoint",
                Method = "PUT",
                StatusCode = 200,
                Duration = TimeSpan.FromMilliseconds(180),
                CreationDateUTC = DateTime.UtcNow
            };

            mockEventReceiver.Setup(m => m.GetEventAmountByKeyAsync(key, It.IsAny<CancellationToken>()))
                .ReturnsAsync(eventAmount);

            mockEventReceiver.Setup(m => m.GetLastEventByKeyAsync(key, It.IsAny<CancellationToken>()))
                .ReturnsAsync(loadEvent);

            mockMethodStatsReceiver.Setup(m => m.GetLastStatisticsAsync(key, It.IsAny<CancellationToken>()))
                .ReturnsAsync((LoadMethodStatistics?)null);

            // Act
            var result = await handler.Handle(new BuildStatisticsCommand<ServerLoadStatistics>(key), CancellationToken.None);

            // Assert
            Assert.That(result.AmountOfEvents, Is.EqualTo(eventAmount));
            Assert.That(result.LastEvent, Is.EqualTo(loadEvent));
            Assert.That(result.LoadMethodStatistics, Is.Not.Null);
        }
    }
}
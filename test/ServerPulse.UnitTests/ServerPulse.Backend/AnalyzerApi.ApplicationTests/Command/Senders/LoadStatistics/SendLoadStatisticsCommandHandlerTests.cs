using AnalyzerApi.Application.Application.Services;
using AnalyzerApi.Core.Dtos.Responses.Events;
using AnalyzerApi.Core.Dtos.Responses.Statistics;
using AnalyzerApi.Core.Models.Statistics;
using AnalyzerApi.Core.Models.Wrappers;
using AutoMapper;
using Moq;

namespace AnalyzerApi.Application.Command.Senders.LoadStatistics.Tests
{
    [TestFixture]
    internal class SendLoadStatisticsCommandHandlerTests
    {
        private Mock<IStatisticsNotifier<ServerLoadStatistics, ServerLoadStatisticsResponse>> mockNotifier;
        private Mock<IMapper> mockMapper;
        private SendLoadStatisticsCommandHandler handler;

        [SetUp]
        public void SetUp()
        {
            mockMapper = new Mock<IMapper>();
            mockNotifier = new Mock<IStatisticsNotifier<ServerLoadStatistics, ServerLoadStatisticsResponse>>();

            handler = new SendLoadStatisticsCommandHandler(mockNotifier.Object, mockMapper.Object);
        }

        [Test]
        public async Task Handle_ValidCommand_SendsStatisticsToGroup()
        {
            // Arrange
            var key = "testKey";
            var statistics = new ServerLoadStatistics
            {
                AmountOfEvents = 100,
                LoadMethodStatistics = new LoadMethodStatistics(),
                LastEvent = new LoadEventWrapper()
                {
                    Id = Guid.NewGuid().ToString(),
                    Key = key,
                    Endpoint = "/test-endpoint",
                    Method = "GET",
                    StatusCode = 200,
                    Duration = TimeSpan.FromMilliseconds(150),
                    CreationDateUTC = DateTime.UtcNow
                }
            };

            var response = new ServerLoadStatisticsResponse
            {
                AmountOfEvents = statistics.AmountOfEvents,
                LoadMethodStatistics = new LoadMethodStatisticsResponse(),
                LastEvent = new LoadEventResponse()
            };

            mockMapper.Setup(m => m.Map<ServerLoadStatisticsResponse>(statistics)).Returns(response);

            // Act
            await handler.Handle(new SendStatisticsCommand<ServerLoadStatistics>(key, statistics), CancellationToken.None);

            // Assert
            mockMapper.Verify(m => m.Map<ServerLoadStatisticsResponse>(statistics), Times.Once);
            mockNotifier.Verify(c => c.NotifyGroupAsync(key, response), Times.Once);
        }
    }
}
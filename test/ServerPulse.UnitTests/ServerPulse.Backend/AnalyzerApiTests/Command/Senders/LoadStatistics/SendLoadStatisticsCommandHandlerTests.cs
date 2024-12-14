using AnalyzerApi.Hubs;
using AnalyzerApi.Infrastructure.Dtos.Responses.Events;
using AnalyzerApi.Infrastructure.Dtos.Responses.Statistics;
using AnalyzerApi.Infrastructure.Models.Statistics;
using AnalyzerApi.Infrastructure.Models.Wrappers;
using AutoMapper;
using Microsoft.AspNetCore.SignalR;
using Moq;
using System.Text.Json;

namespace AnalyzerApi.Command.Senders.LoadStatistics.Tests
{
    [TestFixture]
    internal class SendLoadStatisticsCommandHandlerTests
    {
        private Mock<IHubContext<StatisticsHub<ServerLoadStatistics>, IStatisticsHubClient>> mockHubContext;
        private Mock<IStatisticsHubClient> mockClientProxy;
        private Mock<IMapper> mockMapper;
        private SendLoadStatisticsCommandHandler handler;

        [SetUp]
        public void SetUp()
        {
            mockHubContext = new Mock<IHubContext<StatisticsHub<ServerLoadStatistics>, IStatisticsHubClient>>();
            mockClientProxy = new Mock<IStatisticsHubClient>();
            mockMapper = new Mock<IMapper>();

            var mockClients = new Mock<IHubClients<IStatisticsHubClient>>();
            mockClients.Setup(c => c.Group(It.IsAny<string>())).Returns(mockClientProxy.Object);
            mockHubContext.Setup(h => h.Clients).Returns(mockClients.Object);

            handler = new SendLoadStatisticsCommandHandler(mockHubContext.Object, mockMapper.Object);
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
            await handler.Handle(new SendLoadStatisticsCommand(key, statistics), CancellationToken.None);

            // Assert
            var serializedResponse = JsonSerializer.Serialize(response);

            mockMapper.Verify(m => m.Map<ServerLoadStatisticsResponse>(statistics), Times.Once);
            mockClientProxy.Verify(c => c.ReceiveStatistics(key, serializedResponse), Times.Once);
        }
    }
}
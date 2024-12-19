using AnalyzerApi.Hubs;
using AnalyzerApi.Infrastructure.Dtos.Responses.Events;
using AnalyzerApi.Infrastructure.Dtos.Responses.Statistics;
using AnalyzerApi.Infrastructure.Models.Statistics;
using AnalyzerApi.Infrastructure.Models.Wrappers;
using AutoMapper;
using Microsoft.AspNetCore.SignalR;
using Moq;

namespace AnalyzerApi.Command.Senders.CustomStatistics.Tests
{
    [TestFixture]
    internal class SendCustomStatisticsCommandHandlerTests
    {
        private Mock<IHubContext<StatisticsHub<ServerCustomStatistics>, IStatisticsHubClient>> mockHubContext;
        private Mock<IStatisticsHubClient> mockClientProxy;
        private Mock<IMapper> mockMapper;
        private SendCustomStatisticsCommandHandler handler;

        [SetUp]
        public void SetUp()
        {
            mockHubContext = new Mock<IHubContext<StatisticsHub<ServerCustomStatistics>, IStatisticsHubClient>>();
            mockClientProxy = new Mock<IStatisticsHubClient>();
            mockMapper = new Mock<IMapper>();

            var mockClients = new Mock<IHubClients<IStatisticsHubClient>>();
            mockClients.Setup(c => c.Group(It.IsAny<string>())).Returns(mockClientProxy.Object);
            mockHubContext.Setup(h => h.Clients).Returns(mockClients.Object);

            handler = new SendCustomStatisticsCommandHandler(mockHubContext.Object, mockMapper.Object);
        }

        [Test]
        public async Task Handle_ValidStatistics_SendsToGroup()
        {
            // Arrange
            var key = "testKey";
            var statistics = new ServerCustomStatistics
            {
                LastEvent = new CustomEventWrapper
                {
                    Id = Guid.NewGuid().ToString(),
                    Key = key,
                    Name = "EventName",
                    Description = "EventDescription",
                    SerializedMessage = "SerializedEvent"
                }
            };

            var response = new ServerCustomStatisticsResponse
            {
                LastEvent = new CustomEventResponse
                {
                    Name = "EventName",
                    Description = "EventDescription"
                }
            };

            mockMapper.Setup(m => m.Map<ServerCustomStatisticsResponse>(statistics)).Returns(response);

            // Act
            await handler.Handle(new SendStatisticsCommand<ServerCustomStatistics>(key, statistics), CancellationToken.None);

            // Assert
            mockMapper.Verify(m => m.Map<ServerCustomStatisticsResponse>(statistics), Times.Once);
            mockClientProxy.Verify(c => c.ReceiveStatistics(key, response), Times.Once);
        }
    }
}
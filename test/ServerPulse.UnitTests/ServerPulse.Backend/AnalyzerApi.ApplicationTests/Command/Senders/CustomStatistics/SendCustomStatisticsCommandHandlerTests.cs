using AnalyzerApi.Application.Application.Services;
using AnalyzerApi.Core.Dtos.Responses.Events;
using AnalyzerApi.Core.Dtos.Responses.Statistics;
using AnalyzerApi.Core.Models.Statistics;
using AnalyzerApi.Core.Models.Wrappers;
using AutoMapper;
using Moq;

namespace AnalyzerApi.Application.Command.Senders.CustomStatistics.Tests
{
    [TestFixture]
    internal class SendCustomStatisticsCommandHandlerTests
    {
        private Mock<IStatisticsNotifier<ServerCustomStatistics, ServerCustomStatisticsResponse>> mockNotifier;
        private Mock<IMapper> mockMapper;
        private SendCustomStatisticsCommandHandler handler;

        [SetUp]
        public void SetUp()
        {
            mockNotifier = new Mock<IStatisticsNotifier<ServerCustomStatistics, ServerCustomStatisticsResponse>>();
            mockMapper = new Mock<IMapper>();

            handler = new SendCustomStatisticsCommandHandler(mockNotifier.Object, mockMapper.Object);
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
            mockNotifier.Verify(c => c.NotifyGroupAsync(key, response), Times.Once);
        }
    }
}
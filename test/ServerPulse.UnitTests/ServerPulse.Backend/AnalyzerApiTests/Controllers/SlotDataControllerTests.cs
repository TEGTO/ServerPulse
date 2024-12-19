using AnalyzerApi.Command.Controllers.GetSlotStatistics;
using AnalyzerApi.Infrastructure.Dtos.Responses.Events;
using AnalyzerApi.Infrastructure.Dtos.Responses.Statistics;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace AnalyzerApi.Controllers.Tests
{
    [TestFixture]
    internal class SlotDataControllerTests
    {
        private Mock<IMediator> mockMediator;
        private SlotDataController controller;

        [SetUp]
        public void SetUp()
        {
            mockMediator = new Mock<IMediator>();
            controller = new SlotDataController(mockMediator.Object);
        }

        [Test]
        public async Task GetSlotStatistics_ValidKey_ReturnsOkWithResponse()
        {
            // Arrange
            var key = "validKey";
            var expectedResponse = new SlotStatisticsResponse
            {
                CollectedDateUTC = DateTime.UtcNow,
                GeneralStatistics = new ServerLifecycleStatisticsResponse { IsAlive = true, DataExists = true },
                LoadStatistics = new ServerLoadStatisticsResponse { AmountOfEvents = 5 },
                CustomEventStatistics = new ServerCustomStatisticsResponse(),
                LastLoadEvents = new List<LoadEventResponse>
                {
                    new LoadEventResponse { Endpoint = "/api/test", Method = "GET", StatusCode = 200, Duration = TimeSpan.FromMilliseconds(100) }
                },
                LastCustomEvents = new List<CustomEventResponse>
                {
                    new CustomEventResponse { Name = "CustomEvent", Description = "Test event" }
                }
            };

            mockMediator
                .Setup(m => m.Send(It.IsAny<GetSlotStatisticsQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await controller.GetSlotStatistics(key, CancellationToken.None);

            // Assert
            Assert.IsInstanceOf<OkObjectResult>(result.Result);
            var okResult = result.Result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.That(okResult.Value, Is.EqualTo(expectedResponse));

            mockMediator.Verify(m => m.Send(It.IsAny<GetSlotStatisticsQuery>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}

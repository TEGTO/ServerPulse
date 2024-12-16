using AnalyzerApi.Command.Controllers.GetLoadAmountStatisticsInRange;
using AnalyzerApi.Command.Controllers.GetLoadEventsInDataRange;
using AnalyzerApi.Command.Controllers.GetSomeCustomEvents;
using AnalyzerApi.Command.Controllers.GetSomeLoadEvents;
using AnalyzerApi.Command.Controllers.GetDailyLoadStatistics;
using AnalyzerApi.Infrastructure.Dtos.Responses.Events;
using AnalyzerApi.Infrastructure.Dtos.Responses.Statistics;
using AnalyzerApi.Infrastructure.Requests;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace AnalyzerApi.Controllers.Tests
{
    [TestFixture]
    internal class AnalyzeControllerTests
    {
        private Mock<IMediator> mockMediator;
        private AnalyzeController controller;

        [SetUp]
        public void Setup()
        {
            mockMediator = new Mock<IMediator>();

            controller = new AnalyzeController(mockMediator.Object);
        }

        [Test]
        public async Task GetLoadEventsInDataRange_ValidRequest_ReturnsOkWithResponse()
        {
            // Arrange
            var request = new MessagesInRangeRequest
            {
                Key = "testKey",
                From = DateTime.UtcNow.AddDays(-7),
                To = DateTime.UtcNow
            };

            var expectedResponse = new List<LoadEventResponse>
            {
                new LoadEventResponse { Key = "testKey", Method = "GET", StatusCode = 200 }
            };

            mockMediator
                .Setup(m => m.Send(It.IsAny<GetLoadEventsInDataRangeQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await controller.GetLoadEventsInDataRange(request, CancellationToken.None);

            // Assert
            Assert.IsInstanceOf<OkObjectResult>(result.Result);
            var okResult = result.Result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.That(okResult.Value, Is.EqualTo(expectedResponse));
            mockMediator.Verify(m => m.Send(It.IsAny<GetLoadEventsInDataRangeQuery>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task GetDailyLoadStatistics_ValidKey_ReturnsOkWithResponse()
        {
            // Arrange
            var key = "testKey";

            var expectedResponse = new List<LoadAmountStatisticsResponse>
            {
                new LoadAmountStatisticsResponse { AmountOfEvents = 100, DateFrom = DateTime.UtcNow.AddDays(-1), DateTo = DateTime.UtcNow }
            };

            mockMediator
                .Setup(m => m.Send(It.IsAny<GetDailyLoadStatisticsQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await controller.GetDailyLoadStatistics(key, CancellationToken.None);

            // Assert
            Assert.IsInstanceOf<OkObjectResult>(result.Result);
            var okResult = result.Result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.That(okResult.Value, Is.EqualTo(expectedResponse));
            mockMediator.Verify(m => m.Send(It.IsAny<GetDailyLoadStatisticsQuery>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task GetAmountStatisticsInRange_ValidRequest_ReturnsOkWithResponse()
        {
            // Arrange
            var request = new MessageAmountInRangeRequest
            {
                Key = "testKey",
                From = DateTime.UtcNow.AddDays(-2),
                To = DateTime.UtcNow,
                TimeSpan = TimeSpan.FromDays(1)
            };

            var expectedResponse = new List<LoadAmountStatisticsResponse>
            {
                new LoadAmountStatisticsResponse { AmountOfEvents = 50, DateFrom = DateTime.UtcNow.AddDays(-1), DateTo = DateTime.UtcNow }
            };

            mockMediator
                .Setup(m => m.Send(It.IsAny<GetLoadAmountStatisticsInRangeQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await controller.GetLoadAmountStatisticsInRange(request, CancellationToken.None);

            // Assert
            Assert.IsInstanceOf<OkObjectResult>(result.Result);
            var okResult = result.Result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.That(okResult.Value, Is.EqualTo(expectedResponse));
            mockMediator.Verify(m => m.Send(It.IsAny<GetLoadAmountStatisticsInRangeQuery>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task GetSomeLoadEvents_ValidRequest_ReturnsOkWithResponse()
        {
            // Arrange
            var request = new GetSomeMessagesRequest
            {
                Key = "testKey",
                NumberOfMessages = 5,
                StartDate = DateTime.UtcNow.AddDays(-1),
                ReadNew = true
            };

            var expectedResponse = new List<LoadEventResponse>
            {
                new LoadEventResponse { Key = "testKey", Method = "POST", StatusCode = 201 }
            };

            mockMediator
                .Setup(m => m.Send(It.IsAny<GetSomeLoadEventsQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await controller.GetSomeLoadEvents(request, CancellationToken.None);

            // Assert
            Assert.IsInstanceOf<OkObjectResult>(result.Result);
            var okResult = result.Result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.That(okResult.Value, Is.EqualTo(expectedResponse));
            mockMediator.Verify(m => m.Send(It.IsAny<GetSomeLoadEventsQuery>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task GetSomeCustomEvents_ValidRequest_ReturnsOkWithResponse()
        {
            // Arrange
            var request = new GetSomeMessagesRequest
            {
                Key = "customKey",
                NumberOfMessages = 3,
                StartDate = DateTime.UtcNow.AddHours(-12),
                ReadNew = false
            };

            var expectedResponse = new List<CustomEventResponse>
            {
                new CustomEventResponse { Key = "customKey", Name = "Custom Event", Description = "Test Description" }
            };

            mockMediator
                .Setup(m => m.Send(It.IsAny<GetSomeCustomEventsQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await controller.GetSomeCustomEvents(request, CancellationToken.None);

            // Assert
            Assert.IsInstanceOf<OkObjectResult>(result.Result);
            var okResult = result.Result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.That(okResult.Value, Is.EqualTo(expectedResponse));
            mockMediator.Verify(m => m.Send(It.IsAny<GetSomeCustomEventsQuery>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
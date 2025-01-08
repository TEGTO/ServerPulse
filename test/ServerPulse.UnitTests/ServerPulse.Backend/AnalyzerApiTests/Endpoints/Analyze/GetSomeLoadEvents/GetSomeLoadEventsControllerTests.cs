using AnalyzerApi.Infrastructure.Dtos.Endpoints.Analyze.GetSomeLoadEvents;
using AnalyzerApi.Infrastructure.Dtos.Responses.Events;
using AnalyzerApi.Infrastructure.Models;
using AnalyzerApi.Infrastructure.Models.Wrappers;
using AnalyzerApi.Services.Receivers.Event;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace AnalyzerApi.Endpoints.Analyze.GetSomeLoadEvents.Tests
{
    [TestFixture]
    internal class GetSomeLoadEventsControllerTests
    {
        private Mock<IEventReceiver<LoadEventWrapper>> mockReceiver;
        private Mock<IMapper> mockMapper;
        private GetSomeLoadEventsController controller;

        [SetUp]
        public void Setup()
        {
            mockReceiver = new Mock<IEventReceiver<LoadEventWrapper>>();
            mockMapper = new Mock<IMapper>();

            controller = new GetSomeLoadEventsController(mockReceiver.Object, mockMapper.Object);
        }

        [Test]
        public async Task GetSomeLoadEvents_ValidQuery_ReturnsMappedResponses()
        {
            // Arrange
            var key = "testKey";
            var numberOfMessages = 5;
            var startDate = DateTime.UtcNow.AddHours(-1);
            var request = new GetSomeLoadEventsRequest
            {
                Key = key,
                NumberOfMessages = numberOfMessages,
                StartDate = startDate,
                ReadNew = true
            };

            var events = new List<LoadEventWrapper>
            {
                new LoadEventWrapper { Id = "1", Key = key, Endpoint = "/api/test1", Method = "GET", StatusCode = 200, Duration = TimeSpan.FromSeconds(1), TimestampUTC = startDate },
                new LoadEventWrapper { Id = "2", Key = key, Endpoint = "/api/test2", Method = "POST", StatusCode = 201, Duration = TimeSpan.FromSeconds(2), TimestampUTC = startDate.AddSeconds(10) }
            };

            var mappedResponses = new List<LoadEventResponse>
            {
                new LoadEventResponse { Endpoint = "/api/test1", Method = "GET", StatusCode = 200, Duration = TimeSpan.FromSeconds(1), TimestampUTC = startDate },
                new LoadEventResponse { Endpoint = "/api/test2", Method = "POST", StatusCode = 201, Duration = TimeSpan.FromSeconds(2), TimestampUTC = startDate.AddSeconds(10) }
            };

            mockReceiver.Setup(r => r.GetCertainAmountOfEventsAsync(It.IsAny<GetCertainMessageNumberOptions>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(events);

            mockMapper.Setup(m => m.Map<LoadEventResponse>(It.IsAny<LoadEventWrapper>()))
                .Returns((LoadEventWrapper e) => mappedResponses.First(r => r.Endpoint == e.Endpoint));

            // Act
            var result = await controller.GetSomeLoadEvents(request, CancellationToken.None);

            // Assert
            Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
            var response = (result.Result as OkObjectResult)?.Value as IEnumerable<LoadEventResponse>;
            Assert.IsNotNull(response);

            Assert.That(response.Count(), Is.EqualTo(mappedResponses.Count));

            mockReceiver.Verify(r => r.GetCertainAmountOfEventsAsync(It.IsAny<GetCertainMessageNumberOptions>(), It.IsAny<CancellationToken>()), Times.Once);
            mockMapper.Verify(m => m.Map<LoadEventResponse>(It.IsAny<LoadEventWrapper>()), Times.Exactly(events.Count));
        }
    }
}
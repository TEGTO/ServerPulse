using AnalyzerApi.Infrastructure.Dtos.Endpoints.Analyze.GetLoadEventsInDataRange;
using AnalyzerApi.Infrastructure.Dtos.Responses.Events;
using AnalyzerApi.Infrastructure.Models;
using AnalyzerApi.Infrastructure.Models.Wrappers;
using AnalyzerApi.Services.Receivers.Event;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace AnalyzerApi.Endpoints.Analyze.GetLoadEventsInDataRange.Tests
{
    [TestFixture]
    internal class GetLoadEventsInDataRangeControllerTests
    {
        private Mock<IEventReceiver<LoadEventWrapper>> mockReceiver;
        private Mock<IMapper> mockMapper;
        private GetLoadEventsInDataRangeController controller;

        [SetUp]
        public void Setup()
        {
            mockReceiver = new Mock<IEventReceiver<LoadEventWrapper>>();
            mockMapper = new Mock<IMapper>();

            controller = new GetLoadEventsInDataRangeController(mockReceiver.Object, mockMapper.Object);
        }

        [Test]
        public async Task GetLoadEventsInDataRange_ValidQuery_ReturnsMappedResponses()
        {
            // Arrange
            var key = "testKey";
            var from = DateTime.UtcNow.AddDays(-1);
            var to = DateTime.UtcNow;
            var request = new GetLoadEventsInDataRangeRequest
            {
                Key = key,
                From = from,
                To = to
            };

            var events = new List<LoadEventWrapper>
            {
                new LoadEventWrapper { Id = "1", Key = key, Endpoint = "/api/test1", Method = "GET", StatusCode = 200, Duration = TimeSpan.FromSeconds(1), TimestampUTC = from },
                new LoadEventWrapper { Id = "2", Key = key, Endpoint = "/api/test2", Method = "POST", StatusCode = 400, Duration = TimeSpan.FromSeconds(2), TimestampUTC = to }
            };

            var mappedResponses = new List<LoadEventResponse>
            {
                new LoadEventResponse { Endpoint = "/api/test1", Method = "GET", StatusCode = 200, Duration = TimeSpan.FromSeconds(1), TimestampUTC = from },
                new LoadEventResponse { Endpoint = "/api/test2", Method = "POST", StatusCode = 400, Duration = TimeSpan.FromSeconds(2), TimestampUTC = to }
            };

            mockReceiver.Setup(r => r.GetEventsInRangeAsync(It.IsAny<GetInRangeOptions>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(events);

            mockMapper.Setup(m => m.Map<LoadEventResponse>(It.IsAny<LoadEventWrapper>()))
                .Returns((LoadEventWrapper e) => mappedResponses.First(r => r.Endpoint == e.Endpoint));

            // Act
            var result = await controller.GetLoadEventsInDataRange(request, CancellationToken.None);

            // Assert
            Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
            var response = (result.Result as OkObjectResult)?.Value as IEnumerable<LoadEventResponse>;
            Assert.IsNotNull(response);

            Assert.That(response.Count(), Is.EqualTo(mappedResponses.Count));

            mockReceiver.Verify(r => r.GetEventsInRangeAsync(It.IsAny<GetInRangeOptions>(), It.IsAny<CancellationToken>()), Times.Once);
            mockMapper.Verify(m => m.Map<LoadEventResponse>(It.IsAny<LoadEventWrapper>()), Times.Exactly(events.Count));
        }
    }
}
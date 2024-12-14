using AnalyzerApi.Infrastructure.Dtos.Responses.Events;
using AnalyzerApi.Infrastructure.Models;
using AnalyzerApi.Infrastructure.Models.Wrappers;
using AnalyzerApi.Infrastructure.Requests;
using AnalyzerApi.Services.Receivers.Event;
using AutoMapper;
using Moq;

namespace AnalyzerApi.Command.Controllers.GetLoadEventsInDataRange.Tests
{
    [TestFixture]
    internal class GetLoadEventsInDataRangeQueryHandlerTests
    {
        private Mock<IEventReceiver<LoadEventWrapper>> mockReceiver;
        private Mock<IMapper> mockMapper;
        private GetLoadEventsInDataRangeQueryHandler handler;

        [SetUp]
        public void Setup()
        {
            mockReceiver = new Mock<IEventReceiver<LoadEventWrapper>>();
            mockMapper = new Mock<IMapper>();

            handler = new GetLoadEventsInDataRangeQueryHandler(mockReceiver.Object, mockMapper.Object);
        }

        [Test]
        public async Task Handle_ValidQuery_ReturnsMappedResponses()
        {
            // Arrange
            var key = "testKey";
            var from = DateTime.UtcNow.AddDays(-1);
            var to = DateTime.UtcNow;
            var request = new MessagesInRangeRequest { Key = key, From = from, To = to };
            var query = new GetLoadEventsInDataRangeQuery(request);

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
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.IsNotNull(result);
            Assert.That(result.Count(), Is.EqualTo(mappedResponses.Count));

            mockReceiver.Verify(r => r.GetEventsInRangeAsync(It.IsAny<GetInRangeOptions>(), It.IsAny<CancellationToken>()), Times.Once);
            mockMapper.Verify(m => m.Map<LoadEventResponse>(It.IsAny<LoadEventWrapper>()), Times.Exactly(events.Count));
        }
    }
}
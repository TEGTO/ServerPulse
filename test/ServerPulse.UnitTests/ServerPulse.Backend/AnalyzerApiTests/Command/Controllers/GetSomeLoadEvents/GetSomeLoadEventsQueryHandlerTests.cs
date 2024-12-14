using AnalyzerApi.Infrastructure.Dtos.Responses.Events;
using AnalyzerApi.Infrastructure.Models;
using AnalyzerApi.Infrastructure.Models.Wrappers;
using AnalyzerApi.Infrastructure.Requests;
using AnalyzerApi.Services.Receivers.Event;
using AutoMapper;
using Moq;

namespace AnalyzerApi.Command.Controllers.GetSomeLoadEvents.Tests
{
    [TestFixture]
    internal class GetSomeLoadEventsQueryHandlerTests
    {
        private Mock<IEventReceiver<LoadEventWrapper>> mockReceiver;
        private Mock<IMapper> mockMapper;
        private GetSomeLoadEventsQueryHandler handler;

        [SetUp]
        public void Setup()
        {
            mockReceiver = new Mock<IEventReceiver<LoadEventWrapper>>();
            mockMapper = new Mock<IMapper>();

            handler = new GetSomeLoadEventsQueryHandler(mockReceiver.Object, mockMapper.Object);
        }

        [Test]
        public async Task Handle_ValidQuery_ReturnsMappedResponses()
        {
            // Arrange
            var key = "testKey";
            var numberOfMessages = 5;
            var startDate = DateTime.UtcNow.AddHours(-1);
            var request = new GetSomeMessagesRequest { Key = key, NumberOfMessages = numberOfMessages, StartDate = startDate, ReadNew = true };
            var query = new GetSomeLoadEventsQuery(request);

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
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.IsNotNull(result);
            Assert.That(result.Count(), Is.EqualTo(mappedResponses.Count));

            mockReceiver.Verify(r => r.GetCertainAmountOfEventsAsync(It.IsAny<GetCertainMessageNumberOptions>(), It.IsAny<CancellationToken>()), Times.Once);
            mockMapper.Verify(m => m.Map<LoadEventResponse>(It.IsAny<LoadEventWrapper>()), Times.Exactly(events.Count));
        }
    }
}
using AnalyzerApi.Infrastructure.Dtos.Responses.Events;
using AnalyzerApi.Infrastructure.Models;
using AnalyzerApi.Infrastructure.Models.Wrappers;
using AnalyzerApi.Infrastructure.Requests;
using AnalyzerApi.Services.Receivers.Event;
using AutoMapper;
using Moq;

namespace AnalyzerApi.Command.Controllers.GetSomeCustomEvents.Tests
{
    [TestFixture]
    internal class GetSomeCustomEventsQueryHandlerTests
    {
        private Mock<IEventReceiver<CustomEventWrapper>> mockReceiver;
        private Mock<IMapper> mockMapper;
        private GetSomeCustomEventsQueryHandler handler;

        [SetUp]
        public void Setup()
        {
            mockReceiver = new Mock<IEventReceiver<CustomEventWrapper>>();
            mockMapper = new Mock<IMapper>();

            handler = new GetSomeCustomEventsQueryHandler(mockReceiver.Object, mockMapper.Object);
        }

        [Test]
        public async Task Handle_ValidQuery_ReturnsMappedResponses()
        {
            // Arrange
            var key = "testKey";
            var numberOfMessages = 5;
            var startDate = DateTime.UtcNow.AddHours(-1);
            var request = new GetSomeMessagesRequest { Key = key, NumberOfMessages = numberOfMessages, StartDate = startDate, ReadNew = true };
            var query = new GetSomeCustomEventsQuery(request);

            var events = new List<CustomEventWrapper>
            {
                new CustomEventWrapper { Id = "1", Key = key, Name = "Event1", Description = "Description1", SerializedMessage = "" },
                new CustomEventWrapper { Id = "2", Key = key, Name = "Event2", Description = "Description2", SerializedMessage = "" }
            };

            var mappedResponses = new List<CustomEventResponse>
            {
                new CustomEventResponse { Name = "Event1", Description = "Description1" },
                new CustomEventResponse { Name = "Event2", Description = "Description2" }
            };

            mockReceiver.Setup(r => r.GetCertainAmountOfEventsAsync(It.IsAny<GetCertainMessageNumberOptions>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(events);

            mockMapper.Setup(m => m.Map<CustomEventResponse>(It.IsAny<CustomEventWrapper>()))
                .Returns((CustomEventWrapper e) => mappedResponses.First(r => r.Name == e.Name));

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.IsNotNull(result);
            Assert.That(result.Count(), Is.EqualTo(mappedResponses.Count));

            mockReceiver.Verify(r => r.GetCertainAmountOfEventsAsync(It.IsAny<GetCertainMessageNumberOptions>(), It.IsAny<CancellationToken>()), Times.Once);
            mockMapper.Verify(m => m.Map<CustomEventResponse>(It.IsAny<CustomEventWrapper>()), Times.Exactly(events.Count));
        }
    }
}
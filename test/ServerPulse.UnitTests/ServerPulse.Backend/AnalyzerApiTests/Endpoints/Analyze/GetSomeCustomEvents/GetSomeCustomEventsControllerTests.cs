using AnalyzerApi.Application.Services.Receivers.Event;
using AnalyzerApi.Core.Dtos.Endpoints.Analyze.GetSomeCustomEvents;
using AnalyzerApi.Core.Dtos.Responses.Events;
using AnalyzerApi.Core.Models;
using AnalyzerApi.Core.Models.Wrappers;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace AnalyzerApi.Endpoints.Analyze.GetSomeCustomEvents.Tests
{
    [TestFixture]
    internal class GetSomeCustomEventsControllerTests
    {
        private Mock<IEventReceiver<CustomEventWrapper>> mockReceiver;
        private Mock<IMapper> mockMapper;
        private GetSomeCustomEventsController controller;

        [SetUp]
        public void Setup()
        {
            mockReceiver = new Mock<IEventReceiver<CustomEventWrapper>>();
            mockMapper = new Mock<IMapper>();

            controller = new GetSomeCustomEventsController(mockReceiver.Object, mockMapper.Object);
        }

        [Test]
        public async Task GetSomeCustomEvents_ValidQuery_ReturnsMappedResponses()
        {
            // Arrange
            var key = "testKey";
            var numberOfMessages = 5;
            var startDate = DateTime.UtcNow.AddHours(-1);
            var request = new GetSomeCustomEventsRequest { Key = key, NumberOfMessages = numberOfMessages, StartDate = startDate, ReadNew = true };

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
            var result = await controller.GetSomeCustomEvents(request, CancellationToken.None);

            // Assert
            Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
            var response = (result.Result as OkObjectResult)?.Value as IEnumerable<CustomEventResponse>;
            Assert.IsNotNull(response);

            Assert.That(response.Count(), Is.EqualTo(mappedResponses.Count));

            mockReceiver.Verify(r => r.GetCertainAmountOfEventsAsync(It.IsAny<GetCertainMessageNumberOptions>(), It.IsAny<CancellationToken>()), Times.Once);
            mockMapper.Verify(m => m.Map<CustomEventResponse>(It.IsAny<CustomEventWrapper>()), Times.Exactly(events.Count));
        }
    }
}
using AnalyzerApi.Command.Controllers.ProcessLoadEvents;
using EventCommunication;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace AnalyzerApi.Controllers.Tests
{
    [TestFixture]
    internal class EventProcessingControllerTests
    {
        private Mock<IMediator> mockMediator;
        private EventProcessingController controller;

        [SetUp]
        public void Setup()
        {
            mockMediator = new Mock<IMediator>();
            controller = new EventProcessingController(mockMediator.Object);
        }

        [Test]
        public async Task ProcessLoadEvents_ValidEvents_ReturnsOk()
        {
            // Arrange
            var events = new[]
            {
                new LoadEvent("key1", "/api/test", "GET", 200, TimeSpan.FromMilliseconds(150), DateTime.UtcNow),
                new LoadEvent("key1", "/api/test", "POST", 201, TimeSpan.FromMilliseconds(300), DateTime.UtcNow)
            };

            mockMediator
                .Setup(m => m.Send(It.IsAny<ProcessLoadEventsCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Unit.Value);

            // Act
            var result = await controller.ProcessLoadEvents(events, CancellationToken.None);

            // Assert
            Assert.IsInstanceOf<OkResult>(result);
            mockMediator.Verify(m => m.Send(It.IsAny<ProcessLoadEventsCommand>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
using AnalyzerApi.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;
using ServerPulse.EventCommunication.Events;

namespace AnalyzerApi.Controllers.Tests
{
    [TestFixture]
    internal class EventProcessingControllerTests
    {
        private Mock<IEventProcessor> mockEventProcessor;
        private EventProcessingController controller;

        [SetUp]
        public void Setup()
        {
            mockEventProcessor = new Mock<IEventProcessor>();

            controller = new EventProcessingController(
                mockEventProcessor.Object
            );
        }

        [Test]
        public async Task ProcessLoad_ValidRequests_CallProcessor()
        {
            // Arrange
            var request = new LoadEvent[] { new LoadEvent("key", "endpoint", "method", 200, TimeSpan.Zero, DateTime.MinValue) };
            // Act
            var result = await controller.ProcessLoad(request, CancellationToken.None);
            // Assert
            Assert.IsInstanceOf<OkResult>(result);
            mockEventProcessor.Verify(x => x.ProcessEventsAsync(request, It.IsAny<CancellationToken>()), Times.Once);
        }
        [Test()]
        public async Task ProcessLoad_InvalidRequests_BadRequest()
        {
            // Arrange
            var request = new LoadEvent[] { };
            // Act
            var result = await controller.ProcessLoad(request, CancellationToken.None);
            // Assert
            Assert.IsInstanceOf<BadRequestResult>(result);
            mockEventProcessor.Verify(x => x.ProcessEventsAsync(It.IsAny<LoadEvent[]>(), It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}
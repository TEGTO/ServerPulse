using EventCommunication;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using ServerMonitorApi.Command.SendConfiguration;
using ServerMonitorApi.Command.SendCustomEvents;
using ServerMonitorApi.Command.SendLoadEvents;
using ServerMonitorApi.Command.SendPulse;

namespace ServerMonitorApi.Controllers.Tests
{
    [TestFixture]
    internal class ServerInteractionControllerTests
    {
        private Mock<IMediator> mediatorMock;
        private ServerInteractionController controller;
        private CancellationToken cancellationToken;

        [SetUp]
        public void SetUp()
        {
            mediatorMock = new Mock<IMediator>();

            controller = new ServerInteractionController(mediatorMock.Object);
            cancellationToken = CancellationToken.None;
        }

        [Test]
        public async Task SendPulse_ValidRequest_ReturnsOk()
        {
            // Arrange
            var pulseEvent = new PulseEvent("key1", true);

            mediatorMock.Setup(m => m.Send(It.IsAny<SendPulseCommand>(), cancellationToken))
                .ReturnsAsync(Unit.Value);

            // Act
            var result = await controller.SendPulse(pulseEvent, cancellationToken);

            // Assert
            Assert.That(result, Is.TypeOf<OkResult>());
            mediatorMock.Verify(m => m.Send(It.Is<SendPulseCommand>(c => c.Event == pulseEvent), cancellationToken), Times.Once);
        }

        [Test]
        public async Task SendConfiguration_ValidRequest_ReturnsOk()
        {
            // Arrange
            var configurationEvent = new ConfigurationEvent("key1", TimeSpan.FromMinutes(5));

            mediatorMock.Setup(m => m.Send(It.IsAny<SendConfigurationCommand>(), cancellationToken))
                .ReturnsAsync(Unit.Value);

            // Act
            var result = await controller.SendConfiguration(configurationEvent, cancellationToken);

            // Assert
            Assert.That(result, Is.TypeOf<OkResult>());
            mediatorMock.Verify(m => m.Send(It.Is<SendConfigurationCommand>(c => c.Event == configurationEvent), cancellationToken), Times.Once);
        }

        [Test]
        public async Task SendLoadEvents_ValidRequest_ReturnsOk()
        {
            // Arrange
            var loadEvents = new[]
            {
                new LoadEvent("key1", "/endpoint1", "GET", 200, TimeSpan.FromMilliseconds(500), DateTime.UtcNow),
                new LoadEvent("key1", "/endpoint2", "POST", 201, TimeSpan.FromMilliseconds(300), DateTime.UtcNow)
            };

            mediatorMock.Setup(m => m.Send(It.IsAny<SendLoadEventsCommand>(), cancellationToken))
                .ReturnsAsync(Unit.Value);

            // Act
            var result = await controller.SendLoadEvents(loadEvents, cancellationToken);

            // Assert
            Assert.That(result, Is.TypeOf<OkResult>());
            mediatorMock.Verify(m => m.Send(It.Is<SendLoadEventsCommand>(c => c.Events == loadEvents), cancellationToken), Times.Once);
        }

        [Test]
        public async Task SendCustomEvents_ValidRequest_ReturnsOk()
        {
            // Arrange
            var customEventWrappers = new[]
            {
                new CustomEventContainer(new CustomEvent("key1", "Event1", "Description1"), "Serialized1"),
                new CustomEventContainer(new CustomEvent("key1", "Event2", "Description2"), "Serialized2")
            };

            mediatorMock.Setup(m => m.Send(It.IsAny<SendCustomEventsCommand>(), cancellationToken))
                .ReturnsAsync(Unit.Value);

            // Act
            var result = await controller.SendCustomEvents(customEventWrappers, cancellationToken);

            // Assert
            Assert.That(result, Is.TypeOf<OkResult>());
            mediatorMock.Verify(m => m.Send(It.Is<SendCustomEventsCommand>(c => c.Events == customEventWrappers), cancellationToken), Times.Once);
        }
    }
}
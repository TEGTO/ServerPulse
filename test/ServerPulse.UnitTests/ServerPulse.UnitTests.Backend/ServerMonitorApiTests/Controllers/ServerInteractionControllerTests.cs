using Microsoft.AspNetCore.Mvc;
using Moq;
using ServerMonitorApi.Controllers;
using ServerMonitorApi.Services;
using ServerPulse.EventCommunication;
using ServerPulse.EventCommunication.Events;

namespace ServerMonitorApiTests.Controllers
{
    [TestFixture]
    internal class ServerInteractionControllerTests
    {
        private Mock<IEventSender> mockEventSender;
        private Mock<ISlotKeyChecker> mockSlotChecker;
        private Mock<IEventProcessing> mockEventProcessing;
        private ServerInteractionController controller;

        [SetUp]
        public void Setup()
        {
            mockEventSender = new Mock<IEventSender>();
            mockSlotChecker = new Mock<ISlotKeyChecker>();
            mockEventProcessing = new Mock<IEventProcessing>();
            controller = new ServerInteractionController(mockEventSender.Object, mockSlotChecker.Object, mockEventProcessing.Object);
        }

        #region SendPulse Tests
        [Test]
        public async Task SendPulse_SlotKeyIsValid_CallsEventSender()
        {
            // Arrange
            var key = "validSlotKey";
            var pulseEvent = new PulseEvent(key, true);
            var cancellationToken = CancellationToken.None;
            mockSlotChecker.Setup(x => x.CheckSlotKeyAsync(key, cancellationToken)).ReturnsAsync(true);
            // Act
            var result = await controller.SendPulse(pulseEvent, cancellationToken);
            // Assert
            mockEventSender.Verify(x => x.SendEventsAsync(It.IsAny<PulseEvent[]>(), cancellationToken), Times.Once);
            Assert.IsInstanceOf<OkResult>(result);
        }
        [Test]
        public async Task SendPulse_SlotKeyIsNotFound_ReturnsNotFound()
        {
            // Arrange
            var key = "invalidSlotKey";
            var pulseEvent = new PulseEvent(key, true);
            var cancellationToken = CancellationToken.None;
            mockSlotChecker.Setup(x => x.CheckSlotKeyAsync(key, cancellationToken)).ReturnsAsync(false);
            // Act
            var result = await controller.SendPulse(pulseEvent, cancellationToken);
            // Assert
            Assert.IsInstanceOf<NotFoundObjectResult>(result);
            var notFoundResult = result as NotFoundObjectResult;
            Assert.That(notFoundResult.Value, Is.EqualTo($"Server slot with key '{key}' is not found!"));
        }

        #endregion

        #region SendConfiguration Tests

        [Test]
        public async Task SendConfiguration_SlotKeyIsValid_CallsEventSender()
        {
            // Arrange
            var key = "validSlotKey";
            var configurationEvent = new ConfigurationEvent(key, TimeSpan.FromMinutes(5));
            var cancellationToken = CancellationToken.None;
            mockSlotChecker.Setup(x => x.CheckSlotKeyAsync(key, cancellationToken)).ReturnsAsync(true);
            // Act
            var result = await controller.SendConfiguration(configurationEvent, cancellationToken);
            // Assert
            mockEventSender.Verify(x => x.SendEventsAsync(It.IsAny<ConfigurationEvent[]>(), cancellationToken), Times.Once);
            Assert.IsInstanceOf<OkResult>(result);
        }
        [Test]
        public async Task SendConfiguration_SlotKeyIsNotFound_ReturnsNotFound()
        {
            // Arrange
            var key = "invalidSlotKey";
            var configurationEvent = new ConfigurationEvent(key, TimeSpan.FromMinutes(5));
            var cancellationToken = CancellationToken.None;
            mockSlotChecker.Setup(x => x.CheckSlotKeyAsync(key, cancellationToken)).ReturnsAsync(false);
            // Act
            var result = await controller.SendConfiguration(configurationEvent, cancellationToken);
            // Assert
            Assert.IsInstanceOf<NotFoundObjectResult>(result);
            var notFoundResult = result as NotFoundObjectResult;
            Assert.That(notFoundResult.Value, Is.EqualTo($"Server slot with key '{key}' is not found!"));
        }
        #endregion

        #region SendLoadEvents Tests
        [Test]
        public async Task SendLoadEvents_DifferentKeys_ReturnsBadRequest()
        {
            // Arrange
            var loadEvents = new[]
            {
                new LoadEvent("key1", "endpoint1", "GET", 200, TimeSpan.FromSeconds(10), DateTime.Now),
                new LoadEvent("key2", "endpoint2", "POST", 404, TimeSpan.FromSeconds(20), DateTime.Now)
            };
            var cancellationToken = CancellationToken.None;
            // Act
            var result = await controller.SendLoadEvents(loadEvents, cancellationToken);
            // Assert
            Assert.IsInstanceOf<BadRequestObjectResult>(result);
            var badRequestResult = result as BadRequestObjectResult;
            Assert.That(badRequestResult.Value, Is.EqualTo("All load events must have the same key per request!"));
        }
        [Test]
        public async Task SendLoadEvents_SlotKeyIsValid_CallsEventSenderAndEventProcessing()
        {
            // Arrange
            var key = "validSlotKey";
            var loadEvents = new[]
            {
                new LoadEvent(key, "endpoint1", "GET", 200, TimeSpan.FromSeconds(10), DateTime.Now),
                new LoadEvent(key, "endpoint2", "POST", 404, TimeSpan.FromSeconds(20), DateTime.Now)
            };
            var cancellationToken = CancellationToken.None;
            mockSlotChecker.Setup(x => x.CheckSlotKeyAsync(key, cancellationToken)).ReturnsAsync(true);
            // Act
            var result = await controller.SendLoadEvents(loadEvents, cancellationToken);
            // Assert
            mockEventProcessing.Verify(x => x.SendEventsForProcessingsAsync(loadEvents, cancellationToken), Times.Once);
            mockEventSender.Verify(x => x.SendEventsAsync(loadEvents, cancellationToken), Times.Once);
            Assert.IsInstanceOf<OkResult>(result);
        }
        [Test]
        public async Task SendLoadEvents_SlotKeyIsNotFound_ReturnsNotFound()
        {
            // Arrange
            var key = "invalidSlotKey";
            var loadEvents = new[]
            {
                new LoadEvent(key, "endpoint1", "GET", 200, TimeSpan.FromSeconds(10), DateTime.Now),
                new LoadEvent(key, "endpoint2", "POST", 404, TimeSpan.FromSeconds(20), DateTime.Now)
            };
            var cancellationToken = CancellationToken.None;
            mockSlotChecker.Setup(x => x.CheckSlotKeyAsync(key, cancellationToken)).ReturnsAsync(false);
            // Act
            var result = await controller.SendLoadEvents(loadEvents, cancellationToken);
            // Assert
            Assert.IsInstanceOf<NotFoundObjectResult>(result);
            var notFoundResult = result as NotFoundObjectResult;
            Assert.That(notFoundResult.Value, Is.EqualTo($"Server slot with key '{key}' is not found!"));
        }
        #endregion

        #region SendCustomEvents Tests
        [Test]
        public async Task SendCustomEvents_EmptyCustomEventShells_ReturnsBadRequest()
        {
            // Arrange
            var customEventShells = new CustomEventShell[] { };
            var cancellationToken = CancellationToken.None;
            // Act
            var result = await controller.SendCustomEvents(customEventShells, cancellationToken);
            // Assert
            Assert.IsInstanceOf<BadRequestObjectResult>(result);
            var badRequestResult = result as BadRequestObjectResult;
            Assert.That(badRequestResult.Value, Is.EqualTo("Invalid custom event structure!"));
        }
        [Test]
        public async Task SendCustomEvents_DifferentKeys_ReturnsBadRequest()
        {
            // Arrange
            var customEventShells = new[]
            {
                new CustomEventShell(new CustomEvent("key1", "data1", "desc1")),
                new CustomEventShell(new CustomEvent("key2", "data2",  "desc2"))
            };
            var cancellationToken = CancellationToken.None;
            // Act
            var result = await controller.SendCustomEvents(customEventShells, cancellationToken);
            // Assert
            Assert.IsInstanceOf<BadRequestObjectResult>(result);
            var badRequestResult = result as BadRequestObjectResult;
            Assert.That(badRequestResult.Value, Is.EqualTo("All custom events must have the same key per request!"));
        }
        [Test]
        public async Task SendCustomEvents_SlotKeyIsValid_CallsEventSender()
        {
            // Arrange
            var key = "validSlotKey";
            var customEventShells = new[]
            {
                new CustomEventShell(new CustomEvent(key, "data1", "desc1")),
                new CustomEventShell(new CustomEvent(key, "data2",  "desc2"))
            };
            var cancellationToken = CancellationToken.None;
            mockSlotChecker.Setup(x => x.CheckSlotKeyAsync(key, cancellationToken)).ReturnsAsync(true);
            // Act
            var result = await controller.SendCustomEvents(customEventShells, cancellationToken);
            // Assert
            var serializedEvents = customEventShells.Select(x => x.CustomEventSerialized).ToArray();
            mockEventSender.Verify(x => x.SendCustomEventsAsync(key, serializedEvents, cancellationToken), Times.Once);
            Assert.IsInstanceOf<OkResult>(result);
        }
        [Test]
        public async Task SendCustomEvents_SlotKeyIsNotFound_ReturnsNotFound()
        {
            // Arrange
            var key = "invalidSlotKey";
            var customEventShells = new[]
            {
                new CustomEventShell(new CustomEvent(key, "data1", "desc1")),
                new CustomEventShell(new CustomEvent(key, "data2",  "desc2"))
            };
            var cancellationToken = CancellationToken.None;
            mockSlotChecker.Setup(x => x.CheckSlotKeyAsync(key, cancellationToken)).ReturnsAsync(false);
            // Act
            var result = await controller.SendCustomEvents(customEventShells, cancellationToken);
            // Assert
            Assert.IsInstanceOf<NotFoundObjectResult>(result);
            var notFoundResult = result as NotFoundObjectResult;
            Assert.That(notFoundResult.Value, Is.EqualTo($"Server slot with key '{key}' is not found!"));
        }
        #endregion
    }
}
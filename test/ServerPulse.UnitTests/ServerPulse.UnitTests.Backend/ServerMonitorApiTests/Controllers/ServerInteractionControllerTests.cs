using Microsoft.AspNetCore.Mvc;
using Moq;
using ServerMonitorApi.Controllers;
using ServerMonitorApi.Services;
using ServerPulse.EventCommunication.Events;

namespace ServerMonitorApiTests.Controllers
{
    [TestFixture]
    internal class ServerInteractionControllerTests
    {
        private Mock<IMessageSender> mockMessageSender;
        private Mock<ISlotKeyChecker> mockSlotChecker;
        private ServerInteractionController controller;

        [SetUp]
        public void Setup()
        {
            mockMessageSender = new Mock<IMessageSender>();
            mockSlotChecker = new Mock<ISlotKeyChecker>();
            controller = new ServerInteractionController(mockMessageSender.Object, mockSlotChecker.Object);
        }

        [Test]
        public async Task SendPulse_SlotKeyIsValid_CallsMessageSender()
        {
            // Arrange
            var key = "validSlotKey";
            var message = new PulseEvent(key, true);
            var cancellationToken = CancellationToken.None;
            mockSlotChecker.Setup(x => x.CheckSlotKeyAsync(key, cancellationToken)).ReturnsAsync(true);
            // Act
            var result = await controller.SendPulse(message, cancellationToken);
            // Assert
            mockMessageSender.Verify(x => x.SendPulseEventAsync(message, cancellationToken), Times.Once);
            Assert.IsInstanceOf<OkResult>(result);
        }
        [Test]
        public async Task SendPulse_SlotKeyIsNotFound_ReturnsNotFound()
        {
            // Arrange
            var key = "invalidSlotKey";
            var message = new PulseEvent(key, true);
            var cancellationToken = CancellationToken.None;
            mockSlotChecker.Setup(x => x.CheckSlotKeyAsync(key, cancellationToken)).ReturnsAsync(false);
            // Act
            var result = await controller.SendPulse(message, cancellationToken);
            // Assert
            Assert.IsInstanceOf<NotFoundObjectResult>(result);
            var notFoundResult = result as NotFoundObjectResult;
            Assert.That(notFoundResult.Value, Is.EqualTo($"Server slot with key '{key}' is not found!"));
        }
        [Test]
        public async Task SendConfiguration_SlotKeyIsValid_CallsMessageSender()
        {
            // Arrange
            var key = "validSlotKey";
            var configurationEvent = new ConfigurationEvent(key, TimeSpan.FromMinutes(5));
            var cancellationToken = CancellationToken.None;
            mockSlotChecker.Setup(x => x.CheckSlotKeyAsync(key, cancellationToken)).ReturnsAsync(true);
            // Act
            var result = await controller.SendConfiguration(configurationEvent, cancellationToken);
            // Assert
            mockMessageSender.Verify(x => x.SendConfigurationEventAsync(configurationEvent, cancellationToken), Times.Once);
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
        [Test]
        public async Task SendLoadEvents_EmptyArray_ReturnsBadRequest()
        {
            // Arrange
            var loadEvents = new LoadEvent[] { };
            var cancellationToken = CancellationToken.None;
            // Act + Assert
            Assert.ThrowsAsync<InvalidOperationException>(() => controller.SendLoadEvents(loadEvents, cancellationToken));
        }
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
        public async Task SendLoadEvents_SlotKeyIsValid_CallsMessageSender()
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
            mockMessageSender.Verify(x => x.SendLoadEventsAsync(loadEvents, cancellationToken), Times.Once);
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
    }
}
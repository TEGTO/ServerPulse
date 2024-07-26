using Microsoft.AspNetCore.Mvc;
using Moq;
using ServerInteractionApi.Controllers;
using ServerInteractionApi.Services;

namespace ServerInteractionApiTests.Controllers
{
    [TestFixture]
    public class ServerInteractionControllerTests
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
        public void SendAlive_SlotKeyIsNull_ThrowsArgumentException()
        {
            // Arrange
            string slotKey = null;
            var cancellationToken = CancellationToken.None;
            // Act & Assert
            var exception = Assert.ThrowsAsync<ArgumentException>(() => controller.SendAlive(slotKey, cancellationToken));
            Assert.That(exception.Message, Is.EqualTo("Key must be provided"));
        }

        [Test]
        public async Task SendAlive_SlotKeyIsValid_CallsMessageSender()
        {
            // Arrange
            var slotKey = "validSlotKey";
            var cancellationToken = CancellationToken.None;
            mockSlotChecker.Setup(x => x.CheckSlotKeyAsync(slotKey, cancellationToken)).ReturnsAsync(true);
            // Act
            var result = await controller.SendAlive(slotKey, cancellationToken);
            // Assert
            mockMessageSender.Verify(x => x.SendAliveEventAsync(slotKey, cancellationToken), Times.Once);
            Assert.IsInstanceOf<OkResult>(result);
        }
        [Test]
        public async Task SendAlive_SlotKeyIsNotFound_ReturnsNotFound()
        {
            // Arrange
            var slotKey = "invalidSlotKey";
            var cancellationToken = CancellationToken.None;
            mockSlotChecker.Setup(x => x.CheckSlotKeyAsync(slotKey, cancellationToken)).ReturnsAsync(false);
            // Act
            var result = await controller.SendAlive(slotKey, cancellationToken);
            // Assert
            Assert.IsInstanceOf<NotFoundObjectResult>(result);
            var notFoundResult = result as NotFoundObjectResult;
            Assert.That(notFoundResult.Value, Is.EqualTo($"Server slot with key '{slotKey}' is not found!"));
        }
    }
}
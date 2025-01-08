using EventCommunication;
using MessageBus.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Moq;
using ServerMonitorApi.Options;
using ServerMonitorApi.Services;

namespace ServerMonitorApi.Endpoints.ServerInteraction.SendCustomEvents.Tests
{
    [TestFixture]
    internal class SendCustomEventsControllerTests
    {
        private Mock<ISlotKeyChecker> slotKeyCheckerMock;
        private Mock<IMessageProducer> messageProducerMock;
        private SendCustomEventsController controller;
        private CancellationToken cancellationToken;

        [SetUp]
        public void SetUp()
        {
            slotKeyCheckerMock = new Mock<ISlotKeyChecker>();
            messageProducerMock = new Mock<IMessageProducer>();

            var settings = new MessageBusSettings
            {
                CustomTopic = "KafkaCustomTopic-",
            };

            var optionsMock = new Mock<IOptions<MessageBusSettings>>();
            optionsMock.Setup(x => x.Value).Returns(settings);

            controller = new SendCustomEventsController(slotKeyCheckerMock.Object, messageProducerMock.Object, optionsMock.Object);
            cancellationToken = CancellationToken.None;
        }

        [Test]
        public async Task SendCustomEvents_ValidEvents_ProducesMessagesAndReturnsOk()
        {
            // Arrange
            var events = new[]
            {
                new CustomEventContainer(
                    new CustomEvent("key1", "Event1", "Description1"),
                    "{\"Key\":\"key1\",\"Name\":\"Event1\",\"Description\":\"Description1\"}"
                ),
                new CustomEventContainer(
                    new CustomEvent("key1", "Event2", "Description2"),
                    "{\"Key\":\"key1\",\"Name\":\"Event2\",\"Description\":\"Description2\"}"
                )
            };
            var expectedTopic = "KafkaCustomTopic-key1";

            slotKeyCheckerMock.Setup(s => s.CheckSlotKeyAsync("key1", cancellationToken)).ReturnsAsync(true);
            messageProducerMock.Setup(p => p.ProduceAsync(It.IsAny<string>(), It.IsAny<string>(), cancellationToken)).Returns(Task.CompletedTask);

            // Act
            var result = await controller.SendCustomEvents(events, cancellationToken);

            // Assert
            Assert.That(result, Is.InstanceOf<OkResult>());

            slotKeyCheckerMock.Verify(s => s.CheckSlotKeyAsync("key1", cancellationToken), Times.Once);

            messageProducerMock.Verify(p => p.ProduceAsync(expectedTopic, It.IsAny<string>(), cancellationToken), Times.Exactly(2));
        }

        [Test]
        public async Task SendCustomEvents_EmptyEventArray_ReturnsBadRequest()
        {
            // Act 
            var result = await controller.SendCustomEvents(Array.Empty<CustomEventContainer>(), cancellationToken);

            // Assert
            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
            var response = (result as BadRequestObjectResult)?.Value as string;

            Assert.That(response, Is.EqualTo("Event array could not be null or empty!"));

            slotKeyCheckerMock.Verify(s => s.CheckSlotKeyAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
            messageProducerMock.Verify(p => p.ProduceAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public async Task SendCustomEvents_EventsWithDifferentKeys_ReturnsBadRequest()
        {
            // Arrange
            var events = new[]
            {
                new CustomEventContainer(new CustomEvent("key1", "Event1", "Description1"), "Serialized1"),
                new CustomEventContainer(new CustomEvent("key2", "Event2", "Description2"), "Serialized2")
            };

            // Act 
            var result = await controller.SendCustomEvents(events, cancellationToken);

            // Assert
            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
            var response = (result as BadRequestObjectResult)?.Value as string;

            Assert.That(response, Is.EqualTo("All events must have the same key per request!"));

            slotKeyCheckerMock.Verify(s => s.CheckSlotKeyAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
            messageProducerMock.Verify(p => p.ProduceAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public async Task SendCustomEvents_InvalidSlotKey_ReturnsBadRequest()
        {
            // Arrange
            var events = new[]
            {
                new CustomEventContainer(new CustomEvent("key1", "Event1", "Description1"), "Serialized1")
            };

            slotKeyCheckerMock.Setup(s => s.CheckSlotKeyAsync("key1", cancellationToken)).ReturnsAsync(false);

            // Act 
            var result = await controller.SendCustomEvents(events, cancellationToken);

            // Assert
            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
            var response = (result as BadRequestObjectResult)?.Value as string;

            Assert.That(response, Is.EqualTo("Server slot with key 'key1' is not found!"));

            slotKeyCheckerMock.Verify(s => s.CheckSlotKeyAsync("key1", cancellationToken), Times.Once);
            messageProducerMock.Verify(p => p.ProduceAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public async Task SendCustomEvents_InvalidSerializedEvents_ReturnsConflict()
        {
            var events = new[]
           {
                new CustomEventContainer(
                    new CustomEvent("key1", "Event1", "Description1"),
                    ""
                ),
                new CustomEventContainer(
                    new CustomEvent("key1", "Event2", "Description2"),
                    ""
                )
            };

            slotKeyCheckerMock.Setup(s => s.CheckSlotKeyAsync("key1", cancellationToken)).ReturnsAsync(true);
            messageProducerMock.Setup(p => p.ProduceAsync(It.IsAny<string>(), It.IsAny<string>(), cancellationToken)).Returns(Task.CompletedTask);

            // Act
            var result = await controller.SendCustomEvents(events, cancellationToken);

            // Assert
            Assert.That(result, Is.InstanceOf<ConflictObjectResult>());
            var response = (result as ConflictObjectResult)?.Value as string;

            Assert.That(response, Is.EqualTo("Serialized event could not be deserialized!"));

            slotKeyCheckerMock.Verify(s => s.CheckSlotKeyAsync("key1", cancellationToken), Times.Once);
            messageProducerMock.Verify(p => p.ProduceAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}
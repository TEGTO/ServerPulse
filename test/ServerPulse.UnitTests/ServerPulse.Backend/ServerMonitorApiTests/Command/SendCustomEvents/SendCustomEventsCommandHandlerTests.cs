using EventCommunication;
using MessageBus.Interfaces;
using Microsoft.Extensions.Configuration;
using Moq;
using ServerMonitorApi.Services;

namespace ServerMonitorApi.Command.SendCustomEvents.Tests
{
    [TestFixture]
    internal class SendCustomEventsCommandHandlerTests
    {
        private Mock<ISlotKeyChecker> slotKeyCheckerMock;
        private Mock<IMessageProducer> messageProducerMock;
        private Mock<IConfiguration> configurationMock;
        private SendCustomEventsCommandHandler handler;
        private CancellationToken cancellationToken;

        [SetUp]
        public void SetUp()
        {
            slotKeyCheckerMock = new Mock<ISlotKeyChecker>();
            messageProducerMock = new Mock<IMessageProducer>();
            configurationMock = new Mock<IConfiguration>();
            configurationMock.Setup(c => c[Configuration.KAFKA_CUSTOM_TOPIC]).Returns("KafkaCustomTopic-");

            handler = new SendCustomEventsCommandHandler(slotKeyCheckerMock.Object, messageProducerMock.Object, configurationMock.Object);
            cancellationToken = CancellationToken.None;
        }

        [Test]
        public async Task Handle_ValidEvents_ProducesMessages()
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

            var command = new SendCustomEventsCommand(events);

            // Act
            await handler.Handle(command, cancellationToken);

            // Assert
            slotKeyCheckerMock.Verify(s => s.CheckSlotKeyAsync("key1", cancellationToken), Times.Once);

            messageProducerMock.Verify(p => p.ProduceAsync(expectedTopic, It.IsAny<string>(), cancellationToken), Times.Exactly(2));
        }

        [Test]
        public void Handle_EmptyEventArray_ThrowsInvalidDataException()
        {
            // Arrange
            var command = new SendCustomEventsCommand(Array.Empty<CustomEventContainer>());

            // Act & Assert
            Assert.ThrowsAsync<InvalidDataException>(() => handler.Handle(command, cancellationToken));

            slotKeyCheckerMock.Verify(s => s.CheckSlotKeyAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
            messageProducerMock.Verify(p => p.ProduceAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public void Handle_NullEventArray_ThrowsInvalidDataException()
        {
            // Arrange
            var command = new SendCustomEventsCommand(null!);

            // Act & Assert
            Assert.ThrowsAsync<InvalidDataException>(() => handler.Handle(command, cancellationToken));

            slotKeyCheckerMock.Verify(s => s.CheckSlotKeyAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
            messageProducerMock.Verify(p => p.ProduceAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public void Handle_EventsWithDifferentKeys_ThrowsInvalidOperationException()
        {
            // Arrange
            var events = new[]
            {
                new CustomEventContainer(new CustomEvent("key1", "Event1", "Description1"), "Serialized1"),
                new CustomEventContainer(new CustomEvent("key2", "Event2", "Description2"), "Serialized2")
            };

            var command = new SendCustomEventsCommand(events);

            // Act & Assert
            var ex = Assert.ThrowsAsync<InvalidOperationException>(() => handler.Handle(command, cancellationToken));
            Assert.That(ex.Message, Is.EqualTo("All events must have the same key per request!"));

            slotKeyCheckerMock.Verify(s => s.CheckSlotKeyAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
            messageProducerMock.Verify(p => p.ProduceAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public void Handle_InvalidSlotKey_ThrowsInvalidOperationException()
        {
            // Arrange
            var events = new[]
            {
                new CustomEventContainer(new CustomEvent("key1", "Event1", "Description1"), "Serialized1")
            };

            slotKeyCheckerMock.Setup(s => s.CheckSlotKeyAsync("key1", cancellationToken)).ReturnsAsync(false);

            var command = new SendCustomEventsCommand(events);

            // Act & Assert
            var ex = Assert.ThrowsAsync<InvalidOperationException>(() => handler.Handle(command, cancellationToken));
            Assert.That(ex.Message, Is.EqualTo("Server slot with key 'key1' is not found!"));

            slotKeyCheckerMock.Verify(s => s.CheckSlotKeyAsync("key1", cancellationToken), Times.Once);
            messageProducerMock.Verify(p => p.ProduceAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}
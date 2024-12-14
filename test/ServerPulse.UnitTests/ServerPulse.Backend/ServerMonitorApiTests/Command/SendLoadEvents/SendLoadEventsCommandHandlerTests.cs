using EventCommunication;
using MessageBus.Interfaces;
using Microsoft.Extensions.Configuration;
using Moq;
using ServerMonitorApi.Services;

namespace ServerMonitorApi.Command.SendLoadEvents.Tests
{
    [TestFixture]
    internal class SendLoadEventsCommandHandlerTests
    {
        private Mock<ISlotKeyChecker> slotKeyCheckerMock;
        private Mock<IMessageProducer> messageProducerMock;
        private Mock<IStatisticsEventSender> statisticsEventSenderMock;
        private Mock<IConfiguration> configurationMock;
        private SendLoadEventsCommandHandler handler;
        private CancellationToken cancellationToken;

        [SetUp]
        public void SetUp()
        {
            slotKeyCheckerMock = new Mock<ISlotKeyChecker>();
            messageProducerMock = new Mock<IMessageProducer>();
            statisticsEventSenderMock = new Mock<IStatisticsEventSender>();
            configurationMock = new Mock<IConfiguration>();

            configurationMock.Setup(c => c[Configuration.KAFKA_LOAD_TOPIC]).Returns("KafkaLoadTopic-");

            handler = new SendLoadEventsCommandHandler(
                slotKeyCheckerMock.Object,
                messageProducerMock.Object,
                statisticsEventSenderMock.Object,
                configurationMock.Object
            );

            cancellationToken = CancellationToken.None;
        }

        [Test]
        public async Task Handle_ValidEvents_ProducesMessagesAndSendsToStatistics()
        {
            // Arrange
            var events = new[]
            {
                new LoadEvent("key1", "/endpoint1", "GET", 200, TimeSpan.FromMilliseconds(500), DateTime.UtcNow),
                new LoadEvent("key1", "/endpoint2", "POST", 201, TimeSpan.FromMilliseconds(300), DateTime.UtcNow)
            };
            var expectedTopic = "KafkaLoadTopic-key1";

            slotKeyCheckerMock.Setup(s => s.CheckSlotKeyAsync("key1", cancellationToken)).ReturnsAsync(true);
            messageProducerMock.Setup(p => p.ProduceAsync(It.IsAny<string>(), It.IsAny<string>(), cancellationToken)).Returns(Task.CompletedTask);

            var command = new SendLoadEventsCommand(events);

            // Act
            await handler.Handle(command, cancellationToken);

            // Assert
            slotKeyCheckerMock.Verify(s => s.CheckSlotKeyAsync("key1", cancellationToken), Times.Once);
            messageProducerMock.Verify(p => p.ProduceAsync(expectedTopic, It.IsAny<string>(), cancellationToken), Times.Exactly(2));
            statisticsEventSenderMock.Verify(h => h.SendLoadEventForStatistics(It.IsAny<LoadEvent>(), cancellationToken), Times.Exactly(2));
        }

        [Test]
        public void Handle_EmptyEventArray_ThrowsInvalidDataException()
        {
            // Arrange
            var command = new SendLoadEventsCommand(Array.Empty<LoadEvent>());

            // Act & Assert
            Assert.ThrowsAsync<InvalidDataException>(() => handler.Handle(command, cancellationToken));

            slotKeyCheckerMock.Verify(s => s.CheckSlotKeyAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
            messageProducerMock.Verify(p => p.ProduceAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
            statisticsEventSenderMock.Verify(h => h.SendLoadEventForStatistics(It.IsAny<LoadEvent>(), cancellationToken), Times.Never);
        }

        [Test]
        public void Handle_NullEventArray_ThrowsInvalidDataException()
        {
            // Arrange
            var command = new SendLoadEventsCommand(null!);

            // Act & Assert
            Assert.ThrowsAsync<InvalidDataException>(() => handler.Handle(command, cancellationToken));

            slotKeyCheckerMock.Verify(s => s.CheckSlotKeyAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
            messageProducerMock.Verify(p => p.ProduceAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
            statisticsEventSenderMock.Verify(h => h.SendLoadEventForStatistics(It.IsAny<LoadEvent>(), cancellationToken), Times.Never);
        }

        [Test]
        public void Handle_EventsWithDifferentKeys_ThrowsInvalidOperationException()
        {
            // Arrange
            var events = new[]
            {
                new LoadEvent("key1", "/endpoint1", "GET", 200, TimeSpan.FromMilliseconds(500), DateTime.UtcNow),
                new LoadEvent("key2", "/endpoint2", "POST", 201, TimeSpan.FromMilliseconds(300), DateTime.UtcNow)
            };

            var command = new SendLoadEventsCommand(events);

            // Act & Assert
            var ex = Assert.ThrowsAsync<InvalidOperationException>(() => handler.Handle(command, cancellationToken));
            Assert.That(ex.Message, Is.EqualTo("All events must have the same key per request!"));

            slotKeyCheckerMock.Verify(s => s.CheckSlotKeyAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
            messageProducerMock.Verify(p => p.ProduceAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
            statisticsEventSenderMock.Verify(h => h.SendLoadEventForStatistics(It.IsAny<LoadEvent>(), cancellationToken), Times.Never);
        }

        [Test]
        public void Handle_InvalidSlotKey_ThrowsInvalidOperationException()
        {
            // Arrange
            var events = new[]
            {
                new LoadEvent("key1", "/endpoint1", "GET", 200, TimeSpan.FromMilliseconds(500), DateTime.UtcNow)
            };

            slotKeyCheckerMock.Setup(s => s.CheckSlotKeyAsync("key1", cancellationToken)).ReturnsAsync(false);

            var command = new SendLoadEventsCommand(events);

            // Act & Assert
            var ex = Assert.ThrowsAsync<InvalidOperationException>(() => handler.Handle(command, cancellationToken));
            Assert.That(ex.Message, Is.EqualTo("Server slot with key 'key1' is not found!"));

            slotKeyCheckerMock.Verify(s => s.CheckSlotKeyAsync("key1", cancellationToken), Times.Once);
            messageProducerMock.Verify(p => p.ProduceAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
            statisticsEventSenderMock.Verify(h => h.SendLoadEventForStatistics(It.IsAny<LoadEvent>(), cancellationToken), Times.Never);
        }
    }
}
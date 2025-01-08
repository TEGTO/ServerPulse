using EventCommunication;
using MessageBus.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Moq;
using ServerMonitorApi.Options;
using ServerMonitorApi.Services;

namespace ServerMonitorApi.Endpoints.ServerInteraction.SendLoadEvents.Tests
{
    [TestFixture]
    internal class SendLoadEventsControllerTests
    {
        const string KAFKA_LOAD_TOPIC_PROCESS = "ProcessLoadEvent";

        private Mock<ISlotKeyChecker> slotKeyCheckerMock;
        private Mock<IMessageProducer> messageProducerMock;
        private SendLoadEventsController controller;
        private CancellationToken cancellationToken;

        [SetUp]
        public void SetUp()
        {
            slotKeyCheckerMock = new Mock<ISlotKeyChecker>();
            messageProducerMock = new Mock<IMessageProducer>();

            var settings = new MessageBusSettings
            {
                LoadTopic = "KafkaLoadTopic-",
                LoadTopicProcess = KAFKA_LOAD_TOPIC_PROCESS,
            };

            var optionsMock = new Mock<IOptions<MessageBusSettings>>();
            optionsMock.Setup(x => x.Value).Returns(settings);

            controller = new SendLoadEventsController(
                slotKeyCheckerMock.Object,
                messageProducerMock.Object,
                optionsMock.Object
            );

            cancellationToken = CancellationToken.None;
        }

        [Test]
        public async Task SendLoadEvents_ValidEvents_ProducesMessagesAndSendsToStatisticsReturnsOk()
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

            // Act
            var result = await controller.SendLoadEvents(events, cancellationToken);

            // Assert
            Assert.That(result, Is.InstanceOf<OkResult>());

            slotKeyCheckerMock.Verify(s => s.CheckSlotKeyAsync("key1", cancellationToken), Times.Once);
            messageProducerMock.Verify(p => p.ProduceAsync(expectedTopic, It.IsAny<string>(), cancellationToken), Times.Exactly(2));
            messageProducerMock.Verify(h => h.ProduceAsync(KAFKA_LOAD_TOPIC_PROCESS, It.IsAny<string>(), cancellationToken), Times.Exactly(2));
        }

        [Test]
        public async Task SendLoadEvents_EmptyEventArray_ReturnsBadRequest()
        {
            // Act
            var result = await controller.SendLoadEvents(Array.Empty<LoadEvent>(), cancellationToken);

            // Assert
            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
            var response = (result as BadRequestObjectResult)?.Value as string;

            Assert.That(response, Is.EqualTo("Event array could not be null or empty!"));

            slotKeyCheckerMock.Verify(s => s.CheckSlotKeyAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
            messageProducerMock.Verify(p => p.ProduceAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
            messageProducerMock.Verify(h => h.ProduceAsync(KAFKA_LOAD_TOPIC_PROCESS, It.IsAny<string>(), cancellationToken), Times.Never);
        }

        [Test]
        public async Task SendLoadEvents_EventsWithDifferentKeys_ReturnsBadRequest()
        {
            // Arrange
            var events = new[]
            {
                new LoadEvent("key1", "/endpoint1", "GET", 200, TimeSpan.FromMilliseconds(500), DateTime.UtcNow),
                new LoadEvent("key2", "/endpoint2", "POST", 201, TimeSpan.FromMilliseconds(300), DateTime.UtcNow)
            };

            // Act
            var result = await controller.SendLoadEvents(events, cancellationToken);

            // Assert
            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
            var response = (result as BadRequestObjectResult)?.Value as string;

            Assert.That(response, Is.EqualTo("All events must have the same key per request!"));

            slotKeyCheckerMock.Verify(s => s.CheckSlotKeyAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
            messageProducerMock.Verify(p => p.ProduceAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
            messageProducerMock.Verify(h => h.ProduceAsync(KAFKA_LOAD_TOPIC_PROCESS, It.IsAny<string>(), cancellationToken), Times.Never);
        }

        [Test]
        public async Task SendLoadEvents_InvalidSlotKey_ReturnsBadRequest()
        {
            // Arrange
            var events = new[]
            {
                new LoadEvent("key1", "/endpoint1", "GET", 200, TimeSpan.FromMilliseconds(500), DateTime.UtcNow)
            };

            slotKeyCheckerMock.Setup(s => s.CheckSlotKeyAsync("key1", cancellationToken)).ReturnsAsync(false);

            // Act
            var result = await controller.SendLoadEvents(events, cancellationToken);

            // Assert
            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
            var response = (result as BadRequestObjectResult)?.Value as string;

            Assert.That(response, Is.EqualTo("Server slot with key 'key1' is not found!"));

            slotKeyCheckerMock.Verify(s => s.CheckSlotKeyAsync("key1", cancellationToken), Times.Once);
            messageProducerMock.Verify(p => p.ProduceAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
            messageProducerMock.Verify(h => h.ProduceAsync(KAFKA_LOAD_TOPIC_PROCESS, It.IsAny<string>(), cancellationToken), Times.Never);
        }
    }
}
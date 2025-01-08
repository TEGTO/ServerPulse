using EventCommunication;
using MessageBus.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Moq;
using ServerMonitorApi.Options;
using ServerMonitorApi.Services;

namespace ServerMonitorApi.Endpoints.ServerInteraction.SendPulse.Tests
{
    [TestFixture]
    internal class SendPulseControllerTests
    {
        private Mock<ISlotKeyChecker> slotKeyCheckerMock;
        private Mock<IMessageProducer> messageProducerMock;
        private SendPulseController controller;
        private CancellationToken cancellationToken;

        [SetUp]
        public void SetUp()
        {
            slotKeyCheckerMock = new Mock<ISlotKeyChecker>();
            messageProducerMock = new Mock<IMessageProducer>();

            var settings = new MessageBusSettings
            {
                AliveTopic = "KafkaAliveTopic-",
            };

            var optionsMock = new Mock<IOptions<MessageBusSettings>>();
            optionsMock.Setup(x => x.Value).Returns(settings);

            controller = new SendPulseController(slotKeyCheckerMock.Object, messageProducerMock.Object, optionsMock.Object);
            cancellationToken = CancellationToken.None;
        }

        private static IEnumerable<TestCaseData> ValidPulseEventTestCases()
        {
            yield return new TestCaseData(
                "key1",
                true,
                "KafkaAliveTopic-key1"
            ).SetDescription("Pulse event with key 'key1' and IsAlive=true should produce correct message.");

            yield return new TestCaseData(
                "testKey",
                false,
                "KafkaAliveTopic-testKey"
            ).SetDescription("Pulse event with key 'testKey' and IsAlive=false should produce correct message.");
        }

        [Test]
        [TestCaseSource(nameof(ValidPulseEventTestCases))]
        public async Task SendPulse_ValidPulseEvent_ProducesMessageAndReturnsOk(string key, bool isAlive, string expectedTopic)
        {
            // Arrange
            var pulseEvent = new PulseEvent(key, isAlive);

            slotKeyCheckerMock.Setup(s => s.CheckSlotKeyAsync(pulseEvent.Key, cancellationToken)).ReturnsAsync(true);
            messageProducerMock.Setup(p => p.ProduceAsync(It.IsAny<string>(), It.IsAny<string>(), cancellationToken)).Returns(Task.CompletedTask);

            // Act
            var result = await controller.SendPulse(pulseEvent, cancellationToken);

            // Assert
            Assert.That(result, Is.TypeOf<OkResult>());

            slotKeyCheckerMock.Verify(s => s.CheckSlotKeyAsync(pulseEvent.Key, cancellationToken), Times.Once);
            messageProducerMock.Verify(p => p.ProduceAsync(expectedTopic, It.IsAny<string>(), cancellationToken), Times.Once);
        }

        [Test]
        public async Task SendPulse_InvalidSlotKey_ReturnsBadRequest()
        {
            // Arrange
            var pulseEvent = new PulseEvent("invalidKey", true);

            slotKeyCheckerMock.Setup(s => s.CheckSlotKeyAsync(pulseEvent.Key, cancellationToken)).ReturnsAsync(false);

            // Act
            var result = await controller.SendPulse(pulseEvent, cancellationToken);

            // Assert
            var response = (result as BadRequestObjectResult)?.Value as string;

            Assert.That(response, Is.EqualTo("Server slot with key 'invalidKey' is not found!"));

            slotKeyCheckerMock.Verify(s => s.CheckSlotKeyAsync(pulseEvent.Key, cancellationToken), Times.Once);
            messageProducerMock.Verify(p => p.ProduceAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}
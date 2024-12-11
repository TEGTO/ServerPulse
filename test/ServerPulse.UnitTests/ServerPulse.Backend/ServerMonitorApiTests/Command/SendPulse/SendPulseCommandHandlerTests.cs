using EventCommunication.Events;
using MessageBus.Interfaces;
using Microsoft.Extensions.Configuration;
using Moq;
using ServerMonitorApi.Services;

namespace ServerMonitorApi.Command.SendPulse.Tests
{
    [TestFixture]
    internal class SendPulseCommandHandlerTests
    {
        private Mock<ISlotKeyChecker> slotKeyCheckerMock;
        private Mock<IMessageProducer> messageProducerMock;
        private Mock<IConfiguration> configurationMock;
        private SendPulseCommandHandler handler;
        private CancellationToken cancellationToken;

        [SetUp]
        public void SetUp()
        {
            slotKeyCheckerMock = new Mock<ISlotKeyChecker>();
            messageProducerMock = new Mock<IMessageProducer>();
            configurationMock = new Mock<IConfiguration>();

            configurationMock.Setup(c => c[Configuration.KAFKA_ALIVE_TOPIC]).Returns("KafkaAliveTopic-");

            handler = new SendPulseCommandHandler(slotKeyCheckerMock.Object, messageProducerMock.Object, configurationMock.Object);
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
        public async Task Handle_ValidPulseEvent_ProducesMessage(string key, bool isAlive, string expectedTopic)
        {
            // Arrange
            var pulseEvent = new PulseEvent(key, isAlive);

            slotKeyCheckerMock.Setup(s => s.CheckSlotKeyAsync(pulseEvent.Key, cancellationToken)).ReturnsAsync(true);
            messageProducerMock.Setup(p => p.ProduceAsync(It.IsAny<string>(), It.IsAny<string>(), cancellationToken)).Returns(Task.CompletedTask);

            var command = new SendPulseCommand(pulseEvent);

            // Act
            await handler.Handle(command, cancellationToken);

            // Assert
            slotKeyCheckerMock.Verify(s => s.CheckSlotKeyAsync(pulseEvent.Key, cancellationToken), Times.Once);
            messageProducerMock.Verify(p => p.ProduceAsync(expectedTopic, It.IsAny<string>(), cancellationToken), Times.Once);
        }

        [Test]
        public void Handle_InvalidSlotKey_ThrowsInvalidOperationException()
        {
            // Arrange
            var pulseEvent = new PulseEvent("invalidKey", true);

            slotKeyCheckerMock.Setup(s => s.CheckSlotKeyAsync(pulseEvent.Key, cancellationToken)).ReturnsAsync(false);

            var command = new SendPulseCommand(pulseEvent);

            // Act & Assert
            var ex = Assert.ThrowsAsync<InvalidOperationException>(() => handler.Handle(command, cancellationToken));
            Assert.That(ex.Message, Is.EqualTo("Server slot with key 'invalidKey' is not found!"));

            slotKeyCheckerMock.Verify(s => s.CheckSlotKeyAsync(pulseEvent.Key, cancellationToken), Times.Once);
            messageProducerMock.Verify(p => p.ProduceAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}
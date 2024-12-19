using EventCommunication;
using MessageBus.Interfaces;
using Microsoft.Extensions.Configuration;
using Moq;
using ServerMonitorApi.Services;

namespace ServerMonitorApi.Command.SendConfiguration.Tests
{
    [TestFixture]
    internal class SendConfigurationCommandHandlerTests
    {
        private Mock<ISlotKeyChecker> slotKeyCheckerMock;
        private Mock<IMessageProducer> messageProducerMock;
        private Mock<IConfiguration> configurationMock;
        private SendConfigurationCommandHandler handler;
        private CancellationToken cancellationToken;

        [SetUp]
        public void SetUp()
        {
            slotKeyCheckerMock = new Mock<ISlotKeyChecker>();
            messageProducerMock = new Mock<IMessageProducer>();
            configurationMock = new Mock<IConfiguration>();
            configurationMock.Setup(c => c[Configuration.KAFKA_CONFIGURATION_TOPIC]).Returns("KafkaConfigTopic-");

            handler = new SendConfigurationCommandHandler(slotKeyCheckerMock.Object, messageProducerMock.Object, configurationMock.Object);
            cancellationToken = CancellationToken.None;
        }

        [Test]
        public async Task Handle_ValidEvent_ProducesMessage()
        {
            // Arrange
            var key = "key1";
            var timeSpan = TimeSpan.FromMinutes(5);
            var expectedTopic = "KafkaConfigTopic-key1";

            var configEvent = new ConfigurationEvent(key, timeSpan);

            slotKeyCheckerMock.Setup(s => s.CheckSlotKeyAsync(configEvent.Key, cancellationToken)).ReturnsAsync(true);
            messageProducerMock.Setup(p => p.ProduceAsync(It.IsAny<string>(), It.IsAny<string>(), cancellationToken)).Returns(Task.CompletedTask);

            // Act
            var command = new SendConfigurationCommand(configEvent);
            await handler.Handle(command, cancellationToken);

            // Assert
            slotKeyCheckerMock.Verify(s => s.CheckSlotKeyAsync(configEvent.Key, cancellationToken), Times.Once);
            messageProducerMock.Verify(p => p.ProduceAsync(expectedTopic, It.IsAny<string>(), cancellationToken), Times.Once);
        }

        [Test]
        public void Handle_InvalidSlotKey_ThrowsInvalidOperationException()
        {
            // Arrange
            var configEvent = new ConfigurationEvent("invalidKey", TimeSpan.FromMinutes(1));

            slotKeyCheckerMock.Setup(s => s.CheckSlotKeyAsync(configEvent.Key, cancellationToken)).ReturnsAsync(false);

            var command = new SendConfigurationCommand(configEvent);

            // Act & Assert
            var ex = Assert.ThrowsAsync<InvalidOperationException>(() => handler.Handle(command, cancellationToken));
            Assert.That(ex.Message, Is.EqualTo($"Server slot with key '{configEvent.Key}' is not found!"));

            slotKeyCheckerMock.Verify(s => s.CheckSlotKeyAsync(configEvent.Key, cancellationToken), Times.Once);
            messageProducerMock.Verify(p => p.ProduceAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}
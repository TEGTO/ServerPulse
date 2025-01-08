using EventCommunication;
using MessageBus.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Moq;
using ServerMonitorApi.Options;
using ServerMonitorApi.Services;

namespace ServerMonitorApi.Endpoints.ServerInteraction.SendConfiguration.Tests
{
    [TestFixture]
    internal class SendConfigurationControllerTests
    {
        private Mock<ISlotKeyChecker> slotKeyCheckerMock;
        private Mock<IMessageProducer> messageProducerMock;
        private SendConfigurationController controller;
        private CancellationToken cancellationToken;

        [SetUp]
        public void SetUp()
        {
            slotKeyCheckerMock = new Mock<ISlotKeyChecker>();
            messageProducerMock = new Mock<IMessageProducer>();

            var settings = new MessageBusSettings
            {
                ConfigurationTopic = "KafkaConfigTopic-",
            };

            var optionsMock = new Mock<IOptions<MessageBusSettings>>();
            optionsMock.Setup(x => x.Value).Returns(settings);

            controller = new SendConfigurationController(slotKeyCheckerMock.Object, messageProducerMock.Object, optionsMock.Object);
            cancellationToken = CancellationToken.None;
        }

        [Test]
        public async Task SendConfiguration_ValidEvent_ProducesMessageReturnsOk()
        {
            // Arrange
            var key = "key1";
            var timeSpan = TimeSpan.FromMinutes(5);
            var expectedTopic = "KafkaConfigTopic-key1";

            var configEvent = new ConfigurationEvent(key, timeSpan);

            slotKeyCheckerMock.Setup(s => s.CheckSlotKeyAsync(configEvent.Key, cancellationToken)).ReturnsAsync(true);
            messageProducerMock.Setup(p => p.ProduceAsync(It.IsAny<string>(), It.IsAny<string>(), cancellationToken)).Returns(Task.CompletedTask);

            // Act
            var result = await controller.SendConfiguration(configEvent, cancellationToken);

            // Assert
            Assert.That(result, Is.InstanceOf<OkResult>());

            slotKeyCheckerMock.Verify(s => s.CheckSlotKeyAsync(configEvent.Key, cancellationToken), Times.Once);
            messageProducerMock.Verify(p => p.ProduceAsync(expectedTopic, It.IsAny<string>(), cancellationToken), Times.Once);
        }

        [Test]
        public async Task SendConfiguration_InvalidSlotKey_ReturnsBadRequest()
        {
            // Arrange
            var configEvent = new ConfigurationEvent("invalidKey", TimeSpan.FromMinutes(1));

            slotKeyCheckerMock.Setup(s => s.CheckSlotKeyAsync(configEvent.Key, cancellationToken)).ReturnsAsync(false);

            // Act
            var result = await controller.SendConfiguration(configEvent, cancellationToken);

            // Assert
            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());

            slotKeyCheckerMock.Verify(s => s.CheckSlotKeyAsync(configEvent.Key, cancellationToken), Times.Once);
            messageProducerMock.Verify(p => p.ProduceAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}
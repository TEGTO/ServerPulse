using MessageBus;
using Microsoft.Extensions.Configuration;
using Moq;
using ServerInteractionApi;
using ServerInteractionApi.Services;
using Shared.Dtos.ServerEvent;

namespace ServerInteractionApiTests.Services
{
    [TestFixture]
    public class MessageSenderTests
    {
        private const string KAFKA_ALIVE_TOPIC = "KafkaAliveTopic";
        private const int KAFKA_PARTITIONS_AMOUNT = 3;

        private Mock<IMessageProducer> mockProducer;
        private Mock<IConfiguration> mockConfiguration;
        private MessageSender messageSender;

        [SetUp]
        public void Setup()
        {
            mockProducer = new Mock<IMessageProducer>();
            mockConfiguration = new Mock<IConfiguration>();
            mockConfiguration.SetupGet(c => c[Configuration.KAFKA_ALIVE_TOPIC]).Returns(KAFKA_ALIVE_TOPIC);
            mockConfiguration.SetupGet(c => c[Configuration.KAFKA_PARTITIONS_AMOUNT]).Returns(KAFKA_PARTITIONS_AMOUNT.ToString());
            messageSender = new MessageSender(mockProducer.Object, mockConfiguration.Object);
        }

        [Test]
        public async Task SendAliveEventAsync_CreatesCorrectTopicAndMessage()
        {
            // Arrange
            var slotId = "slot123";
            var expectedTopic = $"{KAFKA_ALIVE_TOPIC}-{slotId}";
            var aliveEvent = new AliveEvent(slotId, true);
            var cancellationToken = CancellationToken.None;
            // Act
            await messageSender.SendAliveEventAsync(slotId, cancellationToken);
            // Assert
            mockProducer.Verify(x => x.ProduceAsync(
                expectedTopic,
                It.IsAny<string>(),
                KAFKA_PARTITIONS_AMOUNT,
                cancellationToken
            ), Times.Once);
        }
        [Test]
        public async Task SendAliveEventAsync_SerializesAliveEventCorrectly()
        {
            // Arrange
            var slotId = "slot123";
            var aliveEvent = new AliveEvent(slotId, true);
            var cancellationToken = CancellationToken.None;
            // Act
            await messageSender.SendAliveEventAsync(slotId, cancellationToken);
            // Assert
            mockProducer.Verify(x => x.ProduceAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<int>(),
                cancellationToken
            ), Times.Once);
        }
        [Test]
        public async Task SendAliveEventAsync_UsesCorrectPartitionAmount()
        {
            // Arrange
            var slotId = "slot123";
            var cancellationToken = CancellationToken.None;
            // Act
            await messageSender.SendAliveEventAsync(slotId, cancellationToken);
            // Assert
            mockProducer.Verify(x => x.ProduceAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
               KAFKA_PARTITIONS_AMOUNT,
                cancellationToken
            ), Times.Once);
        }
    }
}

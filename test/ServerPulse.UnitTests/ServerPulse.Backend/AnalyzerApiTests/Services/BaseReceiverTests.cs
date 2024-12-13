using AnalyzerApi.Infrastructure;
using AnalyzerApi.Services.Receivers;
using MessageBus.Interfaces;
using Microsoft.Extensions.Configuration;
using Moq;

namespace AnalyzerApi.Services.Tests
{
    [TestFixture]
    internal class BaseReceiverTests
    {
        private const string KafkaTimeoutInMilliseconds = "5000";
        private const string TopicName = "test-topic";
        private const string Key = "test-key";

        private Mock<IMessageConsumer> mockMessageConsumer;
        private Mock<IConfiguration> mockConfiguration;
        private TestReceiver receiver;

        [SetUp]
        public void Setup()
        {
            mockMessageConsumer = new Mock<IMessageConsumer>();
            mockConfiguration = new Mock<IConfiguration>();
            mockConfiguration.SetupGet(c => c[Configuration.KAFKA_TIMEOUT_IN_MILLISECONDS])
                .Returns(KafkaTimeoutInMilliseconds);

            receiver = new TestReceiver(mockMessageConsumer.Object, mockConfiguration.Object);
        }

        [Test]
        public async Task ReceiveLastMessageByKeyAsync_ReturnsLastMessage()
        {
            // Arrange
            var cancellationToken = CancellationToken.None;
            var consumeResponse = new ConsumeResponse("message", DateTime.UtcNow);

            mockMessageConsumer.Setup(m => m.ReadLastTopicMessageAsync(TopicName + Key, It.IsAny<int>(), cancellationToken))
                .ReturnsAsync(consumeResponse);

            // Act
            var result = await receiver.GetLastMessageByKeyAsync(TopicName + Key, cancellationToken);

            // Assert
            Assert.IsNotNull(result);
            Assert.That(result, Is.EqualTo(consumeResponse));

            mockMessageConsumer.Verify(m => m.ReadLastTopicMessageAsync(TopicName + Key, It.IsAny<int>(), cancellationToken), Times.Once);
        }

        [Test]
        public void GetTopic_ReturnsConcatenatedTopic()
        {
            // Act
            var result = receiver.GetTopic(TopicName, Key);

            // Assert
            Assert.That(result, Is.EqualTo(TopicName + Key));
        }

        private class TestReceiver : BaseReceiver
        {
            public TestReceiver(IMessageConsumer messageConsumer, IConfiguration configuration)
                : base(messageConsumer, configuration)
            {
            }

            public new async Task<ConsumeResponse?> GetLastMessageByKeyAsync(string topic, CancellationToken cancellationToken)
            {
                return await base.GetLastMessageByKeyAsync(topic, cancellationToken);
            }

            public new string GetTopic(string baseTopic, string key)
            {
                return BaseReceiver.GetTopic(baseTopic, key);
            }
        }
    }
}
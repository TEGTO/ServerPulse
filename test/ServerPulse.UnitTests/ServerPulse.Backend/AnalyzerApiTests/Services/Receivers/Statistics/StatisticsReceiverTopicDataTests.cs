using AnalyzerApi.Infrastructure.Configuration;
using AnalyzerApi.Infrastructure.Models.Statistics;
using AnalyzerApi.Infrastructure.TopicMapping;
using MessageBus.Interfaces;
using MessageBus.Models;
using Microsoft.Extensions.Options;
using Moq;
using Shared;
using System.Text.Json;

namespace AnalyzerApi.Services.Receivers.Statistics.Tests
{
    [TestFixture]
    internal class StatisticsReceiverTests
    {
        private const string TopicOriginName = "statistics-topic";
        private const int TimeoutInMilliseconds = 5;

        private Mock<IMessageConsumer> mockMessageConsumer;
        private StatisticsTopicMapping<TestStatistics> topicData;
        private StatisticsReceiver<TestStatistics> statisticsReceiver;

        [SetUp]
        public void Setup()
        {
            mockMessageConsumer = new Mock<IMessageConsumer>();
            topicData = new StatisticsTopicMapping<TestStatistics>(TopicOriginName);

            var messageBusSettings = new MessageBusSettings
            {
                ReceiveTimeoutInMilliseconds = TimeoutInMilliseconds,
            };

            var mockOptions = new Mock<IOptions<MessageBusSettings>>();
            mockOptions.Setup(x => x.Value).Returns(messageBusSettings);

            statisticsReceiver = new StatisticsReceiver<TestStatistics>(
                mockMessageConsumer.Object,
                mockOptions.Object,
                topicData
            );
        }

        [Test]
        public async Task GetLastStatisticsByKeyAsync_KeyExists_ReturnsStatistics()
        {
            // Arrange
            var cancellationToken = CancellationToken.None;
            var statistics = new TestStatistics();
            var consumeResponse = new ConsumeResponse(JsonSerializer.Serialize(statistics), DateTime.MinValue);
            consumeResponse.Message.TryToDeserialize(out statistics);

            mockMessageConsumer.Setup(m => m.GetLastTopicMessageAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(consumeResponse);

            // Act
            var result = await statisticsReceiver.GetLastStatisticsAsync("key", cancellationToken);

            // Assert
            Assert.IsNotNull(result);
            Assert.That(result.CollectedDateUTC, Is.EqualTo(statistics!.CollectedDateUTC));
        }

        [Test]
        public async Task ReceiveLastStatisticsByKeyAsync_KeyDoesNotExist_ReturnsNull()
        {
            // Arrange
            var cancellationToken = CancellationToken.None;

            mockMessageConsumer.Setup(m => m.GetLastTopicMessageAsync(It.IsAny<string>(), It.IsAny<int>(), cancellationToken))
                .ReturnsAsync((ConsumeResponse?)null);

            // Act
            var result = await statisticsReceiver.GetLastStatisticsAsync("nonexistent-key", cancellationToken);

            // Assert
            Assert.IsNull(result);
        }
    }

    public class TestStatistics : BaseStatistics { }
}
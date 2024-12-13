using AnalyzerApi.Infrastructure;
using AnalyzerApi.Infrastructure.Configurations;
using AnalyzerApi.Infrastructure.Models.Statistics;
using MessageBus.Interfaces;
using Microsoft.Extensions.Configuration;
using Moq;
using Shared;
using System.Text.Json;

namespace AnalyzerApi.Services.Receivers.Statistics.Tests
{
    [TestFixture]
    internal class StatisticsReceiverTests
    {
        private const string TopicOriginName = "statistics-topic";
        private const string KAFKA_TIMEOUT = "5";

        private Mock<IMessageConsumer> mockMessageConsumer;
        private Mock<IConfiguration> mockConfiguration;
        private StatisticsReceiverTopicConfiguration<TestStatistics> topicData;
        private StatisticsReceiver<TestStatistics> statisticsReceiver;

        [SetUp]
        public void Setup()
        {
            mockMessageConsumer = new Mock<IMessageConsumer>();
            mockConfiguration = new Mock<IConfiguration>();
            topicData = new StatisticsReceiverTopicConfiguration<TestStatistics>(TopicOriginName);
            mockConfiguration.Setup(x => x[Configuration.KAFKA_TIMEOUT_IN_MILLISECONDS]).Returns(KAFKA_TIMEOUT);

            statisticsReceiver = new StatisticsReceiver<TestStatistics>(
                mockMessageConsumer.Object,
                mockConfiguration.Object,
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

            mockMessageConsumer.Setup(m => m.ReadLastTopicMessageAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
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

            mockMessageConsumer.Setup(m => m.ReadLastTopicMessageAsync(It.IsAny<string>(), It.IsAny<int>(), cancellationToken))
                .ReturnsAsync((ConsumeResponse?)null);

            // Act
            var result = await statisticsReceiver.GetLastStatisticsAsync("nonexistent-key", cancellationToken);

            // Assert
            Assert.IsNull(result);
        }
    }

    public class TestStatistics : BaseStatistics { }
}
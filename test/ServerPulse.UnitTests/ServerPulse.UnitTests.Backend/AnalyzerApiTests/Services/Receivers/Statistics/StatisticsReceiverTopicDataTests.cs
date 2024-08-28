using AnalyzerApi;
using AnalyzerApi.Domain.Models;
using AnalyzerApi.Services.Interfaces;
using AnalyzerApi.Services.Receivers.Statistics;
using AutoMapper;
using MessageBus.Interfaces;
using Microsoft.Extensions.Configuration;
using Moq;
using Shared;
using System.Text.Json;

namespace AnalyzerApiTests.Services.Receivers.Statistics
{
    [TestFixture]
    internal class StatisticsReceiverTests
    {
        private const string TopicOriginName = "statistics-topic";
        private const string KAFKA_TIMEOUT = "5";

        private Mock<IMessageConsumer> mockMessageConsumer;
        private Mock<IMapper> mockMapper;
        private Mock<IConfiguration> mockConfiguration;
        private StatisticsReceiverTopicData<BaseStatistics> topicData;
        private StatisticsReceiver<BaseStatistics> statisticsReceiver;

        [SetUp]
        public void Setup()
        {
            mockMessageConsumer = new Mock<IMessageConsumer>();
            mockMapper = new Mock<IMapper>();
            mockConfiguration = new Mock<IConfiguration>();
            topicData = new StatisticsReceiverTopicData<BaseStatistics>(TopicOriginName);
            mockConfiguration.Setup(x => x[Configuration.KAFKA_TIMEOUT_IN_MILLISECONDS]).Returns(KAFKA_TIMEOUT);

            statisticsReceiver = new StatisticsReceiver<BaseStatistics>(
                mockMessageConsumer.Object,
                mockMapper.Object,
                mockConfiguration.Object,
                topicData);
        }

        [Test]
        public async Task ReceiveLastStatisticsByKeyAsync_KeyExists_ReturnsStatistics()
        {
            // Arrange
            var cancellationToken = CancellationToken.None;
            var statistics = new BaseStatistics();
            var consumeResponse = new ConsumeResponse(JsonSerializer.Serialize(statistics), DateTime.UtcNow);
            mockMessageConsumer.Setup(m => m.ReadLastTopicMessageAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(consumeResponse);
            mockMapper.Setup(m => m.Map<BaseStatistics>(It.IsAny<string>()))
                .Returns(statistics);
            consumeResponse.Message.TryToDeserialize(out statistics);
            // Act
            var result = await statisticsReceiver.ReceiveLastStatisticsByKeyAsync("key", cancellationToken);
            // Assert
            Assert.IsNotNull(result);
            Assert.That(result.CollectedDateUTC, Is.EqualTo(statistics.CollectedDateUTC));
        }
        [Test]
        public async Task ReceiveLastStatisticsByKeyAsync_KeyDoesNotExist_ReturnsNull()
        {
            // Arrange
            var cancellationToken = CancellationToken.None;
            mockMessageConsumer.Setup(m => m.ReadLastTopicMessageAsync(It.IsAny<string>(), It.IsAny<int>(), cancellationToken))
                .ReturnsAsync((ConsumeResponse?)null);
            // Act
            var result = await statisticsReceiver.ReceiveLastStatisticsByKeyAsync("nonexistent-key", cancellationToken);
            // Assert
            Assert.IsNull(result);
        }
        [Test]
        public async Task GetWholeStatisticsInTimeSpanAsync_ValidKey_ReturnsStatistics()
        {
            // Arrange
            var cancellationToken = CancellationToken.None;
            var timeSpan = TimeSpan.FromHours(1);
            var expectedStatistics = new BaseStatistics { IsInitial = false, CollectedDateUTC = DateTime.UtcNow };
            var consumeResponse = new ConsumeResponse(JsonSerializer.Serialize(expectedStatistics), DateTime.UtcNow);
            mockMessageConsumer.Setup(m => m.ReadLastTopicMessageAsync(It.IsAny<string>(), It.IsAny<int>(), cancellationToken))
                .ReturnsAsync(consumeResponse);
            consumeResponse.Message.TryToDeserialize(out expectedStatistics);
            // Act
            var result = await statisticsReceiver.GetWholeStatisticsInTimeSpanAsync("key", timeSpan, cancellationToken);
            // Assert
            Assert.That(result, Has.Exactly(1).Items);
            Assert.That(result.First().CollectedDateUTC, Is.EqualTo(expectedStatistics.CollectedDateUTC));
        }
        [Test]
        public async Task GetStatisticsInRangeAsync_ValidRange_ReturnsStatistics()
        {
            // Arrange
            var cancellationToken = CancellationToken.None;
            var timeSpan = TimeSpan.FromHours(1);
            var expectedStatistics = new BaseStatistics { IsInitial = false, CollectedDateUTC = DateTime.UtcNow };
            var options = new InRangeQueryOptions("key", DateTime.UtcNow.AddHours(-2), DateTime.UtcNow);
            var consumeResponse = new ConsumeResponse(JsonSerializer.Serialize(expectedStatistics), DateTime.UtcNow);
            mockMessageConsumer.Setup(m => m.ReadLastTopicMessageAsync(It.IsAny<string>(), It.IsAny<int>(), cancellationToken))
                .ReturnsAsync(consumeResponse);
            consumeResponse.Message.TryToDeserialize(out expectedStatistics);
            // Act
            var result = await statisticsReceiver.GetStatisticsInRangeAsync(options, timeSpan, cancellationToken);
            // Assert
            Assert.That(result, Has.Exactly(1).Items);
            Assert.That(result.First().CollectedDateUTC, Is.EqualTo(expectedStatistics.CollectedDateUTC));
        }
    }
}
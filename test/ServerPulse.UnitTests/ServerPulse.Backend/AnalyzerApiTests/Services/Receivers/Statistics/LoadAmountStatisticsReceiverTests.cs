using AnalyzerApi.Infrastructure.Configuration;
using AnalyzerApi.Infrastructure.Models;
using AnalyzerApi.Infrastructure.Models.Statistics;
using AnalyzerApi.Infrastructure.TopicMapping;
using MessageBus.Interfaces;
using MessageBus.Models;
using Microsoft.Extensions.Options;
using Moq;

namespace AnalyzerApi.Services.Receivers.Statistics.Tests
{
    [TestFixture]
    internal class LoadAmountStatisticsReceiverTests
    {
        private const string TopicOriginName = "statistics-topic";
        private const int StatisticsSaveDataInDays = 30;
        private const int TimeoutInMilliseconds = 5;

        private Mock<IMessageConsumer> mockMessageConsumer;
        private LoadAmountStatisticsReceiver loadAmountStatisticsReceiver;
        private StatisticsTopicMapping<LoadAmountStatistics> topicData;

        [SetUp]
        public void Setup()
        {
            mockMessageConsumer = new Mock<IMessageConsumer>();

            var messageBusSettings = new MessageBusSettings
            {
                TopicDataSaveInDays = StatisticsSaveDataInDays,
                ReceiveTimeoutInMilliseconds = TimeoutInMilliseconds,
            };

            var mockOptions = new Mock<IOptions<MessageBusSettings>>();
            mockOptions.Setup(x => x.Value).Returns(messageBusSettings);

            topicData = new StatisticsTopicMapping<LoadAmountStatistics>(TopicOriginName);

            loadAmountStatisticsReceiver = new LoadAmountStatisticsReceiver(
                mockMessageConsumer.Object,
                mockOptions.Object,
                topicData);
        }

        [Test]
        public async Task GetLastStatisticsByKeyAsync_KeyExists_ReturnsLatestStatistics()
        {
            // Arrange
            var cancellationToken = CancellationToken.None;
            var timeSpan = TimeSpan.FromDays(1);
            var messagesPerDay = new Dictionary<DateTime, int>
            {
                { DateTime.UtcNow.Date.AddDays(-1), 100 }
            };

            mockMessageConsumer.Setup(m => m.GetTopicMessageAmountPerTimespanAsync(It.IsAny<GetMessageInDateRangeOptions>(), timeSpan, cancellationToken))
                .ReturnsAsync(messagesPerDay);

            // Act
            var result = await loadAmountStatisticsReceiver.GetLastStatisticsAsync("key", cancellationToken);

            // Assert
            Assert.IsNotNull(result);
            Assert.That(result.AmountOfEvents, Is.EqualTo(100));
        }

        [Test]
        public async Task GetLastStatisticsByKeyAsync_KeyDoesNotExist_ReturnsNull()
        {
            // Arrange
            var cancellationToken = CancellationToken.None;
            var timeSpan = TimeSpan.FromDays(1);
            var messagesPerDay = new Dictionary<DateTime, int>(); // No messages

            mockMessageConsumer.Setup(m => m.GetTopicMessageAmountPerTimespanAsync(It.IsAny<GetMessageInDateRangeOptions>(), timeSpan, cancellationToken))
                .ReturnsAsync(messagesPerDay);

            // Act
            var result = await loadAmountStatisticsReceiver.GetLastStatisticsAsync("key", cancellationToken);

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        public async Task GetWholeStatisticsInTimeSpanAsync_ValidKey_ReturnsStatisticsList()
        {
            // Arrange
            var cancellationToken = CancellationToken.None;
            var timeSpan = TimeSpan.FromDays(1);
            var messagesPerDay = new Dictionary<DateTime, int>
            {
                { DateTime.UtcNow.Date.AddDays(-1), 50 },
                { DateTime.UtcNow.Date.AddDays(-2), 150 }
            };
            mockMessageConsumer.Setup(m => m.GetTopicMessageAmountPerTimespanAsync(It.IsAny<GetMessageInDateRangeOptions>(), timeSpan, cancellationToken))
                               .ReturnsAsync(messagesPerDay);
            // Act
            var result = await loadAmountStatisticsReceiver.GetWholeStatisticsInTimeSpanAsync("key", timeSpan, cancellationToken);
            // Assert
            Assert.That(result.Count(), Is.EqualTo(2));
            Assert.That(result.First().AmountOfEvents, Is.EqualTo(50));
            Assert.That(result.Last().AmountOfEvents, Is.EqualTo(150));
        }

        [Test]
        public async Task GetStatisticsInRangeAsync_ValidRange_ReturnsStatisticsList()
        {
            // Arrange
            var cancellationToken = CancellationToken.None;
            var timeSpan = TimeSpan.FromDays(1);
            var options = new GetInRangeOptions("key", DateTime.UtcNow.AddDays(-2), DateTime.UtcNow);
            var messagesPerDay = new Dictionary<DateTime, int>
            {
                { DateTime.UtcNow.Date.AddDays(-2), 200 },
                { DateTime.UtcNow.Date.AddDays(-1), 100 }
            };

            mockMessageConsumer.Setup(m => m.GetTopicMessageAmountPerTimespanAsync(It.IsAny<GetMessageInDateRangeOptions>(), timeSpan, cancellationToken))
                .ReturnsAsync(messagesPerDay);

            // Act
            var result = await loadAmountStatisticsReceiver.GetStatisticsInRangeAsync(options, timeSpan, cancellationToken);

            // Assert
            Assert.That(result.Count(), Is.EqualTo(2));
            Assert.That(result.First().AmountOfEvents, Is.EqualTo(100));
            Assert.That(result.Last().AmountOfEvents, Is.EqualTo(200));
        }

        [Test]
        public void ConvertToAmountStatistics_MessageAmountExist_ReturnsFilteredAndOrderedStatistics()
        {
            // Arrange
            var messagesPerDay = new Dictionary<DateTime, int>
            {
                { DateTime.UtcNow.Date.AddDays(-1), 50 },
                { DateTime.UtcNow.Date.AddDays(-2), 150 },
                { DateTime.UtcNow.Date.AddDays(-3), 0 }, // Should be filtered out
            };
            // Act
            var result = loadAmountStatisticsReceiver.GetType()
                .GetMethod("ConvertToAmountStatistics", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
                .Invoke(loadAmountStatisticsReceiver, new object[] { messagesPerDay, TimeSpan.FromDays(1) }) as IEnumerable<LoadAmountStatistics>;
            // Assert
            Assert.IsNotNull(result);
            Assert.That(result.Count(), Is.EqualTo(2)); // Should filter out the entry with 0
            Assert.That(result.First().AmountOfEvents, Is.EqualTo(50));
            Assert.That(result.Last().AmountOfEvents, Is.EqualTo(150));
        }
    }
}
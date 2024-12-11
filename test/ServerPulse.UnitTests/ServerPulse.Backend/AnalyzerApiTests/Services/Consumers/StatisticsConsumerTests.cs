using AnalyzerApi.Domain.Dtos.Wrappers;
using AnalyzerApi.Domain.Models;
using AnalyzerApi.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;
using System.Collections.Concurrent;
using System.Reflection;

namespace AnalyzerApi.Services.Consumers.Tests
{
    [TestFixture]
    internal class StatisticsConsumerTests
    {
        private Mock<IStatisticsCollector<MockStatistics>> mockCollector;
        private Mock<IEventReceiver<MockEventWrapper>> mockReceiver;
        private Mock<IStatisticsSender> mockStatisticsSender;
        private Mock<ILogger<StatisticsConsumer<MockStatistics, MockEventWrapper>>> mockLogger;
        private StatisticsConsumer<MockStatistics, MockEventWrapper> consumer;

        [SetUp]
        public void Setup()
        {
            mockCollector = new Mock<IStatisticsCollector<MockStatistics>>();
            mockReceiver = new Mock<IEventReceiver<MockEventWrapper>>();
            mockStatisticsSender = new Mock<IStatisticsSender>();
            mockLogger = new Mock<ILogger<StatisticsConsumer<MockStatistics, MockEventWrapper>>>();
            consumer = new StatisticsConsumer<MockStatistics, MockEventWrapper>(
                mockCollector.Object,
                mockReceiver.Object,
                mockStatisticsSender.Object,
                mockLogger.Object
            );
        }
        [Test]
        public async Task StartConsumingStatistics_AddsListenerAndSendsInitialStatistics()
        {
            // Arrange
            var key = "testKey";
            var initialStatistics = new MockStatistics { IsInitial = true };
            mockCollector.Setup(m => m.ReceiveLastStatisticsAsync(key, It.IsAny<CancellationToken>()))
                         .ReturnsAsync(initialStatistics);
            mockStatisticsSender.Setup(m => m.SendStatisticsAsync(key, initialStatistics, It.IsAny<CancellationToken>()));
            // Act
            consumer.StartConsumingStatistics(key);
            await Task.Delay(500);
            consumer.StopConsumingStatistics(key);
            // Assert
            mockStatisticsSender.Verify(m => m.SendStatisticsAsync(key, initialStatistics, It.IsAny<CancellationToken>()), Times.Once);
        }
        [Test]
        public async Task StartConsumingStatistics_SendsMultipleStatistics()
        {
            // Arrange
            var key = "testKey";
            var eventWrapper = new MockEventWrapper { Key = key };
            var statistics = new MockStatistics();
            mockReceiver.Setup(m => m.ConsumeEventAsync(key, It.IsAny<CancellationToken>()))
                        .Returns(AsyncEnumerable(new List<MockEventWrapper> { eventWrapper }));
            mockCollector.Setup(m => m.ReceiveLastStatisticsAsync(key, It.IsAny<CancellationToken>()))
                         .ReturnsAsync(statistics);
            // Act
            consumer.StartConsumingStatistics(key);
            await Task.Delay(3000);
            consumer.StopConsumingStatistics(key);
            // Assert
            mockStatisticsSender.Verify(m => m.SendStatisticsAsync(key, statistics, It.IsAny<CancellationToken>()), Times.AtLeast(2));
        }
        [Test]
        public async Task StartConsumingStatistics_DoesNotSendAfterStop()
        {
            // Arrange
            var key = "testKey";
            var eventWrapper = new MockEventWrapper { Key = key };
            var statistics = new MockStatistics();
            mockReceiver.Setup(m => m.ConsumeEventAsync(key, It.IsAny<CancellationToken>()))
                        .Returns(AsyncEnumerable(new List<MockEventWrapper> { eventWrapper }));
            mockCollector.Setup(m => m.ReceiveLastStatisticsAsync(key, It.IsAny<CancellationToken>()))
                         .ReturnsAsync(statistics);
            // Act
            consumer.StartConsumingStatistics(key);
            await Task.Delay(1500);
            consumer.StopConsumingStatistics(key);
            await Task.Delay(3000);
            // Assert
            mockStatisticsSender.Verify(m => m.SendStatisticsAsync(key, statistics, It.IsAny<CancellationToken>()), Times.AtMost(2));
        }
        [Test]
        public async Task SubscribeToEventsAsync_SendsStatisticsOnNewEvent()
        {
            // Arrange
            var key = "testKey";
            var eventWrapper = new MockEventWrapper { Key = key };
            var statistics = new MockStatistics();
            mockReceiver.Setup(m => m.ConsumeEventAsync(key, It.IsAny<CancellationToken>()))
                        .Returns(AsyncEnumerable(new List<MockEventWrapper> { eventWrapper }));
            mockCollector.Setup(m => m.ReceiveLastStatisticsAsync(key, It.IsAny<CancellationToken>()))
                         .ReturnsAsync(statistics);
            // Act
            consumer.StartConsumingStatistics(key);
            await Task.Delay(1500);
            consumer.StopConsumingStatistics(key);
            // Assert
            mockStatisticsSender.Verify(m => m.SendStatisticsAsync(It.IsAny<string>(), It.Is<MockStatistics>(s => s.CollectedDateUTC == statistics.CollectedDateUTC), It.IsAny<CancellationToken>()), Times.AtLeastOnce);
            mockStatisticsSender.Verify(m => m.SendStatisticsAsync(It.IsAny<string>(), It.Is<MockStatistics>(s => s.IsInitial == true), It.IsAny<CancellationToken>()), Times.AtLeastOnce);
        }

        private static async IAsyncEnumerable<T> AsyncEnumerable<T>(IEnumerable<T> items)
        {
            foreach (var item in items)
            {
                yield return item;
                await Task.Yield();
            }
        }
        private ConcurrentDictionary<string, CancellationTokenSource> GetStatisticsListeners()
        {
            FieldInfo info = consumer.GetType().GetField("statisticsListeners", BindingFlags.NonPublic | BindingFlags.Instance);
            object value = info.GetValue(consumer);
            return value as ConcurrentDictionary<string, CancellationTokenSource>;
        }
    }
    public class MockStatistics : BaseStatistics { }
    public class MockEventWrapper : BaseEventWrapper { }
}
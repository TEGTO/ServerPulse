using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using System.Collections.Concurrent;
using System.Reflection;

namespace AnalyzerApi.Services.StatisticsDispatchers.Tests
{
    [TestFixture]
    internal class LifecycleStatisticsDispatcherTests
    {
        private const int STATISTICS_COLLECT_INTERVAL = 1000;

        private Mock<IEventReceiver<PulseEventWrapper>> mockPulseReceiver;
        private Mock<IEventReceiver<ConfigurationEventWrapper>> mockConfReceiver;
        private Mock<IStatisticsCollector<ServerStatistics>> mockStatisticsCollector;
        private Mock<IStatisticsSender> mockStatisticsSender;
        private Mock<IConfiguration> mockConfiguration;
        private Mock<ILogger<ServerStatisticsConsumer>> mockLogger;
        private ServerStatisticsConsumer consumer;

        [SetUp]
        public void Setup()
        {
            mockPulseReceiver = new Mock<IEventReceiver<PulseEventWrapper>>();
            mockConfReceiver = new Mock<IEventReceiver<ConfigurationEventWrapper>>();
            mockStatisticsCollector = new Mock<IStatisticsCollector<ServerStatistics>>();
            mockStatisticsSender = new Mock<IStatisticsSender>();
            mockConfiguration = new Mock<IConfiguration>();
            mockLogger = new Mock<ILogger<ServerStatisticsConsumer>>();

            mockConfiguration.SetupGet(c => c[Configuration.STATISTICS_COLLECT_INTERVAL_IN_MILLISECONDS])
                             .Returns(STATISTICS_COLLECT_INTERVAL.ToString());
            consumer = new ServerStatisticsConsumer(
                mockStatisticsCollector.Object,
                mockPulseReceiver.Object,
                mockConfReceiver.Object,
                mockStatisticsSender.Object,
                mockConfiguration.Object,
                mockLogger.Object
            );
        }

        [Test]
        public async Task StartConsumingStatistics_AddsListenerAndSendOnlyInitialStatistics()
        {
            // Arrange
            var key = "testKey";
            var initialStatistics = new ServerStatistics { IsInitial = true };
            mockConfReceiver.Setup(m => m.ReceiveLastEventByKeyAsync(key, It.IsAny<CancellationToken>()))
                            .ReturnsAsync(new ConfigurationEventWrapper { Key = key, ServerKeepAliveInterval = TimeSpan.Zero });
            mockStatisticsCollector.Setup(m => m.ReceiveLastStatisticsAsync(key, It.IsAny<CancellationToken>()))
                                   .ReturnsAsync(initialStatistics);
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
            var statistics = new ServerStatistics { IsAlive = true };
            mockConfReceiver.Setup(m => m.ReceiveLastEventByKeyAsync(key, It.IsAny<CancellationToken>()))
                            .ReturnsAsync(new ConfigurationEventWrapper { Key = key, ServerKeepAliveInterval = TimeSpan.FromSeconds(2) });
            mockStatisticsCollector.Setup(m => m.ReceiveLastStatisticsAsync(key, It.IsAny<CancellationToken>()))
                                   .ReturnsAsync(statistics);
            mockPulseReceiver.Setup(m => m.ConsumeEventAsync(key, It.IsAny<CancellationToken>()))
                             .Returns(AsyncEnumerable(new List<PulseEventWrapper> { new PulseEventWrapper { Key = key, IsAlive = true } }));
            // Act
            consumer.StartConsumingStatistics(key);
            await Task.Delay(3000);
            consumer.StopConsumingStatistics(key);
            // Assert
            mockStatisticsSender.Verify(m => m.SendStatisticsAsync(key, statistics, It.IsAny<CancellationToken>()), Times.AtLeast(2));
        }
        [Test]
        public async Task PeriodicallySendStatisticsAsync_SendsStatisticsAtInterval()
        {
            // Arrange
            var key = "testKey";
            var statistics = new ServerStatistics { IsAlive = true };
            mockConfReceiver.Setup(m => m.ReceiveLastEventByKeyAsync(key, It.IsAny<CancellationToken>()))
                            .ReturnsAsync(new ConfigurationEventWrapper { Key = key, ServerKeepAliveInterval = TimeSpan.FromHours(1) });
            mockStatisticsCollector.Setup(m => m.ReceiveLastStatisticsAsync(key, It.IsAny<CancellationToken>()))
                                   .ReturnsAsync(statistics);
            mockPulseReceiver.Setup(m => m.ConsumeEventAsync(key, It.IsAny<CancellationToken>()))
                             .Returns(AsyncEnumerable(new List<PulseEventWrapper> { new PulseEventWrapper { Key = key, IsAlive = true } }));
            // Act
            consumer.StartConsumingStatistics(key);
            await Task.Delay(3000);
            consumer.StopConsumingStatistics(key);
            // Assert
            mockStatisticsSender.Verify(m => m.SendStatisticsAsync(It.IsAny<string>(), It.IsAny<ServerStatistics>(), It.IsAny<CancellationToken>()), Times.AtLeast(2));
        }
        [Test]
        public async Task StopConsumingStatistics_LogsWhenCanceled()
        {
            // Arrange
            var key = "testKey";
            mockConfReceiver.Setup(m => m.ReceiveLastEventByKeyAsync(key, It.IsAny<CancellationToken>()))
                            .ReturnsAsync(new ConfigurationEventWrapper { Key = key, ServerKeepAliveInterval = TimeSpan.FromHours(1) });
            mockStatisticsCollector.Setup(m => m.ReceiveLastStatisticsAsync(key, It.IsAny<CancellationToken>()))
                                   .ReturnsAsync(new ServerStatistics { IsAlive = true });
            mockPulseReceiver.Setup(m => m.ConsumeEventAsync(key, It.IsAny<CancellationToken>()))
                             .Returns(AsyncEnumerable(new List<PulseEventWrapper>()));
            // Act
            consumer.StartConsumingStatistics(key);
            await Task.Delay(500);
            consumer.StopConsumingStatistics(key);
            // Assert
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
        [Test]
        public async Task StartConsumingStatistics_ChangesConfiguration()
        {
            // Arrange
            var key = "testKey";
            mockConfReceiver.Setup(m => m.ReceiveLastEventByKeyAsync(key, It.IsAny<CancellationToken>()))
                            .ReturnsAsync(new ConfigurationEventWrapper { Key = key, ServerKeepAliveInterval = TimeSpan.FromHours(1) });
            mockPulseReceiver.Setup(m => m.ConsumeEventAsync(key, It.IsAny<CancellationToken>()))
                             .Returns(AsyncEnumerable(new List<PulseEventWrapper>()));
            mockStatisticsCollector.Setup(m => m.ReceiveLastStatisticsAsync(key, It.IsAny<CancellationToken>()))
                                .ReturnsAsync(new ServerStatistics { IsAlive = true });
            // Act
            consumer.StartConsumingStatistics(key);
            await Task.Delay(3000);
            // Assert
            Assert.That(GetDelay(key), Is.EqualTo((int)TimeSpan.FromHours(1).TotalMilliseconds));
            consumer.StopConsumingStatistics(key);
        }
        [Test]
        public async Task StartConsumingStatistics_NotSendsStatisticsAfterStop()
        {
            // Arrange
            var key = "testKey";
            mockConfReceiver.Setup(m => m.ReceiveLastEventByKeyAsync(key, It.IsAny<CancellationToken>()))
                            .ReturnsAsync(new ConfigurationEventWrapper { Key = key, ServerKeepAliveInterval = TimeSpan.FromSeconds(2) });
            mockStatisticsCollector.Setup(m => m.ReceiveLastStatisticsAsync(key, It.IsAny<CancellationToken>()))
                                   .ReturnsAsync(new ServerStatistics { IsAlive = true });
            // Act
            consumer.StartConsumingStatistics(key);
            await Task.Delay(1500);
            consumer.StopConsumingStatistics(key);
            await Task.Delay(3000);
            // Assert
            mockStatisticsSender.Verify(m => m.SendStatisticsAsync(It.IsAny<string>(), It.IsAny<ServerStatistics>(), It.IsAny<CancellationToken>()), Times.AtMost(2));
        }
        [Test]
        public async Task PeriodicallySendStatisticsAsync_StopsSendingWhenIsAliveBecomesFalse()
        {
            // Arrange
            var key = "testKey";
            var statistics = new ServerStatistics { IsAlive = true };
            mockConfReceiver.Setup(m => m.ReceiveLastEventByKeyAsync(key, It.IsAny<CancellationToken>()))
                            .ReturnsAsync(new ConfigurationEventWrapper { Key = key, ServerKeepAliveInterval = TimeSpan.FromSeconds(2) });
            mockPulseReceiver.Setup(m => m.ConsumeEventAsync(key, It.IsAny<CancellationToken>()))
                             .Returns(AsyncEnumerable(new List<PulseEventWrapper> { new PulseEventWrapper { Key = key, IsAlive = false } }));
            mockStatisticsCollector.Setup(m => m.ReceiveLastStatisticsAsync(key, It.IsAny<CancellationToken>()))
                                   .ReturnsAsync(statistics);
            // Act
            consumer.StartConsumingStatistics(key);
            await Task.Delay(1500);
            consumer.StopConsumingStatistics(key);
            // Assert
            mockStatisticsSender.Verify(m => m.SendStatisticsAsync(It.IsAny<string>(), It.IsAny<ServerStatistics>(), It.IsAny<CancellationToken>()), Times.AtMost(2));
        }
        [Test]
        public async Task SubscribeToPulseEventsAsync_ChangesIsAliveBasedOnPulseEvents()
        {
            // Arrange
            var key = "testKey";
            var statistics = new ServerStatistics { IsAlive = true };
            mockStatisticsCollector.Setup(m => m.ReceiveLastStatisticsAsync(key, It.IsAny<CancellationToken>()))
                               .ReturnsAsync(statistics);
            mockPulseReceiver.Setup(m => m.ConsumeEventAsync(key, It.IsAny<CancellationToken>()))
                             .Returns(AsyncEnumerable(new List<PulseEventWrapper>
                             {
                                 new PulseEventWrapper { Key = key, IsAlive = true },
                                 new PulseEventWrapper { Key = key, IsAlive = false }
                             }));
            mockConfReceiver.Setup(m => m.ReceiveLastEventByKeyAsync(key, It.IsAny<CancellationToken>()))
                           .ReturnsAsync(new ConfigurationEventWrapper { Key = key, ServerKeepAliveInterval = TimeSpan.FromSeconds(1) });
            // Act
            consumer.StartConsumingStatistics(key);
            await Task.Delay(1000);
            consumer.StopConsumingStatistics(key);
            //Assert
            Assert.IsFalse(GetIsAliveField(key));
        }
        [Test]
        public async Task PeriodicallySendStatisticsAsync_NotSedningEventOnConsume()
        {
            // Arrange
            var key = "testKey";
            var statistics = new ServerStatistics { IsAlive = false };
            mockPulseReceiver.Setup(m => m.ConsumeEventAsync(key, It.IsAny<CancellationToken>()))
                             .Returns(AsyncEnumerable(new List<PulseEventWrapper> { new PulseEventWrapper { Key = key, IsAlive = false } }));
            mockStatisticsCollector.Setup(m => m.ReceiveLastStatisticsAsync(key, It.IsAny<CancellationToken>()))
                                   .ReturnsAsync(statistics);
            mockConfReceiver.Setup(m => m.ReceiveLastEventByKeyAsync(key, It.IsAny<CancellationToken>()))
                            .ReturnsAsync(new ConfigurationEventWrapper { Key = key, ServerKeepAliveInterval = TimeSpan.FromSeconds(2) });
            // Act
            consumer.StartConsumingStatistics(key);
            await Task.Delay(1500);
            consumer.StopConsumingStatistics(key);
            // Assert
            mockStatisticsSender.Verify(m => m.SendStatisticsAsync(key, statistics, It.IsAny<CancellationToken>()), Times.Exactly(2));
        }
        [Test]
        public async Task PeriodicallySendStatisticsAsync_ResumesSendingWhenIsAliveBecomesTrue()
        {
            // Arrange
            var key = "testKey";
            var statistics = new ServerStatistics { IsAlive = true };
            mockPulseReceiver.Setup(m => m.ConsumeEventAsync(key, It.IsAny<CancellationToken>()))
                             .Returns(AsyncEnumerable(new List<PulseEventWrapper>
                             {
                             new PulseEventWrapper { Key = key, IsAlive = false },
                             new PulseEventWrapper { Key = key, IsAlive = true }
                             }));
            mockStatisticsCollector.Setup(m => m.ReceiveLastStatisticsAsync(key, It.IsAny<CancellationToken>()))
                                   .ReturnsAsync(statistics);
            mockConfReceiver.Setup(m => m.ReceiveLastEventByKeyAsync(key, It.IsAny<CancellationToken>()))
                            .ReturnsAsync(new ConfigurationEventWrapper { Key = key, ServerKeepAliveInterval = TimeSpan.FromSeconds(2) });
            // Act
            consumer.StartConsumingStatistics(key);
            await Task.Delay(3000);
            consumer.StopConsumingStatistics(key);
            // Assert
            mockStatisticsSender.Verify(m => m.SendStatisticsAsync(key, statistics, It.IsAny<CancellationToken>()), Times.AtLeast(1), "Statistics should resume sending when isAlive becomes true again.");
        }
        [Test]
        public async Task StartConsumingStatistics_HandlesEdgeCaseWhenConfigurationEventIsNull()
        {
            // Arrange
            var key = "testKey";
            var statistics = new ServerStatistics { IsAlive = true };
            mockConfReceiver.Setup(m => m.ReceiveLastEventByKeyAsync(key, It.IsAny<CancellationToken>()))
                            .ReturnsAsync((ConfigurationEventWrapper?)null);  // Configuration event is null
            mockPulseReceiver.Setup(m => m.ConsumeEventAsync(key, It.IsAny<CancellationToken>()))
                             .Returns(AsyncEnumerable(new List<PulseEventWrapper> { new PulseEventWrapper { Key = key, IsAlive = true } }));
            mockStatisticsCollector.Setup(m => m.ReceiveLastStatisticsAsync(key, It.IsAny<CancellationToken>()))
                                   .ReturnsAsync(statistics);
            // Act
            consumer.StartConsumingStatistics(key);
            await Task.Delay(1500);
            consumer.StopConsumingStatistics(key);
            // Assert
            mockStatisticsSender.Verify(m => m.SendStatisticsAsync(key, statistics, It.IsAny<CancellationToken>()), Times.AtLeastOnce);
        }

        private bool GetIsAliveField(string key)
        {
            var fieldInfo = consumer.GetType().GetField("listenersIsAlive", BindingFlags.NonPublic | BindingFlags.Instance);
            var dictionary = fieldInfo?.GetValue(consumer) as ConcurrentDictionary<string, bool>;
            if (dictionary.TryGetValue(key, out bool value))
            {
                return value;
            }
            return false;
        }
        private int GetDelay(string key)
        {
            var fieldInfo = consumer.GetType().GetField("listenersDelay", BindingFlags.NonPublic | BindingFlags.Instance);
            var dictionary = fieldInfo?.GetValue(consumer) as ConcurrentDictionary<string, int>;
            if (dictionary.TryGetValue(key, out int value))
            {
                return value;
            }
            return 0;
        }
        private static async IAsyncEnumerable<T> AsyncEnumerable<T>(IEnumerable<T> items)
        {
            foreach (var item in items)
            {
                yield return item;
                await Task.Yield();
            }
        }
    }
}
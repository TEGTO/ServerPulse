using AnalyzerApi;
using AnalyzerApi.Domain.Dtos.Wrappers;
using AnalyzerApi.Domain.Models;
using AnalyzerApi.Services.Consumers;
using AnalyzerApi.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;
using System.Collections.Concurrent;
using System.Reflection;

namespace AnalyzerApiTests.Services.Collector
{
    [TestFixture]
    internal class ServerStatisticsCollectorTests : BaseStatisticsCollectorTests
    {
        private const int STATISTICS_COLLECT_INTERVAL = 1000;

        private Mock<IEventReceiver<PulseEventWrapper>> mockPulseReceiver;
        private Mock<IEventReceiver<ConfigurationEventWrapper>> mockConfReceiver;
        private Mock<IStatisticsReceiver<ServerStatistics>> mockStatisticsReceiver;
        private Mock<ILogger<ServerStatisticsConsumer>> mockLogger;
        private ServerStatisticsConsumer collector;

        [SetUp]
        public override void Setup()
        {
            base.Setup();

            mockPulseReceiver = new Mock<IEventReceiver<PulseEventWrapper>>();
            mockConfReceiver = new Mock<IEventReceiver<ConfigurationEventWrapper>>();
            mockStatisticsReceiver = new Mock<IStatisticsReceiver<ServerStatistics>>();
            mockLogger = new Mock<ILogger<ServerStatisticsConsumer>>();

            mockConfiguration.SetupGet(c => c[Configuration.STATISTICS_COLLECT_INTERVAL_IN_MILLISECONDS])
                             .Returns(STATISTICS_COLLECT_INTERVAL.ToString());

            collector = new ServerStatisticsConsumer(
                mockPulseReceiver.Object,
                mockConfReceiver.Object,
                mockStatisticsReceiver.Object,
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
            mockConfReceiver.Setup(m => m.ReceiveLastEventByKeyAsync(key, It.IsAny<CancellationToken>()))
                            .ReturnsAsync(new ConfigurationEventWrapper { Key = key, ServerKeepAliveInterval = TimeSpan.Zero });
            mockPulseReceiver.Setup(m => m.ReceiveLastEventByKeyAsync(key, It.IsAny<CancellationToken>()))
                             .ReturnsAsync(new PulseEventWrapper { Key = key, IsAlive = true });
            mockStatisticsReceiver.Setup(m => m.ReceiveLastStatisticsByKeyAsync(key, It.IsAny<CancellationToken>()))
                                  .ReturnsAsync(new ServerStatistics { IsAlive = true });
            // Act
            collector.StartConsumingStatistics(key);
            await Task.Delay(500);
            collector.StopConsumingStatistics(key);
            // Assert
            mockStatisticsSender.Verify(m => m.SendStatisticsAsync(It.IsAny<string>(), It.IsAny<ServerStatistics>(), It.IsAny<CancellationToken>()), Times.Once);
        }
        [Test]
        public async Task StartConsumingStatistics_SendsMoreThanOneStatistics()
        {
            // Arrange
            var key = "testKey";
            mockConfReceiver.Setup(m => m.ReceiveLastEventByKeyAsync(key, It.IsAny<CancellationToken>()))
                            .ReturnsAsync(new ConfigurationEventWrapper { Key = key, ServerKeepAliveInterval = TimeSpan.FromSeconds(2) });
            mockPulseReceiver.Setup(m => m.ReceiveLastEventByKeyAsync(key, It.IsAny<CancellationToken>()))
                             .ReturnsAsync(new PulseEventWrapper { Key = key, IsAlive = true, CreationDateUTC = DateTime.MaxValue });
            mockStatisticsReceiver.Setup(m => m.ReceiveLastStatisticsByKeyAsync(key, It.IsAny<CancellationToken>()))
                                  .ReturnsAsync(new ServerStatistics { IsAlive = true });
            // Act
            collector.StartConsumingStatistics(key);
            await Task.Delay(3000);
            collector.StopConsumingStatistics(key);
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
            mockPulseReceiver.Setup(m => m.ReceiveLastEventByKeyAsync(key, It.IsAny<CancellationToken>()))
                             .ReturnsAsync(new PulseEventWrapper { Key = key, IsAlive = true });
            mockStatisticsReceiver.Setup(m => m.ReceiveLastStatisticsByKeyAsync(key, It.IsAny<CancellationToken>()))
                                  .ReturnsAsync(new ServerStatistics { IsAlive = true });
            mockPulseReceiver.Setup(m => m.ConsumeEventAsync(key, It.IsAny<CancellationToken>()))
                             .Returns(AsyncEnumerable(new List<PulseEventWrapper>()));
            mockConfReceiver.Setup(m => m.ConsumeEventAsync(key, It.IsAny<CancellationToken>()))
                            .Returns(AsyncEnumerable(new List<ConfigurationEventWrapper>()));
            // Act
            collector.StartConsumingStatistics(key);
            await Task.Delay(500);
            collector.StopConsumingStatistics(key);
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
        public async Task PeriodicallySendStatisticsAsync_SendsStatisticsAtInterval()
        {
            // Arrange
            var key = "testKey";
            mockConfReceiver.Setup(m => m.ReceiveLastEventByKeyAsync(key, It.IsAny<CancellationToken>()))
                            .ReturnsAsync(new ConfigurationEventWrapper { Key = key, ServerKeepAliveInterval = TimeSpan.FromHours(1) });
            mockPulseReceiver.Setup(m => m.ReceiveLastEventByKeyAsync(key, It.IsAny<CancellationToken>()))
                             .ReturnsAsync(new PulseEventWrapper { Key = key, IsAlive = true });
            mockStatisticsReceiver.Setup(m => m.ReceiveLastStatisticsByKeyAsync(key, It.IsAny<CancellationToken>()))
                                  .ReturnsAsync(new ServerStatistics { IsAlive = true });
            mockPulseReceiver.Setup(m => m.ConsumeEventAsync(key, It.IsAny<CancellationToken>()))
                             .Returns(AsyncEnumerable(new List<PulseEventWrapper> { new PulseEventWrapper { Key = key, IsAlive = true, CreationDateUTC = DateTime.MaxValue } }));
            // Act
            collector.StartConsumingStatistics(key);
            await Task.Delay(3000);
            collector.StopConsumingStatistics(key);
            // Assert
            mockStatisticsSender.Verify(m => m.SendStatisticsAsync(It.IsAny<string>(), It.IsAny<ServerStatistics>(), It.IsAny<CancellationToken>()), Times.AtLeast(2));
        }
        [Test]
        public async Task StartConsumingStatistics_ChangesConfiguration()
        {
            // Arrange
            var key = "testKey";
            mockConfReceiver.Setup(m => m.ReceiveLastEventByKeyAsync(key, It.IsAny<CancellationToken>()))
                            .ReturnsAsync(new ConfigurationEventWrapper { Key = key, ServerKeepAliveInterval = TimeSpan.FromHours(1) });
            mockPulseReceiver.Setup(m => m.ReceiveLastEventByKeyAsync(key, It.IsAny<CancellationToken>()))
                             .ReturnsAsync(new PulseEventWrapper { Key = key, IsAlive = true });
            mockPulseReceiver.Setup(m => m.ConsumeEventAsync(key, It.IsAny<CancellationToken>()))
                             .Returns(AsyncEnumerable(new List<PulseEventWrapper>()));
            mockConfReceiver.Setup(m => m.ConsumeEventAsync(key, It.IsAny<CancellationToken>()))
                            .Returns(AsyncEnumerable(new List<ConfigurationEventWrapper> { new ConfigurationEventWrapper { Key = key, ServerKeepAliveInterval = TimeSpan.FromHours(2) } }));
            // Act
            collector.StartConsumingStatistics(key);
            await Task.Delay(3000);
            // Reflect into the private field "configurations" to verify the new interval
            var fieldInfo = collector.GetType().GetField("configurations", BindingFlags.NonPublic | BindingFlags.Instance);
            var configurations = fieldInfo?.GetValue(collector) as ConcurrentDictionary<string, ConfigurationEventWrapper>;
            configurations.TryGetValue(key, out ConfigurationEventWrapper conf);
            // Assert
            Assert.That(conf.ServerKeepAliveInterval, Is.EqualTo(TimeSpan.FromHours(2)));
            collector.StopConsumingStatistics(key);
        }
        [Test]
        public async Task StartConsumingStatistics_NotSendsStatisticsAfterStop()
        {
            // Arrange
            var key = "testKey";
            mockConfReceiver.Setup(m => m.ReceiveLastEventByKeyAsync(key, It.IsAny<CancellationToken>()))
                            .ReturnsAsync(new ConfigurationEventWrapper { Key = key, ServerKeepAliveInterval = TimeSpan.FromSeconds(2) });
            mockPulseReceiver.Setup(m => m.ReceiveLastEventByKeyAsync(key, It.IsAny<CancellationToken>()))
                             .ReturnsAsync(new PulseEventWrapper { Key = key, IsAlive = true });
            mockStatisticsReceiver.Setup(m => m.ReceiveLastStatisticsByKeyAsync(key, It.IsAny<CancellationToken>()))
                                  .ReturnsAsync(new ServerStatistics { IsAlive = true });
            // Act
            collector.StartConsumingStatistics(key);
            await Task.Delay(1500);
            collector.StopConsumingStatistics(key);
            await Task.Delay(3000);
            // Assert
            mockStatisticsSender.Verify(m => m.SendStatisticsAsync(It.IsAny<string>(), It.IsAny<ServerStatistics>(), It.IsAny<CancellationToken>()), Times.AtMost(2));
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
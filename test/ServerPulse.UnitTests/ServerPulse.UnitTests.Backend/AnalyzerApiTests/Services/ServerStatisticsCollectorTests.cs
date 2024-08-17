using AnalyzerApi;
using AnalyzerApi.Domain.Dtos.Wrappers;
using AnalyzerApi.Domain.Models;
using AnalyzerApi.Services;
using AnalyzerApi.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using System.Collections.Concurrent;
using System.Reflection;

namespace AnalyzerApiTests.Services
{
    [TestFixture]
    internal class ServerStatisticsCollectorTests
    {
        private Mock<IServerStatusReceiver> mockMessageReceiver;
        private Mock<IStatisticsSender> mockStatisticsSender;
        private Mock<IConfiguration> mockConfiguration;
        private Mock<ILogger<ServerStatisticsCollector>> mockLogger;
        private ServerStatisticsCollector serverStatisticsCollector;
        private const int StatisticsCollectInterval = 1000;

        [SetUp]
        public void Setup()
        {
            mockMessageReceiver = new Mock<IServerStatusReceiver>();
            mockStatisticsSender = new Mock<IStatisticsSender>();
            mockConfiguration = new Mock<IConfiguration>();
            mockLogger = new Mock<ILogger<ServerStatisticsCollector>>();

            mockConfiguration.SetupGet(c => c[Configuration.STATISTICS_COLLECT_INTERVAL_IN_MILLISECONDS])
                             .Returns(StatisticsCollectInterval.ToString());

            serverStatisticsCollector = new ServerStatisticsCollector(
                mockMessageReceiver.Object,
                mockStatisticsSender.Object,
                mockConfiguration.Object,
                mockLogger.Object
            );
        }

        [Test]
        public async Task StartConsumingStatistics_ShouldAddListenerAndSendOnlyInitialStatistics()
        {
            // Arrange
            var key = "testKey";
            mockMessageReceiver.Setup(m => m.ReceiveLastConfigurationEventByKeyAsync(key, It.IsAny<CancellationToken>()))
                               .ReturnsAsync(new ConfigurationEventWrapper { Key = key, ServerKeepAliveInterval = TimeSpan.Zero });
            mockMessageReceiver.Setup(m => m.ReceiveLastPulseEventByKeyAsync(key, It.IsAny<CancellationToken>()))
                               .ReturnsAsync(new PulseEventWrapper { Key = key, IsAlive = true });
            mockMessageReceiver.Setup(m => m.ReceiveLastServerStatisticsByKeyAsync(key, It.IsAny<CancellationToken>()))
                               .ReturnsAsync(new ServerStatistics { IsAlive = true });
            // Act
            serverStatisticsCollector.StartConsumingStatistics(key);
            await Task.Delay(500);
            serverStatisticsCollector.StopConsumingStatistics(key);
            // Assert
            mockStatisticsSender.Verify(m => m.SendServerStatisticsAsync(It.IsAny<string>(), It.IsAny<ServerStatistics>(), It.IsAny<CancellationToken>()), Times.Once);
        }
        [Test]
        public async Task StartConsumingStatistics_ShouldSendMoreThanOneStatistics()
        {
            // Arrange
            var key = "testKey";
            mockMessageReceiver.Setup(m => m.ReceiveLastConfigurationEventByKeyAsync(key, It.IsAny<CancellationToken>()))
                               .ReturnsAsync(new ConfigurationEventWrapper { Key = key, ServerKeepAliveInterval = TimeSpan.FromSeconds(2) });
            mockMessageReceiver.Setup(m => m.ReceiveLastPulseEventByKeyAsync(key, It.IsAny<CancellationToken>()))
                               .ReturnsAsync(new PulseEventWrapper { Key = key, IsAlive = true, CreationDateUTC = DateTime.MaxValue });
            mockMessageReceiver.Setup(m => m.ReceiveLastServerStatisticsByKeyAsync(key, It.IsAny<CancellationToken>()))
                               .ReturnsAsync(new ServerStatistics { IsAlive = true });
            // Act
            serverStatisticsCollector.StartConsumingStatistics(key);
            await Task.Delay(3000);
            serverStatisticsCollector.StopConsumingStatistics(key);
            // Assert
            mockStatisticsSender.Verify(m => m.SendServerStatisticsAsync(It.IsAny<string>(), It.IsAny<ServerStatistics>(), It.IsAny<CancellationToken>()), Times.AtLeast(2));
        }
        [Test]
        public async Task StopConsumingStatistics_ShouldLogWhenCanceled()
        {
            // Arrange
            var key = "testKey";
            mockMessageReceiver.Setup(m => m.ReceiveLastConfigurationEventByKeyAsync(key, It.IsAny<CancellationToken>()))
                               .ReturnsAsync(new ConfigurationEventWrapper { Key = key, ServerKeepAliveInterval = TimeSpan.FromHours(1) });
            mockMessageReceiver.Setup(m => m.ReceiveLastPulseEventByKeyAsync(key, It.IsAny<CancellationToken>()))
                               .ReturnsAsync(new PulseEventWrapper { Key = key, IsAlive = true });
            mockMessageReceiver.Setup(m => m.ReceiveLastServerStatisticsByKeyAsync(key, It.IsAny<CancellationToken>()))
                               .ReturnsAsync(new ServerStatistics { IsAlive = true });
            mockMessageReceiver.Setup(m => m.ConsumePulseEventAsync(key, It.IsAny<CancellationToken>()))
                      .Returns(AsyncEnumerable(new List<PulseEventWrapper>()));
            mockMessageReceiver.Setup(m => m.ConsumeConfigurationEventAsync(key, It.IsAny<CancellationToken>()))
                  .Returns(AsyncEnumerable(new List<ConfigurationEventWrapper>()));
            // Act
            serverStatisticsCollector.StartConsumingStatistics(key);
            await Task.Delay(500);
            serverStatisticsCollector.StopConsumingStatistics(key);
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
        public async Task PeriodicallySendStatisticsAsync_ShouldSendStatisticsAtInterval()
        {
            // Arrange
            var key = "testKey";
            var cancellationTokenSource = new CancellationTokenSource();
            mockMessageReceiver.Setup(m => m.ReceiveLastConfigurationEventByKeyAsync(key, It.IsAny<CancellationToken>()))
                               .ReturnsAsync(new ConfigurationEventWrapper { Key = key, ServerKeepAliveInterval = TimeSpan.FromHours(1) });
            mockMessageReceiver.Setup(m => m.ReceiveLastPulseEventByKeyAsync(key, It.IsAny<CancellationToken>()))
                               .ReturnsAsync(new PulseEventWrapper { Key = key, IsAlive = true });
            mockMessageReceiver.Setup(m => m.ReceiveLastServerStatisticsByKeyAsync(key, It.IsAny<CancellationToken>()))
                               .ReturnsAsync(new ServerStatistics { IsAlive = true });
            mockMessageReceiver.Setup(m => m.ConsumePulseEventAsync(key, It.IsAny<CancellationToken>()))
                    .Returns(AsyncEnumerable(new List<PulseEventWrapper> { new PulseEventWrapper { Key = key, IsAlive = true, CreationDateUTC = DateTime.MaxValue } }));
            // Act
            serverStatisticsCollector.StartConsumingStatistics(key);
            await Task.Delay(3000);
            serverStatisticsCollector.StopConsumingStatistics(key);
            // Assert
            mockStatisticsSender.Verify(m => m.SendServerStatisticsAsync(It.IsAny<string>(), It.IsAny<ServerStatistics>(), It.IsAny<CancellationToken>()), Times.AtLeast(2));
        }
        [Test]
        public async Task StartConsumingStatistics_ShouldChangeConfiguration()
        {
            // Arrange
            var key = "testKey";
            var cancellationTokenSource = new CancellationTokenSource();
            mockMessageReceiver.Setup(m => m.ReceiveLastConfigurationEventByKeyAsync(key, It.IsAny<CancellationToken>()))
                               .ReturnsAsync(new ConfigurationEventWrapper { Key = key, ServerKeepAliveInterval = TimeSpan.FromHours(1) });
            mockMessageReceiver.Setup(m => m.ReceiveLastPulseEventByKeyAsync(key, It.IsAny<CancellationToken>()))
                               .ReturnsAsync(new PulseEventWrapper { Key = key, IsAlive = true });
            mockMessageReceiver.Setup(m => m.ConsumePulseEventAsync(key, It.IsAny<CancellationToken>()))
                        .Returns(AsyncEnumerable(new List<PulseEventWrapper>()));
            mockMessageReceiver.Setup(m => m.ConsumeConfigurationEventAsync(key, It.IsAny<CancellationToken>()))
                  .Returns(AsyncEnumerable(new List<ConfigurationEventWrapper> { new ConfigurationEventWrapper { Key = key, ServerKeepAliveInterval = TimeSpan.FromHours(2) } }));
            // Act + Assert
            serverStatisticsCollector.StartConsumingStatistics(key);
            await Task.Delay(3000);
            var fieldInfo = serverStatisticsCollector.GetType().GetField("configurations", BindingFlags.NonPublic | BindingFlags.Instance);
            var configurations = fieldInfo?.GetValue(serverStatisticsCollector) as ConcurrentDictionary<string, ConfigurationEventWrapper>;
            configurations.TryGetValue(key, out ConfigurationEventWrapper conf);
            Assert.That(conf.ServerKeepAliveInterval, Is.EqualTo(TimeSpan.FromHours(2)));
            serverStatisticsCollector.StopConsumingStatistics(key);
        }
        [Test]
        public async Task StartConsumingStatistics_ShouldNotSendStatisticsAfterStop()
        {
            // Arrange
            var key = "testKey";
            mockMessageReceiver.Setup(m => m.ReceiveLastConfigurationEventByKeyAsync(key, It.IsAny<CancellationToken>()))
                               .ReturnsAsync(new ConfigurationEventWrapper { Key = key, ServerKeepAliveInterval = TimeSpan.FromSeconds(2) });
            mockMessageReceiver.Setup(m => m.ReceiveLastPulseEventByKeyAsync(key, It.IsAny<CancellationToken>()))
                               .ReturnsAsync(new PulseEventWrapper { Key = key, IsAlive = true });
            mockMessageReceiver.Setup(m => m.ReceiveLastServerStatisticsByKeyAsync(key, It.IsAny<CancellationToken>()))
                               .ReturnsAsync(new ServerStatistics { IsAlive = true });
            // Act
            serverStatisticsCollector.StartConsumingStatistics(key);
            await Task.Delay(1500);
            serverStatisticsCollector.StopConsumingStatistics(key);
            await Task.Delay(3000);

            // Assert
            mockStatisticsSender.Verify(m => m.SendServerStatisticsAsync(It.IsAny<string>(), It.IsAny<ServerStatistics>(), It.IsAny<CancellationToken>()), Times.AtMost(2));
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
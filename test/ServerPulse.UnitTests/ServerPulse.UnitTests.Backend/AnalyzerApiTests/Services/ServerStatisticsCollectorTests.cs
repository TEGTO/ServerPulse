using AnalyzerApi;
using AnalyzerApi.Domain.Models;
using AnalyzerApi.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using ServerPulse.EventCommunication.Events;

namespace AnalyzerApiTests.Services
{
    [TestFixture]
    internal class ServerStatisticsCollectorTests
    {
        private Mock<IMessageReceiver> mockMessageReceiver;
        private Mock<IStatisticsSender> mockStatisticsSender;
        private Mock<IConfiguration> mockConfiguration;
        private Mock<ILogger<ServerStatisticsCollector>> mockLogger;
        private ServerStatisticsCollector serverStatisticsCollector;
        private const int StatisticsCollectInterval = 1000;

        [SetUp]
        public void Setup()
        {
            mockMessageReceiver = new Mock<IMessageReceiver>();
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
                .ReturnsAsync(new ConfigurationEvent(key, TimeSpan.Zero));
            mockMessageReceiver.Setup(m => m.ReceiveLastPulseEventByKeyAsync(key, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new PulseEvent(key, false));
            mockMessageReceiver
            .Setup(m => m.ConsumePulseEventAsync(key, It.IsAny<CancellationToken>()))
            .Returns(AsyncEnumerable(new List<PulseEvent>()));
            mockMessageReceiver
             .Setup(m => m.ConsumeConfigurationEventAsync(key, It.IsAny<CancellationToken>()))
             .Returns(AsyncEnumerable(new List<ConfigurationEvent>()));
            // Act
            serverStatisticsCollector.StartConsumingStatistics(key);
            await Task.Delay(TimeSpan.FromSeconds(1));
            serverStatisticsCollector.StopConsumingStatistics(key);
            // Assert
            mockStatisticsSender.Verify(m => m.SendStatisticsAsync(It.IsAny<string>(), It.IsAny<ServerStatistics>()), Times.Once);
        }
        [Test]
        public async Task StartConsumingStatistics_ShouldSendMoreThanOneStatistics()
        {
            // Arrange
            var key = "testKey";
            mockMessageReceiver.Setup(m => m.ReceiveLastConfigurationEventByKeyAsync(key, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ConfigurationEvent(key, TimeSpan.FromHours(1)));
            mockMessageReceiver.Setup(m => m.ReceiveLastPulseEventByKeyAsync(key, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new PulseEvent(key, true));
            mockMessageReceiver
             .Setup(m => m.ConsumePulseEventAsync(key, It.IsAny<CancellationToken>()))
             .Returns(AsyncEnumerable(new List<PulseEvent>()));
            mockMessageReceiver
             .Setup(m => m.ConsumeConfigurationEventAsync(key, It.IsAny<CancellationToken>()))
             .Returns(AsyncEnumerable(new List<ConfigurationEvent>()));
            // Act
            serverStatisticsCollector.StartConsumingStatistics(key);
            await Task.Delay(TimeSpan.FromSeconds(1));
            serverStatisticsCollector.StopConsumingStatistics(key);
            // Assert
            mockStatisticsSender.Verify(m => m.SendStatisticsAsync(It.IsAny<string>(), It.IsAny<ServerStatistics>()), Times.AtLeast(2));
        }
        [Test]
        public async Task StartConsumingStatistics_ShouldSendMoreThanTwoStatistics()
        {
            // Arrange
            var key = "testKey";
            mockMessageReceiver.Setup(m => m.ReceiveLastConfigurationEventByKeyAsync(key, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ConfigurationEvent(key, TimeSpan.FromHours(1)));
            mockMessageReceiver.Setup(m => m.ReceiveLastPulseEventByKeyAsync(key, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new PulseEvent(key, true));
            mockMessageReceiver
             .Setup(m => m.ConsumePulseEventAsync(key, It.IsAny<CancellationToken>()))
             .Returns(AsyncEnumerable(new List<PulseEvent>() { new PulseEvent(key, true) }));
            mockMessageReceiver
             .Setup(m => m.ConsumeConfigurationEventAsync(key, It.IsAny<CancellationToken>()))
             .Returns(AsyncEnumerable(new List<ConfigurationEvent>() { new ConfigurationEvent(key, TimeSpan.FromSeconds(2)) }));
            // Act
            serverStatisticsCollector.StartConsumingStatistics(key);
            await Task.Delay(TimeSpan.FromSeconds(4));
            serverStatisticsCollector.StopConsumingStatistics(key);
            // Assert
            mockStatisticsSender.Verify(m => m.SendStatisticsAsync(It.IsAny<string>(), It.IsAny<ServerStatistics>()), Times.AtLeast(3));
        }
        [Test]
        public async Task StopConsumingStatistics_ShouldLogInformationWhenCanceled()
        {
            // Arrange
            var key = "testKey";
            mockMessageReceiver.Setup(m => m.ReceiveLastConfigurationEventByKeyAsync(key, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ConfigurationEvent(key, TimeSpan.Zero));
            mockMessageReceiver.Setup(m => m.ReceiveLastPulseEventByKeyAsync(key, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new PulseEvent(key, false));
            mockMessageReceiver
            .Setup(m => m.ConsumePulseEventAsync(key, It.IsAny<CancellationToken>()))
            .Returns(AsyncEnumerable(new List<PulseEvent>() { new PulseEvent(key, true) }));
            mockMessageReceiver
             .Setup(m => m.ConsumeConfigurationEventAsync(key, It.IsAny<CancellationToken>()))
             .Returns(AsyncEnumerable(new List<ConfigurationEvent>()));
            // Act
            serverStatisticsCollector.StartConsumingStatistics(key);
            await Task.Delay(TimeSpan.FromSeconds(1));
            serverStatisticsCollector.StopConsumingStatistics(key);
            // Assert

            mockLogger.Verify(
              x => x.Log(
                  It.IsAny<LogLevel>(),
                  It.IsAny<EventId>(),
                  It.Is<It.IsAnyType>((v, t) => true),
                  It.IsAny<Exception>(),
                  It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)));
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
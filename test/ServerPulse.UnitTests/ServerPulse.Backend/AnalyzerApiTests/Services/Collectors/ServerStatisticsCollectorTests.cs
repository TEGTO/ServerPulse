using AnalyzerApi.Domain.Dtos.Wrappers;
using AnalyzerApi.Domain.Models;
using AnalyzerApi.Services.Interfaces;
using Moq;

namespace AnalyzerApi.Services.Collectors.Tests
{
    [TestFixture]
    internal class ServerStatisticsCollectorTests
    {
        private Mock<IEventReceiver<PulseEventWrapper>> mockPulseReceiver;
        private Mock<IEventReceiver<ConfigurationEventWrapper>> mockConfReceiver;
        private Mock<IStatisticsReceiver<ServerStatistics>> mockStatisticsReceiver;
        private ServerStatisticsCollector collector;

        [SetUp]
        public void Setup()
        {
            mockPulseReceiver = new Mock<IEventReceiver<PulseEventWrapper>>();
            mockConfReceiver = new Mock<IEventReceiver<ConfigurationEventWrapper>>();
            mockStatisticsReceiver = new Mock<IStatisticsReceiver<ServerStatistics>>();

            collector = new ServerStatisticsCollector(
                mockPulseReceiver.Object,
                mockConfReceiver.Object,
                mockStatisticsReceiver.Object
            );
        }

        [Test]
        public async Task ReceiveLastStatisticsAsync_ReturnsCorrectStatistics_WhenAllTasksSucceed()
        {
            // Arrange
            var key = "testKey";
            var configEvent = new ConfigurationEventWrapper { ServerKeepAliveInterval = TimeSpan.FromSeconds(10), CreationDateUTC = DateTime.UtcNow.AddMinutes(-10) };
            var pulseEvent = new PulseEventWrapper { IsAlive = true, CreationDateUTC = DateTime.UtcNow };
            var lastStatistics = new ServerStatistics { IsAlive = true, ServerUptime = TimeSpan.FromMinutes(0), LastPulseDateTimeUTC = DateTime.UtcNow.AddMinutes(-5) };

            mockConfReceiver.Setup(m => m.ReceiveLastEventByKeyAsync(key, It.IsAny<CancellationToken>()))
                            .ReturnsAsync(configEvent);
            mockPulseReceiver.Setup(m => m.ReceiveLastEventByKeyAsync(key, It.IsAny<CancellationToken>()))
                             .ReturnsAsync(pulseEvent);
            mockStatisticsReceiver.Setup(m => m.ReceiveLastStatisticsAsync(key, It.IsAny<CancellationToken>()))
                                  .ReturnsAsync(lastStatistics);
            // Act
            var result = await collector.ReceiveLastStatisticsAsync(key, CancellationToken.None);
            // Assert
            Assert.IsTrue(result.IsAlive);
            Assert.IsNotNull(result.ServerUptime);
            Assert.That(result.LastServerUptime, Is.InRange(TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(5.1)));
            Assert.That(result.LastPulseDateTimeUTC, Is.EqualTo(pulseEvent.CreationDateUTC));
        }
        [Test]
        public async Task ReceiveLastStatisticsAsync_ReturnsNotAliveStatistics_WhenPulseIsOld()
        {
            // Arrange
            var key = "testKey";
            var configEvent = new ConfigurationEventWrapper { ServerKeepAliveInterval = TimeSpan.FromSeconds(10), CreationDateUTC = DateTime.UtcNow.AddMinutes(-10) };
            var pulseEvent = new PulseEventWrapper { IsAlive = true, CreationDateUTC = DateTime.UtcNow.AddMinutes(-11) };
            var lastStatistics = new ServerStatistics { IsAlive = true, ServerUptime = TimeSpan.FromMinutes(5), LastPulseDateTimeUTC = DateTime.UtcNow.AddMinutes(-5) };
            mockConfReceiver.Setup(m => m.ReceiveLastEventByKeyAsync(key, It.IsAny<CancellationToken>()))
                            .ReturnsAsync(configEvent);
            mockPulseReceiver.Setup(m => m.ReceiveLastEventByKeyAsync(key, It.IsAny<CancellationToken>()))
                             .ReturnsAsync(pulseEvent);
            mockStatisticsReceiver.Setup(m => m.ReceiveLastStatisticsAsync(key, It.IsAny<CancellationToken>()))
                                  .ReturnsAsync(lastStatistics);
            // Act
            var result = await collector.ReceiveLastStatisticsAsync(key, CancellationToken.None);
            // Assert
            Assert.IsFalse(result.IsAlive);
        }
        [Test]
        public async Task ReceiveLastStatisticsAsync_HandlesNullConfigurationAndPulseEvents()
        {
            // Arrange
            var key = "testKey";
            var lastStatistics = new ServerStatistics { IsAlive = true, ServerUptime = TimeSpan.FromMinutes(5), LastPulseDateTimeUTC = DateTime.UtcNow.AddMinutes(-5) };
            mockConfReceiver.Setup(m => m.ReceiveLastEventByKeyAsync(key, It.IsAny<CancellationToken>()))
                            .ReturnsAsync((ConfigurationEventWrapper?)null);
            mockPulseReceiver.Setup(m => m.ReceiveLastEventByKeyAsync(key, It.IsAny<CancellationToken>()))
                             .ReturnsAsync((PulseEventWrapper?)null);
            mockStatisticsReceiver.Setup(m => m.ReceiveLastStatisticsAsync(key, It.IsAny<CancellationToken>()))
                                  .ReturnsAsync(lastStatistics);
            // Act
            var result = await collector.ReceiveLastStatisticsAsync(key, CancellationToken.None);
            // Assert
            Assert.IsFalse(result.IsAlive);
            Assert.IsFalse(result.DataExists);
        }

        [Test]
        public void CalculateIsServerAlive_ReturnsTrue_WhenPulseWithinInterval()
        {
            // Arrange
            var pulseEvent = new PulseEventWrapper { IsAlive = true, CreationDateUTC = DateTime.UtcNow };
            var configEvent = new ConfigurationEventWrapper { ServerKeepAliveInterval = TimeSpan.FromMinutes(5) };
            // Act
            var result = InvokePrivateMethodAsync<bool>(collector, "CalculateIsServerAlive", pulseEvent, configEvent);
            // Assert
            Assert.IsTrue(result);
        }
        [Test]
        public void CalculateIsServerAlive_ReturnsFalse_WhenPulseOutsideInterval()
        {
            // Arrange
            var pulseEvent = new PulseEventWrapper { IsAlive = true, CreationDateUTC = DateTime.UtcNow.AddMinutes(-10) };
            var configEvent = new ConfigurationEventWrapper { ServerKeepAliveInterval = TimeSpan.FromMinutes(5) };
            // Act
            var result = InvokePrivateMethodAsync<bool>(collector, "CalculateIsServerAlive", pulseEvent, configEvent);
            // Assert
            Assert.IsFalse(result);
        }
        [Test]
        public void CalculateServerUptime_ReturnsZero_WhenLastStatisticsIsNull()
        {
            // Act
            var result = InvokePrivateMethodAsync<TimeSpan>(collector, "CalculateServerUptime", new object[] { (ServerStatistics?)null });
            // Assert
            Assert.That(result, Is.EqualTo(TimeSpan.Zero));
        }
        [Test]
        public void CalculateLastUptime_ReturnsCurrentUptime_WhenServerIsAlive()
        {
            // Arrange
            var lastStatistics = new ServerStatistics { IsAlive = true, ServerUptime = TimeSpan.FromMinutes(5) };
            var currentUptime = TimeSpan.FromMinutes(10);
            // Act
            var result = InvokePrivateMethodAsync<TimeSpan>(collector, "CalculateLastUptime", true, lastStatistics, currentUptime);
            // Assert
            Assert.That(result, Is.EqualTo(currentUptime));
        }
        [Test]
        public void CalculateLastUptime_ReturnsLastStatisticsUptime_WhenServerIsNotAlive()
        {
            // Arrange
            var lastStatistics = new ServerStatistics { IsAlive = true, ServerUptime = TimeSpan.FromMinutes(5) };
            // Act
            var result = InvokePrivateMethodAsync<TimeSpan>(collector, "CalculateLastUptime", false, lastStatistics, null);
            // Assert
            Assert.That(result, Is.EqualTo(TimeSpan.FromMinutes(5)));
        }

        private T? InvokePrivateMethodAsync<T>(object obj, string methodName, params object[] parameters)
        {
            var method = obj.GetType().GetMethod(methodName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (method != null)
            {
                return (T?)method.Invoke(obj, parameters);
            }
            return default(T);
        }
    }
}
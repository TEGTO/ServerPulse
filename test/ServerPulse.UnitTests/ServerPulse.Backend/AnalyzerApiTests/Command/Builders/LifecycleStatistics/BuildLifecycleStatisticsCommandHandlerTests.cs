using AnalyzerApi.Infrastructure.Models.Statistics;
using AnalyzerApi.Infrastructure.Models.Wrappers;
using AnalyzerApi.Services.Receivers.Event;
using AnalyzerApi.Services.Receivers.Statistics;
using Moq;
using System.Globalization;

namespace AnalyzerApi.Command.Builders.LifecycleStatistics.Tests
{
    [TestFixture]
    internal class BuildLifecycleStatisticsCommandHandlerTests
    {
        private Mock<IEventReceiver<PulseEventWrapper>> mockPulseReceiver;
        private Mock<IEventReceiver<ConfigurationEventWrapper>> mockConfReceiver;
        private Mock<IStatisticsReceiver<ServerLifecycleStatistics>> mockStatisticsReceiver;
        private BuildLifecycleStatisticsCommandHandler handler;

        [SetUp]
        public void Setup()
        {
            mockPulseReceiver = new Mock<IEventReceiver<PulseEventWrapper>>();
            mockConfReceiver = new Mock<IEventReceiver<ConfigurationEventWrapper>>();
            mockStatisticsReceiver = new Mock<IStatisticsReceiver<ServerLifecycleStatistics>>();

            handler = new BuildLifecycleStatisticsCommandHandler(
                mockPulseReceiver.Object,
                mockConfReceiver.Object,
                mockStatisticsReceiver.Object);
        }

        [Test]
        [TestCase("2023-12-14T10:00:00", null, "2023-12-14T10:05:00", true, Description = "Server remains alive within interval.")]
        [TestCase("2023-12-14T10:00:00", "2023-12-14T10:30:00", "2023-12-14T10:05:00", false, Description = "Server pulse outside interval.")]
        public async Task Handle_IsAliveCalculation(
            string configurationTimestamp,
            string? pulseTimestamp,
            string lastPulseTimestamp,
            bool expectedIsAlive)
        {
            // Arrange
            var key = "testKey";

            var configEvent = new ConfigurationEventWrapper
            {
                Id = Guid.NewGuid().ToString(),
                Key = key,
                CreationDateUTC = DateTime.Parse(configurationTimestamp, new CultureInfo("en-US")),
                ServerKeepAliveInterval = TimeSpan.FromMinutes(10)
            };

            var pulseEvent = new PulseEventWrapper
            {
                Id = Guid.NewGuid().ToString(),
                Key = key,
                CreationDateUTC = string.IsNullOrEmpty(pulseTimestamp) ? DateTime.UtcNow : DateTime.Parse(pulseTimestamp, new CultureInfo("en-US")),
                IsAlive = true
            };

            var lastStatistics = new ServerLifecycleStatistics
            {
                LastPulseDateTimeUTC = DateTime.Parse(lastPulseTimestamp, new CultureInfo("en-US")),
                IsAlive = false,
                ServerUptime = TimeSpan.FromHours(2)
            };

            mockConfReceiver.Setup(m => m.GetLastEventByKeyAsync(key, It.IsAny<CancellationToken>()))
                .ReturnsAsync(configEvent);

            mockPulseReceiver.Setup(m => m.GetLastEventByKeyAsync(key, It.IsAny<CancellationToken>()))
                .ReturnsAsync(pulseEvent);

            mockStatisticsReceiver.Setup(m => m.GetLastStatisticsAsync(key, It.IsAny<CancellationToken>()))
                .ReturnsAsync(lastStatistics);

            // Act
            var result = await handler.Handle(new BuildStatisticsCommand<ServerLifecycleStatistics>(key), CancellationToken.None);

            // Assert
            Assert.That(result.IsAlive, Is.EqualTo(expectedIsAlive));
        }

        [Test]
        [TestCase(60, 30, Description = "Calculates uptime when server is alive.")]
        [TestCase(0, 0, Description = "Zero uptime when server is not alive.")]
        public async Task Handle_CalculateUptime(int initialUptimeMinutes, int pulseAgeSeconds)
        {
            // Arrange
            var key = "testKey";
            var now = DateTime.UtcNow;
            var configEvent = new ConfigurationEventWrapper
            {
                Id = Guid.NewGuid().ToString(),
                Key = key,
                CreationDateUTC = now.AddMinutes(-10),
                ServerKeepAliveInterval = TimeSpan.FromMinutes(10)
            };

            var pulseEvent = new PulseEventWrapper
            {
                Id = Guid.NewGuid().ToString(),
                Key = key,
                CreationDateUTC = now.AddSeconds(-pulseAgeSeconds),
                IsAlive = true
            };

            var lastStatistics = new ServerLifecycleStatistics
            {
                IsAlive = true,
                ServerUptime = TimeSpan.FromMinutes(initialUptimeMinutes),
                LastPulseDateTimeUTC = now.AddSeconds(-pulseAgeSeconds)
            };

            mockConfReceiver.Setup(m => m.GetLastEventByKeyAsync(key, It.IsAny<CancellationToken>()))
                .ReturnsAsync(configEvent);

            mockPulseReceiver.Setup(m => m.GetLastEventByKeyAsync(key, It.IsAny<CancellationToken>()))
                .ReturnsAsync(pulseEvent);

            mockStatisticsReceiver.Setup(m => m.GetLastStatisticsAsync(key, It.IsAny<CancellationToken>()))
                .ReturnsAsync(lastStatistics);

            // Act
            var result = await handler.Handle(new BuildStatisticsCommand<ServerLifecycleStatistics>(key), CancellationToken.None);

            // Assert
            var expectedUptime = lastStatistics.ServerUptime + TimeSpan.FromSeconds(pulseAgeSeconds);
            Assert.That(Math.Round(result.ServerUptime!.Value.TotalSeconds), Is.EqualTo(Math.Round(expectedUptime.Value.TotalSeconds)));
        }

        [Test]
        [TestCase(null, null, null, false, false, Description = "No data: Configuration, Pulse, or Statistics.")]
        [TestCase("2023-12-14T10:00:00", null, null, false, true, Description = "Only Configuration exists.")]
        [TestCase(null, "2023-12-14T10:10:00", null, false, false, Description = "Only Pulse exists.")]
        [TestCase(null, null, "2023-12-14T10:20:00", false, false, Description = "Only Statistics exist.")]
        [TestCase("2023-12-14T10:00:00", "2023-12-14T10:10:00", "2023-12-14T10:20:00", true, true, Description = "All exists.")]
        public async Task Handle_DataAvailability(
            string? configTimestamp,
            string? pulseTimestamp,
            string? statsTimestamp,
            bool expectedIsAlive,
            bool expectedDataExists)
        {
            // Arrange
            var key = "testKey";

            var configEvent = configTimestamp != null
                ? new ConfigurationEventWrapper
                {
                    Id = Guid.NewGuid().ToString(),
                    Key = key,
                    CreationDateUTC = DateTime.Parse(configTimestamp, new CultureInfo("en-US")),
                    ServerKeepAliveInterval = TimeSpan.FromMinutes(10)
                }
                : null;

            var lastStatistics = statsTimestamp != null
                ? new ServerLifecycleStatistics
                {
                    LastPulseDateTimeUTC = DateTime.Parse(statsTimestamp, new CultureInfo("en-US")),
                    IsAlive = true
                }
                : null;

            var pulseEvent = pulseTimestamp != null
                ? new PulseEventWrapper
                {
                    Id = Guid.NewGuid().ToString(),
                    Key = key,
                    CreationDateUTC = DateTime.Parse(pulseTimestamp, new CultureInfo("en-US")),
                    IsAlive = expectedIsAlive
                }
                : null;

            if (pulseEvent != null && configEvent != null && configEvent != null)
            {
                pulseEvent.CreationDateUTC = DateTime.UtcNow;
            }

            mockConfReceiver.Setup(m => m.GetLastEventByKeyAsync(key, It.IsAny<CancellationToken>()))
                .ReturnsAsync(configEvent);

            mockPulseReceiver.Setup(m => m.GetLastEventByKeyAsync(key, It.IsAny<CancellationToken>()))
                .ReturnsAsync(pulseEvent);

            mockStatisticsReceiver.Setup(m => m.GetLastStatisticsAsync(key, It.IsAny<CancellationToken>()))
                .ReturnsAsync(lastStatistics);

            // Act
            var result = await handler.Handle(new BuildStatisticsCommand<ServerLifecycleStatistics>(key), CancellationToken.None);

            // Assert
            Assert.That(result.IsAlive, Is.EqualTo(expectedIsAlive));
            Assert.That(result.DataExists, Is.EqualTo(expectedDataExists));
        }
    }
}
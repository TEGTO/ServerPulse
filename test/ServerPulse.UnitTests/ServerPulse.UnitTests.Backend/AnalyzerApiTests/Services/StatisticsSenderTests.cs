using AnalyzerApi.Domain.Models;
using AnalyzerApi.Hubs;
using AnalyzerApi.Services;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Moq;
using System.Text.Json;

namespace AnalyzerApiTests.Services
{
    [TestFixture]
    internal class StatisticsSenderTests
    {
        private Mock<IHubContext<StatisticsHub, IStatisticsHubClient>> mockHubContext;
        private Mock<IStatisticsHubClient> mockClientProxy;
        private Mock<ILogger<StatisticsSender>> mockLogger;
        private StatisticsSender _statisticsSender;

        [SetUp]
        public void SetUp()
        {
            mockHubContext = new Mock<IHubContext<StatisticsHub, IStatisticsHubClient>>();
            mockClientProxy = new Mock<IStatisticsHubClient>();
            mockLogger = new Mock<ILogger<StatisticsSender>>();

            mockHubContext
                .Setup(hub => hub.Clients.Group(It.IsAny<string>()))
                .Returns(mockClientProxy.Object);

            _statisticsSender = new StatisticsSender(mockHubContext.Object, mockLogger.Object);
        }

        [Test]
        public async Task SendStatisticsAsync_LogsAndSendsStatistics()
        {
            // Arrange
            var key = "test-key";
            var serverStatistics = new ServerStatistics
            {
                IsAlive = true,
                DataExists = true,
                ServerLastStartDateTime = DateTime.UtcNow,
                ServerUptime = TimeSpan.FromHours(5),
                LastServerUptime = TimeSpan.FromMinutes(30),
                LastPulseDateTime = DateTime.UtcNow,
            };
            var expectedSerializedData = JsonSerializer.Serialize(serverStatistics);
            // Act
            await _statisticsSender.SendStatisticsAsync(key, serverStatistics);
            // Assert
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Trace,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Server with key '{key}' statistics: {expectedSerializedData}")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
            mockClientProxy.Verify(
                x => x.ReceiveStatistics(key, expectedSerializedData),
                Times.Once);
        }
    }
}
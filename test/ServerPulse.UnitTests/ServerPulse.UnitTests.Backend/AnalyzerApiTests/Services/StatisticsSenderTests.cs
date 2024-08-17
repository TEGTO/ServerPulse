using AnalyzerApi;
using AnalyzerApi.Domain.Dtos.Responses;
using AnalyzerApi.Domain.Models;
using AnalyzerApi.Hubs;
using AnalyzerApi.Services;
using AutoMapper;
using MessageBus.Interfaces;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using System.Text.Json;

namespace AnalyzerApiTests.Services
{
    [TestFixture]
    internal class StatisticsSenderTests
    {
        private Mock<IHubContext<StatisticsHub<ServerStatisticsCollector>, IStatisticsHubClient>> mockHubStatisticsContext;
        private Mock<IHubContext<StatisticsHub<LoadStatisticsCollector>, IStatisticsHubClient>> mockHubLoadStatisticsContext;
        private Mock<IMessageProducer> mockProducer;
        private Mock<IMapper> mockMapper;
        private Mock<ILogger<StatisticsSender>> mockLogger;
        private Mock<IConfiguration> mockConfiguration;
        private StatisticsSender statisticsSender;

        [SetUp]
        public void SetUp()
        {
            mockHubStatisticsContext = new Mock<IHubContext<StatisticsHub<ServerStatisticsCollector>, IStatisticsHubClient>>();
            mockHubLoadStatisticsContext = new Mock<IHubContext<StatisticsHub<LoadStatisticsCollector>, IStatisticsHubClient>>();
            mockProducer = new Mock<IMessageProducer>();
            mockMapper = new Mock<IMapper>();
            mockLogger = new Mock<ILogger<StatisticsSender>>();
            mockConfiguration = new Mock<IConfiguration>();

            mockConfiguration
                .SetupGet(config => config[Configuration.KAFKA_SERVER_STATISTICS_TOPIC])
                .Returns("server-statistics-");

            statisticsSender = new StatisticsSender(
                mockHubStatisticsContext.Object,
                mockHubLoadStatisticsContext.Object,
                mockProducer.Object,
                mockMapper.Object,
                mockConfiguration.Object,
                mockLogger.Object);
        }

        [Test]
        public async Task SendServerStatisticsAsync_LogsAndSendsStatisticsToKafkaAndHub()
        {
            // Arrange
            var key = "test-key";
            var serverStatistics = new ServerStatistics
            {
                IsAlive = true,
                DataExists = true,
                ServerLastStartDateTimeUTC = DateTime.UtcNow,
                ServerUptime = TimeSpan.FromHours(5),
                LastServerUptime = TimeSpan.FromMinutes(30),
                LastPulseDateTimeUTC = DateTime.UtcNow,
            };
            var expectedSerializedStatistics = JsonSerializer.Serialize(serverStatistics);
            var expectedResponse = new ServerStatisticsResponse();
            var expectedSerializedData = JsonSerializer.Serialize(expectedResponse);
            var topic = "server-statistics-" + key;

            mockMapper.Setup(m => m.Map<ServerStatisticsResponse>(serverStatistics))
                .Returns(expectedResponse);

            mockProducer.Setup(p => p.ProduceAsync(topic, It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var mockClientProxy = new Mock<IStatisticsHubClient>();
            mockHubStatisticsContext
                .Setup(hub => hub.Clients.Group(key))
                .Returns(mockClientProxy.Object);
            // Act
            await statisticsSender.SendServerStatisticsAsync(key, serverStatistics, CancellationToken.None);
            // Assert
            mockProducer.Verify(p => p.ProduceAsync(topic, expectedSerializedStatistics, It.IsAny<CancellationToken>()), Times.Once);
            mockClientProxy.Verify(x => x.ReceiveStatistics(key, expectedSerializedData), Times.Once);
        }

        [Test]
        public async Task SendServerLoadStatisticsAsync_SendsStatisticsToHub()
        {
            // Arrange
            var key = "test-key";
            var serverLoadStatistics = new ServerLoadStatistics
            {
            };
            var expectedResponse = new ServerLoadStatisticsResponse();
            var expectedSerializedData = JsonSerializer.Serialize(expectedResponse);

            mockMapper.Setup(m => m.Map<ServerLoadStatisticsResponse>(serverLoadStatistics))
                .Returns(expectedResponse);

            var mockClientProxy = new Mock<IStatisticsHubClient>();
            mockHubLoadStatisticsContext
                .Setup(hub => hub.Clients.Group(key))
                .Returns(mockClientProxy.Object);

            // Act
            await statisticsSender.SendServerLoadStatisticsAsync(key, serverLoadStatistics, CancellationToken.None);

            // Assert
            mockClientProxy.Verify(x => x.ReceiveStatistics(key, expectedSerializedData), Times.Once);
        }
    }
}
using AnalyzerApi.Domain.Dtos.Responses;
using AnalyzerApi.Domain.Models;
using AnalyzerApi.Hubs;
using AutoMapper;
using MessageBus.Interfaces;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using System.Reflection;
using System.Text.Json;

namespace AnalyzerApi.Services.Tests
{
    [TestFixture]
    internal class StatisticsSenderTests
    {
        private Mock<IHubContext<StatisticsHub<ServerStatistics>, IStatisticsHubClient>> mockHubStatisticsContext;
        private Mock<IHubContext<StatisticsHub<ServerLoadStatistics>, IStatisticsHubClient>> mockHubLoadStatisticsContext;
        private Mock<IHubContext<StatisticsHub<ServerCustomStatistics>, IStatisticsHubClient>> mockHubCustomEventStatisticsContext;
        private Mock<IMessageProducer> mockProducer;
        private Mock<IMapper> mockMapper;
        private Mock<ILogger<StatisticsSender>> mockLogger;
        private Mock<IConfiguration> mockConfiguration;
        private StatisticsSender statisticsSender;

        [SetUp]
        public void SetUp()
        {
            mockHubStatisticsContext = new Mock<IHubContext<StatisticsHub<ServerStatistics>, IStatisticsHubClient>>();
            mockHubLoadStatisticsContext = new Mock<IHubContext<StatisticsHub<ServerLoadStatistics>, IStatisticsHubClient>>();
            mockHubCustomEventStatisticsContext = new Mock<IHubContext<StatisticsHub<ServerCustomStatistics>, IStatisticsHubClient>>();
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
                mockHubCustomEventStatisticsContext.Object,
                mockProducer.Object,
                mockMapper.Object,
                mockConfiguration.Object,
                mockLogger.Object);
        }

        [Test]
        public async Task SendServerStatisticsAsync_ValidKeyAndStatistics_SendsToKafkaAndHub()
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
            var methodInfo = statisticsSender.GetType()
                                   .GetMethod("SendServerStatisticsAsync", BindingFlags.NonPublic | BindingFlags.Instance);
            var task = (Task)methodInfo?.Invoke(statisticsSender, new object[] { key, serverStatistics, CancellationToken.None });
            await task;
            // Assert
            mockProducer.Verify(p => p.ProduceAsync(topic, expectedSerializedStatistics, It.IsAny<CancellationToken>()), Times.Once);
            mockClientProxy.Verify(x => x.ReceiveStatistics(key, expectedSerializedData), Times.Once);
        }
        [Test]
        public async Task SendServerLoadStatisticsAsync_ValidKeyAndStatistics_SendsToHub()
        {
            // Arrange
            var key = "test-key";
            var serverLoadStatistics = new ServerLoadStatistics();
            var expectedResponse = new ServerLoadStatisticsResponse();
            var expectedSerializedData = JsonSerializer.Serialize(expectedResponse);
            mockMapper.Setup(m => m.Map<ServerLoadStatisticsResponse>(serverLoadStatistics))
                .Returns(expectedResponse);
            var mockClientProxy = new Mock<IStatisticsHubClient>();
            mockHubLoadStatisticsContext
                .Setup(hub => hub.Clients.Group(key))
                .Returns(mockClientProxy.Object);
            // Act
            var methodInfo = statisticsSender.GetType()
                             .GetMethod("SendServerLoadStatisticsAsync", BindingFlags.NonPublic | BindingFlags.Instance);
            var task = (Task)methodInfo?.Invoke(statisticsSender, new object[] { key, serverLoadStatistics, CancellationToken.None });
            await task;
            // Assert
            mockClientProxy.Verify(x => x.ReceiveStatistics(key, expectedSerializedData), Times.Once);
        }
        [Test]
        public async Task SendServerCustomStatisticsAsync_ValidKeyAndStatistics_SendsToHub()
        {
            // Arrange
            var key = "test-key";
            var statistics = new ServerCustomStatistics();
            var expectedResponse = new CustomEventStatisticsResponse();
            var expectedSerializedData = JsonSerializer.Serialize(expectedResponse);
            mockMapper.Setup(m => m.Map<CustomEventStatisticsResponse>(statistics))
                .Returns(expectedResponse);
            var mockClientProxy = new Mock<IStatisticsHubClient>();
            mockHubCustomEventStatisticsContext
                .Setup(hub => hub.Clients.Group(key))
                .Returns(mockClientProxy.Object);
            // Act
            var methodInfo = statisticsSender.GetType()
                                .GetMethod("SendServerCustomStatisticsAsync", BindingFlags.NonPublic | BindingFlags.Instance);
            var task = (Task)methodInfo?.Invoke(statisticsSender, new object[] { key, statistics, CancellationToken.None });
            await task;
            // Assert
            mockClientProxy.Verify(x => x.ReceiveStatistics(key, expectedSerializedData), Times.Once);
        }
        [Test]
        public async Task SendStatisticsAsync_ServerStatisticsType_SendsServerStatistics()
        {
            // Arrange
            var key = "test-key";
            var serverStatistics = new ServerStatistics();
            var mockClientProxy = new Mock<IStatisticsHubClient>();
            mockHubStatisticsContext
              .Setup(hub => hub.Clients.Group(key))
              .Returns(mockClientProxy.Object);
            mockProducer.Setup(p => p.ProduceAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            // Act
            await statisticsSender.SendStatisticsAsync(key, serverStatistics, CancellationToken.None);
            // Assert
            mockProducer.Verify(p => p.ProduceAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        }
        [Test]
        public async Task SendStatisticsAsync_ServerLoadStatisticsType_SendsServerLoadStatistics()
        {
            // Arrange
            var key = "test-key";
            var serverLoadStatistics = new ServerLoadStatistics();
            var mockClientProxy = new Mock<IStatisticsHubClient>();
            mockHubLoadStatisticsContext
                .Setup(hub => hub.Clients.Group(key))
                .Returns(mockClientProxy.Object);
            // Act
            await statisticsSender.SendStatisticsAsync(key, serverLoadStatistics, CancellationToken.None);
            // Assert
            mockClientProxy.Verify(x => x.ReceiveStatistics(key, It.IsAny<string>()), Times.Once);
        }
        [Test]
        public async Task SendStatisticsAsync_CustomEventStatisticsType_SendsCustomEventStatistics()
        {
            // Arrange
            var key = "test-key";
            var customEventStatistics = new ServerCustomStatistics();
            var mockClientProxy = new Mock<IStatisticsHubClient>();
            mockHubCustomEventStatisticsContext
                .Setup(hub => hub.Clients.Group(key))
                .Returns(mockClientProxy.Object);
            // Act
            await statisticsSender.SendStatisticsAsync(key, customEventStatistics, CancellationToken.None);
            // Assert
            mockClientProxy.Verify(x => x.ReceiveStatistics(key, It.IsAny<string>()), Times.Once);
        }
        [Test]
        public async Task SendStatisticsAsync_UnsupportedStatisticsType_DoesNotSendStatistics()
        {
            // Arrange
            var key = "test-key";
            var unsupportedStatistics = new UnsupportedStatisticsType();
            // Act
            await statisticsSender.SendStatisticsAsync(key, unsupportedStatistics, CancellationToken.None);
            // Assert
            mockProducer.Verify(p => p.ProduceAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
            mockHubStatisticsContext.Verify(h => h.Clients.Group(It.IsAny<string>()), Times.Never);
            mockHubLoadStatisticsContext.Verify(h => h.Clients.Group(It.IsAny<string>()), Times.Never);
            mockHubCustomEventStatisticsContext.Verify(h => h.Clients.Group(It.IsAny<string>()), Times.Never);
        }

        public class UnsupportedStatisticsType : BaseStatistics
        {
        }
    }
}
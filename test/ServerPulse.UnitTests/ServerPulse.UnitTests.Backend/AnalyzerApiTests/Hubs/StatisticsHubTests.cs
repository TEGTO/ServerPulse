using AnalyzerApi.Hubs;
using AnalyzerApi.Services;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Moq;

namespace AnalyzerApiTests.Hubs
{
    [TestFixture]
    internal class StatisticsHubTests
    {
        private Mock<IServerStatisticsCollector> mockServerStatisticsCollector;
        private Mock<ILogger<StatisticsHub>> mockLogger;
        private Mock<IHubCallerClients<IStatisticsHubClient>> mockClients;
        private Mock<IGroupManager> mockGroups;
        private Mock<HubCallerContext> mockContext;
        private StatisticsHub hub;

        [SetUp]
        public void SetUp()
        {
            mockServerStatisticsCollector = new Mock<IServerStatisticsCollector>();
            mockLogger = new Mock<ILogger<StatisticsHub>>();
            mockClients = new Mock<IHubCallerClients<IStatisticsHubClient>>();
            mockGroups = new Mock<IGroupManager>();
            mockContext = new Mock<HubCallerContext>();

            mockContext.SetupGet(c => c.ConnectionId).Returns("connectionId");
        }
        [TearDown]
        public void TearDown()
        {
            hub.Dispose();
        }
        private StatisticsHub GetHub()
        {
            return new StatisticsHub(mockServerStatisticsCollector.Object, mockLogger.Object)
            {
                Clients = mockClients.Object,
                Groups = mockGroups.Object,
                Context = mockContext.Object,
            };
        }

        [Test]
        public async Task StartListenPulse_AddsClientToGroupAndStartsConsuming()
        {
            // Arrange
            hub = GetHub();
            var key = "test-key";
            // Act
            await hub.StartListenPulse(key);
            await hub.StartListenPulse(key);
            // Assert
            mockGroups.Verify(g => g.AddToGroupAsync("connectionId", key, It.IsAny<CancellationToken>()), Times.Exactly(2));
            mockLogger.Verify(l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Start listening pulse with key '{key}'")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Exactly(2));
            mockServerStatisticsCollector.Verify(s => s.StartConsumingStatistics(key), Times.Exactly(2));
        }

        [Test]
        public async Task OnDisconnectedAsync_RemovesClientFromGroups()
        {
            // Arrange
            hub = GetHub();

            var keys = new List<string> { "key1", "key2" };
            await hub.StartListenPulse(keys[0]);
            await hub.StartListenPulse(keys[1]);
            // Act
            await hub.OnDisconnectedAsync(null);
            // Assert
            mockGroups.Verify(g => g.RemoveFromGroupAsync("connectionId", keys[0], It.IsAny<CancellationToken>()), Times.Once);
            mockGroups.Verify(g => g.RemoveFromGroupAsync("connectionId", keys[1], It.IsAny<CancellationToken>()), Times.Once);
            mockServerStatisticsCollector.Verify(s => s.StopConsumingStatistics(keys[0]), Times.Once);
            mockServerStatisticsCollector.Verify(s => s.StopConsumingStatistics(keys[1]), Times.Once);
        }
    }
}
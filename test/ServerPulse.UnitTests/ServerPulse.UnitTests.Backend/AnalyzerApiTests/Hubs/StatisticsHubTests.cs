using AnalyzerApi.Domain.Models;
using AnalyzerApi.Services.Interfaces;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Moq;
using System.Collections.Concurrent;
using System.Reflection;

namespace AnalyzerApi.Hubs.Tests
{
    [TestFixture]
    internal class StatisticsHubTests
    {
        private Mock<IStatisticsConsumer<TestStatistics>> mockStatisticsConsumer;
        private Mock<ILogger<StatisticsHub<TestStatistics>>> mockLogger;
        private Mock<HubCallerContext> mockContext;
        private Mock<IGroupManager> mockGroups;
        private StatisticsHub<TestStatistics> statisticsHub;

        [SetUp]
        public void Setup()
        {
            mockStatisticsConsumer = new Mock<IStatisticsConsumer<TestStatistics>>();
            mockLogger = new Mock<ILogger<StatisticsHub<TestStatistics>>>();
            mockContext = new Mock<HubCallerContext>();
            mockGroups = new Mock<IGroupManager>();

            statisticsHub = new StatisticsHub<TestStatistics>(mockStatisticsConsumer.Object, mockLogger.Object)
            {
                Context = mockContext.Object,
                Groups = mockGroups.Object
            };
        }
        [TearDown]
        public void TearDown()
        {
            statisticsHub.Dispose();
        }

        [Test]
        public async Task StartListen_ShouldAddClientToGroupAndStartListening()
        {
            // Arrange
            var key = "testKey";
            var connectionId = "testConnectionId";
            mockContext.SetupGet(c => c.ConnectionId).Returns(connectionId);
            // Act
            await statisticsHub.StartListen(key);
            // Assert
            mockGroups.Verify(g => g.AddToGroupAsync(connectionId, key, default), Times.Once);
            mockStatisticsConsumer.Verify(s => s.StartConsumingStatistics(key), Times.Once);
            mockLogger.Verify(l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Start listening key '{key}'")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
        [Test]
        public async Task OnDisconnectedAsync_ShouldRemoveClientFromGroupAndStopListening()
        {
            // Arrange
            var connectionId = "testConnectionId";
            var key = "testKey";
            var connectedClients = GetConnectedClients();
            var listenerAmount = GetListenerAmount();
            connectedClients[connectionId] = new List<string> { key };
            listenerAmount[key] = 1;
            mockContext.SetupGet(c => c.ConnectionId).Returns(connectionId);
            // Act
            await statisticsHub.OnDisconnectedAsync(null);
            // Assert
            mockGroups.Verify(g => g.RemoveFromGroupAsync(connectionId, key, default), Times.Once);
            mockStatisticsConsumer.Verify(s => s.StopConsumingStatistics(key), Times.Once);
            mockLogger.Verify(l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Stop listening key '{key}'")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
        [Test]
        public async Task RemoveClientFromGroupAsync_ShouldNotStopListeningIfOtherListenersExist()
        {
            // Arrange
            var connectionId = "testConnectionId";
            var key = "testKey";
            var connectedClients = GetConnectedClients();
            var listenerAmount = GetListenerAmount();
            connectedClients[connectionId] = new List<string> { key };
            listenerAmount[key] = 2;
            mockContext.SetupGet(c => c.ConnectionId).Returns(connectionId);

            // Act
            await InvokePrivateMethodAsync(statisticsHub, "RemoveClientFromGroupAsync", new List<string> { key });

            // Assert
            mockGroups.Verify(g => g.RemoveFromGroupAsync(connectionId, key, default), Times.Once);
            Assert.That(listenerAmount[key], Is.EqualTo(1));
            mockStatisticsConsumer.Verify(s => s.StopConsumingStatistics(key), Times.Never);
        }

        private async Task InvokePrivateMethodAsync(object obj, string methodName, params object[] parameters)
        {
            var method = obj.GetType().GetMethod(methodName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (method != null)
            {
                var task = (Task)method.Invoke(obj, parameters);
                await task;
            }
        }
        private ConcurrentDictionary<string, List<string>> GetConnectedClients()
        {
            Type type = typeof(StatisticsHub<TestStatistics>);
            FieldInfo info = type.GetField("ConnectedClients", BindingFlags.NonPublic | BindingFlags.Static);
            object value = info.GetValue(null);
            return value as ConcurrentDictionary<string, List<string>>;
        }
        private ConcurrentDictionary<string, int> GetListenerAmount()
        {
            Type type = typeof(StatisticsHub<TestStatistics>);
            FieldInfo info = type.GetField("ListenerAmount", BindingFlags.NonPublic | BindingFlags.Static);
            object value = info.GetValue(null);
            return value as ConcurrentDictionary<string, int>;
        }
    }
    public class TestStatistics : BaseStatistics { };
}
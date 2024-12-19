using AnalyzerApi.Infrastructure.Models.Statistics;
using AnalyzerApi.Services.StatisticsDispatchers;
using AnalyzerApiTests;
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
        private Mock<IStatisticsDispatcher<TestStatistics>> mockStatisticsDispatcher;
        private Mock<ILogger<StatisticsHub<TestStatistics>>> mockLogger;
        private Mock<HubCallerContext> mockContext;
        private Mock<IGroupManager> mockGroups;
        private StatisticsHub<TestStatistics> statisticsHub;

        [SetUp]
        public void Setup()
        {
            mockStatisticsDispatcher = new Mock<IStatisticsDispatcher<TestStatistics>>();
            mockLogger = new Mock<ILogger<StatisticsHub<TestStatistics>>>();
            mockContext = new Mock<HubCallerContext>();
            mockGroups = new Mock<IGroupManager>();

            statisticsHub = new StatisticsHub<TestStatistics>(mockStatisticsDispatcher.Object, mockLogger.Object)
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
        public async Task StartListen_AddsClientToGroupAndStartListening()
        {
            // Arrange
            var key = "testKey";
            var connectionId = "testConnectionId";

            mockContext.SetupGet(c => c.ConnectionId).Returns(connectionId);

            // Act
            await statisticsHub.StartListen(key);

            // Assert
            var connectedClients = statisticsHub.GetFieldValue<ConcurrentDictionary<string, List<string>>>("connectedClients");
            Assert.IsNotNull(connectedClients);
            connectedClients.TryGetValue(connectionId, out var idKeys);
            Assert.IsNotNull(idKeys);
            Assert.IsTrue(idKeys.Contains(key));

            var listenerAmount = statisticsHub.GetFieldValue<ConcurrentDictionary<string, int>>("listenerAmount");
            Assert.IsNotNull(listenerAmount);
            listenerAmount.TryGetValue(key, out var amount);
            Assert.That(amount, Is.EqualTo(1));

            mockGroups.Verify(g => g.AddToGroupAsync(connectionId, key, default), Times.Once);
            mockStatisticsDispatcher.Verify(s => s.StartStatisticsDispatchingAsync(key), Times.Once);
            mockStatisticsDispatcher.Verify(s => s.DispatchInitialStatisticsAsync(key, It.IsAny<CancellationToken>()), Times.Once);
            mockLogger.Verify(l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Start listening to key '{key}'")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()!),
                Times.Once);
        }

        [Test]
        public async Task StartListen_AddsClientToGroupAndStartListeningAmountIsZero()
        {
            // Arrange
            var key = "testKey";
            var connectionId = "testConnectionId";

            var listenerAmount = statisticsHub.GetFieldValue<ConcurrentDictionary<string, int>>("listenerAmount");
            Assert.IsNotNull(listenerAmount);

            listenerAmount[key] = 0;

            mockContext.SetupGet(c => c.ConnectionId).Returns(connectionId);

            // Act
            await statisticsHub.StartListen(key);

            // Assert
            var connectedClients = statisticsHub.GetFieldValue<ConcurrentDictionary<string, List<string>>>("connectedClients");
            Assert.IsNotNull(connectedClients);
            connectedClients.TryGetValue(connectionId, out var idKeys);
            Assert.IsNotNull(idKeys);
            Assert.IsTrue(idKeys.Contains(key));

            listenerAmount.TryGetValue(key, out var amount);
            Assert.That(amount, Is.EqualTo(1));

            mockGroups.Verify(g => g.AddToGroupAsync(connectionId, key, default), Times.Once);
            mockStatisticsDispatcher.Verify(s => s.StartStatisticsDispatchingAsync(key), Times.Once);
            mockStatisticsDispatcher.Verify(s => s.DispatchInitialStatisticsAsync(key, It.IsAny<CancellationToken>()), Times.Once);
            mockLogger.Verify(l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Start listening to key '{key}'")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()!),
                Times.Once);
        }

        [Test]
        public async Task StartListen_KeyIsAlreadyBeingListening_AddsIdToListAndIncreasesKeyListenerAmount()
        {
            // Arrange
            var key = "testKey";
            var connectionId = "testConnectionId";

            var connectedClients = statisticsHub.GetFieldValue<ConcurrentDictionary<string, List<string>>>("connectedClients");
            var listenerAmount = statisticsHub.GetFieldValue<ConcurrentDictionary<string, int>>("listenerAmount");
            Assert.IsNotNull(connectedClients);
            Assert.IsNotNull(listenerAmount);

            connectedClients[connectionId] = new List<string> { key };
            listenerAmount[key] = 1;

            mockContext.SetupGet(c => c.ConnectionId).Returns(connectionId);

            // Act
            await statisticsHub.StartListen(key);

            // Assert
            connectedClients.TryGetValue(connectionId, out var idKeys);
            Assert.IsNotNull(idKeys);
            Assert.IsTrue(idKeys.Contains(key));

            listenerAmount.TryGetValue(key, out var amount);
            Assert.That(amount, Is.EqualTo(2));

            mockGroups.Verify(g => g.AddToGroupAsync(connectionId, key, default), Times.Once);
            mockStatisticsDispatcher.Verify(s => s.DispatchInitialStatisticsAsync(key, It.IsAny<CancellationToken>()), Times.Once);
            mockStatisticsDispatcher.Verify(s => s.StartStatisticsDispatchingAsync(key), Times.Never);
            mockLogger.Verify(l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Start listening to key '{key}'")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()!),
                Times.Never);
        }

        [Test]
        public async Task OnDisconnectedAsync_RemovesClientFromGroupAndStopListening()
        {
            // Arrange
            var connectionId = "testConnectionId";
            var key = "testKey";

            var connectedClients = statisticsHub.GetFieldValue<ConcurrentDictionary<string, List<string>>>("connectedClients");
            var listenerAmount = statisticsHub.GetFieldValue<ConcurrentDictionary<string, int>>("listenerAmount");
            Assert.IsNotNull(connectedClients);
            Assert.IsNotNull(listenerAmount);

            connectedClients[connectionId] = new List<string> { key };
            listenerAmount[key] = 1;

            mockContext.SetupGet(c => c.ConnectionId).Returns(connectionId);

            // Act
            await statisticsHub.OnDisconnectedAsync(null);

            // Assert
            mockGroups.Verify(g => g.RemoveFromGroupAsync(connectionId, key, default), Times.Once);
            mockStatisticsDispatcher.Verify(s => s.StopStatisticsDispatchingAsync(key), Times.Once);
            mockLogger.Verify(l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Stop listening to key '{key}'")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()!),
                Times.Once);

            Assert.That(listenerAmount[key], Is.EqualTo(0));
            Assert.IsFalse(connectedClients.ContainsKey(connectionId));
        }

        [Test]
        public async Task OnDisconnectedAsync_DoesNotStopListeningIfOtherListenersExist()
        {
            // Arrange
            var connectionId1 = "testConnectionId1";
            var connectionId2 = "testConnectionId2";
            var key = "testKey";

            var connectedClients = statisticsHub.GetFieldValue<ConcurrentDictionary<string, List<string>>>("connectedClients");
            var listenerAmount = statisticsHub.GetFieldValue<ConcurrentDictionary<string, int>>("listenerAmount");
            Assert.IsNotNull(connectedClients);
            Assert.IsNotNull(listenerAmount);

            connectedClients[connectionId1] = new List<string> { key };
            connectedClients[connectionId2] = new List<string> { key };
            listenerAmount[key] = 2;

            mockContext.SetupGet(c => c.ConnectionId).Returns(connectionId2);

            // Act
            await statisticsHub.OnDisconnectedAsync(null);

            // Assert
            mockGroups.Verify(g => g.RemoveFromGroupAsync(connectionId2, key, default), Times.Once);
            mockStatisticsDispatcher.Verify(s => s.StopStatisticsDispatchingAsync(key), Times.Never);
            mockLogger.Verify(l => l.Log(
               LogLevel.Information,
               It.IsAny<EventId>(),
               It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Stop listening to key '{key}'")),
               null,
               It.IsAny<Func<It.IsAnyType, Exception, string>>()!),
               Times.Never);

            Assert.That(listenerAmount[key], Is.EqualTo(1));

            Assert.IsTrue(connectedClients.ContainsKey(connectionId1));
            Assert.IsTrue(connectedClients[connectionId1].Contains(key));
            Assert.IsFalse(connectedClients.ContainsKey(connectionId2));
        }

        [Test]
        public async Task OnDisconnectedAsync_IdIsNotInConnectedClients_NoChanges()
        {
            // Arrange
            var connectionId1 = "testConnectionId1";
            var connectionId2 = "testConnectionId2";
            var key = "testKey";

            var connectedClients = statisticsHub.GetFieldValue<ConcurrentDictionary<string, List<string>>>("connectedClients");
            var listenerAmount = statisticsHub.GetFieldValue<ConcurrentDictionary<string, int>>("listenerAmount");
            Assert.IsNotNull(connectedClients);
            Assert.IsNotNull(listenerAmount);

            connectedClients[connectionId1] = new List<string> { key };
            listenerAmount[key] = 1;

            mockContext.SetupGet(c => c.ConnectionId).Returns(connectionId2);

            // Act
            await statisticsHub.OnDisconnectedAsync(null);

            // Assert
            mockGroups.Verify(g => g.RemoveFromGroupAsync(connectionId2, key, default), Times.Never);
            mockStatisticsDispatcher.Verify(s => s.StopStatisticsDispatchingAsync(key), Times.Never);
            mockLogger.Verify(l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Stop listening to key '{key}'")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()!),
                Times.Never);

            Assert.That(listenerAmount[key], Is.EqualTo(1));
            Assert.IsTrue(connectedClients.ContainsKey(connectionId1));
            Assert.IsTrue(connectedClients[connectionId1].Contains(key));
            Assert.IsFalse(connectedClients.ContainsKey(connectionId2));
        }

        [Test]
        public async Task ConcurrentUpdates_MultipleClients_ThreadSafeOperation()
        {
            // Arrange
            var connectionIds = Enumerable.Range(1, 100).Select(i => $"connectionId{i}").ToList();
            var keys = Enumerable.Range(1, 10).Select(i => $"key{i}").ToList();

            var startIdListeningKeyMethod = statisticsHub.GetType().GetMethod("StartIdListeningKey", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(startIdListeningKeyMethod);

            var tasks = connectionIds.Select(connectionId => Task.Run(async () =>
            {
                foreach (var key in keys)
                {
                    var task = (Task)startIdListeningKeyMethod.Invoke(statisticsHub, [key, connectionId])!;
                    await task!;
                }
            }));

            // Act
            await Task.WhenAll(tasks);

            // Assert
            var connectedClients = statisticsHub.GetFieldValue<ConcurrentDictionary<string, List<string>>>("connectedClients");
            var listenerAmount = statisticsHub.GetFieldValue<ConcurrentDictionary<string, int>>("listenerAmount");
            Assert.IsNotNull(connectedClients);
            Assert.IsNotNull(listenerAmount);

            Assert.That(connectedClients.Count, Is.EqualTo(connectionIds.Count));
            foreach (var key in keys)
            {
                Assert.IsTrue(listenerAmount[key] > 0);
            }

            mockGroups.Verify(g => g.AddToGroupAsync(It.IsAny<string>(), It.IsAny<string>(), default), Times.Exactly(1000));
            mockStatisticsDispatcher.Verify(s => s.StartStatisticsDispatchingAsync(It.IsAny<string>()), Times.Exactly(10));
            mockStatisticsDispatcher.Verify(s => s.DispatchInitialStatisticsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Exactly(1000));
            mockLogger.Verify(l => l.Log(
            LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Start listening to key")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()!),
               Times.Exactly(10));
        }

        [Test]
        public async Task ConcurrentRemovals_MultipleClients_ThreadSafeOperation()
        {
            // Arrange
            var connectionIds = Enumerable.Range(1, 100).Select(i => $"connectionId{i}").ToList();
            var keys = Enumerable.Range(1, 10).Select(i => $"key{i}").ToList();

            var startIdListeningKeyMethod = statisticsHub.GetType().GetMethod("StartIdListeningKey", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(startIdListeningKeyMethod);

            var removeClientFromGroupMethod = statisticsHub.GetType().GetMethod("RemoveClientFromGroupAsync", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(removeClientFromGroupMethod);

            var addTasks = connectionIds.Select(connectionId => Task.Run(async () =>
            {
                foreach (var key in keys)
                {
                    var task = (Task)startIdListeningKeyMethod.Invoke(statisticsHub, [key, connectionId])!;
                    await task!;
                }
            }));
            await Task.WhenAll(addTasks);

            // Act
            var removeTasks = connectionIds.Select(connectionId => Task.Run(async () =>
            {
                var task = (Task)removeClientFromGroupMethod.Invoke(statisticsHub, [keys, connectionId])!;
                await task!;
            }));
            await Task.WhenAll(removeTasks);

            // Assert
            var connectedClients = statisticsHub.GetFieldValue<ConcurrentDictionary<string, List<string>>>("connectedClients");
            var listenerAmount = statisticsHub.GetFieldValue<ConcurrentDictionary<string, int>>("listenerAmount");

            Assert.IsNotNull(connectedClients);
            Assert.IsNotNull(listenerAmount);

            Assert.That(connectedClients.Count, Is.EqualTo(0));

            foreach (var key in keys)
            {
                Assert.IsTrue(listenerAmount[key] == 0);
            }

            mockStatisticsDispatcher.Verify(s => s.StopStatisticsDispatchingAsync(It.IsAny<string>()), Times.Exactly(keys.Count));
        }
    }

    public class TestStatistics : BaseStatistics { };
}
using AnalyzerApi.Domain.Models;
using AnalyzerApi.Services;
using Moq;
using ServerPulse.EventCommunication.Events;

namespace AnalyzerApiTests.Services
{
    [TestFixture]
    internal class ServerAnalyzerTests
    {
        private Mock<IMessageReceiver> mockMessageReceiver;
        private ServerAnalyzer serverAnalyzer;

        [SetUp]
        public void Setup()
        {
            mockMessageReceiver = new Mock<IMessageReceiver>();
            serverAnalyzer = new ServerAnalyzer(mockMessageReceiver.Object);
        }

        [Test]
        public async Task GetCurrentServerStatusByKeyAsync_AliveEventIsTrue_ReturnsServerStatusWithAliveTrue()
        {
            // Arrange
            var key = "validSlotKey";
            var aliveEvent = new AliveEvent(key, true);
            var cancellationToken = CancellationToken.None;
            mockMessageReceiver.Setup(x => x.ReceiveLastAliveEventByKeyAsync(key, cancellationToken)).ReturnsAsync(aliveEvent);
            // Act
            var result = await serverAnalyzer.GetCurrentServerStatusByKeyAsync(key, cancellationToken);
            // Assert
            Assert.IsInstanceOf<ServerStatus>(result);
            Assert.IsTrue(result.IsServerAlive);
        }
        [Test]
        public async Task GetCurrentServerStatusByKeyAsync_AliveEventIsFalse_ReturnsServerStatusWithAliveFalse()
        {
            // Arrange
            var key = "validSlotKey";
            var aliveEvent = new AliveEvent(key, false);
            var cancellationToken = CancellationToken.None;
            mockMessageReceiver.Setup(x => x.ReceiveLastAliveEventByKeyAsync(key, cancellationToken)).ReturnsAsync(aliveEvent);
            // Act
            var result = await serverAnalyzer.GetCurrentServerStatusByKeyAsync(key, cancellationToken);
            // Assert
            Assert.IsInstanceOf<ServerStatus>(result);
            Assert.IsFalse(result.IsServerAlive);
        }
        [Test]
        public async Task GetCurrentServerStatusByKeyAsync_EmptyKey_ReturnsServerStatusWithDefaultValue()
        {
            // Arrange
            var key = string.Empty;
            var aliveEvent = new AliveEvent(key, false);
            var cancellationToken = CancellationToken.None;
            mockMessageReceiver.Setup(x => x.ReceiveLastAliveEventByKeyAsync(key, cancellationToken)).ReturnsAsync(aliveEvent);
            // Act
            var result = await serverAnalyzer.GetCurrentServerStatusByKeyAsync(key, cancellationToken);
            // Assert
            Assert.IsInstanceOf<ServerStatus>(result);
            Assert.IsFalse(result.IsServerAlive);
        }
        [Test]
        public async Task GetCurrentServerStatusByKeyAsync_ValidKey_CallsMessageReceiver()
        {
            // Arrange
            var key = "validSlotKey";
            var aliveEvent = new AliveEvent(key, true);
            var cancellationToken = CancellationToken.None;
            mockMessageReceiver.Setup(x => x.ReceiveLastAliveEventByKeyAsync(key, cancellationToken)).ReturnsAsync(aliveEvent);
            // Act
            await serverAnalyzer.GetCurrentServerStatusByKeyAsync(key, cancellationToken);
            // Assert
            mockMessageReceiver.Verify(x => x.ReceiveLastAliveEventByKeyAsync(
                key,
                cancellationToken
            ), Times.Once);
        }
    }
}
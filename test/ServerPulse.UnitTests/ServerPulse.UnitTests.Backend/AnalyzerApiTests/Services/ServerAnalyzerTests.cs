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
        public async Task GetCurrentServerStatusByKeyAsync_PulseEventIsTrue_ReturnsServerStatusWithAliveTrue()
        {
            // Arrange
            var key = "validSlotKey";
            var aliveEvent = new PulseEvent(key, true);
            var cancellationToken = CancellationToken.None;
            mockMessageReceiver.Setup(x => x.ReceiveLastPulseEventByKeyAsync(key, cancellationToken)).ReturnsAsync(aliveEvent);
            // Act
            var result = await serverAnalyzer.GetServerStatisticsByKeyAsync(key, cancellationToken);
            // Assert
            Assert.IsInstanceOf<ServerStatistics>(result);
            Assert.IsTrue(result.IsAlive);
        }
        [Test]
        public async Task GetCurrentServerStatusByKeyAsync_PulseEventIsFalse_ReturnsServerStatusWithAliveFalse()
        {
            // Arrange
            var key = "validSlotKey";
            var aliveEvent = new PulseEvent(key, false);
            var cancellationToken = CancellationToken.None;
            mockMessageReceiver.Setup(x => x.ReceiveLastPulseEventByKeyAsync(key, cancellationToken)).ReturnsAsync(aliveEvent);
            // Act
            var result = await serverAnalyzer.GetServerStatisticsByKeyAsync(key, cancellationToken);
            // Assert
            Assert.IsInstanceOf<ServerStatistics>(result);
            Assert.IsFalse(result.IsAlive);
        }
        [Test]
        public async Task GetCurrentServerStatusByKeyAsync_EmptyKey_ReturnsServerStatusWithDefaultValue()
        {
            // Arrange
            var key = string.Empty;
            var aliveEvent = new PulseEvent(key, false);
            var cancellationToken = CancellationToken.None;
            mockMessageReceiver.Setup(x => x.ReceiveLastPulseEventByKeyAsync(key, cancellationToken)).ReturnsAsync(aliveEvent);
            // Act
            var result = await serverAnalyzer.GetServerStatisticsByKeyAsync(key, cancellationToken);
            // Assert
            Assert.IsInstanceOf<ServerStatistics>(result);
            Assert.IsFalse(result.IsAlive);
        }
        [Test]
        public async Task GetCurrentServerStatusByKeyAsync_ValidKey_CallsMessageReceiver()
        {
            // Arrange
            var key = "validSlotKey";
            var aliveEvent = new PulseEvent(key, true);
            var cancellationToken = CancellationToken.None;
            mockMessageReceiver.Setup(x => x.ReceiveLastPulseEventByKeyAsync(key, cancellationToken)).ReturnsAsync(aliveEvent);
            // Act
            await serverAnalyzer.GetServerStatisticsByKeyAsync(key, cancellationToken);
            // Assert
            mockMessageReceiver.Verify(x => x.ReceiveLastPulseEventByKeyAsync(
                key,
                cancellationToken
            ), Times.Once);
        }
    }
}
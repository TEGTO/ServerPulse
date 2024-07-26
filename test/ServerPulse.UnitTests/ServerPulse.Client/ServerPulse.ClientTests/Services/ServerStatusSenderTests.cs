using Moq;
using ServerPulse.Client;
using ServerPulse.Client.Services;
using ServerPulse.EventCommunication.Events;

namespace ServerPulse.ClientTests.Services
{
    [TestFixture]
    internal class ServerStatusSenderTests
    {
        private Mock<IMessageSender> mockMessageSender;
        private ServerStatusSender serverStatusSender;
        private Configuration configuration;
        private CancellationTokenSource cancellationTokenSource;

        [SetUp]
        public void Setup()
        {
            mockMessageSender = new Mock<IMessageSender>();
            configuration = new Configuration()
            {
                Key = "example",
                EventController = "http://localhost",
                MaxEventSendingAmount = 10,
                ServerKeepAliveInterval = 1
            };
            serverStatusSender = new ServerStatusSender(mockMessageSender.Object, configuration);
            cancellationTokenSource = new CancellationTokenSource();
        }

        [TearDown]
        public void TearDown()
        {
            serverStatusSender?.Dispose();
            cancellationTokenSource?.Dispose();
        }

        [Test]
        public async Task ExecuteAsync_SendsInitialConfigurationEvent()
        {
            // Arrange
            var confEvent = new ConfigurationEvent("serverKey", TimeSpan.FromSeconds(1));
            string confEventJson = confEvent.ToString();
            mockMessageSender.Setup(m => m.SendJsonAsync(confEventJson, It.IsAny<string>(), It.IsAny<CancellationToken>()))
                             .Returns(Task.CompletedTask);
            // Act
            var executeTask = serverStatusSender.StartAsync(cancellationTokenSource.Token);
            await Task.Delay(500);
            // Assert
            mockMessageSender.Verify(m => m.SendJsonAsync(It.IsAny<string>(), It.Is<string>(s => s.EndsWith("/serverinteraction/configuration")), It.IsAny<CancellationToken>()), Times.Once);
            cancellationTokenSource.Cancel();
            await executeTask;
        }
        [Test]
        public async Task ExecuteAsync_SendsAliveEventPeriodically()
        {
            // Arrange
            var aliveEvent = new AliveEvent("serverKey", true);
            string aliveEventJson = aliveEvent.ToString();
            mockMessageSender.Setup(m => m.SendJsonAsync(aliveEventJson, It.IsAny<string>(), It.IsAny<CancellationToken>()))
                             .Returns(Task.CompletedTask);
            // Act
            var executeTask = serverStatusSender.StartAsync(cancellationTokenSource.Token);
            await Task.Delay(3500);
            // Assert
            mockMessageSender.Verify(m => m.SendJsonAsync(It.IsAny<string>(), It.Is<string>(s => s.EndsWith("/serverinteraction/alive")), It.IsAny<CancellationToken>()), Times.AtLeast(3)); // At least 3 times
            cancellationTokenSource.Cancel();
            await executeTask;
        }
        [Test]
        public async Task ExecuteAsync_CancellationStopsPeriodicSending()
        {
            // Arrange
            var aliveEvent = new AliveEvent("serverKey", true);
            string aliveEventJson = aliveEvent.ToString();
            mockMessageSender.Setup(m => m.SendJsonAsync(aliveEventJson, It.IsAny<string>(), It.IsAny<CancellationToken>()))
                             .Returns(Task.CompletedTask);
            // Act
            var executeTask = serverStatusSender.StartAsync(cancellationTokenSource.Token);
            await Task.Delay(1500);
            // Act
            cancellationTokenSource.Cancel();
            await executeTask;
            // Assert
            mockMessageSender.Verify(m => m.SendJsonAsync(It.IsAny<string>(), It.Is<string>(s => s.EndsWith("/serverinteraction/alive")), It.IsAny<CancellationToken>()), Times.AtLeast(1));
            mockMessageSender.Verify(m => m.SendJsonAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.AtMost(2)); // Ensure it stops
        }
    }
}
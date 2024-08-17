using Microsoft.Extensions.Logging;
using Moq;
using ServerPulse.Client;
using ServerPulse.Client.Services;
using ServerPulse.EventCommunication.Events;

namespace ServerPulse.ClientTests.Services
{
    internal class ServerStatusSenderTests
    {
        private Mock<IMessageSender> mockMessageSender;
        private ServerStatusSender serverStatusSender;
        private ServerPulseSettings configuration;
        private CancellationTokenSource cancellationTokenSource;
        private Mock<ILogger<ServerLoadSender>> mockLogger;

        [SetUp]
        public void Setup()
        {
            mockMessageSender = new Mock<IMessageSender>();
            mockLogger = new Mock<ILogger<ServerLoadSender>>();
            configuration = new ServerPulseSettings()
            {
                Key = "example",
                EventController = "http://localhost",
                MaxEventSendingAmount = 10,
                ServerKeepAliveInterval = 1
            };
            serverStatusSender = new ServerStatusSender(mockMessageSender.Object, configuration, mockLogger.Object);
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
            var confEvent = new ConfigurationEvent(configuration.Key, TimeSpan.FromSeconds(configuration.ServerKeepAliveInterval));
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
        public async Task ExecuteAsync_SendsPulseEventPeriodically()
        {
            // Arrange
            var ev = new PulseEvent(configuration.Key, true);
            string evJson = ev.ToString();
            mockMessageSender.Setup(m => m.SendJsonAsync(evJson, It.IsAny<string>(), It.IsAny<CancellationToken>()))
                             .Returns(Task.CompletedTask);
            // Act
            var executeTask = serverStatusSender.StartAsync(cancellationTokenSource.Token);
            await Task.Delay(3500);
            // Assert
            mockMessageSender.Verify(m => m.SendJsonAsync(It.IsAny<string>(), It.Is<string>(s => s.EndsWith("/serverinteraction/pulse")), It.IsAny<CancellationToken>()), Times.AtLeast(3)); // At least 3 times
            cancellationTokenSource.Cancel();
            await executeTask;
        }
        [Test]
        public async Task ExecuteAsync_CancellationStopsPeriodicSending()
        {
            // Arrange
            var ev = new PulseEvent(configuration.Key, true);
            string evJson = ev.ToString();
            mockMessageSender.Setup(m => m.SendJsonAsync(evJson, It.IsAny<string>(), It.IsAny<CancellationToken>()))
                             .Returns(Task.CompletedTask);
            // Act
            var executeTask = serverStatusSender.StartAsync(cancellationTokenSource.Token);
            await Task.Delay(1500);
            // Act
            cancellationTokenSource.Cancel();
            await executeTask;
            // Assert
            mockMessageSender.Verify(m => m.SendJsonAsync(It.IsAny<string>(), It.Is<string>(s => s.EndsWith("/serverinteraction/pulse")), It.IsAny<CancellationToken>()), Times.AtLeast(1));
            mockMessageSender.Verify(m => m.SendJsonAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.AtMost(2)); // Ensure it stops
        }
        [Test]
        public async Task ExecuteAsync_LogsErrorOnInitialConfigurationEventFailure()
        {
            // Arrange
            mockMessageSender.Setup(m => m.SendJsonAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                             .ThrowsAsync(new Exception("Test exception"));
            // Act
            var executeTask = serverStatusSender.StartAsync(cancellationTokenSource.Token);
            await Task.Delay(500);
            // Assert
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("An error occurred while sending load events.")),
                    It.Is<Exception>(ex => ex.Message == "Test exception"),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
            cancellationTokenSource.Cancel();
            await executeTask;
        }
        [Test]
        public async Task ExecuteAsync_LogsErrorOnPulseEventFailure()
        {
            // Arrange
            mockMessageSender.Setup(m => m.SendJsonAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                             .ThrowsAsync(new Exception("Test exception"));
            // Act
            var executeTask = serverStatusSender.StartAsync(cancellationTokenSource.Token);
            await Task.Delay(500);
            // Assert
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("An error occurred while sending load events.")),
                    It.Is<Exception>(ex => ex.Message == "Test exception"),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.AtLeastOnce);
            cancellationTokenSource.Cancel();
            await executeTask;
        }
    }
}
using Microsoft.Extensions.Logging;
using Moq;
using ServerPulse.Client;
using ServerPulse.Client.Services;
using ServerPulse.EventCommunication.Events;

namespace ServerPulse.ClientTests.Services.Tests
{
    internal class ServerStatusSenderTests
    {
        private Mock<IMessageSender> mockMessageSender;
        private ServerStatusSender serverStatusSender;
        private EventSendingSettings<PulseEvent> pulseSettings;
        private EventSendingSettings<ConfigurationEvent> configurationSettings;
        private CancellationTokenSource cancellationTokenSource;
        private Mock<ILogger<ServerStatusSender>> mockLogger;

        [SetUp]
        public void Setup()
        {
            mockMessageSender = new Mock<IMessageSender>();
            mockLogger = new Mock<ILogger<ServerStatusSender>>();
            pulseSettings = new EventSendingSettings<PulseEvent>()
            {
                Key = "example",
                EventController = "http://localhost",
                MaxEventSendingAmount = 10,
                EventSendingInterval = 1
            };
            configurationSettings = new EventSendingSettings<ConfigurationEvent>()
            {
                Key = "example",
                EventController = "http://localhost",
                MaxEventSendingAmount = 10,
                EventSendingInterval = 1
            };
            serverStatusSender = new ServerStatusSender(mockMessageSender.Object, pulseSettings, configurationSettings, mockLogger.Object);
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
            var confEvent = new ConfigurationEvent(pulseSettings.Key, TimeSpan.FromSeconds(pulseSettings.EventSendingInterval));
            string confEventJson = confEvent.ToString();
            mockMessageSender.Setup(m => m.SendJsonAsync(confEventJson, It.IsAny<string>(), It.IsAny<CancellationToken>()))
                             .Returns(Task.CompletedTask);
            // Act
            var executeTask = serverStatusSender.StartAsync(cancellationTokenSource.Token);
            await Task.Delay(500);
            // Assert
            mockMessageSender.Verify(m => m.SendJsonAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
            cancellationTokenSource.Cancel();
            await executeTask;
        }
        [Test]
        public async Task ExecuteAsync_SendsPulseEventPeriodically()
        {
            // Arrange
            var ev = new PulseEvent(pulseSettings.Key, true);
            string evJson = ev.ToString();
            mockMessageSender.Setup(m => m.SendJsonAsync(evJson, It.IsAny<string>(), It.IsAny<CancellationToken>()))
                             .Returns(Task.CompletedTask);
            // Act
            var executeTask = serverStatusSender.StartAsync(cancellationTokenSource.Token);
            await Task.Delay(3500);
            // Assert
            mockMessageSender.Verify(m => m.SendJsonAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.AtLeast(3));
            cancellationTokenSource.Cancel();
            await executeTask;
        }
        [Test]
        public async Task ExecuteAsync_CancellationStopsPeriodicSending()
        {
            // Arrange
            var ev = new PulseEvent(pulseSettings.Key, true);
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
            mockMessageSender.Verify(m => m.SendJsonAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.AtLeast(1));
            mockMessageSender.Verify(m => m.SendJsonAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.AtMost(2));
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
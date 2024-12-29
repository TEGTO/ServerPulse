using EventCommunication;
using Microsoft.Extensions.Logging;
using Moq;
using ServerPulse.Client;
using ServerPulse.Client.Services;

namespace ServerPulse.ClientTests.Services.Tests
{
    internal class ServerStatusSenderTests
    {
        private Mock<IMessageSender> mockMessageSender;
        private ServerStatusSender serverStatusSender;
        private SendingSettings<PulseEvent> pulseSettings;
        private SendingSettings<ConfigurationEvent> configurationSettings;
        private CancellationTokenSource cancellationTokenSource;
        private Mock<ILogger<ServerStatusSender>> mockLogger;

        [SetUp]
        public void Setup()
        {
            mockMessageSender = new Mock<IMessageSender>();
            mockLogger = new Mock<ILogger<ServerStatusSender>>();

            pulseSettings = new SendingSettings<PulseEvent>()
            {
                Key = "example",
                SendingEndpoint = "http://localhost/pulse",
                MaxMessageSendingAmount = 10,
                SendingInterval = 1
            };
            configurationSettings = new SendingSettings<ConfigurationEvent>()
            {
                Key = "example",
                SendingEndpoint = "http://localhost/configuration",
                MaxMessageSendingAmount = 10,
                SendingInterval = 1
            };
            cancellationTokenSource = new CancellationTokenSource();

            serverStatusSender = new ServerStatusSender(mockMessageSender.Object, pulseSettings, configurationSettings, mockLogger.Object);
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
            mockMessageSender.Setup(m => m.SendJsonAsync(It.IsAny<string>(), configurationSettings.SendingEndpoint, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            var executeTask = serverStatusSender.StartAsync(cancellationTokenSource.Token);

            await Task.Delay(1500);

            // Assert
            mockMessageSender.Verify(m => m.SendJsonAsync(It.IsAny<string>(), configurationSettings.SendingEndpoint, It.IsAny<CancellationToken>()), Times.Once);

            await cancellationTokenSource.CancelAsync();

            await executeTask;
        }

        [Test]
        public async Task ExecuteAsync_SendsPulseEventPeriodically()
        {
            // Arrange
            mockMessageSender.Setup(m => m.SendJsonAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            var executeTask = serverStatusSender.StartAsync(cancellationTokenSource.Token);

            await Task.Delay(3500);

            // Assert
            mockMessageSender.Verify(m => m.SendJsonAsync(It.IsAny<string>(), pulseSettings.SendingEndpoint, It.IsAny<CancellationToken>()), Times.AtLeast(3));

            await cancellationTokenSource.CancelAsync();

            await executeTask;
        }

        [Test]
        public async Task ExecuteAsync_CancellationStopsPeriodicSending()
        {
            // Arrange
            mockMessageSender.Setup(m => m.SendJsonAsync(It.IsAny<string>(), pulseSettings.SendingEndpoint, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            var executeTask = serverStatusSender.StartAsync(cancellationTokenSource.Token);

            await Task.Delay(1500);

            // Act
            await cancellationTokenSource.CancelAsync();

            await executeTask;

            // Assert
            mockMessageSender.Verify(m => m.SendJsonAsync(It.IsAny<string>(), configurationSettings.SendingEndpoint, It.IsAny<CancellationToken>()), Times.AtLeast(1));
            mockMessageSender.Verify(m => m.SendJsonAsync(It.IsAny<string>(), pulseSettings.SendingEndpoint, It.IsAny<CancellationToken>()), Times.AtMost(2));
        }

        [Test]
        public async Task ExecuteAsync_LogsErrorOnInitialConfigurationEventFailure()
        {
            // Arrange
            mockMessageSender.Setup(m => m.SendJsonAsync(It.IsAny<string>(), configurationSettings.SendingEndpoint, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Test exception"));

            // Act
            var executeTask = serverStatusSender.StartAsync(cancellationTokenSource.Token);

            await Task.Delay(1500);

            // Assert
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("An error occurred while sending configuration event.")),
                    It.Is<Exception>(ex => ex.Message == "Test exception"),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);

            await cancellationTokenSource.CancelAsync();

            await executeTask;
        }

        [Test]
        public async Task ExecuteAsync_LogsErrorOnPulseEventFailure()
        {
            // Arrange
            mockMessageSender.Setup(m => m.SendJsonAsync(It.IsAny<string>(), pulseSettings.SendingEndpoint, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Test exception"));

            // Act
            var executeTask = serverStatusSender.StartAsync(cancellationTokenSource.Token);

            await Task.Delay(1500);

            // Assert
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("An error occurred while sending pulse event.")),
                    It.Is<Exception>(ex => ex.Message == "Test exception"),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.AtLeastOnce);

            await cancellationTokenSource.CancelAsync();

            await executeTask;
        }
    }
}
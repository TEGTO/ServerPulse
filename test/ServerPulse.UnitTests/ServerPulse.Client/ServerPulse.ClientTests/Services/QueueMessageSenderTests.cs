using Microsoft.Extensions.Logging;
using Moq;
using ServerPulse.Client;
using ServerPulse.Client.Services;
using ServerPulse.EventCommunication.Events;

namespace ServerPulse.ClientTests.Services.Tests
{
    public record class TestEvent(string Key) : BaseEvent(Key);

    [TestFixture]
    internal class QueueMessageSenderTests
    {
        private Mock<IMessageSender> mockMessageSender;
        private QueueMessageSender<TestEvent> queueMessageSender;
        private SendingSettings<TestEvent> configuration;
        private CancellationTokenSource cancellationTokenSource;
        private Mock<ILogger<QueueMessageSender<TestEvent>>> mockLogger;

        [SetUp]
        public void Setup()
        {
            mockMessageSender = new Mock<IMessageSender>();
            mockLogger = new Mock<ILogger<QueueMessageSender<TestEvent>>>();
            configuration = new SendingSettings<TestEvent>
            {
                Key = "example",
                SendingEndpoint = "http://localhost",
                MaxMessageSendingAmount = 10,
                SendingInterval = 1
            };
            queueMessageSender = new QueueMessageSender<TestEvent>(mockMessageSender.Object, configuration, mockLogger.Object);
            cancellationTokenSource = new CancellationTokenSource();
        }
        [TearDown]
        public void TearDown()
        {
            queueMessageSender?.Dispose();
            cancellationTokenSource?.Dispose();
        }

        [Test]
        public async Task ExecuteAsync_SendsEventsPeriodically()
        {
            // Arrange
            var testEvent = new TestEvent("key1");
            queueMessageSender.SendMessage(testEvent);
            // Act
            var executeTask = queueMessageSender.StartAsync(cancellationTokenSource.Token);
            await Task.Delay(1500);
            cancellationTokenSource.Cancel();
            await executeTask;
            // Assert
            mockMessageSender.Verify(m => m.SendJsonAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.AtLeastOnce);
        }
        [Test]
        public async Task ExecuteAsync_SendsCorrectNumberOfEvents()
        {
            // Arrange
            for (int i = 0; i < 15; i++)
            {
                var testEvent = new TestEvent($"key{i}");
                queueMessageSender.SendMessage(testEvent);
            }
            // Act
            var executeTask = queueMessageSender.StartAsync(cancellationTokenSource.Token);
            await Task.Delay(1500);
            cancellationTokenSource.Cancel();
            await executeTask;
            // Assert
            mockMessageSender.Verify(m => m.SendJsonAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.AtLeastOnce);
            mockMessageSender.Verify(m => m.SendJsonAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.AtMost(2));
        }
        [Test]
        public async Task ExecuteAsync_LogsErrorOnSendEventFailure()
        {
            // Arrange
            var testEvent = new TestEvent("key1");
            queueMessageSender.SendMessage(testEvent);
            mockMessageSender.Setup(m => m.SendJsonAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                             .ThrowsAsync(new Exception("Test exception"));
            // Act
            var executeTask = queueMessageSender.StartAsync(cancellationTokenSource.Token);
            await Task.Delay(1500);
            // Assert
            mockLogger.Verify(
               x => x.Log(
                   LogLevel.Error,
                   It.IsAny<EventId>(),
                   It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("An error occurred while sending TestEvent events.")),
                   It.Is<Exception>(ex => ex.Message == "Test exception"),
                   It.IsAny<Func<It.IsAnyType, Exception, string>>()),
               Times.AtLeastOnce);
            cancellationTokenSource.Cancel();
            await executeTask;
        }
    }

}

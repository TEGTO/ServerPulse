using Microsoft.Extensions.Logging;
using Moq;
using ServerPulse.Client;
using ServerPulse.Client.Services;
using ServerPulse.EventCommunication;
using ServerPulse.EventCommunication.Events;
using System.Text.Json;

namespace ServerPulse.ClientTests.Services.Tests
{
    [TestFixture]
    internal class CustomEventSenderTests
    {
        private Mock<IMessageSender> mockMessageSender;
        private QueueMessageSender<CustomEventWrapper> customEventSender;
        private SendingSettings<CustomEventWrapper> configuration;
        private CancellationTokenSource cancellationTokenSource;
        private Mock<ILogger<QueueMessageSender<CustomEventWrapper>>> mockLogger;

        [SetUp]
        public void Setup()
        {
            mockMessageSender = new Mock<IMessageSender>();
            mockLogger = new Mock<ILogger<QueueMessageSender<CustomEventWrapper>>>();
            configuration = new SendingSettings<CustomEventWrapper>
            {
                Key = "customKey",
                SendingEndpoint = "http://localhost/custom",
                MaxMessageSendingAmount = 5,
                SendingInterval = 1
            };
            customEventSender = new QueueMessageSender<CustomEventWrapper>(mockMessageSender.Object, configuration, mockLogger.Object);
            cancellationTokenSource = new CancellationTokenSource();
        }
        [TearDown]
        public void TearDown()
        {
            customEventSender?.Dispose();
            cancellationTokenSource?.Dispose();
        }

        [Test]
        public async Task ExecuteAsync_SendsCustomEventsPeriodically()
        {
            // Arrange
            var customEvent = new CustomEvent("key1", "EventName", "EventDescription");
            customEventSender.SendMessage(new CustomEventWrapper(customEvent, JsonSerializer.Serialize(customEvent)));
            // Act
            var executeTask = customEventSender.StartAsync(cancellationTokenSource.Token);
            await Task.Delay(1500);
            cancellationTokenSource.Cancel();
            await executeTask;
            // Assert
            mockMessageSender.Verify(m => m.SendJsonAsync(It.IsAny<string>(), configuration.SendingEndpoint, It.IsAny<CancellationToken>()), Times.AtLeastOnce);
        }
        [Test]
        public async Task ExecuteAsync_SendsCorrectNumberOfCustomEvents()
        {
            // Arrange
            for (int i = 0; i < 10; i++)
            {
                var customEvent = new CustomEvent($"key{i}", $"EventName{i}", $"EventDescription{i}");
                customEventSender.SendMessage(new CustomEventWrapper(customEvent, JsonSerializer.Serialize(customEvent)));
            }
            // Act
            var executeTask = customEventSender.StartAsync(cancellationTokenSource.Token);
            await Task.Delay(1500);
            cancellationTokenSource.Cancel();
            await executeTask;
            // Assert
            mockMessageSender.Verify(m => m.SendJsonAsync(It.IsAny<string>(), configuration.SendingEndpoint, It.IsAny<CancellationToken>()), Times.AtLeastOnce);
            mockMessageSender.Verify(m => m.SendJsonAsync(It.IsAny<string>(), configuration.SendingEndpoint, It.IsAny<CancellationToken>()), Times.AtMost(2));
        }
        [Test]
        public void GetEventsJson_ReturnsSerializedCustomEventWrappers()
        {
            // Arrange
            for (int i = 0; i < 3; i++)
            {
                var customEvent = new CustomEvent($"key{i}", $"EventName{i}", $"EventDescription{i}");
                customEventSender.SendMessage(new CustomEventWrapper(customEvent, JsonSerializer.Serialize(customEvent)));
            }
            // Act
            var resultJson = customEventSender.GetType().GetMethod("GetEventsJson", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).Invoke(customEventSender, null) as string;
            // Assert
            Assert.IsNotNull(resultJson);
            var deserialized = JsonSerializer.Deserialize<List<CustomEventWrapper>>(resultJson);
            Assert.AreEqual(3, deserialized.Count);
        }

        [Test]
        public async Task ExecuteAsync_LogsErrorOnCustomEventSendFailure()
        {
            // Arrange
            var customEvent = new CustomEvent("key1", "EventName", "EventDescription");
            customEventSender.SendMessage(new CustomEventWrapper(customEvent, JsonSerializer.Serialize(customEvent)));
            mockMessageSender.Setup(m => m.SendJsonAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                             .ThrowsAsync(new Exception("Test exception"));
            // Act
            var executeTask = customEventSender.StartAsync(cancellationTokenSource.Token);
            await Task.Delay(1500);
            // Assert
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("An error occurred while sending CustomEventWrapper events.")),
                    It.Is<Exception>(ex => ex.Message == "Test exception"),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.AtLeastOnce);
            cancellationTokenSource.Cancel();
            await executeTask;
        }
    }
}
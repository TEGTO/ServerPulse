using EventCommunication;
using Microsoft.Extensions.Logging;
using Moq;
using ServerPulse.Client;
using ServerPulse.Client.Services;
using System.Reflection;
using System.Text.Json;

namespace ServerPulse.ClientTests.Services.Tests
{
    [TestFixture]
    internal class CustomEventSenderTests
    {
        private Mock<IMessageSender> mockMessageSender;
        private QueueMessageSender<CustomEventContainer> customEventContainerSender;
        private SendingSettings<CustomEventContainer> configuration;
        private CancellationTokenSource cancellationTokenSource;
        private Mock<ILogger<QueueMessageSender<CustomEventContainer>>> mockLogger;

        [SetUp]
        public void Setup()
        {
            mockMessageSender = new Mock<IMessageSender>();
            mockLogger = new Mock<ILogger<QueueMessageSender<CustomEventContainer>>>();

            configuration = new SendingSettings<CustomEventContainer>
            {
                Key = "customKey",
                SendingEndpoint = "http://localhost/custom",
                MaxMessageSendingAmount = 5,
                SendingInterval = 1
            };
            cancellationTokenSource = new CancellationTokenSource();

            customEventContainerSender = new QueueMessageSender<CustomEventContainer>(mockMessageSender.Object, configuration, mockLogger.Object);
        }

        [TearDown]
        public void TearDown()
        {
            customEventContainerSender?.Dispose();
            cancellationTokenSource?.Dispose();
        }

        private static MethodInfo? GetPrivateMethod(string name, object instance)
        {
            return instance.GetType().GetMethod(name, BindingFlags.NonPublic | BindingFlags.Instance);
        }

        [Test]
        public async Task ExecuteAsync_SendsCustomEventsPeriodically()
        {
            // Arrange
            var customEvent = new CustomEvent("key1", "EventName", "EventDescription");

            customEventContainerSender.SendMessage(new CustomEventContainer(customEvent, JsonSerializer.Serialize(customEvent)));

            // Act
            var executeTask = customEventContainerSender.StartAsync(cancellationTokenSource.Token);
            await Task.Delay(1500);

            await cancellationTokenSource.CancelAsync();

            await executeTask;

            // Assert
            mockMessageSender.Verify(
                m => m.SendJsonAsync(It.IsAny<string>(), configuration.SendingEndpoint, It.IsAny<CancellationToken>()),
                Times.AtLeastOnce);
        }

        [Test]
        public async Task ExecuteAsync_SendsCorrectNumberOfCustomEvents()
        {
            // Arrange
            for (int i = 0; i < 10; i++)
            {
                var customEvent = new CustomEvent($"key{i}", $"EventName{i}", $"EventDescription{i}");
                customEventContainerSender.SendMessage(new CustomEventContainer(customEvent, JsonSerializer.Serialize(customEvent)));
            }

            // Act
            var executeTask = customEventContainerSender.StartAsync(cancellationTokenSource.Token);

            await Task.Delay(1500);

            await cancellationTokenSource.CancelAsync();

            await executeTask;

            // Assert
            mockMessageSender.Verify(
                m => m.SendJsonAsync(It.IsAny<string>(), configuration.SendingEndpoint, It.IsAny<CancellationToken>()),
                Times.AtLeastOnce);
            mockMessageSender.Verify(
                m => m.SendJsonAsync(It.IsAny<string>(), configuration.SendingEndpoint, It.IsAny<CancellationToken>()),
                Times.AtMost(2));
        }

        [Test]
        public void GetEventsJson_ReturnsSerializedCustomEventContainers()
        {
            // Arrange
            for (int i = 0; i < 3; i++)
            {
                var customEvent = new CustomEvent($"key{i}", $"EventName{i}", $"EventDescription{i}");
                customEventContainerSender.SendMessage(new CustomEventContainer(customEvent, JsonSerializer.Serialize(customEvent)));
            }

            // Act
            var method = GetPrivateMethod("GetEventsJson", customEventContainerSender);

            Assert.IsNotNull(method);

            var resultJson = method.Invoke(customEventContainerSender, null) as string;

            // Assert
            Assert.IsNotNull(resultJson);

            var deserialized = JsonSerializer.Deserialize<List<CustomEventContainer>>(resultJson);

            Assert.IsNotNull(deserialized);

            Assert.That(deserialized.Count, Is.EqualTo(3));
        }

        [Test]
        public async Task ExecuteAsync_LogsErrorOnCustomEventSendFailure()
        {
            // Arrange
            var customEvent = new CustomEvent("key1", "EventName", "EventDescription");

            customEventContainerSender.SendMessage(new CustomEventContainer(customEvent, JsonSerializer.Serialize(customEvent)));

            mockMessageSender.Setup(m => m.SendJsonAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Test exception"));

            // Act
            var executeTask = customEventContainerSender.StartAsync(cancellationTokenSource.Token);
            await Task.Delay(1500);

            // Assert
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("An error occurred while sending CustomEventContainer events.")),
                    It.Is<Exception>(ex => ex.Message == "Test exception"),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.AtLeastOnce);

            await cancellationTokenSource.CancelAsync();

            await executeTask;
        }
    }
}
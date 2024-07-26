using Moq;
using ServerPulse.Client;
using ServerPulse.Client.Services;
using ServerPulse.EventCommunication.Events;

namespace ServerPulse.ClientTests.Services
{
    [TestFixture]
    internal class ServerLoadSenderTests
    {
        private Mock<IMessageSender> mockMessageSender;
        private ServerLoadSender serverLoadSender;
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
                EventSendingInterval = 1
            };
            serverLoadSender = new ServerLoadSender(mockMessageSender.Object, configuration);
            cancellationTokenSource = new CancellationTokenSource();
        }
        [TearDown]
        public void TearDown()
        {
            serverLoadSender?.Dispose();
            cancellationTokenSource?.Dispose();
        }

        [Test]
        public async Task ExecuteAsync_SendsEventsPeriodically()
        {
            // Arrange
            var loadEvent = new LoadEvent("key1", "endpoint", "GET", 200, TimeSpan.FromSeconds(1), DateTime.UtcNow);
            serverLoadSender.SendEvent(loadEvent);
            // Act
            var executeTask = serverLoadSender.StartAsync(cancellationTokenSource.Token);
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
                var loadEvent = new LoadEvent($"key{i}", "endpoint", "GET", 200, TimeSpan.FromSeconds(1), DateTime.UtcNow);
                serverLoadSender.SendEvent(loadEvent);
            }
            // Act
            var executeTask = serverLoadSender.StartAsync(cancellationTokenSource.Token);
            await Task.Delay(1500);
            cancellationTokenSource.Cancel();
            await executeTask;
            // Assert
            mockMessageSender.Verify(m => m.SendJsonAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.AtLeast(1));
            mockMessageSender.Verify(m => m.SendJsonAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.AtMost(2));
        }
    }
}
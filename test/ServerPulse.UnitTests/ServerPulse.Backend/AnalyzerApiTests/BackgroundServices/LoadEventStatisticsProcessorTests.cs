using AnalyzerApi.Command.BackgroundServices.ProcessLoadEvents;
using AnalyzerApi.Infrastructure;
using Confluent.Kafka;
using EventCommunication;
using MediatR;
using MessageBus.Interfaces;
using Microsoft.Extensions.Configuration;
using Moq;
using System.Text.Json;

namespace AnalyzerApi.BackgroundServices.Tests
{
    [TestFixture]
    internal class LoadEventStatisticsProcessorTests
    {
        private Mock<IMessageConsumer> mockMessageConsumer;
        private Mock<IMediator> mockMediator;
        private Mock<ITopicManager> mockTopicManager;
        private Mock<IConfiguration> mockConfiguration;
        private LoadEventStatisticsProcessor processor;

        private const string KafkaTopic = "load-event-topic";
        private const int TimeoutInMilliseconds = 5000;

        [SetUp]
        public void SetUp()
        {
            mockMessageConsumer = new Mock<IMessageConsumer>();
            mockMediator = new Mock<IMediator>();
            mockTopicManager = new Mock<ITopicManager>();
            mockConfiguration = new Mock<IConfiguration>();

            mockConfiguration.Setup(c => c[Configuration.KAFKA_LOAD_TOPIC_PROCESS]).Returns(KafkaTopic);
            mockConfiguration.Setup(c => c[Configuration.KAFKA_TIMEOUT_IN_MILLISECONDS]).Returns(TimeoutInMilliseconds.ToString());

            processor = new LoadEventStatisticsProcessor(
                mockMessageConsumer.Object,
                mockMediator.Object,
                mockTopicManager.Object,
                mockConfiguration.Object);
        }

        [TearDown]
        public void TearDown()
        {
            processor.Dispose();
        }

        [Test]
        public async Task ExecuteAsync_CreatesTopicOnStart()
        {
            // Arrange
            var loadEvent = new LoadEvent("testKey", "/api/test", "GET", 200, TimeSpan.FromMilliseconds(100), DateTime.UtcNow);
            var consumeResponse = new ConsumeResponse(JsonSerializer.Serialize(loadEvent), DateTime.UtcNow);

            mockMessageConsumer.Setup(c => c.ConsumeAsync(KafkaTopic, TimeoutInMilliseconds, Offset.Stored, It.IsAny<CancellationToken>()))
                .Returns(MockAsyncEnumerable([consumeResponse]));

            // Act
            await processor.StartAsync(CancellationToken.None);

            // Assert
            mockTopicManager.Verify(t => t.CreateTopicsAsync(
                It.Is<IEnumerable<string>>(topics => topics.Contains(KafkaTopic)),
                It.IsAny<int>(),
                It.IsAny<short>(),
                TimeoutInMilliseconds), Times.Once);
        }

        [Test]
        public async Task ExecuteAsync_ProcessesValidLoadEvents()
        {
            // Arrange
            var loadEvent = new LoadEvent("testKey", "/api/test", "GET", 200, TimeSpan.FromMilliseconds(100), DateTime.UtcNow);
            var consumeResponse = new ConsumeResponse(JsonSerializer.Serialize(loadEvent), DateTime.UtcNow);

            mockMessageConsumer.Setup(c => c.ConsumeAsync(KafkaTopic, TimeoutInMilliseconds, Offset.Stored, It.IsAny<CancellationToken>()))
                .Returns(MockAsyncEnumerable([consumeResponse]));

            // Act
            var stoppingToken = new CancellationTokenSource(TimeSpan.FromMilliseconds(500)).Token;
            await processor.StartAsync(stoppingToken);

            // Assert
            mockMediator.Verify(m => m.Send(
                It.Is<ProcessLoadEventsCommand>(cmd => cmd.Events.Length == 1 && cmd.Events[0].Key == "testKey"),
                It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Test]
        public async Task ExecuteAsync_IgnoresInvalidLoadEvents()
        {
            // Arrange
            var invalidResponse = new ConsumeResponse("Invalid JSON", DateTime.UtcNow);

            mockMessageConsumer.Setup(c => c.ConsumeAsync(KafkaTopic, TimeoutInMilliseconds, Offset.Stored, It.IsAny<CancellationToken>()))
                .Returns(MockAsyncEnumerable(new[] { invalidResponse }));

            // Act
            var stoppingToken = new CancellationTokenSource(TimeSpan.FromMilliseconds(500)).Token;
            await processor.StartAsync(stoppingToken);

            // Assert
            mockMediator.Verify(m => m.Send(It.IsAny<ProcessLoadEventsCommand>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public async Task ExecuteAsync_HandlesEmptyMessagesGracefully()
        {
            // Arrange
            mockMessageConsumer.Setup(c => c.ConsumeAsync(KafkaTopic, TimeoutInMilliseconds, Offset.Stored, It.IsAny<CancellationToken>()))
                .Returns(MockAsyncEnumerable(Array.Empty<ConsumeResponse>()));

            // Act
            var stoppingToken = new CancellationTokenSource(TimeSpan.FromMilliseconds(500)).Token;
            await processor.StartAsync(stoppingToken);

            // Assert
            mockMediator.Verify(m => m.Send(It.IsAny<ProcessLoadEventsCommand>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        private static async IAsyncEnumerable<ConsumeResponse> MockAsyncEnumerable(IEnumerable<ConsumeResponse> responses)
        {
            foreach (var response in responses)
            {
                yield return response;
                await Task.Yield();
            }
        }
    }
}
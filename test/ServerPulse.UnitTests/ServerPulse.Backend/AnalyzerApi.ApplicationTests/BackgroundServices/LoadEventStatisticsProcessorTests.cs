﻿using AnalyzerApi.Application.Command.BackgroundServices.ProcessLoadEvents;
using AnalyzerApi.Application.Configuration;
using Confluent.Kafka;
using EventCommunication;
using MediatR;
using MessageBus.Interfaces;
using MessageBus.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Polly;
using Polly.Registry;
using Polly.Retry;
using System.Reflection;
using System.Text.Json;

namespace AnalyzerApi.Application.BackgroundServices.Tests
{
    [TestFixture]
    internal class LoadEventStatisticsProcessorTests
    {
        private Mock<IMessageConsumer> mockMessageConsumer;
        private Mock<IMediator> mockMediator;
        private Mock<ResiliencePipelineProvider<string>> mockPipelineProvider;
        private Mock<ILogger<LoadEventStatisticsProcessor>> mockLogger;
        private LoadEventStatisticsProcessor processor;

        private const string LoadTopicProcess = "load-event-topic";
        private const int TimeoutInMilliseconds = 5000;
        private const int BatchSize = 3;
        private const int BatchIntervalMilliseconds = 1000;

        [SetUp]
        public void SetUp()
        {
            var resiliencePipeline = new ResiliencePipelineBuilder()
            .AddRetry(new RetryStrategyOptions
            {
                MaxRetryAttempts = 2,
                Delay = TimeSpan.FromMilliseconds(500),
            })
            .Build();

            mockMessageConsumer = new Mock<IMessageConsumer>();
            mockMediator = new Mock<IMediator>();
            mockLogger = new Mock<ILogger<LoadEventStatisticsProcessor>>();
            mockPipelineProvider = new Mock<ResiliencePipelineProvider<string>>();
            mockPipelineProvider.Setup(x => x.GetPipeline(It.IsAny<string>()))
                .Returns(resiliencePipeline);

            var messageBusSettings = new MessageBusSettings()
            {
                LoadTopicProcess = LoadTopicProcess,
                ReceiveTimeoutInMilliseconds = TimeoutInMilliseconds,
            };

            var messageBusOptions = new Mock<IOptions<MessageBusSettings>>();
            messageBusOptions.Setup(x => x.Value).Returns(messageBusSettings);

            var processSettings = new LoadProcessingSettings()
            {
                BatchSize = BatchSize,
                BatchIntervalInMilliseconds = BatchIntervalMilliseconds,
            };

            var processOptions = new Mock<IOptions<LoadProcessingSettings>>();
            processOptions.Setup(x => x.Value).Returns(processSettings);

            processor = new LoadEventStatisticsProcessor(
                mockMessageConsumer.Object,
                mockMediator.Object,
                messageBusOptions.Object,
                processOptions.Object,
                mockPipelineProvider.Object,
                mockLogger.Object);
        }

        [TearDown]
        public void TearDown()
        {
            processor.Dispose();
        }

        [Test]
        public async Task ProcessBatchAsync_ThrowsError_ResilienceRetries()
        {
            // Arrange
            mockMediator.Setup(m => m.Send(It.IsAny<ProcessLoadEventsCommand>(), It.IsAny<CancellationToken>())).ThrowsAsync(new Exception("Some exception."));

            // Act
            MethodInfo methodInfo = processor.GetType().GetMethod("ProcessBatchAsync", BindingFlags.NonPublic | BindingFlags.Instance)!;
            var batch = new List<LoadEvent>(new ProcessLoadEventsCommand(Array.Empty<LoadEvent>()).Events);
            var result = methodInfo.Invoke(processor, [batch, CancellationToken.None]);

            await (Task)result!;

            // Assert
            mockMediator.Verify(m => m.Send(It.IsAny<ProcessLoadEventsCommand>(), It.IsAny<CancellationToken>()), Times.AtLeast(2));

            mockLogger.Verify(x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.AtLeast(1));
        }

        private static IEnumerable<TestCaseData> ConsumeValidMessagesTestCases()
        {
            yield return new TestCaseData(
                new List<ConsumeResponse>
                {
                    new ConsumeResponse("{\"Key\":\"testKey1\",\"Endpoint\":\"/api/test\",\"Method\":\"GET\",\"TimestampUTC\":\"2024-01-01T00:00:00Z\"}", DateTime.UtcNow),
                    new ConsumeResponse("{\"Key\":\"testKey2\",\"Endpoint\":\"/api/test2\",\"Method\":\"POST\",\"TimestampUTC\":\"2024-01-01T00:01:00Z\"}", DateTime.UtcNow)
                },
                2,
                1
            ).SetDescription("Valid load events should be processed.");

            yield return new TestCaseData(
                new List<ConsumeResponse>
                {
                    new ConsumeResponse("{\"Key\":\"testKey1\",\"Endpoint\":\"/api/test\",\"Method\":\"GET\",\"TimestampUTC\":\"2024-01-01T00:00:00Z\"}", DateTime.UtcNow),
                    new ConsumeResponse("{\"Key\":\"testKey2\",\"Endpoint\":\"/api/test2\",\"Method\":\"POST\",\"TimestampUTC\":\"2024-01-01T00:01:00Z\"}", DateTime.UtcNow),
                    new ConsumeResponse("{\"Key\":\"testKey3\",\"Endpoint\":\"/api/test3\",\"Method\":\"POST\",\"TimestampUTC\":\"2024-01-01T00:01:00Z\"}", DateTime.UtcNow)
                },
                3,
                1
            ).SetDescription("Valid load events should be processed in consume loop, amount of events the same as bunch.");

            yield return new TestCaseData(
                new List<ConsumeResponse>
                {
                    new ConsumeResponse("{\"Key\":\"testKey1\",\"Endpoint\":\"/api/test\",\"Method\":\"GET\",\"TimestampUTC\":\"2024-01-01T00:00:00Z\"}", DateTime.UtcNow),
                    new ConsumeResponse("{\"Key\":\"testKey2\",\"Endpoint\":\"/api/test2\",\"Method\":\"POST\",\"TimestampUTC\":\"2024-01-01T00:01:00Z\"}", DateTime.UtcNow),
                    new ConsumeResponse("{\"Key\":\"testKey3\",\"Endpoint\":\"/api/test3\",\"Method\":\"POST\",\"TimestampUTC\":\"2024-01-01T00:01:00Z\"}", DateTime.UtcNow),


                    new ConsumeResponse("{\"Key\":\"testKey1\",\"Endpoint\":\"/api/test\",\"Method\":\"GET\",\"TimestampUTC\":\"2024-01-01T00:00:00Z\"}", DateTime.UtcNow),
                    new ConsumeResponse("{\"Key\":\"testKey2\",\"Endpoint\":\"/api/test2\",\"Method\":\"POST\",\"TimestampUTC\":\"2024-01-01T00:01:00Z\"}", DateTime.UtcNow),
                    new ConsumeResponse("{\"Key\":\"testKey3\",\"Endpoint\":\"/api/test3\",\"Method\":\"POST\",\"TimestampUTC\":\"2024-01-01T00:01:00Z\"}", DateTime.UtcNow)
                },
                3,
                2
            ).SetDescription("Valid load events should be processed a few times, amount of events greater than bunch size.");

            yield return new TestCaseData(
                new List<ConsumeResponse>(),
                0,
                0
            ).SetDescription("No events should result in zero processing.");
        }

        [Test]
        [TestCaseSource(nameof(ConsumeValidMessagesTestCases))]
        public async Task ExecuteAsync_ProcessesValidLoadEvents(List<ConsumeResponse> messages, int expectedBatchCount, int expectedCallCount)
        {
            // Arrange
            mockMessageConsumer.Setup(c => c.ConsumeAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<Offset>(), It.IsAny<CancellationToken>()))
                .Returns(MockAsyncEnumerable(messages));

            // Act
            var stoppingToken = new CancellationTokenSource(TimeSpan.FromMilliseconds(1100)).Token;
            await processor.StartAsync(stoppingToken);
            await Task.Delay(1100);

            // Assert
            mockMediator.Verify(m => m.Send(
                It.Is<ProcessLoadEventsCommand>(cmd => cmd.Events.Length == expectedBatchCount),
                It.IsAny<CancellationToken>()), Times.Exactly(expectedCallCount));
        }

        private static IEnumerable<TestCaseData> InvalidMessagesTestCases()
        {
            yield return new TestCaseData(
                new List<ConsumeResponse>
                {
                    new ConsumeResponse("Invalid JSON", DateTime.UtcNow),
                    new ConsumeResponse(JsonSerializer.Serialize(new TestEvent("someKey")), DateTime.UtcNow),
                    new ConsumeResponse("{\"Key\":\"\",\"Endpoint\":\"/api/test\",\"Method\":\"GET\"}", DateTime.UtcNow),
                    new ConsumeResponse("{\"Key\":\"testKey1\"}", DateTime.UtcNow)
                },
                "Invalid or empty events should not be processed."
            );
        }

        [Test]
        [TestCaseSource(nameof(InvalidMessagesTestCases))]
        public async Task ExecuteAsync_IgnoresInvalidMessages(List<ConsumeResponse> messages, string description)
        {
            // Arrange
            mockMessageConsumer.Setup(c => c.ConsumeAsync(LoadTopicProcess, TimeoutInMilliseconds, Offset.Stored, It.IsAny<CancellationToken>()))
                .Returns(MockAsyncEnumerable(messages));

            // Act
            var stoppingToken = new CancellationTokenSource(TimeSpan.FromMilliseconds(500)).Token;
            await processor.StartAsync(stoppingToken);
            await Task.Delay(1100);

            // Assert
            mockMediator.Verify(m => m.Send(It.IsAny<ProcessLoadEventsCommand>(), It.IsAny<CancellationToken>()), Times.Never);
            mockLogger.Verify(x => x.Log(
               LogLevel.Warning,
               It.IsAny<EventId>(),
               It.IsAny<It.IsAnyType>(),
               It.IsAny<Exception>(),
               It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
               Times.AtLeastOnce);
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

    public record TestEvent(string Key) : BaseEvent(Key);
}
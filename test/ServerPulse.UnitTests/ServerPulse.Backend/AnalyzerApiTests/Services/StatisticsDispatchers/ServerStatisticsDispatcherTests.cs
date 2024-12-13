using AnalyzerApi.Command.Builders;
using AnalyzerApi.Command.Senders;
using AnalyzerApi.Infrastructure.Models.Statistics;
using AnalyzerApi.Infrastructure.Models.Wrappers;
using AnalyzerApi.Services.Receivers.Event;
using AnalyzerApiTests;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using System.Collections.Concurrent;

namespace AnalyzerApi.Services.StatisticsDispatchers.Tests
{
    [TestFixture]
    internal class StatisticsDispatcherTests
    {
        private Mock<IEventReceiver<MockEventWrapper>> mockReceiver;
        private Mock<IMediator> mockMediator;
        private Mock<ILogger<StatisticsDispatcher<MockStatistics, MockEventWrapper>>> mockLogger;
        private StatisticsDispatcher<MockStatistics, MockEventWrapper> dispatcher;

        [SetUp]
        public void Setup()
        {
            mockReceiver = new Mock<IEventReceiver<MockEventWrapper>>();
            mockMediator = new Mock<IMediator>();
            mockLogger = new Mock<ILogger<StatisticsDispatcher<MockStatistics, MockEventWrapper>>>();

            dispatcher = new StatisticsDispatcher<MockStatistics, MockEventWrapper>(
                mockReceiver.Object,
                mockMediator.Object,
                mockLogger.Object
            );
        }

        [TearDown]
        public async Task TearDown()
        {
            await dispatcher.DisposeAsync();
        }

        private static async IAsyncEnumerable<MockEventWrapper> GenerateEvents(int count, int delay, string key)
        {
            for (int i = 0; i < count; i++)
            {
                yield return new MockEventWrapper { Key = key, Id = $"Event{i}", CreationDateUTC = DateTime.UtcNow };
                await Task.Delay(delay);
            }
        }

        [Test]
        [TestCase("key1", true, Description = "Key exists, initial statistics sent successfully.")]
        [TestCase("key2", false, Description = "Key does not exist, initial statistics not sent.")]
        public async Task DispatchInitialStatistics_ValidKey_HandlesProperly(string key, bool shouldSucceed)
        {
            // Arrange
            var mockStatistics = new MockStatistics();

            mockMediator.Setup(m => m.Send(It.IsAny<BuildStatisticsCommand<MockStatistics>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(shouldSucceed ? mockStatistics : null!);

            // Act
            await dispatcher.DispatchInitialStatisticsAsync(key);

            // Assert
            if (shouldSucceed)
            {
                mockMediator.Verify(m => m.Send(It.IsAny<SendStatisticsCommand<MockStatistics>>(), It.IsAny<CancellationToken>()), Times.Once);
            }
            else
            {
                mockMediator.Verify(m => m.Send(It.IsAny<SendStatisticsCommand<MockStatistics>>(), It.IsAny<CancellationToken>()), Times.Never);
            }
        }

        [Test]
        [TestCase(3, 200, Description = "Handles 3 events with 200 delay.")]
        [TestCase(30, 10, Description = "Handles 30 events with 10 delay.")]
        [TestCase(1, 300, Description = "Handles 1 event with 300 delay.")]
        [TestCase(300, 10, Description = "Handles 300 events with 10 delay.")]
        public async Task DispatchStatisticsAsync_HandlesEventsProperly(int eventCount, int eventDelay)
        {
            // Arrange
            var key = "testKey";
            var events = GenerateEvents(eventCount, eventDelay, key);
            var mockStatistics = new MockStatistics();

            mockReceiver.Setup(r => r.GetEventStreamAsync(key, It.IsAny<CancellationToken>())).Returns(events);
            mockMediator.Setup(m => m.Send(It.IsAny<BuildStatisticsCommand<MockStatistics>>(), It.IsAny<CancellationToken>())).ReturnsAsync(mockStatistics);

            // Act
            await dispatcher.StartStatisticsDispatchingAsync(key);

            var listeners = dispatcher.GetFieldValue<ConcurrentDictionary<string, CancellationTokenSource>>("listeners");
            Assert.IsNotNull(listeners);
            Assert.IsTrue(listeners.ContainsKey(key));
            Assert.IsTrue(listeners.Any());

            await Task.Delay(eventCount * eventDelay + 2000);
            await dispatcher.StopStatisticsDispatchingAsync(key);

            // Assert
            mockMediator.Verify(m => m.Send(It.IsAny<BuildStatisticsCommand<MockStatistics>>(), It.IsAny<CancellationToken>()), Times.Exactly(eventCount));
            mockMediator.Verify(m => m.Send(It.IsAny<SendStatisticsCommand<MockStatistics>>(), It.IsAny<CancellationToken>()), Times.Exactly(eventCount));
        }

        [Test]
        [TestCase("key1", 5, 3, 400, 400, Description = "Dispatches at most 3 of 5 events before stopping.")]
        [TestCase("key1", 10, 9, 100, 400, Description = "Dispatches at most 9 of 10 events before stopping.")]
        [TestCase("key2", 50, 6, 100, 100, Description = "Dispatches at most 6 of 50 events before stopping.")]
        [TestCase("key3", 500, 60, 1000, 1000, Description = "Dispatches at most 60 of 500 events before stopping.")]
        public async Task StopStatisticsDispatching_StopsCorrectly(string key, int eventCount, int proceededEventCount, int delay, int stopDelay)
        {
            // Arrange
            var events = GenerateEvents(eventCount, delay, key);
            var mockStatistics = new MockStatistics();

            mockReceiver.Setup(r => r.GetEventStreamAsync(key, It.IsAny<CancellationToken>())).Returns(events);
            mockMediator.Setup(m => m.Send(It.IsAny<BuildStatisticsCommand<MockStatistics>>(), It.IsAny<CancellationToken>())).ReturnsAsync(mockStatistics);

            // Act
            await dispatcher.StartStatisticsDispatchingAsync(key);
            await Task.Delay(stopDelay);
            await dispatcher.StopStatisticsDispatchingAsync(key);
            await Task.Delay(500);

            // Assert
            mockMediator.Verify(m => m.Send(It.IsAny<SendStatisticsCommand<MockStatistics>>(), It.IsAny<CancellationToken>()), Times.AtMost(proceededEventCount));

            mockLogger.Verify(
               log => log.Log(
                   LogLevel.Information,
                   It.IsAny<EventId>(),
                   It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Stopped listening for key '{key}'")),
                   null,
                   It.IsAny<Func<It.IsAnyType, Exception?, string>>()
               ),
               Times.Once
           );

            var listeners = dispatcher.GetFieldValue<ConcurrentDictionary<string, CancellationTokenSource>>("listeners");
            Assert.IsNotNull(listeners);
            Assert.IsFalse(listeners.ContainsKey(key));
            Assert.IsFalse(listeners.Any());
        }

        [Test]
        [TestCase(100, 5, Description = "Thread-safe dispatching with 5 keys and 100 clients.")]
        [TestCase(1000, 50, Description = "Thread-safe dispatching with 50 keys and 1000 clients.")]
        //[TestCase(50, 1000, Description = "Thread-safe dispatching with 1000 keys and 50 clients.")]
        [TestCase(50, 100, Description = "Thread-safe dispatching with 1000 keys and 50 clients.")]
        public async Task DispatchStatisticsAsync_ConcurrentDispatching(int clients, int keys)
        {
            // Arrange
            var keysList = Enumerable.Range(1, keys).Select(i => $"key{i}").ToList();
            var tasks = new List<Task>();

            foreach (var key in keysList)
            {
                var events = GenerateEvents(10, 50, key);
                mockReceiver.Setup(r => r.GetEventStreamAsync(key, It.IsAny<CancellationToken>())).Returns(events);
            }

            mockMediator.Setup(m => m.Send(It.IsAny<BuildStatisticsCommand<MockStatistics>>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(new MockStatistics());

            // Act
            for (int i = 0; i < clients; i++)
            {
                var startTasks = keysList.Select(key => Task.Run(async () =>
                {
                    await dispatcher.StartStatisticsDispatchingAsync(key);
                }));
                tasks.AddRange(startTasks);
            }

            await Task.WhenAll(tasks);

            var listeners = dispatcher.GetFieldValue<ConcurrentDictionary<string, CancellationTokenSource>>("listeners");
            Assert.IsNotNull(listeners);
            Assert.That(listeners.Keys.Count, Is.EqualTo(keys));
            Assert.IsTrue(listeners.Any());

            await Task.Delay(2000);

            var stopTasks = keysList.Select(key => Task.Run(async () =>
            {
                await dispatcher.StopStatisticsDispatchingAsync(key);
            }));
            await Task.WhenAll(stopTasks);

            // Assert
            foreach (var key in keysList)
            {
                mockMediator.Verify(m => m.Send(It.IsAny<BuildStatisticsCommand<MockStatistics>>(), It.IsAny<CancellationToken>()), Times.AtLeast(10));
                mockMediator.Verify(m => m.Send(It.IsAny<SendStatisticsCommand<MockStatistics>>(), It.IsAny<CancellationToken>()), Times.AtLeast(10));
            }

            Assert.That(listeners.Keys.Count, Is.EqualTo(0));
            Assert.IsFalse(listeners.Any());
        }

        [Test]
        [TestCase(typeof(OperationCanceledException), "Dispatching for key 'testKey' was canceled.")]
        [TestCase(typeof(InvalidOperationException), "Error occurred while dispatching for key 'testKey'.")]
        public async Task DispatchStatisticsAsync_LogsExceptionsGracefully(Type exceptionType, string expectedMessage)
        {
            // Arrange
            var key = "testKey";
            var exception = (Exception)Activator.CreateInstance(exceptionType)!;

            mockReceiver.Setup(r => r.GetEventStreamAsync(key, It.IsAny<CancellationToken>()))
                .Returns(GenerateEvents(10, 50, key));
            mockMediator.Setup(m => m.Send(It.IsAny<BuildStatisticsCommand<MockStatistics>>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(exception);

            // Act
            await dispatcher.StartStatisticsDispatchingAsync(key);
            await Task.Delay(100);

            // Assert
            LogLevel expectedLogLevel = exception is OperationCanceledException ? LogLevel.Information : LogLevel.Error;
            mockLogger.Verify(
                log => log.Log(
                    expectedLogLevel,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(expectedMessage)),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                ),
                Times.Once
            );
        }

        [Test]
        public async Task StopStatisticsDispatchingAsync_ExceptionDuringCancellation_LogsError()
        {
            // Arrange
            var key = "testKey";
            var tokenSource = new CancellationTokenSource();

            var listeners = dispatcher.GetFieldValue<ConcurrentDictionary<string, CancellationTokenSource>>("listeners");
            Assert.IsNotNull(listeners);
            listeners.TryAdd(key, tokenSource);

            await tokenSource.CancelAsync();
            tokenSource.Dispose();

            // Act
            await dispatcher.StopStatisticsDispatchingAsync(key);

            // Assert
            mockLogger.Verify(
                log => log.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Failed to stop dispatching for key '{key}'.")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                ),
                Times.Once
            );
        }
    }

    public class MockStatistics : BaseStatistics { }

    public class MockEventWrapper : BaseEventWrapper { }
}
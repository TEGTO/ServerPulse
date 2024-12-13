using AnalyzerApi.Command.Builders;
using AnalyzerApi.Command.Senders;
using AnalyzerApi.Infrastructure.Models.Statistics;
using AnalyzerApi.Infrastructure.Models.Wrappers;
using AnalyzerApi.Services.Receivers.Event;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;

namespace AnalyzerApi.Services.StatisticsDispatchers.Tests
{
    [TestFixture]
    internal class ServerStatisticsDispatcherTests
    {
        private Mock<IEventReceiver<MockEventWrapper>> mockReceiver;
        private Mock<IMediator> mockMediator;
        private Mock<ILogger<StatisticsDispatcher<MockStatistics, MockEventWrapper>>> mockLogger;
        private StatisticsDispatcher<MockStatistics, MockEventWrapper> dispatcher;

        [SetUp]
        public void Setup()
        {
            mockMediator = new Mock<IMediator>();
            mockReceiver = new Mock<IEventReceiver<MockEventWrapper>>();
            mockLogger = new Mock<ILogger<StatisticsDispatcher<MockStatistics, MockEventWrapper>>>();

            dispatcher = new StatisticsDispatcher<MockStatistics, MockEventWrapper>(
                mockReceiver.Object,
                mockMediator.Object,
                mockLogger.Object
            );
        }

        [Test]
        [TestCase("key1", true)]
        [TestCase("key2", false)]
        public async Task StartStatisticsDispatching_AddsListenerAndSendsInitialStatistics(string key, bool isInitial)
        {
            // Arrange
            var initialStatistics = new MockStatistics { };

            mockMediator.Setup(m => m.Send(It.IsAny<BuildStatisticsCommand<MockStatistics>>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(initialStatistics);

            // Act
            dispatcher.StartStatisticsDispatching(key);
            await Task.Delay(500);
            dispatcher.StopStatisticsDispatching(key);

            // Assert
            mockMediator.Verify(m => m.Send(It.IsAny<SendStatisticsCommand<MockStatistics>>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        [TestCase(3, 5)]
        [TestCase(1, 10)]
        public async Task StartStatisticsDispatching_SendsMultipleStatistics(int eventsPerKey, int delayPerEvent)
        {
            // Arrange
            var key = "testKey";
            var eventWrappers = Enumerable.Range(1, eventsPerKey).Select(i => new MockEventWrapper { Key = key, Id = "someId", CreationDateUTC = DateTime.UtcNow }).ToList();
            var statistics = new MockStatistics();

            mockReceiver.Setup(m => m.GetEventStreamAsync(key, It.IsAny<CancellationToken>()))
                        .Returns(AsyncEnumerable(eventWrappers, delayPerEvent));

            mockMediator.Setup(m => m.Send(It.IsAny<BuildStatisticsCommand<MockStatistics>>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(statistics);

            // Act
            dispatcher.StartStatisticsDispatching(key);
            await Task.Delay(eventsPerKey * delayPerEvent);
            dispatcher.StopStatisticsDispatching(key);

            // Assert
            mockMediator.Verify(m => m.Send(It.IsAny<SendStatisticsCommand<MockStatistics>>(), It.IsAny<CancellationToken>()), Times.AtLeast(eventsPerKey));
        }

        [Test]
        [TestCase(2)]
        [TestCase(5)]
        public async Task StartStatisticsDispatching_DoesNotSendAfterStop(int delayBeforeStop)
        {
            // Arrange
            var key = "testKey";
            var statistics = new MockStatistics();

            mockReceiver.Setup(m => m.GetEventStreamAsync(key, It.IsAny<CancellationToken>()))
                        .Returns(AsyncEnumerable(Enumerable.Repeat(new MockEventWrapper { Key = key, Id = "someId", CreationDateUTC = DateTime.UtcNow }, 10), 100));

            mockMediator.Setup(m => m.Send(It.IsAny<BuildStatisticsCommand<MockStatistics>>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(statistics);

            // Act
            dispatcher.StartStatisticsDispatching(key);
            await Task.Delay(delayBeforeStop * 100);
            dispatcher.StopStatisticsDispatching(key);
            await Task.Delay(2000);

            // Assert
            mockMediator.Verify(m => m.Send(It.IsAny<SendStatisticsCommand<MockStatistics>>(), It.IsAny<CancellationToken>()), Times.AtMost(delayBeforeStop));
        }

        private static async IAsyncEnumerable<T> AsyncEnumerable<T>(IEnumerable<T> items, int delay)
        {
            foreach (var item in items)
            {
                yield return item;
                await Task.Delay(delay);
            }
        }
    }

    public class MockStatistics : BaseStatistics { }

    public class MockEventWrapper : BaseEventWrapper { }
}
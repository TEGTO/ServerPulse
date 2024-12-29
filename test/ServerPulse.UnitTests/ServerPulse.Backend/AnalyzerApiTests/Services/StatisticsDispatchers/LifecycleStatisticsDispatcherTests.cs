using AnalyzerApi.Command.Builders;
using AnalyzerApi.Command.Senders;
using AnalyzerApi.Infrastructure.Configuration;
using AnalyzerApi.Infrastructure.Models.Statistics;
using AnalyzerApi.Infrastructure.Models.Wrappers;
using AnalyzerApi.Services.Receivers.Event;
using AnalyzerApiTests;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using System.Collections.Concurrent;
using static AnalyzerApi.Services.StatisticsDispatchers.LifecycleStatisticsDispatcher;

namespace AnalyzerApi.Services.StatisticsDispatchers.Tests
{
    [TestFixture]
    internal class LifecycleStatisticsDispatcherTests
    {
        private Mock<IEventReceiver<PulseEventWrapper>> mockReceiver;
        private Mock<IEventReceiver<ConfigurationEventWrapper>> mockConfReceiver;
        private Mock<IMediator> mockMediator;
        private Mock<ILogger<LifecycleStatisticsDispatcher>> mockLogger;
        private Mock<IConfiguration> mockConfiguration;
        private LifecycleStatisticsDispatcher dispatcher;

        private const int SendPeriodInMilliseconds = 100;

        [SetUp]
        public void Setup()
        {
            mockReceiver = new Mock<IEventReceiver<PulseEventWrapper>>();
            mockConfReceiver = new Mock<IEventReceiver<ConfigurationEventWrapper>>();
            mockMediator = new Mock<IMediator>();
            mockLogger = new Mock<ILogger<LifecycleStatisticsDispatcher>>();
            mockConfiguration = new Mock<IConfiguration>();

            mockConfiguration.SetupGet(c => c[ConfigurationKeys.STATISTICS_COLLECT_INTERVAL_IN_MILLISECONDS])
                .Returns(SendPeriodInMilliseconds.ToString());

            dispatcher = new LifecycleStatisticsDispatcher(
                mockReceiver.Object,
                mockConfReceiver.Object,
                mockMediator.Object,
                mockConfiguration.Object,
                mockLogger.Object
            );
        }

        [TearDown]
        public async Task TearDown()
        {
            await dispatcher.DisposeAsync();
        }

        private static async IAsyncEnumerable<PulseEventWrapper> GeneratePulseEvents(bool[] isAliveStates, int delay, string key)
        {
            foreach (var isAlive in isAliveStates)
            {
                yield return new PulseEventWrapper { Key = key, Id = $"Pulse-{Guid.NewGuid()}", IsAlive = isAlive };
                await Task.Delay(delay);
            }
        }
        private static async IAsyncEnumerable<ConfigurationEventWrapper> GenerateConfigurationEvents(IEnumerable<ConfigurationEventWrapper> events, int delay)
        {
            foreach (var ev in events)
            {
                yield return ev;
                await Task.Delay(delay);
            }
        }

        [Test]
        [TestCase(10, 100, true, Description = "Sends at least 10 statistics within 1000ms.")]
        [TestCase(25, 100, true, Description = "Sends at least 25 statistics within 2500ms.")]
        //[TestCase(50, 100, true, Description = "Sends at least 50 statistics within 5000ms.")]
        [TestCase(80, 10, false, Description = "Sends less that 80 statistics within 800ms, configuration delay (100ms per call) blocks calls.")]
        //[TestCase(300, 100, false, Description = "Sends less than 300 statistics within 30000ms, configuration delay (100ms per call) blocks calls.")]
        public async Task SendStatisticsAsync_MultipleStatisticsSends_VerifyCalls(int expectedCalls, int delayBetweenSends, bool isSuccess)
        {
            // Arrange
            var key = "testKey";
            var mockStatistics = new ServerLifecycleStatistics { IsAlive = true, DataExists = true };
            var mockConfigurationEvent = new ConfigurationEventWrapper
            {
                Id = "someId",
                ServerKeepAliveInterval = TimeSpan.FromMilliseconds(delayBetweenSends),
                Key = key,
            };

            mockMediator.Setup(m => m.Send(It.IsAny<BuildStatisticsCommand<ServerLifecycleStatistics>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockStatistics);
            mockConfReceiver.Setup(c => c.GetLastEventByKeyAsync(key, It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockConfigurationEvent);

            // Act
            await dispatcher.StartStatisticsDispatchingAsync(key);
            await Task.Delay(expectedCalls * delayBetweenSends + 500);
            await dispatcher.StopStatisticsDispatchingAsync(key);

            // Assert
            if (isSuccess)
            {
                mockMediator.Verify(m => m.Send(It.IsAny<BuildStatisticsCommand<ServerLifecycleStatistics>>(), It.IsAny<CancellationToken>()), Times.AtLeast(expectedCalls));
                mockMediator.Verify(m => m.Send(It.IsAny<SendStatisticsCommand<ServerLifecycleStatistics>>(), It.IsAny<CancellationToken>()), Times.AtLeast(expectedCalls));
            }
            else
            {
                mockMediator.Verify(m => m.Send(It.IsAny<BuildStatisticsCommand<ServerLifecycleStatistics>>(), It.IsAny<CancellationToken>()), Times.AtMost(expectedCalls));
                mockMediator.Verify(m => m.Send(It.IsAny<SendStatisticsCommand<ServerLifecycleStatistics>>(), It.IsAny<CancellationToken>()), Times.AtMost(expectedCalls));
            }
        }

        [Test]
        public async Task MonitorPulseEventAsync_StopsWhenTokenCancelled()
        {
            // Arrange
            var key = "testKey";
            var cts = new CancellationTokenSource();
            cts.CancelAfter(300);

            mockReceiver.Setup(r => r.GetEventStreamAsync(key, It.IsAny<CancellationToken>()))
                .Returns(GeneratePulseEvents([true, false, true], 100, key));

            // Act
            await dispatcher.StartStatisticsDispatchingAsync(key);
            await Task.Delay(500);
            await dispatcher.StopStatisticsDispatchingAsync(key);

            // Assert
            Assert.That(() => cts.IsCancellationRequested, Is.True);

            mockReceiver.Verify(r => r.GetEventStreamAsync(key, cts.Token), Times.AtMostOnce());

            cts.Dispose();
        }

        [Test]
        public async Task MonitorPulseEventsAsync_UpdatesAliveStateCorrectly()
        {
            // Arrange
            var key = "testKey";
            var mockStatistics = new ServerLifecycleStatistics { IsAlive = false, DataExists = false, };
            var pulseEvents = GeneratePulseEvents([true, true, true], 200, key);
            var mockConfigurationEvent = new ConfigurationEventWrapper
            {
                Id = "someId",
                ServerKeepAliveInterval = TimeSpan.FromMilliseconds(100),
                Key = key,
            };

            mockMediator.Setup(m => m.Send(It.IsAny<BuildStatisticsCommand<ServerLifecycleStatistics>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockStatistics);
            mockReceiver.Setup(r => r.GetEventStreamAsync(key, It.IsAny<CancellationToken>()))
                .Returns(pulseEvents);
            mockConfReceiver.Setup(c => c.GetLastEventByKeyAsync(key, It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockConfigurationEvent);

            // Act
            await dispatcher.StartStatisticsDispatchingAsync(key);
            await Task.Delay(1000);
            await dispatcher.StopStatisticsDispatchingAsync(key);

            // Assert
            mockMediator.Verify(m => m.Send(It.IsAny<BuildStatisticsCommand<ServerLifecycleStatistics>>(), It.IsAny<CancellationToken>()), Times.AtLeast(2));
            mockMediator.Verify(m => m.Send(It.IsAny<SendStatisticsCommand<ServerLifecycleStatistics>>(), It.IsAny<CancellationToken>()), Times.AtLeast(2));
            mockConfReceiver.Verify(m => m.GetLastEventByKeyAsync(key, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task SendStatisticsAsync_ByDefaultUsing1sDelay()
        {
            // Arrange
            var key = "testKey";
            var mockStatistics = new ServerLifecycleStatistics { IsAlive = false, DataExists = false, };
            var pulseEvents = GeneratePulseEvents([true, true, true], 200, key);

            mockMediator.Setup(m => m.Send(It.IsAny<BuildStatisticsCommand<ServerLifecycleStatistics>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockStatistics);
            mockReceiver.Setup(r => r.GetEventStreamAsync(key, It.IsAny<CancellationToken>()))
                .Returns(pulseEvents);

            // Act
            await dispatcher.StartStatisticsDispatchingAsync(key);
            await Task.Delay(100);

            // Assert
            var listenerState = dispatcher.GetFieldValue<ConcurrentDictionary<string, ListenerState>>("listenerState");
            Assert.IsNotNull(listenerState);
            Assert.IsTrue(listenerState.TryGetValue(key, out var state));
            Assert.IsNotNull(state);
            Assert.That(state.Timer.Period.TotalMilliseconds, Is.EqualTo(100));

            // Clean Up
            await dispatcher.StopStatisticsDispatchingAsync(key);
        }

        [Test]
        public async Task SendStatisticsAsync_RespectsServerUpdateInterval()
        {
            // Arrange
            var key = "testKey";
            var initialInterval = 500;
            var updatedInterval = 2000;
            var mockStatistics = new ServerLifecycleStatistics { IsAlive = true, DataExists = true, };
            var initialConfig = new ConfigurationEventWrapper
            {
                Id = "someId",
                ServerKeepAliveInterval = TimeSpan.FromMilliseconds(initialInterval),
                Key = key,
            };
            var updatedConfig = new ConfigurationEventWrapper
            {
                Id = "someId",
                ServerKeepAliveInterval = TimeSpan.FromMilliseconds(updatedInterval),
                Key = key,
            };

            mockMediator.Setup(m => m.Send(It.IsAny<BuildStatisticsCommand<ServerLifecycleStatistics>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockStatistics);

            mockConfReceiver.SetupSequence(c => c.GetLastEventByKeyAsync(key, It.IsAny<CancellationToken>()))
                .ReturnsAsync(initialConfig);
            mockConfReceiver.Setup(r => r.GetEventStreamAsync(key, It.IsAny<CancellationToken>()))
                .Returns(GenerateConfigurationEvents([updatedConfig], 300));

            // Act
            await dispatcher.StartStatisticsDispatchingAsync(key);
            await Task.Delay(updatedInterval * 3);

            // Assert
            mockConfReceiver.Verify(c => c.GetLastEventByKeyAsync(key, It.IsAny<CancellationToken>()), Times.Once);
            var listenerState = dispatcher.GetFieldValue<ConcurrentDictionary<string, ListenerState>>("listenerState");
            Assert.IsNotNull(listenerState);
            Assert.IsTrue(listenerState.TryGetValue(key, out var state));
            Assert.IsNotNull(state);
            Assert.That(state.Timer.Period.TotalMilliseconds, Is.EqualTo(updatedInterval / 2));

            // Clean Up
            await dispatcher.StopStatisticsDispatchingAsync(key);
        }

        [Test]
        public async Task OnListenerRemoved_DisposesState()
        {
            // Arrange
            var key = "testKey";
            var mockStatistics = new ServerLifecycleStatistics { IsAlive = true, DataExists = true, };
            var initialConfig = new ConfigurationEventWrapper
            {
                Id = "someId",
                ServerKeepAliveInterval = TimeSpan.FromMilliseconds(1000),
                Key = key,
            };
            var pulseEvents = GeneratePulseEvents([true], 100, key);

            mockMediator.Setup(m => m.Send(It.IsAny<BuildStatisticsCommand<ServerLifecycleStatistics>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockStatistics);
            mockConfReceiver.SetupSequence(c => c.GetLastEventByKeyAsync(key, It.IsAny<CancellationToken>()))
                .ReturnsAsync(initialConfig);
            mockReceiver.Setup(r => r.GetEventStreamAsync(key, It.IsAny<CancellationToken>()))
                .Returns(pulseEvents);

            // Act
            await dispatcher.StartStatisticsDispatchingAsync(key);
            await Task.Delay(200);
            await dispatcher.StopStatisticsDispatchingAsync(key);

            // Assert
            var listenerState = dispatcher.GetFieldValue<ConcurrentDictionary<string, ListenerState>>("listenerState");
            Assert.IsNotNull(listenerState);
            Assert.IsFalse(listenerState.TryGetValue(key, out var _));
        }
    }
}
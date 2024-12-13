using AnalyzerApi.Command.Builders;
using AnalyzerApi.Command.Senders;
using AnalyzerApi.Infrastructure;
using AnalyzerApi.Infrastructure.Models.Statistics;
using AnalyzerApi.Infrastructure.Models.Wrappers;
using AnalyzerApi.Services.Receivers.Event;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;

namespace AnalyzerApi.Services.StatisticsDispatchers.Tests
{
    [TestFixture]
    internal class LifecycleStatisticsDispatcherTests
    {
        private const int STATISTICS_COLLECT_INTERVAL = 1000;

        private Mock<IEventReceiver<PulseEventWrapper>> mockPulseReceiver;
        private Mock<IEventReceiver<ConfigurationEventWrapper>> mockConfReceiver;
        private Mock<IConfiguration> mockConfiguration;
        private Mock<IMediator> mockMediator;
        private Mock<ILogger<LifecycleStatisticsDispatcher>> mockLogger;

        private LifecycleStatisticsDispatcher dispatcher;

        [SetUp]
        public void Setup()
        {
            mockPulseReceiver = new Mock<IEventReceiver<PulseEventWrapper>>();
            mockConfReceiver = new Mock<IEventReceiver<ConfigurationEventWrapper>>();
            mockConfiguration = new Mock<IConfiguration>();
            mockMediator = new Mock<IMediator>();
            mockLogger = new Mock<ILogger<LifecycleStatisticsDispatcher>>();

            mockConfiguration.SetupGet(c => c[Configuration.STATISTICS_COLLECT_INTERVAL_IN_MILLISECONDS])
                             .Returns(STATISTICS_COLLECT_INTERVAL.ToString());

            dispatcher = new LifecycleStatisticsDispatcher(
                mockPulseReceiver.Object,
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

        [Test]
        [TestCase(5, 3, Description = "Dispatches and updates pulse states for 3 of 5 events.")]
        public async Task DispatchStatisticsAsync_UpdatesPulseCorrectly(int eventCount, int expectedUpdates)
        {
            // Arrange
            var key = "testKey";
            var events = GeneratePulseEvents(eventCount, 100, key);
            mockPulseReceiver.Setup(r => r.GetEventStreamAsync(key, It.IsAny<CancellationToken>())).Returns(events);

            var mockStatistics = new ServerLifecycleStatistics { IsAlive = true };
            mockMediator
                .Setup(m => m.Send(It.IsAny<BuildStatisticsCommand<ServerLifecycleStatistics>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockStatistics);

            // Act
            await dispatcher.StartStatisticsDispatchingAsync(key);
            await Task.Delay(500);
            await dispatcher.StopStatisticsDispatchingAsync(key);

            // Assert
            mockMediator.Verify(m => m.Send(It.IsAny<BuildStatisticsCommand<ServerLifecycleStatistics>>(), It.IsAny<CancellationToken>()), Times.AtLeast(expectedUpdates));
        }

        [Test]
        [TestCase(3, 1000, Description = "Dispatches 3 periodic statistics with 1 second intervals.")]
        public async Task SendStatisticsAsync_PeriodicDispatching(int dispatchCount, int intervalMs)
        {
            // Arrange
            var key = "testKey";
            var mockStatistics = new ServerLifecycleStatistics { IsAlive = true };
            mockMediator
                .Setup(m => m.Send(It.IsAny<BuildStatisticsCommand<ServerLifecycleStatistics>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockStatistics);

            mockPulseReceiver.Setup(r => r.GetEventStreamAsync(key, It.IsAny<CancellationToken>()))
                        .Returns(GeneratePulseEvents(10, intervalMs, key));

            // Act
            await dispatcher.StartStatisticsDispatchingAsync(key);
            await Task.Delay(dispatchCount * intervalMs + 500);
            await dispatcher.StopStatisticsDispatchingAsync(key);

            // Assert
            mockMediator.Verify(m => m.Send(It.IsAny<SendStatisticsCommand<ServerLifecycleStatistics>>(), It.IsAny<CancellationToken>()), Times.AtLeast(dispatchCount));
        }

        [Test]
        [TestCase("key1", 2000, 3000, Description = "Updates server keep-alive interval.")]
        public async Task SendStatisticsAsync_UpdatesConfigurationInterval(string key, int initialInterval, int updatedInterval)
        {
            // Arrange
            var mockConfigEvent = new ConfigurationEventWrapper { ServerKeepAliveInterval = TimeSpan.FromMilliseconds(updatedInterval) };
            mockPulseReceiver.Setup(r => r.GetEventStreamAsync(key, It.IsAny<CancellationToken>()))
                        .Returns(GeneratePulseEvents(5, initialInterval, key));
            mockConfReceiver.Setup(r => r.GetLastEventByKeyAsync(key, It.IsAny<CancellationToken>()))
                            .ReturnsAsync(mockConfigEvent);

            var mockStatistics = new ServerLifecycleStatistics { IsAlive = true };
            mockMediator.Setup(m => m.Send(It.IsAny<BuildStatisticsCommand<ServerLifecycleStatistics>>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(mockStatistics);

            // Act
            await dispatcher.StartStatisticsDispatchingAsync(key);
            await Task.Delay(5000);
            await dispatcher.StopStatisticsDispatchingAsync(key);

            // Assert
            mockMediator.Verify(m => m.Send(It.IsAny<SendStatisticsCommand<ServerLifecycleStatistics>>(), It.IsAny<CancellationToken>()), Times.AtLeast(2));
        }

        [Test]
        [TestCase(typeof(OperationCanceledException), "Dispatching for key 'testKey' was canceled.")]
        [TestCase(typeof(InvalidOperationException), "Error occurred while dispatching for key 'testKey'.")]
        public async Task DispatchStatisticsAsync_LogsExceptionsGracefully(Type exceptionType, string expectedMessage)
        {
            // Arrange
            var key = "testKey";
            var exception = (Exception)Activator.CreateInstance(exceptionType)!;

            mockPulseReceiver.Setup(r => r.GetEventStreamAsync(key, It.IsAny<CancellationToken>()))
                        .Returns(GeneratePulseEvents(10, 50, key));
            mockMediator.Setup(m => m.Send(It.IsAny<BuildStatisticsCommand<ServerLifecycleStatistics>>(), It.IsAny<CancellationToken>()))
                        .ThrowsAsync(exception);

            // Act
            await dispatcher.StartStatisticsDispatchingAsync(key);
            await Task.Delay(200);

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
    }
}
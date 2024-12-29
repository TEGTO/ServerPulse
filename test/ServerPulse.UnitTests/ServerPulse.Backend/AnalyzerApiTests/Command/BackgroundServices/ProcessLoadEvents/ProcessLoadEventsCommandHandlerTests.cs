using AnalyzerApi.Command.BackgroundServices.ProcessLoadEvents;
using AnalyzerApi.Infrastructure.Configuration;
using AnalyzerApi.Infrastructure.Models.Statistics;
using AnalyzerApi.Services.Receivers.Statistics;
using EventCommunication;
using MessageBus.Interfaces;
using Microsoft.Extensions.Options;
using Moq;

namespace AnalyzerApiTests.Command.BackgroundServices.ProcessLoadEvents
{
    [TestFixture]
    internal class ProcessLoadEventsCommandHandlerTests
    {
        private Mock<IMessageProducer> mockProducer;
        private Mock<IStatisticsReceiver<LoadMethodStatistics>> mockReceiver;
        private ProcessLoadEventsCommandHandler handler;

        private const string LoadMethodStatisticsTopic = "load-method-statistics";

        [SetUp]
        public void Setup()
        {
            mockProducer = new Mock<IMessageProducer>();
            mockReceiver = new Mock<IStatisticsReceiver<LoadMethodStatistics>>();

            var settings = new MessageBusSettings
            {
                LoadMethodStatisticsTopic = LoadMethodStatisticsTopic
            };

            var mockOptions = new Mock<IOptions<MessageBusSettings>>();
            mockOptions.Setup(x => x.Value).Returns(settings);

            handler = new ProcessLoadEventsCommandHandler(
                mockProducer.Object,
                mockReceiver.Object,
                mockOptions.Object
            );
        }

        [Test]
        public async Task Handle_ValidCommand_ProcessesEventsAndSendsStatistics()
        {
            // Arrange
            var key = "testKey";
            var events = new[]
            {
                new LoadEvent(key, "/api/test", "GET", 200, TimeSpan.FromMilliseconds(100), DateTime.UtcNow),
                new LoadEvent(key, "/api/test", "POST", 201, TimeSpan.FromMilliseconds(150), DateTime.UtcNow)
            };

            var command = new ProcessLoadEventsCommand(events);
            var initialStatistics = new LoadMethodStatistics { GetAmount = 1, PostAmount = 1 };

            mockReceiver
                .Setup(r => r.GetLastStatisticsAsync(key, It.IsAny<CancellationToken>()))
                .ReturnsAsync(initialStatistics);

            // Act
            await handler.Handle(command, CancellationToken.None);

            // Assert
            mockProducer.Verify(p =>
                p.ProduceAsync(
                    $"{LoadMethodStatisticsTopic}{key}",
                    It.Is<string>(s => s.Contains("\"GetAmount\":2") && s.Contains("\"PostAmount\":2")),
                    It.IsAny<CancellationToken>()
                ), Times.Once);

            mockReceiver.Verify(r => r.GetLastStatisticsAsync(key, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task Handle_StatisticsUpdatedCorrectly()
        {
            // Arrange
            var key = "testKey";
            var events = new[]
            {
                new LoadEvent(key, "/api/test", "PATCH", 200, TimeSpan.FromMilliseconds(100), DateTime.UtcNow),
                new LoadEvent(key, "/api/test", "PUT", 200, TimeSpan.FromMilliseconds(150), DateTime.UtcNow),
                new LoadEvent(key, "/api/test", "PUT", 200, TimeSpan.FromMilliseconds(120), DateTime.UtcNow)
            };

            var command = new ProcessLoadEventsCommand(events);
            var initialStatistics = new LoadMethodStatistics { PatchAmount = 2 };

            mockReceiver
                .Setup(r => r.GetLastStatisticsAsync(key, It.IsAny<CancellationToken>()))
                .ReturnsAsync(initialStatistics);

            // Act
            await handler.Handle(command, CancellationToken.None);

            // Assert
            mockProducer.Verify(p =>
                p.ProduceAsync(
                    $"{LoadMethodStatisticsTopic}{key}",
                    It.Is<string>(s => s.Contains("\"PatchAmount\":3") && s.Contains("\"PutAmount\":2")),
                    It.IsAny<CancellationToken>()
                ), Times.Once);

            mockReceiver.Verify(r => r.GetLastStatisticsAsync(key, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public void Handle_NullOrEmptyEvents_ThrowsInvalidDataException()
        {
            // Arrange
            var commandWithNullEvents = new ProcessLoadEventsCommand(null!);
            var commandWithEmptyEvents = new ProcessLoadEventsCommand(Array.Empty<LoadEvent>());

            // Act & Assert
            Assert.ThrowsAsync<InvalidDataException>(() => handler.Handle(commandWithNullEvents, CancellationToken.None));
            Assert.ThrowsAsync<InvalidDataException>(() => handler.Handle(commandWithEmptyEvents, CancellationToken.None));
        }

        [Test]
        public async Task Handle_DifferentEventKeys_CreatesStatisticsForEachKey()
        {
            // Arrange
            var events = new[]
            {
                new LoadEvent("key1", "/api/test", "GET", 200, TimeSpan.FromMilliseconds(100), DateTime.UtcNow),
                new LoadEvent("key2", "/api/test", "POST", 201, TimeSpan.FromMilliseconds(150), DateTime.UtcNow)
            };

            var command = new ProcessLoadEventsCommand(events);

            // Act
            await handler.Handle(command, CancellationToken.None);

            // Assert
            mockProducer.Verify(p => p.ProduceAsync(LoadMethodStatisticsTopic + "key1", It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
            mockProducer.Verify(p => p.ProduceAsync(LoadMethodStatisticsTopic + "key2", It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);

            mockReceiver.Verify(r => r.GetLastStatisticsAsync("key1", It.IsAny<CancellationToken>()), Times.Once);
            mockReceiver.Verify(r => r.GetLastStatisticsAsync("key2", It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task Handle_NoExistingStatistics_CreatesNewStatisticsAndSends()
        {
            // Arrange
            var key = "testKey";
            var events = new[]
            {
                new LoadEvent(key, "/api/test", "GET", 200, TimeSpan.FromMilliseconds(100), DateTime.UtcNow),
                new LoadEvent(key, "/api/test", "DELETE", 204, TimeSpan.FromMilliseconds(250), DateTime.UtcNow)
            };

            var command = new ProcessLoadEventsCommand(events);

            mockReceiver
                .Setup(r => r.GetLastStatisticsAsync(key, It.IsAny<CancellationToken>()))
                .ReturnsAsync((LoadMethodStatistics?)null);

            // Act
            await handler.Handle(command, CancellationToken.None);

            // Assert
            mockProducer.Verify(p =>
                p.ProduceAsync(
                    $"{LoadMethodStatisticsTopic}{key}",
                    It.Is<string>(s => s.Contains("\"GetAmount\":1") && s.Contains("\"DeleteAmount\":1")),
                    It.IsAny<CancellationToken>()
                ), Times.Once);

            mockReceiver.Verify(r => r.GetLastStatisticsAsync(key, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task Handle_ConcurrentEventsForSameKey_ProcessesSequentially()
        {
            // Arrange
            var events = new[]
            {
                new LoadEvent("key1", "/api/test", "GET", 200, TimeSpan.FromMilliseconds(100), DateTime.UtcNow),
                new LoadEvent("key1", "/api/test", "POST", 201, TimeSpan.FromMilliseconds(150), DateTime.UtcNow)
            };

            var command = new ProcessLoadEventsCommand(events);

            // Act
            await Task.WhenAll(
                Task.Run(() => handler.Handle(command, CancellationToken.None)),
                Task.Run(() => handler.Handle(command, CancellationToken.None))
            );

            // Assert
            mockReceiver.Verify(r => r.GetLastStatisticsAsync("key1", It.IsAny<CancellationToken>()), Times.Exactly(2));
            mockProducer.Verify(p => p.ProduceAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
        }

    }
}
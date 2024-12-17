using AnalyzerApi.Infrastructure.Models.Statistics;
using EventCommunication;

namespace AnalyzerApi.IntegrationTests.BackgroundServices
{
    internal class LoadEventStatisticsProcessorOneBunchTests : BaseIntegrationTest
    {
        [Test]
        public async Task ProcessLoad_ValidLoadEventsInOneBunch_AddsStatisticsToMessageBus()
        {
            // Arrange
            var loadEvents = new[]
            {
                new LoadEvent("key1", "/api/resource", "GET", 200, TimeSpan.FromMilliseconds(150), DateTime.UtcNow),
                new LoadEvent("key1", "/api/resource", "POST", 201, TimeSpan.FromMilliseconds(200), DateTime.UtcNow),
                new LoadEvent("key1", "/api/resource", "DELETE", 201, TimeSpan.FromMilliseconds(200), DateTime.UtcNow)
            };

            // Act
            await SendEventsAsync(LOAD_PROCESS_TOPIC, "", loadEvents);
            await Task.Delay(7000);

            // Assert
            var statistics = await ReceiveLastObjectFromTopicAsync<LoadMethodStatistics>(LOAD_METHOD_STATISTICS_TOPIC, loadEvents[0].Key);

            Assert.IsNotNull(statistics);
            Assert.That(statistics.GetAmount, Is.EqualTo(1));
            Assert.That(statistics.PostAmount, Is.EqualTo(1));
            Assert.That(statistics.PutAmount, Is.EqualTo(0));
            Assert.That(statistics.PatchAmount, Is.EqualTo(0));
            Assert.That(statistics.DeleteAmount, Is.EqualTo(1));
        }

        [Test]
        public async Task ProcessLoad_ValidLoadEventsInOneBunchWithDifferentKeys_AddsStatisticsToMessageBus()
        {
            // Arrange
            var loadEvents = new[]
            {
                new LoadEvent("different-key1", "/api/resource", "GET", 200, TimeSpan.FromMilliseconds(150), DateTime.UtcNow),
                new LoadEvent("different-key1", "/api/resource", "POST", 201, TimeSpan.FromMilliseconds(200), DateTime.UtcNow),
                new LoadEvent("different-key2", "/api/resource", "DELETE", 201, TimeSpan.FromMilliseconds(200), DateTime.UtcNow)
            };

            // Act
            await SendEventsAsync(LOAD_PROCESS_TOPIC, "", loadEvents);
            await Task.Delay(7000);

            // Assert
            var statistics = await ReceiveLastObjectFromTopicAsync<LoadMethodStatistics>(LOAD_METHOD_STATISTICS_TOPIC, loadEvents[0].Key);

            Assert.IsNotNull(statistics);
            Assert.That(statistics.GetAmount, Is.EqualTo(1));
            Assert.That(statistics.PostAmount, Is.EqualTo(1));
            Assert.That(statistics.PutAmount, Is.EqualTo(0));
            Assert.That(statistics.PatchAmount, Is.EqualTo(0));
            Assert.That(statistics.DeleteAmount, Is.EqualTo(0));
        }

        [Test]
        public async Task ProcessLoad_InvalidLoadEventsInOneBunch_DoesNotAddStatisticsToMessageBus()
        {
            // Arrange
            var notLoadEvents = new[]
            {
                new TestEvent("key2"),
                new TestEvent("key2")
            };

            // Act
            await SendEventsAsync(LOAD_PROCESS_TOPIC, "", notLoadEvents);
            await Task.Delay(500);

            // Assert
            var statistics = await ReceiveLastObjectFromTopicAsync<LoadMethodStatistics>(LOAD_METHOD_STATISTICS_TOPIC, notLoadEvents[0].Key);

            Assert.IsNull(statistics);
        }
    }

    public sealed record class TestEvent(string Key) : BaseEvent(Key);
}
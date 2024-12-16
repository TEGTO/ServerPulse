using AnalyzerApi.Infrastructure.Models.Statistics;
using EventCommunication;

namespace AnalyzerApi.IntegrationTests.BackgroundServices
{
    internal class LoadEventStatisticsProcessorTests : BaseIntegrationTest
    {
        [Test]
        public async Task ProcessLoad_ValidLoadEvents_AddsStatisticsToMessageBus()
        {
            // Arrange
            var loadEvents = new[]
            {
                new LoadEvent("validKey", "/api/resource", "GET", 200, TimeSpan.FromMilliseconds(150), DateTime.UtcNow),
                new LoadEvent("validKey", "/api/resource", "POST", 201, TimeSpan.FromMilliseconds(200), DateTime.UtcNow)
            };

            // Act
            await SendEventsAsync(LOAD_PROCESS_TOPIC, "", loadEvents);
            await Task.Delay(5000);

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
        public async Task ProcessLoad_InvalidLoadEvents_DoesNotAddStatisticsToMessageBus()
        {
            // Arrange
            var notLoadEvents = new[]
            {
                new TestEvent("validKey"),
                new TestEvent("validKey")
            };

            // Act
            await SendEventsAsync(LOAD_PROCESS_TOPIC, "", notLoadEvents);
            await Task.Delay(5000);

            // Assert
            var statistics = await ReceiveLastObjectFromTopicAsync<LoadMethodStatistics>(LOAD_METHOD_STATISTICS_TOPIC, notLoadEvents[0].Key);

            Assert.IsNull(statistics);
        }
    }

    public sealed record class TestEvent(string Key) : BaseEvent(Key);
}
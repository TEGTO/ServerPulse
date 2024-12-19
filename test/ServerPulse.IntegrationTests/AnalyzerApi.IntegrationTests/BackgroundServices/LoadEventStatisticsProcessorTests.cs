using AnalyzerApi.Infrastructure.Models.Statistics;
using EventCommunication;

namespace AnalyzerApi.IntegrationTests.BackgroundServices
{
    [TestFixture, Parallelizable(ParallelScope.All)]
    internal class LoadEventStatisticsProcessorTests : BaseIntegrationTest
    {
        [Test]
        public async Task ProcessLoad_ValidLoadEventsInManyBunches_AddsStatisticsToMessageBus()
        {
            // Arrange
            var key = Guid.NewGuid().ToString();
            var loadEvents = new[]
            {
                new LoadEvent(key, "/api/resource", "GET", 200, TimeSpan.FromMilliseconds(150), DateTime.UtcNow),
                new LoadEvent(key, "/api/resource", "POST", 201, TimeSpan.FromMilliseconds(200), DateTime.UtcNow),
                new LoadEvent(key, "/api/resource", "DELETE", 201, TimeSpan.FromMilliseconds(200), DateTime.UtcNow)
            };

            // Act
            await SendEventsAsync(LOAD_PROCESS_TOPIC, "", [loadEvents[0]]);
            await Task.Delay(1500);
            await SendEventsAsync(LOAD_PROCESS_TOPIC, "", [loadEvents[1]]);
            await Task.Delay(1500);
            await SendEventsAsync(LOAD_PROCESS_TOPIC, "", [loadEvents[2]]);
            await Task.Delay(3000);

            // Assert
            var statistics = await WaitForStatisticsAsync<LoadMethodStatistics>(
                LOAD_METHOD_STATISTICS_TOPIC, loadEvents[0].Key, TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(1));

            Assert.IsNotNull(statistics);
            Assert.That(statistics.GetAmount, Is.LessThanOrEqualTo(1));
            Assert.That(statistics.PostAmount, Is.LessThanOrEqualTo(1));
            Assert.That(statistics.PutAmount, Is.EqualTo(0));
            Assert.That(statistics.PatchAmount, Is.EqualTo(0));
            Assert.That(statistics.DeleteAmount, Is.LessThanOrEqualTo(1));
        }

        [Test]
        public async Task ProcessLoad_InvalidLoadEventsInManyBunches_DoesNotAddStatisticsToMessageBus()
        {
            // Arrange
            var key = Guid.NewGuid().ToString();
            var notLoadEvents = new[]
            {
                new TestEvent(key),
                new TestEvent(key)
            };

            // Act
            await SendEventsAsync(LOAD_PROCESS_TOPIC, "", [notLoadEvents[0]]);
            await Task.Delay(1500);
            await SendEventsAsync(LOAD_PROCESS_TOPIC, "", [notLoadEvents[1]]);
            await Task.Delay(3000);

            // Assert
            var statistics = await WaitForStatisticsAsync<LoadMethodStatistics>(
                LOAD_METHOD_STATISTICS_TOPIC, notLoadEvents[0].Key, TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(1));

            Assert.IsNull(statistics);
        }

        [Test]
        public async Task ProcessLoad_ValidLoadEventsInOneBunch_AddsStatisticsToMessageBus()
        {
            // Arrange
            var key = Guid.NewGuid().ToString();
            var loadEvents = new[]
            {
                new LoadEvent(key, "/api/resource", "GET", 200, TimeSpan.FromMilliseconds(150), DateTime.UtcNow),
                new LoadEvent(key, "/api/resource", "POST", 201, TimeSpan.FromMilliseconds(200), DateTime.UtcNow),
                new LoadEvent(key, "/api/resource", "DELETE", 201, TimeSpan.FromMilliseconds(200), DateTime.UtcNow)
            };

            // Act
            await SendEventsAsync(LOAD_PROCESS_TOPIC, "", loadEvents);
            await Task.Delay(3000);

            // Assert
            var statistics = await WaitForStatisticsAsync<LoadMethodStatistics>(
                LOAD_METHOD_STATISTICS_TOPIC, loadEvents[0].Key, TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(1));

            Assert.IsNotNull(statistics);
            Assert.That(statistics.GetAmount, Is.LessThanOrEqualTo(1));
            Assert.That(statistics.PostAmount, Is.LessThanOrEqualTo(1));
            Assert.That(statistics.PutAmount, Is.EqualTo(0));
            Assert.That(statistics.PatchAmount, Is.EqualTo(0));
            Assert.That(statistics.DeleteAmount, Is.LessThanOrEqualTo(1));
        }

        [Test]
        public async Task ProcessLoad_InvalidLoadEventsInOneBunch_DoesNotAddStatisticsToMessageBus()
        {
            // Arrange
            var key = Guid.NewGuid().ToString();
            var notLoadEvents = new[]
            {
                new TestEvent(key),
                new TestEvent(key)
            };

            // Act
            await SendEventsAsync(LOAD_PROCESS_TOPIC, "", notLoadEvents);
            await Task.Delay(3000);

            // Assert
            var statistics = await WaitForStatisticsAsync<LoadMethodStatistics>(
                LOAD_METHOD_STATISTICS_TOPIC, notLoadEvents[0].Key, TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(1));

            Assert.IsNull(statistics);
        }
    }
}

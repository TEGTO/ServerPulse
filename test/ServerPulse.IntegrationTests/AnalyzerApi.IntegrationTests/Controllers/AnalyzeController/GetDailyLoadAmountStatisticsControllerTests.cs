using AnalyzerApi.Infrastructure.Dtos.Responses.Statistics;
using EventCommunication;
using System.Net;
using System.Text.Json;

namespace AnalyzerApi.IntegrationTests.Controllers.AnalyzeController
{
    [TestFixture, Parallelizable(ParallelScope.Children)]
    internal class GetDailyLoadAmountStatisticsControllerTests : BaseIntegrationTest
    {
        private async Task MakeSamplesAsync(string key)
        {
            var loadEventSamples = new List<LoadEvent>
            {
                new LoadEvent(key, "/api/resource", "GET", 200, TimeSpan.FromMilliseconds(150), DateTime.UtcNow),
                new LoadEvent(key, "/api/resource", "POST", 201, TimeSpan.FromMilliseconds(200), DateTime.UtcNow)
            };

            await SendEventsAsync(LOAD_TOPIC, key, loadEventSamples.ToArray());
            await Task.Delay(1000);
        }

        [Test]
        public async Task GetDailyLoadStatistics_ValidRequest_ReturnsOkWithEvents()
        {
            // Arrange
            var key = Guid.NewGuid().ToString();

            // Act
            await MakeSamplesAsync(key);
            var httpResponse = await client.GetAsync($"analyze/perday/{key}");

            // Assert
            Assert.That(httpResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var content = await httpResponse.Content.ReadAsStringAsync();

            var events = JsonSerializer.Deserialize<List<LoadAmountStatisticsResponse>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            Assert.NotNull(events);
            Assert.That(events.Count, Is.GreaterThanOrEqualTo(1));
            Assert.That(events[0].CollectedDateUTC.Date, Is.EqualTo(DateTime.Now.Date));
            Assert.That(events[0].DateFrom.Date, Is.EqualTo(DateTime.Now.Date.AddDays(-1)));
            Assert.That(events[0].AmountOfEvents, Is.GreaterThanOrEqualTo(2));
        }

        [Test]
        public async Task GetDailyLoadStatistics_ValidRequest_ReturnsCachedOkWithEvents()
        {
            // Arrange
            var key = Guid.NewGuid().ToString();

            // Act
            await MakeSamplesAsync(key);
            var httpResponse = await client.GetAsync($"analyze/perday/{key}");
            await MakeSamplesAsync(key);
            var httpResponse2 = await client.GetAsync($"analyze/perday/{key}");

            // Assert
            Assert.That(httpResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(httpResponse2.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var content = await httpResponse.Content.ReadAsStringAsync();
            var content2 = await httpResponse2.Content.ReadAsStringAsync();

            var events = JsonSerializer.Deserialize<List<LoadAmountStatisticsResponse>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            var events2 = JsonSerializer.Deserialize<List<LoadAmountStatisticsResponse>>(content2, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            Assert.NotNull(events);
            Assert.NotNull(events2);

            Assert.That(events.Count, Is.GreaterThanOrEqualTo(1));
            Assert.That(events2.Count, Is.GreaterThanOrEqualTo(1));

            Assert.That(events[0].CollectedDateUTC.Date, Is.EqualTo(DateTime.Now.Date));
            Assert.That(events2[0].CollectedDateUTC.Date, Is.EqualTo(DateTime.Now.Date));

            Assert.That(events[0].DateFrom.Date, Is.EqualTo(DateTime.Now.Date.AddDays(-1)));
            Assert.That(events2[0].DateFrom.Date, Is.EqualTo(DateTime.Now.Date.AddDays(-1)));

            Assert.That(events[0].AmountOfEvents, Is.GreaterThanOrEqualTo(2));
            Assert.That(events2[0].AmountOfEvents, Is.GreaterThanOrEqualTo(2));
        }

        [Test]
        public async Task GetDailyLoadStatistics_WrongKey_ReturnsOkWithEmpty()
        {
            // Arrange
            var key = Guid.NewGuid().ToString();

            // Act
            var httpResponse = await client.GetAsync($"analyze/perday/{key}");

            // Assert
            Assert.That(httpResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var content = await httpResponse.Content.ReadAsStringAsync();
            var events = JsonSerializer.Deserialize<List<LoadAmountStatisticsResponse>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            Assert.NotNull(events);
            Assert.That(events.Count, Is.EqualTo(0));
        }
    }
}
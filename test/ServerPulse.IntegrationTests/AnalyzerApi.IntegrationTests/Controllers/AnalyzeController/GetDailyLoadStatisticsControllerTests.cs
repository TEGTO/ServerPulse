using AnalyzerApi.Infrastructure.Dtos.Responses.Statistics;
using EventCommunication;
using System.Net;
using System.Text.Json;

namespace AnalyzerApi.IntegrationTests.Controllers.AnalyzeController
{
    internal class GetDailyLoadStatisticsControllerTests : BaseIntegrationTest
    {
        const string KEY = "validKey";

        [OneTimeSetUp]
        public async Task OneTimeSetUp()
        {
            await MakeSamplesAsync();
        }

        private async Task MakeSamplesAsync()
        {
            var loadEventSamples = new List<LoadEvent>
            {
                new LoadEvent(KEY, "/api/resource", "GET", 200, TimeSpan.FromMilliseconds(150), DateTime.UtcNow),
                new LoadEvent(KEY, "/api/resource", "POST", 201, TimeSpan.FromMilliseconds(200), DateTime.UtcNow)
            };

            await SendEventsAsync(LOAD_TOPIC, KEY, loadEventSamples.ToArray());
        }

        [Test]
        public async Task GetDailyLoadStatistics_ValidRequest_ReturnsOkWithEvents()
        {
            // Act
            var httpResponse = await client.GetAsync($"analyze/perday/{KEY}");

            // Assert
            Assert.That(httpResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var content = await httpResponse.Content.ReadAsStringAsync();

            var events = JsonSerializer.Deserialize<List<LoadAmountStatisticsResponse>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            Assert.NotNull(events);
            Assert.That(events.Count, Is.EqualTo(1));
            Assert.That(events[0].CollectedDateUTC.Date, Is.EqualTo(DateTime.Now.Date));
            Assert.That(events[0].DateFrom.Date, Is.EqualTo(DateTime.Now.Date.AddDays(-1)));
            Assert.That(events[0].AmountOfEvents, Is.EqualTo(2));
        }

        [Test]
        public async Task GetDailyLoadStatistics_ValidRequest_ReturnsCachedOkWithEvents()
        {
            // Act
            var httpResponse = await client.GetAsync($"analyze/perday/{KEY}");
            await MakeSamplesAsync();
            var httpResponse2 = await client.GetAsync($"analyze/perday/{KEY}");

            // Assert
            Assert.That(httpResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(httpResponse2.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var content = await httpResponse.Content.ReadAsStringAsync();
            var content2 = await httpResponse2.Content.ReadAsStringAsync();

            var events = JsonSerializer.Deserialize<List<LoadAmountStatisticsResponse>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            var events2 = JsonSerializer.Deserialize<List<LoadAmountStatisticsResponse>>(content2, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            Assert.NotNull(events);
            Assert.NotNull(events2);

            Assert.That(events.Count, Is.EqualTo(1));
            Assert.That(events2.Count, Is.EqualTo(1));

            Assert.That(events[0].CollectedDateUTC.Date, Is.EqualTo(DateTime.Now.Date));
            Assert.That(events2[0].CollectedDateUTC.Date, Is.EqualTo(DateTime.Now.Date));

            Assert.That(events[0].DateFrom.Date, Is.EqualTo(DateTime.Now.Date.AddDays(-1)));
            Assert.That(events2[0].DateFrom.Date, Is.EqualTo(DateTime.Now.Date.AddDays(-1)));

            Assert.That(events[0].AmountOfEvents, Is.EqualTo(2));
            Assert.That(events2[0].AmountOfEvents, Is.EqualTo(2));
        }

        [Test]
        public async Task GetDailyLoadStatistics_WrongKey_ReturnsOkWithEmpty()
        {
            // Act
            var httpResponse = await client.GetAsync($"analyze/perday/wrong-key");

            // Assert
            Assert.That(httpResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var content = await httpResponse.Content.ReadAsStringAsync();
            var events = JsonSerializer.Deserialize<List<LoadAmountStatisticsResponse>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            Assert.NotNull(events);
            Assert.That(events.Count, Is.EqualTo(0));
        }
    }
}
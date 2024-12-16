using AnalyzerApi.Infrastructure.Dtos.Responses.Statistics;
using AnalyzerApi.Infrastructure.Requests;
using EventCommunication;
using System.Net;
using System.Text;
using System.Text.Json;

namespace AnalyzerApi.IntegrationTests.Controllers.AnalyzeController
{
    internal class GetLoadAmountStatisticsInRangePolicyControllerTests : BaseIntegrationTest
    {
        [Test]
        public async Task GetAmountStatisticsInRange_ValidRequest_ReturnsOkWithEvents()
        {
            // Arrange
            await Task.Delay(1000);

            var key = "key1";
            await MakeSamplesForKeyAsync(key);
            await MakeSamplesForKeyAsync(key);

            var request = new MessageAmountInRangeRequest { Key = key, From = DateTime.UtcNow.AddMinutes(-1), To = DateTime.UtcNow, TimeSpan = TimeSpan.FromMilliseconds(100) };

            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/analyze/amountrange");
            httpRequest.Content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

            // Act
            var httpResponse = await client.SendAsync(httpRequest);

            // Assert
            Assert.That(httpResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var content = await httpResponse.Content.ReadAsStringAsync();

            var events = JsonSerializer.Deserialize<List<LoadAmountStatisticsResponse>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            Assert.NotNull(events);

            Assert.That(events.Count, Is.EqualTo(2));

            Assert.True(events.All(x => x.AmountOfEvents == 2));
        }

        [Test]
        public async Task GetAmountStatisticsInRange_ValidRequest_ReturnsCachedOkWithEvents()
        {
            // Arrange
            var key = "key2";
            await MakeSamplesForKeyAsync(key);
            await MakeSamplesForKeyAsync(key);

            var request = new MessageAmountInRangeRequest { Key = key, From = DateTime.UtcNow.AddMinutes(-1), To = DateTime.UtcNow.AddMinutes(5), TimeSpan = TimeSpan.FromMilliseconds(100) };

            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/analyze/amountrange");
            using var httpRequest2 = new HttpRequestMessage(HttpMethod.Post, "/analyze/amountrange");

            httpRequest.Content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");
            httpRequest2.Content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

            // Act
            var httpResponse = await client.SendAsync(httpRequest);
            await MakeSamplesForKeyAsync(key);
            var httpResponse2 = await client.SendAsync(httpRequest2);

            // Assert
            Assert.That(httpResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(httpResponse2.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var content = await httpResponse.Content.ReadAsStringAsync();
            var content2 = await httpResponse2.Content.ReadAsStringAsync();

            var events = JsonSerializer.Deserialize<List<LoadAmountStatisticsResponse>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            var events2 = JsonSerializer.Deserialize<List<LoadAmountStatisticsResponse>>(content2, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            Assert.NotNull(events);
            Assert.NotNull(events2);

            Assert.That(events.Count, Is.EqualTo(2));
            Assert.That(events2.Count, Is.EqualTo(2));

            Assert.True(events.All(x => x.AmountOfEvents == 2));
            Assert.True(events2.All(x => x.AmountOfEvents == 2));
        }

        [Test]
        [TestCase("key3", 1, Description = "Invalid From, must be less than To.")]
        [TestCase("", 0, Description = "Invalid Key, must be not empty.")]
        [TestCase(null, 0, Description = "Invalid Key, must be not null.")]
        public async Task GetAmountStatisticsInRange_InvalidRequest_ReturnsBadRequest(string? key, int fromAddMinutes)
        {
            // Arrange
            var request = new MessageAmountInRangeRequest { Key = key!, From = DateTime.UtcNow.AddMinutes(fromAddMinutes), To = DateTime.UtcNow };

            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/analyze/amountrange");
            httpRequest.Content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

            // Act
            var httpResponse = await client.SendAsync(httpRequest);

            // Assert
            Assert.That(httpResponse.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }

        [Test]
        public async Task GetAmountStatisticsInRange_WrongKey_ReturnsOkWithEmptyArray()
        {
            // Arrange
            await MakeSamplesForKeyAsync("key4");

            var request = new MessageAmountInRangeRequest { Key = "WrongKey", From = DateTime.UtcNow.AddMinutes(-1), To = DateTime.UtcNow, TimeSpan = TimeSpan.FromMilliseconds(100) };

            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/analyze/amountrange");
            httpRequest.Content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

            // Act
            var httpResponse = await client.SendAsync(httpRequest);

            // Assert
            Assert.That(httpResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var content = await httpResponse.Content.ReadAsStringAsync();
            var events = JsonSerializer.Deserialize<List<LoadAmountStatisticsResponse>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            Assert.NotNull(events);
            Assert.That(events.Count, Is.EqualTo(0));
        }

        private async Task MakeSamplesForKeyAsync(string key)
        {
            var loadEventSamples = new List<LoadEvent>
            {
                new LoadEvent(key, "/api/resource", "GET", 200, TimeSpan.FromMilliseconds(150), DateTime.UtcNow),
                new LoadEvent(key, "/api/resource", "POST", 201, TimeSpan.FromMilliseconds(200), DateTime.UtcNow)
            };

            await SendEventsAsync(LOAD_TOPIC, key, loadEventSamples.ToArray());
        }
    }
}

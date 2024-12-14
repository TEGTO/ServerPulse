using AnalyzerApi.Infrastructure.Dtos.Responses.Statistics;
using AnalyzerApi.Infrastructure.Requests;
using EventCommunication;
using System.Net;
using System.Text;
using System.Text.Json;

namespace AnalyzerApi.IntegrationTests.Controllers.AnalyzeController
{
    internal class GetAmountStatisticsInRangeAnalyzeControllerTests : BaseIntegrationTest
    {
        const string KEY = "validKey";

        private List<LoadEvent> loadEventSamples;

        [OneTimeSetUp]
        public async Task OneTimeSetUp()
        {
            loadEventSamples = new()
            {
                new LoadEvent(KEY, "/api/resource", "GET", 200, TimeSpan.FromMilliseconds(150), DateTime.UtcNow),
                new LoadEvent(KEY, "/api/resource", "POST", 201, TimeSpan.FromMilliseconds(200), DateTime.UtcNow)
            };

            await SendEventsAsync(LOAD_TOPIC, KEY, loadEventSamples.ToArray());
        }

        [Test]
        public async Task GetAmountStatisticsInRange_ValidRequest_ReturnsOkWithEvents()
        {
            // Arrange
            await Task.Delay(1000);

            await SendEventsAsync(LOAD_TOPIC, KEY, loadEventSamples.ToArray());

            var request = new MessageAmountInRangeRequest { Key = KEY, From = DateTime.UtcNow.AddMinutes(-1), To = DateTime.UtcNow, TimeSpan = TimeSpan.FromMilliseconds(100) };

            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/analyze/amountrange");
            using var httpRequest2 = new HttpRequestMessage(HttpMethod.Post, "/analyze/amountrange");

            httpRequest.Content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");
            httpRequest2.Content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

            // Act
            var httpResponse = await client.SendAsync(httpRequest);
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
        public async Task GetAmountStatisticsInRange_InvalidRequest_ReturnsBadRequest()
        {
            // Arrange
            var request = new MessageAmountInRangeRequest { Key = KEY, From = DateTime.UtcNow.AddMinutes(1), To = DateTime.UtcNow };

            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/analyze/amountrange");
            httpRequest.Content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

            // Act
            var httpResponse = await client.SendAsync(httpRequest);

            // Assert
            Assert.That(httpResponse.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }

        [Test]
        public async Task GetAmountStatisticsInRange_InvalidKey_ReturnsOkWithEmptyArray()
        {
            // Arrange
            var request = new MessageAmountInRangeRequest { Key = "InvalidKey", From = DateTime.UtcNow.AddMinutes(-1), To = DateTime.UtcNow, TimeSpan = TimeSpan.FromMilliseconds(100) };

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
    }
}

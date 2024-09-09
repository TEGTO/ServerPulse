using AnalyzerApi.Domain.Dtos.Requests;
using AnalyzerApi.Domain.Dtos.Responses;
using ServerPulse.EventCommunication.Events;
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
            Thread.Sleep(1000);
            await SendEventsAsync(LOAD_TOPIC, KEY, loadEventSamples.ToArray());
            var getRequest = new MessageAmountInRangeRequest { Key = KEY, From = DateTime.UtcNow.AddMinutes(-1), To = DateTime.UtcNow, TimeSpan = TimeSpan.FromMilliseconds(100) };
            using var request = new HttpRequestMessage(HttpMethod.Post, "/analyze/amountrange");
            request.Content = new StringContent(JsonSerializer.Serialize(getRequest), Encoding.UTF8, "application/json");
            // Act
            var response = await client.SendAsync(request);
            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            var content = await response.Content.ReadAsStringAsync();
            var events = JsonSerializer.Deserialize<IEnumerable<LoadAmountStatisticsResponse>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            Assert.NotNull(events);
            Assert.That(events.Count(), Is.EqualTo(2));
            Assert.True(events.All(x => x.AmountOfEvents == 2));

            //Redis Tests

            // Arrange
            using var request2 = new HttpRequestMessage(HttpMethod.Post, "/analyze/amountrange");
            request2.Content = new StringContent(JsonSerializer.Serialize(getRequest), Encoding.UTF8, "application/json");
            // Act
            var response2 = await client.SendAsync(request2);
            // Assert
            Assert.That(response2.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            var content2 = await response2.Content.ReadAsStringAsync();
            var events2 = JsonSerializer.Deserialize<List<LoadAmountStatisticsResponse>>(content2, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            Assert.NotNull(events2);
            Assert.NotNull(events2);
            Assert.That(events2.Count(), Is.EqualTo(2));
            Assert.True(events2.All(x => x.AmountOfEvents == 2));
        }
        [Test]
        public async Task GetAmountStatisticsInRange_InvalidRequest_ReturnsBadRequest()
        {
            // Arrange
            var getRequest = new MessageAmountInRangeRequest { Key = KEY, From = DateTime.UtcNow.AddMinutes(1), To = DateTime.UtcNow };
            using var request = new HttpRequestMessage(HttpMethod.Post, "/analyze/amountrange");
            request.Content = new StringContent(JsonSerializer.Serialize(getRequest), Encoding.UTF8, "application/json");
            // Act
            var response = await client.SendAsync(request);
            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }
        [Test]
        public async Task GetAmountStatisticsInRange_InvalidKey_ReturnsOkWithEmptyArray()
        {
            // Arrange
            var getRequest = new MessageAmountInRangeRequest { Key = "InvalidKey", From = DateTime.UtcNow.AddMinutes(-1), To = DateTime.UtcNow, TimeSpan = TimeSpan.FromMilliseconds(100) };
            using var request = new HttpRequestMessage(HttpMethod.Post, "/analyze/amountrange");
            request.Content = new StringContent(JsonSerializer.Serialize(getRequest), Encoding.UTF8, "application/json");
            // Act
            var response = await client.SendAsync(request);
            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            var content = await response.Content.ReadAsStringAsync();
            var events = JsonSerializer.Deserialize<IEnumerable<LoadAmountStatisticsResponse>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            Assert.NotNull(events);
            Assert.That(events.Count(), Is.EqualTo(0));
        }
    }
}

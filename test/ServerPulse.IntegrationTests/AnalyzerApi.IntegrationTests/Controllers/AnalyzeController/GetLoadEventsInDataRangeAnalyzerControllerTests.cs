using AnalyzerApi.Infrastructure.Requests;
using AnalyzerApi.Infrastructure.Wrappers;
using ServerPulse.EventCommunication.Events;
using System.Net;
using System.Text;
using System.Text.Json;

namespace AnalyzerApi.IntegrationTests.Controllers.AnalyzeController
{
    internal class GetLoadEventsInDataRangeAnalyzerControllerTests : BaseIntegrationTest
    {
        const string KEY = "validKey";

        [OneTimeSetUp]
        public async Task OneTimeSetUp()
        {
            var loadEventSamples = new List<LoadEvent>
            {
                new LoadEvent(KEY, "/api/resource", "GET", 200, TimeSpan.FromMilliseconds(150), DateTime.UtcNow),
                new LoadEvent(KEY, "/api/resource", "POST", 201, TimeSpan.FromMilliseconds(200), DateTime.UtcNow)
            };

            await SendEventsAsync(LOAD_TOPIC, KEY, loadEventSamples.ToArray());
        }

        [Test]
        public async Task GetLoadEventsInDataRange_ValidRequest_ReturnsOkWithEvents()
        {
            // Arrange
            var request = new MessagesInRangeRangeRequest { Key = KEY, From = DateTime.MinValue, To = DateTime.MaxValue };

            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/analyze/daterange");
            using var httpRequest2 = new HttpRequestMessage(HttpMethod.Post, "/analyze/daterange");

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

            var events = JsonSerializer.Deserialize<List<LoadEventWrapper>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            var events2 = JsonSerializer.Deserialize<List<LoadEventWrapper>>(content2, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            Assert.NotNull(events);
            Assert.NotNull(events2);

            Assert.That(events.Count, Is.EqualTo(2));
            Assert.That(events2.Count, Is.EqualTo(2));

            Assert.True(events.Any(x => x.Method == "GET"));
            Assert.True(events2.Any(x => x.Method == "GET"));

            Assert.True(events.Any(x => x.Method == "POST"));
            Assert.True(events2.Any(x => x.Method == "POST"));
        }

        [Test]
        public async Task GetLoadEventsInDataRange_InvalidRequest_ReturnsBadRequest()
        {
            // Arrange
            var request = new MessagesInRangeRangeRequest { Key = "", From = DateTime.MinValue, To = DateTime.MinValue };

            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/analyze/daterange");
            httpRequest.Content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

            // Act
            var httpResponse = await client.SendAsync(httpRequest);

            // Assert
            Assert.That(httpResponse.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }

        [Test]
        public async Task GetLoadEventsInDataRange_InvalidKey_ReturnsOkWithEmptyArray()
        {
            // Arrange
            var request = new MessagesInRangeRangeRequest { Key = "InvalidKey", From = DateTime.MinValue, To = DateTime.MaxValue };

            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/analyze/daterange");
            httpRequest.Content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

            // Act
            var httpResponse = await client.SendAsync(httpRequest);

            // Assert
            Assert.That(httpResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var content = await httpResponse.Content.ReadAsStringAsync();
            var events = JsonSerializer.Deserialize<List<LoadEventWrapper>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            Assert.NotNull(events);
            Assert.That(events.Count, Is.EqualTo(0));
        }
    }
}

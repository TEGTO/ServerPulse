using AnalyzerApi.Infrastructure.Dtos.Endpoints.Analyze.GetLoadEventsInDataRange;
using AnalyzerApi.Infrastructure.Models.Wrappers;
using EventCommunication;
using System.Net;
using System.Text;
using System.Text.Json;

namespace AnalyzerApi.IntegrationTests.Controllers.AnalyzeController
{
    [TestFixture, Parallelizable(ParallelScope.Children)]
    internal class GetLoadEventsInDataRangeControllerTests : BaseIntegrationTest
    {
        [Test]
        public async Task GetLoadEventsInDataRange_ValidRequest_ReturnsOkWithEvents()
        {
            // Arrange
            var key = Guid.NewGuid().ToString();

            await MakeSamplesForKeyAsync(key);

            var request = new GetLoadEventsInDataRangeRequest
            {
                Key = key,
                From = DateTime.MinValue,
                To = DateTime.MaxValue
            };

            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/analyze/daterange");
            httpRequest.Content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

            // Act
            var httpResponse = await client.SendAsync(httpRequest);

            // Assert
            Assert.That(httpResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var content = await httpResponse.Content.ReadAsStringAsync();

            var events = JsonSerializer.Deserialize<List<LoadEventWrapper>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            Assert.NotNull(events);

            Assert.That(events.Count, Is.GreaterThanOrEqualTo(2));

            Assert.True(events.Any(x => x.Method == "GET"));

            Assert.True(events.Any(x => x.Method == "POST"));
        }

        [Test]
        public async Task GetLoadEventsInDataRange_ValidRequest_ReturnsCachedOkWithEvents()
        {
            // Arrange
            var key = Guid.NewGuid().ToString();

            await MakeSamplesForKeyAsync(key);

            var request = new GetLoadEventsInDataRangeRequest { Key = key, From = DateTime.MinValue, To = DateTime.MaxValue };

            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/analyze/daterange");
            using var httpRequest2 = new HttpRequestMessage(HttpMethod.Post, "/analyze/daterange");

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

            var events = JsonSerializer.Deserialize<List<LoadEventWrapper>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            var events2 = JsonSerializer.Deserialize<List<LoadEventWrapper>>(content2, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            Assert.NotNull(events);
            Assert.NotNull(events2);

            Assert.That(events.Count, Is.GreaterThanOrEqualTo(2));
            Assert.That(events2.Count, Is.GreaterThanOrEqualTo(2));

            Assert.True(events.Any(x => x.Method == "GET"));
            Assert.True(events2.Any(x => x.Method == "GET"));

            Assert.True(events.Any(x => x.Method == "POST"));
            Assert.True(events2.Any(x => x.Method == "POST"));
        }

        [Test]
        [TestCase("someRandomKey10", 1, Description = "Invalid From, must be less than To.")]
        [TestCase("", 0, Description = "Invalid Key, must be not empty.")]
        [TestCase(null, 0, Description = "Invalid Key, must be not null.")]
        public async Task GetLoadEventsInDataRange_InvalidRequest_ReturnsBadRequest(string? key, int fromAddMinutes)
        {
            // Arrange
            var request = new GetLoadEventsInDataRangeRequest { Key = key!, From = DateTime.MinValue.AddMinutes(fromAddMinutes), To = DateTime.MinValue };

            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/analyze/daterange");
            httpRequest.Content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

            // Act
            var httpResponse = await client.SendAsync(httpRequest);

            // Assert
            Assert.That(httpResponse.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }

        [Test]
        public async Task GetLoadEventsInDataRange_WrongKey_ReturnsOkWithEmptyArray()
        {
            // Arrange
            await MakeSamplesForKeyAsync(Guid.NewGuid().ToString());

            var request = new GetLoadEventsInDataRangeRequest { Key = Guid.NewGuid().ToString(), From = DateTime.MinValue, To = DateTime.MaxValue };

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

        private async Task MakeSamplesForKeyAsync(string key)
        {
            var loadEventSamples = new List<LoadEvent>
            {
                new LoadEvent(key, "/api/resource", "GET", 200, TimeSpan.FromMilliseconds(150), DateTime.UtcNow),
                new LoadEvent(key, "/api/resource", "POST", 201, TimeSpan.FromMilliseconds(200), DateTime.UtcNow)
            };

            await SendEventsAsync(LOAD_TOPIC, key, loadEventSamples.ToArray());

            await Task.Delay(1000);
        }
    }
}

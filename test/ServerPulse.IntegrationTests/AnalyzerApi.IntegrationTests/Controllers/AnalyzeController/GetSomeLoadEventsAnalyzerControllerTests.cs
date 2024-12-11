using AnalyzerApi.Infrastructure.Requests;
using AnalyzerApi.Infrastructure.Wrappers;
using EventCommunication.Events;
using System.Net;
using System.Text;
using System.Text.Json;

namespace AnalyzerApi.IntegrationTests.Controllers.AnalyzeController
{
    internal class GetSomeLoadEventsAnalyzerControllerTests : BaseIntegrationTest
    {
        const string KEY = "validKey";

        [OneTimeSetUp]
        public async Task OneTimeSetUp()
        {
            await SendEventsAsync(LOAD_TOPIC, KEY, new[]
            {
                new LoadEvent(KEY, "/api/resource", "GET", 200, TimeSpan.FromMilliseconds(150), DateTime.UtcNow)
            });

            await Task.Delay(1000);

            await SendEventsAsync(LOAD_TOPIC, KEY, new[]
            {
                 new LoadEvent(KEY, "/api/resource", "POST", 201, TimeSpan.FromMilliseconds(200), DateTime.UtcNow)
            });
        }

        [Test]
        public async Task GetSomeLoadEvents_ValidRequestToReadLastEvent_ReturnsOkWithLastEvent()
        {
            // Arrange
            var request = new GetSomeMessagesRequest { Key = KEY, NumberOfMessages = 1, StartDate = DateTime.UtcNow, ReadNew = false };

            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/analyze/someevents");
            using var httpRequest2 = new HttpRequestMessage(HttpMethod.Post, "/analyze/someevents");

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

            Assert.That(events.Count, Is.EqualTo(1));
            Assert.That(events2.Count, Is.EqualTo(1));

            Assert.True(events[0].Method == "POST");
            Assert.True(events2[0].Method == "POST");
        }

        [Test]
        public async Task GetSomeLoadEvents_ValidRequestToReadTwoLastEvent_ReturnsOkWithLastEvents()
        {
            // Arrange
            var request = new GetSomeMessagesRequest { Key = KEY, NumberOfMessages = 2, StartDate = DateTime.UtcNow, ReadNew = false };

            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/analyze/someevents");
            using var httpRequest2 = new HttpRequestMessage(HttpMethod.Post, "/analyze/someevents");

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

            Assert.True(events[0].Method == "POST");
            Assert.True(events2[0].Method == "POST");

            Assert.True(events[1].Method == "GET");
            Assert.True(events2[1].Method == "GET");
        }

        [Test]
        public async Task GetSomeLoadEvents_ValidRequestToReadFirstEvent_ReturnsOkWithFirstEvent()
        {
            // Arrange
            var request = new GetSomeMessagesRequest { Key = KEY, NumberOfMessages = 1, StartDate = DateTime.MinValue, ReadNew = true };

            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/analyze/someevents");
            using var httpRequest2 = new HttpRequestMessage(HttpMethod.Post, "/analyze/someevents");

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

            Assert.That(events.Count, Is.EqualTo(1));
            Assert.That(events2.Count, Is.EqualTo(1));

            Assert.True(events[0].Method == "GET");
            Assert.True(events2[0].Method == "GET");
        }

        [Test]
        public async Task GetSomeLoadEvents_InvalidRequest_ReturnsBadRequest()
        {
            // Arrange
            var request = new GetSomeMessagesRequest { Key = "", NumberOfMessages = 1, StartDate = DateTime.MinValue, ReadNew = true };

            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/analyze/someevents");
            httpRequest.Content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

            // Act
            var httpResponse = await client.SendAsync(httpRequest);

            // Assert
            Assert.That(httpResponse.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }

        [Test]
        public async Task GetSomeLoadEvents_InvalidKey_ReturnsEmptyArray()
        {
            // Arrange
            var request = new GetSomeMessagesRequest { Key = "InvalidKey", NumberOfMessages = 1, StartDate = DateTime.MinValue, ReadNew = true };

            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/analyze/someevents");
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

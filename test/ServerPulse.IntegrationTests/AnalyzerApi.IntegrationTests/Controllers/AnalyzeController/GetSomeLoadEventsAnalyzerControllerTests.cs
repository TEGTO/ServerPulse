using AnalyzerApi.Domain.Dtos.Requests;
using AnalyzerApi.Domain.Dtos.Wrappers;
using ServerPulse.EventCommunication.Events;
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
            Thread.Sleep(1000);
            await SendEventsAsync(LOAD_TOPIC, KEY, new[]
            {
                 new LoadEvent(KEY, "/api/resource", "POST", 201, TimeSpan.FromMilliseconds(200), DateTime.UtcNow)
            });
        }

        [Test]
        public async Task GetSomeLoadEvents_ValidRequestToReadLastEvent_ReturnsOkWithLastEvent()
        {
            // Arrange
            var getRequest = new GetSomeMessagesRequest { Key = KEY, NumberOfMessages = 1, StartDate = DateTime.UtcNow, ReadNew = false };
            using var request = new HttpRequestMessage(HttpMethod.Post, "/analyze/someevents");
            request.Content = new StringContent(JsonSerializer.Serialize(getRequest), Encoding.UTF8, "application/json");
            // Act
            var response = await client.SendAsync(request);
            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            var content = await response.Content.ReadAsStringAsync();
            var events = JsonSerializer.Deserialize<List<LoadEventWrapper>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            Assert.NotNull(events);
            Assert.That(events.Count(), Is.EqualTo(1));
            Assert.True(events[0].Method == "POST");

            //Redis Tests

            // Arrange
            using var request2 = new HttpRequestMessage(HttpMethod.Post, "/analyze/someevents");
            request2.Content = new StringContent(JsonSerializer.Serialize(getRequest), Encoding.UTF8, "application/json");
            // Act
            var response2 = await client.SendAsync(request2);
            // Assert
            Assert.That(response2.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            var content2 = await response2.Content.ReadAsStringAsync();
            var events2 = JsonSerializer.Deserialize<List<LoadEventWrapper>>(content2, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            Assert.NotNull(events2);
            Assert.That(events2.Count(), Is.EqualTo(1));
            Assert.True(events2[0].Method == "POST");
        }
        [Test]
        public async Task GetSomeLoadEvents_ValidRequestToReadTwoLastEvent_ReturnsOkWithLastEvents()
        {
            // Arrange
            var getRequest = new GetSomeMessagesRequest { Key = KEY, NumberOfMessages = 2, StartDate = DateTime.UtcNow, ReadNew = false };
            using var request = new HttpRequestMessage(HttpMethod.Post, "/analyze/someevents");
            request.Content = new StringContent(JsonSerializer.Serialize(getRequest), Encoding.UTF8, "application/json");
            // Act
            var response = await client.SendAsync(request);
            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            var content = await response.Content.ReadAsStringAsync();
            var events = JsonSerializer.Deserialize<List<LoadEventWrapper>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            Assert.NotNull(events);
            Assert.That(events.Count(), Is.EqualTo(2));
            Assert.True(events[0].Method == "POST");
            Assert.True(events[1].Method == "GET");

            //Redis Tests

            // Arrange
            using var request2 = new HttpRequestMessage(HttpMethod.Post, "/analyze/someevents");
            request2.Content = new StringContent(JsonSerializer.Serialize(getRequest), Encoding.UTF8, "application/json");
            // Act
            var response2 = await client.SendAsync(request2);
            // Assert
            Assert.That(response2.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            var content2 = await response2.Content.ReadAsStringAsync();
            var events2 = JsonSerializer.Deserialize<List<LoadEventWrapper>>(content2, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            Assert.NotNull(events2);
            Assert.That(events2.Count(), Is.EqualTo(2));
            Assert.True(events2[0].Method == "POST");
            Assert.True(events2[1].Method == "GET");
        }
        [Test]
        public async Task GetSomeLoadEvents_ValidRequestToReadFirstEvent_ReturnsOkWithFirstEvent()
        {
            // Arrange
            var getRequest = new GetSomeMessagesRequest { Key = KEY, NumberOfMessages = 1, StartDate = DateTime.MinValue, ReadNew = true };
            using var request = new HttpRequestMessage(HttpMethod.Post, "/analyze/someevents");
            request.Content = new StringContent(JsonSerializer.Serialize(getRequest), Encoding.UTF8, "application/json");
            // Act
            var response = await client.SendAsync(request);
            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            var content = await response.Content.ReadAsStringAsync();
            var events = JsonSerializer.Deserialize<List<LoadEventWrapper>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            Assert.NotNull(events);
            Assert.That(events.Count(), Is.EqualTo(1));
            Assert.True(events[0].Method == "GET");

            //Redis Tests

            // Arrange
            using var request2 = new HttpRequestMessage(HttpMethod.Post, "/analyze/someevents");
            request2.Content = new StringContent(JsonSerializer.Serialize(getRequest), Encoding.UTF8, "application/json");
            // Act
            var response2 = await client.SendAsync(request2);
            // Assert
            Assert.That(response2.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            var content2 = await response2.Content.ReadAsStringAsync();
            var events2 = JsonSerializer.Deserialize<List<LoadEventWrapper>>(content2, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            Assert.NotNull(events2);
            Assert.That(events2.Count(), Is.EqualTo(1));
            Assert.True(events2[0].Method == "GET");
        }
        [Test]
        public async Task GetSomeLoadEvents_InvalidRequest_ReturnsBadRequest()
        {
            // Arrange
            var getRequest = new GetSomeMessagesRequest { Key = "", NumberOfMessages = 1, StartDate = DateTime.MinValue, ReadNew = true };
            using var request = new HttpRequestMessage(HttpMethod.Post, "/analyze/someevents");
            request.Content = new StringContent(JsonSerializer.Serialize(getRequest), Encoding.UTF8, "application/json");
            // Act
            var response = await client.SendAsync(request);
            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }
        [Test]
        public async Task GetSomeLoadEvents_InvalidKey_ReturnsEmptyArray()
        {
            // Arrange
            var getRequest = new GetSomeMessagesRequest { Key = "InvalidKey", NumberOfMessages = 1, StartDate = DateTime.MinValue, ReadNew = true };
            using var request = new HttpRequestMessage(HttpMethod.Post, "/analyze/someevents");
            request.Content = new StringContent(JsonSerializer.Serialize(getRequest), Encoding.UTF8, "application/json");
            // Act
            var response = await client.SendAsync(request);
            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            var content = await response.Content.ReadAsStringAsync();
            var events = JsonSerializer.Deserialize<List<LoadEventWrapper>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            Assert.NotNull(events);
            Assert.That(events.Count(), Is.EqualTo(0));
        }
    }
}

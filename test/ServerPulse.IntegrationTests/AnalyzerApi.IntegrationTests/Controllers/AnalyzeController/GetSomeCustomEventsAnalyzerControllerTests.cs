using AnalyzerApi.Domain.Dtos.Requests;
using AnalyzerApi.Domain.Dtos.Wrappers;
using ServerPulse.EventCommunication.Events;
using System.Net;
using System.Text;
using System.Text.Json;

namespace AnalyzerApi.IntegrationTests.Controllers.AnalyzeController
{
    internal class GetSomeCustomEventsAnalyzerControllerTests : BaseIntegrationTest
    {
        const string KEY = "validKey";

        private readonly List<string> customEvents = new List<string>();

        [OneTimeSetUp]
        public async Task OneTimeSetUp()
        {
            customEvents.Add(JsonSerializer.Serialize(new CustomEvent(KEY, "Request1", "Description1")));
            await SendCustomEventsAsync(CUSTOM_TOPIC, KEY, new[]
            {
              customEvents[0]
            });

            Thread.Sleep(1000);

            customEvents.Add(JsonSerializer.Serialize(new CustomEvent(KEY, "Request2", "Description2")));
            await SendCustomEventsAsync(CUSTOM_TOPIC, KEY, new[]
            {
                 customEvents[1]
            });
        }

        [Test]
        public async Task GetSomeCustomEvents_ValidRequestToReadLastEvent_ReturnsOkWithLastEvent()
        {
            // Arrange
            var getRequest = new GetSomeMessagesRequest { Key = KEY, NumberOfMessages = 1, StartDate = DateTime.UtcNow, ReadNew = false };
            using var request = new HttpRequestMessage(HttpMethod.Post, "/analyze/somecustomevents");
            request.Content = new StringContent(JsonSerializer.Serialize(getRequest), Encoding.UTF8, "application/json");
            // Act
            var response = await client.SendAsync(request);
            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            var content = await response.Content.ReadAsStringAsync();
            var events = JsonSerializer.Deserialize<List<CustomEventWrapper>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            Assert.NotNull(events);
            Assert.That(events.Count(), Is.EqualTo(1));
            Assert.True(events[0].SerializedMessage == customEvents[1]);

            //Redis Tests

            // Arrange
            using var request2 = new HttpRequestMessage(HttpMethod.Post, "/analyze/somecustomevents");
            request2.Content = new StringContent(JsonSerializer.Serialize(getRequest), Encoding.UTF8, "application/json");
            // Act
            var response2 = await client.SendAsync(request2);
            // Assert
            Assert.That(response2.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            var content2 = await response2.Content.ReadAsStringAsync();
            var events2 = JsonSerializer.Deserialize<List<CustomEventWrapper>>(content2, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            Assert.NotNull(events2);
            Assert.That(events2.Count(), Is.EqualTo(1));
            Assert.True(events2[0].SerializedMessage == customEvents[1]);
        }
        [Test]
        public async Task GetSomeCustomEvents_ValidRequestToReadTwoLastEvent_ReturnsOkWithLastEvents()
        {
            // Arrange
            var getRequest = new GetSomeMessagesRequest { Key = KEY, NumberOfMessages = 2, StartDate = DateTime.UtcNow, ReadNew = false };
            using var request = new HttpRequestMessage(HttpMethod.Post, "/analyze/somecustomevents");
            request.Content = new StringContent(JsonSerializer.Serialize(getRequest), Encoding.UTF8, "application/json");
            // Act
            var response = await client.SendAsync(request);
            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            var content = await response.Content.ReadAsStringAsync();
            var events = JsonSerializer.Deserialize<List<CustomEventWrapper>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            Assert.NotNull(events);
            Assert.That(events.Count(), Is.EqualTo(2));
            Assert.True(events[0].SerializedMessage == customEvents[1]);
            Assert.True(events[1].SerializedMessage == customEvents[0]);

            //Redis Tests

            // Arrange
            using var request2 = new HttpRequestMessage(HttpMethod.Post, "/analyze/somecustomevents");
            request2.Content = new StringContent(JsonSerializer.Serialize(getRequest), Encoding.UTF8, "application/json");
            // Act
            var response2 = await client.SendAsync(request2);
            // Assert
            Assert.That(response2.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            var content2 = await response2.Content.ReadAsStringAsync();
            var events2 = JsonSerializer.Deserialize<List<CustomEventWrapper>>(content2, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            Assert.NotNull(events2);
            Assert.That(events2.Count(), Is.EqualTo(2));
            Assert.True(events2[0].SerializedMessage == customEvents[1]);
            Assert.True(events2[1].SerializedMessage == customEvents[0]);
        }
        [Test]
        public async Task GetSomeCustomEvents_ValidRequestToReadFirstEvent_ReturnsOkWithFirstEvent()
        {
            // Arrange
            var getRequest = new GetSomeMessagesRequest { Key = KEY, NumberOfMessages = 1, StartDate = DateTime.MinValue, ReadNew = true };
            using var request = new HttpRequestMessage(HttpMethod.Post, "/analyze/somecustomevents");
            request.Content = new StringContent(JsonSerializer.Serialize(getRequest), Encoding.UTF8, "application/json");
            // Act
            var response = await client.SendAsync(request);
            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            var content = await response.Content.ReadAsStringAsync();
            var events = JsonSerializer.Deserialize<List<CustomEventWrapper>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            Assert.NotNull(events);
            Assert.That(events.Count(), Is.EqualTo(1));
            Assert.True(events[0].SerializedMessage == customEvents[0]);

            //Redis Tests

            // Arrange
            using var request2 = new HttpRequestMessage(HttpMethod.Post, "/analyze/somecustomevents");
            request2.Content = new StringContent(JsonSerializer.Serialize(getRequest), Encoding.UTF8, "application/json");
            // Act
            var response2 = await client.SendAsync(request2);
            // Assert
            Assert.That(response2.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            var content2 = await response2.Content.ReadAsStringAsync();
            var events2 = JsonSerializer.Deserialize<List<CustomEventWrapper>>(content2, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            Assert.NotNull(events2);
            Assert.That(events2.Count(), Is.EqualTo(1));
            Assert.True(events2[0].SerializedMessage == customEvents[0]);
        }
        [Test]
        public async Task GetSomeCustomEvents_InvalidRequest_ReturnsBadRequest()
        {
            // Arrange
            var getRequest = new GetSomeMessagesRequest { Key = "", NumberOfMessages = 1, StartDate = DateTime.MinValue, ReadNew = true };
            using var request = new HttpRequestMessage(HttpMethod.Post, "/analyze/somecustomevents");
            request.Content = new StringContent(JsonSerializer.Serialize(getRequest), Encoding.UTF8, "application/json");
            // Act
            var response = await client.SendAsync(request);
            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }
        [Test]
        public async Task GetSomeCustomEvents_InvalidKey_ReturnsEmptyArray()
        {
            // Arrange
            var getRequest = new GetSomeMessagesRequest { Key = "InvalidKey", NumberOfMessages = 1, StartDate = DateTime.MinValue, ReadNew = true };
            using var request = new HttpRequestMessage(HttpMethod.Post, "/analyze/somecustomevents");
            request.Content = new StringContent(JsonSerializer.Serialize(getRequest), Encoding.UTF8, "application/json");
            // Act
            var response = await client.SendAsync(request);
            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            var content = await response.Content.ReadAsStringAsync();
            var events = JsonSerializer.Deserialize<List<CustomEventWrapper>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            Assert.NotNull(events);
            Assert.That(events.Count(), Is.EqualTo(0));
        }
    }
}
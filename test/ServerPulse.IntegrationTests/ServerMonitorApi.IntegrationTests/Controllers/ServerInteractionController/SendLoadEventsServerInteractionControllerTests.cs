using EventCommunication;
using Moq;
using System.Net;
using System.Text;
using System.Text.Json;

namespace ServerMonitorApi.IntegrationTests.Controllers.ServerInteractionController
{
    internal class SendLoadEventsServerInteractionControllerTests : BaseIntegrationTest
    {
        [Test]
        public async Task SendLoadEvents_ValidLoadEvents_ReturnsOkAndAddsEvents()
        {
            // Arrange
            var loadEvents = new[]
            {
                new LoadEvent("validKey", "/api/resource", "GET", 200, TimeSpan.FromMilliseconds(150), DateTime.UtcNow),
                new LoadEvent("validKey", "/api/resource", "POST", 201, TimeSpan.FromMilliseconds(200), DateTime.UtcNow)
            };

            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/serverinteraction/load");
            httpRequest.Content = new StringContent(JsonSerializer.Serialize(loadEvents), Encoding.UTF8, "application/json");

            // Act
            var httpResponse = await client.SendAsync(httpRequest);

            // Assert
            Assert.That(httpResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var lastMessage = await ReceiveLastTopicEventAsync<LoadEvent>(LOAD_TOPIC, loadEvents[0].Key);

            Assert.IsNotNull(lastMessage);
            Assert.True(loadEvents.Any(x => x.Id == lastMessage.Id)); //Each event will be sent in parallel, so we don't know which one will be the last one
            Assert.True(loadEvents.Any(x => x.Endpoint == lastMessage.Endpoint));

            mockSlotKeyChecker?.Verify(x => x.CheckSlotKeyAsync(loadEvents[0].Key, It.IsAny<CancellationToken>()), Times.Once);
            mockStatisticsEventSender?.Verify(x => x.SendLoadEventForStatistics(It.IsAny<LoadEvent>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
        }

        [Test]
        public async Task SendLoadEvents_InvalidSlotKey_ReturnsConflictAndDoesNotAddEvents()
        {
            // Arrange
            var loadEvents = new[]
            {
                new LoadEvent("invalidKey", "/api/resource", "GET", 200, TimeSpan.FromMilliseconds(150), DateTime.UtcNow)
            };

            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/serverinteraction/load");
            httpRequest.Content = new StringContent(JsonSerializer.Serialize(loadEvents), Encoding.UTF8, "application/json");

            mockSlotKeyChecker?.Setup(x => x.CheckSlotKeyAsync(loadEvents[0].Key, It.IsAny<CancellationToken>())).ReturnsAsync(false);

            // Act
            var httpResponse = await client.SendAsync(httpRequest);

            // Assert
            Assert.That(httpResponse.StatusCode, Is.EqualTo(HttpStatusCode.Conflict));

            var content = await httpResponse.Content.ReadAsStringAsync();

            var jsonResponse = JsonSerializer.Deserialize<JsonElement>(content);
            var messages = jsonResponse.GetProperty("messages").EnumerateArray().Select(m => m.GetString()).ToList();
            Assert.That(messages, Contains.Item($"Server slot with key '{loadEvents[0].Key}' is not found!"));

            mockSlotKeyChecker?.Verify(x => x.CheckSlotKeyAsync(loadEvents[0].Key, It.IsAny<CancellationToken>()), Times.Once);
            mockStatisticsEventSender?.Verify(x => x.SendLoadEventForStatistics(It.IsAny<LoadEvent>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public async Task SendLoadEvents_InvalidLoadEvents_ReturnsBadRequest()
        {
            // Arrange
            var loadEvents = new[]
            {
                new LoadEvent(null!, "/api/resource", "GET", 200, TimeSpan.FromMilliseconds(150), DateTime.UtcNow),
                new LoadEvent(null!, "/api/resource", "POST", 201, TimeSpan.FromMilliseconds(200), DateTime.UtcNow)
            };

            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/serverinteraction/load");
            httpRequest.Content = new StringContent(JsonSerializer.Serialize(loadEvents), Encoding.UTF8, "application/json");

            // Act
            var httpResponse = await client.SendAsync(httpRequest);

            // Assert
            Assert.That(httpResponse.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }

        [Test]
        public async Task SendEmptyEvents_ReturnsBadRequestAndDoesNotAddEvents()
        {
            // Arrange
            var loadEvents = new LoadEvent[0];

            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/serverinteraction/load");
            httpRequest.Content = new StringContent(JsonSerializer.Serialize(loadEvents), Encoding.UTF8, "application/json");

            // Act
            var httpResponse = await client.SendAsync(httpRequest);

            // Assert
            Assert.That(httpResponse.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));

            var content = await httpResponse.Content.ReadAsStringAsync();

            var jsonResponse = JsonSerializer.Deserialize<JsonElement>(content);
            var messages = jsonResponse.GetProperty("messages").EnumerateArray().Select(m => m.GetString()).ToList();
            Assert.That(messages, Contains.Item("Event array could not be null or empty!"));
        }

        [Test]
        public async Task SendLoadEvents_MismatchedKeys_ReturnsConflictAndDoesNotAddEvents()
        {
            // Arrange
            var loadEvents = new[]
            {
                new LoadEvent("key1", "/api/resource", "GET", 200, TimeSpan.FromMilliseconds(150), DateTime.UtcNow),
                new LoadEvent("key2", "/api/resource", "POST", 201, TimeSpan.FromMilliseconds(200), DateTime.UtcNow)
            };

            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/serverinteraction/load");
            httpRequest.Content = new StringContent(JsonSerializer.Serialize(loadEvents), Encoding.UTF8, "application/json");

            // Act
            var httpResponse = await client.SendAsync(httpRequest);

            // Assert
            Assert.That(httpResponse.StatusCode, Is.EqualTo(HttpStatusCode.Conflict));

            var content = await httpResponse.Content.ReadAsStringAsync();

            var jsonResponse = JsonSerializer.Deserialize<JsonElement>(content);
            var messages = jsonResponse.GetProperty("messages").EnumerateArray().Select(m => m.GetString()).ToList();
            Assert.That(messages, Contains.Item("All events must have the same key per request!"));
        }
    }
}
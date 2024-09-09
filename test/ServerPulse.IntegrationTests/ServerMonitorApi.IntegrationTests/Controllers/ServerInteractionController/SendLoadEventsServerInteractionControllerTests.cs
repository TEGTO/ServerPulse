using Moq;
using ServerPulse.EventCommunication.Events;
using System.Net;
using System.Text;
using System.Text.Json;

namespace ServerMonitorApi.IntegrationTests.Controllers.ServerInteractionController
{
    internal class SendLoadEventsServerInteractionControllerTests : BaseIntegrationTest
    {
        [Test]
        public async Task SendLoadEvents_ValidLoadEvents_ReturnsOk()
        {
            // Arrange
            var loadEvents = new[]
            {
                new LoadEvent("validKey", "/api/resource", "GET", 200, TimeSpan.FromMilliseconds(150), DateTime.UtcNow),
                new LoadEvent("validKey", "/api/resource", "POST", 201, TimeSpan.FromMilliseconds(200), DateTime.UtcNow)
            };
            using var request = new HttpRequestMessage(HttpMethod.Post, "/serverinteraction/load");
            request.Content = new StringContent(JsonSerializer.Serialize(loadEvents), Encoding.UTF8, "application/json");
            // Act
            var response = await client.SendAsync(request);
            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            mockSlotKeyChecker.Verify(x => x.CheckSlotKeyAsync(loadEvents.First().Key, It.IsAny<CancellationToken>()), Times.AtLeastOnce);
            mockEventProcessing.Verify(x => x.SendEventsForProcessingsAsync(loadEvents, It.IsAny<CancellationToken>()), Times.AtLeastOnce);
            var lastMessage = await ReceiveLastTopicEventAsync<LoadEvent>(LOAD_TOPIC, loadEvents.First().Key);
            Assert.True(loadEvents.Any(x => x.Id == lastMessage.Id)); //Each event will be sent in parallel, so we don't know which one will be the last one
            Assert.True(loadEvents.Any(x => x.Endpoint == lastMessage.Endpoint));
        }
        [Test]
        public async Task SendLoadEvents_InvalidSlotKey_ReturnsNotFound()
        {
            // Arrange
            var loadEvents = new[]
            {
                new LoadEvent("invalidKey", "/api/resource", "GET", 200, TimeSpan.FromMilliseconds(150), DateTime.UtcNow)
            };
            using var request = new HttpRequestMessage(HttpMethod.Post, "/serverinteraction/load");
            request.Content = new StringContent(JsonSerializer.Serialize(loadEvents), Encoding.UTF8, "application/json");
            mockSlotKeyChecker.Setup(x => x.CheckSlotKeyAsync(loadEvents.First().Key, It.IsAny<CancellationToken>())).ReturnsAsync(false);
            // Act
            var response = await client.SendAsync(request);
            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
            mockSlotKeyChecker.Verify(x => x.CheckSlotKeyAsync(loadEvents.First().Key, It.IsAny<CancellationToken>()), Times.AtLeastOnce);
            var content = await response.Content.ReadAsStringAsync();
            Assert.That(content, Is.EqualTo($"Server slot with key '{loadEvents.First().Key}' is not found!"));
        }
        [Test]
        public async Task SendLoadEvents_MismatchedKeys_ReturnsBadRequest()
        {
            // Arrange
            var loadEvents = new[]
            {
                new LoadEvent("key1", "/api/resource", "GET", 200, TimeSpan.FromMilliseconds(150), DateTime.UtcNow),
                new LoadEvent("key2", "/api/resource", "POST", 201, TimeSpan.FromMilliseconds(200), DateTime.UtcNow)
            };
            using var request = new HttpRequestMessage(HttpMethod.Post, "/serverinteraction/load");
            request.Content = new StringContent(JsonSerializer.Serialize(loadEvents), Encoding.UTF8, "application/json");
            // Act
            var response = await client.SendAsync(request);
            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            var content = await response.Content.ReadAsStringAsync();
            Assert.That(content, Is.EqualTo("All load events must have the same key per request!"));
        }
        [Test]
        public async Task SendLoadEvents_InvalidLoadEvents_ReturnsBadRequest()
        {
            // Arrange
            var loadEvents = new[]
            {
                new LoadEvent(null, "/api/resource", "GET", 200, TimeSpan.FromMilliseconds(150), DateTime.UtcNow),
                new LoadEvent(null, "/api/resource", "POST", 201, TimeSpan.FromMilliseconds(200), DateTime.UtcNow)
            };
            using var request = new HttpRequestMessage(HttpMethod.Post, "/serverinteraction/load");
            request.Content = new StringContent(JsonSerializer.Serialize(loadEvents), Encoding.UTF8, "application/json");
            // Act
            var response = await client.SendAsync(request);
            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }
    }
}
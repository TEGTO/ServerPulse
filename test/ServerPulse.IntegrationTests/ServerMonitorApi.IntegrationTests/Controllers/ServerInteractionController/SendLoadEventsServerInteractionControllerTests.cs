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
            var lastProcessMessage = await ReceiveLastTopicEventAsync<LoadEvent>(LOAD_PROCESS_TOPIC, "");

            Assert.IsNotNull(lastMessage);
            Assert.IsNotNull(lastProcessMessage);

            Assert.True(loadEvents.Any(x => x.Id == lastMessage.Id)); //Each event will be sent in parallel, so we don't know which one will be the last one
            Assert.True(loadEvents.Any(x => x.Id == lastProcessMessage.Id));

            Assert.True(loadEvents.Any(x => x.Endpoint == lastMessage.Endpoint));
            Assert.True(loadEvents.Any(x => x.Endpoint == lastProcessMessage.Endpoint));

            mockSlotKeyChecker?.Verify(x => x.CheckSlotKeyAsync(loadEvents[0].Key, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task SendLoadEvents_InvalidSlotKey_ReturnsBadRequestAndDoesNotAddEvents()
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
            Assert.That(httpResponse.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));

            var content = await httpResponse.Content.ReadAsStringAsync();

            Assert.That(content, Is.EqualTo($"Server slot with key '{loadEvents[0].Key}' is not found!"));

            mockSlotKeyChecker?.Verify(x => x.CheckSlotKeyAsync(loadEvents[0].Key, It.IsAny<CancellationToken>()), Times.Once);
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

            Assert.That(content, Is.EqualTo("Event array could not be null or empty!"));
        }

        [Test]
        public async Task SendLoadEvents_MismatchedKeys_ReturnsBadRequestAndDoesNotAddEvents()
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
            Assert.That(httpResponse.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));

            var content = await httpResponse.Content.ReadAsStringAsync();

            Assert.That(content, Is.EqualTo("All events must have the same key per request!"));
        }
    }
}
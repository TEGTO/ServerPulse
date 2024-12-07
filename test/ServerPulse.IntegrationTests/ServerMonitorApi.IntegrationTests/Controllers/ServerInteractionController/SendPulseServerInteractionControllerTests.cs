using Moq;
using ServerPulse.EventCommunication.Events;
using System.Net;
using System.Text;
using System.Text.Json;

namespace ServerMonitorApi.IntegrationTests.Controllers.ServerInteractionController
{
    internal class SendPulseServerInteractionControllerTests : BaseIntegrationTest
    {
        [Test]
        public async Task SendPulse_ValidPulseEvent_ReturnsOkAndAddsEvent()
        {
            // Arrange
            var pulseEvent = new PulseEvent("validKey", true);

            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/serverinteraction/pulse");
            httpRequest.Content = new StringContent(JsonSerializer.Serialize(pulseEvent), Encoding.UTF8, "application/json");

            // Act
            var httpResponse = await client.SendAsync(httpRequest);

            // Assert
            Assert.That(httpResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var lastMessage = await ReceiveLastTopicEventAsync<PulseEvent>(ALIVE_TOPIC, pulseEvent.Key);

            Assert.IsNotNull(lastMessage);
            Assert.That(lastMessage.Id, Is.EqualTo(pulseEvent.Id));
            Assert.True(lastMessage.IsAlive);

            mockSlotKeyChecker?.Verify(x => x.CheckSlotKeyAsync(pulseEvent.Key, It.IsAny<CancellationToken>()), Times.AtLeastOnce);
        }

        [Test]
        public async Task SendPulse_InvalidSlotKey_ReturnsConflictAndDoesNotAddEvent()
        {
            // Arrange
            var pulseEvent = new PulseEvent("invalidKey", true);

            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/serverinteraction/pulse");
            httpRequest.Content = new StringContent(JsonSerializer.Serialize(pulseEvent), Encoding.UTF8, "application/json");

            mockSlotKeyChecker?.Setup(x => x.CheckSlotKeyAsync(pulseEvent.Key, It.IsAny<CancellationToken>())).ReturnsAsync(false);

            // Act
            var httpResponse = await client.SendAsync(httpRequest);

            // Assert
            Assert.That(httpResponse.StatusCode, Is.EqualTo(HttpStatusCode.Conflict));

            var content = await httpResponse.Content.ReadAsStringAsync();

            var jsonResponse = JsonSerializer.Deserialize<JsonElement>(content);
            var messages = jsonResponse.GetProperty("messages").EnumerateArray().Select(m => m.GetString()).ToList();
            Assert.That(messages, Contains.Item($"Server slot with key '{pulseEvent.Key}' is not found!"));

            mockSlotKeyChecker?.Verify(x => x.CheckSlotKeyAsync(pulseEvent.Key, It.IsAny<CancellationToken>()), Times.AtLeastOnce);
        }

        [Test]
        public async Task SendPulse_InvalidPulse_ReturnsBadRequest()
        {
            // Arrange
            var pulseEvent = new PulseEvent(null!, true);

            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/serverinteraction/pulse");
            httpRequest.Content = new StringContent(JsonSerializer.Serialize(pulseEvent), Encoding.UTF8, "application/json");

            // Act
            var httpResponse = await client.SendAsync(httpRequest);

            // Assert
            Assert.That(httpResponse.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }
    }
}

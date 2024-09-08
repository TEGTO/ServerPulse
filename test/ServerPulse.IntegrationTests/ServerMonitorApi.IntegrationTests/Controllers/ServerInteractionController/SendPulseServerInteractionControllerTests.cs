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
        public async Task SendPulse_ValidPulseEvent_ReturnsOk()
        {
            // Arrange
            var pulseEvent = new PulseEvent("validKey", true);
            using var request = new HttpRequestMessage(HttpMethod.Post, "/serverinteraction/pulse");
            request.Content = new StringContent(JsonSerializer.Serialize(pulseEvent), Encoding.UTF8, "application/json");
            // Act
            var response = await client.SendAsync(request);
            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            mockSlotKeyChecker.Verify(x => x.CheckSlotKeyAsync(pulseEvent.Key, It.IsAny<CancellationToken>()), Times.AtLeastOnce);
            var lastMessage = await ReceiveLastTopicEventByKeyAsync<PulseEvent>(ALIVE_TOPIC, pulseEvent.Key);
            Assert.That(lastMessage.Id, Is.EqualTo(pulseEvent.Id));
            Assert.True(lastMessage.IsAlive);
        }
        [Test]
        public async Task SendPulse_InvalidSlotKey_ReturnsNotFound()
        {
            // Arrange
            var pulseEvent = new PulseEvent("invalidKey", true);
            using var request = new HttpRequestMessage(HttpMethod.Post, "/serverinteraction/pulse");
            request.Content = new StringContent(JsonSerializer.Serialize(pulseEvent), Encoding.UTF8, "application/json");
            mockSlotKeyChecker.Setup(x => x.CheckSlotKeyAsync(pulseEvent.Key, It.IsAny<CancellationToken>())).ReturnsAsync(false);
            // Act
            var response = await client.SendAsync(request);
            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
            mockSlotKeyChecker.Verify(x => x.CheckSlotKeyAsync(pulseEvent.Key, It.IsAny<CancellationToken>()), Times.AtLeastOnce);
            var content = await response.Content.ReadAsStringAsync();
            Assert.That(content, Is.EqualTo($"Server slot with key '{pulseEvent.Key}' is not found!"));
        }
        [Test]
        public async Task SendPulse_InvalidPulse_ReturnsBadRequest()
        {
            // Arrange
            var pulseEvent = new PulseEvent(null, true);
            using var request = new HttpRequestMessage(HttpMethod.Post, "/serverinteraction/pulse");
            request.Content = new StringContent(JsonSerializer.Serialize(pulseEvent), Encoding.UTF8, "application/json");
            // Act
            var response = await client.SendAsync(request);
            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }
    }
}

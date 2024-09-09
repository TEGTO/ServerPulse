using Moq;
using ServerPulse.EventCommunication.Events;
using System.Net;
using System.Text;
using System.Text.Json;

namespace ServerMonitorApi.IntegrationTests.Controllers.ServerInteractionController
{
    internal class SendConfigurationServerInteractionControllerTests : BaseIntegrationTest
    {
        [Test]
        public async Task SendConfiguration_ValidConfigurationEvent_ReturnsOk()
        {
            // Arrange
            var configurationEvent = new ConfigurationEvent("validKey", TimeSpan.FromMinutes(5));
            using var request = new HttpRequestMessage(HttpMethod.Post, "/serverinteraction/configuration");
            request.Content = new StringContent(JsonSerializer.Serialize(configurationEvent), Encoding.UTF8, "application/json");
            // Act
            var response = await client.SendAsync(request);
            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            mockSlotKeyChecker.Verify(x => x.CheckSlotKeyAsync(configurationEvent.Key, It.IsAny<CancellationToken>()), Times.AtLeastOnce);
            var lastMessage = await ReceiveLastTopicEventAsync<ConfigurationEvent>(CONFIGURATION_TOPIC, configurationEvent.Key);
            Assert.That(lastMessage.Id, Is.EqualTo(configurationEvent.Id));
            Assert.That(lastMessage.ServerKeepAliveInterval, Is.EqualTo(TimeSpan.FromMinutes(5)));
        }
        [Test]
        public async Task SendConfiguration_InvalidSlotKey_ReturnsNotFound()
        {
            // Arrange
            var configurationEvent = new ConfigurationEvent("invalidKey", TimeSpan.FromMinutes(5));
            using var request = new HttpRequestMessage(HttpMethod.Post, "/serverinteraction/configuration");
            request.Content = new StringContent(JsonSerializer.Serialize(configurationEvent), Encoding.UTF8, "application/json");
            mockSlotKeyChecker.Setup(x => x.CheckSlotKeyAsync(configurationEvent.Key, It.IsAny<CancellationToken>())).ReturnsAsync(false);
            // Act
            var response = await client.SendAsync(request);
            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
            mockSlotKeyChecker.Verify(x => x.CheckSlotKeyAsync(configurationEvent.Key, It.IsAny<CancellationToken>()), Times.AtLeastOnce);
            var content = await response.Content.ReadAsStringAsync();
            Assert.That(content, Is.EqualTo($"Server slot with key '{configurationEvent.Key}' is not found!"));
        }
        [Test]
        public async Task SendConfiguration_InvalidConfigurationEvent_ReturnsBadRequest()
        {
            // Arrange
            var configurationEvent = new ConfigurationEvent(null, TimeSpan.FromMinutes(5));
            using var request = new HttpRequestMessage(HttpMethod.Post, "/serverinteraction/configuration");
            request.Content = new StringContent(JsonSerializer.Serialize(configurationEvent), Encoding.UTF8, "application/json");
            // Act
            var response = await client.SendAsync(request);
            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }
    }
}

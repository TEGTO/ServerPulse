using EventCommunication;
using Moq;
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

            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/serverinteraction/configuration");
            httpRequest.Content = new StringContent(JsonSerializer.Serialize(configurationEvent), Encoding.UTF8, "application/json");

            // Act
            var httpResponse = await client.SendAsync(httpRequest);

            // Assert
            Assert.That(httpResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var lastMessage = await ReceiveLastTopicEventAsync<ConfigurationEvent>(CONFIGURATION_TOPIC, configurationEvent.Key);

            Assert.IsNotNull(lastMessage);
            Assert.That(lastMessage.Id, Is.EqualTo(configurationEvent.Id));
            Assert.That(lastMessage.ServerKeepAliveInterval, Is.EqualTo(TimeSpan.FromMinutes(5)));

            mockSlotKeyChecker?.Verify(x => x.CheckSlotKeyAsync(configurationEvent.Key, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task SendConfiguration_InvalidSlotKey_ReturnsBadRequest()
        {
            // Arrange
            var configurationEvent = new ConfigurationEvent("invalidKey", TimeSpan.FromMinutes(5));

            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/serverinteraction/configuration");
            httpRequest.Content = new StringContent(JsonSerializer.Serialize(configurationEvent), Encoding.UTF8, "application/json");

            mockSlotKeyChecker?.Setup(x => x.CheckSlotKeyAsync(configurationEvent.Key, It.IsAny<CancellationToken>())).ReturnsAsync(false);

            // Act
            var httpResponse = await client.SendAsync(httpRequest);

            // Assert
            Assert.That(httpResponse.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));

            var content = await httpResponse.Content.ReadAsStringAsync();

            Assert.That(content, Is.EqualTo($"Server slot with key '{configurationEvent.Key}' is not found!"));

            mockSlotKeyChecker?.Verify(x => x.CheckSlotKeyAsync(configurationEvent.Key, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task SendConfiguration_InvalidConfigurationEvent_ReturnsBadRequest()
        {
            // Arrange
            var configurationEvent = new ConfigurationEvent(null!, TimeSpan.FromMinutes(5));

            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/serverinteraction/configuration");
            httpRequest.Content = new StringContent(JsonSerializer.Serialize(configurationEvent), Encoding.UTF8, "application/json");

            // Act
            var httpResponse = await client.SendAsync(httpRequest);

            // Assert
            Assert.That(httpResponse.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }
    }
}

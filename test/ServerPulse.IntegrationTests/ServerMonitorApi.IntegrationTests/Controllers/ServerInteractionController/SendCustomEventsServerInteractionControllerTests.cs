using Moq;
using ServerPulse.EventCommunication;
using ServerPulse.EventCommunication.Events;
using System.Net;
using System.Text;
using System.Text.Json;

namespace ServerMonitorApi.IntegrationTests.Controllers.ServerInteractionController
{
    internal class SendCustomEventsServerInteractionControllerTests : BaseIntegrationTest
    {
        [Test]
        public async Task SendCustomEvents_ValidCustomEvents_ReturnsOk()
        {
            // Arrange
            var ev1 = new CustomEvent("validKey", "CustomEvent1", "Description1");
            var ev2 = new CustomEvent("validKey", "CustomEvent2", "Description2");
            var customEventWrappers = new[]
            {
                new CustomEventWrapper(ev1, JsonSerializer.Serialize(ev1)),
                new CustomEventWrapper(ev2, JsonSerializer.Serialize(ev2))
            };
            using var request = new HttpRequestMessage(HttpMethod.Post, "/serverinteraction/custom");
            request.Content = new StringContent(JsonSerializer.Serialize(customEventWrappers), Encoding.UTF8, "application/json");
            // Act
            var response = await client.SendAsync(request);
            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            mockSlotKeyChecker.Verify(x => x.CheckSlotKeyAsync(customEventWrappers.First().CustomEvent.Key, It.IsAny<CancellationToken>()), Times.AtLeastOnce);
            var lastMessage = await ReceiveLastTopicEventByKeyAsync<CustomEvent>(CUSTOM_TOPIC, customEventWrappers.First().CustomEvent.Key);
            Assert.True(customEventWrappers.Any(x => x.CustomEvent.Id == lastMessage.Id)); //Each event will be sent in parallel, so we don't know which one will be the last one
            Assert.True(customEventWrappers.Any(x => x.CustomEvent.Name == lastMessage.Name));
        }
        [Test]
        public async Task SendCustomEvents_ValidTestCustomEvents_ReturnsOk()
        {
            // Arrange
            var ev1 = new TestCustomEvent("validKey", "CustomEvent1", "Description1", "add1", 0);
            var ev2 = new TestCustomEvent("validKey", "CustomEvent2", "Description2", "add2", 1);
            var customEventWrappers = new[]
            {
                new CustomEventWrapper(ev1, JsonSerializer.Serialize(ev1)),
                new CustomEventWrapper(ev2, JsonSerializer.Serialize(ev2))
            };
            using var request = new HttpRequestMessage(HttpMethod.Post, "/serverinteraction/custom");
            request.Content = new StringContent(JsonSerializer.Serialize(customEventWrappers), Encoding.UTF8, "application/json");
            // Act
            var response = await client.SendAsync(request);
            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            mockSlotKeyChecker.Verify(x => x.CheckSlotKeyAsync(customEventWrappers.First().CustomEvent.Key, It.IsAny<CancellationToken>()), Times.AtLeastOnce);
            var lastMessage = await ReceiveLastTopicEventByKeyAsync<TestCustomEvent>(CUSTOM_TOPIC, customEventWrappers.First().CustomEvent.Key);
            Assert.True(lastMessage.AdditionalField1 == "add1" || lastMessage.AdditionalField1 == "add2");
            Assert.True(lastMessage.AdditionalField2 == 0 || lastMessage.AdditionalField2 == 1);
        }
        [Test]
        public async Task SendCustomEvents_InvalidSlotKey_ReturnsNotFound()
        {
            // Arrange
            var ev1 = new CustomEvent("invalidKey", "CustomEvent1", "Description1");
            var customEventWrappers = new[]
            {
               new CustomEventWrapper(ev1, JsonSerializer.Serialize(ev1))
            };
            using var request = new HttpRequestMessage(HttpMethod.Post, "/serverinteraction/custom");
            request.Content = new StringContent(JsonSerializer.Serialize(customEventWrappers), Encoding.UTF8, "application/json");
            mockSlotKeyChecker.Setup(x => x.CheckSlotKeyAsync(customEventWrappers.First().CustomEvent.Key, It.IsAny<CancellationToken>())).ReturnsAsync(false);
            // Act
            var response = await client.SendAsync(request);
            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
            mockSlotKeyChecker.Verify(x => x.CheckSlotKeyAsync(customEventWrappers.First().CustomEvent.Key, It.IsAny<CancellationToken>()), Times.AtLeastOnce);
            var content = await response.Content.ReadAsStringAsync();
            Assert.That(content, Is.EqualTo($"Server slot with key '{customEventWrappers.First().CustomEvent.Key}' is not found!"));
        }
        [Test]
        public async Task SendCustomEvents_InvalidEvent_ReturnsBadRequest()
        {
            // Arrange
            var customEventWrappers = new[]
           {
                new CustomEventWrapper(null, null)
            };
            using var request = new HttpRequestMessage(HttpMethod.Post, "/serverinteraction/custom");
            request.Content = new StringContent(JsonSerializer.Serialize(customEventWrappers), Encoding.UTF8, "application/json");
            // Act
            var response = await client.SendAsync(request);
            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }
        [Test]
        public async Task SendCustomEvents_MismatchedKeys_ReturnsBadRequest()
        {
            // Arrange
            var ev1 = new TestCustomEvent("key1", "CustomEvent1", "Description1", "add1", 0);
            var ev2 = new TestCustomEvent("key2", "CustomEvent2", "Description2", "add2", 1);
            var customEventWrappers = new[]
            {
                new CustomEventWrapper(ev1, JsonSerializer.Serialize(ev1)),
                new CustomEventWrapper(ev2, JsonSerializer.Serialize(ev2))
            };
            using var request = new HttpRequestMessage(HttpMethod.Post, "/serverinteraction/custom");
            request.Content = new StringContent(JsonSerializer.Serialize(customEventWrappers), Encoding.UTF8, "application/json");
            // Act
            var response = await client.SendAsync(request);
            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            var content = await response.Content.ReadAsStringAsync();
            Assert.That(content, Is.EqualTo("All custom events must have the same key per request!"));
        }
    }
    public record class TestCustomEvent(string Key, string Name, string Description,
        string AdditionalField1, int AdditionalField2) : CustomEvent(Key, Name, Description);
}
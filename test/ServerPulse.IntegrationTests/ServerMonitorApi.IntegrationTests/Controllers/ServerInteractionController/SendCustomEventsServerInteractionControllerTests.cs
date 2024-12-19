using EventCommunication;
using Moq;
using System.Net;
using System.Text;
using System.Text.Json;

namespace ServerMonitorApi.IntegrationTests.Controllers.ServerInteractionController
{
    internal class SendCustomEventsServerInteractionControllerTests : BaseIntegrationTest
    {
        [Test]
        public async Task SendCustomEvents_ValidCustomEvents_ReturnsOkAndAddsEvents()
        {
            // Arrange
            var ev1 = new CustomEvent("validKey", "CustomEvent1", "Description1");
            var ev2 = new CustomEvent("validKey", "CustomEvent2", "Description2");

            var customEventWrappers = new[]
            {
                new CustomEventContainer(ev1, JsonSerializer.Serialize(ev1)),
                new CustomEventContainer(ev2, JsonSerializer.Serialize(ev2))
            };

            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/serverinteraction/custom");
            httpRequest.Content = new StringContent(JsonSerializer.Serialize(customEventWrappers), Encoding.UTF8, "application/json");

            // Act
            var httpResponse = await client.SendAsync(httpRequest);

            // Assert
            Assert.That(httpResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var lastMessage = await ReceiveLastTopicEventAsync<CustomEvent>(CUSTOM_TOPIC, customEventWrappers[0].CustomEvent.Key);

            Assert.IsNotNull(lastMessage);
            Assert.True(customEventWrappers.Any(x => x.CustomEvent.Id == lastMessage.Id)); //Each event will be sent in parallel, so we don't know which one will be the last one
            Assert.True(customEventWrappers.Any(x => x.CustomEvent.Name == lastMessage.Name));

            mockSlotKeyChecker?.Verify(x => x.CheckSlotKeyAsync(customEventWrappers[0].CustomEvent.Key, It.IsAny<CancellationToken>()), Times.AtLeastOnce);
        }

        [Test]
        public async Task SendCustomEvents_ValidCustomEventHeirs_ReturnsOkAndAddsEvents()
        {
            // Arrange
            var ev1 = new TestCustomEvent("validKey", "CustomEvent1", "Description1", "add1", 0);
            var ev2 = new TestCustomEvent("validKey", "CustomEvent2", "Description2", "add2", 1);

            var customEventWrappers = new[]
            {
                new CustomEventContainer(ev1, JsonSerializer.Serialize(ev1)),
                new CustomEventContainer(ev2, JsonSerializer.Serialize(ev2))
            };

            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/serverinteraction/custom");
            httpRequest.Content = new StringContent(JsonSerializer.Serialize(customEventWrappers), Encoding.UTF8, "application/json");

            // Act
            var httpResponse = await client.SendAsync(httpRequest);

            // Assert
            Assert.That(httpResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var lastMessage = await ReceiveLastTopicEventAsync<TestCustomEvent>(CUSTOM_TOPIC, customEventWrappers[0].CustomEvent.Key);

            Assert.IsNotNull(lastMessage);
            Assert.True(lastMessage.AdditionalField1 == "add1" || lastMessage.AdditionalField1 == "add2");
            Assert.True(lastMessage.AdditionalField2 == 0 || lastMessage.AdditionalField2 == 1);

            mockSlotKeyChecker?.Verify(x => x.CheckSlotKeyAsync(customEventWrappers[0].CustomEvent.Key, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task SendCustomEvents_InvalidSlotKey_ReturnsConflictAndDoesNotAddEvents()
        {
            // Arrange
            var ev1 = new CustomEvent("invalidKey", "CustomEvent1", "Description1");

            var customEventWrappers = new[]
            {
               new CustomEventContainer(ev1, JsonSerializer.Serialize(ev1))
            };

            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/serverinteraction/custom");
            httpRequest.Content = new StringContent(JsonSerializer.Serialize(customEventWrappers), Encoding.UTF8, "application/json");

            mockSlotKeyChecker?.Setup(x => x.CheckSlotKeyAsync(customEventWrappers[0].CustomEvent.Key, It.IsAny<CancellationToken>())).ReturnsAsync(false);

            // Act
            var httpResponse = await client.SendAsync(httpRequest);

            // Assert
            Assert.That(httpResponse.StatusCode, Is.EqualTo(HttpStatusCode.Conflict));

            var content = await httpResponse.Content.ReadAsStringAsync();

            var jsonResponse = JsonSerializer.Deserialize<JsonElement>(content);
            var messages = jsonResponse.GetProperty("messages").EnumerateArray().Select(m => m.GetString()).ToList();
            Assert.That(messages, Contains.Item($"Server slot with key '{customEventWrappers[0].CustomEvent.Key}' is not found!"));

            mockSlotKeyChecker?.Verify(x => x.CheckSlotKeyAsync(customEventWrappers[0].CustomEvent.Key, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task SendCustomEvents_InvalidEvent_ReturnsBadRequest()
        {
            // Arrange
            var customEventWrappers = new[]
            {
                new CustomEventContainer(null!, null!)
            };

            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/serverinteraction/custom");
            httpRequest.Content = new StringContent(JsonSerializer.Serialize(customEventWrappers), Encoding.UTF8, "application/json");

            // Act
            var httpResponse = await client.SendAsync(httpRequest);

            // Assert
            Assert.That(httpResponse.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }

        [Test]
        public async Task SendEmptyCustomEvents_ReturnsBadRequestAndDoesNotAddEvents()
        {
            // Arrange
            var customEventWrappers = new TestCustomEvent[0];

            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/serverinteraction/custom");
            httpRequest.Content = new StringContent(JsonSerializer.Serialize(customEventWrappers), Encoding.UTF8, "application/json");

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
        public async Task SendCustomEvents_MismatchedKeys_ReturnsConflictAndDoesNotAddEvents()
        {
            // Arrange
            var ev1 = new TestCustomEvent("key1", "CustomEvent1", "Description1", "add1", 0);
            var ev2 = new TestCustomEvent("key2", "CustomEvent2", "Description2", "add2", 1);

            var customEventWrappers = new[]
            {
                new CustomEventContainer(ev1, JsonSerializer.Serialize(ev1)),
                new CustomEventContainer(ev2, JsonSerializer.Serialize(ev2))
            };

            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/serverinteraction/custom");
            httpRequest.Content = new StringContent(JsonSerializer.Serialize(customEventWrappers), Encoding.UTF8, "application/json");

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

    public record class TestCustomEvent(
        string Key,
        string Name,
        string Description,
        string AdditionalField1,
        int AdditionalField2) : CustomEvent(Key, Name, Description);
}
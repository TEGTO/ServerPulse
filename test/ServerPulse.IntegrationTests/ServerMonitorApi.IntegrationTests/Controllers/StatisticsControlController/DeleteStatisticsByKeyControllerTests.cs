using ServerPulse.EventCommunication;
using ServerPulse.EventCommunication.Events;
using System.Net;
using System.Text;
using System.Text.Json;

namespace ServerMonitorApi.IntegrationTests.Controllers.StatisticsControlController
{
    internal class DeleteStatisticsByKeyControllerTests : BaseIntegrationTest
    {
        private const string KEY = "key";

        [SetUp]
        public async Task SetUp()
        {
            var loadEvents = new[]
             {
                new LoadEvent(KEY, "/api/resource", "GET", 200, TimeSpan.FromMilliseconds(150), DateTime.UtcNow),
                new LoadEvent(KEY, "/api/resource", "POST", 201, TimeSpan.FromMilliseconds(200), DateTime.UtcNow)
            };
            var configurationEvent = new ConfigurationEvent(KEY, TimeSpan.FromMinutes(5));
            var ev1 = new CustomEvent(KEY, "CustomEvent1", "Description1");
            var ev2 = new CustomEvent(KEY, "CustomEvent2", "Description2");
            var customEventWrappers = new[]
            {
                new CustomEventWrapper(ev1, JsonSerializer.Serialize(ev1)),
                new CustomEventWrapper(ev2, JsonSerializer.Serialize(ev2))
            };
            var pulseEvent = new PulseEvent(KEY, true);
            var tasks = new List<Task>
            {
                SendEvent(JsonSerializer.Serialize(loadEvents), "/serverinteraction/load"),
                SendEvent(JsonSerializer.Serialize(configurationEvent), "/serverinteraction/configuration"),
                SendEvent(JsonSerializer.Serialize(customEventWrappers), "/serverinteraction/custom"),
                SendEvent(JsonSerializer.Serialize(pulseEvent), "/serverinteraction/pulse"),
            };
            await Task.WhenAll(tasks);
        }

        [Test]
        public async Task DeleteStatisticsByKey_ValidKey_ReturnsOKWithDeletedStatistics()
        {
            // Act 
            var response = await client.DeleteAsync($"/statisticscontrol/{KEY}");
            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            var loadEventTask = ReceiveLastTopicEventByKeyAsync<LoadEvent>(LOAD_TOPIC, KEY);
            var pulseEventTask = ReceiveLastTopicEventByKeyAsync<LoadEvent>(ALIVE_TOPIC, KEY);
            var customEventTask = ReceiveLastTopicEventByKeyAsync<LoadEvent>(CUSTOM_TOPIC, KEY);
            var configurationEventTask = ReceiveLastTopicEventByKeyAsync<LoadEvent>(CONFIGURATION_TOPIC, KEY);
            var tasks = new List<Task>
            {
                loadEventTask,
                pulseEventTask,
                customEventTask,
                configurationEventTask
            };
            await Task.WhenAll(tasks);
            Assert.That(await loadEventTask, Is.Null);
            Assert.That(await pulseEventTask, Is.Null);
            Assert.That(await customEventTask, Is.Null);
            Assert.That(await configurationEventTask, Is.Null);
        }
        [Test]
        public async Task DeleteStatisticsByKey_InvalidKey_ReturnsOKWithNotDeletedStatistics()
        {
            // Act 
            var response = await client.DeleteAsync($"/statisticscontrol/invalidKey");
            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            var loadEventTask = ReceiveLastTopicEventByKeyAsync<LoadEvent>(LOAD_TOPIC, KEY);
            var pulseEventTask = ReceiveLastTopicEventByKeyAsync<LoadEvent>(ALIVE_TOPIC, KEY);
            var customEventTask = ReceiveLastTopicEventByKeyAsync<LoadEvent>(CUSTOM_TOPIC, KEY);
            var configurationEventTask = ReceiveLastTopicEventByKeyAsync<LoadEvent>(CONFIGURATION_TOPIC, KEY);
            var tasks = new List<Task>
            {
                loadEventTask,
                pulseEventTask,
                customEventTask,
                configurationEventTask
            };
            await Task.WhenAll(tasks);
            Assert.That(await loadEventTask, Is.Not.Null);
            Assert.That(await pulseEventTask, Is.Not.Null);
            Assert.That(await customEventTask, Is.Not.Null);
            Assert.That(await configurationEventTask, Is.Not.Null);
        }

        private async Task SendEvent(string serializedEvent, string path)
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, path);
            request.Content = new StringContent(serializedEvent, Encoding.UTF8, "application/json");
            var response = await client.SendAsync(request);
        }
    }
}

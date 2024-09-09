using AnalyzerApi.Domain.Models;
using ServerPulse.EventCommunication.Events;
using System.Net;
using System.Text;
using System.Text.Json;

namespace AnalyzerApi.IntegrationTests.Controllers.EventProcessingController
{
    internal class ProcessLoadEventProcessingControllerTests : BaseIntegrationTest
    {
        [Test]
        public async Task ProcessLoad_ValidLoadEvents_ReturnsOkAndAddStatisticsToMessageBus()
        {
            // Arrange
            var loadEvents = new[]
            {
                new LoadEvent("validKey", "/api/resource", "GET", 200, TimeSpan.FromMilliseconds(150), DateTime.UtcNow),
                new LoadEvent("validKey", "/api/resource", "POST", 201, TimeSpan.FromMilliseconds(200), DateTime.UtcNow)
            };
            using var request = new HttpRequestMessage(HttpMethod.Post, "/eventprocessing/load");
            request.Content = new StringContent(JsonSerializer.Serialize(loadEvents), Encoding.UTF8, "application/json");
            // Act
            var response = await client.SendAsync(request);
            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var statistics = await ReceiveLastObjectFromTopicAsync<LoadMethodStatistics>(LOAD_METHOD_STATISTICS_TOPIC, loadEvents.First().Key);
            Assert.NotNull(statistics);
            Assert.False(statistics.IsInitial);
            Assert.That(statistics.GetAmount, Is.EqualTo(1));
            Assert.That(statistics.PostAmount, Is.EqualTo(1));
        }
        [Test]
        public async Task ProcessLoad_EventsHaveDifferentKeys_ReturnsBadRequest()
        {
            // Arrange
            var loadEvents = new[]
         {
                new LoadEvent("key1", "/api/resource", "GET", 200, TimeSpan.FromMilliseconds(150), DateTime.UtcNow),
                new LoadEvent("key2", "/api/resource", "POST", 201, TimeSpan.FromMilliseconds(200), DateTime.UtcNow)
            };
            using var request = new HttpRequestMessage(HttpMethod.Post, "/eventprocessing/load");
            request.Content = new StringContent(JsonSerializer.Serialize(loadEvents), Encoding.UTF8, "application/json");
            // Act
            var response = await client.SendAsync(request);
            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }
        [Test]
        public async Task ProcessLoad_InvalidArray_ReturnsBadRequest()
        {
            // Arrange
            LoadEvent[] loadEvents = null;
            using var request = new HttpRequestMessage(HttpMethod.Post, "/eventprocessing/load");
            request.Content = new StringContent(JsonSerializer.Serialize(loadEvents), Encoding.UTF8, "application/json");
            // Act
            var response = await client.SendAsync(request);
            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }
    }
}

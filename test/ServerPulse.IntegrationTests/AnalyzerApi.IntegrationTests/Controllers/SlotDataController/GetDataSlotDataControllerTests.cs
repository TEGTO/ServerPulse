using AnalyzerApi.Domain.Dtos.Responses;
using ServerPulse.EventCommunication.Events;
using System.Net;
using System.Text;
using System.Text.Json;

namespace AnalyzerApi.IntegrationTests.Controllers.SlotDataController
{
    internal class GetDataSlotDataControllerTests : BaseIntegrationTest
    {
        const string KEY = "validKey";

        private readonly List<LoadEvent> loadEventSamples = new List<LoadEvent>();
        private readonly List<string> customEvents = new List<string>();

        [OneTimeSetUp]
        public async Task OneTimeSetUp()
        {
            loadEventSamples.Add(new LoadEvent(KEY, "/api/resource", "GET", 200, TimeSpan.FromMilliseconds(150), DateTime.UtcNow));
            await SendEventsAsync(LOAD_TOPIC, KEY, new[]
            {
              loadEventSamples[0]
            });

            Thread.Sleep(1000);

            loadEventSamples.Add(new LoadEvent(KEY, "/api/resource", "POST", 201, TimeSpan.FromMilliseconds(200), DateTime.UtcNow));
            await SendEventsAsync(LOAD_TOPIC, KEY, new[]
            {
              loadEventSamples[1]
            });

            customEvents.Add(JsonSerializer.Serialize(new CustomEvent(KEY, "Request1", "Description1")));
            await SendCustomEventsAsync(CUSTOM_TOPIC, KEY, new[]
            {
              customEvents[0]
            });

            Thread.Sleep(1000);

            customEvents.Add(JsonSerializer.Serialize(new CustomEvent(KEY, "Request2", "Description2")));
            await SendCustomEventsAsync(CUSTOM_TOPIC, KEY, new[]
            {
                 customEvents[1]
            });

            using var request = new HttpRequestMessage(HttpMethod.Post, "/eventprocessing/load");
            request.Content = new StringContent(JsonSerializer.Serialize(loadEventSamples), Encoding.UTF8, "application/json");
            var response = await client.SendAsync(request);
        }

        [Test]
        public async Task GetData_ValidKey_ReturnsOkAndWithAnalyzedServerSlotData()
        {
            // Act
            var response = await client.GetAsync($"slotdata/{KEY}");
            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            var content = await response.Content.ReadAsStringAsync();
            var statistics = JsonSerializer.Deserialize<SlotDataResponse>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            Assert.NotNull(statistics);
            Assert.That(statistics.CollectedDateUTC.Date, Is.EqualTo(DateTime.UtcNow.Date));
            Assert.That(statistics.LastLoadEvents.Count(), Is.EqualTo(2));
            Assert.That(statistics.LastCustomEvents.Count(), Is.EqualTo(2));
            Assert.That(statistics.CustomEventStatistics.LastEvent.SerializedMessage, Is.EqualTo(customEvents[1]));
            Assert.That(statistics.LoadStatistics.IsInitial, Is.EqualTo(false));
            Assert.That(statistics.LoadStatistics.AmountOfEvents, Is.EqualTo(2));
            Assert.That(statistics.LoadStatistics.CollectedDateUTC.Date, Is.EqualTo(DateTime.UtcNow.Date));
            Assert.That(statistics.LoadStatistics.LastEvent.Key, Is.EqualTo(loadEventSamples[1].Key));
            Assert.That(statistics.LoadStatistics.LastEvent.Method, Is.EqualTo(loadEventSamples[1].Method));
            Assert.That(statistics.LoadStatistics.LoadMethodStatistics.GetAmount, Is.EqualTo(1));
            Assert.That(statistics.LoadStatistics.LoadMethodStatistics.PostAmount, Is.EqualTo(1));
            Assert.That(statistics.LoadStatistics.LoadMethodStatistics.DeleteAmount, Is.EqualTo(0));
            Assert.That(statistics.GeneralStatistics.IsInitial, Is.EqualTo(false));
            Assert.That(statistics.GeneralStatistics.DataExists, Is.EqualTo(false));
            Assert.That(statistics.GeneralStatistics.IsAlive, Is.EqualTo(false));
            Assert.That(statistics.GeneralStatistics.CollectedDateUTC.Date, Is.EqualTo(DateTime.UtcNow.Date));

            //Redis Tests

            // Act
            var response2 = await client.GetAsync($"slotdata/{KEY}");
            // Assert
            Assert.That(response2.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            var content2 = await response2.Content.ReadAsStringAsync();
            var statistics2 = JsonSerializer.Deserialize<SlotDataResponse>(content2, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            Assert.NotNull(statistics2);
            Assert.That(statistics2.CollectedDateUTC.Date, Is.EqualTo(DateTime.UtcNow.Date));
            Assert.That(statistics2.LastLoadEvents.Count(), Is.EqualTo(2));
            Assert.That(statistics2.LastCustomEvents.Count(), Is.EqualTo(2));
            Assert.That(statistics2.CustomEventStatistics.LastEvent.SerializedMessage, Is.EqualTo(customEvents[1]));
            Assert.That(statistics2.LoadStatistics.IsInitial, Is.EqualTo(false));
            Assert.That(statistics2.LoadStatistics.AmountOfEvents, Is.EqualTo(2));
            Assert.That(statistics2.LoadStatistics.CollectedDateUTC.Date, Is.EqualTo(DateTime.UtcNow.Date));
            Assert.That(statistics2.LoadStatistics.LastEvent.Key, Is.EqualTo(loadEventSamples[1].Key));
            Assert.That(statistics2.LoadStatistics.LastEvent.Method, Is.EqualTo(loadEventSamples[1].Method));
            Assert.That(statistics2.LoadStatistics.LoadMethodStatistics.GetAmount, Is.EqualTo(1));
            Assert.That(statistics2.LoadStatistics.LoadMethodStatistics.PostAmount, Is.EqualTo(1));
            Assert.That(statistics2.LoadStatistics.LoadMethodStatistics.DeleteAmount, Is.EqualTo(0));
            Assert.That(statistics2.GeneralStatistics.IsInitial, Is.EqualTo(false));
            Assert.That(statistics2.GeneralStatistics.DataExists, Is.EqualTo(false));
            Assert.That(statistics2.GeneralStatistics.IsAlive, Is.EqualTo(false));
            Assert.That(statistics2.GeneralStatistics.CollectedDateUTC.Date, Is.EqualTo(DateTime.UtcNow.Date));
        }
        [Test]
        public async Task GetData_InvalidKey_ReturnsOkAndWithEmptyAnalyzedServerSlotData()
        {
            // Act
            var response = await client.GetAsync($"slotdata/InvalidKey");
            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            var content = await response.Content.ReadAsStringAsync();
            var statistics = JsonSerializer.Deserialize<SlotDataResponse>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            Assert.NotNull(statistics);
            Assert.That(statistics.CollectedDateUTC.Date, Is.EqualTo(DateTime.UtcNow.Date));
            Assert.That(statistics.LastLoadEvents.Count(), Is.EqualTo(0));
            Assert.That(statistics.LastCustomEvents.Count(), Is.EqualTo(0));
            Assert.Null(statistics.CustomEventStatistics.LastEvent);
            Assert.False(statistics.LoadStatistics.IsInitial);
            Assert.That(statistics.LoadStatistics.AmountOfEvents, Is.EqualTo(0));
            Assert.That(statistics.LoadStatistics.CollectedDateUTC.Date, Is.EqualTo(DateTime.UtcNow.Date));
            Assert.Null(statistics.LoadStatistics.LastEvent);
            Assert.That(statistics.LoadStatistics.LoadMethodStatistics.GetAmount, Is.EqualTo(0));
            Assert.That(statistics.LoadStatistics.LoadMethodStatistics.PostAmount, Is.EqualTo(0));
            Assert.That(statistics.LoadStatistics.LoadMethodStatistics.DeleteAmount, Is.EqualTo(0));
            Assert.False(statistics.GeneralStatistics.IsInitial);
            Assert.That(statistics.GeneralStatistics.DataExists, Is.EqualTo(false));
            Assert.That(statistics.GeneralStatistics.IsAlive, Is.EqualTo(false));
            Assert.That(statistics.GeneralStatistics.CollectedDateUTC.Date, Is.EqualTo(DateTime.UtcNow.Date));
        }
    }
}

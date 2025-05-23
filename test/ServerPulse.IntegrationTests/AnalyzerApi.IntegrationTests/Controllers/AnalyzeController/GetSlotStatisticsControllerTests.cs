﻿using AnalyzerApi.Core.Dtos.Endpoints.Analyze.GetSlotStatistics;
using EventCommunication;
using System.Net;
using System.Text.Json;

namespace AnalyzerApi.IntegrationTests.Controllers.AnalyzeController
{
    [TestFixture, Parallelizable(ParallelScope.Children)]
    internal class GetSlotStatisticsControllerTests : BaseIntegrationTest
    {
        private async Task MakeSamplesForKeyAsync(string key, List<LoadEvent> loadEventSamples, List<string> customEvents)
        {
            loadEventSamples.Add(new LoadEvent(key, "/api/resource", "GET", 200, TimeSpan.FromMilliseconds(150), DateTime.UtcNow));
            await SendEventsAsync(LOAD_TOPIC, key,
            [
                loadEventSamples[0]
            ]);

            loadEventSamples.Add(new LoadEvent(key, "/api/resource", "POST", 201, TimeSpan.FromMilliseconds(200), DateTime.UtcNow));
            await SendEventsAsync(LOAD_TOPIC, key,
            [
                loadEventSamples[1]
            ]);

            customEvents.Add(JsonSerializer.Serialize(new CustomEvent(key, "Request1", "Description1")));
            await SendCustomEventsAsync(CUSTOM_TOPIC, key,
            [
                customEvents[0]
            ]);

            customEvents.Add(JsonSerializer.Serialize(new CustomEvent(key, "Request2", "Description2")));
            await SendCustomEventsAsync(CUSTOM_TOPIC, key,
            [
                customEvents[1]
            ]);

            await SendEventsAsync(LOAD_PROCESS_TOPIC, "", loadEventSamples.ToArray());

            await Task.Delay(10000);
        }

        [Test]
        public async Task GetSlotStatistics_ValidKey_ReturnsOkAndWithAnalyzedServerSlotData()
        {
            //Arrange
            var key = Guid.NewGuid().ToString();
            var loadEventSamples = new List<LoadEvent>();
            var customEvents = new List<string>();

            await MakeSamplesForKeyAsync(key, loadEventSamples, customEvents);

            // Act
            var httpResponse = await client.GetAsync($"/analyze/slotstatistics/{key}");

            // Assert
            Assert.That(httpResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var content = await httpResponse.Content.ReadAsStringAsync();

            var statistics = JsonSerializer.Deserialize<GetSlotStatisticsResponse>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            Assert.NotNull(statistics);
            Assert.That(statistics.CollectedDateUTC.Date, Is.Not.EqualTo(default(DateTime)));
            Assert.That(statistics.LastLoadEvents.Count(), Is.LessThanOrEqualTo(2));
            Assert.That(statistics.LastCustomEvents.Count(), Is.LessThanOrEqualTo(2));
            Assert.NotNull(statistics.CustomEventStatistics);
            Assert.NotNull(statistics.CustomEventStatistics.LastEvent);
            Assert.That(statistics.CustomEventStatistics.LastEvent.SerializedMessage, Is.EqualTo(customEvents[1]));
            Assert.NotNull(statistics.LoadStatistics);
            Assert.That(statistics.LoadStatistics.AmountOfEvents, Is.LessThanOrEqualTo(2));
            Assert.That(statistics.LoadStatistics.CollectedDateUTC.Date, Is.Not.EqualTo(default(DateTime)));
            Assert.NotNull(statistics.LoadStatistics.LastEvent);
            Assert.That(statistics.LoadStatistics.LastEvent.Key, Is.EqualTo(loadEventSamples[1].Key));
            Assert.That(statistics.LoadStatistics.LastEvent.Method, Is.EqualTo(loadEventSamples[1].Method));
            Assert.NotNull(statistics.LoadStatistics.LoadMethodStatistics);
            Assert.That(statistics.LoadStatistics.LoadMethodStatistics.GetAmount, Is.LessThanOrEqualTo(1));
            Assert.That(statistics.LoadStatistics.LoadMethodStatistics.PostAmount, Is.LessThanOrEqualTo(1));
            Assert.That(statistics.LoadStatistics.LoadMethodStatistics.DeleteAmount, Is.EqualTo(0));
            Assert.NotNull(statistics.GeneralStatistics);
            Assert.That(statistics.GeneralStatistics.DataExists, Is.EqualTo(false));
            Assert.That(statistics.GeneralStatistics.IsAlive, Is.EqualTo(false));
            Assert.That(statistics.GeneralStatistics.CollectedDateUTC.Date, Is.Not.EqualTo(default(DateTime)));
        }

        [Test]
        public async Task GetSlotStatistics_ValidKey_ReturnsCachedOkAndWithAnalyzedServerSlotData()
        {
            //Arrange
            var key = Guid.NewGuid().ToString();
            var loadEventSamples = new List<LoadEvent>();
            var customEvents = new List<string>();

            await MakeSamplesForKeyAsync(key, loadEventSamples, customEvents);

            // Act
            var httpResponse = await client.GetAsync($"/analyze/slotstatistics/{key}");
            await MakeSamplesForKeyAsync(key, new List<LoadEvent>(), new List<string>());
            var httpResponse2 = await client.GetAsync($"/analyze/slotstatistics/{key}");

            // Assert
            Assert.That(httpResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(httpResponse2.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var content = await httpResponse.Content.ReadAsStringAsync();
            var content2 = await httpResponse2.Content.ReadAsStringAsync();

            var statistics = JsonSerializer.Deserialize<GetSlotStatisticsResponse>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            var statistics2 = JsonSerializer.Deserialize<GetSlotStatisticsResponse>(content2, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            Assert.NotNull(statistics);
            Assert.NotNull(statistics2);

            Assert.That(statistics.CollectedDateUTC.Date, Is.Not.EqualTo(default(DateTime)));
            Assert.That(statistics2.CollectedDateUTC.Date, Is.Not.EqualTo(default(DateTime)));

            Assert.That(statistics.LastLoadEvents.Count(), Is.LessThanOrEqualTo(2));
            Assert.That(statistics2.LastLoadEvents.Count(), Is.LessThanOrEqualTo(2));

            Assert.That(statistics.LastCustomEvents.Count(), Is.LessThanOrEqualTo(2));
            Assert.That(statistics2.LastCustomEvents.Count(), Is.LessThanOrEqualTo(2));

            Assert.NotNull(statistics.CustomEventStatistics);
            Assert.NotNull(statistics2.CustomEventStatistics);

            Assert.NotNull(statistics.CustomEventStatistics.LastEvent);
            Assert.NotNull(statistics2.CustomEventStatistics.LastEvent);

            Assert.That(statistics.CustomEventStatistics.LastEvent.SerializedMessage, Is.EqualTo(customEvents[1]));
            Assert.That(statistics2.CustomEventStatistics.LastEvent.SerializedMessage, Is.EqualTo(customEvents[1]));

            Assert.NotNull(statistics.LoadStatistics);
            Assert.NotNull(statistics2.LoadStatistics);

            Assert.That(statistics.LoadStatistics.AmountOfEvents, Is.LessThanOrEqualTo(2));
            Assert.That(statistics2.LoadStatistics.AmountOfEvents, Is.LessThanOrEqualTo(2));

            Assert.That(statistics.LoadStatistics.CollectedDateUTC.Date, Is.Not.EqualTo(default(DateTime)));
            Assert.That(statistics2.LoadStatistics.CollectedDateUTC.Date, Is.Not.EqualTo(default(DateTime)));

            Assert.NotNull(statistics.LoadStatistics.LastEvent);
            Assert.NotNull(statistics2.LoadStatistics.LastEvent);

            Assert.That(statistics.LoadStatistics.LastEvent.Key, Is.EqualTo(loadEventSamples[1].Key));
            Assert.That(statistics2.LoadStatistics.LastEvent.Key, Is.EqualTo(loadEventSamples[1].Key));

            Assert.That(statistics.LoadStatistics.LastEvent.Method, Is.EqualTo(loadEventSamples[1].Method));
            Assert.That(statistics2.LoadStatistics.LastEvent.Method, Is.EqualTo(loadEventSamples[1].Method));

            Assert.NotNull(statistics.LoadStatistics.LoadMethodStatistics);
            Assert.NotNull(statistics2.LoadStatistics.LoadMethodStatistics);

            Assert.That(statistics.LoadStatistics.LoadMethodStatistics.GetAmount, Is.LessThanOrEqualTo(1));
            Assert.That(statistics2.LoadStatistics.LoadMethodStatistics.GetAmount, Is.LessThanOrEqualTo(1));

            Assert.That(statistics.LoadStatistics.LoadMethodStatistics.PostAmount, Is.LessThanOrEqualTo(1));
            Assert.That(statistics2.LoadStatistics.LoadMethodStatistics.PostAmount, Is.LessThanOrEqualTo(1));

            Assert.That(statistics.LoadStatistics.LoadMethodStatistics.DeleteAmount, Is.EqualTo(0));
            Assert.That(statistics2.LoadStatistics.LoadMethodStatistics.DeleteAmount, Is.EqualTo(0));

            Assert.NotNull(statistics.GeneralStatistics);
            Assert.NotNull(statistics2.GeneralStatistics);

            Assert.That(statistics.GeneralStatistics.DataExists, Is.EqualTo(false));
            Assert.That(statistics2.GeneralStatistics.DataExists, Is.EqualTo(false));

            Assert.That(statistics.GeneralStatistics.IsAlive, Is.EqualTo(false));
            Assert.That(statistics2.GeneralStatistics.IsAlive, Is.EqualTo(false));

            Assert.That(statistics.GeneralStatistics.CollectedDateUTC.Date, Is.Not.EqualTo(default(DateTime)));
            Assert.That(statistics2.GeneralStatistics.CollectedDateUTC.Date, Is.Not.EqualTo(default(DateTime)));
        }

        [Test]
        public async Task GetSlotStatistics_WrongKey_ReturnsOkAndWithEmptyAnalyzedServerSlotData()
        {
            //Arrange
            var key = Guid.NewGuid().ToString();

            await MakeSamplesForKeyAsync(key, new List<LoadEvent>(), new List<string>());

            // Act
            var httpResponse = await client.GetAsync($"/analyze/slotstatistics/wrong-key");

            // Assert
            Assert.That(httpResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var content = await httpResponse.Content.ReadAsStringAsync();
            var statistics = JsonSerializer.Deserialize<GetSlotStatisticsResponse>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            Assert.NotNull(statistics);
            Assert.That(statistics.CollectedDateUTC.Date, Is.Not.EqualTo(default(DateTime)));
            Assert.That(statistics.LastLoadEvents.Count(), Is.EqualTo(0));
            Assert.That(statistics.LastCustomEvents.Count(), Is.EqualTo(0));
            Assert.NotNull(statistics.CustomEventStatistics);
            Assert.Null(statistics.CustomEventStatistics.LastEvent);
            Assert.NotNull(statistics.LoadStatistics);
            Assert.That(statistics.LoadStatistics.AmountOfEvents, Is.EqualTo(0));
            Assert.That(statistics.LoadStatistics.CollectedDateUTC.Date, Is.Not.EqualTo(default(DateTime)));
            Assert.Null(statistics.LoadStatistics.LastEvent);
            Assert.NotNull(statistics.LoadStatistics.LoadMethodStatistics);
            Assert.That(statistics.LoadStatistics.LoadMethodStatistics.GetAmount, Is.EqualTo(0));
            Assert.That(statistics.LoadStatistics.LoadMethodStatistics.PostAmount, Is.EqualTo(0));
            Assert.That(statistics.LoadStatistics.LoadMethodStatistics.DeleteAmount, Is.EqualTo(0));
            Assert.NotNull(statistics.GeneralStatistics);
            Assert.That(statistics.GeneralStatistics.DataExists, Is.EqualTo(false));
            Assert.That(statistics.GeneralStatistics.IsAlive, Is.EqualTo(false));
            Assert.That(statistics.GeneralStatistics.CollectedDateUTC.Date, Is.Not.EqualTo(default(DateTime)));
        }
    }
}
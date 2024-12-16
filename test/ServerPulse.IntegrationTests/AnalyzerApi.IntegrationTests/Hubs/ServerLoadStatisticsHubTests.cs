using AnalyzerApi.Infrastructure.Models.Statistics;
using EventCommunication;
using Microsoft.AspNetCore.SignalR.Client;
using System.Text.Json;

namespace AnalyzerApi.IntegrationTests.Hubs
{
    [TestFixture]
    internal class ServerLoadStatisticsHubTests : BaseIntegrationTest
    {
        const string KEY = "validKey";

        private HubConnection connection;
        private readonly List<LoadEvent> eventSamples = new List<LoadEvent>();
        private string? receivedKey;
        private string? receivedStatistics;

        [OneTimeSetUp]
        public async Task OneTimeSetUp()
        {
            eventSamples.Add(new LoadEvent(KEY, "/api/resource", "GET", 200, TimeSpan.FromMilliseconds(150), DateTime.UtcNow));
            await SendEventsAsync(LOAD_TOPIC, KEY, new[]
            {
               eventSamples[0]
            });

            await Task.Delay(1000);

            eventSamples.Add(new LoadEvent(KEY, "/api/resource", "POST", 201, TimeSpan.FromMilliseconds(200), DateTime.UtcNow));
            await SendEventsAsync(LOAD_TOPIC, KEY, new[]
            {
               eventSamples[1]
            });

            connection = new HubConnectionBuilder()
                .WithUrl("wss://localhost" + "/loadstatisticshub", options =>
                {
                    options.HttpMessageHandlerFactory = _ => server.CreateHandler();
                })
                .Build();

            connection.On<string, string>("ReceiveStatistics", (key, serializedStatistics) =>
            {
                receivedKey = key;
                receivedStatistics = serializedStatistics;
            });

            await connection.StartAsync();
        }

        [OneTimeTearDown]
        public async Task OneTimeTearDown()
        {
            await connection.StopAsync();
            await connection.DisposeAsync();
        }

        [Test]
        public async Task StartListen_ValidKey_ClientAddedToGroupAndStartsConsumingStatistics()
        {
            // Act
            await connection.SendAsync("StartListen", KEY);

            await Task.Delay(2000);

            // Assert
            Assert.NotNull(receivedKey);
            Assert.That(receivedKey, Is.EqualTo(KEY));
            Assert.NotNull(receivedStatistics);

            var statistics = JsonSerializer.Deserialize<ServerLoadStatistics>(receivedStatistics);
            Assert.NotNull(statistics);

            //Assert.True(statistics.IsInitial);
            Assert.That(statistics.AmountOfEvents, Is.EqualTo(2));
            Assert.NotNull(statistics.LastEvent);
            Assert.That(statistics.LastEvent.Id, Is.EqualTo(eventSamples[1].Id));
            Assert.That(statistics.LastEvent.Method, Is.EqualTo(eventSamples[1].Method));

            // Act - Adding new event
            eventSamples.Add(new LoadEvent(KEY, "/api/resource", "DELETE", 200, TimeSpan.FromMilliseconds(200), DateTime.UtcNow));
            await SendEventsAsync(LOAD_TOPIC, KEY, new[]
            {
               eventSamples[2]
            });

            // Assert - Consuming new event
            Assert.NotNull(receivedKey);
            Assert.That(receivedKey, Is.EqualTo(KEY));
            Assert.NotNull(receivedStatistics);

            statistics = JsonSerializer.Deserialize<ServerLoadStatistics>(receivedStatistics);
            Assert.NotNull(statistics);

            //Assert.False(statistics.IsInitial);
            Assert.That(statistics.AmountOfEvents, Is.EqualTo(3));
            Assert.NotNull(statistics.LastEvent);
            Assert.That(statistics.LastEvent.Id, Is.EqualTo(eventSamples[2].Id));
            Assert.That(statistics.LastEvent.Method, Is.EqualTo(eventSamples[2].Method));
        }
    }
}